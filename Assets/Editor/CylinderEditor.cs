using UnityEngine;
using UnityEditor;       // To  build a custom inspector for Tetrahedron Component
using System.Collections;

// https://stackoverflow.com/questions/49453265/unity-editor-executeineditmode-vs-editor-scripts
//  By default, MonoBehaviours are only executed in play mode
//https://hololens.reality.news/how-to/hololens-dev-101-unity-editor-basics-0175161/

//build a custom inspector for MyCylinder Component
[CustomEditor (typeof (Cylinder))] 
public class CylinderEditor : Editor {
    //Editor is a ScriptableObject.Just like MonoBehaviours, ScriptableObjects derive from the base Unity Object but,
    //unlike MonoBehaviours, you can not attach a ScriptableObject to a GameObject.
    //Instead, you save them as Assets in your Project.
    [MenuItem ("GameObject/Create Other/Cylinder")]
	static void Create(){

		GameObject gameObject = new GameObject("Cylinder");
		Cylinder s = gameObject.AddComponent<Cylinder>();   // Cylinder Script component is instantiated explicitly
                                                            // But in this case, you cannot use its START(), UPDATE() methods
        // Add/Attach the Tetrahedron component to the created empty gameobject named "Tetrahedron"
        // cf. definition of class Tetrahedron:
        // [RequireComponent(typeof(MeshCollider))]
        // [RequireComponent(typeof(MeshFilter))]
        // [RequireComponent(typeof(MeshRenderer))]

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();
		s.Rebuild();  // Make a procedural mesh
	}
	
	public override void OnInspectorGUI ()
	{
		Cylinder obj;

		obj = target as Cylinder;   // target = Editor.target; target as Cylinder = (Cylinder)target

		if (obj == null)
		{
			return;
		}
	
		base.DrawDefaultInspector();
		EditorGUILayout.BeginHorizontal ();
		
		// Rebuild mesh when user click the Rebuild button => Rebuilds() will be called in OnValidate() within Cylinder.cs
		//if (GUILayout.Button("Rebuild")){
		//	obj.Rebuild();
		//}
		EditorGUILayout.EndHorizontal ();
	}
}
