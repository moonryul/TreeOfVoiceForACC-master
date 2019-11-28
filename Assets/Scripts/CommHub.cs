//GITHUB: https://stackoverflow.com/questions/292357/what-is-the-difference-between-git-pull-and-git-fetch

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


//https://docs.microsoft.com/ko-kr/dotnet/csharp/programming-guide/events/how-to-implement-interface-events

// UNity as a component-based framework: While classic Object Oriented Programming(OOP) can be, and is, used, the Unity workflow highly 
//    builds around the structure of components—which requires component-based thinking.
//    If you're familiar with components, that's great; if not, that's not a problem.
//    Here, I'll give you a crash course on components in Unity.


//Fortunately, since Unity was built with components in mind, it has a number of built-in functions that help us achieve this. 
// There are functions to get references to a specific component, to check all objects to see which contain 
// a specific component, etc.With these various functions, you can easily retrieve the information needed to create
// that magical one-way street of knowledge where components can communicate with objects they affect,
// but the component itself has no idea what exactly that object is. Combine this with the use of interfaces,
// and you've got enough programming power to take any approach to the matter, simple or complex.

//Each component itself is completely clueless of any other component creating complete decoupling of components.
//    Then 1 game object script controls the component interaction for that game object giving you one place to change
//    component interaction.It also gives you one place to see how a game object works from a high level which is insanely
//    handy when trying to read code and figure out how a game object works.

//Components should be isolated libraries.The shouldn't directly know about any other code structures of other components
//in my view. So how do you communicate then? You do so with events. Use c#'s EventHandler class to define events inside
//  your components.These are things that the outside world from
//  that component might want to know when something happens 
// inside the component.You can pass primitive types 
// when raising these events if need be as well.
//how do you hook up different components that belong to a game object? You create a script unique to that game object
//where all the hooks happen. I refer to this as a CommHub. 
//This script allows you to see at a high level the interactions of each component that makes 
//that game object work the way it does. It's the command center for your game object


//Your CommHub will have you getting a reference to each component attached to your game object and
// then it'll hooks up all the events. It'll look something like:

//In Start(): call actionPlanController.InitActionPlan
//pointerEventController.onDrag += actionPlanController.ChangeActionTimes;
//pointerEventController.onDrag += actionPlanController.RedrawActionPlan ==> NO, NO: all drawings are controlled by the Renderer Components such as 
// CanvasRenderer and LineRenderer. All you need to change is to change the locations and properties of the gameobjects involved.
//pointerEventController.onPointerClick += actionPlanController.MergeOrSplitActionFields;; 
//                           two right mouse clicks merge or split the fields based on the position of the mouse within the field
//inputField.onEndEdit += actionPlanController.ChangeActionValues
//neuroHeadSetController.onReceived += boidsController.ChangeColorOfElectrodeBoids
//irsensorMasterController.onReceived += boidsController.ChangeBrightnessOfInnerCircleBoids;
// In Update call boidsController.SampleColorsAtLEDPoints
// In Update call ledMasterController.SendLEDData 
// uiMenuController.onLoadActionPlan += actionPlanController.LoadActionPlan
// uiMenuController.onSaveActionPlan += actionPlanController.SaveActionPlan

 // CommHub gameObject as the command center object has the following components attached to it:
 // ActionPlanController, SimpleBoidsTreeOfVoice, BoidsRenderer, ActionPlanUpdateController, LEDColorController,
 // DictManager, ButtonEventController, PointerEventController, LEDColorGenController, NeuroHeadSetController, IRSensorMasterController,
 // LEDMasterController.

public class CommHub : MonoBehaviour
{
       
    public DisplayScriptTreeOfVoice m_displayController;

    public SimpleBoidsTreeOfVoice m_boidsController;

    public BoidRendererTreeOfVoice m_boidsRenderer;


    public ActionPlanController m_actionPlanController;
    public ActionPlanUpdateController m_actionPlanUpdateController;
  
    public ActionPlanFileManager  m_actionPlanFileManager;

    public LEDColorGenController m_LEDColorGenController;
 
    public PointerEventsController m_pointerEventsController;

    public IRSensorMasterController m_IRSensorMasterController;
    public LEDMasterController m_LEDMasterController;
    public NeuroHeadSetController m_neuroHeadSetController;


    Dictionary<String, List<ActionPlanController.Action>> m_actionPlan; //// first defined in SimpleBoidsTreeOfVoice class

    List<GameObject>[] m_inputFieldContainer;
    


    // Use Awake() to initialize field variables.
    public void Awake()
    {

        //It is assumed that all the necessary components are already attached to CommHub gameObject, which  is referred to by
        // gameObject field of this object, the current instance of the current Class.

        m_displayController = this.gameObject.GetComponent<DisplayScriptTreeOfVoice>();
        m_actionPlanController = this.gameObject.GetComponent<ActionPlanController>();
        m_actionPlanUpdateController = this.gameObject.GetComponent<ActionPlanUpdateController>();
        m_boidsController =this. gameObject.GetComponent<SimpleBoidsTreeOfVoice>();
       m_boidsRenderer = this.gameObject.GetComponent<BoidRendererTreeOfVoice>();

        m_actionPlanFileManager= this.gameObject.GetComponent<ActionPlanFileManager>();

        //debugging
        m_LEDColorGenController =this.gameObject.GetComponent<LEDColorGenController>(); // compute Shader use

        if (m_LEDColorGenController == null)
        {
            Debug.LogError("The component LEDColorGenController  should be added to CommHub");
            // Application.Quit();
        }


        m_pointerEventsController = this.gameObject.GetComponent<PointerEventsController>();
        // this  gets the reference  to the instance of  class PointerEventsController
        // The instance is automatically created (by new  PointerEventsController() ) when the component is added to the gameObject
        // The gameboject will has the reference to that instance.

        m_IRSensorMasterController = this.gameObject.GetComponent<IRSensorMasterController>();
        m_LEDMasterController = this.gameObject.GetComponent<LEDMasterController>();
        m_neuroHeadSetController =this.gameObject.GetComponent<NeuroHeadSetController>();

      
    }

    
    private void Start()
    {
        if ( m_actionPlanController == null)
        {
            Debug.LogError(" m_actionPlanController should be set before use");
            Application.Quit();
        }

        m_actionPlan = m_actionPlanController.m_actionPlan;
        m_inputFieldContainer = m_actionPlanController.m_inputFieldContainer;

        // In Start(): call actionPlanController.InitActionPlan
        m_pointerEventsController.onDrag += m_actionPlanUpdateController.ChangeActionTime;

        //pointerEventController.onDrag += actionPlanController.RedrawActionPlan ==> NO, NO: all drawings are controlled by the Renderer Components such as
        // CanvasRenderer and LineRenderer. All you need to change is to change the locations and properties of the gameobjects involved.

        m_pointerEventsController.onPointerClick += m_actionPlanUpdateController.MergeOrSplitActionField;
        //two right mouse clicks merge or split the fields based on the position of the mouse within the field

        // inputField.onEndEdit += actionPlanController.ChangeActionValues ==> This will be done within actionPlanController.initActionPlan

        //https://forum.unity.com/threads/event-trouble-value-does-not-fall-within-the-expected-range.434455/
        m_neuroHeadSetController.onAverageSignalReceived += m_LEDColorGenController.UpdateLEDResponseParameter;

        m_IRSensorMasterController.onAverageSignalReceived += m_LEDColorGenController.UpdateColorBrightnessParameter;

        //In Update call  m_boidsController.SampleColorsAtLEDPoints
        //In Update call ledMasterController.SendLEDData

        if (m_actionPlanController.loadButton is null)
        {
            Debug.LogError(" m_loadButton should be defined in actionPlanController.cs");
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            //UnityEditor.EditorApplication.isPlaying = false;
            UnityEditor.EditorApplication.Exit(0);
#else
                   Application.Quit();
#endif

        }

        else
        {
            m_actionPlanController.loadButton.onClick.AddListener(m_actionPlanFileManager.LoadActionPlan);

        }
        if (m_actionPlanController.saveButton is null )
        {
            Debug.LogError(" m_saveButton should be defined in actionPlanController.cs");
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying = false;
            UnityEditor.EditorApplication.Exit(0);
#else
                   Application.Quit();
#endif

        }
        else
        {
            m_actionPlanController.saveButton.onClick.AddListener(m_actionPlanFileManager.SaveActionPlan);
        }


     

       
                   
        //debugging
   
        m_LEDColorGenController.m_LEDSenderHandler += m_LEDMasterController.UpdateLEDArray;


        // Define Event Handlers for InputField Event          

        for (int i = 0; i < m_actionPlan.Count; i++)
        {


            KeyValuePair<string, List<ActionPlanController.Action>> dictItem = m_actionPlan.ElementAt(i);


            for (int j = 0; j < dictItem.Value.Count; j++)
            {
                //  Get the reference to m_inputFieldContainer[i][j].GetComponent<InputField>()
                InputField _inputField = m_inputFieldContainer[i][j].GetComponent<InputField>();

                int indexToTimedAction = j;

                m_inputFieldContainer[i][j].GetComponent<InputField>().onEndEdit.AddListener
                 (

                  val =>
                  {
                      //if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                      // {
                      //      Debug.Log("End edit on enter");

                      OnValueInput(m_actionPlan, dictItem.Key, indexToTimedAction, _inputField);

                      //  }
                  }
                );

            } // for j

        } // for i




        }//Start()
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






} // public class CommHub 



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