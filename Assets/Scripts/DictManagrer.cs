using UnityEngine;
//using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI; // has InputField
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System; // String class
using System.Linq; // To use Dictionary.ElementAt(i)
//using System.Xml.Serialization;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary; //BinaryFormatter Class 


using System.Xml.Linq;
using System.Text;


//https://answers.unity.com/questions/533058/simplest-dictionary-serialization-to-a-file.html
//void SaveActionPlan()
//{


//    // Create a hashtable of values that will eventually be serialized.
//    //Hashtable addresses = new Hashtable();
//    //addresses.Add("Jeff", "123 Main Street, Redmond, WA 98052");
//    //addresses.Add("Fred", "987 Pine Road, Phila., PA 19116");
//    //addresses.Add("Mary", "PO Box 112233, Palo Alto, CA 94301");

//    // To serialize the hashtable and its key/value pairs,  
//    // you must first open a stream for writing. 
//    // In this case, use a file stream.
//    FileStream fs = new FileStream("DictDataFile.dat", FileMode.Create);

//    //https://answers.unity.com/questions/1320236/what-is-binaryformatter-and-how-to-use-it-and-how.html

//    //// Construct a BinaryFormatter and use it to serialize the data to the stream.
//    //BinaryFormatter is used to serialize an object(meaning it converts it to one long stream of 1s and 0s), and deserialize it(converting 
//    //    that stream back to its usual form with all data intact), 
//    //and is typically used with to save data to the hard disk so it can be loaded again after the game is closed and started up again.
//    BinaryFormatter formatter = new BinaryFormatter();
//    try
//    {
//        //formatter.Serialize(fs, addresses);
//        //You can't access non-static members from a static method. (Note that Main() is static, which is a requirement of .Net).
//        //Just make siprimo and volteado static, by placing the static keyword in front of them. e.g.:
//        //static private long volteado(long a)
//        formatter.Serialize(fs, m_actionPlan);
//    }
//    catch (SerializationException e)
//    {
//        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
//        throw;
//    }
//    finally
//    {
//        fs.Close();
//    }
//}// void Serialize()


//void LoadActionPlan()
//{
//    // Declare the hashtable reference.
//    Hashtable addresses = null;

//    // Open the file containing the data that you want to deserialize.
//    FileStream fs = new FileStream("DictDataFile.dat", FileMode.Open);
//    try
//    {
//        BinaryFormatter formatter = new BinaryFormatter();

//        // Deserialize the hashtable from the file and 
//        // assign the reference to the local variable.
//        addresses = (Hashtable)formatter.Deserialize(fs);
//    }
//    catch (SerializationException e)
//    {
//        Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
//        throw;
//    }
//    finally
//    {
//        fs.Close();
//    }

//    // To prove that the table deserialized correctly, 
//    // display the key/value pairs.
//    foreach (DictionaryEntry de in addresses)
//    {
//        Console.WriteLine("{0} lives at {1}.", de.Key, de.Value);
//    }
//}//   void Deserialize()



//BinaryFormatter is used to serialize an object (meaning it converts it to one long stream of 1s and 0s), 
//and deserialize it(converting that stream back to its usual form with all data intact), and is typically used with 
//to save data to the hard disk so it can be loaded again after the game is closed and started up again.

//There's a good tutorial here that goes over using BinaryFormatter and FileStream to save and load data, but it's a bit long.

//As for simpler examples, these are bits of my own SaveManager class I ended up with after watching that tutorial:

// public class SaveManager
//{
//    private SaveGlob saveGlob;    // the Dictionary used to save and load data to/from disk
//    protected string savePath;
//    public SaveManager()
//    {
//        this.savePath = Application.persistentDataPath + "/save.dat";
//        this.saveGlob = new SaveGlob();
//        this.loadDataFromDisk();
//    }
//    /**
//     * Saves the save data to the disk
//     */
//    public void saveDataToDisk()
//    {
//        BinaryFormatter bf = new BinaryFormatter();
//        FileStream file = File.Create(savePath);
//        bf.Serialize(file, saveGlob);
//        file.Close();
//    }

//    /**
//     * Loads the save data from the disk
//     */
//    public void loadDataFromDisk()
//    {
//        if (File.Exists(savePath))
//        {
//            BinaryFormatter bf = new BinaryFormatter();
//            FileStream file = File.Open(savePath, FileMode.Open);
//            this.saveGlob = (SaveGlob)bf.Deserialize(file);
//            file.Close();
//        }
//    }



//void Serialize()
//{
//    // Create a hashtable of values that will eventually be serialized.
//    //Hashtable addresses = new Hashtable();
//    //addresses.Add("Jeff", "123 Main Street, Redmond, WA 98052");
//    //addresses.Add("Fred", "987 Pine Road, Phila., PA 19116");
//    //addresses.Add("Mary", "PO Box 112233, Palo Alto, CA 94301");

//    // To serialize the hashtable and its key/value pairs,  
//    // you must first open a stream for writing. 
//    // In this case, use a file stream.
//    FileStream fs = new FileStream("DictDataFile.dat", FileMode.Create);

//    // Construct a BinaryFormatter and use it to serialize the data to the stream.
//    BinaryFormatter formatter = new BinaryFormatter();
//    try
//    {
//        //formatter.Serialize(fs, addresses);
//        //You can't access non-static members from a static method. (Note that Main() is static, which is a requirement of .Net).
//        //Just make siprimo and volteado static, by placing the static keyword in front of them. e.g.:
//        //static private long volteado(long a)
//        formatter.Serialize(fs, m_actionPlan);
//    }
//    catch (SerializationException e)
//    {
//        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
//        throw;
//    }
//    finally
//    {
//        fs.Close();
//    }
//}// void Serialize()


//void Deserialize()
//{
//    // Declare the hashtable reference.
//    Hashtable addresses = null;

//    // Open the file containing the data that you want to deserialize.
//    FileStream fs = new FileStream("DictDataFile.dat", FileMode.Open);
//    try
//    {
//        BinaryFormatter formatter = new BinaryFormatter();

//        // Deserialize the hashtable from the file and 
//        // assign the reference to the local variable.
//        addresses = (Hashtable)formatter.Deserialize(fs);
//    }
//    catch (SerializationException e)
//    {
//        Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
//        throw;
//    }
//    finally
//    {
//        fs.Close();
//    }

//    // To prove that the table deserialized correctly, 
//    // display the key/value pairs.
//    foreach (DictionaryEntry de in addresses)
//    {
//        Console.WriteLine("{0} lives at {1}.", de.Key, de.Value);
//    }
//}//   void Deserialize()


public class DictManager: MonoBehaviour
    
  
{
    
    // Define delegate instances
    //public event Action<PointerEventData> onScroll, onPointerEnter;

    //ActionPlanController m_actionPlanController; // the reference is obtained from ActionPLanController component attached to
    //                                             // CommHub GameObject.
    //public Dictionary<String, List<SimpleBoidsTreeOfVoice.Action>> m_actionPlan; //// first defined in SimpleBoidsTreeOfVoice class

    ////public Dictionary<string, List<int>> myDict = new Dictionary<string, List<int>>();

    //public string fileName = "dict.bin";

    //string dictPath { get { return Application.dataPath + "/" + fileName; } }

    //string dictPath { get { return UnityEngine.Application.dataPath + "/" + fileName; } }

     void Start()
    {
        //// get it from ActionPLanController component attached to CommHub
        //m_actionPlanController = this.gameObject.GetComponent<ActionPlanController>();

        //if (m_actionPlanController == null)
        //{
        //    Debug.LogError("Add ActionPlanController component to CommHub gameObject");
        //    UnityEngine.Application.Quit();
         
        //}

        //m_actionPlan = m_actionPlanController.m_actionPlan;
    }
    public void SaveActionPlan() // zero argument EventHandler triggered by PointerClick event
    {
       // _SaveActionPlan(m_actionPlan, fileName);

    }
    public void LoadActionPlan() // Zero Argument Event Handler triggered by PointerClick event
    {
       // _LoadActionPlan(m_actionPlan, fileName);
    }
     void _SaveActionPlan(Dictionary<string, List<ActionPlanController.Action>> myDict, string fileName)
    {
        //string dictPath = UnityEngine.Application.dataPath + "/" + fileName;

        ////FileStream file = File.Create(dictPath);

        //FileStream file  = new FileStream( dictPath, FileMode.OpenOrCreate , FileAccess.Write);

        //BinaryFormatter bf = new BinaryFormatter();

        //bf.Serialize(file, myDict);
        //file.Close(); ;


    }


    void _LoadActionPlan( Dictionary<string, List<ActionPlanController.Action>> myDict, string fileName )
    {


        //// Open OpenDialog
        //OpenFileDialog openDialog = new OpenFileDialog();

        //openDialog.Filter = "ActionPlan(*.act)|*.act";

        //openDialog.InitialDirectory = Directory.GetCurrentDirectory();
        //openDialog.Title = "Open Action Plan";
        //if (openDialog.ShowDialog() != DialogResult.OK)
        //    return;

        //// FileStream(SafeFileHandle handle, FileAccess access);
        //FileStream file  = new FileStream(openDialog.FileName, FileMode.Open, FileAccess.Read);

        ////https://m.blog.naver.com/PostView.nhn?blogId=y2kgr&logNo=80208479466&categoryNo=13&proxyReferer=&proxyReferer=https%3A%2F%2Fwww.google.com%2F

        ////FileStream  fs = new FileStream(openDialog.FileName, FileMode.Open);
        ////reader = new StreamReader( fs, mode)

        //// https://docs.microsoft.com/ko-kr/dotnet/framework/winforms/controls/how-to-open-files-using-the-openfiledialog-component

        //// using (var reader = new StreamReader( openDialog.FileName)  )

        //BinaryFormatter bf = new BinaryFormatter();
        //myDict = (Dictionary<string, List< SimpleBoidsTreeOfVoice.Action > >) bf.Deserialize(file);
        //file.Close();   
        
    }

} //public class
