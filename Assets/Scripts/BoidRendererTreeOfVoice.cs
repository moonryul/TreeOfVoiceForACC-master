using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
 

public class BoidRendererTreeOfVoice : MonoBehaviour
{

    // ComputeBuffer: GPU data buffer, mostly for use with compute shaders.
    // you can create & fill them from script code, and use them in compute shaders or regular shaders.


     // Declare other Component _boids; you can drag any gameobject that has that Component attached to it.
     // This will acess the Component directly rather than the gameobject itself.


    SimpleBoidsTreeOfVoice m_boids; // _boids.BoidBuffer is a ComputeBuffer
  


    [SerializeField] Material m_boidInstanceMaterial;

 

    // [SerializeField] protected Vector3 RoomMinCorner = new Vector3(-10f, 0f, -10f);
    // [SerializeField] protected Vector3 RoomMaxCorner = new Vector3(10f, 12f, 10f);


    // 보이드의 수
    // public int BoidsNum = 256;


    
    // 보이드의 수
    int BoidsNum;

    // GPU Instancing
    // Graphics.DrawMeshInstancedIndirect
    // 인스턴스화 된 쉐이더를 사용하여 특정 시간동안 동일한 메시를 그릴 경우 사용
     CircleMesh m_instanceMeshCircle;
     CylinderMesh m_instanceMeshCylinder;

    [SerializeField]  Mesh m_instanceMeshSphere;

    public bool m_useCircleMesh = true;
   
    //[SerializeField]  Mesh _instanceMeshHuman1;
    //[SerializeField]  Mesh _instanceMeshHuman2;
    //[SerializeField]  Mesh _instanceMeshHuman3;
    //[SerializeField]  Mesh _instanceMeshHuman4;

   // public MeshSetting _meshSetting = new MeshSetting(1.0f, 1.0f);

   // int _meshNo; // set from SimpleBoids (used to be)

    Mesh m_boidInstanceMesh;
    public float m_scale = 1.0f; // the scale of the instance mesh

    //private ComputeBuffer colorBuffer;

    //     ArgsOffset      인스턴스 당 인덱스 수    (Index count per instance)
    //                     인스턴스 수              (Instance count)
    //                     시작 인덱스 위치         (Start index location)
    //                     기본 정점 위치           (Base vertex location)
    //                     시작 인스턴스 위치       (Start instance location)
    ComputeBuffer m_boidArgsBuffer;
   

    uint[] m_boidArgs = new uint[5] { 0, 0, 0, 0, 0 };


    uint numIndices;
    Vector3[] vertices3D;

    int[] indices;


    float height = 10; // m; scale = 0.1 ~ 0.3
    float radius = 0.1f; // 0.1 m =10cm

    // parameters for cylinder construction
    int nbSides = 18;
    int nbHeightSeg = 1;


    //// Create Vector2 vertices
    float unitRadius = 1f; // radius = 1m


    private void Awake()
    { // initialize me


        // check if the global component object is defined
        if (m_boidInstanceMaterial == null)
        {
            Debug.LogError("The global Variable _boidInstanceMaterial is not  defined in Inspector");
            // EditorApplication.Exit(0);
            return;
        }

        m_instanceMeshCircle = new CircleMesh(unitRadius);               

        m_instanceMeshCylinder =new CylinderMesh(height, radius, nbSides, nbHeightSeg);

        m_boidArgsBuffer = new ComputeBuffer(
            1, // count
            m_boidArgs.Length * sizeof(uint),

            ComputeBufferType.IndirectArguments
        );

        

        if (m_useCircleMesh) // use 2D boids => creat the mesh in a script
        {

            m_boidInstanceMesh = m_instanceMeshCircle.m_mesh;

        }

        else
        {
            m_boidInstanceMesh = m_instanceMeshCylinder.m_mesh;


        }
        //  else {
        //            Debug.LogError("useCircleMesh or useSphereMesh should be checked");
        //            //If we are running in a standalone build of the game
        //            #if UNITY_STANDALONE
        //            //Quit the application
        //            Application.Quit();
        //            #endif

        //            //If we are running in the editor
        //            #if UNITY_EDITOR
        //            //Stop playing the scene
        //            // UnityEditor.EditorApplication.isPlaying = false;
        //            //Setting isPlaying delays the result until after all script code has completed for this frame.

        //            EditorApplication.Exit(0);
        //            #endif
        //        }



        //Debug.Log("number of indices=");
        //Debug.Log(_instanceMesh.GetIndexCount(0));

        

    }
    void Start () 
	{   // initialize others

        // get the reference to SimpleBoidsTreeOfVoice

        m_boids = this.gameObject.GetComponent<SimpleBoidsTreeOfVoice>();
    

        if (m_boids == null)
        {
            Debug.LogError("SimpleBoidsTreeOfVoice component should be attached to CommHub");
            //EditorApplication.Exit(0);
            // Application.Quit();
            return;
            
        }
                


        // check if _boids.BoidBuffer is not null
        if (m_boids.m_BoidBuffer is null) return; // nothing to render; 


        // _boids.BoidBuffer.GetData(boidArray);
        // https://unity3d.com/kr/learn/tutorials/topics/graphics/gentle-introduction-shaders

        //Debug.Log("Current Num of Boids=");
        //Debug.Log(_boids.BoidsNum);


        //https://answers.unity.com/questions/979080/how-to-pass-an-array-to-a-shader.html
        //_instanceMaterial.SetFloatArray("GroundMaxCorner", GroundMaxCornerF);
        //_instanceMaterial.SetFloatArray("GroundMinCorner", GroundMinCornerF);

        // _instanceMaterial.SetFloatArray("CeilingMaxCorner", CeilingMaxCornerF);
        // _instanceMaterial.SetFloatArray("CeilingMinCorner", CeilingMinCornerF);

        // // Shader vectors are always Vector4s.
        // But the value here is converted to a Vector3.
        //Vector3 value = Vector3.one;
        //Renderer renderer = GetComponent<Renderer>();
        //renderer.material.SetVector("_SomeVariable", value);

        // Use SetVector() rather than setFloatArray


        // BOIDS Drawing

        numIndices = m_boidInstanceMesh ? m_boidInstanceMesh.GetIndexCount(0) : 0;
        //GetIndexCount(submesh = 0)



        m_boidArgs[0] = numIndices;  // the number of indices in the set of triangles
        m_boidArgs[1] = (uint)m_boids.m_BoidsNum; // the number of instances

        m_boidArgsBuffer.SetData(m_boidArgs);


        m_boidInstanceMaterial.SetVector("_Scale", new Vector3(m_scale, m_scale, m_scale) );

        //m_boidInstanceMaterial.SetVector("GroundMaxCorner", m_boids.GroundMaxCorner);
        //m_boidInstanceMaterial.SetVector("GroundMinCorner", m_boids.GroundMinCorner);

        //m_boidInstanceMaterial.SetVector("CeilingMaxCorner", m_boids.CeilingMaxCorner);
        //m_boidInstanceMaterial.SetVector("CeilingMinCorner", m_boids.CeilingMinCorner);

        m_boidInstanceMaterial.SetBuffer("_BoidBuffer", m_boids.m_BoidBuffer); 
        // m_boids.BoidBuffer is ceated in SimpleBoids.cs
        // This buffer is shared between CPU and GPU



    } // Start()
	
	// Update is called once per frame
	public void Update () 
	{
		RenderInstancedMesh();     
        

       
    }

	private void OnDestroy()
	{
		if(m_boidArgsBuffer == null) return;
		m_boidArgsBuffer.Release();
		m_boidArgsBuffer = null;


    }

	private void RenderInstancedMesh()

	{
        //"_BoidBuffer" is changed by SimpleBoidsTreeOfVoice
        // for debugging, comment out
        Graphics.DrawMeshInstancedIndirect(
            m_boidInstanceMesh,
            0,
            m_boidInstanceMaterial, // This material defines the shader which receives instanceID
            new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
            m_boidArgsBuffer // this contains the information about the instances: see below
        );

        // _boidArgs = 
        ////                     인스턴스 당 인덱스 수    (Index count per instance)
        ////                     인스턴스 수              (Instance count)
        ////                     시작 인덱스 위치         (Start index location)
        ////                     기본 정점 위치           (Base vertex location)
        ////                     시작 인스턴스 위치       (Start instance location)
        //ComputeBuffer _argsBuffer;
        // uint[] _boidArgs = new uint[5] { 0, 0, 0, 0, 0 };


        // reading from the buffer written by regular shaders
        //https://gamedev.stackexchange.com/questions/128976/writing-and-reading-computebuffer-in-a-shader

        // _boids.BoidBuffer.GetData(boidArray);



    }//private void RenderInstancedMesh()

    //  public struct BoidData
    //  {
    //     public Vector2 Position; // the position of a boid center; float x and float y
    //     public Vector2 Scale; // the scale factors of x and z directions
    //     public float Angle; // the head angle of a boid: from 0 to 2 *PI
    //     public float Speed;            // the speed of a boid
    //     public Vector4 Color;         // RGBA color
    //     public Vector2 SoundGrain; // soundGrain = (freq, amp)
    //      public float Duration;     // duration of a boid each frame
    //      public int  WallNo;      // indicates whether the boid is on ground or on ceiling
    //   }

}
