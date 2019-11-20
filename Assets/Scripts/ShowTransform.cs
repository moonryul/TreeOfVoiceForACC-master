using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTransform : MonoBehaviour
{
    public RectTransform m_UIRectTrans;
   

    public Vector3 m_localPosition;
    public Vector3 m_position;

   
    // Start is called before the first frame update
    void Start()
    {
        m_UIRectTrans = gameObject.GetComponent<RectTransform>();


        m_localPosition = m_UIRectTrans.localPosition;
        m_position = m_UIRectTrans.position;

   
    }

    // Update is called once per frame
    void Update()
    {
        m_UIRectTrans = gameObject.GetComponent<RectTransform>();

        m_localPosition = m_UIRectTrans.localPosition;
        m_position = m_UIRectTrans.position;

       


    }
}
