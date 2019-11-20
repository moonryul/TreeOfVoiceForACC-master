
#define UNITY_EDITOR
#define TRACE_ON

using UnityEngine;
using System.Collections;
using System;

using UnityEditor;
using System.IO;



//In Java as well as in c# there is no separate method declaration.    
//The declaration of the method is done with its implementation.You also do not need to keep track of 
//    file includes    so that the classes know about eachother as long as they are in the same namespace.

//RequireComponent automatically adds the required component to the 
// the gameObject to which the Script component will be added.

// Debug Shaders:
// https://forum.unity.com/threads/how-to-print-shaders-var-please.26052/
// https://docs.unity3d.com/ScriptReference/Material.GetMatrix.html

// GameObjects and Components: Only Components(MonoBehaviours) need to be attached to GameObjects, and if fact only they CAN be.
//The great majority of scripts in games are not Components, but data objects and utility classes for performing operations on data objects, 
//    like with any other program.You can create a normal non-MonoBehaviour script by just right clicking in the Project files tab and saying 
//    Create C# Script, naming the script, opening it in an IDE (double clicking it works), and deleting the part where it derives from MonoBehaviour.
//    If it doesn't derive from MonoBehaviour, then it's not a component, and you don't need to / can't attach it to GameObject. 


[RequireComponent (typeof (MeshCollider))]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class Pyramid : MonoBehaviour
 {

    string mPath = "Assets/Resources/debug.txt";
    StreamReader mReader; 
    StreamWriter mWriter;
 

    //This is Main Camera in the Scene
    public Camera mMainCamera; // Camera is a component
    Renderer mPyramidRenderer;
    Material mPyramidMaterial;

    Mesh mMesh; // mesh for the pyramid

    public Cylinder mCylinder;   //The value of mCylinder will be set in the inspector

    ComputeBuffer mIntersectionBuffer, mPosInCameraBuffer, mPosInClipBuffer, mNormalInCameraBuffer, mUVMapBuffer;

    //
    // ComputeBuffer(int count, int stride, ComputeBufferType type);
    // 
    Vector4[] mvInter, mvPosInCam, mvPosInClip, mvNormalInCam;
    //
    Vector2[] mUVMap;      // these arrays should be blittable. An 1d array of floats
    

    // Pyramid component has been attached to the "Pyradmid" gameObject in the 
    // Pyradmid class.

   


    [System.Serializable]
    public struct PyramidParam
    {
        public float Height;
        //public Vector3 Origin; // Origin is (0,0,0) all the time
        public float Width;
        public float Depth;
    }



    [SerializeField, Header("Pyramid Parameters"), Space(20)]
    public PyramidParam mPyramidParam =  // use "object initializer syntax" to initialize the structure:https://www.tutorialsteacher.com/csharp/csharp-object-initializer
                                         // See also: https://stackoverflow.com/questions/3661025/why-are-c-sharp-3-0-object-initializer-constructor-parentheses-optional

      new PyramidParam
      {
          Height = 0.1f,  // the length unit is  meter
         // Origin  = new Vector3(0f,0f,0f),
          Width = 0.2f,
          Depth = 0.1f
      };


    Vector3[] MyCalNormals(Mesh mesh)
    {
        Vector3[] normals = new Vector3[mesh.vertices.Length];

        // Use the list of vertex indices for each triangle to compute the normal to the triangle
        // and use it as the normal to the vertices of the triangle

        for (int i = 0; i < mesh.triangles.Length; ++i)
        {

            if ( (i + 1) % 3 == 0)
            {
                int i0 = mesh.triangles[i-2];    // get the index to the vertex for the (i-1) th item in mesh.triangles

                int i1 = mesh.triangles[i-1];
                int i2 = mesh.triangles[i];

                Vector3 ab = mesh.vertices[i1] - mesh.vertices[i0];
                Vector3 bc = mesh.vertices[i2] - mesh.vertices[i1];
                Vector3 normal = Vector3.Cross(ab, bc).normalized; // Cross() is defined by the Left-Hand Rule
                normals[i-2] = normal; normals[i -1] = normal; normals[i ] = normal;
            }
        }

        return normals;

    } // MyCalNormals()


    public void Rebuild()
    {
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        // The  "gameObject" field of the Pyramid Script object is created when this Script (actually an instance of it)
        // is attached a particular object, which is the Pyramid gameObject in this case.   
		if (meshFilter==null){
			Debug.LogError("MeshFilter not found!");
			return;
		}


        mMesh = meshFilter.sharedMesh;

        if (mMesh == null)
        {
            meshFilter.mesh = new Mesh();
            mMesh = meshFilter.sharedMesh;
        }
        mMesh.Clear();


        // Refer to https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/

        //Create a pyramid: https://www.reddit.com/r/Unity3D/comments/3e1rxy/beginner_question_creating_a_pyramid/
        // To understand the structure of a mesh, Read https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/
        // Also: 
        // Refer to http://ilkinulas.github.io/development/unity/2016/04/30/cube-mesh-in-unity3d.html
        // https://catlikecoding.com/unity/tutorials/procedural-grid/
        //https://answers.unity.com/questions/441722/splitting-up-verticies.html

        // Create a mesh for the Pyramid within the Script rather than importing the predefined pyradmid made by 3D tools e.g. Maya

        // create 5 vertices for the pyramid

        Vector3 p0 = new Vector3(-0.5f * mPyramidParam.Width, 0.0f, -0.5f * mPyramidParam.Depth); // the left/rear corner of the base
        Vector3 p1 = new Vector3(0.5f * mPyramidParam.Width, 0.0f, -0.5f * mPyramidParam.Depth); // the right/rear   corner
        Vector3 p2 = new Vector3(0.5f * mPyramidParam.Width, 0.0f, 0.5f * mPyramidParam.Depth); // the right/front corner
        Vector3 p3 = new Vector3(-0.5f * mPyramidParam.Width, 0.0f, 0.5f * mPyramidParam.Depth);              // the left/front corner
        Vector3 p4 = new Vector3(0.0f, mPyramidParam.Height, 0.0f); // the apex

        mMesh.vertices = new Vector3[]  // duplicate the vertices so that each triangle has its own 
                                       // p0 is duplicated 4 times, p2 4 times, p1 and p3 3 times, p4 4 times => 18 vertices 
        {
            p0, p1, p2,                 // the rear triangle of the base 
            p0, p2, p3,                 // the front triangle of the base
            p0, p1,p4,                  // the rear side triangle
            p1, p2, p4,                 // the right side triangle
            p2, p3, p4,                  // the front side triangle 
            p3, p0, p4                   // the left side triangle
        };

        mMesh.triangles = new int[]  // The set of indices for each triangle: the normal of the triangle is determined by the left-hand rule
                                    
        {
            0, 1, 2,
            3, 4, 5,
            8, 7, 6,
            11, 10, 9,
            14, 13, 12,
            17, 16, 15
        };

        mMesh.RecalculateNormals(); // calculate the normal of each vertex using the face normals. 
                                   // Because no vertices are shared among the adjacent triangles, the normals to the vertcies are not 
                                   // interpolated among the adjacent triangles.

        Vector3[] myNormals = MyCalNormals(mMesh);

        // Print the normals calculated by MyCalNormals() and mesh.RecalculateNormals() to 
        // compare them.


        MyIO.DebugLog("mesh.RecalNormals() versus myCalNormals():");
        for (int i=0; i < mMesh.vertices.Length; i++)
        {
            MyIO.DebugLog("vertex index =" + i + ":" + "normal=");
            MyIO.DebugLog(mMesh.normals[i]);
            MyIO.DebugLog(myNormals[i]);
        }
         
        //mesh.RecalculateNormals();
		mMesh.RecalculateBounds();

    }  //public void Rebuild()


    //  OnValidate function is called in editor mode only on the following cases:
    //  (1) once script loaded;
   //   (2) if any value is changed in inspector

    void OnValidate()
    {
        Rebuild(); // rebuilds the pyramid using the changed paramater values

    }

    // Update is called once per frame
    void OnPostRender()
    {
        mIntersectionBuffer.GetData(mvInter);
        mPosInCameraBuffer.GetData(mvPosInCam);
        mPosInClipBuffer.GetData(mvPosInClip);
        mNormalInCameraBuffer.GetData(mvNormalInCam);

        mUVMapBuffer.GetData(mUVMap);

    

        //  float[] v = new float[Camera.main.pixelWidth * Camera.main.pixelHeight * 3];
        Vector4 pixel = new Vector4();
        string text = "";


        MyIO.DebugLog("Intersection Debug" );
        MyIO.DebugLog("Intersection Debug");

        for (int i = 0; i < mMesh.triangles.Length; i++)
          
            {
            //pixel[0] = mvInter[i * 4 + 0];
            //pixel[1] = mvInter[i * 4 + 1];
            //pixel[2] = mvInter[i * 4 + 2];
            //pixel[3] = mvInter[i * 4 + 3];

            pixel = mvInter[i];

            text = "vertex index =" + i + ":";

                MyIO.DebugLog(text);
                MyIO.DebugLog("intersection point:");
                MyIO.DebugLog(pixel);


               // mWriter.WriteLine(text + pixel.ToString() );
            
                                         


            }



        MyIO.DebugLog("Pos In Cam  Debug");
        MyIO.DebugLog("Pos In Cam  Debug");
        for (int i = 0; i < mMesh.triangles.Length; i++)

        {
            //pixel[0] = mvPosInCam[i * 4 + 0];
            //pixel[1] = mvPosInCam[i * 4 + 1];
            //pixel[2] = mvPosInCam[i * 4 + 2];
            //pixel[3] = mvPosInCam[i * 4 + 3];

            pixel = mvPosInCam[i];

            text = "vertex index=" + i + ":";

            MyIO.DebugLog(text);
            MyIO.DebugLog("Pos In Camera:");
            MyIO.DebugLog(pixel);


            //mWriter.WriteLine(text + pixel.ToString());




        }

        MyIO.DebugLog("Pos In Clip   Debug");
        MyIO.DebugLog("Pos In Clip  Debug");

        for (int i = 0; i < mMesh.triangles.Length; i++)

        {
            //pixel[0] = mvPosInClip[i * 4 + 0];
            //pixel[1] = mvPosInClip[i * 4 + 1];
            //pixel[2] = mvPosInClip[i * 4 + 2];
            //pixel[3] = mvPosInClip[i * 4 + 3];


            pixel = mvPosInClip[i];

            text = "vertex index" + i + ":";

            MyIO.DebugLog(text);
            MyIO.DebugLog("Pos In Clip:");
            MyIO.DebugLog(pixel);


          //  mWriter.WriteLine(text + pixel.ToString());




        }

        MyIO.DebugLog("Nomral in Cam  Debug");
        MyIO.DebugLog("Normal in Cam  Debug");


        for (int i = 0; i < mMesh.triangles.Length; i++)

        {
            //pixel[0] = mvNormalInCam[i * 4 + 0];
            //pixel[1] = mvNormalInCam[i * 4 + 1];
            //pixel[2] = mvNormalInCam[i * 4 + 2];
            //pixel[3] = mvNormalInCam[i * 4 + 3];


            pixel = mvNormalInCam[i];
            text = "vertex index" + i + ":";

            MyIO.DebugLog(text);
            MyIO.DebugLog("Normal In Camera:");
            MyIO.DebugLog(pixel);


            //  mWriter.WriteLine(text + pixel.ToString());

        }

            MyIO.DebugLog("UVMap  Debug");
            MyIO.DebugLog("UVMap Debug");

        //ndc.y* _ScreenWidth +ndc.x * _ScreenHeight
            for (int i = 0; i < mMainCamera.pixelWidth; i++)
            for (int j = 0; j < mMainCamera.pixelHeight; j++)

            {

                //Vector2 texCoord = mUVMap[ (j * mMainCamera.pixelWidth + i) * 2 + 0];
                //pixel[1] = mUVMap[ (j * mMainCamera.pixelWidth + i) * 2 + 1];
                //pixel[2] = 0.0f;
                //pixel[3] = 0.0f;

                pixel = new Vector4( mUVMap[(j * mMainCamera.pixelWidth + i) ].x, mUVMap[(j * mMainCamera.pixelWidth + i)].y,
                                     0.0f,0.0f);

                text = "pixel coord =" + "("  + i + "," + j + ")";

                MyIO.DebugLog(text);
                MyIO.DebugLog("tex uv coord:");
                MyIO.DebugLog(pixel);


                // mWriter.WriteLine(text + pixel.ToString() );




            }



      
        // mWriter.Close(); // 

    } // OnPostRender()

    //garbage collector extra 
    //OnDestroy occurs when a Scene or game ends.Stopping the Play mode when running from inside the Editor will end the application.
    //    As this end happens an OnDestroy will be executed.Also, if a Scene is closed and a new Scene is loaded the OnDestroy call 
    //    will be made.   When built as a standalone application OnDestroy calls are made when Scenes end.
    //    A Scene ending typically means a new Scene is loaded.

    void OnDestroy()
    {
        if (mIntersectionBuffer != null) 
            mIntersectionBuffer.Dispose();

        if (mPosInCameraBuffer != null)
            mPosInCameraBuffer.Dispose();

        if (mPosInClipBuffer != null)
            mPosInClipBuffer.Dispose();

        if (mNormalInCameraBuffer != null)
            mNormalInCameraBuffer.Dispose();


    }

    void Start()
    {
       
        mWriter = new StreamWriter(mPath, true);
       // mReader = new StreamReader(mPath, true);



        if (ReferenceEquals(mMainCamera, null))
        {
            MyIO.DebugLog("mMainCamera component  is not set in the inspector; Stop the process");
            return;


        }
       

        //This enables Main Camera
        mMainCamera.enabled = true;

        // Set the material properties of the Cylinder  gameObject

        if ( ReferenceEquals( mCylinder, null) )
        {
            MyIO.DebugLog("mCylinder gameObject is not set in the inspector; Stop the process");
            return;


        }


        mPyramidRenderer = gameObject.GetComponent<MeshRenderer>();

        mPyramidMaterial = mPyramidRenderer.material;

        if ( ReferenceEquals( mPyramidMaterial,  null) ) // Is material null?
        {
            MyIO.DebugLog("The material for Pyramid's Renderer is not set in the inspector; Stop the process");
            return;

        }

        //https://answers.unity.com/questions/1192139/projection-matrix-in-unity.html
        // https://docs.unity3d.com/ScriptReference/GL.GetGPUProjectionMatrix.html
        // off-center projectionMatrix (OpenGL style): 
        // [ 2n/r-l     0      r+l/r-l        0
        //   0      2n/t-b    t+b/t-b        0
        //   0         0      -(f+n)/f-n  -2fn/f-n
        //  0         0        -1           0    ] 
        // == // [ 2n/r-l     0      0/r-l        0
        //   0      2n/t-b    0/t-b        0
        //   0         0      -(f+n)/f-n  -2fn/f-n
        //  0         0        -1           0    ]  
        //So "forward" in OpenGL is "-z".In Unity forward is "+z".Most hand - rules you might know from math are inverted in Unity
        //    .For example the cross product usually uses the right hand rule c = a x b where a is thumb, b is index finger and c is the middle
        //    finger.In Unity you would use the same logic, but with the left hand.

        //    However this does not affect the projection matrix as Unity uses the OpenGL convention for the projection matrix.
        //    The required z - flipping is done by the cameras worldToCameraMatrix.
        //    So the projection matrix should look the same as in OpenGL.

        //    By using my ProjectionMatrixEditorWindow you can view(get) and edit(set) the projection matrix of 
        //    the main camera inside the editor. 
        //    This is an editor script so just place it in a folder called "editor".You can open the window 
        //    via menu(Tools --> ProjectionMatrixEditor).

        //    As you can see the resulting matrix looks similar to the one you've posted. However in Unity L and R (as well as T and B) 
        //    are always of equal size so the (R+L) term (as well as the (T+B) term) result in "0".

        //     Unity has an example script in the docs how to setup a custom off center perspective matrix.

        //    Unity can run on different platforms using different APIs (DirectX / OpenGL) the actual projection matrix representation
        //     inside the GPU might be different from the representation you use in Unity.
        //    However you don't have to worry about that since Unity handles this automatically. The only case where it does matter when you directly pass a matrix from your code to a shader. In that case Unity offers the method GL.GetGPUProjectionMatrix which converts the given projection matrix into the right format used by the GPU.

        //   So to sum up how the MVP matrix is composed:

        //   M = transform.localToWorldMatrix of the object

        //   V = camera.worldToCameraMatrix

        //   P = GL.GetGPUProjectionMatrix(camera.projectionMatrix)

        //   MVP = P V M

        mPyramidMaterial.SetFloat("_CylinderHeight", mCylinder.mCylinderParam.height );
        mPyramidMaterial.SetFloat("_CylinderRadius", mCylinder.mCylinderParam.radius);

        //transform.position = position	The world space position of the Transform.
        //transform.localToWorldMatrix = Matrix that transforms a point from local space into world space (Read Only).

        Matrix4x4 cylinderLocalToWorldMatrix = mCylinder.gameObject.transform.localToWorldMatrix;

        MyIO.DebugLog("cylinderLocalToWorldMatrix");
        MyIO.DebugLog( cylinderLocalToWorldMatrix);

        // Transform the origin coord into the OpenGL camera space, because we will use the Opengl camera space coords in the shader
        // https://stackoverflow.com/questions/2624422/efficient-4x4-matrix-inverse-affine-transform
        // You should be able to exploit the fact that the matrix is affine to speed things up over a full inverse. Namely, if your matrix looks like this

        // A = [M   b]
        //     [0   1]
        //where A is 4x4, M is 3x3, b is 3x1, and the bottom row is (0, 0, 0, 1), then

        // inv(A) = [inv(M) - inv(M) * b]
        //            [0              1]
        // WorldToCameraMatrix = inv(CameraToWorldMatrix)

        // refer to https://docs.unity3d.com/ScriptReference/Camera-worldToCameraMatrix.html about 
        //mMainCamera.worldToCameraMatrix
        MyIO.DebugLog("worldToCameraMatrix");
        MyIO.DebugLog(mMainCamera.worldToCameraMatrix);

        Vector4 cylinderOriginInCamera = mMainCamera.worldToCameraMatrix * cylinderLocalToWorldMatrix *
                                         new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

        MyIO.DebugLog("cylindeOriginInCamera");
        MyIO.DebugLog(cylinderOriginInCamera);

        mPyramidMaterial.SetVector("_CylinderOriginInCamera", cylinderOriginInCamera);


        //cf. _MainTex("CylinderTexture", 2D) = "white" { }

        //// Cylinder properties should be included: MOON

        //_CylinderOrigin("Cylinder Origin", Vector) = (0.0, 0.0, 0.0, 0.0)

        //_CylinderHeight("Cylinder Height", Float) = 0.0

        //_CylinderRadius("Cylinder Radius", Float) = 0.0



        //ComputeBufferType.Default: In HLSL shaders, this maps to StructuredBuffer<T> or RWStructuredBuffer<T>.
        mIntersectionBuffer = new ComputeBuffer(mMesh.triangles.Length, 4 * sizeof(float), ComputeBufferType.Default);
        mPosInCameraBuffer = new ComputeBuffer(mMesh.triangles.Length, 4 * sizeof(float), ComputeBufferType.Default);
        mPosInClipBuffer = new ComputeBuffer(mMesh.triangles.Length, 4 * sizeof(float), ComputeBufferType.Default);
        mNormalInCameraBuffer = new ComputeBuffer(mMesh.triangles.Length, 4 * sizeof(float), ComputeBufferType.Default);

        mUVMapBuffer = new ComputeBuffer(mMainCamera.pixelWidth * mMainCamera.pixelHeight , 2*sizeof(float), ComputeBufferType.Default);

        // Set the ComputeBuffer for shader debugging
        // But a RWStructuredBuffer, requires SetRandomWriteTarget to work at all in a non-compute-shader. 
        //This is all Unity API magic which in some ways is convenient 

        Graphics.SetRandomWriteTarget(1, mIntersectionBuffer);
        Graphics.SetRandomWriteTarget(2, mPosInCameraBuffer);
        Graphics.SetRandomWriteTarget(3, mPosInClipBuffer);
        Graphics.SetRandomWriteTarget(4, mNormalInCameraBuffer);

        Graphics.SetRandomWriteTarget(5, mUVMapBuffer);

        //SetRandomWriteTarget(int index, ComputeBuffer uav, bool preserveCounterValue = false);
        // The "1" represents the target index ie u1.
        // Uses "unordered access views" (UAV) in UsingDX11GL3Features. 
        // These "random write" targets are set similarly to how multiple render targets are set.
        // initial fill buffer with red pixels


        ////mvInter = new float[mMesh.triangles.Length * 4];
        ////mvPosInCam = new float[mMesh.triangles.Length * 4];
        ////mvPosInClip = new float[mMesh.triangles.Length * 4];
        ////mvNormalInCam = new float[mMesh.triangles.Length * 4];

        ////mUVMap = new float[mMainCamera.pixelWidth * mMainCamera.pixelHeight * 2];



        mvInter = new Vector4[mMesh.triangles.Length];
        mvPosInCam = new Vector4[mMesh.triangles.Length];
        mvPosInClip = new Vector4[mMesh.triangles.Length];
        mvNormalInCam = new Vector4[mMesh.triangles.Length];

        mUVMap = new Vector2[mMainCamera.pixelWidth * mMainCamera.pixelHeight];


        mIntersectionBuffer.SetData(mvInter);
        mPosInCameraBuffer.SetData(mvPosInCam);
        mPosInClipBuffer.SetData(mvPosInClip);
        mNormalInCameraBuffer.SetData(mvNormalInCam);

        mUVMapBuffer.SetData(mUVMap);

        mPyramidMaterial.SetInt("_ScreenWidth", mMainCamera.pixelWidth);
        mPyramidMaterial.SetInt("_ScreenHeight", mMainCamera.pixelHeight);

        mPyramidMaterial.SetBuffer("_IntersectionBuffer", mIntersectionBuffer);
        mPyramidMaterial.SetBuffer("_PosInCameraBuffer", mPosInCameraBuffer);
        mPyramidMaterial.SetBuffer("_PosInClipBuffer", mPosInClipBuffer);
        mPyramidMaterial.SetBuffer("_NormalInCameraBuffer", mNormalInCameraBuffer);

        mPyramidMaterial.SetBuffer("_UVMapBuffer", mUVMapBuffer);

        //Graphics.ClearRandomWriteTargets();

    } // Start()

// Update is called once per frame
void Update ()
    {
       OnPostRender();
    }

}  // class Pyramid
