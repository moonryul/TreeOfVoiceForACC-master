using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

public class BoidLEDRendererTreeOfVoice : MonoBehaviour
{

    // Update is called once per frame
    bool m_fileClosed = false;
    // Cameras for DrwaMeshInstanced()

    Camera m_MainCamera, m_CameraToGround, m_CameraToCeiling, m_CameraToFrontWall;

    //http://blog.deferredreality.com/write-to-custom-data-buffers-from-shaders/
    //The register(u2) represents which internal gpu registrar to bind the data structure to.
    //You need to specify the same in C#, and keep in mind this is global on the GPU.
    ComputeBuffer m_MatrixBuffer;
    ComputeBuffer m_LEDBoidPosBuffer;
    int m_numOfShaderMatrices = 3; // 
    //
    // ComputeBuffer(int count, int stride, ComputeBufferType type);
    // 
    Matrix4x4[] m_MatrixArray; // arrays should be blittable. An 1d array of floats
    Vector4[] m_LEDBoidPosArray;

    // public Matrix4x4(Vector4 column0, Vector4 column1, Vector4 column2, Vector4 column3);

    //public float this[int index] { get; set; }
    //public float this[int row, int column] { get; set; }


    SimpleBoidsTreeOfVoice m_boids; // _boids.BoidBuffer is a ComputeBuffer

    LEDColorGenController m_LEDColorGenController;
    BoidRendererTreeOfVoice m_boidRendererTreeOfVoice;

    [SerializeField] Material m_boidLEDInstanceMaterial;
    
    CylinderMesh m_instanceMeshCylinder;
    CircleMesh m_instanceMeshCircle;

    float m_unitRadius = 1.0f;

    Mesh m_boidInstanceMesh;

  
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
    StreamWriter m_writer;
    FileStream m_oStream;

    private void Awake()
    {

        m_fileClosed = false;
        // DEBUG code
        string fileName = "LEDBoidInfo";
        //https://rocabilly.tistory.com/34
        //"yyyy.MM.dd.HH.mm.ss"
        string fileIndex = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        fileIndex.Replace(" ", string.Empty);
        //fileIndex = string.Join("",
        //      fileIndex.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

        string path = "Assets/Resources/DebugFiles/" + fileName + fileIndex + ".txt";


        File.CreateText(path).Dispose();
        ////Write some text to the test.txt file
        m_writer = new StreamWriter(path, false); // do not append



        // Find the cameras for DrawMeshIntanced()

        m_MainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        m_CameraToGround = GameObject.FindWithTag("CameraToGround").GetComponent<Camera>();
        m_CameraToCeiling = GameObject.FindWithTag("CameraToCeiling").GetComponent<Camera>();
        m_CameraToFrontWall = GameObject.FindWithTag("CameraToFrontWall").GetComponent<Camera>();

        if (m_MainCamera == null)
        {
            Debug.LogError("The main Camera is not defined");
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
# endif  
        }

        if (m_CameraToGround == null)
        {
            Debug.LogError("The CameraToGround is not defined");
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
# endif  
        }
        if (m_CameraToCeiling == null)
        {
            Debug.LogError("The CameraToCeiling is not defined");
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
# endif  
        }
        if (m_CameraToFrontWall == null)
        {
            Debug.LogError("The CameraToFrontWall is not defined");
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
# endif  
        }




        m_LEDColorGenController = this.gameObject.GetComponent<LEDColorGenController>();
        m_boids = this.gameObject.GetComponent<SimpleBoidsTreeOfVoice>();



        if (m_boidLEDInstanceMaterial == null)
        {
            Debug.LogError("The global Variable m_boidLEDInstanceMaterial is not  defined in Inspector");
            // EditorApplication.Exit(0);

#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
#endif


        }

        m_boidRendererTreeOfVoice = this.gameObject.GetComponent<BoidRendererTreeOfVoice>();
        // Get the mesh instance

        //m_boidInstanceMesh = m_boidRendererTreeOfVoice.m_boidInstanceMesh;

        //m_instanceMeshCircle = new CircleMesh(m_unitRadius);

        //// m_instanceMeshCylinder =  new CylinderMesh(m_unitHeight, m_cylinderRadiusScale, nbSides, nbHeightSeg);

        ////m_boidInstanceMesh = m_instanceMeshCylinder.m_mesh;

        //m_boidInstanceMesh = m_instanceMeshCircle.m_mesh;
        m_boidLEDArgsBuffer = new ComputeBuffer(
          1,
          m_boidLEDArgs.Length * sizeof(uint),
          ComputeBufferType.IndirectArguments
         );

        // Debug: print mesh information

        ///
       // numIndices = m_boidInstanceMesh ? m_boidInstanceMesh.GetIndexCount(0) : 0;
        //GetIndexCount(submesh = 0)
        // GetNormals, GetVertices

        //Mesh.vertexCount
        //Mesh.triangles.Length
        //List<Vector3> vertices = new List<Vector3>(m_boidInstanceMesh.vertexCount);

        //List<Vector3> normals = new List<Vector3>(m_boidInstanceMesh.vertexCount);

        //m_boidInstanceMesh.GetVertices(vertices);
        //m_boidInstanceMesh.GetNormals(normals);

        //for (int i = 0; i < m_boidInstanceMesh.vertexCount; i++)
        //{
        //    //m_writer.WriteLine(i + "th vertex pos=" +  vertices[i]);
        //    //m_writer.WriteLine(i + "th vertex normal=" + normals[i]);
        //    // normals[i] = -normals[i];

        //    //     The normals of the Mesh.
        //    //  public Vector3[] normals { get; set; }
        //    m_boidInstanceMesh.normals[i] *= -1; // reverse the direction of the normal for 2D circle mesh



        //    //}

        //    // m_writer.Close(); you need to write in Update() as well


        //}
    } // Awake()
    void Start () 
	{

        m_boidInstanceMesh = m_boidRendererTreeOfVoice.m_boidInstanceMesh;



        //ComputeBufferType.Default: In HLSL shaders, this maps to StructuredBuffer<T> or RWStructuredBuffer<T>.
        m_MatrixBuffer = new ComputeBuffer(m_numOfShaderMatrices,
                                           Marshal.SizeOf(typeof(Matrix4x4)),
                                            ComputeBufferType.Default);

        ////(4*4) * sizeof(float) == Marshal.SizeOf(typeof(Matrix4x4))
        ///
        // Set the ComputeBuffer for shader debugging
        // But a RWStructuredBuffer, requires SetRandomWriteTarget to work at all in a non-compute-shader. 
        //This is all Unity API magic which in some ways is convenient 

        // for debugging

       // Graphics.SetRandomWriteTarget(2, m_MatrixBuffer);

        ////cf. in shader: RWStructuredBuffer<float4x4> _MatrixBuffer : register(u1);
        //SetRandomWriteTarget(int index, ComputeBuffer uav, bool preserveCounterValue = false);
        // The "1" represents the target index ie u1.
        // Uses "unordered access views" (UAV) in UsingDX11GL3Features. 
        // These "random write" targets are set similarly to how multiple render targets are set.


        //_MatrixBuffer[0] = UNITY_MATRIX_M;
        //_MatrixBuffer[1] = UNITY_MATRIX_V;
        //_MatrixBuffer[2] = UNITY_MATRIX_P;

        m_MatrixArray = new Matrix4x4[m_numOfShaderMatrices];


        m_MatrixBuffer.SetData(m_MatrixArray);


        // compute buffer for ledboid:
        //ComputeBufferType.Default: In HLSL shaders, this maps to StructuredBuffer<T> or RWStructuredBuffer<T>.
        m_LEDBoidPosBuffer = new ComputeBuffer(m_LEDColorGenController.m_totalNumOfLEDs,
                                           Marshal.SizeOf(typeof(Vector4)),
                                            ComputeBufferType.Default);

        ////(4*4) * sizeof(float) == Marshal.SizeOf(typeof(Matrix4x4))
        ///
        // Set the ComputeBuffer for shader debugging
        // But a RWStructuredBuffer, requires SetRandomWriteTarget to work at all in a non-compute-shader. 
        //This is all Unity API magic which in some ways is convenient 

        // for debugging
        //Graphics.SetRandomWriteTarget(3, m_LEDBoidPosBuffer);

        // //RWStructuredBuffer<float4> _LEDBoidPosBuffer : register(u2);

        //SetRandomWriteTarget(int index, ComputeBuffer uav, bool preserveCounterValue = false);
        // The "1" represents the target index ie u1.
        // Uses "unordered access views" (UAV) in UsingDX11GL3Features. 
        // These "random write" targets are set similarly to how multiple render targets are set.


// for debugging
       // m_LEDBoidPosArray = new Vector4[m_LEDColorGenController.m_totalNumOfLEDs];


       // m_LEDBoidPosBuffer.SetData(m_LEDBoidPosArray);



        // m_BoidLEDArray = new BoidLEDData[m_LEDColorGenController.m_totalNumOfLEDs];
        // This array was used to get the data of BoidLEDBuffer for debugging purpose below

        if (m_boids == null)
        {
            Debug.LogError("SimpleBoidsTreeOfVoice component should be attached to CommHub");
            //EditorApplication.Exit(0);
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
#endif


        }
                           
        if (m_LEDColorGenController.m_BoidLEDBuffer == null)
        {
            Debug.LogError("m_LEDColorGenController.m_BoidLEDBuffer should be set in Awake() of  LEDColorGenController");

#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
           

        }

        
        ///
        numIndices = m_boidInstanceMesh ? m_boidInstanceMesh.GetIndexCount(0) : 0;
        //GetIndexCount(submesh = 0)


        m_boidLEDArgs[0] = numIndices;  // the number of indices per instance
        m_boidLEDArgs[1] = (uint) m_LEDColorGenController.m_totalNumOfLEDs; // the number of instances

        m_boidLEDArgsBuffer.SetData( m_boidLEDArgs) ;

        //m_boidLEDInstanceMaterial.SetInt("_BoidOrLED", 0); // Boid
        //m_boidLEDInstanceMaterial.SetInt("_BoidOrLED", 1); // LED


        //m_boidLEDInstanceMaterial.SetFloat("_LEDCeilingHeight", m_LEDCeilingHeight );

        m_boidLEDInstanceMaterial.SetVector("_Scale", new Vector3(m_scale, m_scale, m_scale));

        //m_boidLEDInstanceMaterial.SetVector("GroundMaxCorner", m_boids.GroundMaxCorner);
        //m_boidLEDInstanceMaterial.SetVector("GroundMinCorner", m_boids.GroundMinCorner);

        //m_boidLEDInstanceMaterial.SetVector("CeilingMaxCorner", m_boids.CeilingMaxCorner);
        //m_boidLEDInstanceMaterial.SetVector("CeilingMinCorner", m_boids.CeilingMinCorner);

        m_boidLEDInstanceMaterial.SetBuffer("_BoidLEDBuffer", m_LEDColorGenController.m_BoidLEDBuffer);

        //m_boidLEDInstanceMaterial.SetBuffer("_MatrixBuffer", m_MatrixBuffer);
        //m_boidLEDInstanceMaterial.SetBuffer("_LEDBoidPosBuffer", m_LEDBoidPosBuffer);
        // This is the shared buffer between CPU and GPU

        //m_boidLEDInstanceMaterial.SetBuffer("_BoidBuffer", m_boids.m_BoidBuffer);


        //"_BoidLEDBuffer" is computed   // m_LEDColorGenController.m_BoidLEDBuffer is ready in and AWake() and Update() of m_LEDColorGenController.



    } // Start()





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

        //public static void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, 
        //    Bounds bounds, ComputeBuffer bufferWithArgs, int argsOffset = 0, 
        //    MaterialPropertyBlock properties = null, Rendering.ShadowCastingMode,
        //    castShadows = ShadowCastingMode.On, bool receiveShadows = true, int layer = 0, 
        //    Camera camera = null, Rendering.LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes, 
        //    LightProbeProxyVolume lightProbeProxyVolume = null);

        //int layer = 0; // Default layer

        //Graphics.DrawMeshInstancedIndirect(
        //    m_boidInstanceMesh,
        //    0,
        //     m_boidLEDInstanceMaterial, // This material defines the shader which receives instanceID
        //    new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //    m_boidLEDArgsBuffer, // this contains the information about the instances: see below
        //    0, null, ShadowCastingMode.Off, false, layer, null //m_MainCamera
        //    );


        Graphics.DrawMeshInstancedIndirect(
             m_boidInstanceMesh, // a mesh to be drawn
            0,
            m_boidLEDInstanceMaterial, // This material defines the parameters and buffers passed to the shader

            new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
            m_boidLEDArgsBuffer // this contains the number of instances and index coung per instance
        );


        // Asynchronous GetData:
        // https://github.com/SlightlyMad/AsyncTextureReader
        //https://forum.unity.com/threads/asynchronously-getting-data-from-the-gpu-directx-11-with-rendertexture-or-computebuffer.281346/

        //I don't see a way around this personally, you might reduce it via a native plugin 
        //but that is a lot of work for ~5fps unless you REALLY need it.

        // The slowdown of GetData() is due to a pipeline stall. CPU execution blocks on the GetData() call, 
        //flushes all pending commands to the GPU, waits for the GPU to flush its own pipeline,
        //and then finally initiates the transfer. The transfer itself is fairly quick 
        //- it's all the pipeline cleanup/flushing that must take place beforehand.

        //        The general way to hide this latency is through asynchronous transfers,
        //        where the CPU doesn't block on the call, thus allowing the GPU to perform the transfer
        //            at a point 
        //            which is convenient for it to do so. This eliminates the lag felt from the stall, but
        //            data will be several frames old when received (although that usually doesn't matter).

        //You can do this easily in OpenGL, not sure about D3D(although I'd be very surprised if you can't).
        //        Problem is that it's fairly low-level and Unity doesn't expose this functionality
        //            (yet, hopefully).

        //        I'm reading up on that very thing as we speak:

        //http://docs.unity3d.com/Manual/NativePlugins.html
        //        and
        //        http://docs.unity3d.com/Manual/NativePluginInterface.html

        //I think I can do this, but need a way to access a ComputeBuffer's native handle. 
        //            You can do so with Textures (Texture.GetNativeTexturePtr), but I don't currently 
        //            see an easy way to do so with ComputeBuffers.Will update if I find anything further.

        //        2016, ReadPixels continues to stall the GPU without an alternative.

        //A native plugin is still the only way around it(confirmed by unity support)

        //Just FYI, if anyone finds this thread looking for a solution. 
        MyIO.DebugLog("Shader Matrices Debug");
        //int numOfShaderMatrices = 3; // 

        //_MatrixBuffer[0] = UNITY_MATRIX_M;
        //_MatrixBuffer[1] = UNITY_MATRIX_V;
        //_MatrixBuffer[2] = UNITY_MATRIX_P;



        //if (!m_fileClosed)
        //{
        //    // get the matrix array from the compute buffer associated with m_boidInstanceMaterial
        //    // of DrawMeshInstancedIndirect()
        //    m_MatrixBuffer.GetData(m_MatrixArray);
        //    m_LEDBoidPosBuffer.GetData(m_LEDBoidPosArray);
        //    m_writer.WriteLine("main Camera: \n");

        //    for (int i = 0; i < m_numOfShaderMatrices; i++)

        //    {
        //        m_writer.WriteLine(i + "th matrix+\n" + m_MatrixArray[i]);

        //    }


        //    for (int i = 0; i < m_LEDColorGenController.m_totalNumOfLEDs; i++)

        //    {
        //        m_writer.WriteLine(i + "th LED pos=\n" + m_LEDBoidPosArray[i]);

        //    }

        //}



        //Graphics.DrawMeshInstancedIndirect(
        //    m_boidInstanceMesh,
        //    0,
        //    m_boidLEDInstanceMaterial, // This material defines the shader which receives instanceID
        //    new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //    m_boidLEDArgsBuffer, // this contains the information about the instances: see below
        //    0, null, ShadowCastingMode.Off, false, layer, m_CameraToGround
        //    );

        



        //if (!m_fileClosed)
        //{
        //    m_MatrixBuffer.GetData(m_MatrixArray);
        //    m_LEDBoidPosBuffer.GetData(m_LEDBoidPosArray);
        //    m_writer.WriteLine("cameraToGround: \n");

        //    for (int i = 0; i < m_numOfShaderMatrices; i++)

        //    {
        //        m_writer.WriteLine(i + "th matrix+\n" + m_MatrixArray[i]);

        //    }
        //    for (int i = 0; i < m_LEDColorGenController.m_totalNumOfLEDs; i++)

        //    {
        //        m_writer.WriteLine(i + "th LED pos=\n" + m_LEDBoidPosArray[i]);

        //    }

        //}




        //Graphics.DrawMeshInstancedIndirect(
        //   m_boidInstanceMesh,
        //   0,
        //   m_boidLEDInstanceMaterial, // This material defines the shader which receives instanceID
        //   new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //   m_boidLEDArgsBuffer, // this contains the information about the instances: see below
        //   0, null, ShadowCastingMode.Off, false, layer, m_CameraToCeiling
        //   );

        

        //if (!m_fileClosed)
        //{
        //    m_MatrixBuffer.GetData(m_MatrixArray);
        //    m_LEDBoidPosBuffer.GetData(m_LEDBoidPosArray);

        //    m_writer.WriteLine("CameraToCeiling: \n");

        //    for (int i = 0; i < m_numOfShaderMatrices; i++)

        //    {
        //        m_writer.WriteLine(i + "th matrix+\n" + m_MatrixArray[i]);

        //    }
        //    for (int i = 0; i < m_LEDColorGenController.m_totalNumOfLEDs; i++)

        //    {
        //        m_writer.WriteLine(i + "th LED pos=\n" + m_LEDBoidPosArray[i]);

        //    }
        //}


        //Graphics.DrawMeshInstancedIndirect(
        //   m_boidInstanceMesh,
        //   0,
        //   m_boidLEDInstanceMaterial, // This material defines the shader which receives instanceID
        //   new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //   m_boidLEDArgsBuffer, // this contains the information about the instances: see below
        //   0, null, ShadowCastingMode.Off, false, layer, m_CameraToFrontWall
        //   );

        

        //if (!m_fileClosed)
        //{
        //    m_MatrixBuffer.GetData(m_MatrixArray);
        //    m_LEDBoidPosBuffer.GetData(m_LEDBoidPosArray);

        //    m_writer.WriteLine("CameraToFrontWall: \n");

        //    for (int i = 0; i < m_numOfShaderMatrices; i++)

        //    {
        //        m_writer.WriteLine(i + "th matrix+\n" + m_MatrixArray[i]);

        //    }
        //    for (int i = 0; i < m_LEDColorGenController.m_totalNumOfLEDs; i++)

        //    {
        //        m_writer.WriteLine(i + "th LED pos=\n" + m_LEDBoidPosArray[i]);

        //    }

        //}
        if (!m_fileClosed)
        {
            m_writer.Close();
       
            m_fileClosed = true;
        }

     

        //Graphics.DrawMeshInstancedIndirect(
        //     m_boidInstanceMesh, // a mesh to be drawn
        //    0,
        //    m_boidLEDInstanceMaterial, // This material defines the parameters and buffers passed to the shader
            
        //    new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //    m_boidLEDArgsBuffer // this contains the number of instances and index coung per instance
        //);


          
       
    }

	private void OnDestroy()
	{
	

        if (m_boidLEDArgsBuffer == null) return;
        m_boidLEDArgsBuffer.Release();
        m_boidLEDArgsBuffer = null;

    }


}//class BoidLEDRendererTreeOfVoice 
