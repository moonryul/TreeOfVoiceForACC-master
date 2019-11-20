using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
 

public class BoidLEDRendererTreeOfVoice : MonoBehaviour
{

    

// ComputeBuffer: GPU data buffer, mostly for use with compute shaders.
// you can create & fill them from script code, and use them in compute shaders or regular shaders.


// Declare other Component _boids; you can drag any gameobject that has that Component attached to it.
// This will acess the Component directly rather than the gameobject itself.


    SimpleBoidsTreeOfVoice m_boids; // _boids.BoidBuffer is a ComputeBuffer

    LEDColorGenController m_LEDColorGenController;

    [SerializeField] Material m_boidLEDInstanceMaterial;
    
    CylinderMesh m_instanceMeshCylinder;
    CircleMesh m_instanceMeshCircle;

    float m_unitRadius = 1.0f;

    Mesh m_boidInstanceMesh;

    public float m_ceilingHeight = 4.5f; 
    //public float m_innerCylinderHeightScale = 4.5f; // 4.5 m; Unit Hight = 1 m
    //public float m_outerCylinderHeightScale = 2.5f; // 4.5 m; Unit Hight = 1 m

  
    public float m_scale = 1.0f; // the scale of the instance mesh

    ComputeBuffer m_boidLEDArgsBuffer;
     
    uint[] m_boidLEDArgs = new uint[5] { 0, 0, 0, 0, 0 };

    uint numIndices;

    // parameters for cylinder construction
   
 
    int nbSides = 18;
    int nbHeightSeg = 1;

    public struct BoidLEDData
    {
        public Matrix4x4 BoidFrame;
        public Vector3 Position; //
        public Vector3 HeadDir; // heading direction of the boid on the local plane
        public Vector4 Color;         // RGBA color
        public Vector3 Scale;
        public int WallNo;      // the number of the wall whose boids defined the light sources of the branch cylinder
                                // 0=> the inner  circular wall. 
                                // 1 => the outer circular wall;
        public int NearestBoidID;
        public int NeighborCount;
    }



    BoidLEDData[] m_BoidLEDArray;

    private void Awake()
    {
        m_LEDColorGenController = this.gameObject.GetComponent<LEDColorGenController>();
        m_boids = this.gameObject.GetComponent<SimpleBoidsTreeOfVoice>();



        if (m_boidLEDInstanceMaterial == null)
        {
            Debug.LogError("The global Variable m_boidLEDInstanceMaterial is not  defined in Inspector");
            // EditorApplication.Exit(0);
            Application.Quit();
            //return;

        }


        m_instanceMeshCircle = new CircleMesh(m_unitRadius);

        // m_instanceMeshCylinder =  new CylinderMesh(m_unitHeight, m_cylinderRadiusScale, nbSides, nbHeightSeg);

        //m_boidInstanceMesh = m_instanceMeshCylinder.m_mesh;

        m_boidInstanceMesh = m_instanceMeshCircle.m_mesh;
        m_boidLEDArgsBuffer = new ComputeBuffer(
          1,
          m_boidLEDArgs.Length * sizeof(uint),
          ComputeBufferType.IndirectArguments
         );

    } // Awake()
    void Start () 
	{
    
        m_BoidLEDArray = new BoidLEDData[m_LEDColorGenController.m_totalNumOfLEDs];

              if (m_boids == null)
        {
            Debug.LogError("SimpleBoidsTreeOfVoice component should be attached to CommHub");
            //EditorApplication.Exit(0);

            Application.Quit();
            //return;

        }
                           
        if (m_LEDColorGenController.m_BoidLEDBuffer == null)
        {
            Debug.LogError("m_LEDColorGenController.m_BoidLEDBuffer should be set in Awake() of  LEDColorGenController");
            Application.Quit();
           // return; // nothing to render; 

        }

        
        ///
        numIndices = m_boidInstanceMesh ? m_boidInstanceMesh.GetIndexCount(0) : 0;
        //GetIndexCount(submesh = 0)


        m_boidLEDArgs[0] = numIndices;  // the number of indices per instance
        m_boidLEDArgs[1] = (uint) m_LEDColorGenController.m_totalNumOfLEDs; // the number of instances

        m_boidLEDArgsBuffer.SetData( m_boidLEDArgs) ;

        //m_boidLEDInstanceMaterial.SetInt("_BoidOrLED", 0); // Boid
        //m_boidLEDInstanceMaterial.SetInt("_BoidOrLED", 1); // LED


        //m_boidLEDInstanceMaterial.SetFloat("_LEDChainHeight", m_ceilingHeight );

        m_boidLEDInstanceMaterial.SetVector("_Scale", new Vector3(m_scale, m_scale, m_scale));

        //m_boidLEDInstanceMaterial.SetVector("GroundMaxCorner", m_boids.GroundMaxCorner);
        //m_boidLEDInstanceMaterial.SetVector("GroundMinCorner", m_boids.GroundMinCorner);

        //m_boidLEDInstanceMaterial.SetVector("CeilingMaxCorner", m_boids.CeilingMaxCorner);
        //m_boidLEDInstanceMaterial.SetVector("CeilingMinCorner", m_boids.CeilingMinCorner);

        m_boidLEDInstanceMaterial.SetBuffer("_BoidLEDBuffer", m_LEDColorGenController.m_BoidLEDBuffer);
        // This is the shared buffer between CPU and GPU

        //m_boidLEDInstanceMaterial.SetBuffer("_BoidBuffer", m_boids.m_BoidBuffer);


        //"_BoidLEDBuffer" is computed   // m_LEDColorGenController.m_BoidLEDBuffer is ready in and AWake() and Update() of m_LEDColorGenController.



    } // Start()




    // Update is called once per frame
    public void Update () 
	{

       // m_LEDColorGenController.m_BoidLEDBuffer.GetData(m_BoidLEDArray);

       // Debug.Log("In Update(): BoidLEDRenderTreeOfVoice:");

        //for (int i = 0; i < m_LEDColorGenController.m_totalNumOfLEDs; i++)
        //{

        //    Debug.Log(i + "th LED Position" + m_BoidLEDArray[i].Position);
        //    Debug.Log(i + "th LED HeadDir" + m_BoidLEDArray[i].HeadDir);
        //    Debug.Log(i + "th LED Color" + m_BoidLEDArray[i].Color);

        //}

        ////BOIDLEDCyliner drawing


        // m_LEDColorGenController.m_BoidLEDBuffer.SetData(m_BoidLEDArray);
        // cf:  m_boidLEDInstanceMaterial.SetBuffer("_BoidLEDBuffer", m_LEDColorGenController.m_BoidLEDBuffer);

        Graphics.DrawMeshInstancedIndirect(
             m_boidInstanceMesh, // a mesh to be drawn
            0,
            m_boidLEDInstanceMaterial, // This material defines the parameters and buffers passed to the shader
            
            new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
            m_boidLEDArgsBuffer // this contains the number of instances and index coung per instance
        );


          
       
    }

	private void OnDestroy()
	{
	

        if (m_boidLEDArgsBuffer == null) return;
        m_boidLEDArgsBuffer.Release();
        m_boidLEDArgsBuffer = null;

    }


}//class BoidLEDRendererTreeOfVoice 
