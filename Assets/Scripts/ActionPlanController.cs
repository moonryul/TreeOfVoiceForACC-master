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

using System.Xml.Linq;
using System.Text;

// Canvas Event Camera Target Display
//https://forum.unity.com/threads/multi-display-canvases-not-working-in-5-4-2.439429/
//Despite saying this issue was resolved in 5.4.2, I'm unable to register input across displays. I currently have 3 displays in one scene, 
//    with UI elements being rendered in camera space for each display. The UI Event system is only registering 
//    on one display. Switching to the other two displays in the game tab does not effect the UI Event System.

//For example, the Event System registers a button click on Display 2. When I switch the game tab to Display 3, 
//the Event System still registers all clicks for Display 2.

//Screen Space - Camera
//This is similar to Screen Space - Overlay, but in this render mode the Canvas is placed a given distance in front of 
//    a specified Camera
//.The UI elements are rendered by this camera, which means that the Camera settings affect the appearance of the UI. 
//    If the Camera is set to Perspective, the UI elements will be rendered with perspective, and 
//    the amount of perspective distortion can be controlled by the Camera Field of View.If the screen is resized, 
//    changes resolution, or the camera frustum changes,
//    the Canvas will automatically change size to match as well.

// Create a Canvas that holds a Text GameObject.

//https://forum.unity.com/threads/setting-custom-canvas-size.263788/
// The Canvas is meant to cover the entire screen when set to Screen Space modes, hence the entire Game View when in the editor.


//http://www.aclockworkberry.com/managing-screen-resolution-and-aspect-ratio-in-unity-3d/
//https://github.com/gportelli/UnityScreenResolutionManager

//


//[Serializable]
//public class MainInput_Ray
//{
//    public Ray ray = new Ray();
//    public RaycastHit hit;
//    public bool hittingSomething { get; set; }
//}


//public class InputFieldSubmitOnly : InputField
//{
//    protected override void Start()
//    {
//        base.Start();

//        for (int i = 0; i < this.onEndEdit.GetPersistentEventCount(); ++i)
//        {
//            int index = i; // Local copy for listener delegate
//            this.onEndEdit.SetPersistentListenerState(index, UnityEventCallState.Off);
//            this.onEndEdit.AddListener(delegate (string text) {
//                if (!EventSystem.current.alreadySelecting)
//                {
//                    ((Component)this.onEndEdit.GetPersistentTarget(index)).SendMessage(this.onEndEdit.GetPersistentMethodName(index), text);
//                }
//            });
//        }
//    }
//}

//•IPointerDownHandler - OnPointerDown - Called when a pointer is pressed on the object
//•IPointerUpHandler - OnPointerUp - Called when a pointer is released(called on the GameObject that the pointer is clicking)
//•IPointerClickHandler - OnPointerClick - Called when a pointer is pressed and released on the same object
//•IInitializePotentialDragHandler - OnInitializePotentialDrag - Called when a drag target is found, can be used to initialise values
//•IBeginDragHandler - OnBeginDrag - Called on the drag object when dragging is about to begin
//•IDragHandler - OnDrag - Called on the drag object when a drag is happening
//•IEndDragHandler - OnEndDrag - Called on the drag object when a drag finishes

//ActionPlanController has 3 main methods: InitActions, UpdateActionValue, UpdateActionTime, UpdateActionSlot, RedrawActionPlan
// each of which is invoked directly by the main CommHub script or through delegates which is controlled by main CommHub
public class ActionPlanController : MonoBehaviour
{

    public class Action
    {
        public List<float> T;
        public float V;

    }

    Button m_loadButton;
    Button m_saveButton;

    public Button loadButton
       { get { return m_loadButton; }
         set {m_loadButton = value; }
        }

    public Button saveButton
    {
        get { return m_saveButton; }
        set { m_saveButton = value; }
    }

    // define Event Deletates
    public delegate void OnSaveActionPlan(Dictionary<string, List<int>> dict, string _file);
    public static OnSaveActionPlan onSaveActionPlan;

    public delegate void OnLoadActionPlan(Dictionary<string, List<int>> dict, string _file);
    public static OnLoadActionPlan onLoadActionPlan;

    public SimpleBoidsTreeOfVoice m_boidsController; // 

    // This is a component class; So you can attach it to the gameobject to which ActionPlannerController
    // Component is attached. Then you can get the reference to this component by gameObject.GetComponent<SimpleBoidsTreeOfVoice>()

    public float m_AnimationCycle = 390;

    public int m_contentWidth = 10000;  // the size of the content will be set accoring to the size of the action table
    public int m_contentHeight= 2000;

    public int m_scrollRectWidth;
    public int m_scrollRectHeight;

    public int  m_canvasWidth, m_canvasHeight; // the canvas size is set to the size of the game view screen automatically
                                         // The actuall scroll rect size is set to the size of the canvas

    public Dictionary<String, List<Action> > m_actionPlan; //// first defined in SimpleBoidsTreeOfVoice class


    int m_lastScreenWidth;
    int m_lastScreenHeight;
    bool stay = true;

 
    public MyScrollRect m_myScrollRect;  // set in the inspector

    public GameObject m_canvasObj, m_scrollViewObj, m_viewportObj;
    public GameObject m_contentObj, m_contentTitleObj, m_contentTimeLineObj, m_contentKeysObj, m_contentValuesObj;
    public GameObject m_scrollbarHorizontalObj, m_scrollbarVerticalObj;

    public GameObject m_contentTimeLineClipRectObj, m_contentKeysClipRectObj, m_contentValuesClipRectObj;

    public GameObject m_saveButtonObj, m_loadButtonObj;

    CanvasRenderer m_viewportCanvasRenderer;

    public Canvas m_canvas; // referred to in FileBrowser.cs
   
    RectTransform m_canvasRectTransform; 
    float m_distanceToCamera;

    // Raycasting
    private PointerEventData m_pointerData;
    private GameObject m_RootElementUI;


    GameObject[] m_timeTopBarContainer, m_horizontalLines, m_verticalLines;

    float m_timeScaleFactor; // the scale factor for converting the time value to the length in pixels

    public List<GameObject>[] m_inputFieldContainer, m_textContainer, m_placeholderContainer; // gameobject which has InputField as a component

    public Hashtable m_inputFieldHashTable;

    GameObject[] m_nameContainer;

    List<Text>[] m_text;


    List<InputField> [] m_inputField; // inputField is a component

    RectTransform m_inputFieldRectTransform; // RectTransform inheirts Transform properties, e.g. localPosition, localScale
    
    RenderMode m_guiMode;

    Camera m_guiCamera;
    Camera m_mainCamera;


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
    
    List<Action> m_timedActions;

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

  

    void OnGUI()
    {
      //The Label shows the current Rect settings on the screen (GUI => origin = left top)
        GUI.Label(new Rect(20, 20, 500, 80), "Rect : " + m_canvasRectTransform.rect);
    }

    
    // Use Awake() to initialize field variables.
    public void Awake()
        {

        //   https://forum.unity.com/threads/rectmask2d-does-not-work-when-canvas-render-mode-is-sceen-space-camera-or-world-space-2017-2-0f3.499966/
        //public class FixRectMask2dWebGL : MonoBehaviour
        //{
        //    private void Awake()
        //    {
        //        var items = GetComponentsInChildren<MaskableGraphic>(true);
        //        for (int i = 0; i < items.Length; i++)
        //        {
        //            Material m = items[i].materialForRendering;
        //            if (m != null)
        //                m.EnableKeyword("UNITY_UI_CLIP_RECT");
        //        }
        //    }
        //}


        //var items = GetComponentsInChildren<MaskableGraphic>(true);
        //    for (int i = 0; i < items.Length; i++)
        //    {
        //        Material m = items[i].materialForRendering;
        //        if (m != null)
        //            m.EnableKeyword("UNITY_UI_CLIP_RECT");
        //    }
    

    //Shader.EnableKeyword("UNITY_UI_CLIP_RECT");

        // Initialize me:
        m_paramSize = "_CeilingCirculationWeight".Length;
      

        // Initialize me:
        ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        m_fontSize = ArialFont.fontSize;
        m_lineHeight = ArialFont.lineHeight;

        // Create a Text UI element
        m_textComponent = gameObject.AddComponent<Text>();

        m_textComponent.supportRichText = false;
        m_textComponent.color = new Color(1f, 0f, 0f);

        m_textComponent.font = ArialFont;
        m_textComponent.material = ArialFont.material;

        m_textComponent.fontSize = 12;
        m_textComponent.alignment = TextAnchor.MiddleCenter;
        m_textComponent.lineSpacing = 0;

        m_textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        m_textComponent.verticalOverflow = VerticalWrapMode.Overflow;


        //TextGenerator textGen = new TextGenerator();
        //TextGenerationSettings generationSettings = m_myTextBox.GetGenerationSettings(m_myTextBox.rectTransform.rect.size);
        //float width = textGen.GetPreferredWidth(newText, generationSettings);
        //float height = textGen.GetPreferredHeight(newText, generationSettings);

        //       The RectTransform may not update until the end of a frame, but I've seen an immediate change in the value I get when calling LayoutUtility.GetPreferredHeight(transform). This will tell you how tall the Text would be if it were able to overflow all of its vertical content. Try:

        //Debug.Log(LayoutUtility.GetPreferredHeight(transform) + " vs " + transform.rect.height);
        //       text.text = e.text;
        //       Debug.Log(LayoutUtility.GetPreferredHeight(transform) + " vs " + transform.rect.height);
        //       Where transform is a RectTransform and text is a Text.

        // I know this is an old thread but one way to force Canvases to update is to use Canvas.ForceUpdateCanvases().
        // The Rect of the TextComponent will now be updated. Keep in mind that this forces all canvases to update their layout.

        //   http://docs.unity3d.com/ScriptReference/Canvas.ForceUpdateCanvases.html


        m_textGen = new TextGenerator();
        Vector2 extents = new Vector2(0, 100);
        m_generationSettings = m_textComponent.GetGenerationSettings( extents);

        string sampleText = "_CeilingCirculationWeight";
        m_paramTextWidth = m_textGen.GetPreferredWidth( sampleText, m_generationSettings);
        m_paramTextHeight = m_textGen.GetPreferredHeight( sampleText, m_generationSettings);

        sampleText = "1234567890"; // The width of the interval between two time points (which is regarded as 10 seconds)
        m_timeTextWidth = m_textGen.GetPreferredWidth(sampleText, m_generationSettings);
        m_timeTextHeight = m_textGen.GetPreferredHeight(sampleText, m_generationSettings);


        sampleText = "0.1"; // value such as weightss
        m_valueTextWidth = m_textGen.GetPreferredWidth(sampleText, m_generationSettings);
        m_valueTextHeight = m_textGen.GetPreferredHeight(sampleText, m_generationSettings);

        //https://stackoverflow.com/questions/43592712/what-does-actually-fontsize-mean-in-unity/43595518

        //  Text component font sizes in Unity are measured in "pixels of height" and are 1:1 with screen pixels
        //  (except when the transform--or any of its parent transforms--are scaled, 
        //   or if the canvas itself is in Worldspace (in which case you kinda have to guess)).

        //  On the text object's Transform: you can additionally scale the object and get the 
        //   (apparent) font size to change although scaling it up will make it look blurry and scaling 
        //   it down will make it look aliased. Either way it looks bad.

        //   All that said, I've found best results to be to double the font size and reduce the transform scale to 0.5 
        //   as the text anti-aliasing at the default scale looks blurry to me, so using a bigger font size 
        //  and scaling the object transform back down sharpens it up. 
        //  

        //Debug.Log("Arial Font size="); Debug.Log(m_fontSize);
        //Debug.Log("Arial Font Line Height =");
        //Debug.Log(m_lineHeight);




        // The gameObject to which the  Script component UISetActionPlan is attached to Canvas gameObject.
        // This Canvas gameObject has a Canvas component by default    

        //m_canvasHeight = Screen.height; //The pixel size of the canvas is set to the resolution of the game view,
        //                                // which are Screen.height, Screen.width of the primary display (display 1)
        //m_canvasWidth = Screen.width;

        // Get the reference to Canvas GameObject
        // someObject = transform.parent.gameObject.transform.GetChild(0).gameObject;

        //m_canvasObj = this.gameObject;
        // UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
        //
        //Nop.GO's at the top of the GO hierarchy will have transform.parent set to null.
        //If you really need a global parent, you could just do that yourself by moving all your GOs into one top-level GO.


        Debug.Log("CommHub:" + this.gameObject );

        m_canvasObj = this.gameObject.transform.GetChild(0).gameObject;

        Debug.Log("Canvas Obj:" + m_canvasObj);

        //        public void SetParent(Transform parent, bool worldPositionStays);

        //        worldPositionStays
        //If true, the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before.
        //worldPositionStays 가 true일 경우. 자식이 되는 오브젝트의 월드 좌표는 변경되지 않는다(화면상으로 보기엔 그냥 그대로 있는것 같다는 말). 하지만 부모가 변경 되었으므로 로컬 좌표가 변경된다. 

        //출처: https://kukuta.tistory.com/177 [HardCore in Programming]

        // Get the first child of the canvas:
        // m_headerBarObj = m_canvasObj.transform.GetChild(0.gameObject;
        // Get the second child of the canvas:



        m_scrollViewObj = m_canvasObj.transform.GetChild(0).gameObject;
        // m_scrollViewObj.transform.SetParent(m_canvasObj.transform, false);

        // Get the first child of the ScrollView obj,  which is the viewport Obj
        m_viewportObj = m_scrollViewObj.transform.GetChild(0).gameObject;
       // m_viewportObj.transform.SetParent(m_scrollViewObj.transform, false);

        m_scrollbarHorizontalObj = m_scrollViewObj.transform.GetChild(1).gameObject;
       // m_scrollbarHorizontalObj.transform.SetParent(m_scrollViewObj.transform, false);

        m_scrollbarVerticalObj = m_scrollViewObj.transform.GetChild(2).gameObject;
        // m_scrollbarVerticalObj.transform.SetParent(m_scrollViewObj.transform, false);

        Debug.Log("viewport Obj:" + m_viewportObj);

        m_contentObj = m_viewportObj.transform.GetChild(0).gameObject;

        Debug.Log("content  Obj:" + m_contentObj );

        m_contentTitleObj = m_viewportObj.transform.GetChild(1).gameObject;
        
        m_contentTimeLineClipRectObj = m_viewportObj.transform.GetChild(2).gameObject;
        m_contentTimeLineObj = m_contentTimeLineClipRectObj.transform.GetChild(0).gameObject;
      
      
        m_contentKeysClipRectObj = m_viewportObj.transform.GetChild(3).gameObject;
               
    
        m_contentKeysObj = m_contentKeysClipRectObj.transform.GetChild(0).gameObject;
      

        m_contentValuesClipRectObj = m_viewportObj.transform.GetChild(4).gameObject;
        m_contentValuesObj = m_contentValuesClipRectObj.transform.GetChild(0).gameObject;

       

        Debug.Log("Gameobject found:");
        Debug.Log(m_canvasObj.name);
        Debug.Log(m_scrollViewObj.name);
        Debug.Log(m_viewportObj.name);
        Debug.Log(m_contentObj.name);

        // change the world camera of the canvas to EventCamera which uses Display 2

        //m_canvas.worldCamera = GameObject.Find("EventCamera").GetComponent<Camera>();

        m_canvas = m_canvasObj.GetComponent<Canvas>(); // == this.gameObject.GetComponent<Canvas>()



        //Unity UI was an issue.
        //Screen Space Overlay captures data from the primary display 
        // when setting up the UI on secondary. 
        //            So things get out of position and scale etc.

        //I set the Secondary Displays UI to Screenspace Camera instead.
        //and ensured that camera 2 existed and had DISPLAY2 set.


        //https://stackoverflow.com/questions/43614662/unity-change-the-display-camera-for-the-scene-and-the-target-display-in-the-can

        Debug.Log("RenderMode of Canvas in Awake()=");
        Debug.Log(m_canvas.renderMode);

        Debug.Log("Target Display of Canvas in Awake()=");
        Debug.Log(m_canvas.targetDisplay);

        Debug.Log("Change Target Display of Canvas from 0 to 1 which is the targetDisplay" +
                     "of the Event Camera in Awake(): This works only for Screen Overlay mode");

        m_canvas.targetDisplay = m_canvas.worldCamera.targetDisplay;
        // int targetDisplay { get; set; }
        Debug.Log("New Target Display of Canvas in Awake():");

    

        Debug.Log(m_canvas.targetDisplay);

        m_canvas.targetDisplay = 1;

        // int targetDisplay { get; set; }
        Debug.Log("New Target Display of Canvas after := 1 in Awake():");



        Debug.Log(m_canvas.targetDisplay);

        Debug.Log("Target Display of the Event Camera of canvas in Awake()=");
        Debug.Log(m_canvas.worldCamera.targetDisplay);

        // m_canvas.worldCamera.targetDisplay = 1; 
        // targetDisplay is index starting from 0; different from that of the inspector

        //Debug.Log("Changed Target Display of the Event Camera=");
        //Debug.Log(m_canvas.worldCamera.targetDisplay);


        Debug.Log("Scale Factor of Canvas in Awake()=");
        Debug.Log(m_canvas.scaleFactor);

        // m_canvas.targetDisplay = 2;



        //Debug.Log("Changed Target Display of Canvas [Screen Space-Overlay mode]=");
        //Debug.Log(m_canvas.targetDisplay);

        Debug.Log("  canvas resolution in Awake(); This size is changed to the main gameview resolution" +
            "in Start(); You can see this in the inspector when you click the canvas object;" +
            "The primary gameview is the default space of the canvas. But when you click" +
            "the secondary gameview, then the canvas dimension changes to the secondary view dimension" +
            "So set the m_canvas size in Awake()=");
        Debug.Log(m_canvas.pixelRect);



        m_canvasRectTransform = m_canvas.GetComponent<RectTransform>(); // used in onGUI() uses display 1 (primary display)

        m_guiCamera = m_canvas.worldCamera;

       m_mainCamera = Camera.main;



        // m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //gameObject.AddComponent<CanvasScaler>();
        //gameObject.AddComponent<GraphicRaycaster>();= 


        // get the distance from the _guiCamera to the plane of the canvas
        //  m_distanceToMain = Vector3.Distance(m_guiCamera.gameObject.transform.position, m_canvas.gameObject.transform.position);
        //m_distanceToCamera = Vector3.Distance(m_guiCamera.transform.position, m_canvas.transform.position);

        m_distanceToCamera = m_canvas.planeDistance;
        // For simplicity it is assumed that plane distance is set so that the z coordinates of the plane is zero
        // relative to the event camera.

        // Every component is attached to a gameobject which has a transform component by default; UI component has also a transform component
       
        // move canvas to position in front of main camera
        //m_canvas.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * m_distanceToCamera);

        // get the camera height at the frustum range- if it's orthographic, it's constant, so that's easy

        // By default the aspect ratio is automatically calculated from the screen's aspect ratio,
        //even if the camera is not rendering to full area. If you modify the aspect ratio of the camera, 
        //the value will stay until you call camera.ResetAspect(); which resets the aspect to the screen's aspect ratio.

        

        MyIO.DebugLog("The Screen (Gameview, Display 1) in Awake()=");
        MyIO.DebugLog( Screen.width + "x" + Screen.height);

        MyIO.DebugLog("The aspect of the Screen (Gameview, Display 1) in Awake()=");
        MyIO.DebugLog( (float)Screen.width/ (float)Screen.height );

        MyIO.DebugLog("The aspect of the main camera in Awake()=");
        MyIO.DebugLog(m_mainCamera.aspect);


        MyIO.DebugLog("The aspect of the canvas screen in Awake()=");
        MyIO.DebugLog((float)m_canvas.pixelRect.width / (float)m_canvas.pixelRect.height);

        MyIO.DebugLog("The aspect of the event camera in Awake()=");
        MyIO.DebugLog(m_guiCamera.aspect);

        Debug.Log("Screen Current Resolution (The native best resolution) in Awake()=");
        Debug.Log(Screen.currentResolution);






        float camHeight, camWidth;
        if (m_guiCamera.orthographic)
        {
            camHeight = m_guiCamera.orthographicSize * 2;
            camWidth= m_guiCamera.orthographicSize * 2;
        }
        else
        {
            camHeight = 2.0f * m_distanceToCamera * Mathf.Tan(Mathf.Deg2Rad * (m_guiCamera.fieldOfView * .5f));
            camWidth = camHeight * m_guiCamera.aspect;

           

         // NOTE: The aspect ratio of the camera is always changed to that of the screen (gameview)

            // camWidth = camHeight * m_guiCamera.aspect;
            // m_guiCamera.aspect =  Screen.width/ Screen.height;
        }

        //Initialize actionPlan

        InitActionPlan();

     

        SetupActionPlanUILocAndSize();

        SetupActionFileButtons();

        //m_canvas.targetDisplay = 2; No effect. Canvas.targetDisplay in Screen Space-Camera mode is 
        // 1.      

        // The canvas is set to the same width/height  as the screen resolution.
        // The scale of the canvas is used to transform the pixel coordinates of the canvas to the world coordinates,
        // which are rendered by the camera.

        //m_canvas.GetComponent<RectTransform>().localScale = new Vector3(camWidth / m_canvasWidth, camHeight / m_canvasHeight, 1);

        //m_canvas.transform.localScale = new Vector3(camWidth / m_canvasWidth, camHeight / m_canvasHeight, 1);

        //Debug.Assert(m_canvas.GetComponent<RectTransform>().localScale == m_canvas.transform.localScale,
        //                                    " the transform and rectTransform should have the same values");


        //var _canvasObj = GetComponentInParent<Canvas>();
        //    _guiCamera = _canvasObj.worldCamera;
        //    _guiMode = _canvasObj.renderMode;
        //    _rectTransform = GetComponent<RectTransform>();
        //    _text = GetComponentInChildren<Text>();
        //    _inside = false;       


    }// public void Awake()


    private void Start()
    {
        // m_canvas.gameObject.SetActive(true); // gameObject is the member of Component which is 
        // a super class of Canvas component

        //https://forum.unity.com/threads/how-do-i-control-which-gui-item-has-input-focus.263679/

        // Let the canvas get the focus so that the its content is displayed as intended
        //EventSystemManager.currentSystem.SetSelectedGameObject( input.gameObject, null );

        // Set the size of the canvas to the correct one determined in Awake()

        //m_canvas.pixelRect.width = m_canvasWidth;

        //m_canvasObj.GetComponent<RectTransform>().sizeDelta =
        //               new Vector2(m_canvasWidth, m_canvasHeight);

        //if (m_canvasObj != null)
        //{
        //    EventSystem.current.SetSelectedGameObject(m_canvasObj);
        //}

        //m_canvas.worldCamera = GameObject.Find("EventCamera").GetComponent<Camera>();

        //m_canvas = m_canvasObj.GetComponent<Canvas>(); // == this.gameObject.GetComponent<Canvas>()


        //Debug.Log("RenderMode of Canvas in Start()=");
        //Debug.Log(m_canvas.renderMode);

        m_canvas.targetDisplay = m_canvas.worldCamera.targetDisplay;

        Debug.Log("Target Display of Canvas in Start()=");
        Debug.Log(m_canvas.targetDisplay);


        Debug.Log("Target Display of the Event Camera of canvas in Start()=");
        Debug.Log(m_canvas.worldCamera.targetDisplay);

        // m_canvas.worldCamera.targetDisplay = 1; 
        // targetDisplay is index starting from 0; different from that of the inspector

        ////Debug.Log("Changed Target Display of the Event Camera=");
        ////Debug.Log(m_canvas.worldCamera.targetDisplay);


        //Debug.Log("Scale Factor of Canvas in Start()=");
        //Debug.Log(m_canvas.scaleFactor);

        //// m_canvas.targetDisplay = 2;



        ////Debug.Log("Changed Target Display of Canvas [Screen Space-Overlay mode]=");
        ////Debug.Log(m_canvas.targetDisplay);

        //Debug.Log("  canvas resolution in Start()=");
        //Debug.Log(m_canvas.pixelRect);



        //m_canvasRectTransform = m_canvas.GetComponent<RectTransform>(); // used in onGUI() uses display 1 (primary display)

        //m_guiCamera = m_canvas.worldCamera;

        //m_mainCamera = Camera.main;

        //// m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ////gameObject.AddComponent<CanvasScaler>();
        ////gameObject.AddComponent<GraphicRaycaster>();= 


        //// get the distance from the _guiCamera to the plane of the canvas
        ////  m_distanceToMain = Vector3.Distance(m_guiCamera.gameObject.transform.position, m_canvas.gameObject.transform.position);
        ////m_distanceToCamera = Vector3.Distance(m_guiCamera.transform.position, m_canvas.transform.position);

        //m_distanceToCamera = m_canvas.planeDistance;
        //// For simplicity it is assumed that plane distance is set so that the z coordinates of the plane is zero
        //// relative to the event camera.

        //// Every component is attached to a gameobject which has a transform component by default; UI component has also a transform component

        //// move canvas to position in front of main camera
        ////m_canvas.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * m_distanceToCamera);

        //// get the camera height at the frustum range- if it's orthographic, it's constant, so that's easy

        //// By default the aspect ratio is automatically calculated from the screen's aspect ratio,
        ////even if the camera is not rendering to full area. If you modify the aspect ratio of the camera, 
        ////the value will stay until you call camera.ResetAspect(); which resets the aspect to the screen's aspect ratio.



        //MyIO.DebugLog("The Screen (Gameview, Display 1) in Start()=");
        //MyIO.DebugLog(Screen.width + "x" + Screen.height);

        //MyIO.DebugLog("The aspect of the Screen (Gameview, Display 1) in Start()=");
        //MyIO.DebugLog((float)Screen.width / (float)Screen.height);



        //MyIO.DebugLog("The aspect of the main camera in Start()=");
        //MyIO.DebugLog(m_mainCamera.aspect);


        //MyIO.DebugLog("The aspect of the canvas screen in Start()=");
        //MyIO.DebugLog((float)m_canvas.pixelRect.width / (float)m_canvas.pixelRect.height);


        //MyIO.DebugLog("The aspect of the event camera in Start()=");
        //MyIO.DebugLog(m_guiCamera.aspect);

        //Debug.Log("Screen Current Resolution (The native best resolution) in Start()=");
        //Debug.Log(Screen.currentResolution);






        // SetupActionPlanUILocAndSize();
    }

    //Call this function externally to set the text of the template and activate the tooltip
    //public void SetTooltip(string ttext)
    //    {

    //        if (_guiMode == RenderMode.ScreenSpaceCamera)
    //        {
    //            //set the text and fit the tooltip panel to the text size
    //            _text.text = ttext;

    //            _rectTransform.sizeDelta = new Vector2(_text.preferredWidth + 40f, _text.preferredHeight + 25f);

    //            OnScreenSpaceCamera();

    //        }
    //    }

    //    //call this function on mouse exit to deactivate the template
    //    public void HideTooltip()
    //    {
    //        if (_guiMode == RenderMode.ScreenSpaceCamera)
    //        {
    //            this.gameObject.SetActive(false);
    //            _inside = false;
    //        }
    //    }

    //    // Update is called once per frame
    //    void FixedUpdate()
    //    {
    //        if (_inside)
    //        {
    //            if (_guiMode == RenderMode.ScreenSpaceCamera)
    //            {
    //                OnScreenSpaceCamera();
    //            }
    //        }
    //    }

    //main tooltip edge of screen guard and movement
    //    public void OnScreenSpaceCamera()
    //    {
    //        //Viewport space is normalized and relative to the camera. The bottom-left of the viewport is (0,0); the top-right is (1,1). 
    //        //The z position is in world units from the camera.
    //        Vector3 newPos = m_guiCamera.ScreenToViewportPoint(Input.mousePosition );
    //        Vector3 newPosWVP = m_guiCamera.ViewportToWorldPoint(newPos);

    //        width = _rectTransform.sizeDelta[0];
    //        height = _rectTransform.sizeDelta[1];

    //        // check and solve problems for the tooltip that goes out of the screen on the horizontal axis
    //        float val;

    //        Vector3 lowerLeft = _guiCamera.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f));
    //        Vector3 upperRight = _guiCamera.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, 0.0f));

    //        //check for right edge of screen
    //        val = (newPosWVP.x + width / 2);
    //        if (val > upperRight.x)
    //        {
    //            Vector3 shifter = new Vector3(val - upperRight.x, 0f, 0f);
    //            Vector3 newWorldPos = new Vector3(newPosWVP.x - shifter.x, newPos.y, 0f);
    //            newPos.x = _guiCamera.WorldToViewportPoint(newWorldPos).x;
    //        }
    //        //check for left edge of screen
    //        val = (newPosWVP.x - width / 2);
    //        if (val < lowerLeft.x)
    //        {
    //            Vector3 shifter = new Vector3(lowerLeft.x - val, 0f, 0f);
    //            Vector3 newWorldPos = new Vector3(newPosWVP.x + shifter.x, newPos.y, 0f);
    //            newPos.x = _guiCamera.WorldToViewportPoint(newWorldPos).x;
    //        }

    //        // check and solve problems for the tooltip that goes out of the screen on the vertical axis

    //        //check for upper edge of the screen
    //        val = (newPosWVP.y + height / 2);
    //        if (val > upperRight.y)
    //        {
    //            Vector3 shifter = new Vector3(0f, 35f + height / 2, 0f);
    //            Vector3 newWorldPos = new Vector3(newPos.x, newPosWVP.y - shifter.y, 0f);
    //            newPos.y = _guiCamera.WorldToViewportPoint(newWorldPos).y;
    //        }

    //        //check for lower edge of the screen (if the shifts of the tooltip are kept as in this code,
    //          no need for this as the tooltip always appears above the mouse bu default)
    //        val = (newPosWVP.y - height / 2);
    //        if (val < lowerLeft.y)
    //        {
    //            Vector3 shifter = new Vector3(0f, 35f + height / 2, 0f);
    //            Vector3 newWorldPos = new Vector3(newPos.x, newPosWVP.y + shifter.y, 0f);
    //            newPos.y = _guiCamera.WorldToViewportPoint(newWorldPos).y;
    //        }

    //        this.transform.position = new Vector3(newPosWVP.x, newPosWVP.y, 0f);
    //        this.gameObject.SetActive(true);
    //        _inside = true;
    //    }
    //}



    //(1) Awake versus Start: The difference between Awake and Start is that Start is only called if the script instance is enabled.
    //This allows you to delay any initialization code,
    // until it is really needed.Awake is always called before any Start functions. This allows you to order initialization of scripts
    //Regarding Awake and Start, I treat Awake as "initialize me" and Start as "initialize my connections to others." 

    //You can't rely on the state of other objects during Awake, since they may not have Awoken themselves yet, 
    //so it's not safe to call their methods. Once you get to Start, all objects are Awake and can call each other.

    // (2)  Transform vs RectTransform:
    //        Transform is a component for defining a GameObject's relative position, rotation, and scale. 
    //        The parent, and zero or more child objects are also described here.
    //         Most objects added to a scene get this assigned automatically.

    //        RectTransform is a Transform, but with extra info for managing an object as a UI element.
    //         So, some aspects might be constrained(greyed out) by parent objects.This includes 
    //         information for managing a relative pivot and anchor, for example.
    //         You can also use the T mode in the Unity scene view to adjust things 
    //         with a RectTransform, which can make it nice to edit UI edges and anchors.


    //Objects added to the scene through the Unity menu creating a UI object will have RectTransform automatically, 
    //as will any empty objects added as children to that object.

    // (3) Create a Text UI element: Debugging to check the difference between RectTransform.localPosition and RectTransform.anchoredPosition
    //  The local position, as the origin of the gameobject relative to the parent frame, is fixed to the center by default (povit X, Y=0.5)
    // But the anchoredPosition is the position of the pivot relative to the anchors which may change, so has a different value when 
    // the anchor changes. But when the anchor is set to the center of the parent (which is the default), 
    // the anchored position is the same as the local position.

    // The reference frame of the parent canvas is at the center of the screen, and the pivot of the canvas is at this center
    // The origin of the UI element is specified with respect to the reference frame of its parent. 
    //https://stackoverflow.com/questions/35531980/how-to-create-and-place-input-fields-on-a-canvas-via-script
    //https://answers.unity.com/questions/842610/instantiating-ui-elements-in-46.html?childToView=953022#answer-953022
    //https://stackoverflow.com/questions/41194515/unity-create-ui-control-from-script

   
    void InitActionPlan()
    {

        //Initialize me:

        m_actionPlan = new Dictionary<String, List<Action>>()
        {

             //{ "_BoidsNum", new List<Action> {   new Action() { T = new List<float> {0, 390 },
             //                                                   V = 1500 },
             //                                 }
             //},

              { "_SpeedFactor", new List<Action> { new Action() { T =new List<float>{ 0, 30}, V  = 0.7f  },
                                                    new Action() { T = new List<float>{30, 140 }, V = 0.9f},

                                                 new Action() { T = new List<float>{140, 200 }, V =0.6f },
                                                new Action() { T = new List<float>{200, 390 }, V =0.8f  },

                                                 }
             },

              { "_ScaleFactor", new List<Action> {  new Action() { T  = new List<float>{0,  390 },  V =1f },

                                                  }
             },

              { "_SeparateRadius", new List<Action> {   new Action() { T=new List<float> {0, 30 }, V= 2f },

                                                    new Action() { T = new List<float>{30, 60 }, V = 0.5f },
                                                        new Action() { T = new List<float>{60, 100 }, V = 0.9f },
                                                            new Action() { T = new List<float>{100, 140 }, V = 1.2f  },

                                                new Action() { T = new List<float>{140,  230 }, V =4.4f},
                                                 new Action() { T= new List<float>{230, 260 }, V = 0.5f},
                                                  new Action() { T = new List<float>{260,  300 }, V = 4.4f },

                                                new Action() { T =new List<float> {300, 360 }, V =2f  },
                                                    new Action() { T = new List<float>{360, 390 }, V = 0.5f  },


                                               }
             },

              { "_SeparateWeight", new List<Action> {  new Action() { T=new List<float> {0, 30 }, V = 0.43f },

                                                    new Action() { T =new List<float> {30, 60 }, V = 0.1f },
                                                        new Action() { T =new List<float>{ 60, 100 }, V = 0.3f},
                                                            new Action() { T =new List<float>{ 100, 140 }, V =0.2f },

                                                    new Action() { T = new List<float>{140, 230 }, V = 0.3f },
                                                            new Action() { T =new List<float>{ 230, 260 },  V =0.1f },
                                                                new Action() { T =new List<float> {260, 300 }, V = 0.3f },

                                                    new Action() { T =new List<float> {300, 360 }, V = 0.1f  },
                                                        new Action() { T = new List<float>{360, 390 }, V = 0.8f },

                                               }
             },

              { "_AlignmentRadius", new List<Action> {  new Action() { T =new List<float> {0, 30 }, V = 2f},

                                                    new Action() { T= new List<float>{30, 60 }, V= 2.8f },
                                                        new Action() { T= new List<float>{60, 100 }, V = 3.5f },
                                                            new Action() { T = new List<float>{100, 140 }, V = 1.7f},

                                                    new Action() { T =new List<float>{ 140, 200 }, V = 1.8f },

                                                        new Action() { T = new List<float>{200, 230 }, V = 3.2f},
                                                            new Action() { T = new List<float>{230, 260 }, V =2.8f },
                                                                new Action() { T =new List<float>{ 260, 300 }, V = 1.8f  },

                                                    new Action() { T = new List<float>{300, 360 }, V =4.0f},
                                                        new Action() { T = new List<float>{360, 390 }, V =2.1f },

                                               }
             },

              { "_AlignmentWeight", new List<Action> {  new Action() { T = new List<float>{0, 30 }, V =  0.3f },

                                                    new Action() { T =  new List<float>{30, 60 }, V =  0.6f  },
                                                        new Action() { T =  new List<float>{60, 140 }, V=  0.3f},

                                                    new Action() { T =  new List<float>{140, 230 }, V= 0.1f  },

                                                            new Action() { T =  new List<float>{230, 260 }, V = 0.6f },
                                                                new Action() { T =  new List<float>{260, 300 }, V = 0.8f },

                                                    new Action() { T =  new List<float>{300, 360 }, V =  0.8f },
                                                        new Action() { T =  new List<float>{360, 390 }, V = 0.6f },


                                               }
             },

              { "_CohesionRadius", new List<Action> {  new Action() { T =  new List<float>{0, 30 }, V = 0.2f },

                                                        new Action() { T= new List<float>{30, 60 }, V= 1.2f },
                                                            new Action() {T= new List<float>{60, 140 }, V = 0.1f },

                                                    new Action() { T =  new List<float>{140, 230 }, V =  2.9f  },
                                                     new Action() { T =  new List<float>{230, 300 }, V =  1.1f },
                                                   new Action() { T =  new List<float>{300, 360 }, V =  2f },
                                                        new Action() { T = new List<float>{ 360, 390 }, V = 1.1f},



                                               }
             },

              { "_CohesionWeight", new List<Action> {

                  new Action() { T  = new List<float>{ 0, 30 }, V = 0.2f },

                    new Action() { T= new List<float> {30, 60 }, V = 0.02f },
                        new Action() { T=  new List<float>{60, 100 }, V = 0.4f },
                            new Action() { T =  new List<float>{100,140 },  V = 0.1f },

                new Action() { T =  new List<float>{140, 230 }, V = 0.4f },
                         new Action() { T=  new List<float>{230, 260 }, V = 0.1f },
                            new Action() { T =  new List<float>{260,300 }, V =  0.7f  },

                new Action() { T=  new List<float>{300, 360 }, V = 0.2f },
                    new Action() { T=  new List<float>{360, 390 }, V = 0.1f },

                                               }
             },



            //
            { "_GroundFlockingWeight", new List<Action> {

                new Action() { T = new List<float>{ 0, 30 }, V = 0.3f },

                    new Action() { T =  new List<float>{30, 60 }, V = 0.5f },
                        new Action() { T = new List<float>{ 60, 100 }, V = 1.4f},
                            new Action() { T= new List<float> {100,140 }, V=  0.4f},

                new Action() { T = new List<float> {140,200 }, V =  0.2f },

                    new Action() { T =  new List<float>{200,230 }, V =  1.9f },
                        new Action() { T =  new List<float>{230, 260 },  V = 0.5f },
                            new Action() { T = new List<float>{ 260, 300 }, V =  1.8f },

                new Action() { T=  new List<float>{300,360 },  V =  0.3f },
                    new Action() { T=  new List<float>{360, 390 }, V = 0.5f  },

                                               }
             },

            { "_GroundDivergeWeight", new List<Action> {

                new Action() { T = new List<float>{ 0,30 },  V =  0.2f  },

                    new Action() { T =  new List<float>{30,60 },  V = 0.5f },
                        new Action() { T= new List<float>{ 60,100 }, V =  0.5f },
                            new Action() { T =  new List<float>{100,140 },  V = 0.5f },

                new Action() { T =  new List<float>{140, 200},  V = 0.3f  },

                    new Action() { T =  new List<float>{200,230 },  V = 0.7f },
                        new Action() { T=  new List<float>{230,260 }, V =  0.1f },
                            new Action() { T =  new List<float>{260,300 },  V = 0.1f },

                new Action() { T =  new List<float>{300, 360 }, V =   0.7f },
                    new Action() { T =  new List<float>{360, 390 }, V =0.1f },

                                               }
             },

            { "_GroundCirculationWeight", new List<Action> {

                new Action() { T =  new List<float>{0, 30 },  V = 0.5f },

                    new Action() { T =  new List<float>{30,60 },  V = 0.2f  },
                        new Action() { T =  new List<float>{60, 100 },  V = 0.3f },
                            new Action() { T =  new List<float>{100, 140 }, V = 0.3f  },
                new Action() { T =  new List<float>{140,200 },  V =  0.5f  },
                    new Action() { T=  new List<float>{200, 230 },  V = 0.3f  },
                        new Action() { T = new List<float> {230, 260 },  V = 0.2f  },
                            new Action() { T=  new List<float>{260,300 },  V = 0.3f },

                new Action() { T = new List<float> {300, 360 }, V =0.1f  },
                    new Action() { T=  new List<float>{360, 390 }, V = 0.2f},

                                               }
             },


            { "_CeilingFlockingWeight", new List<Action> {

                new Action() { T = new List<float> {0, 30 }, V = 0.2f },

                    new Action() { T =  new List<float>{30,60 }, V = 3.4f },
                        new Action() { T =  new List<float>{60, 100 }, V =  4.8f  },
                            new Action() { T =  new List<float>{100, 140 }, V = 1.0f   },

                new Action() { T =  new List<float>{ 140, 200 },  V=  3.0f   },
                    new Action() { T =  new List<float>{200, 230 }, V =  3.9f   },
                        new Action() { T = new List<float> {230,260 }, V = 1.2f },
                            new Action() { T=  new List<float>{260, 300 }, V =  1.0f },

                new Action() { T = new List<float> {300, 360 }, V = 0.2f },
                    new Action() { T=  new List<float>{360, 390 }, V = 1.2f  },



                                               }
             },

            { "_CeilingConvergeWeight", new List<Action> {

                new Action() { T =  new List<float>{0, 30 },  V = 0.3f  },

                    new Action() { T= new List<float> {30, 60 }, V =  0.5f   },
                        new Action() { T =  new List<float>{60, 140 },  V = 0.5f   },


                 new Action() { T = new List<float>{ 140, 200 }, V = 0.3f  },
                    new Action() { T = new List<float> {200, 230 },  V =  0.6f  },
                        new Action() { T=  new List<float>{230, 260 },  V =  0.3f },
                            new Action() { T=  new List<float>{260,300 },  V = 0.1f },

                new Action() { T = new List<float> {300, 360 }, V = 0.3f },
                    new Action() { T = new List<float> {360, 390 }, V = 0.3f },


                                               }
             },

            { "_CeilingCirculationWeight", new List<Action> {

                new Action() { T=  new List<float>{0, 30},  V =  0.5f   },

                    new Action() { T =  new List<float>{30, 60 }, V = 0.2f  },
                        new Action() { T = new List<float> {60, 100 }, V =  0.3f },
                            new Action() { T=  new List<float>{100f,140 },  V = 0.2f },

                new Action() { T=  new List<float>{140, 200 },  V =0.5f },

                    new Action() { T=  new List<float>{200,230 },  V =0.1f },
                        new Action() { T = new List<float> {230f,260 },  V = 0.1f },
                            new Action() { T = new List<float> {260, 300 }, V =0.2f  },

                new Action() { T =  new List<float>{300, 360 }, V =  0.5f },
                    new Action() { T =  new List<float>{360,390 }, V =   0.1f  },

                                               }
             },




            //
            { "_GroundMinHue", new List<Action> {

                new Action() { T = new List<float>{ 0, 60 }, V =  0f},
                       new Action() { T =  new List<float>{60,100 },  V = 0f},
                            new Action() { T =  new List<float>{100, 140 }, V = 0.0f },

                new Action() { T =  new List<float>{140,200 }, V =  0.0f },

                    new Action() { T =  new List<float>{200,300 },  V = 0.0f },
                      new Action() { T=  new List<float>{300, 360 }, V = 0.0f},
                    new Action() { T =  new List<float>{360, 390 }, V = 0.0f },


                                               }
             },

            { "_GroundMaxHue", new List<Action> {

                new Action() { T = new List<float> {0, 60}, V = 180f },
                    new Action() { T = new List<float> {60,100 },  V = 180f},
                         new Action() { T = new List<float> {100,140 },  V = 180f },

                new Action() { T = new List<float> {140,200},  V =180f  },

                    new Action() { T =  new List<float>{200, 230 }, V = 180f  },
                        new Action() { T=  new List<float>{230, 300 }, V =  180f },

                new Action() { T =  new List<float>{300, 360 },  V =180f },
                    new Action() { T =  new List<float>{360, 390 },  V =180f },

                                               }
             },

            { "_GroundMinSaturation", new List<Action> {

                new Action() { T =  new List<float>{ 0, 140 }, V = 0f },
                    new Action() { T =  new List<float>{140,200 },  V = 0f },

                    new Action() { T =  new List<float>{200, 230 }, V = 0.0f },
                        new Action() { T =  new List<float>{230, 300 }, V = 0.0f   },

                new Action() { T =  new List<float>{300, 360 },  V =  0.0f },
                    new Action() { T =  new List<float>{360,390 }, V =  0f },

                                               }
             },

            { "_GroundMaxSaturation", new List<Action> {

                new Action() { T =  new List<float>{0,140 },  V =  0.5f },

                new Action() { T =  new List<float>{140,230 },  V =  0.5f },
                       new Action() { T = new List<float> {230, 300 },  V =0.5f },

                new Action() { T = new List<float> {300, 360 }, V = 0.5f  },
                    new Action() { T =  new List<float>{360, 390 }, V = 0.5f},

                                               }
             },

            { "_GroundMinValue", new List<Action> {

                new Action() { T = new List<float> {0,140 }, V =0.0f},

                new Action() { T=  new List<float>{140,230 },  V =  0.0f },

                new Action() { T =  new List<float>{230, 360 }, V = 0.0f },
                    new Action() { T =  new List<float>{360,390 }, V =0.0f },

                                               }
             },

            { "_GroundMaxValue", new List<Action> {

                                     new Action() { T =  new List<float>{0,390 },  V =  0.5f },
                                 }
             },

        { "_GroundMinAlpha", new List<Action> {

                new Action() { T =  new List<float>{0, 140 }, V = 0.2f },

                new Action() { T =  new List<float>{140,230 }, V=  0.3f },
                    new Action() { T = new List<float>{230, 300 },  V =1.0f},

                new Action() { T =  new List<float>{300, 390 }, V =   0.2f },

                                               }
             },

            { "_GroundMaxAlpha", new List<Action> {

                new Action() { T =  new List<float>{0, 140 }, V =  0.8f },

                new Action() { T = new List<float> {140,230 }, V =  0.9f  },
                    new Action() { T =  new List<float>{230, 300 }, V = 1.0f},

                new Action() { T =  new List<float>{300, 360 }, V=   0.4f},
                    new Action() { T=  new List<float>{360, 390 },  V =  0.8f },


                                               }
             },
                                                  


            //
            { "_CeilingMinHue", new List<Action> {

                new Action() { T =  new List<float>{0, 30 }, V =  180f },

                    new Action() { T =  new List<float>{30, 60 }, V = 180f } ,
                        new Action() { T =  new List<float>{60, 100 }, V =  180f },
                            new Action() { T =  new List<float>{100, 140 }, V =180f },

                new Action() { T =  new List<float>{140, 230 }, V = 180f },
                         new Action() { T =  new List<float>{230,300 },  V = 180f },

                new Action() { T=  new List<float>{300, 360 }, V=  180f },
                    new Action() { T =  new List<float>{360,390 }, V =  180f },


                                               }
             },

            { "_CeilingMaxHue", new List<Action> {

                new Action() { T = new List<float> {0, 30f }, V =  360f },

                    new Action() { T=  new List<float>{30,60 },  V =360f },
                        new Action() { T= new List<float> {60,100 },  V = 360f },
                            new Action() { T=  new List<float>{100,140 },  V = 360f } ,

                new Action() { T =  new List<float>{140, 230 }, V =360f },

                        new Action() { T=  new List<float>{230, 390 }, V=   360f },

                      }
             },

            { "_CeilingMinSaturation", new List<Action> {

                new Action() { T =  new List<float>{0, 230 }, V = 0.5f  },
                    new Action() { T =  new List<float>{230, 360 }, V =  0.5f },

                    new Action() { T =  new List<float>{360,390 }, V =  0.5f },

                    }
             },

            { "_CeilingMaxSaturation", new List<Action> {

                new Action() { T=  new List<float>{0, 140 }, V =   1.0f  },

                new Action() { T=  new List<float>{140,300 }, V = 1f },

                new Action() { T = new List<float> {300,360 }, V =  1.0f  },
                    new Action() { T = new List<float> {360, 390 }, V = 1.0f },


                }
             },

            { "_CeilingMinValue", new List<Action> {

                new Action() { T = new List<float> {0, 140 }, V = 0.5f },

                new Action() { T =  new List<float>{140,230 },  V = 0.5f },
                    new Action() { T=  new List<float>{230, 360 }, V =0.5f } ,

                        new Action() { T =  new List<float>{360,390 }, V = 0.5f },


                 }
             },

            { "_CeilingMaxValue", new List<Action> {

                new Action() { T = new List<float> {0,390 }, V=  1.0f   },

                }
             },

            { "_CeilingMinAlpha", new List<Action> {

                new Action() { T =  new List<float>{ 0, 390 }, V = 0.2f  },

                }
             },

            { "_CeilingMaxAlpha", new List<Action> {

                new Action() { T =  new List<float>{0, 140 }, V = 0.8f  },

                new Action() { T = new List<float> {140,230 },  V =  0.6f },
                    new Action() { T =  new List<float>{230, 300 }, V =0.8f },


                new Action() { T= new List<float> {300,360 }, V =  0.8f },
                    new Action() { T =  new List<float>{360,390 }, V =0.8f  },

                }
             },

           

            { "_SamplingRadius", new List<Action> {

                new Action() { T =  new List<float>{0, 140 }, V = 1.0f  },

                new Action() { T = new List<float> {140,230 },  V =  1f },
                    new Action() { T =  new List<float>{230, 300 }, V =1.0f },


                new Action() { T= new List<float> {300,360 }, V =  1.0f },
                    new Action() { T =  new List<float>{360,390 }, V =1.0f  },

                }
             },

             //m_boids.DetermineParamValue("_LEDChainHeight", currTime, ref m_LEDChainHeight);
       // m_boids.DetermineParamValue("_SphericalMotion", currTime, ref m_boids.m_SphericalMotion);
       { "_HemisphereGroundPosition", new List<Action> {
           // positive or zero => the upper hemisphere above the _HemisphereGroundPosition
           // negative => the lower hemisphere below the _HemisphereGroundPosition

                new Action() { T =  new List<float>{0, 140 }, V = 0.0f  }, // 

                new Action() { T = new List<float> {140,230 },  V =  0.0f },
                    new Action() { T =  new List<float>{230, 300 }, V =-0.1f },


                new Action() { T= new List<float> {300,360 }, V =  -0.1f },
                    new Action() { T =  new List<float>{360,390 }, V = -0.1f  },

                }
             },

     

    }; //    actionPlan = new Dictionary<String, List<Action> >  ()

    }
    GameObject CreateText(Transform canvas_transform, float x, float y, string text_to_print, int font_size, Color text_color)
    {
        GameObject UItextGO = new GameObject("Text2");
        UItextGO.transform.SetParent(canvas_transform);

        RectTransform trans = UItextGO.AddComponent<RectTransform>();
        trans.anchoredPosition = new Vector2(x, y);

        Text text = UItextGO.AddComponent<Text>();
        text.text = text_to_print;
        text.fontSize = font_size;
        text.color = text_color;

        return UItextGO;
    }



    //TextGenerator textGen = new TextGenerator();
    //TextGenerationSettings generationSettings = _textComponent.GetGenerationSettings(_textComponent.rectTransform.rect.size);
    //float textWidth = textGen.GetPreferredWidth(_textComponent.text, generationSettings);
    //float textHeight = textGen.GetPreferredHeight(_textComponent.text, generationSettings);

    //Debug.Log(_textComponent.text + ":");
    //Debug.Log("preferred text size =");
    //Debug.Log(textWidth + "x" + textHeight);


    private void OnValidate()
    {
        ////Inspector Debug
        //Debug.Log(" AnchoredPosition of Content Obj:");

        //if (m_contentObj == null) return;

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().anchoredPosition);

        //Debug.Log(" LocalPosition of Content Obj:");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().localPosition );


        //Debug.Log(" global Position of Content Obj:");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().position); 

        //Debug.Log(" sizeDeltaof Content Obj:");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().sizeDelta);

        //Debug.Log(" center of rect of Content Obj:");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().rect.center);

        //Debug.Log(" size of rect of Content Obj:");

        //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().rect.size);


    }

    float lastValueHorizontal;
    void HorizontalScrollbarCallBack(float value)
    {

        ////cL.worldCamera = yourCamera;
        ////https://jinsdevlog.tistory.com/27
        ////유니티 WorldSpace Position에서 Canvas Position으로 전환하기
        //UnityEngine.Debug.Log("Curr  Hori Scrolling Value: " + value);

        //if (lastValueHorizontal > value)
        //{
        //    UnityEngine.Debug.Log("Hori Scrolling UP: " + value);

        //    Vector3 localPos = m_contentObj.GetComponent<RectTransform>().localPosition;
        //    localPos[0] += value * m_canvasWidth;
        //    m_contentObj.GetComponent<RectTransform>().localPosition = new Vector3(localPos[0], localPos[1], localPos[2]);
            

        //}
        //else
        //{
        //    UnityEngine.Debug.Log("Hori Scrolling DOWN: " + value);

        //    Vector3 localPos = m_contentObj.GetComponent<RectTransform>().localPosition;
        //    localPos[0] -= value * m_canvasWidth;
        //    m_contentObj.GetComponent<RectTransform>().localPosition = new Vector3(localPos[0], localPos[1], localPos[2]);

        //}
        //lastValueHorizontal = value;
    }

   
    void SetupActionFileButtons()    
    {
        Debug.Log("I am in Setup ActionFileButtons");




        // Define save Buttons:
        // The first child of Canvas Obj is ScrollView, the second Save Button, and the third Load Button
        m_saveButtonObj = m_canvasObj.transform.GetChild(1).gameObject;

        // GameObject saveButtonTextObj = saveButtonObj.transform.GetChild(0).gameObject;

        //  GameObject saveButtonObj = new GameObject("Action Save Button");
        //  GameObject saveButtonTextObj = new GameObject("Action  Button Text");

        //  saveButtonObj.transform.SetParent(m_canvasObj.transform, false);

        //  saveButtonTextObj.transform.SetParent(saveButtonObj.transform, false);

        //  saveButtonObj.AddComponent<RectTransform>();
        //  saveButtonObj.AddComponent<CanvasRenderer>(); // CanvasRenderer component is used to render Button
        //  saveButtonObj.AddComponent<Button>();
        //  saveButtonObj.layer = 5;

        //// save the saveButton to used by other components and places
        ///

        m_saveButton = m_saveButtonObj.GetComponent<Button>(); // // m_loadButton will be referred to CommHub.cs

        //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
        //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
        //saveButtonObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        //saveButtonObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        //saveButtonObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        //anchoredPosition:   The position of the left top pivot of this RectTransform relative to the left top anchor reference point.
        // m_timeTopBarContainer[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);


        //  Set the pivot of this Rect ( m_timeTopBarContainer[0] Rect) relative to the anchor frame 
        //  which is the left top of the ContentTitle Rect
        //  saveButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(m_paramTextWidth, m_canvasHeight-m_paramTextHeight, 0.0f);

        m_saveButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -(m_canvasHeight - 2 * m_paramTextHeight), 0.0f);
        //saveButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(m_paramTextWidth, m_canvasHeight-m_paramTextHeight, 0.0f);

        m_saveButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, 2 * m_paramTextHeight);
        //NOTE:

        //m_scrollRectWidth = m_canvasWidth;
        //m_scrollRectHeight = m_canvasHeight - (int)m_paramTextHeight;
        //// The scrollView contains the viewport and the horizotal and vertical bar; 
        //// Make space for the save and load buttons just below the scrollView
        //// within the canvas => Make the vertical size of the scrollView shorter.




        //saveButtonTextObj.layer = 5;
        //saveButtonTextObj.AddComponent<RectTransform>();
        ////m_textContainer[i][j].AddComponent<CanvasRenderer>();  // // canvasRender is added automatically

        //saveButtonTextObj.AddComponent<Text>();
        //saveButtonTextObj.AddComponent<CanvasRenderer>();

        //saveButtonTextObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        //saveButtonTextObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        //saveButtonTextObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        //saveButtonTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
        //// the center pivot position wrt its left top frame 
        //saveButtonTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, m_paramTextHeight);


        String saveButtonLablel = "Save Action Plan";

        //// set the properties of the textComponent Field of the input field
        //saveButtonTextObj.GetComponent<Text>().supportRichText = false;
        //saveButtonTextObj.GetComponent<Text>().color = new Color(0f, 0f, 0f);

        ////Font ArialFontField = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        //// text.font = ArialFont;
        //// text.material = ArialFont.material;


        //saveButtonTextObj.GetComponent<Text>().font = ArialFont;
        //saveButtonTextObj.GetComponent<Text>().material = ArialFont.material;

        //saveButtonTextObj.GetComponent<Text>().fontSize = 12;
        //saveButtonTextObj.GetComponent<Text>().lineSpacing = 1;
        //saveButtonTextObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        //saveButtonTextObj.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
        //saveButtonTextObj.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;

        //// _textComponentField.horizontalOverflow = HorizontalWrapMode.Wrap;
        //// _textComponentField.verticalOverflow = VerticalWrapMode.Truncate;

        //saveButtonTextObj.GetComponent<Text>().text = saveButtonLablel;

        m_saveButtonObj.GetComponentInChildren<Text>().text = saveButtonLablel;

        //saveButtonTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
        //// the center pivot position wrt its left top frame 
        //saveButtonTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, m_paramTextHeight);


        // Define load Buttons:


        //GameObject loadButtonObj = new GameObject("Load Save Button");
        //GameObject loadButtonTextObj = new GameObject("Action  Button Text");

        // The first child of Canvas Obj is ScrollView, the second Save Button, and the third Load Button

         m_loadButtonObj = m_canvasObj.transform.GetChild(2).gameObject;

        // GameObject loadButtonTextObj = loadButtonObj.transform.GetChild(0).gameObject;

        //loadButtonObj.transform.SetParent(m_canvasObj.transform, false);

        //loadButtonTextObj.transform.SetParent(loadButtonObj.transform, false);

        //loadButtonObj.AddComponent<RectTransform>();
        //loadButtonObj.AddComponent<CanvasRenderer>();
        //loadButtonObj.AddComponent<Button>();
        //loadButtonObj.layer = 5;

        //// save the loadButton to used by other components and places
        m_loadButton = m_loadButtonObj.GetComponent<Button>(); // m_loadButton will be referred to CommHub.cs

        ////anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
        ////anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
        //loadButtonObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        //loadButtonObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        //loadButtonObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        ////anchoredPosition:   The position of the left top pivot of this RectTransform relative to the left top anchor reference point.
        //// m_timeTopBarContainer[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);


        ////  Set the pivot of this Rect ( m_timeTopBarContainer[0] Rect) relative to the anchor frame which is the left top of the ContentTitle Rect
        m_loadButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(m_paramTextWidth, -(m_canvasHeight - 2 * m_paramTextHeight), 0.0f);
        m_loadButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, 2 * m_paramTextHeight);


        //loadButtonTextObj.layer = 5;
        //loadButtonTextObj.AddComponent<RectTransform>();
        ////m_textContainer[i][j].AddComponent<CanvasRenderer>();  // // canvasRender is added automatically

        //loadButtonTextObj.AddComponent<Text>();

        //loadButtonTextObj.AddComponent<CanvasRenderer>();
        //loadButtonTextObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        //loadButtonTextObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        //loadButtonTextObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        //loadButtonTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
        //// the center pivot position wrt its left top frame 
        //loadButtonTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, m_paramTextHeight);


        String loadButtonLablel = "Load Action Plan";

        //// set the properties of the textComponent Field of the input field
        //loadButtonTextObj.GetComponent<Text>().supportRichText = false;
        //loadButtonTextObj.GetComponent<Text>().color = new Color(0f, 0f, 0f);

        ////Font ArialFontField = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        //// text.font = ArialFont;
        //// text.material = ArialFont.material;


        //loadButtonTextObj.GetComponent<Text>().font = ArialFont;
        //loadButtonTextObj.GetComponent<Text>().material = ArialFont.material;

        //loadButtonTextObj.GetComponent<Text>().fontSize = 12;
        //loadButtonTextObj.GetComponent<Text>().lineSpacing = 1;
        //loadButtonTextObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        //loadButtonTextObj.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
        //loadButtonTextObj.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;

        //// _textComponentField.horizontalOverflow = HorizontalWrapMode.Wrap;
        //// _textComponentField.verticalOverflow = VerticalWrapMode.Truncate;

        //loadButtonTextObj.GetComponent<Text>().text = loadButtonLablel;

        m_loadButtonObj.GetComponentInChildren<Text>().text = loadButtonLablel;


        //loadButtonTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
        //// the center pivot position wrt its left top frame 
        //loadButtonTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, m_paramTextHeight);


        // GameObject testButtonObj = m_canvasObj.transform.GetChild(3).gameObject;

        //GameObject testButtonTextObj = loadButtonObj.transform.GetChild(0).gameObject;

        // testButtonObj.GetComponentInChildren<Text>().text = "test button";

        //testButtonTextObj.GetComponent<Text>().text = "test button";


    }//   void SetupActionFileButtons()    


// https://stackoverflow.com/questions/53005040/what-i-have-learned-about-unity-scrollrect-scrollview-optimization-performan
//https://wergia.tistory.com/25 ScrollView 사용법
void SetupActionPlanUILocAndSize()
{
 
    //"initialize my connections to other scripts which have been initialized by their own Awake()

    // Check if global components are defined
    m_boidsController = this.gameObject.GetComponent<SimpleBoidsTreeOfVoice>();

    if (m_boidsController == null)
    {
        Debug.LogError("The component SimpleBoidsTreeOfVoice should be added to CommHub");
        //Application.Quit();

    }
    // m_actionPlan = m_boidsController.m_actionPlan; // m_actionPlan is created in this script

    m_AnimationCycle = m_boidsController.m_AnimationCycle;


    // set the sizes of the canvas, the scrollRect, and the content Rect

    m_canvasHeight = (int)m_canvas.pixelRect.height;

    //The pixel size of the canvas is set to the resolution of the game view,
    //  which is the target display of the Event Camera (Screen Space-Camera mode);
    // We will change the canvas mode to WorldSpace later. But we will use this
    // canvas size to set the size of the scrollView

    m_canvasWidth = (int)m_canvas.pixelRect.width;


        //m_scrollRectWidth = m_canvasWidth;
        //m_scrollRectHeight = m_canvasHeight;

        float scrollBarHorizontalHeight = m_scrollbarHorizontalObj.GetComponent<RectTransform>().sizeDelta[1];
    float scrollBarVerticalWidth = m_scrollbarVerticalObj.GetComponent<RectTransform>().sizeDelta[0];

    m_contentWidth = (int)(m_AnimationCycle / 10 * m_timeTextWidth + m_paramTextWidth);

    // The content is displayed on the scroll view rect which includes the view port, the horizontal scrollbar, and the 
    // vertial scroll bar at the bottom most part and the right most part of the scroll view. The right most part of 
    // the content will be occluded by the vertical scroll bar. So, the extra empty area is added to the content panel.

    m_contentWidth = m_contentWidth + (int)(scrollBarVerticalWidth / 2);

    // m_AnimationCycle is the duration in which a single animation clip is specified. After that, the animation is 
    // repeated, although the boids motion and therey the color distribution would not the same in each repetition.
    // Every m_AnimationCycle/10, the time values are displayed, the interval of which is regarded as 10 seconds)

    // There are m_actionPlan.Count + 1 lines in the action plan, the first of which is the time line
    m_contentHeight = (int)(m_paramTextHeight * (m_actionPlan.Count + 1));

    // m_contentHeight = m_contentHeight + (int) scrollBarHorizontalHeight;

    // The content height may be smaller than that of the scrollRect. 
    // This causes the disabling of scrolling => ScrollRect component is able to handle this case

    // Set the geometries of the canvas are internally defined: the reference is at the left bottom of the game view screen, and 
    // the pivot of the canvas is at the center. 
    //  All the other child rects are set so that the anchors are at the left top and the pivot is at the left top.
    //  This makes it easier to locate UI elements.

    // This is not needed because the size of the canvas is already given.
    // m_canvasWidth = (int)m_canvas.pixelRect.width;

    m_canvasObj.GetComponent<RectTransform>().sizeDelta
                = new Vector2(m_canvasWidth, m_canvasHeight);

       

    // Make the size of the canvas (to which the Event Camera's view volume is defined) equal to the size of the
    // content panel onto which the contents of the UI are specified. The part of which defined by the viewport
    // will be displayed on the screen

    // m_canvasObj.GetComponent<RectTransform>().localPosition =  new Vector3(0,0,0);
    //  m_canvasObj.GetComponent<RectTransform>().sizeDelta
    //                            = new Vector2(m_contentWidth, m_contentHeight);
    // This does not change the size of the  canvas in Screen Space mode. It is fixed to the gameview screen size at the beginning.
    // To use the custom size canvas, use "World Space" canvas mode: https://forum.unity.com/threads/setting-custom-canvas-size.263788/



    // Set the Event Camera so that its view volume covers the entire content panel

    // The canvas for the event camera is located at z =0; The size of the canvas is the same as the size of
    // the world in which boids move. The event camera for the canvas is set in UISetActionPlan.cs

    //m_canvasObj.GetComponent<RectTransform>().transform.localScale)

    Camera eventCamera;

    // Get the reference to the Event Camera
    eventCamera = GameObject.Find("EventCamera").GetComponent<Camera>();


    Debug.Log(eventCamera.name + ": camera pos=");
    Debug.Log(eventCamera.transform.position);

    Vector3 targetPos = m_canvas.GetComponent<RectTransform>().position;


    Debug.Log("camera targetPos=");
    Debug.Log(targetPos);


    Debug.Log("computed target vector:");

    Vector3 vecToTarget = targetPos - eventCamera.transform.position;

    Debug.Log(vecToTarget);

    float canvasWidth = m_canvas.GetComponent<RectTransform>().sizeDelta[0];
    float canvasHeight = m_canvas.GetComponent<RectTransform>().sizeDelta[1];

    float heightOfFOV = canvasHeight;
    float fieldOfView = 2.0f * Mathf.Rad2Deg *
                         Mathf.Atan(heightOfFOV / vecToTarget.magnitude);


    Debug.Log("computed field of view:");
    Debug.Log(fieldOfView);

    Debug.Log("computed aspect (width/height):");

    float aspect = canvasWidth / canvasHeight;
    Debug.Log(aspect);


    // Reset the field of view and aspect of the Event Camera
    //eventCamera.fieldOfView = fieldOfView;
    //eventCamera.aspect = aspect;

    // Change the canvas mode to WorldSpace

    // m_canvas.renderMode = RenderMode.WorldSpace; // RenderMode enum def in UnityEngine
    // m_canvas.worldCamera = eventCamera;


    //c.TargetDisplay
    //Debug.Log("Target Display (index starting from 0)of the Event Camera:");
    //Debug.Log(c.targetDisplay);

    //Debug.Log(" Resolution=");
    //Debug.Log(Display.displays[c.targetDisplay].systemWidth + "x" + Display.displays[c.targetDisplay].systemHeight);

    // When the anchors are at the same place, .sizeDelta is equal to .size

    // Set the anchors and the size of the scroll view rect:

    //m_scrollViewObj.GetComponent<RectTransform>().anchorMin = new Vector2(0,1); // (0,1) is defined relative to the parent rect
    //m_scrollViewObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    // set the relationship of the pivot  with respect to the whole rect

    //m_scrollViewObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

    // set the achored position of the pivot of the scrollview relative to the left top anchor
    //m_scrollViewObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

    // set the size of the rect [displayed relative to the position of the pivot
    // ScrollView: center/middle, pivot = (0.5,0.5), anchoredPosition =(0,0)

    //http://www.aclockworkberry.com/managing-screen-resolution-and-aspect-ratio-in-unity-3d/

    //m_scrollRectWidth = Display.displays[c.targetDisplay].systemWidth;
    //m_scrollRectHeight = Display.displays[c.targetDisplay].systemHeight;


    m_scrollRectWidth = m_canvasWidth;
    m_scrollRectHeight = m_canvasHeight - 2 * (int)m_paramTextHeight;
    //m_scrollRectHeight = m_canvasHeight;

    // The scrollView contains the viewport and the horizotal and vertical bar; 
    // Make space for the save and load buttons just below the scrollView
    // within the canvas => Make the vertical size of the scrollView shorter.

    //m_scrollRectHeight = m_scrollRectHeight - (int) scrollBarHorizontalHeight;
    //m_scrollRectWidth = m_scrollRectWidth - (int) scrollBarVerticalWidth;


    // Set the parent rect (canvas) as the left top anchor frame
    m_scrollViewObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_scrollViewObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_scrollViewObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1); // use the left top corner as the pivot of the child rect

    //  Set the pivot of this Rect (ScrollView Rect) relative to the anchor frame which is the left top of the canvas rect
    m_scrollViewObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);


    m_scrollViewObj.GetComponent<RectTransform>().sizeDelta
               = new Vector2(m_scrollRectWidth, m_scrollRectHeight);

    // The RectTransform of viewport is specified automatically as the stretch/stretch relative to the parent, ScrollView rect

    //Set the pivot of this Rect relative to the  parent frame which is the center of the Canvas Rect
    // 
    //m_scrollViewObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);


    //Anything beyond the viewport's dimensions should be cutoff by the mask.

    // Set the anchors and the pivots of  the horizontal scrollbar, the vertical scrollbar,  and the viewport relative to the ScrollView


    //float scrollBarHorizontalHeight = m_scrollbarHorizontalObj.GetComponent<RectTransform>().sizeDelta[1];
    //float scrollBarVerticalWidth = m_scrollbarVerticalObj.GetComponent<RectTransform>().sizeDelta[0];


    //// set the achored position of the pivot and the size of the horizontal scrollbar
    //m_scrollbarHorizontalObj.GetComponent<RectTransform>().anchoredPosition 
    //                   = new Vector3(scrollBarVerticalWidth, -(m_scrollRectHeight - scrollBarHorizontalHeight), 0);

    //m_scrollbarHorizontalObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_scrollRectWidth- scrollBarVerticalWidth, 
    //                                                                                scrollBarHorizontalHeight);


    ////the vertical scrollbar
    //m_scrollbarVerticalObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1); // (0,1) is defined relative to the parent rect
    //m_scrollbarVerticalObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    //// set the relationship of the pivot with respect to this  rect

    //m_scrollbarVerticalObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

    // set the achored position of the pivot and the size of the vertical scrollbar
    //m_scrollbarVerticalObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

    //m_scrollbarVerticalObj.GetComponent<RectTransform>().sizeDelta = new Vector2( scrollBarVerticalWidth, 
    //                                                                              m_scrollRectHeight - scrollBarHorizontalHeight);



    ////the viewport scrollbar
    //m_viewportObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1); // (0,1) is defined relative to the parent rect
    //m_viewportObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    //// set the relationship of the pivot with respect to the this rect

    //m_viewportObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

    //// set the anchored position of the pivot and the size of the viewport
    //m_viewportObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(scrollBarVerticalWidth, 0, 0);

    //m_viewportObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_scrollRectWidth - scrollBarVerticalWidth, m_scrollRectHeight - scrollBarHorizontalHeight);



    //https://stackoverflow.com/questions/43736079/position-of-rect-transform-in-unity-ui
    //        When positioning a Rect Transform it’s useful to first determine it has or should have any stretching behavior or not.
    //            Stretching behavior happens when the anchorMin and anchorMax properties are not identical.
    //        (1) For a non - stretching Rect Transform, the position is set most easily by setting the anchoredPosition 
    //    and the sizeDelta properties.
    //            The anchoredPosition specifies the position of the pivot relative to the anchors.
    //    The sizeDelta is just the same   as the size when there’s no stretching.
    //        (2) For a stretching Rect Transform, it can be simpler to set the position using the offsetMin and offsetMax properties.
    //            The offsetMin property specifies the corner of the lower left corner of the rect relative to the lower left anchor. 
    //            The offsetMax property specifies the corner of the upper right corner of the rect relative to the upper right
    // Set the anchors of the content Rect relative its parent Rect, which is ScrollRect;
    // The anchor is set to the top left corner of the parent

    //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
    //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.

    // m_contentObj.GetComponent<RectTransform>().anchorMin = new Vector2(0,1);
    //m_contentObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    // set the relationship of the pivot position with respect to the whole rect

    //m_contentObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

    //anchoredPosition:   The position of the pivot of this RectTransform relative to the anchor reference point.
    // m_contentObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);


    //Debug.Log("OffsetMin  the Canvas Rect=");
    //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().offsetMin);
    //Debug.Log("OffsetMax  the Canvas Rect=");
    //MyIO.DebugLog(m_contentObj.GetComponent<RectTransform>().offsetMax);


    // set the anchors and the pivot of the content
    m_contentObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1); // (0,1) is defined relative to the parent rect
    m_contentObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    // set the relationship of the pivot with respect to  this rect

    m_contentObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

    // set the anchored position of the pivot and the size of the content

    m_contentObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
    m_contentObj.GetComponent<RectTransform>().sizeDelta
                               = new Vector2(m_contentWidth, m_contentHeight);


    // Set the parent rect (viewport) as the left top anchor frame
    m_contentTitleObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_contentTitleObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_contentTitleObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1); // use the left top corner as the pivot of the child rect

    //  Set the pivot of this Rect (contentTitle Rect) relative to the anchor frame which is the left top of the viewport
    m_contentTitleObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

    m_contentTitleObj.GetComponent<RectTransform>().sizeDelta
                               = new Vector2(m_paramTextWidth, m_paramTextHeight);

    //m_contentTitleObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);


    // m_contentTitleObj.transform.SetParent(m_viewportObj.transform, false);

    // m_contentTitleObj.transform.SetParent(m_viewportObj.transform, true);


    // Set the parent rect (viewport) as the left top anchor frame
    m_contentTimeLineClipRectObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_contentTimeLineClipRectObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_contentTimeLineClipRectObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1); // use the left top corner as the pivot of the child rect

    //  Set the pivot of this Rect (contentTimeLine Rect) relative to the anchor frame which is the left top of the viewport
    m_contentTimeLineClipRectObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(m_paramTextWidth, 0, 0);
    m_contentTimeLineClipRectObj.GetComponent<RectTransform>().sizeDelta
                               = new Vector2(m_scrollRectWidth - m_paramTextWidth, m_paramTextHeight);

    // Set the parent rect (viewport) as the left top anchor frame
    m_contentTimeLineObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_contentTimeLineObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_contentTimeLineObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1); // use the left top corner as the pivot of the child rect

    //  Set the pivot of this Rect (contentTimeLine Rect) relative to the anchor frame which is the left top of the viewport
    m_contentTimeLineObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
    m_contentTimeLineObj.GetComponent<RectTransform>().sizeDelta
                               = new Vector2(m_contentWidth - m_paramTextWidth, m_paramTextHeight);


    // m_contentTimeLineObj.GetComponent<RectTransform>().localPosition = new Vector3(m_paramTextWidth, 0, 0);
    // m_contentTimeLineObj.transform.SetParent(m_viewportObj.transform, false);
    // m_contentTimeLineObj.transform.SetParent(m_viewportObj.transform, true);


    //  Set the anchors and pivot of this Rect (contentKeysClipRect) relative to the anchor frame which is the left top of the viewport
    m_contentKeysClipRectObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_contentKeysClipRectObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_contentKeysClipRectObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1); // use the left top corner as the pivot of the child rect

    //  Set the pivot location of this Rect (contentKeysClipRect) relative to the anchor frame
    m_contentKeysClipRectObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -m_paramTextHeight, 0);
    m_contentKeysClipRectObj.GetComponent<RectTransform>().sizeDelta
                               = new Vector2(m_paramTextWidth, m_scrollRectHeight - m_paramTextHeight);



    // //  Set the anchors and pivot of this Rect (contentKeysClipRect) relative to the anchor frame
    //which is the left top of the (contentKeysClipRect)
    m_contentKeysObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_contentKeysObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_contentKeysObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1); // use the left top corner as the pivot of the child rect

    //  Set the pivot location of this Rect (contentKeys Rect) relative to the anchor frame which is the left top of the parent,  contentKeysClipRect
    m_contentKeysObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
    m_contentKeysObj.GetComponent<RectTransform>().sizeDelta
                               = new Vector2(m_paramTextWidth, m_contentHeight - m_paramTextHeight);




    //RectTransform m_ContentKeysRectTransform = m_contentKeysObj.GetComponent<RectTransform>();

    // m_contentKeysObj.GetComponent<RectTransform>().localPosition = new Vector3(0, -m_paramTextHeight, 0);

    //m_contentKeysObj.transform.SetParent(m_viewportObj.transform, false);
    //m_contentKeysObj.transform.SetParent(m_viewportObj.transform, true);


    //  //  Set the pivot of this Rect (contentValuesClip  Rect) relative to the anchor frame which is the left top of the viewport
    m_contentValuesClipRectObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_contentValuesClipRectObj.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_contentValuesClipRectObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1); // use the left top corner as the pivot of the child rect

    //  Set the pivot of this Rect (contentValuesClip  Rect) relative to the anchor frame which is the left top of the viewport
    m_contentValuesClipRectObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(m_paramTextWidth, -m_paramTextHeight, 0);
    m_contentValuesClipRectObj.GetComponent<RectTransform>().sizeDelta
                               = new Vector2(m_scrollRectWidth - m_paramTextWidth, m_scrollRectHeight - m_paramTextHeight);

    // Set the anchors and pivot of this RectTransform  as the left top anchor frame, ContentValuesClipRect
    m_contentValuesObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_contentValuesObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_contentValuesObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1); // use the left top corner as the pivot of the child rect

    //  Set the pivot location of this Rect (contentValues  Rect) relative to the anchor frame which is the left top of the ContentValuesClipRect
    m_contentValuesObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
    m_contentValuesObj.GetComponent<RectTransform>().sizeDelta
                               = new Vector2(m_contentWidth - m_paramTextWidth, m_contentHeight - m_paramTextHeight);



    // m_contentValuesObj.GetComponent<RectTransform>().localPosition = new Vector3(m_paramTextWidth, -m_paramTextHeight, 0);
    //m_contentValuesObj.transform.SetParent(m_viewportObj.transform, false);
    //m_contentValuesObj.transform.SetParent(m_viewportObj.transform, true);

    //Enable The canvas renderer of each content panel to clip its content within the specified RectTransform

    //When a Rect Mask 2D component is added to a GameObject, the visibility of its children will be affected by the shape of its Rect Transform. 
    //    An Image component is not required on the parent object for the Rect Mask 2D to function.

    //WOW https://forum.kerbalspaceprogram.com/index.php?/topic/151354-unity-ui-creation-tutorial/
    //https://books.google.co.kr/books?id=oPBZDwAAQBAJ&pg=PA264&lpg=PA264&dq=rectmask2d+example&source=bl&ots=e8DLqG10Aw&sig=ACfU3U2Ffo4O6xfsvCYqQm-Kf8xEcGpjJA&hl=ko&sa=X&ved=2ahUKEwibwYKy6qXlAhVK7WEKHSFuDeUQ6AEwCXoECAgQAQ#v=onepage&q=rectmask2d%20example&f=false


    //CanvasRenderer titleCanvasRenderer = m_contentTitleObj.GetComponent<CanvasRenderer>();
    //CanvasRenderer timeLineCanvasRenderer = m_contentTimeLineObj.GetComponent<CanvasRenderer>();
    //CanvasRenderer keysCanvasRenderer = m_contentKeysObj.GetComponent<CanvasRenderer>();
    //CanvasRenderer valuesCanvasRenderer = m_contentValuesObj.GetComponent<CanvasRenderer>();

    // m_viewportCanvasRenderer = m_viewportObj.GetComponent<CanvasRenderer>();
    //Make sure to use a RectMask2D
    //https://paulmarrington.github.io/Unity-Documentation/UI/
    //https://m.blog.naver.com/PostView.nhn?blogId=yoohee2018&logNo=220865589779&proxyReferer=https%3A%2F%2Fwww.google.com%2F

    //https://learning.oreilly.com/library/view/mastering-ui-development/9781787125520/e45da41a-f063-4f1c-a4d9-611d1b0ccece.xhtml


    //https://forum.unity.com/threads/rectmask2d-shader-requirements.496333/

    // https://hansangeun.com/archives/90: Shader "UI/Default" 

    //void clip(float4 x)
    //{
    //    if (any(x < 0))
    //        discard;
    //}

    // ----------------------------------------------------------------------------
    // <copyright file="FastWithRectMaskSupport.shader" company="Supyrb">
    //   Copyright (c) 2017 Supyrb. All rights reserved.
    // </copyright>
    // <author>
    //   Johannes Deml
    //   send@johannesdeml.com
    // </author>
    // ----------------------------------------------------------------------------

    //        using System.Collections;
    //        using System.Collections.Generic;
    //        using UnityEngine;

    //https://forum.unity.com/threads/is-there-an-example-for-using-canvasrenderer.284575/
    //        Thanks.I searched through that code and found MaskableGraphic which uses CanvasRenderer.
    //MaskableGraphic appears to be exactly what I was looking for --a way to create arbitrary 2d shapes on a Canvas
    //and have them get masked by a mask in a parent ScrollRect.
    //      I was able to figure out how to use MaskableGraphic when I found the example here:
    //    http://docs.unity3d.com/460/Documentation/ScriptReference/UI.Graphic.html
    //http://programmersought.com/article/97051854070/;jsessionid=1F977B42CE3BD5F37D501315321E296E

    //        The[Graphic][2] class is a base class provided by the C# library of the Unity UI system, 
    //        all provided to the canvas system.Draw geometric content. The UI system C# class inherits it.
    //        Most built-in UI system drawing classes are implemented through the MaskableGraphic subclass, 
    //        which implements the IMaskable interface and can be masked.
    //The main subclasses of the Drawable class are Image and Text , which provide content that corresponds to their name.


    //        Graphic is the graphical functional base class of UGUI, which must depend on the CanvasRenderer and RectTransform components to run.
    //The built-in UI system is implemented via MaskableGraphic, which uses the IMaskabel interface and can be masked.
    //Text and Image are inherited from MaskableGraphic, ILayoutElement(and others)

    //        Canvas is a Unity component written in native code, and the canvas is responsible for its internalGeometric shapeMerge into batch, 
    //            generate appropriate rendering instructions, and send them to the Unity graphics system.
    //            These operations are done by native C++ code, which is called rebatch or batch build.
    //            When a canvas is marked as containing a geometry that needs to be re - batched, the canvas is called a dirty canvas.
    //The important role of this canvas in UGUI is to generate UI components, then generate command commands, then pass them to the GPU, 
    //and finally draw them by the GPU, which is a process. In the process of generating UI components, the layout is also included,
    //            which is where the UI is displayed, including their size.

    //        The geometry is provided by the CanvasRenderer component to the canvas
    //Batch processing is to package the qualified UI elements into a batch and let the GPU draw them out at one time.
    //From the perspective of API calls, Batch and Draw call are equivalent, but their actual meaning is different in the game engine: Batch generally refers to the Draw call after packaging.
    //Batch processing needs to meet the following criteria
    //Under the same canvas.
    //Use the same material
    //Render at the same time
    //RectTransform is coplanar(the same depth) and does not overlap.
    //Under the same parent mask(different mask will cause drawcall to increase)
    //Sub - canvas

    //        When you build an interface using the UI, all geometry is drawn in a transparent queue.Every pixel that is rasterized from a polygon is sampled, even if they are completely obscured by other opaque polygons.
    //Geometry generated by the UI system with alpha blending, drawing from back to front
    //The geometry is sampled regardless of whether it is occluded or not.

    //https://bitbucket.org/Unity-Technologies/ui/src/2019.1/
    //public class UIClipTest : MonoBehaviour
    //    {
    //        public Vector4 clipRect;
    //        public MeshRenderer meshRenderer;

    //        void Start()
    //        {
    //            meshRenderer = this.GetComponent<MeshRenderer>();
    //        }

    //        void Update()
    //        {
    //            SetColorPropertyBlock(meshRenderer);
    //        }
    //        private void SetColorPropertyBlock(Renderer renderer)
    //        {
    //            MaterialPropertyBlock properties = new MaterialPropertyBlock();
    //            properties.SetVector("_ClipRect", clipRect);
    //            renderer.SetPropertyBlock(properties);
    //        }
    //    }

    //        Shader "UI/FastWithRectMaskSupport"
    //{
    //            Properties
    //    {
    //        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
    //        _Color("Tint", Color) = (1,1,1,1)
    //    }

    //CGINCLUDE
    //# include "UnityCG.cginc"
    //# include "UnityUI.cginc"

    //fixed4 _Color;
    //    fixed4 _TextureSampleAdd;
    //float4 _ClipRect;

    //struct appdata_t
    //{
    //    float4 vertex   : POSITION;
    //        float4 color    : COLOR;
    //        float2 texcoord : TEXCOORD0;
    //    };

    //struct v2f
    //{
    //    float4 vertex   : SV_POSITION;
    //        fixed4 color    : COLOR;
    //        half2 texcoord  : TEXCOORD0;
    //        float4 worldPosition : TEXCOORD1;
    //    };

    //v2f vert(appdata_t IN)
    //{
    //    v2f OUT;
    //    OUT.worldPosition = IN.vertex;
    //    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

    //    OUT.texcoord = IN.texcoord;

    //# ifdef UNITY_HALF_TEXEL_OFFSET
    //    OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1, 1);
    //#endif

    //    OUT.color = IN.color * _Color;
    //    return OUT;
    //}

    //sampler2D _MainTex;
    //fixed4 frag(v2f IN) : SV_Target
    //    {
    //        half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
    //color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

    //# ifdef UNITY_UI_ALPHACLIP
    //clip(color.a - 0.001);
    //        #endif

    //        return color;
    //    }
    //    ENDCG

    //    SubShader
    //{
    //    Tags
    //    {
    //        "Queue" = "Transparent"
    //            "IgnoreProjector" = "True"
    //            "RenderType" = "Transparent"
    //            "PreviewType" = "Plane"
    //            "CanUseSpriteAtlas" = "True"
    //        }

    //    Cull Off
    //        Lighting Off
    //        ZWrite Off
    //        ZTest [unity_GUIZTestMode]
    //    Blend SrcAlpha OneMinusSrcAlpha

    //        Pass
    //    {
    //        CGPROGRAM
    //#pragma vertex vert
    //#pragma fragment frag
    //#pragma multi_compile __ UNITY_UI_ALPHACLIP
    //        ENDCG
    //        }
    //}
    //}

    //UnityGet2DClipping: https://www.twblogs.net/a/5d16b642bd9eee1ede0546a5

    //inline float UnityGet2DClipping(in float2 position, in float4 clipRect)
    //{
    //    float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
    //    return inside.x * inside.y;
    //}

    //step(a, x) = 0 if x < a ;
    //           1 if x >= a.

    //        Unity Technologies
    //Joined:
    //        Feb 23, 2011
    //Posts:
    //        2,269

    //_UseClipRect is used by the 2D Rect Mask and set to one when the 2D Rect Mask is enabled.
    //The UI.Mask does in fact use the Stencil and does not use those two material properties. 

    //https://www.oreilly.com/library/view/unity-2017-game/9781788392365/ef68b4b8-4945-43e9-b628-12a6e2fd7445.xhtml
    //https://forum.unity.com/threads/rectmask2d-does-not-work-when-canvas-render-mode-is-sceen-space-camera-or-world-space-2017-2-0f3.499966/
    //https://answers.unity.com/questions/1011196/create-ui-mask-from-childrens-images.html
    //The mask component takes the image directly from the Image component on the same gameObject.So, to change the image of the mask, 
    //all you have to do is set the sprite of the Image component on the parent to the sprite being used by the image component of the child.

    //transform.parent.GetComponent<Image>().sprite = GetComponent<Image>().sprite;

    // CanvasRenderer m_CanvasRenderer = m_canvasObj.GetComponent<CanvasRenderer>();

    //// Rect(float x, float y, float width, float height);
    //https://www.hallgrimgames.com/blog/2018/11/25/custom-unity-ui-meshes

    //http://t-machine.org/index.php/2016/07/09/unity3d-missingdocs-canvasrenderer-setmesh-making-it-work-mostly/

    //Rect timeLineRect = new Rect(m_viewportObj.GetComponent<RectTransform>().rect.x + m_paramTextWidth,
    //                       m_viewportObj.GetComponent<RectTransform>().rect.y +  (m_scrollRectHeight - m_paramTextHeight),
    //                        m_scrollRectWidth - m_paramTextWidth, m_paramTextHeight);
    Rect keysRect = new Rect(m_viewportObj.GetComponent<RectTransform>().rect.x,
                             m_viewportObj.GetComponent<RectTransform>().rect.y,
                             m_paramTextWidth, (m_scrollRectHeight - m_paramTextHeight));

    Rect valuesRect = new Rect(m_viewportObj.GetComponent<RectTransform>().rect.x + m_paramTextWidth,
                            m_viewportObj.GetComponent<RectTransform>().rect.y,
                             m_scrollRectWidth - m_paramTextWidth, (m_scrollRectHeight - m_paramTextHeight));

    //titleCanvasRenderer.EnableRectClipping(m_contentTitleObj.GetComponent<RectTransform>().rect);
    //timeLineCanvasRenderer.EnableRectClipping(timeLineRect);
    //keysCanvasRenderer.EnableRectClipping(keysRect);
    // valuesCanvasRenderer.EnableRectClipping(valuesRect);

    //m_CanvasRenderer.EnableRectClipping(new Rect(0,0, 10, 20) );

    //m_viewportCanvasRenderer.EnableRectClipping(keysRect);
    //m_viewportCanvasRenderer.EnableRectClipping(valuesRect);


    m_myScrollRect = m_scrollViewObj.GetComponent<MyScrollRect>();

    // Set the ViewBoundsOffSet which represent the actual viewports for the horizontal and vertical scroll, which are smaller than
    // viewport specified in the inspector

    m_myScrollRect.m_ViewBoundsOffset = new Vector2(m_paramTextWidth, m_paramTextHeight);

    // myScrollRect.content = null; // This makes the scroll view inactive. It is needed to intercept it behavior by what I write.

    // myScrollRect.horizontalScrollbar.onValueChanged.AddListener(HorizontalScrollbarCallBack);


    //https://gamedev.stackexchange.com/questions/163340/how-to-clip-what-is-outside-scroll-view-when-using-a-custom-ui-element

    //Create the time top bar whose size is Screen.width and whose time period is
    // m_AnimationCycle seconds


    m_timeTopBarContainer = new GameObject[(int)(m_AnimationCycle / 10) + 2];

    // create two  more places to use the 0th place for the header of the table "Parameter"
    // for the last time point of m_AnimationCycle

    m_timeTopBarContainer[0] = new GameObject("Parameter Title Text");

    // Set the title object as a child of the Content object

    m_timeTopBarContainer[0].transform.SetParent(m_contentTitleObj.transform, false);

    m_timeTopBarContainer[0].AddComponent<RectTransform>();
    m_timeTopBarContainer[0].AddComponent<CanvasRenderer>();
    m_timeTopBarContainer[0].layer = 5;


    // Create a Text UI element
    Text _titleTextComponent = m_timeTopBarContainer[0].AddComponent<Text>();

    _titleTextComponent.supportRichText = false;
    _titleTextComponent.color = new Color(1f, 0f, 0f);

    _titleTextComponent.font = ArialFont;
    _titleTextComponent.material = ArialFont.material;

    _titleTextComponent.fontSize = 12;
    _titleTextComponent.lineSpacing = 1;
    _titleTextComponent.alignment = TextAnchor.MiddleLeft;

    _titleTextComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
    _titleTextComponent.verticalOverflow = VerticalWrapMode.Overflow;

    //_textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
    //_textComponent.verticalOverflow = VerticalWrapMode.Truncate;

    _titleTextComponent.text = "Parameter";


    //// within the ContentTimeLine ContentTitle RectTransform      
    //m_currentLocalYPosition = -m_paramTextHeight/ 2; // shift down  by  the half of the text height for the first line

    //m_currentLocalXPosition = m_paramTextWidth / 2;  // shift to the right by the half of the text width for the first field


    m_currentLocalYPosition = 0;
    m_currentLocalXPosition = 0;




    //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
    //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
    m_timeTopBarContainer[0].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_timeTopBarContainer[0].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_timeTopBarContainer[0].GetComponent<RectTransform>().pivot = new Vector2(0, 1);
    //anchoredPosition:   The position of the left top pivot of this RectTransform relative to the left top anchor reference point.
    // m_timeTopBarContainer[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);




    //  Set the pivot of this Rect ( m_timeTopBarContainer[0] Rect) relative to the anchor frame which is the left top of the ContentTitle Rect
    m_timeTopBarContainer[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
    m_timeTopBarContainer[0].GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, m_paramTextHeight);



    // https://answers.unity.com/questions/921726/how-to-get-the-size-of-a-unityengineuitext-for-whi.html


    // Draw the time line
    //m_currentLocalYPosition = -(m_paramTextHeight) / 2;  // within the ContentTimeLine RectTransform       
    //m_currentLocalXPosition = m_timeTextWidth / 2;


    m_currentLocalYPosition = 0;

    m_currentLocalXPosition = 0;

    for (int j = 0; j <= (int)(m_AnimationCycle / 10); j++) // each time point is displaced by  10 second

    //   m_timeTextWidth is the width of "1234567890" 

    {
        // Create time points UI elements

        m_timeTopBarContainer[j + 1] = new GameObject("Time Text" + j);

        // Set the ith time point  as a child of the Content object
        m_timeTopBarContainer[j + 1].transform.SetParent(m_contentTimeLineObj.transform, false);

        m_timeTopBarContainer[j + 1].AddComponent<RectTransform>();
        m_timeTopBarContainer[j + 1].AddComponent<CanvasRenderer>();
        m_timeTopBarContainer[j + 1].layer = 5;

        // Add and Get the reference to m_timeTopBarContainer[j + 1].GetComponent<Text>()
        Text _timeTextComponent = m_timeTopBarContainer[j + 1].AddComponent<Text>();

        _timeTextComponent.supportRichText = false;
        _timeTextComponent.color = new Color(0f, 0f, 0f);

        _timeTextComponent.font = ArialFont;
        _timeTextComponent.material = ArialFont.material;

        _timeTextComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        _timeTextComponent.verticalOverflow = VerticalWrapMode.Overflow;

        //_timeTextComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        //_timeTextComponent.verticalOverflow = VerticalWrapMode.Truncate;

        _timeTextComponent.fontSize = 12;
        _timeTextComponent.lineSpacing = 1;

        _timeTextComponent.alignment = TextAnchor.MiddleLeft;

        _timeTextComponent.text = (j * 10).ToString(); // time point in seconds


        //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
        //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
        m_timeTopBarContainer[j + 1].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        m_timeTopBarContainer[j + 1].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        m_timeTopBarContainer[j + 1].GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        //anchoredPosition:   The position of the left top pivot of this RectTransform relative to the anchor left top reference point.
        //m_timeTopBarContainer[j + 1].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);


        //  Set the pivot of this Rect ( m_timeTopBarContainer[j + 1] Rect) relative to the anchor frame which is the left top of the ContentTimeLine Rect
        m_timeTopBarContainer[j + 1].GetComponent<RectTransform>().anchoredPosition = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
        m_timeTopBarContainer[j + 1].GetComponent<RectTransform>().sizeDelta
                                                = new Vector2(m_timeTextWidth, m_timeTextHeight);


        // // COORD: shift to the right for the next field on the ith line
        m_currentLocalXPosition += m_timeTextWidth;


    } //  for (int j = 0; j <= (int)(m_AnimationCycle/10); j++)              

    // Create  GameObjects for the parameter name and their timed values. 
    // UI elements should be created as Gameobjects so that they can be rendered by Canvas Renderer

    m_nameContainer = new GameObject[m_actionPlan.Count];

    m_inputFieldContainer = new List<GameObject>[m_actionPlan.Count];

    m_textContainer = new List<GameObject>[m_actionPlan.Count];
    m_placeholderContainer = new List<GameObject>[m_actionPlan.Count];


    // Draw each line of actual parameter names


    m_currentLocalYPosition = 0;
    m_currentLocalXPosition = 0;

    for (int i = 0; i < m_actionPlan.Count; i++)
    {
        // struct  KeyValuePair
        KeyValuePair<string, List<Action>> dictItem = m_actionPlan.ElementAt(i);

        // The first field (parameter name) on the ith line
        m_nameContainer[i] = new GameObject(dictItem.Key);

        m_nameContainer[i].layer = 5;
        m_nameContainer[i].AddComponent<RectTransform>();
        m_nameContainer[i].AddComponent<CanvasRenderer>();


        // Add and Get the reference to m_nameContainer[i].GetComponent<Text>()

        Text _paramTextComponent = m_nameContainer[i].AddComponent<Text>();

        // Define the properties of the Text component of the nameContainer
        _paramTextComponent.supportRichText = false;
        _paramTextComponent.color = new Color(1f, 0f, 0f);


        _paramTextComponent.font = ArialFont;
        _paramTextComponent.material = ArialFont.material;

        _paramTextComponent.fontSize = 12;
        _paramTextComponent.lineSpacing = 1;
        _paramTextComponent.alignment = TextAnchor.MiddleLeft;

        _paramTextComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        _paramTextComponent.verticalOverflow = VerticalWrapMode.Overflow;

        //_paramTextComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        //_paramTextComponent.verticalOverflow = VerticalWrapMode.Truncate;
        //  _paramTextComponent.text will be a string of the input data



        _paramTextComponent.text = dictItem.Key; // The name of the ith parameter

        // Set the ith parameter name  as a child of the Content object
        m_nameContainer[i].transform.SetParent(m_contentKeysObj.transform, false);

        //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
        //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
        m_nameContainer[i].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        m_nameContainer[i].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        m_nameContainer[i].GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        //anchoredPosition:   The position of the pivot of this RectTransform relative to the anchor reference point.
        // m_nameContainer[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        //m_nameContainer[i].transform.localPosition = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
        // Set the pivot of  m_nameContainer[i] Rect relative to the left top (anchor) of the parent contentKeys Rect
        m_nameContainer[i].GetComponent<RectTransform>().anchoredPosition
                               = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
        m_nameContainer[i].GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, m_paramTextHeight);


        // COORD: shift down  the y position for the next line
        m_currentLocalYPosition -= m_paramTextHeight;

    } // for    for (int i = 0; i < m_actionPlan.Count; i++)

    LineRenderer _lineRenderer;
    Vector3 startPoint, endPoint;

    // Create UI gameobjects for InputFields for the values of parameter along the time line
    for (int i = 0; i < m_actionPlan.Count; i++)
    {

        //Create a list of GameObjects for the ith array element of the list array

        m_inputFieldContainer[i] = new List<GameObject>();
        m_textContainer[i] = new List<GameObject>();
        m_placeholderContainer[i] = new List<GameObject>();
        // struct  KeyValuePair
        KeyValuePair<string, List<Action>> dictItem = m_actionPlan.ElementAt(i);


        for (int j = 0; j < dictItem.Value.Count; j++)
        {
            m_inputFieldContainer[i].Add(new GameObject("Input Field" + i + ":" + j));

            m_textContainer[i].Add(new GameObject("Input Field Text" + i + ":" + j));

            m_placeholderContainer[i].Add(new GameObject("Placeholder Text" + i + ":" + j));

        }

        m_inputFieldContainer[i].Add(new GameObject("Input Field" + i + ":" + dictItem.Value.Count));

    }

    //// Draw the values of each parameter along the time line


    //LineRenderer _lineRenderer;
    //Vector3 startPoint, endPoint;

    Material mat;

    m_inputFieldHashTable = new Hashtable();


    m_currentLocalYPosition = 0;
    float intervalTextWidth;

    for (int i = 0; i < m_actionPlan.Count; i++)
    {

        KeyValuePair<string, List<Action>> dictItem = m_actionPlan.ElementAt(i);

        m_currentLocalXPosition = 0; //// within the  ContentValues RectTransform 


        // Draw values of the ith parameter along the time line. 
        for (int j = 0; j < dictItem.Value.Count; j++) // { Action = {T1,T2},  V={v1.V2} } 
        {

            m_inputFieldContainer[i][j].layer = 5;
            m_inputFieldContainer[i][j].AddComponent<RectTransform>();
            // m_inputFieldContainer[i][j].AddComponent<CanvasRenderer>(); // inputField needs not canvasRenderer


            // ********Set the jth inputField Object of the ith parameter name  as a child of the ContentValue object *********

            m_inputFieldContainer[i][j].transform.SetParent(m_contentValuesObj.transform, false);


            m_inputFieldContainer[i][j].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            m_inputFieldContainer[i][j].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            m_inputFieldContainer[i][j].GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            //anchoredPosition:   The position of the pivot of this RectTransform relative to the anchor reference point.
            // m_inputFieldContainer[i][j].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            // the center pivot position wrt its left top  frame

            //  set the location and the size of the InputField container
            //  Set the pivot of this Rect ( m_inputFieldContainer[i][j] Rect) relative to the anchor frame 
            //  which is the left top of the ContentValues  Rect
            m_inputFieldContainer[i][j].GetComponent<RectTransform>().anchoredPosition
                                             = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);


            // Add the inputField to the HashTable
            // public virtual void Add(object key, object value);

            m_inputFieldHashTable.Add(m_inputFieldContainer[i][j].name, new Vector2(i, j));

            float timeInterval = (dictItem.Value[j].T[1] - dictItem.Value[j].T[0]);
            //float timePointInterval = timeInterval / 10;

            //     m_contentWidth = (int) ( m_AnimationCycle/10 * m_timeTextWidth + m_paramTextWidth);

            // float intervalTextWidth = timeInterval * ((m_canvasWidth - m_paramTextWidth) / m_AnimationCycle);
            // ==>  float intervalTextWidth = timeInterval * ((m_contentWidth - m_paramTextWidth) / m_AnimationCycle);

            intervalTextWidth = timeInterval * (m_timeTextWidth / 10);


            m_inputFieldContainer[i][j].GetComponent<RectTransform>().sizeDelta
                                                    = new Vector2(intervalTextWidth, m_valueTextHeight);

            // Add InputField Component to the inputField container
            //  

            //An Input Field is a way to make the text of a Text Control editable. 
            // Like the other interaction controls, it’s not a visible UI
            //element in itself and must be combined with one or more visual UI elements in order to be visible.

            m_inputFieldContainer[i][j].AddComponent<InputField>();


            //  Get the reference to m_inputFieldContainer[i][j].GetComponent<InputField>()
            InputField _inputField = m_inputFieldContainer[i][j].GetComponent<InputField>();

            int indexToTimedAction = j;

            //  m_inputFieldContainer[i][j].GetComponent<InputField>().onEndEdit.AddListener(

            //    val =>
            //    {
            //    //if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            //    // {
            //    //      Debug.Log("End edit on enter");
            //    OnValueInput(m_actionPlan, dictItem.Key, indexToTimedAction, _inputField);
            //    //  }
            //}
            //  );


            // your inputfield UI object has 2 children objects, one called "Placeholder" and another one called "Text" and
            //    both of them have a Text component,

            // Create two children to m_inputFieldContainer[i][j]

            // ***********Set the Text child object of the InputField UI object ***********

            m_textContainer[i][j].transform.SetParent(m_inputFieldContainer[i][j].transform, false);

            m_textContainer[i][j].layer = 5;
            m_textContainer[i][j].AddComponent<RectTransform>();
            //m_textContainer[i][j].AddComponent<CanvasRenderer>();  // // canvasRender is added automatically

            m_textContainer[i][j].AddComponent<Text>();


            m_textContainer[i][j].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            m_textContainer[i][j].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            m_textContainer[i][j].GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            m_textContainer[i][j].GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
            // the center pivot position wrt its left top frame 
            m_textContainer[i][j].GetComponent<RectTransform>().sizeDelta = new Vector2(intervalTextWidth, m_valueTextHeight);


            String _valueTextField;
            _valueTextField = dictItem.Value[j].V.ToString("F1");

            // set the properties of the textComponent Field of the input field
            m_textContainer[i][j].GetComponent<Text>().supportRichText = false;
            m_textContainer[i][j].GetComponent<Text>().color = new Color(0f, 0f, 0f);

            //Font ArialFontField = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            // text.font = ArialFont;
            // text.material = ArialFont.material;


            m_textContainer[i][j].GetComponent<Text>().font = ArialFont;
            m_textContainer[i][j].GetComponent<Text>().material = ArialFont.material;

            m_textContainer[i][j].GetComponent<Text>().fontSize = 12;
            m_textContainer[i][j].GetComponent<Text>().lineSpacing = 1;
            m_textContainer[i][j].GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            m_textContainer[i][j].GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
            m_textContainer[i][j].GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;

            // _textComponentField.horizontalOverflow = HorizontalWrapMode.Wrap;
            // _textComponentField.verticalOverflow = VerticalWrapMode.Truncate;

            m_textContainer[i][j].GetComponent<Text>().text = _valueTextField;



            //How to change placeholder text in Unity UI using script?
            //(1) gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text
            //(2) You could also downcast the `placeholder` object since Text inherits from Graphic class.
            //Something like this will also work.

            //    Graphic graphic = gameObject.GetComponent<InputField>().placeholder;
            //((Text) graphic).text = "Hello";

            // Placeholder  is an optional ‘empty’ graphic to show that InputField text field is empty.
            // Note that this ‘empty' graphic still displays  even when the InputField is selected
            // (that is; when there is focus on it).
            //   A placeholder graphic can be used to show subtle hints or make it more obvious that the control is an InputField.


            //    When creating visual UI components you should inherit from this class.
            //https://answers.unity.com/questions/862069/unity-ui-461-placeholder-graphic-for-input-field-c.html



            // **********Set the placeholder child object of the InputField *************

            m_placeholderContainer[i][j].transform.SetParent(m_inputFieldContainer[i][j].transform, false);

            m_placeholderContainer[i][j].layer = 5;
            m_placeholderContainer[i][j].AddComponent<RectTransform>();
            // m_placeholderContainer[i][j].AddComponent<CanvasRenderer>(); // canvasRender is added automatically

            m_placeholderContainer[i][j].AddComponent<Text>();

            // set the properties of the textComponent Field of the input field
            m_placeholderContainer[i][j].GetComponent<Text>().supportRichText = false;
            m_placeholderContainer[i][j].GetComponent<Text>().color = new Color(0f, 0f, 0f);

            //Font ArialFontField = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            // text.font = ArialFont;
            // text.material = ArialFont.material;


            m_placeholderContainer[i][j].GetComponent<Text>().font = ArialFont;
            m_placeholderContainer[i][j].GetComponent<Text>().material = ArialFont.material;

            m_placeholderContainer[i][j].GetComponent<Text>().fontSize = 12;
            m_placeholderContainer[i][j].GetComponent<Text>().lineSpacing = 1;
            m_placeholderContainer[i][j].GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            m_placeholderContainer[i][j].GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
            m_placeholderContainer[i][j].GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;

            // _textComponentField.horizontalOverflow = HorizontalWrapMode.Wrap;
            // _textComponentField.verticalOverflow = VerticalWrapMode.Truncate;

            m_placeholderContainer[i][j].GetComponent<Text>().text = _valueTextField;


            m_placeholderContainer[i][j].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            m_placeholderContainer[i][j].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            m_placeholderContainer[i][j].GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            m_placeholderContainer[i][j].GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
            m_placeholderContainer[i][j].GetComponent<RectTransform>().sizeDelta = new Vector2(intervalTextWidth, m_valueTextHeight);

            //Set the image, text Component, and the initial value of the inputField
            // _inputField.targetGraphic = _image;

            m_inputFieldContainer[i][j].GetComponent<InputField>().targetGraphic = null;
            //Unity provides a default set of image assets. If you set the targetGraphic to null,
            //then it will take the default UI shader, which is why you will see an entirely white patch 
            //where your InputField is located.
            //https://gamedev.stackexchange.com/questions/172342/remove-or-change-graphic-of-inputfield-target-graphic/172343

            m_inputFieldContainer[i][j].GetComponent<InputField>().textComponent = m_textContainer[i][j].GetComponent<Text>();
            m_inputFieldContainer[i][j].GetComponent<InputField>().placeholder = m_placeholderContainer[i][j].GetComponent<Text>();
            // Text inherits from Graphic; upcasting is always allowed.

            // internally inputField.text also updates the InputField.textComponent's Text.

            m_inputFieldContainer[i][j].GetComponent<InputField>().text = m_textContainer[i][j].GetComponent<Text>().text;

            //because InputField itself isn't a Graphic, it's just a fancy wrapper and it needs an actual Text (it is also a Graphic)
            //  that will be drawn




            // Add line renderer and other components to the inputField container
            //  The LineRenderer in unity takes a list of points(stored as Vector3s) and draws a line through them. It does this in one of two ways.
            //  Local Space: (Default)All points are positioned relative to transform. So if your GameObject moves or rotates, the line would also move and rotate.
            //  World Space: (You would need to check the Use World Space Checkbox) The line will be rendered in a fixed position in the world that exactly matched the Positions in the list. If the gameObject moves or rotates, the line would be unchanged
            // Transform.TransformPoint
            // It takes a local space point(which is how the data is stored in the line renderer by default) and transforms it to world space.

            // Add a LineRenderer component to the InputField

            m_inputFieldContainer[i][j].AddComponent<LineRenderer>(); // LineRenderer is a component which should be added to a gameobject

            _lineRenderer = m_inputFieldContainer[i][j].GetComponent<LineRenderer>();

            //  mat = Resources.Load<Material>("Materials/LineRenderMaterial");
            // _lineRenderer.material = mat;
            //orting Layers and Order in Layer are used by the Sprite Renderer to determine the render order of sprites in a scene.
            //_lineRenderer.sortingLayerName = "OnTop";
            //_lineRenderer.sortingOrder = 5;
            _lineRenderer.positionCount = 2;
            startPoint = new Vector3(0, 0, 0); // relative to the frame of the inputField
            endPoint = new Vector3(0, -m_valueTextHeight, 0);
            _lineRenderer.SetPosition(0, startPoint); // in localspace
            _lineRenderer.SetPosition(1, endPoint);
            _lineRenderer.startWidth = 0.2f;
            _lineRenderer.endWidth = 0.2f;
            _lineRenderer.useWorldSpace = false;

            // default line material: pink color


            // _lineRenderer.material.color = Color.white;




            // COORD: shift to the right  for the next field on the same line                         
            m_currentLocalXPosition += intervalTextWidth; // move to the end of the field
                                                          // else: if (j == dictItem.Value.Count)

        }//   for (int j = 0; j < dictItem.Value.Count; j++) // { Action = {T1,T2},  V={v1.V2} } 



        int k = dictItem.Value.Count; // at the last vertical edge of the table

        m_inputFieldContainer[i][k].layer = 5;
        m_inputFieldContainer[i][k].AddComponent<RectTransform>();
        // m_inputFieldContainer[i][j].AddComponent<CanvasRenderer>(); // inputField needs not canvasRenderer


        // ********Set the last inputField Object of the ith parameter name  as a child of the ContentValues object *********

        m_inputFieldContainer[i][k].transform.SetParent(m_contentValuesObj.transform, false);

        m_inputFieldContainer[i][k].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        m_inputFieldContainer[i][k].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        m_inputFieldContainer[i][k].GetComponent<RectTransform>().pivot = new Vector2(0, 1);

        m_inputFieldContainer[i][k].GetComponent<RectTransform>().anchoredPosition
                                         = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);

        intervalTextWidth = m_timeTextWidth;
        // Add a LineRenderer component to the InputField

        m_inputFieldContainer[i][k].GetComponent<RectTransform>().sizeDelta
                                                = new Vector2(intervalTextWidth, m_valueTextHeight);

        m_inputFieldContainer[i][k].AddComponent<LineRenderer>(); // LineRenderer is a component which should be added to a gameobject

        _lineRenderer = m_inputFieldContainer[i][k].GetComponent<LineRenderer>();

        //_lineRenderer.sortingLayerName = "OnTop";
        //_lineRenderer.sortingOrder = 5;
        _lineRenderer.positionCount = 2;
        startPoint = new Vector3(0, 0, 0); // relative to the frame of the inputField
        endPoint = new Vector3(0, -m_valueTextHeight, 0);
        _lineRenderer.SetPosition(0, startPoint);
        _lineRenderer.SetPosition(1, endPoint);
        _lineRenderer.startWidth = 0.2f;
        _lineRenderer.endWidth = 0.2f;
        _lineRenderer.useWorldSpace = false;




        // COORD: shift down  the y position for the next line
        m_currentLocalYPosition -= m_paramTextHeight;


    }// for (int i = 0; i < m_actionPlan.Count; i++)




    // Create horizontal lines  for the action plan table

    m_horizontalLines = new GameObject[m_actionPlan.Count + 1];

    // Draw the first horizontal line
    m_currentLocalYPosition = 0;
    m_currentLocalXPosition = 0;

    m_horizontalLines[0] = new GameObject("horizontal line" + 0);
    m_horizontalLines[0].layer = 5;

    // Set the line   as a child of the ContentValues  object
    m_horizontalLines[0].transform.SetParent(m_contentValuesObj.transform, false);

    m_horizontalLines[0].AddComponent<RectTransform>();

    //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
    //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
    m_horizontalLines[0].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_horizontalLines[0].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_horizontalLines[0].GetComponent<RectTransform>().pivot = new Vector2(0, 1);
    //anchoredPosition:   The position of the pivot of this RectTransform relative to the anchor reference point.
    // m_nameContainer[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

    //m_nameContainer[i].transform.localPosition = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
    // Set the pivot of  m_nameContainer[i] Rect relative to the left top (anchor) of the parent contentKeys Rect
    m_horizontalLines[0].GetComponent<RectTransform>().anchoredPosition
                           = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
    m_horizontalLines[0].GetComponent<RectTransform>().sizeDelta = new Vector2(m_contentWidth - m_paramTextWidth, m_paramTextHeight);


    m_horizontalLines[0].AddComponent<LineRenderer>(); // LineRenderer is a component which should be added to a gameobject

    _lineRenderer = m_horizontalLines[0].GetComponent<LineRenderer>();
    mat = Resources.Load<Material>("Materials/LineRenderMaterial");
    _lineRenderer.material = mat;
    _lineRenderer.material.color = Color.green;

    //_lineRenderer.sortingLayerName = "OnTop";
    //_lineRenderer.sortingOrder = 5;
    _lineRenderer.positionCount = 2;
    startPoint = new Vector3(0, 0, 0); // relative to the frame of the inputField
    endPoint = new Vector3(m_contentWidth - m_paramTextWidth, 0, 0);
    _lineRenderer.SetPosition(0, startPoint);
    _lineRenderer.SetPosition(1, endPoint);
    _lineRenderer.startWidth = 0.01f;
    _lineRenderer.endWidth = 0.01f;
    _lineRenderer.useWorldSpace = false;

    m_currentLocalYPosition -= m_paramTextHeight;


    // draw the next horizontal lines

    for (int i = 1; i <= m_actionPlan.Count; i++)
    {


        m_horizontalLines[i] = new GameObject("horizontal line" + i);
        m_horizontalLines[i].layer = 5;

        // Set the line   as a child of the ContentValues  object
        m_horizontalLines[i].transform.SetParent(m_contentValuesObj.transform, false);

        m_horizontalLines[i].AddComponent<RectTransform>();

        //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
        //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
        m_horizontalLines[i].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        m_horizontalLines[i].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        m_horizontalLines[i].GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        //anchoredPosition:   The position of the pivot of this RectTransform relative to the anchor reference point.
        // m_nameContainer[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        //m_nameContainer[i].transform.localPosition = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
        // Set the pivot of  m_nameContainer[i] Rect relative to the left top (anchor) of the parent contentKeys Rect
        m_horizontalLines[i].GetComponent<RectTransform>().anchoredPosition
                               = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
        m_horizontalLines[i].GetComponent<RectTransform>().sizeDelta = new Vector2(m_contentWidth - m_paramTextWidth, m_paramTextHeight);



        m_horizontalLines[i].AddComponent<LineRenderer>(); // LineRenderer is a component which should be added to a gameobject

        mat = Resources.Load<Material>("Materials/LineRenderMaterial");
        _lineRenderer.material = mat;
        _lineRenderer.material.color = Color.green;

        _lineRenderer = m_horizontalLines[i].GetComponent<LineRenderer>();
        //_lineRenderer.sortingLayerName = "OnTop";
        //_lineRenderer.sortingOrder = 5;
        _lineRenderer.positionCount = 2;
        startPoint = new Vector3(0, 0, 0); // relative to the frame of the inputField
        endPoint = new Vector3(m_contentWidth - m_paramTextWidth, 0, 0);
        _lineRenderer.SetPosition(0, startPoint);
        _lineRenderer.SetPosition(1, endPoint);
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.useWorldSpace = false;

        m_currentLocalYPosition -= m_paramTextHeight;


    }//         for (int i = 1; i < m_actionPlan.Count; i++
     //*** horizontal lines  ****

    // Create vertical lines for the action plan table

    m_verticalLines = new GameObject[(int)(m_AnimationCycle / 10) + 1];

    m_currentLocalYPosition = 0;

    m_currentLocalXPosition = 0;


    // Draw the first vertical line
    // Create time points UI elements

    m_verticalLines[0] = new GameObject("vertical Line" + 0);



    m_verticalLines[0].AddComponent<RectTransform>();
    // m_verticalLines[0].AddComponent<CanvasRenderer>();
    m_verticalLines[0].layer = 5; // UI layer


    // Set the jth vertical line  as a child of m_contentTimeLineClipRectObj

    m_verticalLines[0].transform.SetParent(m_contentValuesObj.transform, false);

    //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
    //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
    m_verticalLines[0].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
    m_verticalLines[0].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
    m_verticalLines[0].GetComponent<RectTransform>().pivot = new Vector2(0, 1);


    //  Set the pivot of this Rect relative to the anchor frame which is the left top of the m_contentValuesObj
    // anchoredPosition is another way to set the frame relative to the parent frame; The usual way is localPosition; Both use
    // different references; The first uses anchors and the second uses the default reference frame defined at the center of the
    // parent rect.
    m_verticalLines[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
    m_verticalLines[0].GetComponent<RectTransform>().sizeDelta
                                            = new Vector2(m_timeTextWidth, (m_contentHeight - m_paramTextHeight));


    m_verticalLines[0].AddComponent<LineRenderer>(); // LineRenderer is a component which should be added to a gameobject

    _lineRenderer = m_verticalLines[0].GetComponent<LineRenderer>();
    mat = Resources.Load<Material>("Materials/LineRenderMaterial");

    _lineRenderer.material = mat;
    _lineRenderer.material.color = Color.white;

    //_lineRenderer.sortingLayerName = "OnTop";
    //_lineRenderer.sortingOrder = 5;
    _lineRenderer.positionCount = 2;
    startPoint = new Vector3(0, 0, 0); // relative to the the gameobject (verticalLine) reference frame defined by verticalLine.anchoredPosition() 
    endPoint = new Vector3(0, -(m_contentHeight - m_paramTextHeight), 0);
    _lineRenderer.SetPosition(0, startPoint);
    _lineRenderer.SetPosition(1, endPoint);
    _lineRenderer.startWidth = 0.05f;
    _lineRenderer.endWidth = 0.05f;
    _lineRenderer.useWorldSpace = false;
    _lineRenderer.startColor = new Color(0, 0, 0);
    _lineRenderer.endColor = new Color(0, 0, 0);

    m_currentLocalXPosition += m_timeTextWidth;

    for (int j = 1; j <= (int)(m_AnimationCycle / 10); j++) // each time point is displaced by  10 second

    //   m_timeTextWidth is the width of "1234567890" 

    {
        // Create time points UI elements

        m_verticalLines[j] = new GameObject("vertical Line" + j);



        m_verticalLines[j].AddComponent<RectTransform>();
        // m_verticalLines[j].AddComponent<CanvasRenderer>();
        m_verticalLines[j].layer = 5;


        // Set the jth vertical line  as a child of m_contentTimeLineClipRectObj
        m_verticalLines[j].transform.SetParent(m_contentValuesObj.transform, false);

        //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
        //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
        m_verticalLines[j].GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        m_verticalLines[j].GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        m_verticalLines[j].GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        //anchoredPosition:   The position of the left top pivot of this RectTransform relative to the anchor left top reference point.

        //  Set the pivot of this Rect (frame) relative to  anchor reference at the left top of the parent rect, m_contentValuesObj
        // => It affects the localPosition of this Rect's pivot, which is measured from the origin of the parent coordinate system (the center
        //  of the parent rect) 
        m_verticalLines[j].GetComponent<RectTransform>().anchoredPosition = new Vector3(m_currentLocalXPosition, m_currentLocalYPosition, 0.0f);
        m_verticalLines[j].GetComponent<RectTransform>().sizeDelta
                                                = new Vector2(m_timeTextWidth, (m_contentHeight - m_paramTextHeight));



        m_verticalLines[j].AddComponent<LineRenderer>(); // LineRenderer is a component which should be added to a gameobject

        _lineRenderer = m_verticalLines[j].GetComponent<LineRenderer>();

        mat = Resources.Load<Material>("Materials/LineRenderMaterial");

        _lineRenderer.material = mat;
        _lineRenderer.material.color = Color.white;
        //_lineRenderer.sortingLayerName = "OnTop";
        //_lineRenderer.sortingOrder = 5;
        _lineRenderer.positionCount = 2;
        startPoint = new Vector3(0, 0, 0); // relative to the frame of the gameObject, m_verticalLines[j]
        endPoint = new Vector3(0, -(m_contentHeight - m_paramTextHeight), 0);
        _lineRenderer.SetPosition(0, startPoint);
        _lineRenderer.SetPosition(1, endPoint);
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.endColor = new Color(0, 0, 0);
        _lineRenderer.startColor = new Color(0, 0, 0);



        // // COORD: shift to the right for the next field on the ith line
        m_currentLocalXPosition += m_timeTextWidth;

    } //  for (int j = 1; j <= (int)(m_AnimationCycle/10); j++)
      //*** Vertical Lines***


}//   void SetupActionPlanUILocAndSize()    


void OnDestroy()
    {
        stay = false;
    }

    //https://mrbinggrae.tistory.com/86
    //This means that whatever the resolution, the Slider will always be drawn exactly with the same dimensions 
    //    from the Anchor point to the pivot point of the UI GameObject(by default, at the center), no matter how the screen is resized
    void UpdateActionPlanLayout()
    {

        float fieldHeight = m_canvasHeight / m_actionPlan.Count;

        float currentLocalXPosition = -m_canvasWidth / 2;
        float currentLocalYPosition = m_canvasHeight / 2;

        for (int i = 0; i < m_actionPlan.Count; i++)
        {
            currentLocalYPosition -= fieldHeight / 2; // The current row position

            // Add Inputfields to each key
            var item = m_actionPlan.ElementAt(i);
       
            List<ActionPlanController.Action> itemValue = item.Value;


            // Add UI components to the gameobjects just created
            for (int j = 0; j < itemValue.Count; j++) // { Action = {T1,T2},  V={v1.V2} } 
            {
                
                float currentFieldWidth = m_canvasWidth * (itemValue[j].T[1] - itemValue[j].T[0]) / m_AnimationCycle;

                currentLocalXPosition += currentFieldWidth / 2; // the current column position

                m_inputFieldContainer[i][j].transform.localPosition = new Vector3(currentLocalXPosition, currentLocalYPosition, 0.0f);

                //m_inputFieldObj1.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
                //m_inputFieldObj1.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0f);


                m_inputFieldContainer[i][j].GetComponent<RectTransform>().sizeDelta = new Vector2(currentFieldWidth, fieldHeight);
                               

            }// for (int j = 0; j < itemValue.Count; j++)
            

            i++; //increment the key index
        }// foreach (var key in keys)
               

    } //   void UpdateActionPlanLayout()



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