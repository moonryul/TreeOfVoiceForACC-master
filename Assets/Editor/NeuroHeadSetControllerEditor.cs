using UnityEngine;
using UnityEditor;
using System.Collections;

// CustomEditor() 애트리뷰트를 사용해서 어떤 타입을 커스터마이즈 할 것인지를 명시해 주어야 한다.
[CustomEditor( typeof(NeuroHeadSetController) )]
// 모든 인스펙터 커스터마이즈 클래스는 Editor를 상속받아야 합니다.
public class NeuroHeadSetControllerEditor : Editor
{
    // 커스터마이즈 할 대상의 레퍼런스를 저장
    NeuroHeadSetController _neuro;

    // 커스터마이즈된 에디터의 인스턴스가 생성될 때 호출
    void OnEnable()
    {
        // target은 위의 CustomEditor() 애트리뷰트에서 설정해 준 타입의 객체에 대한 레퍼런스
        // object형이므로 실제 사용할 타입으로 캐스팅 해 준다.
        _neuro = target as NeuroHeadSetController;
    }

    // 인스펙터에 표시되는 GUI들은 이 메서드에서 다루어져야 한다.
    // 여기에서는 EditorGUILayout 이 제공하는 메서드를 사용해서
    // 미리 정의된 에디터 레이아웃으로 컨트롤들을 배치한다.
    public override void OnInspectorGUI()
    {
        // 컨트롤들을 가로로 배치하기 위해 BeginHorizontal()/EndHorizontal() 메서드를 사용한다.
        //EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();

        //c static void LabelField(string label, params GUILayoutOption[] options);

        // label = "Amplitude Range"	Label in front of the label field.
        // label2 = null  The label to show to the right.
        string label = "Amplitude Range";
        string label2 = null;

        EditorGUILayout.LabelField(label, label2);  
       
        // 5가지 중 하나로 선택하도록 IntPopup() 컨트롤을 사용한다.
        string[] amplitudeRangeNames = new string[] { "-100 to 100 uV", "-1 to 1 mV", "-10 to 10 mV", "-100 to 100 mV", "-1 to 1V" };
        int[] amplitudeRangeValues = new int[] { 100, 1000, 10000, 100000,1000000 };

        _neuro.m_amplitudeRange = EditorGUILayout.IntPopup(_neuro.m_amplitudeRange, amplitudeRangeNames, amplitudeRangeValues);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();

        label = "Notch Filter";
        
        EditorGUILayout.LabelField( label, label2);
        // 3가지 중 하나로 선택하도록 IntPopup() 컨트롤을 사용한다.
        string[] notchFreqNames = new string[] { "None", "50Hz", "60Hz" };
        int[] notchFilterNumbers = new int[] { 0,1,2 };

        _neuro.m_notchFilter = EditorGUILayout.IntPopup(_neuro.m_notchFilter, notchFreqNames, notchFilterNumbers );
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical();

        label = "Standard Filter";
        
        EditorGUILayout.LabelField( label, label2);
        // 5가지 중 하나로 선택하도록 IntPopup() 컨트롤을 사용한다.
        string[] standardFreqNames = new string[] { "None", "1-50Hz", "7-13Hz", "15-50Hz", "5-50Hz" };
        int[] standardFilterNumbers= new int[] { 0, 1, 2,3,4 };

        _neuro.m_standardFilter = EditorGUILayout.IntPopup(_neuro.m_standardFilter, standardFreqNames, standardFilterNumbers);
        EditorGUILayout.EndVertical();



        // 리셋 버튼 배치
        //EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginHorizontal();

        // 버튼 사이즈를 조절하기 위한 빈 레이블
        // 만약 빈 레이블이 없다면 아래의 리셋 버튼은 인스펙터의 가로길이에 딱 맞게 출력됩니다.

        label = null;
        EditorGUILayout.LabelField(label, label);
        // 리셋 버튼
        if (GUILayout.Button("Reset"))
        {
            _neuro.Start();
        }
        EditorGUILayout.EndHorizontal();

        // 테스트용 컨트롤들 - 어떻게 동작하는지 보고 싶으면 주석을 풀고 확인해 보세요 ^^
       // OnInspectorGUIForTest();

        // GUI가 변경되었다면 타겟을 다시 렌더링 하도록 하기 위해 dirty 상태로 마크한다.
        if (GUI.changed)
            EditorUtility.SetDirty(target);

        // Show default inspector property editor
        DrawDefaultInspector();
    }

    //// 실제 사용하지는 않지만 다른 컨트롤들이 어떻게 동작하는 알아보기 위해 한 번 살펴보시기 바랍니다.
    //private void OnInspectorGUIForTest()
    //{
    //    // 라인 - 절대 좌표의 압박이 있습니다. 인스펙터에서는 Z값을 사용하면 렌더링 되지 않습니다.
    //    Handles.color = Color.red;
    //    Handles.DrawLine(new Vector3(0, 480, 0), new Vector3(250, 480, 0));

    //    // 베지어 곡선 - 베지어 곡선의 경우 동작이 약간 불안정할 수가 있으므로 주의 (특히, 텍스쳐 색상 관련)
    //    // 베지어 곡선에 사용될 텍스쳐 - 텍스쳐가 설정되지 않으면 유니티 에디터 자체가 비정상 종료됨 =_=;;
    //    Texture2D bezier_texture = new Texture2D(1, 2);
    //    bezier_texture.SetPixel(0, 0, Color.green);
    //    bezier_texture.SetPixel(0, 1, Color.green);
    //    Handles.DrawBezier(new Vector3(10, 480, 0), new Vector3(250, 480, 0),
    //                        new Vector3(100, 450, 0), new Vector3(150, 450, 0),
    //                        Color.green,
    //                        bezier_texture,
    //                        1.5f);

    //    // 인스펙터 타이틀 바
    //    EditorGUI.InspectorTitlebar(new Rect(0, 500, 300, 30), false, target);

    //    // 2D 텍스쳐 관련 부분은 정상적으로 동작시킬 수 있는 방법을 아직 못 알아냈네요 ㅠㅠ;;
    //}

}
