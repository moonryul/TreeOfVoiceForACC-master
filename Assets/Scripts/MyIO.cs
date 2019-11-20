
#define UNITY_EDITOR
#define TRACE_ON

using UnityEngine;
using System.Collections;
using System;

using UnityEditor;
using System.IO;

public class MyIO
{


    // Debug LOG
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("TRACE_ON")]

    public static void DebugLog(int  str)
    {
        //UnityEngine.Debug.LogFormat("Number: {0}, string: {1}, number again: {0}, character: {2}", num, str, chr);
        UnityEngine.Debug.Log(str);

    }   //void DebugLog(string str)

    public static void DebugLog(float str)
    {
        //UnityEngine.Debug.LogFormat("Number: {0}, string: {1}, number again: {0}, character: {2}", num, str, chr);
        UnityEngine.Debug.Log(str);

    }   //void DebugLog(string str)

    public static void DebugLog(string str)
    {
        //UnityEngine.Debug.LogFormat("Number: {0}, string: {1}, number again: {0}, character: {2}", num, str, chr);
        UnityEngine.Debug.Log(str);

    }   //void DebugLog(string str)

    public static void DebugLog(Vector2 vector)
    {

        Debug.Log(vector.ToString("F4"));

    }   //void DebugLogVector()


    public static void DebugLog(Vector3 vector)
    {

        Debug.Log(vector.ToString("F4"));

    }   //void DebugLogVector()


    public static void DebugLog(Vector4 vector)
    {

        Debug.Log(vector.ToString("F4"));

    }   //void DebugLogVector()
    public static void DebugLog(Rect rect)
    {

        Debug.Log(rect.ToString("F4"));

    }   //void DebugLogVector()

    public static void DebugLog(Matrix4x4 mat)     // struct Matrix4x4 mat; mat[i,j]
    {
        int rowLength = 4;
        int colLength = 4;
        string arrayString = "";

        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                arrayString = string.Format("{0} {1} {2} {3}", mat[i, 0], mat[i, 1], mat[i, 2], mat[i, 3]);
            }
            // arrayString += System.Environment.NewLine + System.Environment.NewLine;
            arrayString += System.Environment.NewLine;
            Debug.Log(arrayString);
        }

        // Debug.Log(arrayString);



    }   //void DebugLog()


} // MyIO

