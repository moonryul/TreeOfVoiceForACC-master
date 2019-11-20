
using UnityEngine;
using UnityEngine.EventSystems;

using System; // String class


//About Event Handling in Unity:


// Linking UnityEvents and Event Interfaces:

//https://forum.unity.com/threads/how-to-use-onpointerenter-event.294801/
//using UnityEngine;
//using System.Collections;
//using UnityEngine.EventSystems;

//class Int2Event : UnityEvent<int, int>
//{
//}

//public class PointerEventsController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    // Set those in the inspector or via AddListener exactly the same as onClick of a button
//    public UnityEvent onPointerEnter;
//    public UnityEvent onPointerExit;
//  event Action<int, int> OnClick; 

// e.g. Int2Event OnClick = new Int2Event();

// var pointerEventsController  = new PointerEventsController();
//  pointerEventsController.OnClick.AddListener((x, y) => Debug.LogFormat("clicked at {0}, {0}", x, y));

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        // evtl put some general button fucntionality here

//        onPointerEnter.Invoke(..);
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        // evtl put some general button fucntionalit here

//        onPointerExit.Invoke();
//    }
//}


//First off, let’s look at how you use C# events:
//class MyClass
//{
//    // Declare an event for users to add their listeners to
//    event Action<int, int> onClick; 
//where Action "class" (special class called delegate type)
// is defined by public delegate void Action(); onClick is a delegate instance, which is also called EventHandler, 
// which handles all the event handling methods added to this Handler.

//    void OnClick() defined within EventInterface
//    {
//        // Call Invoke() to call the listeners
//        onClick.Invoke(11, 22);

//        // Alternatively, call the event like a function
//        onClick(11, 22);
//    }
//}

//// Use += to add a listener function to be called when the event is dispatched
//var myc = new MyClass();
//myc.onClick += (x, y) => Debug.LogFormat("clicked at {0}, {0}", x, y);


//Now let’s look at Unity’s UnityEvent class:
//// Use the namespace with UnityEvent in it
//using UnityEngine.Events;

//// Make a class extending UnityEvent, mark it [Serializable]
//[Serializable]
//class Int2Event : UnityEvent<int, int>
//{
//}

//class MyClass
//{
//    // Declare and create an event for users to add their listeners to
//    Int2Event onClick = new Int2Event();

//    void OnClick(), which is supposed to be called from somewhere, e.g. From EventSystem through Interfaces; EventSystem searches for gameobjects
//    whose components defined the Foo eventHandler, a delegate.

//    {
//        // Call Invoke() to call the listeners
//        onClick.Invoke(11, 22);
//    }
//}

//// Use AddListener to add a listener function to be called when the event is dispatched
//var myc = new MyClass();
//myc.onClick.AddListener((x, y) => Debug.LogFormat("clicked at {0}, {0}", x, y));
//The strange part here is the requirement that you create your own class extending UnityEvent.
//    You can skip this if you don’t have any parameters. Otherwise, you end up making empty, 
//        [Serializable] classes for each event.
//    Other than this, using these events is very much like using C# events.

// The difference between two systems: How EventHandler (Delegate) is defined:

//event Action csharpEv0; where public delegate void Action();
//UnityEvent unityEv0 = new UnityEvent();





//https://forum.unity.com/threads/is-there-any-global-eventtrigger-callback-thats-called-when-setselectedgameobject-is-set.385921/
//Best option is to have a script, attached to the EventSystem to monitor the Selected object
//    during the update loop and when it changes, then raise an event from that script.

// EventSystem.cs to be attached to the EventSystem gameobject
//https://bitbucket.org/Unity-Technologies/ui/src/0155c39e05ca5d7dcc97d9974256ef83bc122586/UnityEngine.UI/EventSystem/EventSystem.cs?at=5.2&fileviewer=file-view-default

//https://answers.unity.com/questions/1411148/add-event-trigger-parameters-using-c.html

//https://riptutorial.com/unity3d/example/24734/user-interface-system--ui-

//    The UI components usually provide their main listener easily :
//Button : onClick
//Dropdown : onValueChanged
//InputField : onEndEdit, onValidateInput, onValueChanged
//Scrollbar : onValueChanged
//ScrollRect : onValueChanged
//Slider : onValueChanged
//Toggle : onValueChanged



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

// onDrag += eventHandlerFunc


//The Event Trigger receives events from the Event System
// and calls registered functions for each event.

//The Event Trigger can be used to specify functions you wish to be called for each Event System event.
//    You can assign multiple functions to a single event and whenever the Event Trigger receives that event
//        it will call those functions.

//Note that attaching an Event Trigger component to a GameObject
// will make that object intercept all events, and no event bubbling will occur from this object!

//Events

//Each of the Supported Events can optionally be included in the Event Trigger by clicking the Add New Event Type button.





// Supported Events:
//The Event System  supports a number of events, and they can be customised further in user custom user written Input Modules.

//The events that are supported by the Standalone Input Module and Touch Input Module are provided 
//     by interface and can be implemented on a MonoBehaviour by implementing the interface. 
//    If you have a valid Event System configured the events will be called at the correct time.
//•IPointerEnterHandler - OnPointerEnter - Called when a pointer enters the object
//•IPointerExitHandler - OnPointerExit - Called when a pointer exits the object
//•IPointerDownHandler - OnPointerDown - Called when a pointer is pressed on the object
//•IPointerUpHandler - OnPointerUp - Called when a pointer is released(called on the GameObject that the pointer is clicking)
//•IPointerClickHandler - OnPointerClick - Called when a pointer is pressed and released on the same object
//•IInitializePotentialDragHandler - OnInitializePotentialDrag - Called when a drag target is found, can be used to initialise values
//•IBeginDragHandler - OnBeginDrag - Called on the drag object when dragging is about to begin
//•IDragHandler - OnDrag - Called on the drag object when a drag is happening
//•IEndDragHandler - OnEndDrag - Called on the drag object when a drag finishes
//•IDropHandler - OnDrop - Called on the object where a drag finishes
//•IScrollHandler - OnScroll - Called when a mouse wheel scrolls
//•IUpdateSelectedHandler - OnUpdateSelected - Called on the selected object each tick
//•ISelectHandler - OnSelect - Called when the object becomes the selected object
//•IDeselectHandler - OnDeselect - Called on the selected object becomes deselected
//•IMoveHandler - OnMove - Called when a move event occurs (left, right, up, down, ect)
//•ISubmitHandler - OnSubmit - Called when the submit button is pressed
//•ICancelHandler - OnCancel - Called when the cancel button is pressed

//public class EventTriggerSample : MonoBehaviour
//{


// The Following shows how EventTrigger class is used to define events.
//https://answers.unity.com/questions/854251/how-do-you-add-an-ui-eventtrigger-by-script.html
//In researching this myself I noticed that EventTrigger will check every callback for all 12 event types, 
//regardless of whether the callback handles a specific event. I know it's only responding to user initiated UI events
//    so performance isn't critical but it still seems a poor (read inefficient) way to implement this.

//    EventTrigger trigger = GetComponentInParent<EventTrigger>();
//    EventTrigger.Entry entry = new EventTrigger.Entry();
//    entry.eventID = EventTriggerType.PointerClick;
// entry.callback.AddListener((eventData) => { Foo();
//} );
//5. trigger.delegates.Add(entry);

//Something very important that is not precised: "GetComponentInParent()" will search 
//    for a component EventTrigger so there should be an EventTrigger on the object. Click on the object, 
//    then "Add Component", then add "Event Trigger" and let it empty, but from now on your code will work.

//minor update to this answer: "delegates" has been deprecated; line 5 in the answer should now use "triggers" instead:
// trigger.triggers.Add(entry);


//    newButton.GetComponent<Button>().onClick.AddListener(()=>TestScript());



//or:
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;

//5. public class ClickableButton : MonoBehaviour, IPointerClickHandler
//    {

//        public void OnPointerClick(PointerEventData eventData)
//        {
//            if (eventData.button == PointerEventData.InputButton.Left)
//                10.Debug.Log("Left click");
//            else if (eventData.button == PointerEventData.InputButton.Middle)
//                Debug.Log("Middle click");
//            else if (eventData.button == PointerEventData.InputButton.Right)
//                Debug.Log("Right click");



//            15.     }
//    }


// void OnClicked(BaseEventData eventData)
//        {
//            PointerEventData pointerEventData = (PointerEventData)eventData;
//            Debug.Log("Click: pointerEventData=" + pointerEventData);
//            10. }



//https://forum.unity.com/threads/is-there-any-global-eventtrigger-callback-thats-called-when-setselectedgameobject-is-set.385921/
//Best option is to have a script, attached to the EventSystem to monitor the Selected object
//    during the update loop and when it changes, then raise an event from that script.

// EventSystem.cs to be attached to the EventSystem gameobject
//https://bitbucket.org/Unity-Technologies/ui/src/0155c39e05ca5d7dcc97d9974256ef83bc122586/UnityEngine.UI/EventSystem/EventSystem.cs?at=5.2&fileviewer=file-view-default

//https://answers.unity.com/questions/1411148/add-event-trigger-parameters-using-c.html

//https://riptutorial.com/unity3d/example/24734/user-interface-system--ui-

//    The UI components usually provide their main listener easily :
//Button : onClick
//Dropdown : onValueChanged
//InputField : onEndEdit, onValidateInput, onValueChanged
//Scrollbar : onValueChanged
//ScrollRect : onValueChanged
//Slider : onValueChanged
//Toggle : onValueChanged



//    ********************************************
// The First MEthod is to use EventTrigger class;
// Use the Second Method: https://stackoverflow.com/questions/30459132/unity3d-programmatically-assign-eventtrigger-handlers
//    Second method(implementation of IPointerEnterHandler and IPointerExitHandler interfaces) is what you're looking for.
//        But to trigger OnPointerEnter and OnPointerExit methods your scene must contain GameObject named "EventSystem"
//        with EventSystem-component (this GameObject created automatically when you add any UI-element to the scene, 
//            and if its not here - create it by yourself) and components for different input methods 
//        (such as StandaloneInputModule and TouchInputModule). 
//Also Canvas(your button's root object with Canvas component) must have GraphicRaycaster component to be able to detect UI-elements
//by raycasting into them.

//        ********************************


// The Second Method is alos recommended by this site:
// https://answers.unity.com/questions/879156/how-would-i-detect-a-right-click-with-an-event-tri.html


//EventTrigger.cs source code: https://github.com/tenpn/unity3d-ui/blob/master/UnityEngine.UI/EventSystem/EventTrigger.cs

//        So if you put an EventTrigger on your GameObject, it will respond to press and mouse events.
//    However, what they don't say, is that all the events the EventSystem generates come with event data 
//            that the event gets fired with. 
//            All the data from EventSystem events subclass BaseEventData. 
//If you write a function that takes a BaseEventData as a parameter and place it in a script attached to a GameObject,
//                then when you select that GameObject in the EventsTrigger component, it will appear 
//                at the top of the available function list in the editor, separate from normal functions
//                talked about here. The EventTrigger will automatically provide that function with stuff 
//                that you can then use. That's what I've done in PointerClickHandler().It handles all Pointer Click events
//                for the EventTrigger I set.
//The StandAloneInputModule, which subclasses the PointerInputModule, uses their own pointer events, 
//called the PointerEventData(a subclass of BaseEventData).The contain the good stuff, like what was last pressed,
//        how many clicks there have been and which mouse button was doing the pressing.
//The most useful things to know is that pointerPress tells you what GameObject the press was on and clickCount
//            tells you the number of successive clicks within a certain time -out.


//var sStrings = restoreplayerpos.Split(","[0]);

//float x = float.Parse(sStrings[0]);
//float y = float.Parse(sStrings[1]);
//float z = float.Parse(sStrings[2]);

//var playerObject = GameObject.Find("player");
//playerObject.transform.position = new Vector3(x, y, z);



//IEnumerator check_for_resize()
//{
//    ////m_lastScreenWidth = Screen.width;
//    ////m_lastScreenHeight = Screen.height;

//    m_lastScreenWidth = (int)m_canvas.pixelRect.width;
//    m_lastScreenHeight = (int)m_canvas.pixelRect.height;

//    Debug.Log("[new] current canvas(screen) =");
//    Debug.Log(m_canvas.pixelRect);


//    while (stay)
//    {
//        if (m_lastScreenWidth != (int)m_canvas.pixelRect.width || m_lastScreenHeight != (int)m_canvas.pixelRect.height)
//        {
//            UpdateActionPlanLayout(); // respond to the changed screen

//            //m_lastScreenWidth = Screen.width;
//            //m_lastScreenHeight = Screen.height;

//            m_lastScreenWidth = (int)m_canvas.pixelRect.width;
//            m_lastScreenHeight = (int)m_canvas.pixelRect.height;

//            //m_canvasWidth = Screen.width;
//            //m_canvasHeight = Screen.height;

//            m_canvasWidth = (int)m_canvas.pixelRect.width;
//            m_canvasHeight = (int)m_canvas.pixelRect.height;

//        }
//        yield return new WaitForSeconds(0.3f);
//        //the coroutine  activates   UpdateActionPlanLayout() if screen was resized every 0.3f seconds.
//    }
//}//  IEnumerator check_for_resize()



public class PointerEventsController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
                IBeginDragHandler, IDragHandler, IEndDragHandler,
                IPointerEnterHandler, IScrollHandler, IPointerExitHandler //,IDeselectHandler
{


    // Raycasting
    private PointerEventData m_pointerData;
    private GameObject m_RootElementUI;

    // Define delegate instances
    public event Action<PointerEventData> onScroll, onPointerEnter, onPointerExit, onPointerDown, onPointerUp, onPointerClick,
                                     onBeginDrag, onDrag, onEndDrag;

    ///In CoomHub, define:
    // PointerEventController pointerEventController;
    // pointerEventsController.onClick += (x, y) => Debug.LogFormat("clicked at {0}, {0}", x, y);
    public void Awake()
    {
        Debug.Log("I am in Awake() of PointerEventsController");
    }
    public void OnScroll(PointerEventData eventData)
    {
        Debug.Log("Scrolling");
       // onScroll.Invoke(eventData);// execute the event handler functions registered to the delegate onScroll ("event handler") by CommHub
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer Enter");
       // onPointerEnter.Invoke(eventData); // execute the event handler functions registered to the delegate  onPointerEnter ("event handler") by CommHub
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer Exit");
       // onPointerExit.Invoke(eventData);// execute the event handler functions registered to the delegate    onPointerExit ("event handler") by CommHub
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down");
       // onPointerDown.Invoke(eventData);// execute the event handler functions registered to the delegate    onPointerDown ("event handler") by CommHub

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Pointer Up");
       // onPointerUp.Invoke(eventData); // execute the event handler functions registered to the delegate    onPointerUp ("event handler") by CommHub
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        Debug.Log("Pointer Cliks");
       // onPointerClick.Invoke(eventData);// execute the event handler functions registered to the delegate    onPointerClick ("event handler") by CommHub

    } //   public void OnPointerClick(PointerEventData eventData)


    public void OnBeginDrag(PointerEventData eventData)
    {
        //this.transform.position = eventData.position;
        // check which inputField is clicked. Search over  m_inputFieldContainer[i][j]
        // public virtual void Add(object key, object value);
        //pointerDrag, delta

        Debug.Log("Dragging starts");
       // onBeginDrag.Invoke(eventData); // execute the event handler functions registered to the delegate     onBeginDrag ("event handler") by CommHub

    }


    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
       // onDrag.Invoke(eventData); // execute the event handler functions registered to the delegate     onDrag ("event handler") by CommHub



    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging ended");
       // onEndDrag.Invoke(eventData);// execute the event handler functions registered to the delegate     onEndDrag ("event handler") by CommHub


    }



} // PointerEventsController