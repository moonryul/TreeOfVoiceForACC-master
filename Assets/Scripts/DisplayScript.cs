using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

// Attached to Main Camera
public class DisplayScript: MonoBehaviour
{
    [SerializeField] protected bool IsCameraUpdated = false; 
    [SerializeField] protected Vector3 GroundMinCorner; 
    [SerializeField] protected Vector3 GroundMaxCorner;

    [SerializeField] protected Vector3 CeilingMinCorner;
    [SerializeField] protected Vector3 CeilingMaxCorner;

    //Display.Length will always be 1 in the Editor. We dont have an implementation of display detection 
    //in editor as it works different. We are using Editor windows, not actual displays.
    //You can still use multiple display in the editor you just dont need to activate the displays
    //like you do in a release.

    //https://stackoverflow.com/questions/43066541/unity-multiple-displays-not-working

    //Layers can be used for selective rendering from cameras or ignoring raycasts. 
    //Unity generates 32 layers. Layers from 8 and above are unused. They can be used for specific game reasons. Layers are named and used during the development of the game. Layers are added and viewed by clicking the top-right editor Layout button.

    void ResetCamera()
    {


        // Finding multiple cameras: https://answers.unity.com/questions/15801/finding-cameras.html
        //(1) In your code use the following sysntax:
        //Camera myCamera = GameObject.FindWithTag("myCamera").GetComponent <> Camera > ();
        //
        //(2): 1) Simply drag a reference of the desired camera to a variable of type Camera in your script.

        //     2) use Camera.main if the camera you want is the only active one right now.

        //3) If you have multiple cameras named uniquely, 
        //check foreach (Camera c in Camera.allCameras) and  c.gameObject.name == "DesiredCamera" 
        //then that is the camera you want.
        //.ner, far, fieldOfView
        // //Start the Camera field of view at 60
        //m_FieldOfView = 60.0f; This is the vertical field of view; 
        // .aspect = width / height
        //.name


        // Script inherits from MonoBehavior => Behavior => Component => object
        // Camera component inherits from Behavior => Component => object
        // Behaviours are Components that can be enabled or disabled.
        // For example, Rigidbody cannot be enabled/disabled. 
        //This is why it inherits from the Component class instead of Behaviour.
        // MonoBehaviour is the base class from which every Unity script derives.
        //MonoBehaviour:
        //The most important thing to note about MonoBehaviour is that you need it when you have to use corutines, 
        //Invoking, or any Unity callback functions such as physics OnCollisionEnter function, Start, OnEnable, OnDisable, etc.
        //MonoBehaviour inherits from Behaviour so that your scripts can be enabled / disabled.
        //Note that Behaviour and Component are used by Unity for internal stuff. 
        // You should not try to inherit your script from these.


        //   Debug.Log("display number = " + Display.displays.Length);

        //public Vector3 GroundMinCorner = new Vector3(-10f, 0f, -10f);
        //public Vector3 GroundMaxCorner = new Vector3(10f, 0f, 10f);

        //public Vector3 CeilingMinCorner = new Vector3(-10f, 12f, -10f);
        //public Vector3 CeilingMaxCorner = new Vector3(10f, 12f, 10f);


        Vector3 GroundCenter = (GroundMinCorner + GroundMaxCorner) / 2f;
        Vector3 CeilingCenter = (CeilingMinCorner + CeilingMaxCorner) / 2f;
        //   for (int i = 0; i < Display.displays.Length; i++)
        // scan the active cameras:
        foreach (Camera c in Camera.allCameras)
        {
            Debug.Log("gameObj name=" + c.gameObject.name);
            Debug.Log("camera name=" + c.name);

            Debug.Log("vertical field of view ="); Debug.Log(c.fieldOfView);
            Debug.Log("aspect (=w/h) = "); Debug.Log(c.aspect);

           // bool check = c.name == "MainCamera";

            //Camera c = myCams[i];

            // if (c.name == "MainCamera") return;


            //public Vector3 GroundMinCorner = new Vector3(-10f, 0f, -10f);
            //public Vector3 GroundMaxCorner = new Vector3(10f, 0f, 10f);

            //public Vector3 CeilingMinCorner = new Vector3(-10f, 12f, -10f);
            //public Vector3 CeilingMaxCorner = new Vector3(10f, 12f, 10f);


            if (c.name == "CameraToGround")
            {
                Debug.Log(c.name);

                Debug.Log("CameraToGround: camera pos=");
                Debug.Log(c.transform.position);

                // targetPos on the ground

                Vector3 targetPos = GroundCenter; 
                Debug.Log("camera targetPos=");
                Debug.Log(targetPos);


                Vector3 vecToTarget = targetPos - c.transform.position;

                Debug.Log("the vector to the target pos");
                Debug.Log(vecToTarget);

                float heightOfFOV = (targetPos.x - GroundMinCorner.x);

                float fieldOfView = 2.0f * Mathf.Rad2Deg *
                                 Mathf.Atan(heightOfFOV / vecToTarget.magnitude);
                Debug.Log("computed field of view:");
                Debug.Log(fieldOfView);

               
                float aspect = Mathf.Abs(2 * GroundMinCorner.z) / (2f * heightOfFOV);
                Debug.Log("computed aspect (width/height):");
                Debug.Log(aspect);

                //Debug.Log("computed aspect2 (w/h): = equal to the first?");
                //float  aspect = Mathf.Abs(GroundMinCorner.x) / (2 * Mathf.Abs(GroundMinCorner.z) );
                //Debug.Log(aspect);


                c.fieldOfView = fieldOfView;
                c.aspect = aspect;

            }




            if (c.name == "CameraToGroundLeft")
            {
                Debug.Log(c.name);

                Debug.Log("CameraToGroundLeft: camera pos=");
                Debug.Log(c.transform.position);

                // targetPos on the ground

                Vector3 targetPos = new Vector3( (GroundMinCorner.x + GroundCenter.x)/ 2f, GroundMinCorner.y, (GroundMinCorner.z + GroundMaxCorner.z)/2f ); 
                Debug.Log("camera targetPos=");
                Debug.Log(targetPos);


                Debug.Log("the vector to the target:");  
                Vector3 vecToTarget = targetPos - c.transform.position;
                Debug.Log(vecToTarget);

                float heightOfFOV = (targetPos.x - GroundMinCorner.x);   

                float fieldOfView = 2.0f * Mathf.Rad2Deg *
                                 Mathf.Atan( heightOfFOV  / vecToTarget.magnitude);

                Debug.Log("computed field of view:");
                Debug.Log(fieldOfView);

                Debug.Log("computed aspect (width/height):");
                float aspect = Mathf.Abs(2 * GroundMinCorner.z) / (2f * heightOfFOV);
                Debug.Log(aspect);

                //Debug.Log("computed aspect2 (w/h): = equal to the first?");
                //float  aspect = Mathf.Abs(GroundMinCorner.x) / (2 * Mathf.Abs(GroundMinCorner.z) );
                //Debug.Log(aspect);


                c.fieldOfView = fieldOfView;
                c.aspect = aspect;

            }


            //public Vector3 GroundMinCorner = new Vector3(-10f, 0f, -10f);
            //public Vector3 GroundMaxCorner = new Vector3(10f, 0f, 10f);

            //public Vector3 CeilingMinCorner = new Vector3(-10f, 12f, -10f);
            //public Vector3 CeilingMaxCorner = new Vector3(10f, 12f, 10f);

            if (c.name == "CameraToGroundRight")
            {
                Debug.Log(c.name);

                Debug.Log("CameraToGroundRight: camera pos=");
                Debug.Log(c.transform.position);

                Vector3 targetPos = new Vector3((GroundMaxCorner.x + GroundCenter.x) / 2f, GroundMaxCorner.y, (GroundMinCorner.z + GroundMaxCorner.z) / 2f);
               // Vector3 targetPos =  new Vector3(GroundMaxCorner.x/2f, GroundMaxCorner.y, 0f) ;
                Debug.Log("camera targetPos=");
                Debug.Log(targetPos);


                Debug.Log("computed  field of view:");
                Vector3 vecToTarget = targetPos - c.transform.position;

                // The height ( on the y axis of the camera) of the camera field of view
                float heightOfFOV = Mathf.Abs( targetPos.x - GroundMaxCorner.x );
                //float heightOnAxis = (targetPos - new Vector3(0f, 0f, GroundMinCorner.z)).magnitude;
                
                float fieldOfView = 2.0f * Mathf.Rad2Deg *
                                 Mathf.Atan(heightOfFOV / vecToTarget.magnitude);
                Debug.Log(fieldOfView);

                Debug.Log("computed aspect (width/height):");
                float aspect = Mathf.Abs( 2* GroundMaxCorner.z) / (2f * heightOfFOV);
                Debug.Log(aspect);


                //Debug.Log("computed aspect2 (w/h):");
                //float aspect = Mathf.Abs(GroundMaxCorner.x) / (2 * Mathf.Abs(GroundMaxCorner.z) );
                //Debug.Log(aspect);


                c.fieldOfView = fieldOfView;
                c.aspect = aspect;

            }

            //public Vector3 GroundMinCorner = new Vector3(-10f, 0f, -10f);
            //public Vector3 GroundMaxCorner = new Vector3(10f, 0f, 10f);

            //public Vector3 CeilingMinCorner = new Vector3(-10f, 12f, -10f);
            //public Vector3 CeilingMaxCorner = new Vector3(10f, 12f, 10f);

            if (c.name == "CameraToCeiling")
            //CameraToCeilingLeft
            {

                Debug.Log("CameraToCeiling: camera pos=");
                Debug.Log(c.transform.position);

                Vector3 targetPos = CeilingCenter;
                // Vector3 targetPos = new Vector3(CeilingMinCorner.x / 2f, 0f, 0f);

                Debug.Log("camera targetPos=");
                Debug.Log(targetPos);


                Debug.Log("the vector to the target:");

                Vector3 vecToTarget = targetPos - c.transform.position;
                Debug.Log(vecToTarget);

                float heightOfFOV = (targetPos.x - CeilingMinCorner.x);
                //float heightOnAxis = (targetPos - new Vector3(0f, 0f, CeilingMinCorner.z)).magnitude;

                float fieldOfView = 2.0f * Mathf.Rad2Deg *
                                 Mathf.Atan(heightOfFOV / vecToTarget.magnitude);

                Debug.Log("computed field of view:");
                Debug.Log(fieldOfView);

                Debug.Log("computed aspect  (width/height):");
                float aspect = Mathf.Abs(2 * CeilingMinCorner.z) / (2f * heightOfFOV);
                Debug.Log(aspect);


                //Debug.Log("computed aspect (width/height): ");
                //float aspect = Mathf.Abs( CeilingMinCorner.x) / (2 * Mathf.Abs(CeilingMinCorner.z));
                //Debug.Log(aspect);


                c.fieldOfView = fieldOfView;
                c.aspect = aspect;
            }


            if (c.name == "CameraToCeilingLeft")
            //CameraToCeilingLeft
            {

                Debug.Log("CameraToCeilingLeft: camera pos=");
                Debug.Log(c.transform.position);

                Vector3 targetPos = new Vector3((CeilingMinCorner.x + CeilingCenter.x) / 2f, CeilingMinCorner.y, (CeilingMinCorner.z + CeilingMaxCorner.z) / 2f);
                // Vector3 targetPos = new Vector3(CeilingMinCorner.x / 2f, 0f, 0f);

                Debug.Log("camera targetPos=");
                Debug.Log(targetPos);


                Debug.Log("computed field of vie:");

                Vector3 vecToTarget = targetPos - c.transform.position;

                float heightOfFOV = (targetPos.x - CeilingMinCorner.x);
                //float heightOnAxis = (targetPos - new Vector3(0f, 0f, CeilingMinCorner.z)).magnitude;

                float fieldOfView = 2.0f * Mathf.Rad2Deg *
                                 Mathf.Atan(heightOfFOV / vecToTarget.magnitude);
                Debug.Log(fieldOfView);

                Debug.Log("computed aspect  (width/height):");
                float aspect = Mathf.Abs( 2* CeilingMinCorner.z) / (2f * heightOfFOV);
                Debug.Log(aspect);


                //Debug.Log("computed aspect (width/height): ");
                //float aspect = Mathf.Abs( CeilingMinCorner.x) / (2 * Mathf.Abs(CeilingMinCorner.z));
                //Debug.Log(aspect);


                c.fieldOfView = fieldOfView;
                c.aspect = aspect;
            }

            //public Vector3 GroundMinCorner = new Vector3(-10f, 0f, -10f);
            //public Vector3 GroundMaxCorner = new Vector3(10f, 0f, 10f);

            //public Vector3 CeilingMinCorner = new Vector3(-10f, 12f, -10f);
            //public Vector3 CeilingMaxCorner = new Vector3(10f, 12f, 10f);

            if (c.name == "CameraToCeilingRight")
            {

                Debug.Log("CameraToCeilingRight: camera pos=");
                Debug.Log(c.transform.position);

                Vector3 targetPos = new Vector3((CeilingMaxCorner.x + CeilingCenter.x) / 2f, CeilingMaxCorner.y, (CeilingMinCorner.z + CeilingMaxCorner.z) / 2f);
                //Vector3 targetPos = new Vector3 ( CeilingMaxCorner.x/2f, 0f, 0f);
                Debug.Log("camera targetPos=");
                Debug.Log(targetPos);


                Debug.Log("computed field of vie:");

                Vector3 vecToTarget = targetPos - c.transform.position;

                float heightOfFOV = Mathf.Abs(targetPos.x - CeilingMaxCorner.x);
              //  float heightOnAxis = (targetPos - new Vector3(0f, 0f, CeilingMaxCorner.z)).magnitude;

                float fieldOfView = 2.0f * Mathf.Rad2Deg *
                                 Mathf.Atan(heightOfFOV / vecToTarget.magnitude);
                Debug.Log(fieldOfView);

                Debug.Log("computed aspect (width/height):");
                float aspect = Mathf.Abs(2 * CeilingMaxCorner.z) / (2f * heightOfFOV);
                Debug.Log(aspect);


                //Debug.Log("computed aspect2 (w/h) ");
                //float aspect = Mathf.Abs(CeilingMaxCorner.x) / (2 * Mathf.Abs(CeilingMaxCorner.z));



                c.fieldOfView = fieldOfView;
                c.aspect = aspect;
            }


            if (c.name == "CameraToFrontWall"  || c.name == "Main Camera" || c.name == "EventCamera")
            {

                Debug.Log( c.name + ": camera pos=");
                Debug.Log(c.transform.position);

                Vector3 targetPos = new Vector3(0.0f, (CeilingMinCorner.y + GroundMinCorner.y) / 2.0f, CeilingMinCorner.z);

                Debug.Log("camera targetPos=");
                Debug.Log(targetPos);


                Debug.Log("computed target vector:");

                Vector3 vecToTarget = targetPos - c.transform.position;

                Debug.Log(vecToTarget);
                float heightOfFOV =  1.2f * Mathf.Abs(targetPos.y - CeilingMaxCorner.y);
                //  float heightOnAxis = (targetPos - new Vector3(0f, 0f, CeilingMaxCorner.z)).magnitude;

                float fieldOfView = 2.0f * Mathf.Rad2Deg *
                                 Mathf.Atan(heightOfFOV / vecToTarget.magnitude);


                Debug.Log("computed field of view:");
                Debug.Log(fieldOfView);

                Debug.Log("computed aspect (width/height):");

                float aspect = Mathf.Abs(2 * CeilingMaxCorner.x) / (2f * heightOfFOV);


                Debug.Log(aspect);



                c.fieldOfView = fieldOfView;
                c.aspect = aspect;

            }

               

            //if (c.name == "CameraToLeftWall")
            //{
            //    //Debug.Log("camera pos=");
            //    //Debug.Log(c.transform.position);


            //    Vector3 targetPos = new Vector3(GroundMinCorner.x, (CeilingMaxCorner.y + GroundMinCorner.y) / 2.0f, 0.0f);

            //    //Debug.Log("camera targetPos=");
            //    //Debug.Log(targetPos);


            //    //Debug.Log("computed field of view:");

            //    Vector3 vecToTarget = targetPos - c.transform.position;

            //    float fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan((CeilingMaxCorner.y - GroundMinCorner.y) / 2.0f / vecToTarget.magnitude);
            //  //  Debug.Log(fieldOfView);

            //   // Debug.Log("computed aspect:");
            //    float aspect = (GroundMaxCorner.z - GroundMinCorner.z) / (CeilingMaxCorner.y - GroundMinCorner.y);
            //   // Debug.Log(aspect);

            //    c.fieldOfView = fieldOfView;
            //    c.aspect = aspect;
            //}


            //if (c.name == "CameraToRightWall")
            //{

            //    //Debug.Log("camera pos=");
            //    //Debug.Log(c.transform.position);


            //    Vector3 targetPos = new Vector3(GroundMaxCorner.x, (CeilingMaxCorner.y + GroundMinCorner.y) / 2.0f, 0.0f);

            //    //Debug.Log("camera targetPos=");
            //    //Debug.Log(targetPos);

            //    //Debug.Log("computed field of vie:");

            //    Vector3 vecToTarget = targetPos - c.transform.position;

            //    float fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan((CeilingMaxCorner.y - GroundMinCorner.y) / 2.0f / vecToTarget.magnitude);
            //    Debug.Log(fieldOfView);

            //    Debug.Log("computed  aspect:");
            //    float aspect = (GroundMaxCorner.z - GroundMinCorner.z) / (CeilingMaxCorner.y - GroundMinCorner.y);
            //    Debug.Log(aspect);

            //    c.fieldOfView = fieldOfView;
            //    c.aspect = aspect;
            //}


        } // for all cameras


    } // OnValidate()


    Camera[] myCams = new Camera[6];


    void mapCameraToDisplay()
    {
        //Loop over Connected Displays
        Debug.Log("################################");

        for (int i = 0; i < Display.displays.Length; i++)
        {

            Debug.Log("Display" + (i + 1) + "Resolution=");
            Debug.Log(Display.displays[i].systemWidth + "x" + Display.displays[i].systemHeight);

            Debug.Log("Screen" + (i + 1) + "Resolution=");
            Debug.Log(Screen.width + "x" + Screen.height);

            myCams[i].targetDisplay = i; //Set the Display in which to render the camera to

            //https://gamedev.stackexchange.com/questions/142353/setting-resolution-of-multiple-display-screens
            //  to make a secondary display go fullscreen, you just need to set its resolution to the native one:
            
            Display.displays[i].Activate(); //Enable the display
            Display.displays[i].SetRenderingResolution(Display.displays[i].systemWidth,
                                           Display.displays[i].systemHeight);
        }

        Debug.Log("################################");
    }

    void OnDisplaysUpdated()
    {
        Debug.Log("Display Sizes  Are Changed");
        //Loop over Connected Displays
        for (int i = 0; i < Display.displays.Length; i++)
        {

            Debug.Log("Display" + (i+1) + "Resolution="); 
            Debug.Log(Display.displays[i].systemWidth + "x" + Display.displays[i].systemHeight);



           // myCams[i].targetDisplay = i; //Set the Display in which to render the camera to

            //https://gamedev.stackexchange.com/questions/142353/setting-resolution-of-multiple-display-screens
            //  to make a secondary display go fullscreen, you just need to set its resolution to the native one:

           // Display.displays[i].Activate(); //Enable the display
            Display.displays[i].SetRenderingResolution(Display.displays[i].systemWidth,
                                           Display.displays[i].systemHeight);
        }

    }


    private void Awake()
    {
        //Get Main Camera
        myCams[0] = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        //Find All other Cameras
        //myCams[1] = GameObject.Find("CameraToGroundLeft").GetComponent<Camera>();
        //myCams[2] = GameObject.Find("CameraToGroundRight").GetComponent<Camera>();
        //myCams[3] = GameObject.Find("CameraToCeilingLeft").GetComponent<Camera>();
        //myCams[4] = GameObject.Find("CameraToCeilingRight").GetComponent<Camera>();
        //myCams[5] = GameObject.Find("CameraToFrontWall").GetComponent<Camera>();

        //Find All other Cameras
        myCams[1] = GameObject.Find("CameraToGround").GetComponent<Camera>();
        myCams[2] = GameObject.Find("CameraToCeiling").GetComponent<Camera>();
        myCams[3] = GameObject.Find("CameraToFrontWall").GetComponent<Camera>();

   // https://forum.unity.com/threads/using-physics-raycast-to-interact-with-canvas-in-worldspace-vr-and-other-environments.563848/
        //Call function when new display is connected
        Display.onDisplaysUpdated += OnDisplaysUpdated;

        //Map each Camera to a Display
        mapCameraToDisplay();
        //Screen.SetResolution( 2 * 1920, 1080, false); // false = no full screen
        //if (Display.displays.Length > 1)
        //{
        //   // Display.displays[0].SetRenderingResolution(1920, 1080);
        //    Display.displays[0].SetParams( 1920, 1080, 0,0);
        //    Display.displays[1].Activate();
        //    //Display.displays[1].SetRenderingResolution(1920, 1080);
        //    Display.displays[1].SetParams(1920, 1080, 0, 1080);
        //}
    }


    // Use this for initialization
    private void Start () 
	{

        // GroundMaxCorner = GetComponent<SimpleBoids>().GroundMaxCorner; 
        // This Returns the component of Type type if the game object has one attached, null if it doesn't.
        // Use gameObject.GetComponent<SimpleBoids>().GroundMaxCorner for clarity

        //BoidsNum = GetComponent<SimpleBoids>().BoidsNum;
        // SimpleBoids component object is attached to the "CameraToGroundLeft" gameobject to which this "DisplayScript" 
        // component is attached. 
        GroundMaxCorner = gameObject.GetComponent<SimpleBoids>().GroundMaxCorner; // SimpleBoids class is defined with the same project (same namespace)
                                                                       // accessible from here
        GroundMinCorner = gameObject.GetComponent<SimpleBoids>().GroundMinCorner;

        CeilingMaxCorner = gameObject.GetComponent<SimpleBoids>().CeilingMaxCorner;
        CeilingMinCorner = gameObject.GetComponent<SimpleBoids>().CeilingMinCorner;

        ResetCamera();



    }


}
