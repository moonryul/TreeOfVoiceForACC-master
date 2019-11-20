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

public class ButtonUITest : MonoBehaviour
{

    float m_paramTextWidth;
    float m_paramTextHeight;


    Text m_textComponent;

    TextGenerator m_textGen;
    TextGenerationSettings m_generationSettings;

    Button m_loadButton;
    Button m_saveButton;

    public Button loadButton
       { get { return m_loadButton; }
         set {m_loadButton = value; }
        }

    public Button saveButton
    {
        get { return m_loadButton; }
        set { m_loadButton = value; }
    }

    // define Event Deletates
    public delegate void OnSaveActionPlan(Dictionary<string, List<int>> dict, string _file);
    public static OnSaveActionPlan onSaveActionPlan;

    public delegate void OnLoadActionPlan(Dictionary<string, List<int>> dict, string _file);
    public static OnLoadActionPlan onLoadActionPlan;

    public SimpleBoidsTreeOfVoice m_boidsController; // 


    public int  m_canvasWidth, m_canvasHeight; // the canvas size is set to the size of the game view screen automatically
                                         // The actuall scroll rect size is set to the size of the canvas

    public GameObject m_canvasObj, m_scrollViewObj, m_viewportObj;
   

    Canvas m_canvas;
   
    RectTransform m_canvasRectTransform; 
    float m_distanceToCamera;



    // Use Awake() to initialize field variables.
    public void Awake()
    {

        // Create a Text UI element
        m_textComponent = this.gameObject.AddComponent<Text>();

        m_textGen = new TextGenerator();
        Vector2 extents = new Vector2(0, 100);
        m_generationSettings = m_textComponent.GetGenerationSettings(extents);

        string sampleText = "_CeilingCirculationWeight";
        m_paramTextWidth = m_textGen.GetPreferredWidth(sampleText, m_generationSettings);
        m_paramTextHeight = m_textGen.GetPreferredHeight(sampleText, m_generationSettings);



        Debug.Log("CommHub:" + this.gameObject);

        m_canvasObj = this.gameObject.transform.GetChild(0).gameObject;

        Debug.Log("Canvas Obj:" + m_canvasObj);


        //m_canvas.worldCamera = GameObject.Find("EventCamera").GetComponent<Camera>();
        m_canvas = m_canvasObj.GetComponent<Canvas>(); // == this.gameObject.GetComponent<Canvas>()


        //Unity UI was an issue.Screen Space Overlay captures data from the primary display when setting up the UI on secondary. 
        //            So things get out of position and scale etc.

        //I set the Secondary Displays UI to Screenspace Camera instead.and ensured that camera 2 existed and had DISPLAY2 set.

        Debug.Log("Target Display of the Event Camera=");
        Debug.Log(m_canvas.worldCamera.targetDisplay);

        // m_canvas.worldCamera.targetDisplay = 1; 
        // targetDisplay is index starting from 0; different from that of the inspector

        //Debug.Log("Changed Target Display of the Event Camera=");
        //Debug.Log(m_canvas.worldCamera.targetDisplay);

        //https://stackoverflow.com/questions/43614662/unity-change-the-display-camera-for-the-scene-and-the-target-display-in-the-can

        Debug.Log("Screen Current Resolution (The native best resolution) =");
        Debug.Log(Screen.currentResolution);

        Debug.Log("Target Display of Canvas [Screen Space-Overlay mode]=");
        Debug.Log(m_canvas.targetDisplay);


        Debug.Log("Scale Factor of Canvas [Screen Space-Overlay mode]=");
        Debug.Log(m_canvas.scaleFactor);

        // m_canvas.targetDisplay = 2;



        //Debug.Log("Changed Target Display of Canvas [Screen Space-Overlay mode]=");
        //Debug.Log(m_canvas.targetDisplay);

        Debug.Log("[init] current canvas(screen) =");
        Debug.Log(m_canvas.pixelRect);



        m_canvasRectTransform = m_canvas.GetComponent<RectTransform>(); // used in onGUI() uses display 1 (primary display)



        m_distanceToCamera = m_canvas.planeDistance;

        MyIO.DebugLog("The Screen (Gameview) =");
        MyIO.DebugLog(Screen.width + "x" + Screen.height);

        MyIO.DebugLog("The aspect of the Screen (Gameview, Display 2)=");
        MyIO.DebugLog((float)Screen.width / (float)Screen.height);


        MyIO.DebugLog("The aspect of the Canvas (Screen Space - Camera Mode)=");
        MyIO.DebugLog(m_canvas.pixelRect);

        SetupButtonGUI();

    } // Awake()

        void SetupButtonGUI()
        {
            Debug.Log("I am in Canvas Test");



            //"initialize my connections to others, which have been initialized by their own Awake()

            // Check if global components are defined
            m_boidsController = this.gameObject.GetComponent<SimpleBoidsTreeOfVoice>();

            if (m_boidsController == null)
            {
                Debug.LogError("The component SimpleBoidsTreeOfVoice should be added to CommHub");
                //Application.Quit();

            }
            // m_actionPlan = m_boidsController.m_actionPlan; // m_actionPlan is created in this script

            float m_AnimationCycle = m_boidsController.m_AnimationCycle;


            // set the sizes of the canvas, the scrollRect, and the content Rect

            m_canvasHeight = (int)m_canvas.pixelRect.height;

            //The pixel size of the canvas is set to the resolution of the game view,
            //  which is the target display of the Event Camera (Screen Space-Camera mode);
            // We will change the canvas mode to WorldSpace later. But we will use this
            // canvas size to set the size of the scrollView

            m_canvasWidth = (int)m_canvas.pixelRect.width;

            //m_scrollRectWidth = m_canvasWidth;
            //m_scrollRectHeight = m_canvasHeight;


            // The content is displayed on the scroll view rect which includes the view port, the horizontal scrollbar, and the 
            // vertial scroll bar at the bottom most part and the right most part of the scroll view. The right most part of 
            // the content will be occluded by the vertical scroll bar. So, the extra empty area is added to the content panel.


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



            // Define save Buttons:


            //GameObject saveButtonObj = new GameObject("Action Save Button");
            //GameObject saveButtonTextObj = new GameObject("Action  Button Text");

            //saveButtonObj.transform.SetParent(m_canvasObj.transform, false);

            //saveButtonTextObj.transform.SetParent(saveButtonObj.transform, false);

            //        m_canvasObj = this.gameObject.transform.GetChild(0).gameObject;

            //GameObject saveButtonObj = m_canvasObj.transform.GetChild(1).gameObject;

            GameObject saveButtonObj = m_canvasObj.transform.GetChild(0).gameObject;

            //saveButtonObj.AddComponent<RectTransform>();
            //saveButtonObj.AddComponent<CanvasRenderer>(); // CanvasRenderer component is used to render Button
            //saveButtonObj.AddComponent<Button>();
            //saveButtonObj.layer = 5;

            // save the saveButton to used by other components and places
            m_saveButton = saveButtonObj.GetComponent<Button>();

            //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
            //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
            saveButtonObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            saveButtonObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            saveButtonObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            //anchoredPosition:   The position of the left top pivot of this RectTransform relative to the left top anchor reference point.
            // m_timeTopBarContainer[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);


            //  Set the pivot of this Rect ( m_timeTopBarContainer[0] Rect) relative to the anchor frame 
            //  which is the left top of the ContentTitle Rect
            //  saveButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(m_paramTextWidth, m_canvasHeight-m_paramTextHeight, 0.0f);

            saveButtonObj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, m_canvasHeight - m_paramTextHeight, -1);
            //saveButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(m_paramTextWidth, m_canvasHeight-m_paramTextHeight, 0.0f);

            saveButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, m_paramTextHeight);
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

            // set the properties of the textComponent Field of the input field
            //saveButtonTextObj.GetComponent<Text>().supportRichText = false;
            //saveButtonTextObj.GetComponent<Text>().color = new Color(0f, 0f, 0f);

            //Font ArialFontField = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            // text.font = ArialFont;
            // text.material = ArialFont.material;


            //saveButtonTextObj.GetComponent<Text>().font = ArialFont;
            //saveButtonTextObj.GetComponent<Text>().material = ArialFont.material;

            //saveButtonTextObj.GetComponent<Text>().fontSize = 12;
            //saveButtonTextObj.GetComponent<Text>().lineSpacing = 1;
            //saveButtonTextObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            //saveButtonTextObj.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
            //saveButtonTextObj.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;

            // _textComponentField.horizontalOverflow = HorizontalWrapMode.Wrap;
            // _textComponentField.verticalOverflow = VerticalWrapMode.Truncate;

            //saveButtonTextObj.GetComponent<Text>().text = saveButtonLablel;

            // Button has a child gameObject called "Text" which has a Text Component.

            saveButtonObj.transform.GetChild(0).gameObject.GetComponent<Text>().text = saveButtonLablel;

            // Define load Buttons:

            // GameObject loadButtonObj = m_canvasObj.transform.GetChild(2).gameObject;
            GameObject loadButtonObj = m_canvasObj.transform.GetChild(1).gameObject;


            //GameObject loadButtonObj = new GameObject("Load Save Button");
            //GameObject loadButtonTextObj = new GameObject("Action  Button Text");

            //loadButtonObj.transform.SetParent(m_canvasObj.transform, false);

            //loadButtonTextObj.transform.SetParent(loadButtonObj.transform, false);

            //loadButtonObj.AddComponent<RectTransform>();
            //loadButtonObj.AddComponent<CanvasRenderer>();
            //loadButtonObj.AddComponent<Button>();
            //loadButtonObj.layer = 5;

            // save the loadButton to used by other components and places
            m_loadButton = loadButtonObj.GetComponent<Button>();

            //anchorMin: The normalized position in the parent RectTransform that the upper right corner of this Rect is anchored to.
            //anchorMin: The normalized position in the parent RectTransform that the lower left corner of this Rect is anchored to.
            loadButtonObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            loadButtonObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            loadButtonObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            //anchoredPosition:   The position of the left top pivot of this RectTransform relative to the left top anchor reference point.
            // m_timeTopBarContainer[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);


            //  Set the pivot of this Rect ( m_timeTopBarContainer[0] Rect) relative to the anchor frame which is the left top of the ContentTitle Rect
            loadButtonObj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(m_paramTextWidth, m_canvasHeight - m_paramTextHeight, -1);

            // saveButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, m_canvasHeight - m_paramTextHeight, 0.0f);

            //loadButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, m_canvasHeight - m_paramTextHeight, 0.0f);
            loadButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_paramTextWidth, m_paramTextHeight);


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


            String loadButtonLablel = "load Action Plan";

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

            loadButtonObj.transform.GetChild(0).gameObject.GetComponent<Text>().text = loadButtonLablel;

            //loadButtonTextObj.GetComponent<Text>().text = loadButtonLablel;


        }//   void SetupButtonGUI()   

    } //class ButtonUITest

