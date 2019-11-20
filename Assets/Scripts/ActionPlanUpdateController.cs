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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary; //BinaryFormatter Class 


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

public class ActionPlanUpdateController: MonoBehaviour


{
    public ActionPlanController m_actionPlanController; // set in the inspector


    public List<GameObject>[] m_inputFieldContainer, m_textContainer, m_placeholderContainer;    // set in the inspector

    public Hashtable m_inputFieldHashTable;

   // public Dictionary<String, List<SimpleBoidsTreeOfVoice.Action>> m_actionPlan; //// first defined in SimpleBoidsTreeOfVoice class


    

    //public string fileName = "dict.bin";

    //string dictPath { get { return Application.dataPath + "/" + fileName; } }

    //string dictPath { get { return UnityEngine.Application.dataPath + "/" + fileName; } }

    void Start()
    {
        // Check if global components are defined
        //if (m_boidsController == null)
        //{
        //    Debug.LogError("The global Variable m_boidsController is not  defined in Inspector");

        //}

        m_actionPlanController = this.gameObject.GetComponent<ActionPlanController>();

        if (m_actionPlanController == null)
        {
            Debug.LogError("The ActionPlanController component should be added to CommHub");

        }
        //m_actionPlan = m_boidsController.m_actionPlan;

        m_inputFieldContainer = m_actionPlanController.m_inputFieldContainer;
        m_textContainer = m_actionPlanController.m_textContainer;
         m_placeholderContainer =  m_actionPlanController.m_placeholderContainer;
        m_inputFieldHashTable = m_actionPlanController.m_inputFieldHashTable;
    }
    public void ChangeActionTime(PointerEventData eventData) // added to onDrag
    {
      
       //

    }


    public void MergeOrSplitActionField(PointerEventData eventData) // added to onDrag
    {
        //  // if the drag is meant to merge or split  fields
       //
   }
   
  
    //Checks if there is anything entered into the input field.
    //http://rapapa.net/?p=2936
    //    


    //    I have tried the following but this doesn't work:
    //     myInput.onEndEdit.AddListener(delegate{StopInput(myInput);
    //});

    //void StopInput(InputField _input)
    //{
    //    print(_input.text);
    //    _input.DeactivateInputField();
    //}

    //the above also causes the following error when clicking another object while entering text:
    //Attempting to select while already selecting an object

    //https://forum.unity.com/threads/unity-ui-4-6-inputfield-bug-or-behavoir.289210/

    //    The issue is:

    //Once an input field has been clicked on, and text entered.If it is then submitted by pressing enter or 
    //    by clicking on another game object,
    //    if I then move my character controller around a bit the input field will keep picking up the WSDA keys
    //    or anything else I press.

    //Also, if there is more than one input field, the focus will randomly switch from one input field 
    //    to another after I press submit or move around a bit.


    //    Enter doesn't make the input field lose focus. Have you considered just disabling the input field?
    //        If you're using WASD for movement I'm going to guess the input field is probably not on the screen 
    // or being used, yes?     You can just do the InputField.enabled = false 
    //and then reenable it later if you want input to go in.

    //Basically what is happening is you aren't switching to some other control so the input field is still t
    //            aking in put. If you have multiple input fields it might be useful to have more than one submit
    //button per field or one big submit button that just turns them all off when you're done.

    //pressing enter just stops the editing it never deselects the graphic.
    //    If you want to deselect it, listen for the onsubmit event and then
    //    call EventSystem.current.SetSelectedGameObject(null);

    //Phil, since this is timely, I thought I should ask here. I have a similar problem.
    //    When I select the text of an input field via script, that input field is then selected 
    //    until a player deselects it by clicking elsewhere. Is that the correct default behavior?

    //well its the same behaviour as all other components its just visible with input field.pressing a button selects it,
    //    you will only get the deselect when you navigate away or press something else. 


    //_inputField.onValueChanged.AddListener(
    //    val =>
    //    {
    //        //if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
    //        // {
    //        //      Debug.Log("End edit on enter");
    //        OnValueChanged(m_actionPlan, dictItem.Key, indexToTimedAction, _inputField);
    //        //  }
    //    }
    // );

    // _inputField.onEndEdit.AddListener(delegate { StopInput(_inputField); });

    //  InputField:
    //     public class SubmitEvent : UnityEvent<string>
    //     public class OnChangeEvent: UnityEvent<string>   
    //     SubmitEvent onEndEdit;
    //     OnChangeEvent onValueChanged;

    // Be careful about the for loop variable j: https://answers.unity.com/questions/912202/buttononclicaddlistenermethodobject-wrong-object-s.html




    //// Delegates: https://forum.unity.com/threads/c-delegates-i-love-you.150321/
    //// The original word for delegate is "function pointer." The idea is you have have a variable
    //that holds a function. Simple idea, been in use for 30+ years. 
    //    But C# renamed them "delegates" just to make them more confusing (function pointer is what it is.
    //    delegate is what it does, sort of.) 

    //In other words, a delegate declaration describes a method signature (a method’s return type and its argument types),
    //while an instance of that delegate can be assigned, referenced and invoke a method matching that signature. 
    //                                                                                                                    
    //You have probably encountered Action, Func and EventHandler before, they are all examples of delegates. 
    //If you have ever used functions like Array.Find or List.ForEach, then you have also used delegates.


    //https://docs.unity3d.com/ScriptReference/RectTransformUtility.ScreenPointToLocalPointInRectangle.html
    //public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint); 

    // When ScreenPointToLocalPointInRectangle is used from within an event handler that provides a PointerEventData object, 
    //the correct camera can be obtained by using PointerEventData.enterEventData (for hover functionality) 
    //or PointerEventData.pressEventCamera(for click functionality).

    //public GameObject pointerPress { get; set; }    public Camera pressEventCamera { get; }
    //public InputButton button { get; set; }    public bool dragging { get; set; }
    //public Vector2 scrollDelta { get; set; }    public int clickCount { get; set; }

    //[Obsolete("Use either pointerCurrentRaycast.worldPosition or pointerPressRaycast.worldPosition")]
    //public Vector3 worldPosition { get; set; }    public Vector2 pressPosition { get; set; }
    //public Vector2 delta { get; set; }    public Vector2 position { get; set; }
    //public int pointerId { get; set; }    public RaycastResult pointerPressRaycast { get; set; }
    //public RaycastResult pointerCurrentRaycast { get; set; }    public GameObject pointerDrag { get; set; }
    //public GameObject lastPress { get; }    public GameObject pointerEnter { get; set; }
    //Vector3 worldPosition { get; set; }
    //public enum InputButton
    //{
    //    Left = 0,
    //    Right = 1,
    //    Middle = 2
    //}

    // inherited member: selectedObject

    //https://forum.unity.com/threads/pointereventdata-has-no-object-information.427843/

    // position: The position value is window-based. This is zero-zero bottom left.
    //PointerEventData.pressPosition  returns the same value as PointerEventData.position for a click.
    //However, for a click and drag, this returns the coordinate where the pointer was initially clicked
    //while PointerEventData.position always returns the coordinates of the pointer’s current position.

    //https://stackoverflow.com/questions/49660365/reference-of-eventdata-pointercurrentraycast-gameobject

    //var colors = GetComponent<Button>().colors;
    //colors.normalColor = Color.red;
    //    GetComponent<Button>().colors = colors;
    //GetComponent<Button>().colors.highlightedColor = Color.blue;
    //GetComponent<Button>().colors.normalColor = Color.cyan;
    //GetComponent<Button>().colors.pressedColor = Color.green;

    List<GameObject> m_selectedFields = new List<GameObject>(); // Add(), Clear()


    public void OnPointerClick(PointerEventData eventData)
    {

        Debug.Log("Pointer Cliks");

        // check which inputField is clicked. Search over  m_inputFieldContainer[i][j]
        // public virtual void Add(object key, object value);

        if (eventData.lastPress == null) return;

        Vector2 index = (Vector2)m_inputFieldHashTable[eventData.lastPress];
        RectTransform fieldRect = m_inputFieldContainer[(int)index.x][(int)index.y].GetComponent<RectTransform>();

        bool intersected = RectTransformUtility.ScreenPointToLocalPointInRectangle
                      (fieldRect, eventData.pressPosition, eventData.pressEventCamera, out Vector2 localPoint);
        //localPoint: Point in local space of the rect transform.


        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Left click");

            if (intersected)
            {
                // Check if the clicked field is clicked at the left or right edge of the field or not 
                // In the first case, the two fields adjcent to the edge will be merged
                // In the second case, the field will be split
                return;
            }
            else return;


        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Debug.Log("Middle click");
            return;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Right click");
            // Check if the clicked field is clicked at the left or right edge of the field
            // If so, the clicked edge will be moved to the left or right according to the drag delta

            if (intersected)
            {


                return;
            }
            else return;


        }



    } //   public void OnPointerClick(PointerEventData eventData)

    //https://stackoverflow.com/questions/36048106/horrors-of-onpointerdown-versus-onbegindrag-in-unity3d

    //    I want to start by saying that Input and Touches are not crappy.They are still usefull and were the best way to check for touch on mobile devices 
    //        before OnPointerDown and OnBeginDrag came along.OnMouseDown() you can call crappy because it was not optimized for mobile.
    //        For a beginner who just started to learn Unity, Input and Touches are their options.
    //  As for your question, OnPointerDown and OnBeginDrag are NOT the-same.Although they almost do the-same thing but they were implemented 
    //            to perform in different ways. Below I will describe most of these:
    //OnPointerDown: Called when there is press/touch on the screen (when there is a click or finger is pressed down on touch screen)
    //OnPointerUp: Called when press/touch is released(when click is released or finger is removed from the touch screen)
    //OnBeginDrag: Called once before a drag is started(when the finger/mouse is moved for the first time while down)
    //OnDrag : Repeatedly called when user is dragging on the screen(when the finger/mouse is moving on the touch screen)
    //OnEndDrag: Called when drag stops(when the finger/mouse is no longer moving on the touch screen). 
    //OnPointerDown versus OnBeginDrag and OnEndDrag
    //OnPointerUp will NOT be called if OnPointerDown has not been called.OnEndDrag will NOT be called if OnBeginDrag has not been called.
    //        Its like the curly braces in C++,C#, you open it '{' and you close it '}'. 

    //THE DIFFERENCE: OnPointerDown will be called once and immediately when finger/mouse is on the touch screen.Nothing 
    //    else will happen until there is a mouse movement or 
    //    the finger moves on the screen then OnBeginDrag will be called once followed by OnDrag.

    // The difference is that OnBeginDrag doesn't get called until the touch/mouse has moved a certain minimum distance, the drag threshold.
    //You can set the drag threshold on the Event System componen

    //https://stackoverflow.com/questions/36048106/horrors-of-onpointerdown-versus-onbegindrag-in-unity3d

    //https://stackoverflow.com/questions/37473802/unity3d-ui-calculation-for-position-dragging-an-item

    int m_index2RightField;
    int m_index2LeftField;
    int m_index2ThisField;

    public void OnBeginDrag(PointerEventData eventData)
    {
        //this.transform.position = eventData.position;
        // check which inputField is clicked. Search over  m_inputFieldContainer[i][j]
        // public virtual void Add(object key, object value);
        //pointerDrag, delta

        Debug.Log("Dragging starts");
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.lastPress == null) return;

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        else
        {


            Vector2 index = (Vector2)m_inputFieldHashTable[eventData.pointerDrag];
            RectTransform fieldRect = m_inputFieldContainer[(int)index.x][(int)index.y].GetComponent<RectTransform>();

            bool intersected = RectTransformUtility.ScreenPointToLocalPointInRectangle
                          (fieldRect, eventData.pressPosition, eventData.pressEventCamera, out Vector2 localPoint);
            //localPoint: Point in local space of the rect transform.


            m_index2RightField = (int)index.y + 1;
            m_index2LeftField = (int)index.y - 1;
            m_index2ThisField = (int)index.y;

            if (intersected)
            {
                if (eventData.delta.x > 0)
                {
                    // extend this field to the right, while shrinking the field to the right by the same amount

                    m_inputFieldContainer[(int)index.x][m_index2ThisField].GetComponent<RectTransform>().sizeDelta
                                      += new Vector2(eventData.delta.x, 0);

                    m_inputFieldContainer[(int)index.x][m_index2RightField].GetComponent<RectTransform>().anchoredPosition
                                     += new Vector2(eventData.delta.x, 0);
                    m_inputFieldContainer[(int)index.x][m_index2RightField].GetComponent<RectTransform>().sizeDelta
                                    -= new Vector2(eventData.delta.x, 0);
                }
                else
                {
                    // extend this field to the left while shrinking the field to the left by the same amount

                    m_inputFieldContainer[(int)index.x][m_index2ThisField].GetComponent<RectTransform>().sizeDelta
                                      += new Vector2(eventData.delta.x, 0);

                    m_inputFieldContainer[(int)index.x][m_index2ThisField].GetComponent<RectTransform>().anchoredPosition
                                     -= new Vector2(eventData.delta.x, 0);
                    m_inputFieldContainer[(int)index.x][m_index2LeftField].GetComponent<RectTransform>().sizeDelta
                                    -= new Vector2(eventData.delta.x, 0);

                }
            }

        } // else / RightButton?

        //transform.position += (Vector3)eventData.delta;
        //https://stackoverflow.com/questions/37473802/unity3d-ui-calculation-for-position-dragging-an-item
        //https://www.reddit.com/r/Unity3D/comments/3eo2ms/help_how_to_translate_eventcurrentdelta_to_world/

        //Vector3 guiDelta = new Vector3(e.delta.x, e.delta.y, 0f);
        //Vector3 screenDelta = GUIUtility.GUIToScreenPoint(guiDelta);
        //Ray worldRay = gameCamera.ScreenPointToRay(screenDelta);
        //// z-offset is not all that important if your camera is orthographic
        //Vector3 worldDelta = worldRay.GetPoint(zOffsetInRay);

        //object.position += worldDelta;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging ended");

    }

    //•IPointerDownHandler - OnPointerDown - Called when a pointer is pressed on the object
    //•IPointerUpHandler - OnPointerUp - Called when a pointer is released(called on the GameObject that the pointer is clicking)
    //•IPointerClickHandler - OnPointerClick - Called when a pointer is pressed and released on the same object
    //•IInitializePotentialDragHandler - OnInitializePotentialDrag - Called when a drag target is found, can be used to initialise values
    //•IBeginDragHandler - OnBeginDrag - Called on the drag object when dragging is about to begin
    //•IDragHandler - OnDrag - Called on the drag object when a drag is happening
    //•IEndDragHandler - OnEndDrag - Called on the drag object when a drag finishes

    void OnValueInput(Dictionary<string, List<ActionPlanController.Action>> actionPlan,
                    string key, int indexToTimedAction, InputField inputField)

    {

        //if (inputField.isFocused == false)
        //{
        //    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
        //    inputField.OnPointerClick(new PointerEventData(EventSystem.current));
        //}

        Debug.Log("On EndEdit:");

        Debug.Log("input field placeholder text:");
        Debug.Log(((Text)inputField.placeholder).text);

        Debug.Log("input field.text:");
        Debug.Log(inputField.text);


        Debug.Log("input field textComponent.text:");
        Debug.Log(inputField.textComponent.text);


        Debug.Log("input field caret position:");
        Debug.Log(inputField.caretPosition);



        //    You can use float.Parse(string s) to convert to a float or if you don't know if the string is definitely a number
        //        then you can use float.TryParse( string s, out float result )
        //        which returns true if it can convert. Not sure if is works if there is a $ at the end but i think it should

        //string phrase = "The quick brown fox jumps over the lazy dog.";
        string word = inputField.text.ToString();
        float number;



        bool result = float.TryParse(word, out number);

        if (result) //  is number
        {
            actionPlan[key][indexToTimedAction].V = number; // timedAction was  of type Action which is a struct. So could not  modify its fields
                                                            // Solved by changing Action into class

            return;

        }


        // otherwise, re-input
        Debug.LogError("The input should be a number");

        return;
    }// void OnValueInput


    //https://forum.unity.com/threads/cant-change-caret-position.431105/

    void OnValueChanged(Dictionary<string, List<ActionPlanController.Action>> actionPlan,
                    string key, int indexToTimedAction, InputField inputField)

    {

        //if (inputField.isFocused == false)
        //{
        //    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
        //    inputField.OnPointerClick(new PointerEventData(EventSystem.current));
        //}

        Debug.Log("On Value Changed:");

        //GetComponent<Text>().text
        //gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "Something";

        //Graphic graphic = gameObject.GetComponent<InputField>().placeholder;
        //((Text)graphic).text = "Hello";


        Debug.Log("input field placeholder text:");
        Debug.Log(((Text)inputField.placeholder).text);

        Debug.Log("input field.text:");
        Debug.Log(inputField.text);

        Debug.Log("input field textComponent.text:");
        Debug.Log(inputField.textComponent.text);


        Debug.Log("input field caret position:");
        Debug.Log(inputField.caretPosition);



        //    You can use float.Parse(string s) to convert to a float or if you don't know if the string is definitely a number
        //        then you can use float.TryParse( string s, out float result )
        //        which returns true if it can convert. Not sure if is works if there is a $ at the end but i think it should

        //string phrase = "The quick brown fox jumps over the lazy dog.";
        string word = inputField.text.ToString();
        float number;



        bool result = float.TryParse(word, out number);

        if (result) //  is number
        {
            actionPlan[key][indexToTimedAction].V = number; // timedAction was  of type Action which is a struct. So could not  modify its fields
                                                            // Solved by changing Action into class

            return;

        }


        // otherwise, re-input
        Debug.LogError("The input should be a number");

        return;
    }// void OnValueChanged



} //public class SaveMyDic : BaseBehaviour
