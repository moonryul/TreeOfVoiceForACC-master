
#define UNITY_EDITOR
#define TRACE_ON

using UnityEngine;
using System.Collections;
using System;
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

    

public class ProjectorParamScript : MonoBehaviour
 {

    public Pyramid mPyramid;

    //This is Main Camera in the Scene
    public Camera mMainCamera;

  
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("TRACE_ON")]
    void DebugLog(string str)
    {
        //UnityEngine.Debug.LogFormat("Number: {0}, string: {1}, number again: {0}, character: {2}", num, str, chr);
       UnityEngine.Debug.Log(str);

    }   //void DebugLog(string str)


    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("TRACE_ON")]

    void DebugLogVector(Vector3 vector)
    {
      
        Debug.Log(vector.ToString("F4"));
       
    }   //void DebugLogVector()

    void DebugLogVector(Vector4 vector)
    {

        Debug.Log(vector.ToString("F4"));

    }   //void DebugLogVector()


    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("TRACE_ON")]
    void DebugLogMatrix(Matrix4x4  mat)     // struct Matrix4x4 mat; mat[i,j]
    {
        int rowLength = 4;
        int colLength = 4; 
        string arrayString = "";
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                arrayString += string.Format("{0} ", mat[i, j]);
            }
            arrayString += System.Environment.NewLine + System.Environment.NewLine;
        }

        Debug.Log(arrayString);



    }   //void DebugLogVector()

    Vector3[] ComputeNormals(float Hight, float Width, float Depth)
    {
        Vector3[] normals = new Vector3[4]; 

        // compute the normal to each side of the pyramid

        normals[0] = new Vector3(0.0f, 0.0f, 0.0f);
        return normals;

    }
   



    void Start()
    {
        //This gets the Main Camera from the Scene
        mMainCamera = Camera.main;  // main is a static variable

        Material material  = gameObject.GetComponent<Projector>().material; // gameObject = Projector gameObject 
                                                                            // to which Projector component is attached

        if ( ReferenceEquals( material,  null) ) // Is material null?
        {
            DebugLog("The material for the projector component  is not set in the inspector; Stop the process");
            return;

        }

        // The parameters of the projector shader
       // _PyramidApex("Pyramid Apex", Vector) = (0.0, 0.0, 0.0, 1.0)

        //_PyramidBaseWidth("Pyramid Base Width", Float = 0.0

        //_PyramidBaseDepth("Pyramid Base Width", Float = 0.0

        //float3 _PyramidNormals[4]; /

    
        material.SetFloat("_PyramidWidth", mPyramid.mPyramidParam.Width);
        material.SetFloat("_PyramidDepth", mPyramid.mPyramidParam.Depth);



        //transform.position = position	The world space position of the Transform.
        //transform.localToWorldMatrix = Matrix that transforms a point from local space into world space (Read Only).

        Matrix4x4 pyramidLocalToWorldMatrix = mPyramid.gameObject.transform.localToWorldMatrix;

        DebugLog("pyramidLocalToWorldMatrix");
        DebugLogMatrix(pyramidLocalToWorldMatrix);

        // Transform the origin coord into the OpenGL camera space, because we will use the Opengl camera space coords in the shader

        Vector4 mPyramidApex = new Vector4(0.0f, mPyramid.mPyramidParam.Height, 0.0f, 1.0f);
        Vector4 pyramidApexInCamera = mMainCamera.worldToCameraMatrix * pyramidLocalToWorldMatrix * mPyramidApex;
                                        
        

        DebugLog("pyramidApexInCamera");
        DebugLogVector(pyramidApexInCamera);

        material.SetVector("_PyramidApex", pyramidApexInCamera);

        // Set the normals to the pyramid sides


        Vector3[] pyramidNormals = new Vector3[4];

        pyramidNormals = ComputeNormals(mPyramid.mPyramidParam.Height, mPyramid.mPyramidParam.Width,
                                          mPyramid.mPyramidParam.Depth);

        for (int i = 0; i < 4; i++)
        {
            material.SetVector("_PyramidNormals" + i.ToString(), 
                   mMainCamera.worldToCameraMatrix * pyramidLocalToWorldMatrix * pyramidNormals[i]);
                 
            
        }
    } //  void Start()

                         

// Update is called once per frame
    void Update ()
     { }

}  // class Pyramid
