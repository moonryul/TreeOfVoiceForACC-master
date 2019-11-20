using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowContentValuesRectTransform : MonoBehaviour
{
    public RectTransform m_UIRectTrans;
    public MyScrollRect m_scrollRect;
    public Scrollbar m_scrollbarHorizontal, m_scrollbarVerticalTopDown;

    public Vector3 m_localPosition;
    public Vector3 m_position;

    public float m_horiValue, m_vertiValue;

    // Start is called before the first frame update
    void Start()
    {
        m_scrollRect = gameObject.GetComponentInParent<MyScrollRect>();

        m_UIRectTrans = gameObject.GetComponent<RectTransform>();

        m_scrollRect.contentValues = m_UIRectTrans;
            

        m_scrollbarHorizontal = m_scrollRect.transform.GetChild(1).gameObject.GetComponent<Scrollbar>();
        m_scrollbarVerticalTopDown = m_scrollRect.transform.GetChild(2).gameObject.GetComponent<Scrollbar>(); 

        m_localPosition = m_UIRectTrans.localPosition;
        m_position = m_UIRectTrans.position;

        m_horiValue = m_scrollbarHorizontal.value;
        m_vertiValue = m_scrollbarVerticalTopDown.value;
    }

    // Update is called once per frame
    void Update()
    {
        //m_UIRectTrans = gameObject.GetComponent<RectTransform>();

        m_localPosition = m_UIRectTrans.localPosition;
        m_position = m_UIRectTrans.position;

        m_horiValue = m_scrollbarHorizontal.value;
        m_vertiValue = m_scrollbarVerticalTopDown.value;

        //       public RectTransform contentKeys { get { return m_ContentKeys; } set { m_ContentKeys = value; } }
        //public RectTransform contentValues { get { return m_ContentValues; } set { m_ContentValues = value; } }
        m_scrollRect.contentValues = m_UIRectTrans;
        //m_scrollRect.contentKeys = m_UIRectTrans;
}
}
