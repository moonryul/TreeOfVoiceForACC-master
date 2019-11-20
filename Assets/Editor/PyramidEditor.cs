using UnityEngine;
using UnityEditor;       // To  build a custom inspector for Tetrahedron Component
using System.Collections;

//build a custom inspector for MyCylinder Component
[CustomEditor (typeof (Pyramid))] 
public class PyramidEditor : Editor
{
    //Editor is a ScriptableObject.Just like MonoBehaviours, ScriptableObjects derive from the base Unity Object but,
    //unlike MonoBehaviours, you can not attach a ScriptableObject to a GameObject.
    //Instead, you save them as Assets in your Project.
    [MenuItem ("GameObject/Create Other/Pyramid")]
	static void Create()
    {

		GameObject gameObject = new GameObject("Pyramid");
		Pyramid s = gameObject.AddComponent<Pyramid>();
        // Add/Attach the Pyramid component to the created empty gameobject named "Pyramid"
         
                                       
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();
		s.Rebuild();  // Make a procedural mesh
	}
	
	public override void OnInspectorGUI ()
	{
		Pyramid obj;

		obj = target as Pyramid;   // Editor.target;  (Pyramid)target

		if (obj == null)
		{
			return;
		}
	
		base.DrawDefaultInspector();
		EditorGUILayout.BeginHorizontal ();
		
		// Rebuild mesh when user click the Rebuild button
		//if (GUILayout.Button("Rebuild")){
		//	obj.Rebuild();
		//}
		EditorGUILayout.EndHorizontal ();
	}
}  // public class PyramidEditor
