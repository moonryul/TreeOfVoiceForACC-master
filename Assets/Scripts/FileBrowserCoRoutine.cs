using System;
using System.Runtime.InteropServices;

using UnityEngine;
using System.Collections;

using System.Xml.Serialization;


//using System.Windows.Forms;
using System.IO;

using System.Collections.Generic;

using UnityEngine.EventSystems;

using System.Runtime.Serialization.Formatters.Binary; //BinaryFormatter Class 

using SimpleFileBrowser;

public class FileBrowserCoRoutine : MonoBehaviour
{
    string m_dictPath = UnityEngine.Application.dataPath + "/ActionPlans/";

    ActionPlanController m_actionPlanController; // the reference is obtained from ActionPLanController component attached to
                                                 //                                             // CommHub GameObject.

    public Dictionary<String, List<ActionPlanController.Action>> m_actionPlan; //// first defined in SimpleBoidsTreeOfVoice class

     // Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
    // Warning: FileBrowser can only show 1 dialog at a time

    void Start()
    {
     
        //// get it from ActionPLanController component attached to CommHub
        m_actionPlanController = this.gameObject.GetComponent<ActionPlanController>();

        // this.gameObject is CommHub object to which ActionPlanFileManager component is attached

        if (m_actionPlanController == null)
        {
            Debug.LogError("Add ActionPlanController component to CommHub gameObject");
            UnityEngine.Application.Quit();

        }

        m_actionPlan = m_actionPlanController.m_actionPlan;


        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".txt", ".pdf"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter(".jpg");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        // Show a save file dialog 
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Initial path: "C:\", Title: "Save As", submit button text: "Save"

        string initialPath = "C:\\";
        

        //    _LoadActionPlan(m_actionPlan);
        //
        //FileBrowser.ShowSaveDialog( (path) => { this._SaveActionPlan(path); },  null, false, m_dictPath, "Save As", "Save" );

        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Initial path: default (Documents), Title: "Select Folder", submit button text: "Select"

        //FileBrowser.ShowLoadDialog((path) => { Debug.Log("Selected: " + path); },
        //                               () => { Debug.Log("Canceled"); },
        //                               true, null, "Select Folder", "Select");

        //FileBrowser.ShowLoadDialog((path) => { this._LoadActionPlan(path); },
        //                               () => { Debug.Log("Canceled"); },
        //                               true, null, "Select Folder", "Select");


        //// Coroutine example
        //StartCoroutine(ShowLoadDialogCoroutine());
    } // Start()

    static public IEnumerator ShowLoadDialogCoroutine(FileBrowser.OnSuccess onSuccess, FileBrowser.OnCancel onCancel, string initialPath)
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(onSuccess, onCancel, false, initialPath, "Load File", "Load");

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);

        if (FileBrowser.Success)
        {
            // If a file was chosen, read its bytes via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result);
        }
    } // ShowLoadDialogCoroutine()


    static public IEnumerator ShowSaveDialogCoroutine(FileBrowser.OnSuccess onSuccess, FileBrowser.OnCancel onCancel, string initialPath)
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForSaveDialog(onSuccess, onCancel, false, initialPath, "Load File", "Load");

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);

        if (FileBrowser.Success)
        {
            // If a file was chosen, read its bytes via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result);
        }
    } // ShowSaveDialogCoroutine()



    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
    }




} //  class FileBrowserTest : MonoBehaviour
