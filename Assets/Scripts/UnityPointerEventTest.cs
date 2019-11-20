using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // has InputField
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System; // String class
using System.Linq; // To use Dictionary.ElementAt(i)
//using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary; //BinaryFormatter Class 
using System.IO;



public class UnityPointerEventTest : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, 
                IBeginDragHandler, IDragHandler, IEndDragHandler,
                IPointerEnterHandler,  IScrollHandler, IPointerExitHandler //,IDeselectHandler
{

    public SimpleBoidsTreeOfVoice _boids;

    public float m_AnimationCycle = 390;

    public int m_contentWidth = 10000;  // the size of the content will be set accoring to the size of the action table
    public int m_contentHeight= 2000;

    public int m_scrollRectWidth;
    public int m_scrollRectHeight;

    public int  m_canvasWidth, m_canvasHeight; // the canvas size is set to the size of the game view screen automatically
                                         // The actuall scroll rect size is set to the size of the canvas

    public Dictionary<String, List<ActionPlanController.Action>> m_actionPlan; //// first defined in SimpleBoidsTreeOfVoice class


    int m_lastScreenWidth;
    int m_lastScreenHeight;
    bool stay = true;

 
    public MyScrollRect m_myScrollRect;  // set in the inspector

    public GameObject m_canvasObj, m_scrollViewObj, m_viewportObj;
    public GameObject m_contentObj, m_contentTitleObj, m_contentTimeLineObj, m_contentKeysObj, m_contentValuesObj;
    public GameObject m_scrollbarHorizontalObj, m_scrollbarVerticalObj;

    public GameObject m_contentTimeLineClipRectObj, m_contentKeysClipRectObj, m_contentValuesClipRectObj;

    CanvasRenderer m_viewportCanvasRenderer;

    Canvas m_canvas;
   
    RectTransform m_canvasRectTransform; 
    float m_distanceToCamera;

    // Raycasting
    private PointerEventData m_pointerData;
    private GameObject m_RootElementUI;


    GameObject[] m_timeTopBarContainer, m_horizontalLines, m_verticalLines;

    float m_timeScaleFactor; // the scale factor for converting the time value to the length in pixels
    List<GameObject>[] m_inputFieldContainer, m_textContainer, m_placeholderContainer; // gameobject which has InputField as a component

    Hashtable m_inputFieldHashTable;

    GameObject[] m_nameContainer;

    List<Text>[] m_text;


    List<InputField> [] m_inputField; // inputField is a component

    RectTransform m_inputFieldRectTransform; // RectTransform inheirts Transform properties, e.g. localPosition, localScale
    
    RenderMode m_guiMode;

    Camera m_guiCamera;

    Font ArialFont;
    float m_aspectOfArial = 0.52f;

    //https://stackoverflow.com/questions/52426526/in-unity-how-to-set-text-height-to-make-sure-at-least-show-one-line

    //Point sizes are defined as 1/72 of an inch. That is, a 72-point font is approximately 1 inch from the lowest descent to the highest ascent. 
    //So the maximum height of a glyph in a 72pt font is about 1 inch.
    //
    // Point sizes are defined as 1/72 of an inch.That is, a 72-point font is approximately 1 inch from the lowest descent to the highest ascent.
    //So the maximum height of a glyph in a 72pt font is about 1 inch. 
    //Apple's iphone tech specs page claims that the iPhone currently has a resolution of 163 pixels per inch. 
    // So 72 points is 163 pixels, or about 2.2639 pixels per point.

    //Aspect ratio of characters: https://www.lifewire.com/aspect-ratio-table-common-fonts-3467385

    //https://stackoverflow.com/questions/55594268/what-unit-is-font-measured-in

    // font size and the line height:
    //Your text height needs to be Math.Ceil(font_size* 1.35) in size...but that multiplier depends on the specific font you're using!
    //    Unity's default Arial font has a multiplier of 12% (1.12) 
    //The easiest way to find this multiplier is to set the font size to 100 and then find the height needed for it to display. 
    //Then knowing that ratio, you'll be able to correctly calculate the height needed for a given font size.
    
    List<ActionPlanController.Action> m_timedActions;

    float m_currentLocalXPosition;
    float m_currentLocalYPosition;

    float m_fieldHeight;
    float m_paramSize; // the max character number of the parameters

    float m_fontSize, m_lineHeight;

    Text m_textComponent;

    float m_paramTextWidth, m_paramTextHeight, m_timeTextWidth, m_timeTextHeight, m_valueTextWidth, m_valueTextHeight;

    float m_timeIntervalsCount, m_timeInterval;
    
    TextGenerator m_textGen;
    TextGenerationSettings m_generationSettings;


    // Finally use this:
    //https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.formatters.binary.binaryformatter?view=netframework-4.8
    // Similar one:  https://stackoverflow.com/questions/36852213/how-to-serialize-and-save-a-gameobject-in-unity
    // public Dictionary<string, List<int>> myDict = new Dictionary<string, List<int>>();

    void Serialize()
    {
        // Create a hashtable of values that will eventually be serialized.
        //Hashtable addresses = new Hashtable();
        //addresses.Add("Jeff", "123 Main Street, Redmond, WA 98052");
        //addresses.Add("Fred", "987 Pine Road, Phila., PA 19116");
        //addresses.Add("Mary", "PO Box 112233, Palo Alto, CA 94301");

        // To serialize the hashtable and its key/value pairs,  
        // you must first open a stream for writing. 
        // In this case, use a file stream.
        FileStream fs = new FileStream("DictDataFile.dat", FileMode.Create);

        // Construct a BinaryFormatter and use it to serialize the data to the stream.
        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            //formatter.Serialize(fs, addresses);
            //You can't access non-static members from a static method. (Note that Main() is static, which is a requirement of .Net).
            //Just make siprimo and volteado static, by placing the static keyword in front of them. e.g.:
            //static private long volteado(long a)
            formatter.Serialize(fs, m_actionPlan);
        }
        catch (SerializationException e)
        {
            Console.WriteLine("Failed to serialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }
    }// void Serialize()


    void Deserialize()
    {
        // Declare the hashtable reference.
        Hashtable addresses = null;

        // Open the file containing the data that you want to deserialize.
        FileStream fs = new FileStream("DictDataFile.dat", FileMode.Open);
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();

            // Deserialize the hashtable from the file and 
            // assign the reference to the local variable.
            addresses = (Hashtable)formatter.Deserialize(fs);
        }
        catch (SerializationException e)
        {
            Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }

        // To prove that the table deserialized correctly, 
        // display the key/value pairs.
        foreach (DictionaryEntry de in addresses)
        {
            Console.WriteLine("{0} lives at {1}.", de.Key, de.Value);
        }
    }//   void Deserialize()


    //void OnGUI()
    //{
    //  //The Label shows the current Rect settings on the screen (GUI => origin = left top)
    //    GUI.Label(new Rect(20, 20, 500, 80), "Rect : " + m_canvasRectTransform.rect);
    //}


    // Use Awake() to initialize field variables.
    public void Awake()
    {

    }
    public void OnScroll(PointerEventData eventData)
    {
        Debug.Log("Scrolling");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer Enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer Exit");
    }
   


    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Pointer Up");
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        Debug.Log("Pointer Cliks");

        // check which inputField is clicked. Search over  m_inputFieldContainer[i][j]
        // public virtual void Add(object key, object value);

        if (eventData.lastPress == null) return;


        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Left click");
            
          

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

           


        }



    } //   public void OnPointerClick(PointerEventData eventData)

   
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


          

        } // else / RightButton?

       

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging ended");

    }

    

    void OnValueInput(Dictionary<string, List<ActionPlanController.Action> > actionPlan, 
                    string key, int indexToTimedAction, InputField inputField )
      
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
   
            if (result ) //  is number
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
        Debug.Log( ( (Text)inputField.placeholder ).text );
            
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


void OnDestroy()
    {
        stay = false;
    }

    //https://mrbinggrae.tistory.com/86
    //This means that whatever the resolution, the Slider will always be drawn exactly with the same dimensions 
    //    from the Anchor point to the pivot point of the UI GameObject(by default, at the center), no matter how the screen is resized
   

void Update()
    {
        //    if (inputField.isFocused == false)
        //    {
        //        EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
        //        inputField.OnPointerClick(new PointerEventData(EventSystem.current));
        //    }


        //MyIO.DebugLog("The Updated Screen (Gameview, Display 2)=");
        //MyIO.DebugLog(Screen.width + "x" + Screen.height);

        //https://forum.unity.com/threads/test-if-ui-element-is-visible-on-screen.276549/?_ga=2.243694470.2030322823.1571061643-1616839984.1565653891#post-2978773

        //bool isFullyVisible = myRectTransform.IsFullyVisibleFrom(myCamera);
        //RaycastWorldUI();


        //Canvas Structure  Debug

        //Debug.Log(" **************************************************");
        //Debug.Log(" **************************************************");

        //Debug.Log(" Anchor of canvas Obj:");
        //Debug.Log(" AnchorMin:");
        //MyIO.DebugLog(m_canvasObj.GetComponent<RectTransform>().anchorMin);

        //Debug.Log(" AnchorMax:");
        //MyIO.DebugLog(m_canvasObj.GetComponent<RectTransform>().anchorMax);
        //Debug.Log(" Pivot:");
        //MyIO.DebugLog(m_canvasObj.GetComponent<RectTransform>().pivot);


        //https://forum.unity.com/threads/recttransform-anchoredposition-relative-to-its-canvas.397924/

        //        AFAIK anchoredPosition is delta / distance of UI objects rectTransform pivot to anchor corners.
        //            So if you have all four anchors in "flower" shape, and move your object right from it 100 pixels,
        //        and pivot is in center of your object, your anchoredPosition is 100,0.
        //

        //        First of all, ensure you set the parent before setting local position, then, you have two options according 
        //            to the type of Rect Transform: non - stretching and stretching ones.
        //  (1) For a non - stretching Rect Transform, the position is set most easily by setting the anchoredPosition and 
        //            the sizeDelta properties.The anchoredPosition specifies the position of the pivot relative to the anchors. 
        //            The sizeDelta is just the same as the size when there’s no stretching.

        // (2) For a stretching Rect Transform, it can be simpler to set the position using the offsetMin
        //   and offsetMax properties. The offsetMin property specifies the corner of the lower left corner 
        //    of the rect relative to the lower left anchor. The offsetMax property specifies the corner of 
        //    the upper right corner of the rect relative to the upper right anchor.
        //   RectTransform rt = GetComponent<RectTransform>();
        //  rt.offsetMin = rt.offsetMax = Vector2.zero;

        //Debug.Log(" AnchoredPosition of canvas Obj:");

        //MyIO.DebugLog(m_canvasObj.GetComponent<RectTransform>().anchoredPosition);

        //Debug.Log(" LocalPosition of canvas Obj (its pivot) (wrt The parent of the canvas obj, the world Transform):");

        //MyIO.DebugLog(m_canvasObj.GetComponent<RectTransform>().localPosition);


        //Debug.Log(" global Position of canvast Obj:");

        //MyIO.DebugLog(m_canvasObj.GetComponent<RectTransform>().position);

        //Debug.Log(" sizeDeltaof canvas Obj:");

        //MyIO.DebugLog(m_canvasObj.GetComponent<RectTransform>().sizeDelta);

        //Debug.Log(" center of rect of Canvas Obj:");

        //MyIO.DebugLog(m_canvasObj.GetComponent<RectTransform>().rect.center);

        //Debug.Log(" size of rect of Canvas Obj:");

        //MyIO.DebugLog(m_canvasObj.GetComponent<RectTransform>().rect.size);


        //Debug.Log(" **************************************************");
        //Debug.Log(" Anchor of Scrollview  Obj:");
        //Debug.Log(" AnchorMin:");
        //MyIO.DebugLog(m_scrollViewObj.GetComponent<RectTransform>().anchorMin);

        //Debug.Log(" AnchorMax:");
        //MyIO.DebugLog(m_scrollViewObj.GetComponent<RectTransform>().anchorMax);
        //Debug.Log(" Pivot:");
        //MyIO.DebugLog(m_scrollViewObj.GetComponent<RectTransform>().pivot);

        //Debug.Log(" AnchoredPosition of Scrollview Obj:");


        //MyIO.DebugLog(m_scrollViewObj.GetComponent<RectTransform>().anchoredPosition);

        //Debug.Log(" LocalPosition of Scroll view Obj (wrt to the frame of the parent (canvas)):");

        //MyIO.DebugLog(m_scrollViewObj.GetComponent<RectTransform>().localPosition);


        //Debug.Log(" global Position of ScrollView Obj:");

        //MyIO.DebugLog(m_scrollViewObj.GetComponent<RectTransform>().position);

        //Debug.Log(" sizeDelta of ScrollView Obj:");

        //MyIO.DebugLog(m_scrollViewObj.GetComponent<RectTransform>().sizeDelta);

        //Debug.Log(" center of rect of scrollview Obj:");

        //MyIO.DebugLog(m_scrollViewObj.GetComponent<RectTransform>().rect.center);

        //Debug.Log(" size of rect of ScrollView Obj:");

        //MyIO.DebugLog(m_scrollViewObj.GetComponent<RectTransform>().rect.size);



        //Debug.Log(" **************************************************");
        //Debug.Log(" Anchor of viewport  Obj:");
        //Debug.Log(" AnchorMin:");
        //MyIO.DebugLog(m_viewportObj.GetComponent<RectTransform>().anchorMin);

        //Debug.Log(" AnchorMax:");
        //MyIO.DebugLog(m_viewportObj.GetComponent<RectTransform>().anchorMax);
        //Debug.Log(" Pivot:");
        //MyIO.DebugLog(m_viewportObj.GetComponent<RectTransform>().pivot);
        //Debug.Log(" AnchoredPosition of viewport Obj:");


        //MyIO.DebugLog(m_viewportObj.GetComponent<RectTransform>().anchoredPosition);

        //Debug.Log(" LocalPosition of viewport Obj (wrt to the frame of the parent (Scrollview):");

        //MyIO.DebugLog(m_viewportObj.GetComponent<RectTransform>().localPosition);


        //Debug.Log(" global Position of viewport Obj:");

        //MyIO.DebugLog(m_viewportObj.GetComponent<RectTransform>().position);

        //Debug.Log(" sizeDelta of viewport Obj:");

        //MyIO.DebugLog(m_viewportObj.GetComponent<RectTransform>().sizeDelta);

        //Debug.Log(" center of rect of viewport Obj (wrt its pivot location):");

        //MyIO.DebugLog(m_viewportObj.GetComponent<RectTransform>().rect.center);

        //Debug.Log(" size of rect of viewport Obj:");

        //MyIO.DebugLog(m_viewportObj.GetComponent<RectTransform>().rect.size);

        //Debug.Log(" **************************************************");
        //Debug.Log(" Anchor of Content  Obj:");
        //Debug.Log(" AnchorMin:");
        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().anchorMin);

        //Debug.Log(" AnchorMax:");
        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().anchorMax);
        //Debug.Log(" Pivot:");
        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().pivot);

        //Debug.Log(" AnchoredPosition of content  Obj:");


        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().anchoredPosition);

        //Debug.Log(" LocalPosition of content Obj (wrt the frame of the parent (viewport)):");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().localPosition);


        //Debug.Log(" global Position of content Obj:");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().position);

        //Debug.Log(" sizeDelta of conent Obj:");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().sizeDelta);

        //Debug.Log(" center of rect of content Obj (wrt the pivot):");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().rect.center);

        //Debug.Log(" size of rect of content Obj:");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().rect.size);




    } // Update()


    // RayCasting: https://forum.unity.com/threads/eventsystem-raycastall-alternative-workaround.372234/
    //https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.EventSystem.html
    //https://www.reddit.com/r/Unity3D/comments/8raca3/how_do_i_get_eventsystemscurrentraycastall_to/

    //https://forum.unity.com/threads/is-onmousedown-healthy-to-use.510173/

    //    To check if the mouse is over any UI element you can use

    //EventSystem.current.IsPointerOverGameObject()
    //In case that function isn’t acting to your liking and you need to debug, or if you just want to know what object is under the mouse, you can use

    //PointerEventData pointerData = new PointerEventData(EventSystem.current)
    //    {
    //        position = Input.mousePosition
    //};

    //    List<RaycastResult> results = new List<RaycastResult>();
    //    EventSystem.current.RaycastAll(pointerData, results);

    //results.ForEach((result) => {
    //Debug.Log(result);
    //});




    //Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition)
    //public class NewBehaviourScript : MonoBehaviour, IPointerClickHandler
    //{
    //    public void OnPointerClick(PointerEventData eventData)
    //    {
    //        Debug.Log("Clicked" + gameObject.name);
    //    }
    //}

    //                using UnityEngine;
    //                using System.Collections;

    //public class ExampleScript : MonoBehaviour
    //        {
    //            public Camera camera;

    //            void Start()
    //            {
    //                RaycastHit hit;
    //                Ray ray = camera.ScreenPointToRay(Input.mousePosition);

    //                if (Physics.Raycast(ray, out hit))
    //                {
    //                    Transform objectHit = hit.transform;

    //                    // Do something with the object that was hit by the raycast.
    //                }
    //            }
    //      }


    //returns true if a ui elemend was clicked
    bool isUIElementRaycasted(string gameObjectName)
        {
            if (m_RootElementUI == null)
                return false;
            else
            {
                if (gameObjectName == m_RootElementUI.name)
                {
                    m_RootElementUI = null;
                    return true;
                }
                else return false;
            }
        }

      
     void RaycastWorldUI()
        {
        //    GetMouseButtonDown(int button);
        ////
        //// Summary:
        ////     Returns true during the frame the user releases the given mouse button.
        ////
        //// Parameters:
        ////   button:

        // The following is an old way of UI in unity.
        // Try to use  the new one:
        //At the core of Unity's new input event system, there are several interfaces that describe all the different types of events
        //the base implementation is able to handle:

        if (Input.GetMouseButtonDown(0)) // the left mouse button clicked
                                             // button values are 0 for the primary button (often the left button), 
                                            // 1 for secondary button, and 2 for the middle button.
        {
            m_pointerData = new PointerEventData(EventSystem.current);
            //public static EventSystem current { get; set; }
            // Return the current EventSystem.

            m_pointerData.position = Input.mousePosition;

            Debug.Log("mouse position in RayCastWorldUI=");
            Debug.Log(Input.mousePosition);

            //Debug.Log("pointer Event Data=");

            //Debug.Log(m_pointerData);


            Debug.Log("pointerPress(GameObject that recieved OnPointer=");
            Debug.Log(m_pointerData.pointerPress);

            Debug.Log("clock count=");
            Debug.Log(m_pointerData.clickCount);

            Debug.Log("world position=");
            Debug.Log(m_pointerData.pointerCurrentRaycast.worldPosition);
               
   
            Debug.Log("The screen space coordinates of the last pointer click:");
            Debug.Log(m_pointerData.pressPosition);
            Debug.Log("Current pointer position");
            Debug.Log(m_pointerData.position);
            Debug.Log("Identification of the pointer:");
            Debug.Log(m_pointerData.pointerId);
    


           List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll( m_pointerData, results);
            

            if (results.Count > 0)
                {

                    if (results[0].gameObject.layer == LayerMask.NameToLayer("UI"))
                    {

                        string dbg = "Root Element: {0} \n GrandChild Element: {1}";

                        m_RootElementUI = results[results.Count - 1].gameObject;

                    // //Set this GameObject you clicked as the currently selected in the EventSystem
                    // m_EventSystem.SetSelectedGameObject( m_RootElementUI );


                        Debug.Log(string.Format(dbg, results[results.Count - 1].gameObject.name, results[0].gameObject.name));
                        //Debug.Log("Root Element: " + results[results.Count - 1].gameObject.name);
                        //Debug.Log("GrandChild Element: " + results[0].gameObject.name);

                        for (int i = 0; i < results.Count; i++)
                        {
                          Debug.Log("Hit Element: " + results[i].gameObject.name);
                        }

                        results.Clear();
                } // if (results.Count > 0)
            } //if (Input.GetMouseButtonDown(0))
        } //  if (Input.GetMouseButtonDown(0))

        } //  void RaycastWorldUI()


    //     RaycastResult associated with the pointer press.
    // public RaycastResult pointerPressRaycast { get; set; }
    //
    // Summary:
    //     RaycastResult associated with the current event.
    // public RaycastResult pointerCurrentRaycast { get; set; }
    //
    // Summary:
    //     The object that is receiving OnDrag.
    // public GameObject pointerDrag { get; set; }
    //
    // Summary:
    //     The object that the press happened on even if it can not handle the press event.
    // public GameObject rawPointerPress { get; set; }
    //
    // Summary:
    //     The GameObject for the last press event.
    //public GameObject lastPress { get; }
    //
    // Summary:
    //     The object that received 'OnPointerEnter'.
    //public GameObject pointerEnter { get; set; }




}//class UIWorldDetect



////https://forum.unity.com/threads/draging-a-cube-with-unityengine-eventsystems.383639/
//*
//* 3D project
//* Create a cube
//* Create an empty gameobject and attach an eventsystem to the empty object, add Standalone input module
//* Add a Physics Raycaster to the main camera
//* Add this scrip to the cube
//*
//* Now you can drag the cube in the x and y directons.
//*
//* Add touch input to the eventsystem and it works on ios and android.
//*
//* */

//public class Dragg : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
//{

//    float dx;
//    float dy;
//    Vector3 newPos;

//    MeshRenderer mesh;

//    Color col;
//    Color startCol;


//    void Start()
//    {
//        newPos = Vector3.zero;

//        mesh = GetComponent<MeshRenderer>();
//        startCol = mesh.material.color;
//        col = Color.blue;
//    }

//    void Update()
//    {

//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        mesh.material.color = col;
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        if (eventData.dragging)
//        {
//            return;
//        }
//        mesh.material.color = startCol;
//    }

//    public void OnBeginDrag(PointerEventData eventData)
//    {
//        dx = eventData.pointerPressRaycast.worldPosition.x - transform.position.x;
//        dy = eventData.pointerPressRaycast.worldPosition.y - transform.position.y;
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        newPos.x = eventData.pointerCurrentRaycast.worldPosition.x - dx;
//        newPos.y = eventData.pointerCurrentRaycast.worldPosition.y - dy;
//        if (eventData.pointerCurrentRaycast.worldPosition.x == 0 || eventData.pointerCurrentRaycast.worldPosition.y == 0)
//        {
//            newPos.x = eventData.delta.x;
//            newPos.y = eventData.delta.y;

//            transform.Translate(newPos * Time.deltaTime);
//            return;
//        }
//        transform.position = newPos;
//    }

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        Debug.Log("OnEndDrag");
//    }

//    public void OnPointerClick(PointerEventData eventData)
//    {

//        Debug.Log("OnPointerClick");
//    }
//}

//https://answers.unity.com/questions/867313/ui-46-input-field-how-do-i-use-the-onendedit.html
//using UnityEngine;
// using System.Collections;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;

// public class CreateCanvasButton : MonoBehaviour
//{




//    void Start()
//    {
//        // create event system
//        GameObject eventsystem = new GameObject();
//        eventsystem.AddComponent<EventSystem>();
//        eventsystem.AddComponent<StandaloneInputModule>();
//        eventsystem.AddComponent<TouchInputModule>();


//        string name = "Canvas";

//        GameObject newCanvasGO = new GameObject();
//        newCanvasGO.name = name;
//        newCanvasGO.transform.SetParent(this.transform);
//        newCanvasGO.AddComponent<Canvas>();
//        newCanvasGO.AddComponent<CanvasScaler>();
//        newCanvasGO.AddComponent<GraphicRaycaster>();

//        RectTransform theCanvasRectTransform = newCanvasGO.GetComponent<RectTransform>();
//        theCanvasRectTransform.anchoredPosition3D = new Vector3(0f, 0f, 0f);
//        theCanvasRectTransform.sizeDelta = new Vector2(2400f, 3800f);
//        theCanvasRectTransform.anchorMin = new Vector2(0f, 0f);
//        theCanvasRectTransform.anchorMax = new Vector2(1f, 1f);
//        theCanvasRectTransform.localScale = new Vector3(0.0005f, 0.0005f, 1f);
//        theCanvasRectTransform.localPosition = new Vector3(0f, 2.9f, 0f);
//        Canvas theCanvas = newCanvasGO.GetComponent<Canvas>();
//        theCanvas.worldCamera = Camera.main;
//        newCanvasGO.SetActive(true);
//        newCanvasGO.AddComponent<Image>();
//        Image canvasImage = newCanvasGO.GetComponent<Image>();
//        Sprite mySprite = Resources.Load<Sprite>("Background");
//        canvasImage.sprite = mySprite;
//        canvasImage.type = Image.Type.Sliced;
//        // create name title text
//        GameObject titleTextGO = new GameObject();
//        titleTextGO.name = "Name_text";
//        titleTextGO.transform.parent = newCanvasGO.transform;
//        Text titleText = titleTextGO.AddComponent<Text>();
//        RectTransform titleTextRT = titleTextGO.GetComponent<RectTransform>();
//        titleTextRT.anchoredPosition3D = new Vector3(0f, 0f, 0f);
//        titleTextRT.sizeDelta = new Vector2(877, 199);
//        titleTextRT.localPosition = new Vector3(-22f, 1722f, 0f);
//        titleTextRT.localScale = new Vector3(1f, 1f, 1f);
//        titleText.text = "Title";
//        Font theFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
//        titleText.font = theFont;
//        titleText.fontSize = 140;
//        titleText.color = Color.black;
//        titleText.alignment = TextAnchor.MiddleCenter;
//        titleText.resizeTextForBestFit = true;
//        titleText.resizeTextMinSize = 10;
//        titleText.resizeTextMaxSize = 160;
//        // create nickname title text
//        GameObject TextGO = new GameObject();
//        TextGO.name = "InputField";
//        TextGO.transform.parent = newCanvasGO.transform;
//        TextGO.AddComponent<RectTransform>();
//        RectTransform TextRT = TextGO.GetComponent<RectTransform>();
//        TextGO.AddComponent<InputField>();
//        InputField myInput = TextGO.GetComponent<InputField>();
//        TextRT.anchoredPosition3D = new Vector3(0f, 0f, 0f);
//        TextRT.sizeDelta = new Vector2(1669, 3370);
//        TextRT.localPosition = new Vector3(-35f, -188f, 0f);
//        TextRT.localScale = new Vector3(1f, 1f, 1f);
//        myInput.transition = Selectable.Transition.None;
//        myInput.text = "SomeText that never appears";


//        //myInput.onEndEdit (myEndEdit()); // this line doens't work


//        GameObject textForInputGO = new GameObject();
//        textForInputGO.name = "Text_ForInputField";
//        textForInputGO.transform.SetParent(TextGO.transform);
//        RectTransform textRT = textForInputGO.AddComponent<RectTransform>();
//        Text textscript = textForInputGO.AddComponent<Text>();
//        textRT.sizeDelta = new Vector2(1356f, 300f);
//        textRT.localPosition = new Vector3(0f, -0f, 0f);
//        textRT.localRotation = new Quaternion(0f, 0f, 0f, 0f);
//        textRT.localScale = new Vector3(1f, 1f, 1f);
//        myInput.textComponent = textscript;
//        textscript.supportRichText = false;
//        textscript.resizeTextForBestFit = true;
//        textscript.resizeTextMaxSize = 300;
//        textscript.resizeTextMinSize = 10;
//        textscript.font = theFont;
//        textscript.text = "This is the main body text.";
//        textscript.color = Color.black;
//        textscript.alignment = TextAnchor.MiddleCenter;


//        // create button
//        GameObject invisibleButtonGO = new GameObject();
//        invisibleButtonGO.name = "invisibleButton";
//        invisibleButtonGO.transform.parent = newCanvasGO.transform;
//        invisibleButtonGO.AddComponent<RectTransform>();
//        RectTransform invisibleButtonRT = invisibleButtonGO.GetComponent<RectTransform>();
//        invisibleButtonRT.anchoredPosition3D = new Vector3(0f, 0f, 0f);
//        invisibleButtonRT.sizeDelta = new Vector2(358, 354);
//        invisibleButtonRT.localPosition = new Vector3(1021f, -1723f, 0f);
//        invisibleButtonRT.localScale = new Vector3(1f, 1f, 1f);
//        invisibleButtonGO.AddComponent<Button>();
//        Button invisibleButton = invisibleButtonGO.GetComponent<Button>();
//        invisibleButton.transition = Selectable.Transition.None;
//        invisibleButton.onClick.AddListener(() => handleButton());

//        //add image for ray casting invisable button:
//        Image image = invisibleButtonGO.AddComponent<Image>();
//        image.color = new Color(0, 0, 0, 0);

//    }

//    void handleButton()
//    {
//        print("pressed!");
//    }

//    public void myEndEdit(string thestring)
//    {
//        print(thestring);
//        // I want to call this method on submission or clicking off the input field and stop the input field from picking up more 
//        // key presses until it is clicked again.
//    }

//}

//https://forum.unity.com/threads/inputfield-s-inside-a-scrollview.446976/

//    using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;


//namespace Keop
//{

//    [RequireComponent(typeof(InputField))]
//    public class ScrollInputFieldFixer : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
//    {
//        private ScrollRect _scrollRect = null;
//        private InputField _input = null;
//        private bool _isDragging = false;


//        private void Start()
//        {
//            _scrollRect = GetComponentInParent<ScrollRect>();
//            _input = GetComponent<InputField>();
//            _input.DeactivateInputField();
//            _input.enabled = false;
//        }


//        public void OnBeginDrag(PointerEventData data)
//        {
//            if (_scrollRect != null && _input != null)
//            {
//                _isDragging = true;
//                _input.DeactivateInputField();
//                _input.enabled = false;
//                _scrollRect.SendMessage("OnBeginDrag", data);
//            }
//        }


//        public void OnEndDrag(PointerEventData data)
//        {
//            if (_scrollRect != null && _input != null)
//            {
//                _isDragging = false;
//                _scrollRect.SendMessage("OnEndDrag", data);
//            }
//        }


//        public void OnDrag(PointerEventData data)
//        {
//            if (_scrollRect != null && _input != null)
//            {
//                _scrollRect.SendMessage("OnDrag", data);
//            }
//        }


//        public void OnPointerClick(PointerEventData data)
//        {
//            if (_scrollRect != null && _input != null)
//            {
//                if (!_isDragging && !data.dragging)
//                {
//                    _input.enabled = true;
//                    _input.ActivateInputField();
//                }
//            }
//        }
//    }
//}

//keop, Oct 26, 2018

//https://forum.unity.com/threads/how-to-addlistener-featuring-an-argument.266934/
//WOW! 
//public class FloatEvent : UnityEvent<float> { } //parameterized EMPTY  class; just needs to exist

//public FloatEvent onSomeFloatChange = new FloatEvent();

//void SomethingThatInvokesTheEvent()
//{
//    onSomeFloatChange.Invoke(3.14f);
//}

////Elsewhere:
//onSomeFloatChange.AddListener(SomeListener); 
// e.g._inputField.onEndEdit.AddListener(delegate { InputValue(indexToTimedAction, _inputField, timedAction); });
// Here onSomeFloatChange ==> onEndEdit; onEndEdit: UnityEvent<InputField> { }

//void SomeListener(float f) // SomeListener ==> InputValue above
//{
//    Debug.Log("Listened to change on value " + f); //prints "Listened to change on value 3.14"
//}


//void Start()
//{
//    int i = 0;
//    foreach (var button in buttonCont)
//    {
//        int _i = ++i;
//        button.GetComponent<Button>().onClick.AddListener(delegate { TestFn(_i); });
//    }
//}
//That way you are creating a seperate variable to be captured by the closure every time, so its value is never modified.

//@shaderop: RE archaic style - Eh, I'd say that's purely subjective in this case. One is a lambda, the other a delegate. The advantange of delegates is you can use it to wrap more than one function call (delegate{Foo(a); a += 10; Bar(a);}); the advantage of lambdas in this case is it's shorter and typically easier on the eyes. I'd advise anyone to just use whichever they find easiest/ most readable. 


//Senshi, Oct 5, 2015




//Joined:
//Nov 24, 2010
//Posts:
//942

//Senshi said: ↑ 
//The advantange of delegates is you can use it to wrap more than one function call
//I don't think that is correct. You can definitely go to town in a lambda expression, e.g:
//Code(csharp) :
//onClick.AddListener(() => {
//  Foo();
//Bar();
//AdInfinitum(); 
//});
//The is valid code that will compile.It's also quite common in the wild from what I have seen.

// The newest method: https://stackoverflow.com/questions/36244660/simple-event-system-in-unity/36249404#36249404

//public class SimpleEvent : MonoBehaviour
//{  //My UnityEvent Manager  public UnityEvent myUnityEvent = new UnityEvent(); 
//                                      === Event Constructor; this is used to construct Button, Slider, InputField, etc

//    void Start()
//    {    //Subscribe my delegate to my UnityEvent Manager   
//           myUnityEvent.AddListener(MyAwesomeDelegate);  
           //Execute all registered delegates   
           //  myUnityEvent.Invoke();  }

//  //My Delegate function  private void MyAwesomeDelegate()  {    Debug.Log("My Awesome UnityEvent lives");  } }