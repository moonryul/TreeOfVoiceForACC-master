using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;
using System;
using System.Runtime.InteropServices;
public class BoidRendererTreeOfVoice : MonoBehaviour
{

    const int MAX_SIZE_OF_BUFFER = 10000;
    public SimpleBoidsTreeOfVoice.BoidData[] m_boidArray;


    bool m_fileWritten= false;
    // Cameras for DrwaMeshInstanced()

    Camera m_MainCamera, m_CameraToGround, m_CameraToCeiling, m_CameraToFrontWall;

    //http://blog.deferredreality.com/write-to-custom-data-buffers-from-shaders/
    //The register(u2) represents which internal gpu registrar to bind the data structure to.
    //You need to specify the same in C#, and keep in mind this is global on the GPU.
    ComputeBuffer m_MatrixBuffer;
    int m_numOfShaderMatrices = 3; // 
    //
    // ComputeBuffer(int count, int stride, ComputeBufferType type);
    // 
    Matrix4x4[] m_MatrixArray; // arrays should be blittable. An 1d array of floats

    
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

    public bool m_useCircleMesh = false;
    public bool m_useSphereMesh = true;
    public bool m_useCylinderMesh = false;

    //[SerializeField]  Mesh _instanceMeshHuman1;
    //[SerializeField]  Mesh _instanceMeshHuman2;
    //[SerializeField]  Mesh _instanceMeshHuman3;
    //[SerializeField]  Mesh _instanceMeshHuman4;

    // public MeshSetting _meshSetting = new MeshSetting(1.0f, 1.0f);

    // int _meshNo; // set from SimpleBoids (used to be)

    public Mesh m_boidInstanceMesh;
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

    StreamWriter  m_writer;
    FileStream m_oStream;
    string m_path;

    private void Awake()
    { // initialize me

        m_boidArray = new SimpleBoidsTreeOfVoice.BoidData[MAX_SIZE_OF_BUFFER];


        m_fileWritten = false;

        // DEBUG code
        string fileName = "SimpleBoidRenderer";
        string fileIndex = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        fileIndex.Replace(" ", string.Empty);
        //fileIndex = string.Join("",
        //      fileIndex.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        m_path = "Assets/Resources/DebugFiles/" + fileName + fileIndex + ".txt";

       // File.CreateText(path).Dispose();
        //Write some text to the test.txt file
        //m_writer = new StreamWriter(path, false); // do not append
        //m_ioStream = new FileStream(path,
        //                               FileMode.OpenOrCreate,
        //                               FileAccess.ReadWrite,
        //                               FileShare.None);

     
        //m_oStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
        //m_writer = new System.IO.StreamWriter(m_oStream);


        //var oStream = new FileStream(test, FileMode.Append, FileAccess.Write, FileShare.Read);
        //var iStream = new FileStream(test, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        // Find the cameras for DrawMeshIntanced()

        m_MainCamera  = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        m_CameraToGround = GameObject.FindWithTag("CameraToGround").GetComponent<Camera>();
        m_CameraToCeiling = GameObject.FindWithTag("CameraToCeiling").GetComponent<Camera>();
        m_CameraToFrontWall = GameObject.FindWithTag("CameraToFrontWall").GetComponent<Camera>();
       
        if ( m_MainCamera ==null)
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



        // check if the global component object is defined
        if (m_boidInstanceMaterial == null)
        {
            Debug.LogError("The global Variable _boidInstanceMaterial is not  defined in Inspector");
            // EditorApplication.Exit(0);
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
# endif   
        }

                     

      

        m_boidArgsBuffer = new ComputeBuffer(
            1, // count
            m_boidArgs.Length * sizeof(uint),

            ComputeBufferType.IndirectArguments
        );

        

        if (m_useCircleMesh) // use 2D boids => creat the mesh in a script
        {
            m_instanceMeshCircle = new CircleMesh(unitRadius);

            m_boidInstanceMesh = m_instanceMeshCircle.m_mesh;


            //for (int i = 0; i < m_boidInstanceMesh.vertexCount; i++)
            //{

            //    //     The normals of the Mesh.
            //    //  public Vector3[] normals { get; set; }
            //    m_boidInstanceMesh.normals[i] *= -1; // reverse the direction of the normal for 2D circle mesh

            //}

        }

        else if (m_useSphereMesh) 
        {
            // Use the sphere mesh
            if (m_instanceMeshSphere == null)
            {

                Debug.LogError("_instanceMeshSphere is not  defined in Inspector");
                // EditorApplication.Exit(0);
#if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
# endif   
            }
            else
            {
                m_boidInstanceMesh = m_instanceMeshSphere;
                m_boidInstanceMesh.RecalculateNormals();
                //                Normals are calculated from all shared vertices.

                //                 Meshes sometimes don't share all vertices. For example,
                //                 a vertex at a UV seam is split into two vertices, so the RecalculateNormals function creates 
                //                 normals that are not smooth at the UV seam.

                // draw the mesh inforation

                int numOfVertices = m_boidInstanceMesh.vertexCount;
                using (m_writer = File.CreateText(m_path))
                {
                    for (int i = 0; i < numOfVertices; i++)
                    {
                        m_writer.WriteLine(i + "th vertex pos=" + m_boidInstanceMesh.vertices[i]);
                        m_writer.WriteLine(i + "th vertex normal=" + m_boidInstanceMesh.normals[i]);


                    }
                } // using 
            }


        }

        else if (m_useCylinderMesh)
        {
           
                m_instanceMeshCylinder = new CylinderMesh(height, radius, nbSides, nbHeightSeg);

                if (m_instanceMeshCylinder == null)
                {
                    Debug.LogError("_instanceCylinderSphere is not created well");
                // EditorApplication.Exit(0);
#if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
# endif   
                }
                else
                {
                  m_boidInstanceMesh = m_instanceMeshCylinder.m_mesh;
                  //m_boidInstanceMesh.RecalculateNormals();
                //                Normals are calculated from all shared vertices.

                //                 Meshes sometimes don't share all vertices. For example,
                //                 a vertex at a UV seam is split into two vertices, so the RecalculateNormals function creates 
                //                 normals that are not smooth at the UV seam.
            }


        }//if (m_useCylinderMesh)


        else
        {
            Debug.LogError("useCircleMesh or useSphereMesh or useCylinderMesh should be checked");
            //If we are running in a standalone build of the game
#if UNITY_STANDALONE
            //Quit the application
            Application.Quit();
#endif

            //If we are running in the editor
#if UNITY_EDITOR
            //Stop playing the scene
            // UnityEditor.EditorApplication.isPlaying = false;
            //Setting isPlaying delays the result until after all script code has completed for this frame.

            EditorApplication.Exit(0);
#endif
        }



        //Debug.Log("number of indices=");
        //Debug.Log(_instanceMesh.GetIndexCount(0));


    }
    void Start () 
	{   // initialize others



        //_MatrixBuffer[0] = UNITY_MATRIX_M;
        //_MatrixBuffer[1] = UNITY_MATRIX_V;
        //_MatrixBuffer[2] = UNITY_MATRIX_P;


        //ComputeBufferType.Default: In HLSL shaders, this maps to StructuredBuffer<T> or RWStructuredBuffer<T>.
        //m_MatrixBuffer = new ComputeBuffer(m_numOfShaderMatrices,
        //                                   Marshal.SizeOf(typeof(Matrix4x4)), 
        //                                    ComputeBufferType.Default);

        ////(4*4) * sizeof(float) == Marshal.SizeOf(typeof(Matrix4x4))
        ///
        // Set the ComputeBuffer for shader debugging
        // But a RWStructuredBuffer, requires SetRandomWriteTarget to work at all in a non-compute-shader. 
        //This is all Unity API magic which in some ways is convenient 

        // for debugging
       // Graphics.SetRandomWriteTarget(1, m_MatrixBuffer);
        
        //SetRandomWriteTarget(int index, ComputeBuffer uav, bool preserveCounterValue = false);
        // The "1" represents the target index ie u1.
        // Uses "unordered access views" (UAV) in UsingDX11GL3Features. 
        // These "random write" targets are set similarly to how multiple render targets are set.
       


        //m_MatrixArray = new Matrix4x4[m_numOfShaderMatrices];


        //m_MatrixBuffer.SetData(m_MatrixArray);
       
        
        // get the reference to SimpleBoidsTreeOfVoice

        m_boids = this.gameObject.GetComponent<SimpleBoidsTreeOfVoice>();
    

        if (m_boids == null)
        {
            Debug.LogError("SimpleBoidsTreeOfVoice component should be attached to CommHub");
            //EditorApplication.Exit(0);
            // Application.Quit();
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
                   Application.Quit();
# endif            
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

        //Graphics.SetRandomWriteTarget(1, m_boids.m_BoidBuffer);

        m_boidInstanceMaterial.SetBuffer("_BoidBuffer", m_boids.m_BoidBuffer);
        // m_boids.BoidBuffer is ceated in SimpleBoids.cs
        // This buffer is shared between CPU and GPU

        // for debugging
        //m_boidInstanceMaterial.SetBuffer("_MatrxiBuffer", m_MatrixBuffer);



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
        //int layer = 0;
        //Graphics.DrawMeshInstancedIndirect(
        //   m_boidInstanceMesh,
        //   0,
        //   m_boidInstanceMaterial, // This material defines the shader which receives instanceID
        //   new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //   m_boidArgsBuffer, // this contains the information about the instances: see below
        //   0, null, ShadowCastingMode.Off, false, layer, m_CameraToCeiling
        //   );


        Graphics.DrawMeshInstancedIndirect(
             m_boidInstanceMesh, // a mesh to be drawn
            0,
            m_boidInstanceMaterial, // This material defines the parameters and buffers passed to the shader

            new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
            m_boidArgsBuffer // this contains the number of instances and index coung per instance
        );


        //   public static void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex,
        //Material material, Bounds bounds, ComputeBuffer bufferWithArgs, 
        //int argsOffset, MaterialPropertyBlock properties,
        //       ShadowCastingMode castShadows, bool receiveShadows, int layer, Camera camera);

        //It allows drawing from a compute buffer that has draw parameters stored
        //    (instance counts, vert counts), and in your shader you can setup 
        //    per-instance data from a computer buffer(instead of using constant buffers,
        //    as regular GPU instancing does now).

        //At the current stage, you would need to assign a "instance index" as instanced property
        //    to all instances, and use this property to access to a compute buffer.
        //    .

        //int layer = 0; // Default layer

        //Graphics.DrawMeshInstancedIndirect(
        //    m_boidInstanceMesh,
        //    0,
        //    m_boidInstanceMaterial, // This material defines the shader which receives instanceID
        //    new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //    m_boidArgsBuffer, // this contains the information about the instances: see below
        //    0, null, ShadowCastingMode.Off, false, layer, null // m_MainCamera
        //    );

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
        // MyIO.DebugLog("Shader Matrices Debug");
        //int numOfShaderMatrices = 3; // 

        //_MatrixBuffer[0] = UNITY_MATRIX_M;
        //_MatrixBuffer[1] = UNITY_MATRIX_V;
        //_MatrixBuffer[2] = UNITY_MATRIX_P;

        // for debugging



        //if (!m_fileClosed)
        //{
        //    // get the matrix array from the compute buffer associated with m_boidInstanceMaterial
        //    // of DrawMeshInstancedIndirect()
        //    //m_MatrixBuffer.GetData(m_MatrixArray);
        //m_boids.m_BoidBuffer.GetData(m_boidArray);

        //if (!m_fileWritten)
        //{
        //    using (m_writer = File.CreateText(m_path))
        //    {


        //        //m_writer.WriteLine("Iteration Num of Simuation:" + totalNumOfSimulations);
        //        //m_writer.WriteLine("m_SimulationDeltaT of action plan:" + m_SimulationDeltaT);
        //        //StreamWriter(string path, bool append);
        //        //writer.WriteLine("Test");
        //        // m_boids.m_BoidBuffer


        //        for (int i = 0; i < (int)m_boids.m_BoidsNum / 100; i++)
        //        {

        //            //Debug.Log("boidNo = "); Debug.Log(i);
        //            m_writer.WriteLine("boidNo = " + i);
        //            //Debug.Log("boid Wall No = ");
        //            m_writer.WriteLine("boid Wall No = " + m_boidArray[i].WallNo);

        //            Debug.Log(m_boidArray[i].WallNo);
        //            Debug.Log("position = = ");
        //            Debug.Log(m_boidArray[i].Position);


        //            m_writer.WriteLine("position = = " + m_boidArray[i].Position);

        //            m_writer.WriteLine("normal = = " + m_boidArray[i].Normal);
        //        }
        //    } // using 

        //    m_fileWritten = true;
        //} // if (!m_fileWritten)


        //    m_writer.WriteLine("main Camera: \n");

        //    for (int i = 0; i < m_numOfShaderMatrices; i++)

        //    {
        //        m_writer.WriteLine(i + "th matrix+\n" + m_MatrixArray[i]);

        //    }

        //}

        //Graphics.DrawMeshInstancedIndirect(
        //    m_boidInstanceMesh,
        //    0,
        //    m_boidInstanceMaterial, // This material defines the shader which receives instanceID
        //    new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //    m_boidArgsBuffer, // this contains the information about the instances: see below
        //    0, null, ShadowCastingMode.Off, false, layer, m_CameraToGround
        //    );




        //if (!m_fileClosed)
        //{
        //    m_MatrixBuffer.GetData(m_MatrixArray);
        //    m_writer.WriteLine("cameraToGround: \n");

        //    for (int i = 0; i < m_numOfShaderMatrices; i++)

        //    {
        //        m_writer.WriteLine(i + "th matrix+\n" + m_MatrixArray[i]);

        //    }
        //}




        //int layer = 0;
        //Graphics.DrawMeshInstancedIndirect(
        //   m_boidInstanceMesh,
        //   0,
        //   m_boidInstanceMaterial, // This material defines the shader which receives instanceID
        //   new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //   m_boidArgsBuffer, // this contains the information about the instances: see below
        //   0, null, ShadowCastingMode.Off, false, layer, m_CameraToCeiling
        //   );



        //if (!m_fileClosed)
        //{
        //    m_MatrixBuffer.GetData(m_MatrixArray);
        //    m_writer.WriteLine("CameraToCeiling: \n");

        //    for (int i = 0; i < m_numOfShaderMatrices; i++)

        //    {
        //        m_writer.WriteLine(i + "th matrix+\n" + m_MatrixArray[i]);

        //    }
        //}




        //Graphics.DrawMeshInstancedIndirect(
        //   m_boidInstanceMesh,
        //   0,
        //   m_boidInstanceMaterial, // This material defines the shader which receives instanceID
        //   new Bounds(m_boids.RoomCenter, m_boids.RoomSize),
        //   m_boidArgsBuffer, // this contains the information about the instances: see below
        //   0, null, ShadowCastingMode.Off, false, layer, m_CameraToFrontWall
        //   );



        //if (!m_fileClosed)
        //{
        //    m_MatrixBuffer.GetData(m_MatrixArray);
        //    m_writer.WriteLine("CameraToFrontWall: \n");

        //    for (int i = 0; i < m_numOfShaderMatrices; i++)

        //    {
        //        m_writer.WriteLine(i + "th matrix+\n" + m_MatrixArray[i]);

        //    }

        //}



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

}
