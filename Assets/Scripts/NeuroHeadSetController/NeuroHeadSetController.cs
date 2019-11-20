using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using UnityEngine;

using System.Timers;

//https://forum.unity.com/threads/monitor-components-by-bipbipspil-released.278524/
//https://stackoverflow.com/questions/27491406/understanding-c-sharp-field-initialization-requirements/27492048

//namespace WindowsForm {
public class NeuroHeadSetController : MonoBehaviour

{
        //https://stackoverflow.com/questions/38099520/i-dont-understand-why-we-need-the-new-keyword
        //The variable initializers are executed in the textual order in which they appear in the class declaration.
        //A variable initializer for an instance field cannot reference the instance being created.
        //Thus, it is a compile-time error to reference this in a variable initializer, 
        //as it is a compile-time error for a variable initializer to reference any instance member through a simple-name
        //Since all field initializers are translated into instructions in the constructor(s) 
        //which go before any other statements in the constructor,
        //there is no technical reason why this should not be possible. So it is a design choice.
    public struct AxisY
        {
            public int Maximum;
            public int Minimum;
        }

     public struct Chart
        {
            public List<double> yValues;
            public AxisY axisY;

        };

    // parameters for sening EEG data
    public int m_amplitudeRange = 1; // -1 to 1 mV
    public int m_notchFilter = 0;
    public int m_standardFilter =0;


    public double[] m_electrodeData;

    public Chart chart1, chart2, chart3, chart4, chart5, chart6, chart7, chart8, chart9;

   
    public string m_fileToWriteEEG = "EEGLog.txt";

    public string m_portName = "COM0"; // should be specified in the inspector

    SerialPort m_serialPort; 

    StreamWriter sw;

    bool m_isRecordingOn = false;

    // Setup events for sending LED data to m_LEDMasterController

    public delegate void OnAverageSignalReceived(double[] EEGData8Channels);
    public event OnAverageSignalReceived onAverageSignalReceived;


    struct DaneSerialPort // bytes array at arrived at the serial port
      {
            public byte[] zmienna; // buffer
      };

    Queue<DaneSerialPort> driver = new Queue<DaneSerialPort>(); // driver = queue of the arrived byte arrays.



    public void Start()
    {
        // Set up the serial Port

        m_electrodeData = new double[8];

        m_serialPort = new SerialPort(m_portName, 115200); // bit rate= 567000 bps


        //m_SerialPort.ReadTimeout = 50;
        m_serialPort.ReadTimeout = 1000;  // sets the timeout value before reporting error
                                          //  m_SerialPort1.WriteTimeout = 5000??

        try
        {
            m_serialPort.Open();
        }

        catch (Exception ex)
        {
            Debug.Log( "Error:" + ex.ToString() );

        }

        m_serialPort.DataReceived += SerialPort_DataReceived;

        Timer t = new Timer();
        t.Elapsed += new ElapsedEventHandler(OnTimer);
        t.Interval = 500;
        t.Start();

   
        //serialPort1
        chart1 = new Chart();
        chart1.yValues = new List<double>();


        chart2 = new Chart();
        chart2.yValues = new List<double>();

        chart3 = new Chart();
        chart3.yValues = new List<double>();

        chart4 = new Chart();
        chart4.yValues = new List<double>();

        chart5 = new Chart();
        chart5.yValues = new List<double>();

        chart6 = new Chart();
        chart6.yValues = new List<double>();

        chart7 = new Chart();
        chart7.yValues = new List<double>();

        chart8 = new Chart();
        chart8.yValues = new List<double>();

        chart9 = new Chart();
        chart9.yValues = new List<double>();





        //InitializeComponent();

        //foreach (string s in SerialPort.GetPortNames())
        //{
        //    comboBox1.Items.Add(s);
        //}

        //The Cyton board has an on-board RFDuino radio module acting as a "Device". 
        //The Cyton system includes a USB dongle for the PC, which acts as the RFDuino "Host".

        // USB has the rate of data transfer 12 Mbps for disk-drives and 1.5Mbps for devices
        // that need less bandwidth.
        // Serial ports are also known as communication (COM) ports = 11200bps.
        //https://github.com/OpenBCI/Docs/blob/master/Hardware/03-Cyton_Data_Format.md
        //Each FTDI adapter contains a USB microcontroller which talks a proprietary protocol 
        //via USB and transforms that into the regular UART signals and vice versa.

        //The RFDuino USB dongle (the RFDuino "Host") is connected to an FTDI (USB<->Serial)
        // converter configured to appear to the computer as if it is a standard serial port running 
        //at a rate of 115200 baud using the typical 8-N-1.

        //comboBox1.SelectedItem = "COM16";

        ////textBox2.Text = Application.StartupPath;
        //textBox3.Text = "test.txt";

        //radioButton4.Checked = true;
        //radioButton5.Checked = true;
        //radioButton8.Checked = true;

        //button1.Enabled = true;

        //button2.Enabled = false;
        //button3.Enabled = false;
        //button4.Enabled = false;
        //button5.Enabled = false;
        //button6.Enabled = false;

        //checkBox1.Checked = true;
        //checkBox2.Checked = true;
        //checkBox3.Checked = true;
        //checkBox4.Checked = true;
        //checkBox5.Checked = true;
        //checkBox6.Checked = true;
        //checkBox7.Checked = true;
        //checkBox8.Checked = true;
    }// public void Start()



    // Event Handler
    //   void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            DaneSerialPort odebrane_dane; //the arrived byte array.

            byte[] buffer = new byte[m_serialPort.BytesToRead];

            m_serialPort.Read(buffer, 0, buffer.Length);

            odebrane_dane.zmienna = buffer;

            driver.Enqueue(odebrane_dane); // add the current buffer content to the buffer queue
    }//SerialPort_DataReceived

    // Event Handler
    //private void button1_Click(object sender, EventArgs e)
    //{
    //    serialPort.PortName = comboBox1.Text;
    //    serialPort1.BaudRate = 115200;
    //    try
    //    {
    //        serialPort1.Open();
    //        button1.Enabled = false;
    //        button2.Enabled = true;
    //        button3.Enabled = true;
    //    }
    //    catch
    //    {
    //        MessageBox.Show("Nie można otworzyć portu" + comboBox1.Text.ToString());
    //    }
    //   serialPort1.DataReceived += serialPort1_DataReceived;
    //public event SerialDataReceivedEventHandler DataReceived; in System.IO.Ports;
    // System.EventHandler () vs. SerialDataReceivedEventHandler()

    // delegate void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e) DataReceived ;
    //=> this creates a new delegate (pointer)

    //Event Performance: C# vs. UnityEvent https://jacksondunstan.com/articles/3335

    // Event Handler
    private void StopTransmission(object sender, EventArgs e)
        {
            turnOFF_SW();
            turnOFF_transmision();
            m_serialPort.Close();
            //button1.Enabled = true;
            //button2.Enabled = false;
            //button3.Enabled = false;
            //button4.Enabled = false;
            //button5.Enabled = false;
            //button6.Enabled = false;
        }

        //Once the OpenBCI has initialized itself and sent the $$$, it waits for commands.
        //    In other words, it sends no data until it is told to start sending data.To begin data transfer, transmit a single ASCII b.
        //    Once the b is received, continuous transfer of data in binary format will ensue. To turn off the fire hose, send an s.
        private void StartTransmission(object sender, EventArgs e)
        {
            char[] buff = new char[1];
            buff[0] = 'b';  //  Once the b is received, continuous transfer of data in binary format will ensue
            try
            {
                m_serialPort.Write(buff, 0, 1);// Write(char[] buffer, int offset, int count);
            }
            catch
            {
                Debug.Log( "EEG transmittion Failed");
            }
           
        }

        private void TurnOffTransmission(object sender, EventArgs e)
        {
            turnOFF_SW();
            turnOFF_transmision();////To turn off the fire hose, send an s.
            //button4.Enabled = false;
            //button3.Enabled = true;
        }

        private void WriteToFile(object sender, EventArgs e)
        {
            string dictPath = UnityEngine.Application.dataPath + "/" + m_fileToWriteEEG;

           try
            {
                Directory.CreateDirectory(UnityEngine.Application.dataPath  );
                sw = new StreamWriter(dictPath);
                m_isRecordingOn = true;
            }
            catch
            {
                Debug.LogError("File for EEG recording failed to be created.");
            }
            //button5.Enabled = false;
            //button6.Enabled = true;
        }

        private void TurnOffRecording(object sender, EventArgs e)
        {
            turnOFF_SW();
        //    button5.Enabled = true;
        //    button6.Enabled = false;
        }

        // Event Handler
        private void OnTimer(object sender, EventArgs e)
        {
            double[] daneRys;

            // read all the data in the queue:

            while (driver.Count > 0) // driver = the queue of buffer contents
            {
                // get the current data buffer at the front of the queue

                DaneSerialPort data = driver.Dequeue(); // driver.Count decreases

                for (int g = 0; g < data.zmienna.Length; g++)
                {   // interpret the current byte data.zmienna[g]

                    daneRys = Convert.interpretBinaryStream(data.zmienna[g]); // public byte[] zmienna; // buffer

                    if (daneRys != null) // the 8 channel data is completely read => Add the double data of each channel to the list of
                                          // values for each channel.
                    {
                        //daneRys = ValueOrZero(daneRys); // daneRys = array of 8 places => ValueOrZero sets the non-activated channel to 0
                                                        //  which indicates which channel is active.
                        writeEEGToFile(daneRys); // within This function, daneRys is converted to microVolt unit
                        createEEGPlot(daneRys);
                    }

                    // else: get the next byte
                } // get the next buffer at the queue
            }

    }// OnTimer

    private void writeEEGToFile(double[] daneRys)
        {
            //ScaleFactor (Volts/count) = 4.5 Volts / gain / (2^23 - 1); gain =24; 

            double mnoz = (4.5 / 24 / (Math.Pow(2, 23) - 1)) * (Math.Pow(10, 6));

            for (int i = 0; i < 8; i++)
            {
                daneRys[i + 1] = daneRys[i + 1] * mnoz;
            }
            if (m_isRecordingOn)
            {
                sw.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}", daneRys[0], daneRys[1], daneRys[2], daneRys[3], daneRys[4], daneRys[5], daneRys[6], daneRys[7], daneRys[8], daneRys[9], daneRys[10], daneRys[11]);
            }
        }

    private void createEEGPlot(double[] dane)
    {
        dane = Filtering(dane);
        //DataPoint Add(params double[] y);
        // public int AddXY(double xValue, double yValue);

        // Draw Lines:
        //https://gamedev.stackexchange.com/questions/96964/how-to-correctly-draw-a-line-in-unity
        //https://docs.unity3d.com/ScriptReference/Array.html
        //https://loadofprogrammer.tistory.com/145
        //https://www.educba.com/c-sharp-array-vs-list/

        chart1.yValues.Add(dane[0]); // X= 1, Y= dane[0] = the sample number => chart1; chart1.Series[i] is the ith graph; dane[0] is the first element of params array y
        chart2.yValues.Add(dane[1]);  // X=2; Y= dane[1] = the 3 byte data for the channel 1 => chart2. 
        chart3.yValues.Add(dane[2]);  // X=3, Y= dane[2] = the 3 byte data for the channel 2 => chart3
        chart4.yValues.Add(dane[3]);//...
        chart5.yValues.Add(dane[4]);
        chart6.yValues.Add(dane[5]);
        chart7.yValues.Add(dane[6]);
        chart8.yValues.Add(dane[7]);
        chart9.yValues.Add(dane[8]);   // X=9, Y = dane[9] = the 3 byte data for the channel 9 => chart10

        //while (chart1.Count > 1250)  { // chart1 has more than 1250 time points ==> remove the data until it is less than 1250

        //    chart1.RemoveAt(chart1.Count - 1);
        //    chart2.RemoveAt(chart2.Count - 1);
        //    chart3.RemoveAt(chart3.Count - 1);
        //    chart4.RemoveAt(chart4.Count - 1);
        //    chart5.RemoveAt(chart5.Count - 1);
        //    chart6.RemoveAt(chart6.Count - 1);
        //    chart7.RemoveAt(chart7.Count - 1);
        //    chart8.RemoveAt(chart8.Count - 1);
        //    chart9.RemoveAt(chart9.Count - 1);


        //}// while (chart1.Count > 1250)  // chart1 has more than 1250 data 

        if (chart1.yValues.Count > 1250)
        {

            chart1.yValues.RemoveRange(1250, chart1.yValues.Count - 1250); // ( count1.Count-1 ) - (1250-1)
            chart2.yValues.RemoveRange(1250, chart2.yValues.Count - 1250); // ( count1.Count-1 ) - (1250-1)
            chart3.yValues.RemoveRange(1250, chart3.yValues.Count - 1250); // ( count1.Count-1 ) - (1250-1)
            chart4.yValues.RemoveRange(1250, chart4.yValues.Count - 1250); // ( count1.Count-1 ) - (1250-1)
            chart5.yValues.RemoveRange(1250, chart5.yValues.Count - 1250); // ( count1.Count-1 ) - (1250-1)
            chart6.yValues.RemoveRange(1250, chart6.yValues.Count - 1250); // ( count1.Count-1 ) - (1250-1)
            chart7.yValues.RemoveRange(1250, chart7.yValues.Count - 1250); // ( count1.Count-1 ) - (1250-1)
            chart8.yValues.RemoveRange(1250, chart8.yValues.Count - 1250); // ( count1.Count-1 ) - (1250-1)
            chart9.yValues.RemoveRange(1250, chart9.yValues.Count - 1250); // ( count1.Count-1 ) - (1250-1)

        }

        // Raise the event of reporting the filtered EEG data to NeuroHeadSetController.

        //m_electrodeData[0] = chart2.yValues[0];
        //m_electrodeData[1] = chart3.yValues[0];
        //m_electrodeData[2] = chart4.yValues[0];
        //m_electrodeData[3] = chart5.yValues[0];
        //m_electrodeData[4] = chart6.yValues[0];
        //m_electrodeData[5] = chart7.yValues[0];
        //m_electrodeData[6] = chart8.yValues[0];
        //m_electrodeData[7] = chart9.yValues[0];

         m_electrodeData = new double[] {chart2.yValues[0] ,chart3.yValues[0],chart4.yValues[0], chart5.yValues[0],
                                          chart6.yValues[0],chart7.yValues[0],chart8.yValues[0],chart9.yValues[0] };


        // raise the filtered EEG data received event

        onAverageSignalReceived.Invoke( m_electrodeData );

        } //private void createEEGPlot(double[] dane)


        //https://neurobb.com/t/openbci-why-are-1-50hz-bandpass-and-60hz-notch-filters-both-applied-by-default/23/2

        private double[] Filtering(double[] dane)
        {
            

            for (int i = 0; i < 8; i++)
            {
                dane[i + 1] = Filters.FiltersSelect(m_standardFilter, m_notchFilter, dane[i + 1], i);
            }

            return dane;
    } // Filtering

    private void turnOFF_SW()
        {
            if (m_isRecordingOn)
            {
                m_isRecordingOn = false;
                sw.Close();
            }
        }

     private void turnOFF_transmision()
        {
            char[] buff = new char[1];
            buff[0] = 's';//To turn off the fire hose, send an s.
            m_serialPort.Write(buff, 0, 1);
        }


} // NeuroHeadSetController

