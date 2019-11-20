using System;
using System.Runtime.InteropServices;

using UnityEngine;
using System.Collections;
using System.Text;
using System.Xml.Serialization;

using SimpleFileBrowser;

//using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Collections.Generic;

using UnityEngine.EventSystems;

using System.Runtime.Serialization.Formatters.Binary; //BinaryFormatter Class 

/*
typedef struct tagOFN { 
  DWORD         lStructSize; 
  HWND          hwndOwner; 
  HINSTANCE     hInstance; 
  LPCTSTR       lpstrFilter; 
  LPTSTR        lpstrCustomFilter; 
  DWORD         nMaxCustFilter; 
  DWORD         nFilterIndex; 
  LPTSTR        lpstrFile; 
  DWORD         nMaxFile; 
  LPTSTR        lpstrFileTitle; 
  DWORD         nMaxFileTitle; 
  LPCTSTR       lpstrInitialDir; 
  LPCTSTR       lpstrTitle; 
  DWORD         Flags; 
  WORD          nFileOffset; 
  WORD          nFileExtension; 
  LPCTSTR       lpstrDefExt; 
  LPARAM        lCustData; 
  LPOFNHOOKPROC lpfnHook; 
  LPCTSTR       lpTemplateName; 
#if (_WIN32_WINNT >= 0x0500)
  void *        pvReserved;
  DWORD         dwReserved;
  DWORD         FlagsEx;
#endif // (_WIN32_WINNT >= 0x0500)
} OPENFILENAME, *LPOPENFILENAME; 
*/


//https://github.com/gkngkc/UnityStandaloneFileBrowser


//https://stackoverflow.com/questions/14366066/are-p-invoke-in-out-attributes-optional-for-marshaling-arrays

//https://www.mono-project.com/docs/advanced/pinvoke/
// WOW: Details https://www.mono-project.com/docs/advanced/pinvoke/#classes-and-structures-as-return-values
//http://originaldll.com/file/system.windows.forms.dll/31267.html
//cf:
//https://stackoverflow.com/questions/40472019/c-sharp-c-marshaling-an-in-out-struct-containing-char


//https://docs.microsoft.com/en-us/windows/win32/api/commdlg/ns-commdlg-openfilenamea

//Because a managed class is a reference type, when it is passed by value,
//a pointer to the class is passed to unmanaged code.
//    This is exactly what the unmanaged function expects.

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    //https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/w5tyztk9(v=vs.100)?redirectedfrom=MSDN#see-also

    public int structSize = 0;
    public IntPtr hwnd = IntPtr.Zero;
    public IntPtr inst = IntPtr.Zero;
    public string filter = null;
    public string customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public string file = null;
    public int maxFile = 0;
    public string fileTitle = null;
    public int maxFileTitle = 0;
    public string initialDir = null;
    public string title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public string defExt = null;
    public IntPtr custData = IntPtr.Zero;
    //public int custData = 0;
    public IntPtr pHook = IntPtr.Zero;
    public string templateName = null;

    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

//c++
//typedef struct tagOFNA {
//  DWORD         lStructSize; // int
//  HWND          hwndOwner;  //IntPtr
//  HINSTANCE     hInstance;  // IntPtr
//  LPCSTR        lpstrFilter; // string
//  LPSTR         lpstrCustomFilter; // string
//  DWORD         nMaxCustFilter;  // int
//  DWORD         nFilterIndex;   //int
//  LPSTR         lpstrFile;  //string
//  DWORD         nMaxFile;
//  LPSTR         lpstrFileTitle;
//  DWORD         nMaxFileTitle;
//  LPCSTR        lpstrInitialDir;
//  LPCSTR        lpstrTitle;
//  DWORD         Flags;
//  WORD          nFileOffset; // short
//  WORD          nFileExtension;  // short
//  LPCSTR        lpstrDefExt;   // string
//  LPARAM        lCustData;  // IntPtr
//  LPOFNHOOKPROC lpfnHook;   //IntPtr
//  LPCSTR        lpTemplateName; // string
//  LPEDITMENU    lpEditInfo; // string
//  LPCSTR        lpstrPrompt;  // string
//  void          *pvReserved; // IntPtr
//  DWORD         dwReserved; // int
//  DWORD         FlagsEx;   //int
//} OPENFILENAMEA, *LPOPENFILENAMEA;

//https://stackoverflow.com/questions/4991369/calling-unmanaged-c-code-from-c-sharp
// Unmaged Code in C#:
//https://www.codeproject.com/Articles/66244/Marshaling-with-C-Chapter-2-Marshaling-Simple-Type
//https://limbioliong.wordpress.com/2011/08/19/passing-a-pointer-to-a-structure-from-c-to-c-part-1/

//Call GetOpenFileNameW.You can do this without converting your entire app to Unicode which may be the most expedient solution.


//Windows API comes in 2 flavours, ANSI and Unicode.The former has functions with an A suffix. 
//The latter have a W suffix. You are currently using the former.

//https://stackoverflow.com/questions/40472019/c-sharp-c-marshaling-an-in-out-struct-containing-char

//I.e.GetOpenFileName is really a macro, and expands to GetOpenFileNameA by default. 

//https://stackoverflow.com/questions/39956697/marshal-wchar-t-from-c-to-c-sharp-as-an-out-parameter


//CallingConvention = CallingConvention.Cdecl) 
//platform invoke (P/invoke):
//P/Invoke is a technology that allows you to access structs, callbacks, and functions in unmanaged
//    libraries from your managed code.Most of the P/Invoke API is contained in two namespaces: 
//        System and System.Runtime.InteropServices.Using these two namespaces give you the tools
//        to describe
//    how you want to communicate with the native component.
//https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
//https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.callingconvention?view=netframework-4.8
public class DllTest
{
    //https://stackoverflow.com/questions/33815276/what-is-the-difference-between-in-out-and-ref-when-using-pinvoke-in-c
    //[DllImport( "Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, 
    //                 CharSet = CharSet.Auto )     ]

    [DllImport( "Comdlg32.dll", CharSet = CharSet.Auto )     ]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    //public static extern bool GetOpenFileName(   OpenFileName ofn);
    [DllImport("comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, 
                        CharSet = CharSet.Auto  ) ]
    public static extern bool  GetSaveFileName( [In,Out] OpenFileName sas);
    //public static extern bool  GetSaveFileName(   OpenFileName sas); 
    // OpenFileName is a class so sas is a reference to an object

    //The usage of ref or out is not arbitrary. If the native code requires pass-by-reference 
    //(a pointer) then you must use those keywords if the parameter type is a value type.
    //    So that the jitter knows to generate a pointer to the value. And you must omit them 
    //        if the parameter type is a reference type (class),
    //objects are already pointers under the hood.

    //definitive manual: https://docs.microsoft.com/en-us/dotnet/framework/interop/default-marshaling-behavior

}

//https://docs.microsoft.com/en-us/dotnet/framework/interop/

//The.NET Framework promotes interaction with COM components, COM+ services, external type libraries,
//    and many operating system services.Data types, method signatures, and error-handling mechanisms vary 
//    between managed and unmanaged object models.To simplify interoperation between.
//        NET Framework components and unmanaged code and to ease the migration path, 
//        the common language runtime conceals from both clients and servers the differences 
//        in these object models.


//Code that executes under the control of the runtime is called managed code.Conversely, 
//code that runs outside the runtime is called unmanaged code.COM components, ActiveX interfaces, and Windows API functions are examples of unmanaged code.
//https://docs.microsoft.com/en-us/dotnet/framework/interop/consuming-unmanaged-dll-functions

// saveFileDialog, readFile dialog
//https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.savefiledialog?view=netframework-4.8
//https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.openfiledialog?view=netframework-4.8

////https://answers.unity.com/questions/533058/simplest-dictionary-serialization-to-a-file.html
////void SaveActionPlan()
////{


////    // Create a hashtable of values that will eventually be serialized.
////    //Hashtable addresses = new Hashtable();
////    //addresses.Add("Jeff", "123 Main Street, Redmond, WA 98052");
////    //addresses.Add("Fred", "987 Pine Road, Phila., PA 19116");
////    //addresses.Add("Mary", "PO Box 112233, Palo Alto, CA 94301");

////    // To serialize the hashtable and its key/value pairs,  
////    // you must first open a stream for writing. 
////    // In this case, use a file stream.
////    FileStream fs = new FileStream("DictDataFile.dat", FileMode.Create);

////    //https://answers.unity.com/questions/1320236/what-is-binaryformatter-and-how-to-use-it-and-how.html

////    //// Construct a BinaryFormatter and use it to serialize the data to the stream.
////    //BinaryFormatter is used to serialize an object(meaning it converts it to one long stream of 1s and 0s), and deserialize it(converting 
////    //    that stream back to its usual form with all data intact), 
////    //and is typically used with to save data to the hard disk so it can be loaded again after the game is closed and started up again.
////    BinaryFormatter formatter = new BinaryFormatter();
////    try
////    {
////        //formatter.Serialize(fs, addresses);
////        //You can't access non-static members from a static method. (Note that Main() is static, which is a requirement of .Net).
////        //Just make siprimo and volteado static, by placing the static keyword in front of them. e.g.:
////        //static private long volteado(long a)
////        formatter.Serialize(fs, m_actionPlan);
////    }
////    catch (SerializationException e)
////    {
////        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
////        throw;
////    }
////    finally
////    {
////        fs.Close();
////    }
////}// void Serialize()


////void LoadActionPlan()
////{
////    // Declare the hashtable reference.
////    Hashtable addresses = null;

////    // Open the file containing the data that you want to deserialize.
////    FileStream fs = new FileStream("DictDataFile.dat", FileMode.Open);
////    try
////    {
////        BinaryFormatter formatter = new BinaryFormatter();

////        // Deserialize the hashtable from the file and 
////        // assign the reference to the local variable.
////        addresses = (Hashtable)formatter.Deserialize(fs);
////    }
////    catch (SerializationException e)
////    {
////        Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
////        throw;
////    }
////    finally
////    {
////        fs.Close();
////    }

////    // To prove that the table deserialized correctly, 
////    // display the key/value pairs.
////    foreach (DictionaryEntry de in addresses)
////    {
////        Console.WriteLine("{0} lives at {1}.", de.Key, de.Value);
////    }
////}//   void Deserialize()



////BinaryFormatter is used to serialize an object (meaning it converts it to one long stream of 1s and 0s), 
////and deserialize it(converting that stream back to its usual form with all data intact), and is typically used with 
////to save data to the hard disk so it can be loaded again after the game is closed and started up again.

////There's a good tutorial here that goes over using BinaryFormatter and FileStream to save and load data, but it's a bit long.

////As for simpler examples, these are bits of my own SaveManager class I ended up with after watching that tutorial:

//// public class SaveManager
////{
////    private SaveGlob saveGlob;    // the Dictionary used to save and load data to/from disk
////    protected string savePath;
////    public SaveManager()
////    {
////        this.savePath = Application.persistentDataPath + "/save.dat";
////        this.saveGlob = new SaveGlob();
////        this.loadDataFromDisk();
////    }
////    /**
////     * Saves the save data to the disk
////     */
////    public void saveDataToDisk()
////    {
////        BinaryFormatter bf = new BinaryFormatter();
////        FileStream file = File.Create(savePath);
////        bf.Serialize(file, saveGlob);
////        file.Close();
////    }

////    /**
////     * Loads the save data from the disk
////     */
////    public void loadDataFromDisk()
////    {
////        if (File.Exists(savePath))
////        {
////            BinaryFormatter bf = new BinaryFormatter();
////            FileStream file = File.Open(savePath, FileMode.Open);
////            this.saveGlob = (SaveGlob)bf.Deserialize(file);
////            file.Close();
////        }
////    }



////void Serialize()
////{
////    // Create a hashtable of values that will eventually be serialized.
////    //Hashtable addresses = new Hashtable();
////    //addresses.Add("Jeff", "123 Main Street, Redmond, WA 98052");
////    //addresses.Add("Fred", "987 Pine Road, Phila., PA 19116");
////    //addresses.Add("Mary", "PO Box 112233, Palo Alto, CA 94301");

////    // To serialize the hashtable and its key/value pairs,  
////    // you must first open a stream for writing. 
////    // In this case, use a file stream.
////    FileStream fs = new FileStream("DictDataFile.dat", FileMode.Create);

////    // Construct a BinaryFormatter and use it to serialize the data to the stream.
////    BinaryFormatter formatter = new BinaryFormatter();
////    try
////    {
////        //formatter.Serialize(fs, addresses);
////        //You can't access non-static members from a static method. (Note that Main() is static, which is a requirement of .Net).
////        //Just make siprimo and volteado static, by placing the static keyword in front of them. e.g.:
////        //static private long volteado(long a)
////        formatter.Serialize(fs, m_actionPlan);
////    }
////    catch (SerializationException e)
////    {
////        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
////        throw;
////    }
////    finally
////    {
////        fs.Close();
////    }
////}// void Serialize()


////void Deserialize()
////{
////    // Declare the hashtable reference.
////    Hashtable addresses = null;

////    // Open the file containing the data that you want to deserialize.
////    FileStream fs = new FileStream("DictDataFile.dat", FileMode.Open);
////    try
////    {
////        BinaryFormatter formatter = new BinaryFormatter();

////        // Deserialize the hashtable from the file and 
////        // assign the reference to the local variable.
////        addresses = (Hashtable)formatter.Deserialize(fs);
////    }
////    catch (SerializationException e)
////    {
////        Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
////        throw;
////    }
////    finally
////    {
////        fs.Close();
////    }

////    // To prove that the table deserialized correctly, 
////    // display the key/value pairs.
////    foreach (DictionaryEntry de in addresses)
////    {
////        Console.WriteLine("{0} lives at {1}.", de.Key, de.Value);
////    }
////}//   void Deserialize()

//http://gram.gs/gramlog/xml-serialization-and-deserialization-in-unity/


//     public interface IXmlSerializable
//{
//    XmlSchema GetSchema();
//    void ReadXml(XmlReader reader);
//    void WriteXml(XmlWriter writer);
//}

[XmlRoot("Dictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable

{
    #region IXmlSerializable Members
    public System.Xml.Schema.XmlSchema GetSchema()
    {
        return null;
    }
    public void ReadXml(System.Xml.XmlReader reader)
    {
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
        bool wasEmpty = reader.IsEmptyElement;
        reader.Read();
        if (wasEmpty)
            return;
        while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
        {
            reader.ReadStartElement("item");
            reader.ReadStartElement("key");
            TKey key = (TKey)keySerializer.Deserialize(reader);
            reader.ReadEndElement();
            reader.ReadStartElement("value");
            TValue value = (TValue)valueSerializer.Deserialize(reader);
            reader.ReadEndElement();
            this.Add(key, value);
            reader.ReadEndElement();
            reader.MoveToContent();
        }
        reader.ReadEndElement();
    }
    public void WriteXml(System.Xml.XmlWriter writer)
    {
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
        foreach (TKey key in this.Keys)
        {
            writer.WriteStartElement("item");
            writer.WriteStartElement("key");
            keySerializer.Serialize(writer, key);
            writer.WriteEndElement();
            writer.WriteStartElement("value");
            TValue value = this[key];
            valueSerializer.Serialize(writer, value);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
    #endregion
}
//https://stackoverflow.com/questions/9088227/using-getopenfilename-instead-of-openfiledialog
public class ActionPlanFileManager : MonoBehaviour


{
    string m_dictPath;
    string m_fileName;
    
    
    ActionPlanController m_actionPlanController; // the reference is obtained from ActionPLanController component attached to
                                                 //                                             // CommHub GameObject.

    // Define delegate instances
    public event Action<PointerEventData> onPointerEnter;


    public Dictionary<String, List<ActionPlanController.Action>> m_actionPlan; //// first defined in SimpleBoidsTreeOfVoice class

    ////public Dictionary<string, List<int>> myDict = new Dictionary<string, List<int>>();

  

    //string dictPath { get { return Application.dataPath + "/" + fileName; } }

    //string dictPath { get { return UnityEngine.Application.dataPath + "/" + fileName; } }

    void Start()
    {


        m_dictPath = UnityEngine.Application.dataPath + "/ActionPlans/";
        m_fileName = "actionPlan1.xml";
    //m_fileName = "actionPlan.xml";
    //// get it from ActionPLanController component attached to CommHub
    m_actionPlanController = this.gameObject.GetComponent<ActionPlanController>();
        // this.gameObject is CommHub object to which ActionPlanFileManager component is attached

        if (m_actionPlanController == null)
        {
            Debug.LogError("Add ActionPlanController component to CommHub gameObject");
            UnityEngine.Application.Quit();

        }

        m_actionPlan = m_actionPlanController.m_actionPlan;
    }


    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
    }


    public void SaveActionPlan() // zero argument EventHandler triggered by PointerClick event
    {
        //public static bool ShowSaveDialog( OnSuccess onSuccess, OnCancel onCancel,
        //bool folderMode = false, string initialPath = null,
        //
        //                                  string title = "Save", string saveButtonText = "Save" )
        // static bool ShowSaveDialog()

        bool saveFileCreated = FileBrowser.ShowSaveDialog( (path) => { this._SaveActionPlan(path); },
                                      () => { Debug.Log("Canceled"); },
                                       false, m_dictPath, "Save", "Save");

        if ( !saveFileCreated)
        {
            Debug.LogError("Save File Failed to be created!");
            return;

        }
    //    private static FileBrowser Instance
    //{
    //    get
    //    {
    //        if (m_instance == null)
    //        {
    //            m_instance = Instantiate(Resources.Load<GameObject>("SimpleFileBrowserCanvas")).GetComponent<FileBrowser>();
    //            DontDestroyOnLoad(m_instance.gameObject);
    //            m_instance.gameObject.SetActive(false);
    //        }

    //        return m_instance;
    //    }
    //}

    //FileBrowserCoRoutine.ShowSaveDialogCoroutine(
    //   (path) => { this._SaveActionPlan(path); },
    //    () => { Debug.Log("Canceled"); },
    //    m_dictPath + m_fileName);


}
    public void LoadActionPlan() // Zero Argument Event Handler triggered by PointerClick event
    {

        bool loadFileSelected = FileBrowser.ShowLoadDialog((path) => { this._LoadActionPlan(path); },
                                      () => { Debug.Log("Canceled"); },
                                       false, m_dictPath, "Select File", "Select");
        if ( !loadFileSelected)
        {
            Debug.LogError("Load File Failed to be Selected");
            return;
        }
        //FileBrowserCoRoutine.ShowLoadDialogCoroutine(
        //    (path) => { this._LoadActionPlan(path); },
        //     () => { Debug.Log("Canceled"); },
        //     m_dictPath + m_fileName);

    }




    public void _SaveActionPlan(string dictPath)
    {
      


        //StartCoroutine(ExecuteAfterTime(10000));


        FileStream fs = new FileStream(dictPath, FileMode.Create);


        TextWriter textWriter = new StreamWriter(fs);
        SerializableDictionary<string, List<ActionPlanController.Action>> serializableDict = new SerializableDictionary<string,
                                                             List<ActionPlanController.Action>>();
        // copy dictionary to serializable dictionary
        foreach (var kvp in m_actionPlan)
        {
            Debug.Log(kvp.Key + ":" + kvp.Value);
            serializableDict.Add(kvp.Key, kvp.Value);

        }

        XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string,
                                                     List<ActionPlanController.Action>>));

        serializer.Serialize(textWriter, serializableDict);
        textWriter.Close();



    }// _SaveActionPlan


    public void _LoadActionPlan(string dictPath)
    {


        FileStream fs = new FileStream(dictPath, FileMode.Open);

        //BinaryReader br = new BinaryReader(fs);



        XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string,
                                                   List<ActionPlanController.Action>>));

        // StreamReader: TextReader
        // public object Deserialize(XmlReader xmlReader, XmlDeserializationEvents events);
        //public object Deserialize(Stream stream);
        //public object Deserialize(TextReader textReader);

        TextReader textReader = new StreamReader(fs);
        SerializableDictionary<string, List<ActionPlanController.Action>> serializableDict
                 = (SerializableDictionary<string, List<ActionPlanController.Action>>)
                                                 serializer.Deserialize(textReader);
        textReader.Close();


        // public class Action
        //{
        //    public List<float> T;
        //    public float V;

        //}

        // copy serializable dictionary to  dictionary
        foreach (var kvp in serializableDict)
        {
            Debug.Log(" serializableDict:" + kvp.Key + ":");
            // kvp.Value is a List
            foreach (var value in kvp.Value)

            {
                Debug.Log("[" + value.T[0] + "," + value.T[1] + "]:" + value.V);
            }

        }



        // copy serializable dictionary to  dictionary
        foreach (var kvp in serializableDict)
        {
            m_actionPlan[kvp.Key] = kvp.Value;
        }

        // debug
        // copy serializable dictionary to  dictionary
        foreach (var kvp in m_actionPlan)
        {
            Debug.Log("m_actionPlan:" + kvp.Key + ":");

            foreach (var value in kvp.Value)
            // kvp.Value is a List              

            {
                Debug.Log("[" + value.T[0] + "," + value.T[1] + "]:" + value.V);

            }


        }


    } // _LoadActionPlan()




    //https://forum.unity.com/threads/finally-a-serializable-dictionary-for-unity-extracted-from-system-collections-generic.335797/
    // Unity doesn't know how to serialize generic dictionaries. 

    //https://www.codeproject.com/Questions/454134/Serialize-Dictionary-in-csharp
    void _SaveActionPlan(Dictionary<string, List<ActionPlanController.Action>> myDict)
    {
        // string dictPath = UnityEngine.Application.dataPath + "/ActionPlans/" + m_fileName;


        OpenFileName sfn = new OpenFileName();




        sfn.structSize = Marshal.SizeOf(sfn);
        sfn.filter = "All Files\0*.xml\0\0"; //"All Files\0*.*\0\0".
        // ofn.filter = "Log files\0*.log\0Batch files\0*.bat\0";
        sfn.file = new string(new char[256]);
        sfn.maxFile = sfn.file.Length;
        sfn.fileTitle = new string(new char[64]);
        sfn.maxFileTitle = sfn.fileTitle.Length;
        sfn.initialDir = UnityEngine.Application.dataPath + "/ActionPlan/";
        sfn.title = "Wrtie Action Plan File";
        sfn.defExt = "xml";
        sfn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        //OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_ALLOWMULTISELECT | OFN_NOCHANGEDIR

        //https://stackoverflow.com/questions/43573907/c-sharp-interop-returning-struct-from-unmanaged-code-with-out-parameter-gives

        //StartCoroutine(ExecuteAfterTime(10000));



        if (!DllTest.GetSaveFileName(sfn))
        {
            Debug.LogError("File Saving Dialog Failed");
            return;
        }

        //return;


        //// Open SaveFileDialog
        //SaveFileDialog save = new SaveFileDialog();
        //save.Filter = "ActionPlanFile(*.xml)|*.xml";
        //save.InitialDirectory = Directory.GetCurrentDirectory();
        //save.Title = "Save Action Plan";
        //if (save.ShowDialog() != DialogResult.OK)
        //    return;

        //m_fileName

        FileStream fs = new FileStream(sfn.file, FileMode.Create);
        //FileStream fs = new FileStream(m_fileName, FileMode.Create);


        TextWriter textWriter = new StreamWriter(fs);
        SerializableDictionary<string, List<ActionPlanController.Action>> serializableDict = new SerializableDictionary<string,
                                                             List<ActionPlanController.Action>>();
        // copy dictionary to serializable dictionary
        foreach (var kvp in myDict)
        {
            Debug.Log(kvp.Key + ":" + kvp.Value);
            serializableDict.Add(kvp.Key, kvp.Value);

        }

        XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string,
                                                     List<ActionPlanController.Action>>));

        serializer.Serialize(textWriter, serializableDict);
        textWriter.Close();


        //BinaryReader br = new BinaryReader(fs);


        ////var binaryFormatter = new BinaryFormatter();

        ////var fi = new System.IO.FileInfo(dictPath);

        ////try
        ////{
        ////    using (var binaryFile = fi.Create())
        ////    {
        ////        binaryFormatter.Serialize(binaryFile, myDict);
        ////        binaryFile.Flush();
        ////        binaryFile.Close();
        ////    }

        ////}

        ////catch (Exception ex)
        ////{
        ////    Debug.LogError(" Exception: " + ex.ToString());

        ////}

        ////Dictionary<String, List<ActionPlanController.Action>>
        //// b is this
        //SerializableDictionary<string, List<ActionPlanController.Action>> b = new SerializableDictionary<string,
        //                                                     List<ActionPlanController.Action>>();
        //// copy dictionary to serializable dictionary
        //foreach (var kvp in myDict)
        //{
        //    b.Add(kvp.Key, kvp.Value);

        //}




        ////List<string> stringList = new List<string>();
        //// stringList.Add("1");
        //// b.Add("One", stringList);

        //XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string,
        //                                             List<ActionPlanController.Action>>));
        //TextWriter textWriter = new StreamWriter(saveFileName);
        //serializer.Serialize(textWriter, b);
        //textWriter.Close();


        //FileStream file = File.Create(dictPath);

        //FileStream file = new FileStream(dictPath, FileMode.OpenOrCreate, FileAccess.Write);

        //BinaryFormatter bf = new BinaryFormatter();
        //// Serialize(Stream serializationStream, object graph);
        //try
        //{
        //    bf.Serialize(file, myDict);
        //}

        //catch (Exception ex)
        //{
        //    Debug.LogError(" Exception: " + ex.ToString()) ;

        //}
        //file.Close(); ;

    }// _SaveActionPlan


    void _LoadActionPlan(Dictionary<string, List<ActionPlanController.Action>> myDict)
    {
        //    // Open Dialog
        //    OpenFileDialog open = new OpenFileDialog();
        //    open.Filter = "ActionPlanFile(*.xml)|*.xml";
        //    open.InitialDirectory = Directory.GetCurrentDirectory();
        //    open.Title = "Open Action Plan";
        //    if (open.ShowDialog() != DialogResult.OK)
        //        return;

        OpenFileName ofn = new OpenFileName();

        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = "All Files\0*.xml\0\0";
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = UnityEngine.Application.dataPath + "/ActionPlan/";
        ofn.title = "Open Action Plan File";
        ofn.defExt = "xml";
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        //OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_ALLOWMULTISELECT | OFN_NOCHANGEDIR

        if (!DllTest.GetOpenFileName(ofn))
        {
            Debug.LogError("File Opening Dialog Failed");
            return;
        }

        FileStream fs = new FileStream(ofn.file, FileMode.Open);

        //FileStream fs = new FileStream(m_fileName, FileMode.Open);
        //BinaryReader br = new BinaryReader(fs);



        XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string,
                                                   List<ActionPlanController.Action>>));

        // StreamReader: TextReader
        // public object Deserialize(XmlReader xmlReader, XmlDeserializationEvents events);
        //public object Deserialize(Stream stream);
        //public object Deserialize(TextReader textReader);

        TextReader textReader = new StreamReader(fs);
        SerializableDictionary<string, List<ActionPlanController.Action>> serializableDict
                 = (SerializableDictionary<string, List<ActionPlanController.Action>>)
                                                 serializer.Deserialize(textReader);
        textReader.Close();


        // public class Action
        //{
        //    public List<float> T;
        //    public float V;

        //}

        // copy serializable dictionary to  dictionary
        foreach (var kvp in serializableDict)
        {
            Debug.Log(" serializableDict:" + kvp.Key + ":");
            // kvp.Value is a List
            foreach (var value in kvp.Value)

            {
                Debug.Log("[" + value.T[0] + "," + value.T[1] + "]:" + value.V);
            }

        }



        // copy serializable dictionary to  dictionary
        foreach (var kvp in serializableDict)
        {
            myDict[kvp.Key] = kvp.Value;
        }

        // debug
        // copy serializable dictionary to  dictionary
        foreach (var kvp in myDict)
        {
            Debug.Log("myDict:" + kvp.Key + ":");

            foreach (var value in kvp.Value)
            // kvp.Value is a List              

            {
                Debug.Log("[" + value.T[0] + "," + value.T[1] + "]:" + value.V);

            }


        }


        //OpenFileName ofn = new OpenFileName();
        //ofn.structSize = Marshal.SizeOf(ofn);
        //ofn.filter = "All Files\0*.xml\0\0";
        //ofn.file = new string(new char[256]);
        //ofn.maxFile = ofn.file.Length;
        //ofn.fileTitle = new string(new char[64]);
        //ofn.maxFileTitle = ofn.fileTitle.Length;
        //ofn.initialDir = UnityEngine.Application.dataPath;
        //ofn.title = "Open Action Plan File";
        ////ofn.defExt = "JPG";
        //ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        //OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR

        //if (DllTest.GetOpenFileName(ofn))
        //{

        //    //  FileStream(string path, FileMode mode, FileAccess access);
        //    FileStream fileStream = new FileStream(ofn.file, FileMode.Open, FileAccess.Read);

        //    //https://m.blog.naver.com/PostView.nhn?blogId=y2kgr&logNo=80208479466&categoryNo=13&proxyReferer=&proxyReferer=https%3A%2F%2Fwww.google.com%2F

        //    // //FileStream  fs = new FileStream(openDialog.FileName, FileMode.Open);
        //    // //reader = new StreamReader( fs, mode)

        //    // // https://docs.microsoft.com/ko-kr/dotnet/framework/winforms/controls/how-to-open-files-using-the-openfiledialog-component

        //    // // using (var reader = new StreamReader( openDialog.FileName)  )



        //    // //https://stackoverflow.com/questions/37094671/save-a-dictionary-to-a-binary-file


        //    // var binaryFormatter = new BinaryFormatter();

        //    // var fi = new System.IO.FileInfo(ofn.file);


        //    //// Dictionary<int, string> readBack;
        //    // using (var binaryFile = fi.OpenRead())
        //    // {
        //    //     myDict = (Dictionary<string, List<ActionPlanController.Action>>)
        //    //                           binaryFormatter.Deserialize(binaryFile);
        //    // }


        //    // b is this
        //   // SerializableDictionary<string, List<ActionPlanController.Action>>
        //   //              b = new SerializableDictionary<string,
        //    //                            List<ActionPlanController.Action>>();

        //    //b = (SerializableDictionary<string, List<ActionPlanController.Action>>)myDict;

        //    //List<string> stringList = new List<string>();
        //    // stringList.Add("1");
        //    // b.Add("One", stringList);

        //    XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string,
        //                                                 List<ActionPlanController.Action>>));
        //    TextReader textReader = new StreamReader(fileStream);
        //    SerializableDictionary<string, List<ActionPlanController.Action>> b 
        //             = (SerializableDictionary<string, List<ActionPlanController.Action>>)
        //                                             serializer.Deserialize(textReader);
        //    textReader.Close();

        //    // copy serializable dictionary to  dictionary
        //    foreach (var kvp in b)
        //    {
        //        myDict.Add(kvp.Key, kvp.Value);

        //    }

        //// debug
        //    foreach (var kvp in myDict)
        //        Debug.Log(kvp.Key + ":" + kvp.Value);
        //}//if
    } // _LoadActionPlan()

} // Mono class
    //https://stackoverflow.com/questions/35021125/how-to-write-to-a-file-using-streamwriter


    //    using (var fileStream = new FileStream(String.Format("Person{0}.txt", Id), FileMode.OpenOrCreate))
    //using (var streamWriter = new StreamWriter(fileStream))
    //{
    //    streamWriter.WriteLine("ID: " + Id);
    //    streamWriter.WriteLine("DOB: " + dOB);
    //    streamWriter.WriteLine("Name: " + name);
    //    streamWriter.WriteLine("Age: " + age);
    //}

    //    base.SaveData();
    //using (var fileStream = new FileStream(String.Format("Person{0}.txt", Id), FileMode.Append))
    //using (var streamWriter = new StreamWriter(fileStream))
    //{
    //    streamWriter.WriteLine("Cod: " + cod);
    //    streamWriter.WriteLine("Credits: " + credits);
    //}




