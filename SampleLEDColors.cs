using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Reflection;


using Random = UnityEngine.Random;

// Color: struct in UnityEngine
// WOW: a Component is actually an instance of a class so the first step is to get a reference to the
// Component instance you want to work with. This is done by GetComponent<>(): e,g Rigidbody rb = GetComponent<RigidBody>();
// To get a reference to an instance of other Components attached to the same gameObject. The same gameobject refers to
// the gameobject to which the current script class which mention the other components.



// Canvas and Game View, Scene View:
//With an "overlay canvas" your UI will be stretched over the entire scene;
//    however, it will be scaled to the size of 1 UI pixel == 1 Unity Unit,
//    so it will appear huge in your scene. This will look correct in the game view.

//The Canvas is a Game Object with a Canvas component on it, 
//    and all UI elements must be children of such a Canvas.

//The Canvas area is shown as a rectangle in the Scene View.This makes it easy to position UI elements 
//    without needing to have the Game View visible at all times.

//Canvas on multiple gameviews:
//https://forum.unity.com/threads/multi-display-canvases-not-working-in-5-4-2.439429/#post-2844936
// On the other hand, I try to set WordSpace for using event camera for Display2. 
//but its only works on display 1 attached camera.

//https://forum.unity.com/threads/multiple-camera-with-multiple-canvas-ui-button-event-problem.747788/

//Does it work when you do a build?
//The editor does not have the same multiple display support as player builds.Unfortuantly the UI is unable to detect the display index when in the Editor and so input will go across all displays, this however should not happen in a player build.
//You could try the hack I provided here: https://forum.unity.com/threads/multi-display-canvases-not-working-in-5-4-2.439429/#post-4981778


/// <summary>
/// Canvas + Physics RayCaster
/// </summary>
//https://forum.unity.com/threads/using-physics-raycast-to-interact-with-canvas-in-worldspace-vr-and-other-environments.563848/
//All Raycaster components have to be attached to the Canvas that you want to receive the event through. With Physics raycasters, rays are parsed through the camera for the canvas to resolve 3D object within the view of the canvas.
// NO. Physics raycasters go on cameras. Graphics raycasters go on canvases.
public class SampleLEDColors : MonoBehaviour
{



    const int BLOCK_SIZE = 1024; // The number of threads in a single thread group

    const int MAX_SIZE_OF_BUFFER = 10000;

    const float epsilon = 1e-2f;
    const float M_PI = 3.1415926535897932384626433832795f;

    [SerializeField] SimpleBoidsTreeOfVoice _boids; // _boids.BoidBuffer is a ComputeBuffer
    //_instanceMaterial.SetBuffer("_BoidBuffer", _boids.BoidBuffer);

    // 컴퓨트 쉐이더
    // Mention another Component instance.
    [SerializeField] protected ComputeShader BoidLEDComputeShader;

    //https://www.reddit.com/r/Unity3D/comments/7ppldz/physics_simulation_on_gpu_with_compute_shader_in/
    // 보이드의 버퍼
    public ComputeBuffer BoidBuffer { get; protected set; } // null reference

 
    public BoidData[] m_boidArray;   //
    
    //When you create a struct object using the new operator, it gets created and the appropriate constructor is called.
    //Unlike classes, structs can be instantiated without using the new operator. 
    //If you do not use new, the fields will remain unassigned and the object cannot be used until all of the fields are initialized.
    
    int m_threadGroupSize;

    //public void Insert(int index, T item);
    //public int LastIndexOf(T item);
    //public int LastIndexOf(T item, int index);
    //public int LastIndexOf(T item, int index, int count);
    //public bool Remove(T item);
    // public void RemoveAt(int index);
    public struct BoidData
    {
        // public Vector3  WallOrigin; // the reference position of the wall (the boid reference frame) on which the boid is 

        //public Vector3 EulerAngles; // the rotation of the boid reference frame
        public Vector3 Position; // the position of the  boid in the boid reference frame        

        public Vector3 Scale; // the scale factors
        public Vector3 HeadDir; // heading direction of the boid on the local plane
        public float Speed;            // the speed of a boid

        public float Radius; // the radius of the circle boid
        public Vector4 Color;         // RGBA color
        public Vector2 SoundGrain; // soundGrain = (freq, amp)
        public float Duration;     // duration of a boid each frame
        public int WallNo;      // the number of the wall on which the boid lie. 0=> the ground
                                // 1 => the ceiling, 2 => left wall, 3=> right wall. 4=> front wall
    }

    public struct LEDData
    {
        // public Vector3  WallOrigin; // the reference position of the wall (the boid reference frame) on which the boid is 

        //public Vector3 EulerAngles; // the rotation of the boid reference frame
        public Vector3 Position; //

        public Vector4 Color;         // RGBA color
        public int WallNo;      // the number of the wall whose boids defined the light sources of the branch cylinder
                                // 0=> the core circular wall. 
                                // 1 => the outer circular wall;
    }

    public struct BranchCylinder
    {
        // public Vector3  WallOrigin; // the reference position of the wall (the boid reference frame) on which the boid is 

        //public Vector3 EulerAngles; // the rotation of the boid reference frame
        public Vector3 Position; // the position of the  cylinder origin in the boid reference frame        
        public float Height;

        public float Radius; // the radius of the cylinder
        public Vector4 Color;         // RGBA color of the cylinder; This color is a weighted sum of the colors of the neighbor
                                      // boids of the branch cylinder which is located at Position

        public int WallNo;      // the number of the wall whose boids defined the light sources of the branch cylinder
                                // 0=> the core circular wall. 
                                // 1 => the outer circular wall;
    }


    //public static Color HSVToRGB(float H, float S, float V);
    protected void InitializeValues()
    {
     


        BoidLEDComputeShader.SetInt("_BoidsNum", (int)m_BoidsNum);
        BoidLEDComputeShader.SetBuffer(KernelIdLED, "_BoidBuffer", _boids.BoidBuffer);

     




    } //nitializeValues()


    float findAngleForVector(Vector3 vec)
    {
        float theta = Mathf.Atan2(vec.z, vec.x); // theta ranges (0,pi) or (0 -pi)

        if (theta < 0)
        { // negative theta means that vec (x,y) is in 3rd or 4th quadrant, measuring in the clockwise direction
            return (2 * M_PI + theta); // angle measured in the counterclockwise direction
        }
        else
        {
            return theta;
        }
    }


    float randomSign()
    {
        //  When defining Random.Range(1.0f, 3.0f) we will get results from 1.0 to 3.0;
        //When defining Random.Range(1,3) we will get results from 1 to 2

        return Random.Range(0, 2) == 0 ? -1f : 1f;
    }


    protected void InitializeBuffers()
    {
        // 버퍼의 초기화
        //https://stackoverflow.com/questions/21596373/compute-shaders-input-3d-array-of-floats
        //https://forum.unity.com/threads/possible-for-a-compute-buffer-to-pass-a-struct-containing-a-vector3-array.370329/
        //https://forum.unity.com/threads/size-of-computebuffer-for-mesh-vertices.446972/
        //Also by checking the decompiled file of Vector3 on github, I confirmed that Vector3 indeed only consists of 3 floats


       // BoidBuffer = new ComputeBuffer(MAX_SIZE_OF_BUFFER, Marshal.SizeOf(typeof(BoidData)));

       // m_boidArray = new BoidData[MAX_SIZE_OF_BUFFER];
       


        // for debugging

        //  Debug.Log("Boids Initialization");

        //   for (int i=0; i < BoidsNum; i++)
        //   {

        //    Debug.Log("boidNo = "); Debug.Log(i);
        //    Debug.Log("position = = "); 
        //    Debug.Log(boidArray[i].Position);

        //    Debug.Log("color = = ");
        //   Debug.Log(boidArray[i].Color);

        // }

        //
        

     

      

    } // InitializeBuffers()








} // class SimpleBoids
