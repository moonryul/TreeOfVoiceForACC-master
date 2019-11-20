using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using UnityEngine.UI;

using System.IO.Ports;


public class IRSensorMasterController : MonoBehaviour
{

    // Setup events for sending LED data to m_LEDMasterController

    public delegate void OnAverageSignalReceived (int[] avgDistances);
    public event OnAverageSignalReceived onAverageSignalReceived;

    //////////////////////////////////
    //
    // Private Variable
    //
    //////////////////////////////////
    /// <summary>
    /// 
    public string m_portName = "COM0"; // should be specified in the inspector
    SerialPort m_serialPort;

     public byte[] m_IRDistanceBytes; // 4 bytes for two distances
    float m_Delay;
    public const int m_IRDistanceBytesCount = 4; // 

    //////////////////////////////////
    //
    // Function
    //
    //////////////////////////////////

  

    

    void Start()
    {
        // Serial Communication between C# and Arduino
        //http://www.hobbytronics.co.uk/arduino-serial-buffer-size = how to change the buffer size of arduino
        //https://www.codeproject.com/Articles/473828/Arduino-Csharp-and-Serial-Interface
        //Don't forget that the Arduino reboots every time you open the serial port from the computer - 
        //and the bootloader runs for a second or two gobbling up any serial data you send its way..
        //This takes a second or so, and during that time any data that you send is lost.
        //https://forum.arduino.cc/index.php?topic=459847.0


        // Set up the serial Port

        m_serialPort = new SerialPort(m_portName, 115200); // bit rate= 567000 bps

        //m_serialPort.ReceivedBytesThreshold = 4 * 2; // four sensors each with 2 bytes

        //m_SerialPort.ReadTimeout = 50;
        //m_serialPort.ReadTimeout = 1000;  // sets the timeout value before reporting error
        //  m_SerialPort1.WriteTimeout = 5000??

        try
        {
            m_serialPort.Open();
        }

        catch (Exception ex)
        {
            Debug.Log("Error:" + ex.ToString()) ;
        }

        //m_SerialToArduinoMgr = new SerialToArduinoMgr();

        //m_SerialToArduinoMgr.Setup();

        //var port = m_SerialToArduinoMgr.port;





        // Deletage SerialDataReceivedEventHandler is a "pointer" to a method
        // It refers to the code itself, not the value of evaluating the code
        //  public event SerialDataReceivedEventHandler DataReceived; in  SerialPort
        //namespace System.IO.Ports
        //        {
        //[MonitoringDescription("SerialPortDesc")]
        //public class SerialPort : Component

        m_serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

        //m_SerialPort.DataReceived += DataReceivedHandler; ???
        //Delegates can hold references only to methods defined with a method signature that exactly matches the signature of the delegate.

        //            public delegate void MyDelegate(string myString);
        //    Hide Copy Code
        //// instantiate the delegate and register a method with the new instance. 
        //MyDelegate del = new MyDelegate(MyMethod);
        //    After a delegate is instantiated, additional methods can be registered with the delegate instance, like this:
        //Hide Copy Code
        //    del += new MyDelegate(MyOtherMethod);
        //    At this point, the delegate can be invoked, like this:
        //Hide Copy Code
        //    del("my string value");


        //Because the Timer component cannot possibly know which specific method to call, it specifies a delegate type(and therefore signature of a method) 
        //    to be invoked.Then you connect your method — with the requisite signature — to the Timer component by registering your method 
        //    with a delegate instance of the delegate type expected by the Timer component. The Timer component can then run your code 
        //    by invoking the delegate which, in turn, calls your method.Note that the Timer component still knows nothing about your specific method. 
        //    All the Timer component knows about is the delegate.The delegate, in turn, knows about your method because you registered your method 
        //    with that delegate.
        //    The end result is that the Timer component causes your method to run, but without knowing anything about your specific method.

        //Rather than calling a method at that point, our code can invoke a delegate instance — which, in turn, calls any methods that are registered 
        //    with the delegate instance.
        //The C# compiler takes your delegate declaration and inserts a new delegate class in the output assembly.

        //A delegate that exists in support of an event is referred to as an "event handler". To be clear, an "event handler" is a delegate, 
        //    although delegates are frequently not event handlers.

        //        Line 1: public delegate void MyDelegate(string whatHappened);
        //Line 2: public event MyDelegate MyEvent;
        //Line 2 (i.e., the usage of the delegate type) is what turns that delegate into an event handler. 

        //    In order to communicate that a particular delegate type is being used as an event handler, a naming convention has emerged 
        //whereby the delegate type name ends with "Handler" (more on this later).

        //public delegate void EventHandler(object sender, EventArgs e);

        //public delegate void EventHandler(object sender, EventArgs e);

        //            Consider the following 1.x code that registers an event handling method with an event. This code explicitly instantiates the event handler (delegate) in order to register the associated method with the event.
        //Hide Copy Code
        //thePublisher.EventName += new MyEventHandlerDelegate(EventHandlingMethodName);
        //    The following 2.0+ code uses delegate inference to register the same method with the event. Notice the following code appears to register the event handling method directly with the event.
        //Hide Copy Code
        //    thePublisher.EventName += EventHandlingMethodName;

        //            static void EventHandlingMethod(object sender, EventArgs e)
        //            {
        //                Console.WriteLine("Handled by a named method");
        //            }

        //            thePublisher.EventName += new MyEventHandlerDelegate(EventHandlingMethod);
        //            The above logic can be rewritten with an anonymous method, like this:
        //Hide Copy Code
        //thePublisher.EventName += delegate {
        //    Console.WriteLine("Handled by anonymous method");
        //};

                              
        //http://www.csharpstudy.com/CSharp/CSharp-event.aspx
        //https://www.codeproject.com/articles/20550/c-event-implementation-fundamentals-best-practices

        m_IRDistanceBytes = new byte[m_IRDistanceBytesCount]; // 


        //// define an action
        //Action updateArduino = () => {       

        //   port.Write( m_IRDistances, 0, m_IRDistances.Length); 
        //    // The WriteBufferSize of the Serial Port is 1024, whereas that of Arduino is 64
        //    //https://stackoverflow.com/questions/22768668/c-sharp-cant-read-full-buffer-from-serial-port-arduino

        //};


        ////m_Thread = null;
        ////if(connected) { // create and start a thread for the action updateArduino
        //m_Thread = new Thread(new ThreadStart(updateArduino)); // ThreadStart() is a delegate (pointer type)
        //m_Thread.Start();

    }// void Start()



    void Update()
    {

    }


    void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {

        SerialPort port = (SerialPort)sender;

        //byte[] temp = new byte[port.ReadBufferSize];

        //Read(byte[] buffer, int offset, int count);
        //http://www.devkorea.co.kr/bbs/board.php?bo_table=m03_qna&wr_id=62215
        //https://www.instructables.com/id/Serial-Port-Programming-With-NET/

        //I think the serialport receives one byte at a time so you may have to use the readbyte and then compose the lines yourself.
        //try
        //{
        //    if (mySerialPort.BytesToRead > 0) //if there is data in the buffer
        //    {
        //        mySerialPort.ReadByte(); //read a byte
        //    }
        //    //other code that can execute without being held up by read method.
        //}
        //catch (IOException ex)
        //{
        //    //error handling logic
        //}
        //https://docs.microsoft.com/ko-kr/dotnet/api/system.io.stream.read?view=netframework-4.8


        //        Read(Byte[], Int32, Int32)
        //파생 클래스에서 재정의되면 현재 스트림에서 바이트의 시퀀스를 읽고, 읽은 바이트 수만큼 스트림 내에서 앞으로 이동합니다.
        //C#

        //복사
        //public abstract int Read(byte[] buffer, int offset, int count);
        //    매개 변수
        //buffer
        //Byte[]
        //바이트 배열입니다.이 메서드는 지정된 바이트 배열의 값이 offset과 (offset + count - 1) 사이에서 현재 원본으로부터 읽어온 바이트로 교체된 상태로 반환됩니다.
        //offset
        //Int32
        //현재 스트림에서 읽은 데이터를 저장하기 시작하는 buffer의 바이트 오프셋(0부터 시작)입니다.
        //count
        //Int32
        //현재 스트림에서 읽을 최대 바이트 수입니다.
        //반환
        //Int32
        //버퍼로 읽어온 총 바이트 수입니다. 이 바이트 수는 현재 바이트가 충분하지 않은 경우 요청된 바이트 수보다 작을 수 있으며 스트림의 끝에 도달하면 0이 됩니다.
        //예외
        //ArgumentException
        //offset 및 count의 합계가 버퍼 길이보다 큽니다.
        //ArgumentNullException
        //buffer가 null인 경우
        //ArgumentOutOfRangeException
        //offset 또는 count가 음수입니다.
        //IOException
        //I/O 오류가 발생했습니다.
        //NotSupportedException
        //스트림이 읽기를 지원하지 않습니다.
        //ObjectDisposedException
        //스트림이 닫힌 후에 메서드가 호출되었습니다.
        //예제
        //다음 예제에서는 사용 하는 방법을 보여 줍니다 Read 데이터 블록을 읽을 수 있습니다.



        //Stream s = new MemoryStream();
        //for (int i = 0; i < 122; i++)
        //{
        //    s.WriteByte((byte)i);
        //}
        //s.Position = 0;

        //// Now read s into a byte buffer with a little padding.
        //byte[] bytes = new byte[s.Length + 10];

        // //public abstract int Read(byte[] buffer, int offset, int count);

        // 4096 = SerialPort.ReadBufferSize
        int numBytesToRead = m_IRDistanceBytes.Length;

       
        int numBytesRead   = port.Read(m_IRDistanceBytes, 0, numBytesToRead);
        

        Console.WriteLine("number of bytes read: {0:d}", numBytesRead);
        
        int[] averageDistances = new int[m_IRDistanceBytesCount / 2]; // two bytes of m_IRDistances form a single distance

        int intForLeftByte0 = m_IRDistanceBytes[0] << 8;
        int intForRightByte0= m_IRDistanceBytes[1];

        averageDistances[0] = intForLeftByte0 | intForRightByte0;

        int intForLeftByte1 = m_IRDistanceBytes[2] << 8;
        int intForRightByte1 = m_IRDistanceBytes[3];

        averageDistances[1] = intForLeftByte1 | intForRightByte1;
               

        onAverageSignalReceived.Invoke(averageDistances);


    } //     void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)


}//public class IRSensorMasterController 

