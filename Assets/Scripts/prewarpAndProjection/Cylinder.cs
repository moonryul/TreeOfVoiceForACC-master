using UnityEngine;
using System.Collections;
 
[RequireComponent (typeof (MeshCollider))]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]       
public class Cylinder : MonoBehaviour {
    // Cylinder  has been attached to the "Cylinder" gameObject in the 
    // CylinderEditor class.

    // Cylinder is created as a special case of cone with topRadius = bottomRadius:
    // Refer to https://wiki.unity3d.com/index.php/ProceduralPrimitives

    // public bool sharedVertices = false;     // Triangles share vertices?

    [System.Serializable]
    public struct CylinderParam
    {
        // The parameters of a cylinder 
        public float height;
        public float radius;
       // public Vector3 origin; // the origin is (0,0,0) all the time.
        public int nbSides;
        public int nbHeightSeg; // Not implemented yet

    }

    int nbVerticesCap;
    float bottomRadius;
    float topRadius;


    [SerializeField, Header("Cylinder Parameters"), Space(20)]
    public CylinderParam mCylinderParam = new CylinderParam()
    {
        // The parameters of a cylinder 
        height = 0.1f,
        radius = 0.25f,
        //origin = new Vector3(0f,0f,0f),
        nbSides = 18,
        nbHeightSeg = 1, // Not implemented yet

    };

    

    public void Rebuild(){

		MeshFilter meshFilter = GetComponent<MeshFilter>(); // GetComponent<MeshFiler>() i gameObject.GetComponent<MeshFilter>()

        if (meshFilter==null){
			Debug.LogError("MeshFilter not found!");
			return;
		}

          
		
		Mesh mesh = meshFilter.sharedMesh;
        // In the case of MeshFilter (and SkinnedMeshRenderer), calling mesh will cause an instance
        //to be created if it hasn't already done so, allowing you to modify the mesh 
        // without manipulating all the other instances. sharedMesh doesn't creae a new instance, 
        //which is generally what you want to use when reading from the mesh and not modifying.
        if (mesh == null){
			meshFilter.mesh = new Mesh();
			mesh = meshFilter.sharedMesh;
		}
		mesh.Clear();


        #region Vertices

        nbVerticesCap = mCylinderParam.nbSides + 1;
        bottomRadius = mCylinderParam.radius;
        topRadius = mCylinderParam.radius;

        // bottom + top + sides
        Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap 
                                         + mCylinderParam.nbSides * mCylinderParam.nbHeightSeg * 2 + 2];
        int vert = 0;
        float _2pi = Mathf.PI * 2f;

        // Bottom cap
        vertices[vert++] = new Vector3(0f, 0f, 0f);
        while (vert <= mCylinderParam.nbSides)
        {
            float rad = (float)vert / mCylinderParam.nbSides * _2pi;
            vertices[vert] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0f,
                                          Mathf.Sin(rad) * bottomRadius);
            vert++;
        }

        // Top cap
        vertices[vert++] = new Vector3(0f, mCylinderParam.height, 0f);
        while (vert <= mCylinderParam.nbSides * 2 + 1)
        {
            float rad = (float)(vert - mCylinderParam.nbSides - 1) / mCylinderParam.nbSides * _2pi;
            vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius,
                                        mCylinderParam.height, Mathf.Sin(rad) * topRadius);
            vert++;
        }

        // Sides
        int v = 0;
        while (vert <= vertices.Length - 4)
        {
            float rad = (float)v / mCylinderParam.nbSides * _2pi;
            vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius,
                                         mCylinderParam.height, Mathf.Sin(rad) * topRadius);
            vertices[vert + 1] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0, 
                                             Mathf.Sin(rad) * bottomRadius);
            vert += 2;
            v++;
        }
        vertices[vert] = vertices[mCylinderParam.nbSides * 2 + 2];
        vertices[vert + 1] = vertices[mCylinderParam.nbSides * 2 + 3];
        #endregion

        #region Normales

        // bottom + top + sides
        Vector3[] normales = new Vector3[vertices.Length];
        vert = 0;

        // Bottom cap
        while (vert <= mCylinderParam.nbSides)
        {
            normales[vert++] = Vector3.down;
        }

        // Top cap
        while (vert <= mCylinderParam.nbSides * 2 + 1)
        {
            normales[vert++] = Vector3.up;
        }

        // Sides
        v = 0;
        while (vert <= vertices.Length - 4)
        {
            float rad = (float)v / mCylinderParam.nbSides * _2pi;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            normales[vert] = new Vector3(cos, 0f, sin);
            normales[vert + 1] = normales[vert];

            vert += 2;
            v++;
        }
        normales[vert] = normales[mCylinderParam.nbSides * 2 + 2];
        normales[vert + 1] = normales[mCylinderParam.nbSides * 2 + 3];
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];

        // Bottom cap
        int u = 0;
        uvs[u++] = new Vector2(0.5f, 0.5f);
        while (u <= mCylinderParam.nbSides)
        {
            float rad = (float)u / mCylinderParam.nbSides * _2pi;
            uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
            u++;
        }

        // Top cap
        uvs[u++] = new Vector2(0.5f, 0.5f);
        while (u <= mCylinderParam.nbSides * 2 + 1)
        {
            float rad = (float)u / mCylinderParam.nbSides * _2pi;
            uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
            u++;
        }

        // Sides
        int u_sides = 0;
        while (u <= uvs.Length - 4)
        {
            float t = (float)u_sides / mCylinderParam.nbSides;
            uvs[u] = new Vector3(t, 1f);
            uvs[u + 1] = new Vector3(t, 0f);
            u += 2;
            u_sides++;
        }
        uvs[u] = new Vector2(1f, 1f);
        uvs[u + 1] = new Vector2(1f, 0f);
        #endregion

        #region Triangles
        int nbTriangles = mCylinderParam.nbSides + mCylinderParam.nbSides + mCylinderParam.nbSides * 2;
        int[] triangles = new int[nbTriangles * 3 + 3];

        // Bottom cap
        int tri = 0;
        int i = 0;
        while (tri < mCylinderParam.nbSides - 1)
        {
            triangles[i] = 0;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = tri + 2;
            tri++;
            i += 3;
        }
        triangles[i] = 0;
        triangles[i + 1] = tri + 1;
        triangles[i + 2] = 1;
        tri++;
        i += 3;

        // Top cap
        //tri++;
        while (tri < mCylinderParam.nbSides * 2)
        {
            triangles[i] = tri + 2;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = nbVerticesCap;
            tri++;
            i += 3;
        }

        triangles[i] = nbVerticesCap + 1;
        triangles[i + 1] = tri + 1;
        triangles[i + 2] = nbVerticesCap;
        tri++;
        i += 3;
        tri++;

        // Sides
        while (tri <= nbTriangles)
        {
            triangles[i] = tri + 2;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = tri + 0;
            tri++;
            i += 3;

            triangles[i] = tri + 1;
            triangles[i + 1] = tri + 2;
            triangles[i + 2] = tri + 0;
            tri++;
            i += 3;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;


        mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		
	}

    //  OnValidate function is called in editor mode only on the following cases:
    //  (1) once script loaded;
    //   (2) if any value is changed in inspector

    void OnValidate()
    {
        Rebuild(); // rebuilds the cylinder using the changed paramater values

    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
