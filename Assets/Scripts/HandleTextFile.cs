using UnityEngine;
using UnityEditor;
using System.IO;

public class HandleTextFile
{
   
    static void WriteString(string fileName)
    {
        string path = "Assets/Resources/DebugFile/" + fileName;

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("Test");
        writer.Close();

        //Re-import the file to update the reference in the editor
       // AssetDatabase.ImportAsset(path);
        //TextAsset asset = (TextAsset) Resources.Load("test");

        //Print the text from the file
       // Debug.Log(asset.text);
    }

    static void ReadString()
    {
        string path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }

}