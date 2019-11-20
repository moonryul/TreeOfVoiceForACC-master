
using UnityEngine;


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
public class CanvasController : MonoBehaviour
{

    public int m_contentWidth = 10000;  // the size of the content will be set accoring to the size of the action table
    public int m_contentHeight = 2000;

    public int m_scrollRectWidth;
    public int m_scrollRectHeight;

    public int m_canvasWidth, m_canvasHeight; // the canvas size is set to the size of the game view screen automatically
                                              // The actuall scroll rect size is set to the size of the canvas



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

   

    // Use Awake() to initialize field variables.
    public void Awake()
    {



        Debug.Log("CommHub:" + this.gameObject);

        m_canvasObj = this.gameObject.transform.GetChild(0).gameObject;

        Debug.Log("Canvas Obj:" + m_canvasObj);


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

        Debug.Log("content  Obj:" + m_contentObj);

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

        Debug.Log("Target Display of the Event Camera of canvas in Awake()=");
        Debug.Log(m_canvas.worldCamera.targetDisplay);


        Debug.Log("Scale Factor of Canvas in Awake()=");
        Debug.Log(m_canvas.scaleFactor);


        Debug.Log("  canvas resolution in Awake(); This size is changed to the main gameview resolution" +
            "in Start(); You can see this in the inspector when you click the canvas object;" +
            "The primary gameview is the default space of the canvas. But when you click" +
            "the secondary gameview, then the canvas dimension changes to the secondary view dimension" +
            "So set the m_canvas size in Awake()=");

        Debug.Log(m_canvas.pixelRect);


        // set the sizes of the canvas, the scrollRect, and the content Rect

        m_canvasHeight = (int)m_canvas.pixelRect.height;

        //The pixel size of the canvas is set to the resolution of the game view,
        //  which is the target display of the Event Camera (Screen Space-Camera mode);
        // We will change the canvas mode to WorldSpace later. But we will use this
        // canvas size to set the size of the scrollView

        m_canvasWidth = (int)m_canvas.pixelRect.width;




    }// public void Awake()


    private void Start()
    {

        m_canvas.targetDisplay = m_canvas.worldCamera.targetDisplay;

        Debug.Log("Target Display of Canvas in Start()=");
        Debug.Log(m_canvas.targetDisplay);


        Debug.Log("Target Display of the Event Camera of canvas in Start()=");
        Debug.Log(m_canvas.worldCamera.targetDisplay);


        m_canvasObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_canvasWidth, m_canvasHeight);


        // set the sizes of the canvas, the scrollRect, and the content Rect

       // m_canvasHeight = (int)m_canvas.pixelRect.height;

        //The pixel size of the canvas is set to the resolution of the game view,
        //  which is the target display of the Event Camera (Screen Space-Camera mode);
        // We will change the canvas mode to WorldSpace later. But we will use this
        // canvas size to set the size of the scrollView

        //m_canvasWidth = (int)m_canvas.pixelRect.width;
    } // start()



    void Update()
    {


        m_canvas.targetDisplay = m_canvas.worldCamera.targetDisplay;

        Debug.Log("Target Display of Canvas in Start()=");
        Debug.Log(m_canvas.targetDisplay);


        Debug.Log("Target Display of the Event Camera of canvas in Start()=");
        Debug.Log(m_canvas.worldCamera.targetDisplay);


        m_canvasObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_canvasWidth, m_canvasHeight);


        // set the sizes of the canvas, the scrollRect, and the content Rect

        //m_canvasHeight = (int)m_canvas.pixelRect.height;

        //The pixel size of the canvas is set to the resolution of the game view,
        //  which is the target display of the Event Camera (Screen Space-Camera mode);
        // We will change the canvas mode to WorldSpace later. But we will use this
        // canvas size to set the size of the scrollView

       // m_canvasWidth = (int)m_canvas.pixelRect.width;

    } // Update()

} // class

   