using UnityEngine;
using UnityEditor;       // To  build a custom inspector for Tetrahedron Component
using System.Collections;

//build a custom inspector for Tetrahedron Component
[CustomEditor (typeof (Tetrahedron))] 
public class TetrahedronEditor : Editor {
    //Editor is a ScriptableObject.Just like MonoBehaviours, ScriptableObjects derive from the base Unity Object but,
    //unlike MonoBehaviours, you can not attach a ScriptableObject to a GameObject.
    //Instead, you save them as Assets in your Project.
    [MenuItem ("GameObject/Create Other/Tetrahedron")]
	static void Create(){

		GameObject gameObject = new GameObject("Tetrahedron");
		Tetrahedron s = gameObject.AddComponent<Tetrahedron>();
        // Add/Attach the Tetrahedron component to the created empty gameobject named "Tetrahedron"
        //cf. definition of class Tetrahedron:
        // [RequireComponent(typeof(MeshCollider))]
        // [RequireComponent(typeof(MeshFilter))]
        // [RequireComponent(typeof(MeshRenderer))]

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();
		s.Rebuild();  // Make a procedural mesh
	}
	
	public override void OnInspectorGUI ()
	{
		Tetrahedron obj;

		obj = target as Tetrahedron;   // Editor.target   == (Tetrahedron)target

		if (obj == null)
		{
			return;
		}
	
		base.DrawDefaultInspector();
		EditorGUILayout.BeginHorizontal ();
		
		// Rebuild mesh when user click the Rebuild button
		if (GUILayout.Button("Rebuild")){
			obj.Rebuild();
		}
		EditorGUILayout.EndHorizontal ();
	}
}
