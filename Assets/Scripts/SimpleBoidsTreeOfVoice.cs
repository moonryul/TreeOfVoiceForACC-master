using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Reflection;

using System.IO;

using Random = UnityEngine.Random;


public class SimpleBoidsTreeOfVoice : MonoBehaviour
{

    ActionPlanController m_actionPlanController; 

    const int BLOCK_SIZE = 1024; // The number of threads in a single thread group

    const int MAX_SIZE_OF_BUFFER = 10000;

    const float epsilon = 1e-2f;
    const float M_PI = 3.1415926535897932384626433832795f;

    float m_SceneStartTime; // = Time.time;
    float m_currTime;

    public float m_AnimationCycle = 390.0f; // 390 sec <=> Screen.width; 390sec/(Screen.Width *2)??

    public float m_SimulationDeltaT;

    public bool UseActionPlan = true;
   

    // 보이드의 수
    [Range(500, 10000)]
    public float m_BoidsNum = 1000f;


    
    [Range(0.0f, 3.0f)]
    [SerializeField] public float MinBoidRadius = 0.1f;
    [Range(0.0f, 3.0f)]
    [SerializeField] public float MaxBoidRadius = 0.5f;


    [Range(0.0f, 3.0f)]
    [SerializeField] private float _minSpeed = 0.5f;
    [Range(1.0f, 5.0f)]
    [SerializeField] private float _maxSpeed= 3.0f;



    [Range(0.0f, 3.0f)]
    [SerializeField] private float _speedFactor = 1.0f;

    [Range(0.0f, 3.0f)]
    [SerializeField] private float _scaleFactor = 1.0f;
       
   

    // the neighborhood conditions and weights for the three flocking actions
    // 분리   
    // boid radius is about 1.0m => The separation distance is about  two times as the radius
    [SerializeField] private BoidSetting _separate = new BoidSetting(2.0f, 0.5f);
    // 정렬: in the same direction
    [SerializeField] private BoidSetting _alignment = new BoidSetting(4f, 0.3f);
    // 응집: form a group: the range is greater than alignment
    [SerializeField] private BoidSetting _cohesion = new BoidSetting(6f, 0.2f);




    //[SerializeField] private float _groundFlockingWeight = 0.3f;
    //// 목표지점 추구
    //[SerializeField] private float _groundDivergeWeight = 0.2f;
    //// 중심회전
    //[SerializeField] private float _groundCirculationWeight = 0.5f;


    [SerializeField] private GroundWeight _groundWeight = new GroundWeight(0.3f, 0.2f, 0.5f);



    [SerializeField] private CeilingWeight _ceilingWeight = new CeilingWeight(0.2f, 0.3f, 0.5f);




    [Range(-180.0f, 0.0f)]
    [SerializeField] private float _groundMinHue = -180.0f;

    [Range(-180f, 0.0f)]
    [SerializeField] private float _groundMaxHue = 0.0f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _groundMinSaturation = 0.0f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _groundMaxSaturation = 0.5f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _groundMinValue = 0.0f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _groundMaxValue = 0.5f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _groundMinAlpha = 0.2f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _groundMaxAlpha = 1.0f;



    [Range(0f, 180f)]
    [SerializeField] private float _ceilingMinHue = 0.0f;

    [Range(0f, 180f)]
    [SerializeField] private float _ceilingMaxHue = 180f;


    [Range(0.0f, 1.0f)]
    [SerializeField] private float _ceilingMinSaturation = 0.5f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _ceilingMaxSaturation = 1.0f;


    [Range(0.0f, 1.0f)]
    [SerializeField] private float _ceilingMinValue = 0.5f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _ceilingMaxValue = 1.0f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _ceilingMinAlpha = 0.2f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _ceilingMaxAlpha = 1.0f;
          
    // 벽의 크기
    public Vector3 GroundMinCorner = new Vector3(-10f, -10f, -10f);
    public Vector3 GroundMaxCorner = new Vector3(10f, -10f, 10f);

    public Vector3 CeilingMinCorner = new Vector3(-10f, 10f, -10f);
    public Vector3 CeilingMaxCorner = new Vector3(10f, 10f, 10f);

    // 시뮬레이션 공간의 센터 
    public Vector3 RoomCenter
    {
        get { return (GroundMinCorner + CeilingMaxCorner) / 2f; }
    }

    public Vector3 RoomSize
    {
        get { return CeilingMaxCorner - GroundMinCorner; }
    }


    public float GroundRadius = 10f;
    public float CeilingRadius = 10f;
    // public float CeilingInnerRadius = 0.7f;

    public float CeilingInnerRadius = 0.7f;
    
    public int numOfWalls = 2; // ground, ceiling, front wall

    public int m_numOfWallGizmos = 3; // ground, ceiling, front wall
    static private bool m_wallTransformsDefined = false;

    public float m_MinDomainRadius = 0.7f; // the minumum radius of of the xz domain; used LED boid rendering
    public float m_MaxDomainRadius = 10; // // the maximum radius of of the xz domain; used in LED boid rendering


    // 컴퓨트 쉐이더
    // Mention another Component instance.
    [SerializeField] protected ComputeShader m_BoidComputeShader; // set in the inspector

    //https://www.reddit.com/r/Unity3D/comments/7ppldz/physics_simulation_on_gpu_with_compute_shader_in/
    // 보이드의 버퍼
    public ComputeBuffer m_BoidBuffer { get; protected set; } // null reference


    int m_BufferStartIndex, m_BufferEndIndex;

    protected int m_KernelIdGround;
    
    public BoidData[] m_boidArray;

    int totalNumOfSimulations = 0;
    bool m_IsGizmoDrawn = false;
    StreamWriter m_writer;
    FileStream m_oStream;


    //When you create a struct object using the new operator, it gets created and the appropriate constructor is called.
    //Unlike classes, structs can be instantiated without using the new operator. 
    //If you do not use new, the fields will remain unassigned and the object cannot be used until all of the fields are initialized.



    // Transforms that represent the 5 walls

    //Transform groundTransform = new Transform();
    //The Above Causes "Inaccessible  Errors":  you shouldn't be calling the constructor for Transform or any other Component. 
    //Transform is a component. You can't create components like that, 
    //since they don't exist by themselves; you can create a GameObject, and add components to it.


    public GameObject[] gameObjForWallTransforms;

    // In the above, Your code creates only the array of type GameOjbect, but neither of its items.
    // Basically, you need to store instances of Sample into this array.
    //
    // To put it simple, without any fancy LINQ etc.:
    // Sample[] samples = new Sample[100];
    // for (int i = 0; i<samples.Length; i++) samples[i] = new Sample();

  

    private bool IsBoidsNumSet = false;

    //public class Action
    //{

    //    public List<float> T;
    //    public float V;


    //}

    List<string> m_actionKeys;
    int m_threadGroupSize;

    //public void Insert(int index, T item);
    //public int LastIndexOf(T item);
    //public int LastIndexOf(T item, int index);
    //public int LastIndexOf(T item, int index, int count);
    //public bool Remove(T item);
    // public void RemoveAt(int index);

    public Dictionary<String, List<ActionPlanController.Action>> m_actionPlan;

    public struct BoidData
    {
        // public Vector3  WallOrigin; // the reference position of the wall (the boid reference frame) on which the boid is 

        //public Vector3 EulerAngles; // the rotation of the boid reference frame
        public Matrix4x4 BoidFrame; // Affine Frame of the Boid, which also includes Position data
        public Vector3 Position; // the position of the  boid in the boid reference frame        

        public Vector3 Scale; // the scale factors
        public Vector3 HeadDir; // heading direction of the boid on the local plane
        public float Speed;            // the speed of a boid

        public float Radius; // the radius of the circle boid
        public Vector3 ColorHSV;
        public Vector4 Color;         // RGBA color
        public Vector2 SoundGrain; // soundGrain = (freq, amp)
        public float Duration;     // duration of a boid each frame
        public int WallNo;      // the number of the wall on which the boid lie. 0=> the ground
                                // 1 => the ceiling, 2 => left wall, 3=> right wall. 4=> front wall
    }


   ;

//class Test
//{
//    public static void Main()
//    {
//        string path = @"c:\temp\MyTest.txt";
//        if (!File.Exists(path))
//        {
//            // Create a file to write to.
//            using (StreamWriter sw = File.CreateText(path))
//            {
//                sw.WriteLine("Hello");
//                sw.WriteLine("And");
//                sw.WriteLine("Welcome");
//            }
//        }

//        // Open the file to read from.
//        using (StreamReader sr = File.OpenText(path))
//        {
//            string s = "";
//            while ((s = sr.ReadLine()) != null)
//            {
//                Console.WriteLine(s);
//            }
//        }
//    }
//}

private void Awake()
    {

        // DEBUG code
        string fileName = "SimpleBoidSimul";
        string fileIndex = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        fileIndex.Replace(" ", string.Empty);
        //fileIndex = string.Join("", 
        //       fileIndex.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        // String[] Split(String[] separator, StringSplitOptions options);
        //Because string is a reference type and the default value for all reference types is null. 

        //https://answers.unity.com/questions/990496/ioexception-sharing-violation-on-path-please-help.html?_ga=2.11865712.591554826.1574537830-1174358732.1569135042
       
        //File.CreateText(pathToLoad)).Dispose();
        //using (TextWriter writer = new StreamWriter(pathToLoad, false))
        //{
        //    writer.WriteLine("00:00:00,00/00/0000,1,1,500,20000,1500,50,20,10,5,2,1,1,0,10,50,50");
        //    writer.Close();
        //}

        string path = "Assets/Resources/DebugFiles/" + fileName +  fileIndex + ".txt";

        File.CreateText(path).Dispose();

        ////Write some text to the test.txt file
        m_writer = new StreamWriter(path, false); // do not append
        //m_oStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
        ////m_ioStream = new FileStream(path,
        ////                               FileMode.OpenOrCreate,
        ////                               FileAccess.ReadWrite,
        ////                               FileShare.None);
        //m_writer = new System.IO.StreamWriter(m_oStream);

        //// 벽의 크기
        //GroundMinCorner = new Vector3(-10f, -10f, -10f);
        //GroundMaxCorner = new Vector3(10f, -10f, 10f);

        //CeilingMinCorner = new Vector3(-10f, 10f, -10f);
        //CeilingMaxCorner = new Vector3(10f, 10f, 10f);

        // 시뮬레이션 공간의 센터 
        //RoomCenter = (GroundMinCorner + CeilingMaxCorner) / 2f;
        //RoomSize = CeilingMaxCorner - GroundMinCorner;




        m_MinDomainRadius = CeilingInnerRadius; // the minumum radius of of the xz domain; used LED boid rendering
        m_MaxDomainRadius = GroundRadius;

        m_actionPlanController = this.gameObject.GetComponent<ActionPlanController>();

        if (m_actionPlanController is null)
        {
            Debug.Log("ActionPlanController component should be added to CommHub");
        }

        m_threadGroupSize = Mathf.CeilToInt(m_BoidsNum / (float)BLOCK_SIZE);


        Debug.Log("################################");
        Debug.Log("I am in Awake() in SimpleBoids initializing for the simulation of boids");
        Debug.Log("################################");



        InitializeValues();
        InitializeBuffers();


    }//  private void Awake()



    private void Start()
    {
        //"initialize my connections to others, which have been initialized by their own Awake()

        m_actionPlan = m_actionPlanController.m_actionPlan;

        m_SceneStartTime = Time.time; // set the current time in millisecond
        m_currTime = m_SceneStartTime;

        Debug.Log("Start Time=" + m_SceneStartTime);


    }


    // Update is called once per frame
    private void Update()
    {

        //Debug.Log("Start Time=" + m_SceneStartTime);

        //Debug.Log("currTime = " + Time.time);
        //Debug.Log("delta Time  = " + Time.deltaTime);

        // get the current time
        m_currTime = Time.time; //  seconds

        //Time.time simply gives you a numeric value which is equal to the number of seconds
        //which have elapsed since the project started playing.
        // Time.time (and Time.deltaTime) only change their value once per frame.


        //Simulate(); // for debugging
    }


    private void OnDrawGizmos()
    {
       

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(RoomCenter, RoomSize);

        Vector3 wallOrigin, initEulerAngles;
        //const int maxNumOfWalls = 5;

        if (!m_wallTransformsDefined)  // if true, skip the creation of walls
        {
            m_wallTransformsDefined = true;


            gameObjForWallTransforms = new GameObject[m_numOfWallGizmos];

            GameObject gameObj = GameObject.Find("GroundWall");
            if (gameObj == null)
            {

                gameObj = new GameObject("GroundWall");
            }

            wallOrigin = new Vector3(0.0f, GroundMinCorner.y, 0.0f);
            //   initEulerAngles = new Vector3(0.0f, 0.0f, 0.0f); // the rotation frame of the boid = the unity global frame 
            gameObj.transform.position = wallOrigin;
            ////
            // Summary:
            //     Returns the euler angle representation of the rotation.
            //  public Vector3 eulerAngles { get; set; }
            //    gameObjForWallTransforms[0].transform.rotation.eulerAngles= initEulerAngles;
            // OR transform.rotation = Quaternion.AngleAxis(xRot, transform.right);

            // OR https://forum.unity.com/threads/rotation-order.13469/
            // For example, in whichever order you like....
            //         rotation = Quaternion.AngleAxis(xAngle, Vector3.right) * Quaternion.AngleAxis(zAngle, Vector3.forward) *
            //                                   Quaternion.AngleAxis(yAngle, Vector3.up);

            //gameObjForWallTransforms[0].transform.eulerAngles = Mathf.Rad2Deg * initEulerAngles;

            //gameObjForWallTransforms[0].transform.rotation = Quaternion.identity;
            //Mathf.Rad2Deg * initEulerAngles;
            gameObj.transform.rotation = Quaternion.AngleAxis(0.5f * M_PI * Mathf.Rad2Deg, Vector3.right); // Z point upward
            gameObj.transform.localScale = Vector3.one;

            gameObjForWallTransforms[0] = gameObj;



            // wall 1
            gameObj = GameObject.Find("CeilingWall");
            if (gameObj == null)
            {
                gameObj = new GameObject("CeilingWall");
            }


            wallOrigin = new Vector3(0.0f, CeilingMaxCorner.y, 0.0f);

            //https://gamedev.stackexchange.com/questions/140579/euler-right-handed-to-quaternion-left-handed-conversion-in-unity/140581
            //initEulerAngles = new Vector3(M_PI, 0.0f, 0.0f); //  pitch  = 180: the x to the right, the z to the back, the y down

            gameObj.transform.position = wallOrigin;
            // gameObjForWallTransforms[1].transform.eulerAngles = Mathf.Rad2Deg * initEulerAngles;
            gameObj.transform.rotation = Quaternion.AngleAxis(-0.5f * M_PI * Mathf.Rad2Deg, Vector3.right); // Z point upward

            gameObj.transform.localScale = Vector3.one;

            gameObjForWallTransforms[1] = gameObj;


            // wall 2

            gameObj = GameObject.Find("FrontWall");
            if (gameObj == null)
            {
                gameObj = new GameObject("FrontWall");
            }

            wallOrigin = new Vector3(0.0f, 0.0f, GroundMinCorner.z); // front wall center

            // Euler Angles: R = Ry * Rx * Rz 
            //https://forum.unity.com/threads/roll-pitch-and-yaw-from-quaternion.63498/
            //  Unity uses the order Z-X-Y, which is fairly sensible given Unity's conventions. => Z = Roll, X = pitch, Y=Yaw
            //initEulerAngles = new Vector3(0.0f,-M_PI / 2f, -M_PI / 2f); // Rool=-90 => Pitch =0 => Yaw =-90

            gameObj.transform.position = wallOrigin;
            //gameObjForWallTransforms[2].transform.eulerAngles =-M_PI / 2f * initEulerAngles;
            //gameObjForWallTransforms[2].transform.rotation = Quaternion.AngleAxis(-M_PI / 2f * Mathf.Rad2Deg, Vector3.forward)
            //                                               * Quaternion.AngleAxis(-M_PI / 2f * Mathf.Rad2Deg, Vector3.up);

            gameObj.transform.rotation = Quaternion.identity;
            gameObj.transform.localScale = Vector3.one;
            gameObjForWallTransforms[2] = gameObj;


            // For debugging           

            for (int i = 0; i < m_numOfWallGizmos; i++)
            {
                Debug.Log("Wall Orientation = "); Debug.Log(i);

                Debug.Log("Euelr Angles="); Debug.Log(gameObjForWallTransforms[i].transform.eulerAngles);
                Debug.Log("Rotation ="); Debug.Log(gameObjForWallTransforms[i].transform.rotation);

                Debug.Log("right  ="); Debug.Log(gameObjForWallTransforms[i].transform.right);
                Debug.Log("forward ="); Debug.Log(gameObjForWallTransforms[i].transform.forward);
                Debug.Log("up  ="); Debug.Log(gameObjForWallTransforms[i].transform.up);
            }


        } // ! wallTransformsDefined


        // Walls are already created.
        //Draw the coordinate systems of each wall at the wallOrigin each frame.
        if (m_wallTransformsDefined)
        {
            for (int i = 0; i < m_numOfWallGizmos; i++)
            {

                // Debug.Log("Wall Gizmos  are drawn: " + i); ;

                Gizmos.color = Color.red;

                Gizmos.DrawRay(gameObjForWallTransforms[i].transform.position, gameObjForWallTransforms[i].transform.right);

                Gizmos.color = Color.green;

                Gizmos.DrawRay(gameObjForWallTransforms[i].transform.position, gameObjForWallTransforms[i].transform.up);

                Gizmos.color = Color.blue;

                Gizmos.DrawRay(gameObjForWallTransforms[i].transform.position, gameObjForWallTransforms[i].transform.forward);

            }
        } // if (m_wallTransformsDefined)

    } //  private void OnDrawGizmos()

  

    private void OnDestroy()
    {
        if (m_BoidBuffer == null) return;
        m_BoidBuffer.Release();
        m_BoidBuffer = null;
    }



    //public static Color HSVToRGB(float H, float S, float V);
    protected void InitializeValues()
    {
     
        IsBoidsNumSet = true;         

        m_BufferEndIndex = (int)m_BoidsNum;

        m_BoidComputeShader.SetFloat("_MinDomainRadius", m_MinDomainRadius);
        m_BoidComputeShader.SetFloat("_MaxDomainRadius", m_MaxDomainRadius);

        m_BoidComputeShader.SetInt("_BoidsNum", (int)m_BoidsNum);
       // m_BoidComputeShader.SetInt("_NumOfWalls", numOfWalls);


        //BoidComputeShader.SetFloat("_Mass", _mass);
                     
        m_BoidComputeShader.SetFloat("_SeparateRadius", _separate.Radius);
        m_BoidComputeShader.SetFloat("_SeparateWeight", _separate.Weight);
        m_BoidComputeShader.SetFloat("_AlignmentRadius", _alignment.Radius);
        m_BoidComputeShader.SetFloat("_AlignmentWeight", _alignment.Weight);
        m_BoidComputeShader.SetFloat("_CohesionRadius", _cohesion.Radius);
        m_BoidComputeShader.SetFloat("_CohesionWeight", _cohesion.Weight);


        m_BoidComputeShader.SetFloat("_MinSpeed", _minSpeed);
        m_BoidComputeShader.SetFloat("_MaxSpeed", _maxSpeed);

        m_BoidComputeShader.SetFloat("_SpeedFactor", _speedFactor);
        m_BoidComputeShader.SetFloat("_ScaleFactor", _scaleFactor);

        m_BoidComputeShader.SetFloat("_GroundFlockingWeight", _groundWeight.FlockingWeight);
        m_BoidComputeShader.SetFloat("_GroundDivergeWeight", _groundWeight.DivergeWeight);
        m_BoidComputeShader.SetFloat("_GroundCirculationWeight", _groundWeight.CirculationWeight);

        m_BoidComputeShader.SetFloat("_CeilingFlockingWeight", _ceilingWeight.FlockingWeight);
        m_BoidComputeShader.SetFloat("_CeilingConvergeWeight", _ceilingWeight.ConvergeWeight);
        m_BoidComputeShader.SetFloat("_CeilingCirculationWeight", _ceilingWeight.CirculationWeight);



        m_BoidComputeShader.SetFloat("_GroundMinHue", _groundMinHue);
        m_BoidComputeShader.SetFloat("_GroundMaxHue", _groundMaxHue);
        m_BoidComputeShader.SetFloat("_GroundMinSaturation", _groundMinSaturation);
        m_BoidComputeShader.SetFloat("_GroundMaxSaturation", _groundMaxSaturation);
        m_BoidComputeShader.SetFloat("_GroundMinValue", _groundMinValue);
        m_BoidComputeShader.SetFloat("_GroundMaxValue", _groundMaxValue);

        m_BoidComputeShader.SetFloat("_GroundMinAlpha", _groundMinAlpha);
        m_BoidComputeShader.SetFloat("_GroundMaxAlpha", _groundMaxAlpha);



        m_BoidComputeShader.SetFloat("_CeilingMinHue", _ceilingMinHue);
        m_BoidComputeShader.SetFloat("_CeilingMaxHue", _ceilingMaxHue);
        m_BoidComputeShader.SetFloat("_CeilingMinSaturation", _ceilingMinSaturation);
        m_BoidComputeShader.SetFloat("_CeilingMaxSaturation", _ceilingMaxSaturation);
        m_BoidComputeShader.SetFloat("_CeilingMinValue", _ceilingMinValue);
        m_BoidComputeShader.SetFloat("_CeilingMaxValue", _ceilingMaxValue);

        m_BoidComputeShader.SetFloat("_CeilingMinAlpha", _ceilingMinAlpha);
        m_BoidComputeShader.SetFloat("_CeilingMaxAlpha", _ceilingMaxAlpha);


        m_BoidComputeShader.SetVector("_GroundMaxCorner", GroundMaxCorner);
        m_BoidComputeShader.SetVector("_GroundMinCorner", GroundMinCorner);


        m_BoidComputeShader.SetVector("_CeilingMaxCorner", CeilingMaxCorner);
        m_BoidComputeShader.SetVector("_CeilingMinCorner", CeilingMinCorner);
        
        m_BoidComputeShader.SetFloat("_GroundRadius", GroundRadius);
        m_BoidComputeShader.SetFloat("_CeilinRadius", CeilingRadius);

        m_BoidComputeShader.SetFloat("_CeilingInnerRadius", CeilingInnerRadius);

        
        m_KernelIdGround = m_BoidComputeShader.FindKernel("SimulateBoids");
       
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
                      

        m_BoidBuffer = new ComputeBuffer(MAX_SIZE_OF_BUFFER, Marshal.SizeOf(typeof(BoidData)));

        m_boidArray = new BoidData[MAX_SIZE_OF_BUFFER];


        //var boidArray = new BoidData[BoidsNum];
        SetBoidArray(m_boidArray,(int)m_BoidsNum);

        //
        //For each kernel we are setting the buffers that are used by the kernel, so it would read and write to those buffers

        
        m_BoidBuffer.SetData(m_boidArray); // buffer is R or RW
       
        m_BoidComputeShader.SetBuffer(m_KernelIdGround, "_BoidBuffer", m_BoidBuffer);
     
       
    } // InitializeBuffers()




    protected void SetBoidArray(BoidData[] m_boidArray, int numberOfElements)
    {


        Vector3  initScale;
        float thetaPos, phiPos,  initSpeed, initRadiusX, initRadiusY, initRadiusZ;
            

        for (int i=0; i < numberOfElements; i++)
        {
            // set the head direction of the boid


            //On windows, the acceptable range of Sin is approximately between -9223372036854775295 to 9223372036854775295. 
            //This range may differ on other platforms. For values outside of the acceptable range, the Sin method returns the input value, rather than throwing an exception.
            // The hole on the wall: radius = 0.7 m, height = 12m; tan(theta) = 0.7/12

             float thetaOfHole = Mathf.Atan(0.7f / 12);

             thetaPos = Random.Range(thetaOfHole, M_PI);

            //    Debug.Log("atan of 07/12= " + thetaOfHole);


             phiPos = Random.Range(0, 2 * M_PI); // azimuth for the boid position

            // float radius = Random.Range(m_MinDomainRadius, m_MaxDomainRadius);
            float radius = m_MaxDomainRadius;
             m_boidArray[i].Position = new Vector3(radius * Mathf.Sin(thetaPos)* Mathf.Cos(phiPos), 
                                                      radius * Mathf.Cos(thetaPos), 
                                                      radius * Mathf.Sin(thetaPos) * Mathf.Sin(phiPos) );

            //  get the rotation frame of boid at m_boidArray[i].Position:

            // Each boid is located on on the tangent of the sphere moving foward on the tagent plane;
            // The boid frame follows the Unity convention where the x is to the right on the tangent plane,
            //  the z is forward direction on the tangent plane, and the y axis is the up direction, which is
            // perpendicular to the tangent plane;
            // The up (y, back side) direction of the boid is negative to the  normal of the 2D circle mesh
            // of the boid; The normal direction points to the center of the sphere; The light is within
            // the sphere.

            // The "forward" in OpenGL is "-z".In Unity forward is "+z".Most hand - rules you might know from math are inverted in Unity
            //    .For example the cross product usually uses the right hand rule c = a x b where a is thumb, b is index finger and c is the middle
            //    finger.In Unity you would use the same logic, but with the left hand.

            //    However this does not affect the projection matrix as Unity uses the OpenGL convention for the projection matrix.
            //    The required z - flipping is done by the cameras worldToCameraMatrix.
            //    So the projection matrix should look the same as in OpenGL.

            // Compute the Unity affine frame for each boid on the sphere (uses the left hand rule).
            // The direction of the Position is used as the up direction [Y axis) (points to the dorsal part)
            // of the boid. The Z axis is set to the perpendicular to the plane formed by the Y axis
            // and the world UP vector  Vector3.up(0,1,0), and points the local forward of the boid. 
            // The z axis is the head (moving) direction  of the boid. The X axis is the local right 
            // of the 2D circle boid.


            Vector3 position = m_boidArray[i].Position;
            Vector3 YAxis = position.normalized; // The direction vector of the boid
            // is considered as the local up vector of the boid frame.
            
            Vector3 ZAxis  = Vector3.Cross(YAxis, Vector3.up ); // XAxis = perpendicular to the
            // plane formed by the boid up and the global up.
            // It is tangent to the surface of sphere, and used as the forward head direction
            // of the boid. 
            // 

            Vector3 XAxis  = Vector3.Cross(YAxis, ZAxis);  // the side (rightward) direction of the
                                                           // boid 

            Vector4 col0 = new  Vector4( XAxis[0], XAxis[1], XAxis[2], 0.0f);
            Vector4 col1 = new Vector4( YAxis[0], YAxis[1], YAxis[2], 0.0f);
            Vector4 col2 = new Vector4(ZAxis[0], ZAxis[1], ZAxis[2], 0.0f);
            Vector4 col3 = new  Vector4( position[0], position[1], position[2], 1.0f);

            Matrix4x4 boidFrame = new Matrix4x4(col0, col1, col2, col3);
                // XAxis, YAxis, ZAxis become the first, second, third columns of the boidFrame matrix
            //boidFrame.SetColumn(0, new  Vector4( XAxis[0], XAxis[1], XAxis[2], 0.0f) );
            //boidFrame.SetColumn(1, new Vector4( YAxis[0], YAxis[1], YAxis[2], 0.0f));
            //boidFrame.SetColumn(2, new Vector4(ZAxis[0], ZAxis[1], ZAxis[2], 0.0f));
            //boidFrame.SetColumn(3, new Vector4( position[0], position[1], position[2], 1.0f));

            m_boidArray[i].BoidFrame = boidFrame; // affine frame; Matrix4x4 is a value type

            //thetaDir = Random.Range(0, M_PI);
            //phiDir = Random.Range(0, 2 * M_PI); // azimuth for the head direction

            //m_boidArray[i].HeadDir = new Vector3(Mathf.Cos(phiDir), 0.0f, Mathf.Sin(phiDir));

            m_boidArray[i].HeadDir = ZAxis;
                // The HeadDir is defined relative to the boidFrame XZ axes; the actual moving direction
                //, the global headDir is computed by applying the boidFrame matrix to the local HeadDir


             initRadiusX = Random.Range(MinBoidRadius, MaxBoidRadius); // 0.1 ~ 0.3
             initRadiusY = Random.Range(MinBoidRadius, MaxBoidRadius);
             initRadiusZ = Random.Range(MinBoidRadius, MaxBoidRadius);

            //initScale = new Vector3(initRadiusX, initRadiusY, initRadiusZ);
            initScale = new Vector3(initRadiusX, initRadiusX, initRadiusX);
            initSpeed = Random.Range(_minSpeed, _maxSpeed);
       
             m_boidArray[i].Scale = initScale;

             m_boidArray[i].Speed = initSpeed;              


        } // for  (int i = 0; i < numberOfElements; i++)


    } // SetBoidArray()


    public void DetermineParamValue(string name,  out float paramValue)
    {
        // check if deltaTime is more than SceneDuration of  the action plan => reset SceneStartTime to
        // the current time.

        if (m_SimulationDeltaT >= m_AnimationCycle)
        {
            m_SceneStartTime = m_currTime;
           // m_SimulationDeltaT = 0f; // wrap to the beginning of actionPlan to repeat the scene

        }
        // ref float paramValue has a value before the function was alled


        m_SimulationDeltaT = m_currTime - m_SceneStartTime; //  the delta time since the beginning of the
                                                    // current animation cycle
        //Debug.Log("DeltaT=");
        //Debug.Log(deltaT);

        List<ActionPlanController.Action> timedActions = m_actionPlan[name];


        ActionPlanController.TimeInterval timedActionBinaryTree
                = m_actionPlanController.m_actionPlanWithBinaryTree[name];

        // find the interval in the timedActions to which deltaTime belongs
        // for (int j = 0; j < timedActions.Count; j++) // actionPlan is a piecewise linear function of each parameter
        // {

        // m_SimulationDeltaT satisfies one of the following conditions always
        // The first "action" for the current parameter, timedActions[0], has 
        // the first interval ( T[0], T[1]). If the delta is less than the midpoint of the interval
        // the midpoint value of the interval, timedActions[0].V, is used.
        if (m_SimulationDeltaT < (timedActions[0].T[0] + timedActions[0].T[1]) / 2)
            {
                paramValue = timedActions[0].V;
                return;
            } // first interval

            // The last "action" for the current parameter, timedActions[0], has 
            // the interval ( T[0], T[1]). If the delta is greater than the midpoint of the 
            // last interval, the midpoint value of the interval, 
            //  timedActions[timedActions.Count - 1].V, is used.

            else if (m_SimulationDeltaT >= (timedActions[timedActions.Count - 1].T[0] + timedActions[timedActions.Count - 1].T[1]) / 2)
            { // the ith action is found for the current parameter
                paramValue = timedActions[timedActions.Count - 1].V;
                return;
            } // last interval

            // Otherwise, the value of the current parameter is interpolated between the midpoint
            // value of the previous interval and the midpoint value of the current interval
            else
            { // m_SimulationDeltaT is between timedActions[k - 1].T[1] and timedActions[k].T[0]
                // for some k.

                // int k = findCurrentActionIndex(timedActions, m_SimulationDeltaT);
                int k = m_actionPlanController.searchForActionIndex(timedActionBinaryTree, m_SimulationDeltaT);

            //int actionIndex
            //              = m_actionPlanController.searchForActionIndex(timedActionBinaryTree, m_SimulationDeltaT);

            //if ( actionIndex == -1 )
            //    {
            //    Debug.LogError(" No action index has been found for the current time");

            //    #if UNITY_EDITOR
            //    // Application.Quit() does not work in the editor so
            //    UnityEditor.EditorApplication.isPlaying = false;
            //    #else
            //       Application.Quit();
            //   #endif

            //   }
            //if ( k != actionIndex)
            //     {
            //      Debug.LogError(" Binary Search does not work");
            //    m_writer.WriteLine("Binary Search does not work");
            //     #if UNITY_EDITOR
            //       // Application.Quit() does not work in the editor so
            //       UnityEditor.EditorApplication.isPlaying = false;
            //        #else
            //       Application.Quit();
            //       #endif

            //     }

                if (k == -1)
                {
                 Debug.LogError(" Some time intervals of the action plan table are specified incorrectly. " +
                       "please corrent them first");

                m_writer.WriteLine(" Some time intervals of the action plan table are specified incorrectly. " +
                       "please corrent them first");
                paramValue = -1; // undefined out paramValue

                #if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                   UnityEditor.EditorApplication.isPlaying = false;
                #else
                   Application.Quit();
                #endif

                }
                else
                {
                    float firstTimePoint = (timedActions[k - 1].T[0] + timedActions[k - 1].T[1]) / 2;
                    float secondTimePoint = (timedActions[k].T[0] + timedActions[k].T[1]) / 2;
                    float t = (m_SimulationDeltaT - firstTimePoint) /
                                       (secondTimePoint - firstTimePoint);

                    paramValue = timedActions[k - 1].V * (1 - t) + timedActions[k].V * t;
                    return;
                }
            } // else (intermediate intervals)

                   
} // DetermineParaValue()

public int findCurrentActionIndex(List<ActionPlanController.Action> timedActions, float simulationDeltaT)
{


    for (int k = 1; k <= timedActions.Count - 1; k++)
    {
            float firstTimePoint = (timedActions[k - 1].T[0] + timedActions[k - 1].T[1]) / 2;
            float secondTimePoint = (timedActions[k].T[0] + timedActions[k].T[1]) / 2;

            if (simulationDeltaT >=  firstTimePoint  && 
                         simulationDeltaT < secondTimePoint )
            {
               return k;
             }
             else continue;
    } // for

    //Debug.LogError(" Some time intervals of the action plan table are specified incorrectly. " +
    //   "please corrent them first");

    return -1;

}//findCurrentInterval()

protected void Simulate() // called from Update()
    {


        //var threadGroupSize = Mathf.CeilToInt( (float) BoidsNum / (float) BLOCK_SIZE);

        // Determine the values of the parameters according to actionPlan

        if (UseActionPlan)
        {
            DetermineParamValue("_SpeedFactor", out _speedFactor);
            DetermineParamValue("_ScaleFactor",  out _scaleFactor);


            DetermineParamValue("_SeparateRadius",out _separate.Radius);
            DetermineParamValue("_SeparateWeight", out _separate.Weight);

            DetermineParamValue("_AlignmentRadius",  out _alignment.Radius);
            DetermineParamValue("_AlignmentWeight",  out _alignment.Weight);

            DetermineParamValue("_CohesionRadius",  out _cohesion.Radius);
            DetermineParamValue("_CohesionWeight", out _cohesion.Weight);

            DetermineParamValue("_GroundFlockingWeight",  out _groundWeight.FlockingWeight);
            DetermineParamValue("_GroundDivergeWeight", out _groundWeight.DivergeWeight);
            DetermineParamValue("_GroundCirculationWeight",  out _groundWeight.CirculationWeight);

            DetermineParamValue("_CeilingFlockingWeight",  out _ceilingWeight.FlockingWeight);
            DetermineParamValue("_CeilingConvergeWeight",  out _ceilingWeight.ConvergeWeight);
            DetermineParamValue("_CeilingCirculationWeight", out _ceilingWeight.CirculationWeight);


            DetermineParamValue("_GroundMinHue",  out _groundMinHue);
            DetermineParamValue("_GroundMaxHue",  out _groundMaxHue);
            DetermineParamValue("_GroundMinSaturation",  out _groundMinSaturation);
            DetermineParamValue("_GroundMaxSaturation", out _groundMaxSaturation);
            DetermineParamValue("_GroundMinValue",  out _groundMinValue);
            DetermineParamValue("_GroundMaxValue",  out _groundMaxValue);

            DetermineParamValue("_GroundMinAlpha", out _groundMinAlpha);
            DetermineParamValue("_GroundMaxAlpha",out _groundMaxAlpha);



            DetermineParamValue("_CeilingMinHue", out _ceilingMinHue);
            DetermineParamValue("_CeilingMaxHue", out _ceilingMaxHue);
            DetermineParamValue("_CeilingMinSaturation",  out _ceilingMinSaturation);
            DetermineParamValue("_CeilingMaxSaturation", out _ceilingMaxSaturation);
            DetermineParamValue("_CeilingMinValue",  out _ceilingMinValue);
            DetermineParamValue("_CeilingMaxValue",  out _ceilingMaxValue);

            DetermineParamValue("_CeilingMinAlpha",  out _ceilingMinAlpha);
            DetermineParamValue("_CeilingMaxAlpha",  out _ceilingMaxAlpha);


            // apply the current values of the parameters to the compute shader

            m_BoidComputeShader.SetFloat("_SpeedFactor", _speedFactor);
            m_BoidComputeShader.SetFloat("_ScaleFactor", _scaleFactor);


            m_BoidComputeShader.SetFloat("_SeparateRadius", _separate.Radius);
            m_BoidComputeShader.SetFloat("_SeparateWeight", _separate.Weight);

            m_BoidComputeShader.SetFloat("_AlignmentRadius", _alignment.Radius);
            m_BoidComputeShader.SetFloat("_AlignmentWeight", _alignment.Weight);

            m_BoidComputeShader.SetFloat("_CohesionRadius", _cohesion.Radius);
            m_BoidComputeShader.SetFloat("_CohesionWeight", _cohesion.Weight);

            m_BoidComputeShader.SetFloat("_GroundFlockingWeight", _groundWeight.FlockingWeight);
            m_BoidComputeShader.SetFloat("_GroundDivergeWeight", _groundWeight.DivergeWeight);
            m_BoidComputeShader.SetFloat("_GroundCirculationWeight", _groundWeight.CirculationWeight);

            m_BoidComputeShader.SetFloat("_CeilingFlockingWeight", _ceilingWeight.FlockingWeight);
            m_BoidComputeShader.SetFloat("_CeilingConvergeWeight", _ceilingWeight.ConvergeWeight);
            m_BoidComputeShader.SetFloat("_CeilingCirculationWeight", _ceilingWeight.CirculationWeight);



            m_BoidComputeShader.SetFloat("_GroundMinHue", _groundMinHue);
            m_BoidComputeShader.SetFloat("_GroundMaxHue", _groundMaxHue);
            m_BoidComputeShader.SetFloat("_GroundMinSaturation", _groundMinSaturation);
            m_BoidComputeShader.SetFloat("_GroundMaxSaturation", _groundMaxSaturation);
            m_BoidComputeShader.SetFloat("_GroundMinValue", _groundMinValue);
            m_BoidComputeShader.SetFloat("_GroundMaxValue", _groundMaxValue);

            m_BoidComputeShader.SetFloat("_GroundMinAlpha", _groundMinAlpha);
            m_BoidComputeShader.SetFloat("_GroundMaxAlpha", _groundMaxAlpha);



            m_BoidComputeShader.SetFloat("_CeilingMinHue", _ceilingMinHue);
            m_BoidComputeShader.SetFloat("_CeilingMaxHue", _ceilingMaxHue);
            m_BoidComputeShader.SetFloat("_CeilingMinSaturation", _ceilingMinSaturation);
            m_BoidComputeShader.SetFloat("_CeilingMaxSaturation", _ceilingMaxSaturation);
            m_BoidComputeShader.SetFloat("_CeilingMinValue", _ceilingMinValue);
            m_BoidComputeShader.SetFloat("_CeilingMaxValue", _ceilingMaxValue);

            m_BoidComputeShader.SetFloat("_CeilingMinAlpha", _ceilingMinAlpha);
            m_BoidComputeShader.SetFloat("_CeilingMaxAlpha", _ceilingMaxAlpha);



            m_BoidComputeShader.SetFloat("_DeltaTime", Time.deltaTime);

            //Debug.Log("DeltaTime [second]=" + Time.deltaTime);

            //https://msdn.microsoft.com/en-us/library/windows/desktop/ff471566(v=vs.85).aspx
            //https://stackoverflow.com/questions/19860586/compute-shader-with-numthreads-1-1-1-runs-extremly-slow

            // The disptach call invokes threadGroupSize(256) * 1 * 1 Thread Groups in undefined order

           
           
            //cf.    m_KernelIdGround = m_BoidComputeShader.FindKernel("SimulateCSGround");
            m_BoidComputeShader.Dispatch(m_KernelIdGround, m_threadGroupSize, 1, 1);
           
            // Each thread group, e.g.  SV_GroupID = (2,0,0) will contain BLOCK_SIZE * 1 * 1 threads according to the
            // declaration "numthreads(BLOCK_SIZE, 1, 1)]" in the computeshader.
            // E.G., Thread ID  SV_GroupThreadID = (7,0,0) refers to a particular thread in a given thread group
            // This Thread ID is also represented by the global index SV_DispatchThreadID
            // = ( [2,0,0] * (BLOCK_SIZE,1,1] + [7,0,0] ) =(BoidID, 0,0)


        } // if (UseActionPlan)


        // for debugging
        //totalNumOfSimulations++;

        //Debug.Log("Iteration Num of Simuation:");
        //Debug.Log(totalNumOfSimulations);

        //Debug.Log("m_SimulationDeltaT of action plan:");
        //Debug.Log(m_SimulationDeltaT);

     
        //m_writer.WriteLine("Iteration Num of Simuation:" + totalNumOfSimulations);
        //m_writer.WriteLine("m_SimulationDeltaT of action plan:" + m_SimulationDeltaT);
        // StreamWriter(string path, bool append);
        // writer.WriteLine("Test");
        //// m_boids.m_BoidBuffer
       // m_BoidBuffer.GetData(m_boidArray); // used in LEDColorGenController

       // for (int i = 0; i < (int)m_BoidsNum / 100; i++)
       // {

       //     //Debug.Log("boidNo = "); Debug.Log(i);
       //     m_writer.WriteLine("boidNo = " + i);
       //     //Debug.Log("boid Wall No = ");
       //     m_writer.WriteLine("boid Wall No = " + m_boidArray[i].WallNo);

       //     // Debug.Log(m_boidArray[i].WallNo);
       //     //Debug.Log("position = = ");
       //     //Debug.Log(m_boidArray[i].Position);

       //     m_writer.WriteLine("position = = " + m_boidArray[i].Position);

       //     //Debug.Log("Boid Radius= ");
       //     //Debug.Log(m_boidArray[i].Position.magnitude);

       //     m_writer.WriteLine("Boid Radius= " + m_boidArray[i].Position.magnitude);

       //     //Debug.Log("Boid color (HSV) = = ");
       //     //Debug.Log(m_boidArray[i].ColorHSV); // h,s,l ranges from 0 to 1
       //     m_writer.WriteLine("Boid HSV: (atan2, angDeg, hDeg) = " + m_boidArray[i].ColorHSV);

       //     //Debug.Log("Boid color (RGB computed in shader) = = ");
       //     //Debug.Log(m_boidArray[i].Color);

       //     if (m_boidArray[i].WallNo == 0)
       //     {
       //         m_writer.WriteLine("Boid (_groundMinHue,_groundMaxHue,_groundMinValue," +
       //             "_groundMaxValue) in Script= " + _groundMinHue + " " +
       //             _groundMaxHue + " " + _groundMinValue + " " + _groundMaxValue);

       //      }
       //     else
       //     {
       //         m_writer.WriteLine("Boid (_ceilingMinHue,_ceilingMaxHue,_ceilingMinValue," +
       //            "_ceilingMaxValue) in Script= " + _ceilingMinHue + " " +
       //            _ceilingMaxHue + " " + _ceilingMinValue + " " + _ceilingMaxValue);

       //     }

       //// }

       //    m_writer.WriteLine("Boid (_minHue,_maxHue,_minValue,_maxValue) in shader= " + m_boidArray[i].Color);


           // Color color = Color.HSVToRGB(m_boidArray[i].ColorHSV.x / 360,
           //                   m_boidArray[i].ColorHSV.y, m_boidArray[i].ColorHSV.z);


            ////Debug.Log("color (RGB by Unity API) = = ");
            ////Debug.Log(color);

           // m_writer.WriteLine("Boid RGB from HSV by Unity API = = " + color);


            //    //    //m_boidArray[i].Color = new Vector4(color.r, color.g, color.b, m_boidArray[i].Color.w);


       // }//     for (int i = 0; i < (int)m_BoidsNum; i++)

            //BoidBuffer.SetData(m_boidArray); // buffer is R or RW

            /*
            BoidCountBuffer.GetData(boidCountArray);

            int numOfBoidsWithinBound = 0;
            for (int i = 0; i < numOfWalls; i++)
            {
                Debug.LogFormat("WallNo = {0}, num of boids within bound =  {1} , ratio over the total= {2}",
                               i, boidCountArray[i], boidCountArray[i] / (float)m_BoidsNum);


                numOfBoidsWithinBound += boidCountArray[i];


            }

            Debug.LogFormat("total num within bound= {0}, ratio over the tatal =  {1}",
                                  numOfBoidsWithinBound, numOfBoidsWithinBound / (float)m_BoidsNum);
           */

            //Color.HSVToRGB(h,s,v)

        } // Simulate()


    } // class SimpleBoids
