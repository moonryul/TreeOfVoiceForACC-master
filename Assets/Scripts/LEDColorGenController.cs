using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System;
using System.IO;
using Random = UnityEngine.Random;

public class LEDColorGenController : MonoBehaviour
{

    //ComputeBuffer m_BoidLEDRenderDebugBuffer;
    //ComputeBuffer m_BoidLEDRenderDebugBuffer0;

    //// // ComputeBuffer(int count, int stride, ComputeBufferType type);

    //BoidLEDRenderDebugData[] m_ComputeBufferArray;
    //Vector4[] m_ComputeBufferArray0;

    public struct BoidLEDRenderDebugData
    {
        // public Vector3  WallOrigin; // the reference position of the wall (the boid reference frame) on which the boid is 

        //public Vector3 EulerAngles; // the rotation of the boid reference frame
        public int BoidLEDID; // the position of the  boid in the boid reference frame        

        public int  NearestBoidID; // the scale factors
        public int  NeighborCount; // heading direction of the boid on the local plane
        public float NeighborRadius; // the radius of the circle boid
        public Vector4 NearestBoidColor;         // RGBA color
        public Vector4 AvgColor;         // RGBA color

    }


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



    //public struct BoidData
    //{
    //    // public Vector3  WallOrigin; // the reference position of the wall (the boid reference frame) on which the boid is 

    //    //public Vector3 EulerAngles; // the rotation of the boid reference frame
    //    public Vector3 Position; // the position of the  boid in the boid reference frame        

    //    public Vector3 Scale; // the scale factors
    //    public Vector3 HeadDir; // heading direction of the boid on the local plane
    //    public float Speed;            // the speed of a boid

    //    public float Radius; // the radius of the circle boid
    //    public Vector3 ColorHSV;
    //    public Vector4 ColorRGB;         // RGBA color
    //    public Vector2 SoundGrain; // soundGrain = (freq, amp)
    //    public float Duration;     // duration of a boid each frame
    //    public int WallNo;      // the number of the wall on which the boid lie. 0=> the ground
    //                            // 1 => the ceiling, 2 => left wall, 3=> right wall. 4=> front wall
    //}

  
    public SimpleBoidsTreeOfVoice m_boids;
    public ActionPlanController m_actionPlanController;

    public int m_colorSamplingMethod = 0; // 0 = get the nearest neighbor color
                                          // 1 = get the average color of the neighbors

    public int m_totalNumOfLEDs; // computed within script

  
    public float m_samplingRadius = 0.05f; //10cm

    public float m_LEDChainHeight = 9; // the height of the LED chain 

    public float m_Hemisphere = 1;

    // m_HemisphereGroundPosition is the reference position. If it is positive or zero,
    // the hemisphere above this position will be used to sample LED colors
    // It ranges from - m_MaxDomainRadius to m_DomainRadius; from the bottom of the sphere to 
    // the top of the sphere. 

  

    //https://www.reddit.com/r/Unity3D/comments/7ppldz/physics_simulation_on_gpu_with_compute_shader_in/

    protected int m_kernelIDLED;

    [SerializeField] protected ComputeShader m_BoidLEDComputeShader; 
    // m_BoidLEDComputeShader is set to SampleLEDColors.compute in the inspector          
    public ComputeBuffer m_BoidLEDBuffer { get; protected set; }

    BoidLEDData[] m_BoidLEDArray;

    byte[] m_LEDArray;

   
    
    public float m_LEDInterval = 0.2f; // 20cm
    //public int m_SphericalMotion = 0;  

    public float m_startingRadiusOfInnerChain = 0.1f; // m
    // the startingRadiusOfInnerChain should be equal to the CeilingInnerRadius of SimpleBoidsForTreeOfVoice
    public float m_endingRadiusOfInnerChainThreeTurns = 0.8f;

    public float m_startingRadiusOfOuterChain = 1.4f; // m
    public float m_endingRadiusOfOuterChainThreeTurns = 2f;

   // public float m_MaxDomainRadius = 10; // 10
    //public float m_MinDomainRadius = 0.7f; // 0.7

    //public float m_MaxChainRadius = 2; // 2 m
    //public float m_MinChainRadius = 0.7f;  // 0.7 m

    //cf.  public float CeilingInnerRadius = 0.7f;

    public int m_firstChain = 30;
    public int m_secondChain = 44;
    public int m_thirdChain = 50;
    public int m_fourthChain = 53;

    public float m_beginFromInChain1 = -135f;
    public float m_beginFromInChain2 = -70f;



    float m_startAngleOfChain1; // this is the angle where r0, that is, a0 is defined, that is, on the local x axis
    float m_startAngleOfChain2;


    // delegate signature (interface definition)

    public delegate void LEDSenderHandler(byte[] m_LEDArray);
    public event LEDSenderHandler m_LEDSenderHandler;

    protected const int BLOCK_SIZE = 256; // The number of threads in a single thread group

    //protected const int MAX_SIZE_OF_BUFFER = 1000;

    int m_threadGroupSize;

    const float epsilon = 1e-2f;
    const float M_PI = 3.1415926535897932384626433832795f;


    //m_neuroHeadSetController.onAverageSignalReceived += m_ledColorGenController.UpdateLEDResponseParameter;
    //m_irSensorMasterController.onAverageSignalReceived += m_ledColorGenController.UpdateColorBrightnessParameter;

    StreamWriter m_writer;
    FileStream m_oStream;

    private void Awake()
    {// initialize me

        //// DEBUG code
        //string fileName = "LEDBoidGen";

        ////"yyyy.MM.dd.HH.mm.ss"
        //string fileIndex = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        //string path = "Assets/Resources/DebugFiles/" + fileName + fileIndex + ".txt";


        //File.CreateText(path).Dispose();
        ////FileStream fileStream = new FileStream(@"file_no.txt",
        ////                               FileMode.OpenOrCreate,
        ////                               FileAccess.ReadWrite,
        ////                               FileShare.None);

        ////Write some text to the test.txt file
        // m_writer = new StreamWriter(path, false); // do not append
        //m_ioStream = new FileStream(path,
        //                               FileMode.OpenOrCreate,
        //                               FileAccess.ReadWrite,
        //                               FileShare.None);

        //m_oStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
        //m_writer = new System.IO.StreamWriter(m_oStream);




        m_totalNumOfLEDs = m_firstChain  + m_secondChain
                          + m_thirdChain + m_fourthChain;

        m_startAngleOfChain1 = m_beginFromInChain1  * M_PI / 180; // degree
        m_startAngleOfChain2 = m_beginFromInChain2 *  M_PI / 180; // degree

        

        //m_threadGroupSize = Mathf.CeilToInt(m_BoidsNum / (float)BLOCK_SIZE);

        m_threadGroupSize = Mathf.CeilToInt(m_totalNumOfLEDs / (float)BLOCK_SIZE);



        m_LEDArray = new byte[m_totalNumOfLEDs * 3];

        //m_boidArray = new BoidData[ (int) m_BoidsNum ]; // for debugging


        if (m_BoidLEDComputeShader == null)
        {
            Debug.LogError("BoidLEDComputeShader  should be set in the inspector");
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying = false;
            UnityEditor.EditorApplication.Exit(0);
#else
                   Application.Quit();
#endif
        }


        m_kernelIDLED = m_BoidLEDComputeShader.FindKernel("SampleLEDColors");


        
        //  m_BoidLEDRenderDebugBuffer = new ComputeBuffer(m_totalNumOfLEDs,
        //                                  4 * sizeof(float), ComputeBufferType.Default);

        // Type of the buffer, default is ComputeBufferType.Default (structured buffer)
        //m_BoidLEDRenderDebugBuffer = new ComputeBuffer(m_totalNumOfLEDs, Marshal.SizeOf(typeof(BoidLEDRenderDebugData)));

        //m_BoidLEDRenderDebugBuffer0 = new ComputeBuffer(m_totalNumOfLEDs, Marshal.SizeOf(typeof(Vector4)));

        //// Set the ComputeBuffer for shader debugging
        //// But a RWStructuredBuffer, requires SetRandomWriteTarget to work at all in a non-compute-shader. 
        ////This is all Unity API magic which in some ways is convenient 

        //Graphics.SetRandomWriteTarget(1, m_BoidLEDRenderDebugBuffer);

        // m_ComputeBufferArray = new BoidLEDRenderDebugData[m_totalNumOfLEDs];
        //m_ComputeBufferArray0 = new Vector4[m_totalNumOfLEDs];

        //m_BoidLEDRenderDebugBuffer.SetData(m_ComputeBufferArray);
        //m_BoidLEDRenderDebugBuffer0.SetData(m_ComputeBufferArray0);



        Debug.Log("In Awake() in LEDColorGenController:");

        //for (int i = 0; i < m_totalNumOfLEDs; i++)
        //{
           
        //    Debug.Log(i + "th LED Position" + m_BoidLEDArray[i].Position);
        //    Debug.Log(i + "th LED HeadDir" + m_BoidLEDArray[i].HeadDir);
        //    Debug.Log(i + "th LED Color" + m_BoidLEDArray[i].Color);
        //    Debug.Log(i + "th LED Color: NeighborCount" + m_BoidLEDArray[i].NeighborCount);

        //}


        // m_BoidLEDComputeShader.SetBuffer(m_kernelIDLED, "_BoidLEDRenderDebugBuffer", m_BoidLEDRenderDebugBuffer);

        //m_BoidLEDComputeShader.SetBuffer(m_kernelIDLED, "_BoidLEDRenderDebugBuffer0", m_BoidLEDRenderDebugBuffer0);
    } // Awake()
    void Start()
    {
       //initialize others
        m_boids = this.gameObject.GetComponent<SimpleBoidsTreeOfVoice>();

      
        //m_BoidBuffer = m_boids.m_BoidBuffer;

        if (m_boids== null)
        {
            Debug.LogError("SimpleBoidsTreeOfVoice component should be added to CommHub");
            // Application.Quit();
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
          //  UnityEditor.EditorApplication.isPlaying = false;
            UnityEditor.EditorApplication.Exit(0);
#else
                   Application.Quit();
#endif

        }


        m_BoidLEDComputeShader.SetFloat("_CeilingInnerRadius", m_startingRadiusOfInnerChain);
        m_BoidLEDComputeShader.SetFloat("_MaxChainRadius", m_endingRadiusOfOuterChainThreeTurns);

        m_BoidLEDComputeShader.SetFloat("_Hemisphere", m_Hemisphere);

        m_BoidLEDComputeShader.SetFloat("_MaxDomainRadius", m_boids.m_MaxDomainRadius);
        // m_BoidLEDComputeShader.SetFloat("_MinDomainRadius", m_boids.m_MinDomainRadius);
        m_BoidLEDComputeShader.SetFloat("_CeilingInnerRadius", m_boids.m_CeilingInnerRadius);

        //m_BoidsNum = (int)m_boids.m_BoidsNum;

        m_BoidLEDComputeShader.SetInt("_BoidsNum", (int)m_boids.m_BoidsNum);
           
        m_BoidLEDComputeShader.SetBuffer(m_kernelIDLED,  "_BoidBuffer", m_boids.m_BoidBuffer);

        m_BoidLEDComputeShader.SetInt("_ColorSamplingMethod", m_colorSamplingMethod);

        m_BoidLEDComputeShader.SetFloat("_SamplingRadius", m_samplingRadius);


        //define BoidLED Buffer

        m_BoidLEDBuffer = new ComputeBuffer(m_totalNumOfLEDs, Marshal.SizeOf(typeof(BoidLEDData)));

        m_BoidLEDArray = new BoidLEDData[m_totalNumOfLEDs];

        //For each kernel we are setting the buffers that are used by the kernel, so it would read and write to those buffers

        // For the part of boidArray that is set by data are filled by null. 
        // When the array boidArray is created each element is set by null.

        // create a m_BoidLEDArray to link to m_BoidLEDBuffer:
        SetBoidLEDArray(m_BoidLEDArray); // THe Boid LEDs array is defined without their colors

        m_BoidLEDBuffer.SetData(m_BoidLEDArray); // buffer is R or RW

        m_BoidLEDComputeShader.SetBuffer(m_kernelIDLED, "_BoidLEDBuffer", m_BoidLEDBuffer);


    }// void Start()

    public void OnValidate()
    {
    }

    protected void SetBoidLEDArray(BoidLEDData[] m_BoidLEDArray)
    {


        float radius;
        float theta, phi;

        // Arange a chain of 40 LEDs (with interval of 50cm) along the the logarithmic spiral 
        // r = a * exp( b * theta), where 0 <= theta <= 3 * 2pi: 
        // band with radii 1m and 0.85m three  rounds. Arrange another chain along the circle band 
        // with radii 0.85m and 0.7m  three rounds, each round with differnt radii.
        //Two chains are arranged so that their LEDs are placed in a zigzag manner.

        //x = r*cos(th); y = r *sin(th)
        // x = a*exp(b*th)cos(th), y = a*exp(b*th)sin(th)
        // dr/dth = b*r; 

        //conditions: r0 = a exp( b 0) = 0.85; r1 = a exp( b* 3 * 2pi)
        // r0 = a exp(0) = a; a = r0; r1 = 0.85 * exp( b* 4pi) ==> b = the radius growth rate.

        // exp( b * 4pi) = r1/ a; b * 6pi = ln( r1/a). b = ln( r1/a) / 6pi;

        //  L( r(th), th0, th) = a( root( 1 + b^2) /b ) [ exp( b * th) - exp( b * th0) ]
        // = root(1 + b^2) / b * [ a*exp(b *th) - a * exp(th0) ] 
        // L( r(th), th0, th_i) =  root(1 + b^2)/b * [ r(th_i)  - r(th0)] = i * 0.5, 0.5= led Interval 
        //  => the value of th_i can be determined.

        // The ith LED  will be placed at location (r_i, th_i) such that  L(r(th_i), th0, th_i) = 0.5 * i, r_i = a*exp(b*th_i), 
        //  i =0 ~ 39

        //Define the parameters  a and b of the logarithmic spiral curve r = a * exp(b * th).

        float r0 = m_startingRadiusOfInnerChain;  // a1  in r = a1 * exp(b1 * th) is set so that the radius r0 is 0.7 when th =0;
        float r1 = m_endingRadiusOfInnerChainThreeTurns; // r1 = a1 exp (b1* 3 * 2pi)

        float r2 = m_startingRadiusOfOuterChain; ; //  a2  in r = a2 * exp( b2 * th); b2 is set so that r is r2 when th =0;
        float r3 = m_endingRadiusOfOuterChainThreeTurns; // r3 = a2* exp(b2* 3 * 2pi)

        float a1 = r0;
        float b1 = Mathf.Log(r1 / a1) / (6 * M_PI);

        float a2 = r2;
        float b2 = Mathf.Log(r3 / a2) / (6 * M_PI);
                
       
  
        
        Debug.Log("Inner Chain:");

        float LEDChainLength = 0;

        for (int i = 0; i < m_firstChain ; i++ )
        {
            // set the head direction of the boid:  direction angle on xz plane

            //  thi_i: the angle on the local coordinate system:

            float th_i = GetAngularPositionOfLED(a1, b1, 0.0f, ref LEDChainLength,
                                                 m_LEDInterval,  i);
            float r_i = a1 * Mathf.Exp(b1 * th_i );

            float th_i_g = th_i +  ( M_PI/180) * m_beginFromInChain1;

            //Debug.Log(i + "th LED Ploar POS (th,r) [global coord]:" + new Vector2(th_i_g * 180 / M_PI, r_i).ToString("F4"));

            m_BoidLEDArray[i].Position = new Vector3(r_i  * Mathf.Cos(th_i_g), m_LEDChainHeight, r_i * Mathf.Sin(th_i_g));

            // Set the rotation frame of LED boid at m_boidLEDArray[i].Position:

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


            Vector3 position = m_BoidLEDArray[i].Position;
            Vector3 YAxis = position.normalized; // The direction vector of the boid
                                                 // is considered as the up vector of the boid frame.

            Vector3 XAxis = Vector3.Cross(YAxis, Vector3.up); // XAxis = perpendicular to the
            // plane formed by the boid up and the global up. It is the rightward basis
            // left hand rule
            // 

            Vector3 ZAxis = Vector3.Cross(XAxis, YAxis);  // the forward direction 


            Matrix4x4 boidFrame = new Matrix4x4();
            // XAxis, YAxis, ZAxis become the first, second, third columns of the boidFrame matrix
            boidFrame.SetColumn(0, new Vector4(XAxis[0], XAxis[1], XAxis[2], 0.0f));
            boidFrame.SetColumn(1, new Vector4(YAxis[0], YAxis[1], YAxis[2], 0.0f));
            boidFrame.SetColumn(2, new Vector4(ZAxis[0], ZAxis[1], ZAxis[2], 0.0f));
            boidFrame.SetColumn(3, new Vector4(position[0], position[1], position[2], 1.0f));

            m_BoidLEDArray[i].BoidFrame = boidFrame; // affine frame

            

            m_BoidLEDArray[i].HeadDir = ZAxis;


            float initRadiusX = Random.Range(m_boids.MinBoidRadius, m_boids.MaxBoidRadius); // 0.1 ~ 0.3
            float initRadiusY = Random.Range(m_boids.MinBoidRadius, m_boids.MaxBoidRadius);
            float initRadiusZ = Random.Range(m_boids.MinBoidRadius, m_boids.MaxBoidRadius);


            // m_BoidLEDArray[i].Scale = new Vector3(initRadiusX, initRadiusY, initRadiusZ);
             m_BoidLEDArray[i].Scale = new Vector3(initRadiusX, initRadiusX, initRadiusX); 

            //m_writer.WriteLine(  i+ "th LED POS:" + m_BoidLEDArray[i].Position);
            //m_writer.WriteLine(i + "th LED frame:\n" + m_BoidLEDArray[i].BoidFrame);

        } // for  (int i )

        //Debug.Log("Second Chain:");
        //for (int i = 0; i < m_numOfChain2; i++)
        //{
        //    // set the head direction of the boid:  direction angle on xz plane

        //    float th_i = GetAngularPositionOfLED(a1, b1, m_startAngleOfChain2, ledInterval,i);
        //    float r_i = a1 * Mathf.Exp(b1 * th_i);

        //    Debug.Log(i + "th LED Ploar POS (th,r):" + (new Vector2(th_i * 180 / M_PI, r_i) ).ToString("F4") );

        //    m_BoidLEDArray[m_numOfChain1 + i].HeadDir = new Vector3(Mathf.Cos(th_i), 0.0f, Mathf.Sin(th_i));


        //    Debug.Log(i + "th LED HeadDir:" + m_BoidLEDArray[m_numOfChain1 + i].HeadDir.ToString("F4"));

        //    Vector3 ledPos = r_i * m_BoidLEDArray[m_numOfChain1 + i].HeadDir;

        //    m_BoidLEDArray[m_numOfChain1 + i].Position = ledPos;

        //    float initScaleX = Random.Range(MinCylinderRadius, MaxCylinderRadius); // 0.5 ~ 1.0
        //                                                                           //float initScaleY = Random.Range(MinCylinderRadius, MaxCylinderRadius);
        //                                                                           //float initScaleZ = Random.Range(MinCylinderRadius, MaxCylinderRadius);

        //    m_BoidLEDArray[m_numOfChain1 + i].Scale = new Vector3(initScaleX, initScaleX, initScaleX);

                       
        //    Debug.Log(i + "th LED POS:" + ledPos.ToString("F4") );

        //} // for  (int i )

        Debug.Log("Outer Chain:");
        LEDChainLength = 0;

        for (int i = 0; i < m_secondChain + m_thirdChain + m_fourthChain; i++)
        {
            // set the head direction of the boid:  direction angle on xz plane

            float th_i = GetAngularPositionOfLED(a2, b2, 0.0f, ref LEDChainLength,
                                                 m_LEDInterval,  i);
            float r_i = a2 * Mathf.Exp(b2 * th_i);

            float th_i_g = th_i + (M_PI / 180) * m_beginFromInChain2 ;

            // Debug.Log(i + "th LED Ploar POS (th,r):" + new Vector2(th_i_g * 180 / M_PI, r_i).ToString("F4"));

            m_BoidLEDArray[m_firstChain + i].Position = new Vector3(r_i * Mathf.Cos(th_i_g), m_LEDChainHeight, r_i * Mathf.Sin(th_i_g));

            // Set the rotation frame of LED boid at m_boidLEDArray[i].Position:

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


            Vector3 position = m_BoidLEDArray[i].Position;
            Vector3 YAxis = position.normalized; // The direction vector of the boid
                                                 // is considered as the local up vector of the boid frame.

            Vector3 ZAxis = Vector3.Cross(YAxis, Vector3.up); // XAxis = perpendicular to the
            // plane formed by the boid up and the global up.
            // It is tangent to the surface of sphere, and used as the forward head direction
            // of the boid. 
            // 

            Vector3 XAxis = Vector3.Cross(YAxis, ZAxis);  // the side (rightward) direction of the
                                                          // boid 

            Vector4 col0 = new Vector4(XAxis[0], XAxis[1], XAxis[2], 0.0f);
            Vector4 col1 = new Vector4(YAxis[0], YAxis[1], YAxis[2], 0.0f);
            Vector4 col2 = new Vector4(ZAxis[0], ZAxis[1], ZAxis[2], 0.0f);
            Vector4 col3 = new Vector4(position[0], position[1], position[2], 1.0f);

            Matrix4x4 boidFrame = new Matrix4x4(col0, col1, col2, col3);

           
            m_BoidLEDArray[m_firstChain + i].BoidFrame = boidFrame;

            m_BoidLEDArray[m_firstChain + i].HeadDir = ZAxis;


            float initRadiusX = Random.Range(m_boids.MinBoidRadius, m_boids.MaxBoidRadius); // 0.1 ~ 0.3
            float initRadiusY = Random.Range(m_boids.MinBoidRadius, m_boids.MaxBoidRadius);
            float initRadiusZ = Random.Range(m_boids.MinBoidRadius, m_boids.MaxBoidRadius);


            m_BoidLEDArray[m_firstChain + i].Scale = new Vector3(initRadiusX, initRadiusX, initRadiusX);

            //m_writer.WriteLine( (m_firstChain + i) + "th LED POS:" + m_BoidLEDArray[i].Position);
            //m_writer.WriteLine( (m_firstChain + i) + "th LED frame:" + m_BoidLEDArray[i].BoidFrame);

            // Debug.Log(i + "th LED POS:" + ledPos.ToString("F4"));

        } // for  (int i )

       // m_writer.Close();


        //Debug.Log("Fourth Chain:");
        //for (int i = 0; i < m_numOfChain4; i++)
        //{
        //    // set the head direction of the boid:  direction angle on xz plane

        //    float th_i = GetAngularPositionOfLED(a2, b2, m_startAngleOfChain4, ledInterval,i);
        //    float r_i = a2 * Mathf.Exp(b2 * th_i);


        //    Debug.Log(i + "th LED Ploar POS (th,r):" + new Vector2( th_i * 180 / M_PI, r_i).ToString("F4"));

        //    m_BoidLEDArray[m_numOfChain1 + m_numOfChain2 + m_numOfChain3 +  i].HeadDir = new Vector3(Mathf.Cos(th_i), 0.0f, Mathf.Sin(th_i));

        //    Debug.Log(i + "th LED HeadDir:" + m_BoidLEDArray[m_numOfChain1 + m_numOfChain2 + m_numOfChain3 + i].HeadDir.ToString("F4"));

        //    Vector3 ledPos = r_i * m_BoidLEDArray[m_numOfChain1 + +m_numOfChain2 + m_numOfChain3 + i].HeadDir;

        //    m_BoidLEDArray[m_numOfChain1 + +m_numOfChain2 + m_numOfChain3 + i].Position = ledPos;

        //    float initScaleX = Random.Range(MinCylinderRadius, MaxCylinderRadius); // 0.5 ~ 1.0
        //                                                                           //float initScaleY = Random.Range(MinCylinderRadius, MaxCylinderRadius);
        //                                                                           //float initScaleZ = Random.Range(MinCylinderRadius, MaxCylinderRadius);

        //    m_BoidLEDArray[m_numOfChain1 + m_numOfChain2 + m_numOfChain3 + i].Scale = new Vector3(initScaleX, initScaleX, initScaleX);




        //    Debug.Log(i + "th LED POS:" + ledPos.ToString("F4"));
        //} // for  (int i )



    } // SetBoidLEDArray()


    // Get th_i for the ith LED along the sprial curve r = a * exp(b*th_i)
    float GetAngularPositionOfLED(float a, float b, float th0, ref float LEDChainLength, float ledInterval, int ledNo)
    {// // The ith LED  will be placed at location (r_i, th_i)
     //    such that  L(r(th), th0, th_i) = root(1 + b^2)/b * [ r(th_i)  - r(th0)] =ledInterval  * i, 
 
        float r_th_0 = a * Mathf.Exp(b * th0);

        float r_th_i = (LEDChainLength) / (Mathf.Sqrt(1 + b * b) / b) + r_th_0;
        float th_i = Mathf.Log((r_th_i / a)) / b;

        LEDChainLength += ledInterval;

        return th_i;

    }    //    r(th_i)  = a*exp(b*th_i), 

    public void UpdateLEDResponseParameter(double[] electrodeData) // eight EEG amplitudes
    {
    }

    public  void UpdateColorBrightnessParameter(int[] approachVectors) // four approach vectors; for testing use only one
    {
    }


    void Update()
    {
        //  Debug.Log("I am updating the LED colors ih LEDColorGenController");

        //cf.    m_kernelIDLED = m_BoidComputeShader.FindKernel("SampleLEDColors");

        // Call a particular kernel "SampleLEDColors" in the m_BoidLEDComputeShader;

        // m_BoidBuffer is set by the dispatching  BoidComputeShader in SimpleBoidsTreeOfVoice;

        // Now set m_BoidLEDBuffer by dispatching BoidLEDCOmputeShader.

       // float currTime = Time.time; //  seconds

       
        // m_boids.DetermineParamValue("_SamplingRadius",  out m_samplingRadius);

       // m_BoidLEDComputeShader.SetFloat("_SamplingRadius", m_samplingRadius);

        m_boids.DetermineParamValue("_Hemisphere", out m_Hemisphere);
     
        m_BoidLEDComputeShader.SetFloat("_Hemisphere", m_Hemisphere );

        m_BoidLEDComputeShader.SetFloat("_SamplingRadius", m_samplingRadius); // you can change inspector variable' value at runtime

        m_BoidLEDComputeShader.Dispatch(m_kernelIDLED, m_threadGroupSize, 1, 1);

        //note:  m_BoidLEDComputeShader.SetBuffer(m_kernelIDLED, "_BoidLEDBuffer", m_BoidLEDBuffer);
        // note:   m_BoidLEDBuffer will be used  in:
        //  m_Boid m_boidLEDInstanceMaterial.SetBuffer("_BoidLEDBuffer", m_LEDColorGenController.m_BoidLEDBuffer);

        // Update is called once per frame
        //m_BoidLEDRenderDebugBuffer.GetData(m_ComputeBufferArray);
        //m_BoidLEDRenderDebugBuffer0.GetData(m_ComputeBufferArray0);

        m_BoidLEDBuffer.GetData(m_BoidLEDArray); // Get the boidLED data to send to the arduino

        // Debug.Log("BoidLEDRender Debug");



        //_BoidLEDRenderDebugBuffer0[pId][0] = (float)minIndex;
        //_BoidLEDRenderDebugBuffer0[pId][1] = minDist;
        //_BoidLEDRenderDebugBuffer0[pId][2] = neighborCnt;

        // for (int i = 0; i < m_totalNumOfLEDs; i++)


        //{
        // Debug.Log(i + "th boid LED ID =" + m_ComputeBufferArray[i].BoidLEDID);
        //Debug.Log(i + "th boid LED (min index) Nearest Neighbor ID=" + m_ComputeBufferArray[i].NearestBoidID);
        //Debug.Log(i + "th boid LED Neighbor Count=" + m_ComputeBufferArray[i].NeighborCount);
        //Debug.Log(i + "th boid LED Neighbor Radius=" + m_ComputeBufferArray[i].NeighborRadius);
        //Debug.Log(i + "th boid LED Nearest Neighbor Color=" + m_ComputeBufferArray[i].NearestBoidColor);
        //Debug.Log(i + "th boid LED Avg Color:" + m_ComputeBufferArray[i].AvgColor);

        //Debug.Log(i + "th boid LED min Index [ver0] =" + m_ComputeBufferArray0[i][0]);
        //Debug.Log(i + "th boid LED min Dist [ver0]=" + m_ComputeBufferArray0[i][1] );
        //Debug.Log(i + "th boid LED Neighbor Count [ver0]=" + m_ComputeBufferArray0[i][2]);

        //Debug.Log(i + "th boid LED Nearest Boid ID [m_BoidLEDBuffer] =" + m_BoidLEDArray[i].NearestBoidID);
        //Debug.Log(i + "th boid LED Nearest Boid Color [m_BoidLEDBuffer] =" + m_BoidLEDArray[i].Color);



        // }


        // Each thread group, e.g.  SV_GroupID = (0,0,0) will contain BLOCK_SIZE * 1 * 1 threads according to the
        // declaration "numthreads(BLOCK_SIZE, 1, 1)]" in the computeshader.

        //This call sets  m_BoidLEDBuffer, which is passed to the LED shader directly

        // LEDColorGenController.m_BoidLEDBuffer will be used to render the LED Branches by  _boidLEDInstanceMaterial.

        // Get the current values of the boidLEDs computed by BoidLEDComputeShader

        // debugging



        //m_BoidLEDBuffer.GetData(m_BoidLEDArray); // Get the boidLED data to send to the arduino

        ////public static float/iny Range(float min, float max);

        // Copy m_BoidLEDArray to m_LEDArray to send them to the master Arduino via serial communication.


        //Debug.Log("In Update() in LEDColorGenController:");



        //https://gamedev.stackexchange.com/questions/128976/writing-and-reading-computebuffer-in-a-shader

        m_BoidLEDBuffer.GetData(m_BoidLEDArray);

        //DEBUG code
        //Debug.Log("In Update(): LEDColorGenController:");

        //for (int i = 0; i < m_totalNumOfLEDs; i++)
        //{

        //    Debug.Log(i + "th LED Position" + m_BoidLEDArray[i].Position);
        //    Debug.Log(i + "th LED HeadDir" + m_BoidLEDArray[i].HeadDir);
        //    Debug.Log(i + "th LED Color" + m_BoidLEDArray[i].Color);

        //}



        for (int i = 0; i < m_totalNumOfLEDs; i++)
        {
            m_LEDArray[i * 3] =    (byte)(255 * m_BoidLEDArray[i].Color[0]); // Vector4 Color
            m_LEDArray[i * 3 + 1] = (byte)(255 * m_BoidLEDArray[i].Color[1]);
            m_LEDArray[i * 3 + 2] = (byte)(255 * m_BoidLEDArray[i].Color[2]);


            //Debug.Log(i + "th LED Position" + m_BoidLEDArray[i].Position.ToString("F4"));
            //Debug.Log(i + "th LED HeadDir" + m_BoidLEDArray[i].HeadDir.ToString("F4") );
            //Debug.Log(i + "th LED Color" + m_BoidLEDArray[i].Color.ToString("F4") );
            //Debug.Log(i + "th LED Color: NeighborCount" + m_BoidLEDArray[i].NeighborCount);





            //  Debug.Log(i + "th LED Position" + m_BoidLEDArray[i].Position);
            // Debug.Log(i + "th LED Color" + m_BoidLEDArray[i].Color);

            //Debug.Log(i + "th LED Color (value range check) from m_boids.m_boidArray" 
            //    + m_boids.m_boidArray[  m_BoidLEDArray[i].NearestBoidID ].Color );

            // for debugging, copy m_boidArray colors and positions and scales to m_BoidLEDArray
            //m_BoidLEDArray[i].Position = m_boids.m_boidArray[i].Position;
            //m_BoidLEDArray[i].Scale = m_boids.m_boidArray[i].Scale;
            //m_BoidLEDArray[i].Color = m_boids.m_boidArray[i].Color;

            //m_LEDArray[i * 3] = (byte)(255 * m_BoidLEDArray[i].Color[0]); // Vector4 Color
            //m_LEDArray[i * 3 + 1] = (byte)(255 * m_BoidLEDArray[i].Color[1]);
            //m_LEDArray[i * 3 + 2] = (byte)(255 * m_BoidLEDArray[i].Color[2]);
        }


        //m_BoidLEDBuffer.SetData( m_BoidLEDArray ); // LEDColorGenController.m_BoidLEDBuffer  is used

        // to rendeirng the boid LED cylinders in BoidRendererTreeOfVoice.

        //for (int i = 0; i < m_totalNumOfLEDs; i++)
        //{
        //    m_LEDArray[i * 3] = (byte)(255 * m_BoidLEDArray[i].Color[0]); // Vector4 Color
        //    m_LEDArray[i * 3 + 1] = (byte)(255 * m_BoidLEDArray[i].Color[1]);
        //    m_LEDArray[i * 3 + 2] = (byte)(255 * m_BoidLEDArray[i].Color[2]);
        //}


        Debug.Log("LED Data Send Event Handler called in LEDColorGenController");

        if (m_LEDSenderHandler is null)
        {
            Debug.LogError(" Event Handler Methods should be added to m_LEDSenderHandler in CommHub.cs");
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying = false;
            UnityEditor.EditorApplication.Exit(0);
#else
                   Application.Quit();
#endif

        }
        else
        {
            m_LEDSenderHandler.Invoke(m_LEDArray);
        }

 
     } // Update()



} //  LEDColorGenController class
