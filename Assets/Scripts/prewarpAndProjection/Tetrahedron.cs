using UnityEngine;
using System.Collections;

//RequireComponent automatically adds the required component to the 
// the gameObject to which the Script component will be added.

[RequireComponent (typeof (MeshCollider))]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class Tetrahedron : MonoBehaviour {
    // Tetrahedron has been attached to the "Tetrahedron" gameObject in the 
    // TetrahedronEditor class.


    [System.Serializable]
    public struct TetrahedronParam
    {
        public float BaseSideLength;
        public float Height;
    }



    [SerializeField, Header("TetraHedron Parameters"), Space(20)]
    public TetrahedronParam mTetrahedron =  // use "object initializer syntax" to initialize the structure:https://www.tutorialsteacher.com/csharp/csharp-object-initializer
                                    // See also: https://stackoverflow.com/questions/3661025/why-are-c-sharp-3-0-object-initializer-constructor-parentheses-optional

      new TetrahedronParam
      {
          BaseSideLength = 0.1f,  // the length unit is  meter
          Height = 0.2f
      };


    public bool sharedVertices = false;
	
	public void Rebuild(){
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter==null){
			Debug.LogError("MeshFilter not found!");
			return;
		}

        float baseDepth = Mathf.Sqrt(0.75f * mTetrahedron.BaseSideLength );
        

        Vector3 p0 = new Vector3(0,0,0);
		Vector3 p1 = new Vector3(mTetrahedron.BaseSideLength, 0 ,0);
        Vector3 p2 = new Vector3(0.5f * mTetrahedron.BaseSideLength, 0, baseDepth);
        Vector3 p3 = new Vector3( 0.5f * mTetrahedron.BaseSideLength, mTetrahedron.Height, 0.5f*baseDepth );
		
		Mesh mesh = meshFilter.sharedMesh;
		if (mesh == null){
			meshFilter.mesh = new Mesh();
			mesh = meshFilter.sharedMesh;
		}
		mesh.Clear();
		if (sharedVertices){
			mesh.vertices = new Vector3[]{p0,p1,p2,p3};
			mesh.triangles = new int[]{
				0,1,2,
				0,2,3,
				2,1,3,
				0,3,1
			};	
			// basically just assigns a corner of the texture to each vertex
			mesh.uv = new Vector2[]{
				new Vector2(0,0),
				new Vector2(1,0),
				new Vector2(0,1),
				new Vector2(1,1),
			};
		} else {
			mesh.vertices = new Vector3[]{
				p0,p1,p2,
				p0,p2,p3,
				p2,p1,p3,
				p0,p3,p1
			};
			mesh.triangles = new int[]{
				0,1,2,
				3,4,5,
				6,7,8,
				9,10,11
			};
			
			Vector2 uv0 = new Vector2(0,0);
			Vector2 uv1 = new Vector2(1,0);
			Vector2 uv2 = new Vector2(0.5f,1);
			
			mesh.uv = new Vector2[]{
				uv0,uv1,uv2,
				uv0,uv1,uv2,
				uv0,uv1,uv2,
				uv0,uv1,uv2
			};
			
		}
		
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
