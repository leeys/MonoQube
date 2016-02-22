using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeArea : MonoBehaviour {

    private MeshFilter meshFilter;
    private Mesh mesh;
    private MeshCollider meshCollider;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();

    public bool isEmpty = true;

    void Awake()
    {
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshFilter = gameObject.GetComponent<MeshFilter>();
    }

	// Use this for initialization
	void Start () {
        //mesh.vertices = ;
        //mesh.normals = ;
        //mesh.uv = ;
    }

    
    // Update is called once per frame
    void Update () {
	
	}
    Vector3[] v = {
            //Back
            new Vector3( -0.5f, -0.5f, -0.5f ),
            new Vector3( -0.5f,  0.5f, -0.5f ),
            new Vector3(  0.5f,  0.5f, -0.5f ),
            new Vector3(  0.5f, -0.5f, -0.5f ),

            //Right
            new Vector3(  0.5f, -0.5f, -0.5f ),
            new Vector3(  0.5f,  0.5f, -0.5f ),
            new Vector3(  0.5f,  0.5f,  0.5f ),
            new Vector3(  0.5f, -0.5f,  0.5f ),

            //Forward
            new Vector3(  0.5f, -0.5f, 0.5f ),
            new Vector3(  0.5f,  0.5f, 0.5f ),
            new Vector3( -0.5f,  0.5f, 0.5f ),
            new Vector3( -0.5f, -0.5f, 0.5f ),

            //Left
            new Vector3( -0.5f, -0.5f, 0.5f ),
            new Vector3( -0.5f,  0.5f, 0.5f ),
            new Vector3( -0.5f,  0.5f, -0.5f ),
            new Vector3( -0.5f, -0.5f, -0.5f ),

            //Up
            new Vector3( -0.5f, 0.5f, -0.5f ),
            new Vector3( -0.5f, 0.5f,  0.5f ),
            new Vector3(  0.5f, 0.5f,  0.5f ),
            new Vector3(  0.5f, 0.5f, -0.5f ),

            //Down
            new Vector3( -0.5f, -0.5f,  0.5f ),
            new Vector3( -0.5f, -0.5f, -0.5f ),
            new Vector3(  0.5f, -0.5f, -0.5f ),
            new Vector3(  0.5f, -0.5f,  0.5f ),
            };
    public void GenerateMesh(MapGenerator.Area area)
    {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        mesh = new Mesh();
        
        meshFilter.mesh = mesh;

        Vector3[] quad = new Vector3[4];

        Vector3[] normal = new Vector3[4];
        int[] quadIdx = new int[6];

        for (int x = area.halfMin.x; x < area.halfMax.x; x++)
            for (int y = area.halfMin.y; y < area.halfMax.y; y++)
                for (int z = area.halfMin.z; z < area.halfMax.z; z++)
                {
                    Vector3 cubePos = area.map.IndexToPosition(new Int3(x, y, z));

                    int sideFlag = area.map.map[x, y, z].isEnable;

                    int startIdx = vertices.Count;

                    if ((sideFlag & (int)OpenSide.FORWARD) != 0)
                    {
                        quad[0] = cubePos + new Vector3(  0.5f, -0.5f, 0.5f );
                        quad[1] = cubePos + new Vector3(  0.5f,  0.5f, 0.5f );
                        quad[2] = cubePos + new Vector3( -0.5f,  0.5f, 0.5f );
                        quad[3] = cubePos + new Vector3( -0.5f, -0.5f, 0.5f );
                        vertices.AddRange(quad);

                        normal[0] = normal[1] = normal[2] = normal[3] = Vector3.forward;

                        quadIdx[0] = startIdx + 0;
                        quadIdx[1] = startIdx + 1;
                        quadIdx[2] = startIdx + 2;
                        quadIdx[3] = startIdx + 0;
                        quadIdx[4] = startIdx + 2;
                        quadIdx[5] = startIdx + 3;
                        triangles.AddRange(quadIdx);
                        startIdx += 4;
                    }

                    if ((sideFlag & (int)OpenSide.BACK) != 0)
                    {
                        quad[0] = cubePos + new Vector3( -0.5f, -0.5f, -0.5f );
                        quad[1] = cubePos + new Vector3( -0.5f,  0.5f, -0.5f );
                        quad[2] = cubePos + new Vector3(  0.5f,  0.5f, -0.5f );
                        quad[3] = cubePos + new Vector3(  0.5f, -0.5f, -0.5f );
                        vertices.AddRange(quad);

                        normal[0] = normal[1] = normal[2] = normal[3] = Vector3.back;

                        quadIdx[0] = startIdx + 0;
                        quadIdx[1] = startIdx + 1;
                        quadIdx[2] = startIdx + 2;
                        quadIdx[3] = startIdx + 0;
                        quadIdx[4] = startIdx + 2;
                        quadIdx[5] = startIdx + 3;
                        triangles.AddRange(quadIdx);
                        startIdx += 4;
                    }

                    if ((sideFlag & (int)OpenSide.RIGHT) != 0)
                    {
                        quad[0] = cubePos + new Vector3(0.5f, -0.5f, -0.5f);
                        quad[1] = cubePos + new Vector3(  0.5f,  0.5f, -0.5f );
                        quad[2] = cubePos + new Vector3(  0.5f,  0.5f,  0.5f );
                        quad[3] = cubePos + new Vector3(  0.5f, -0.5f,  0.5f );
                        vertices.AddRange(quad);

                        normal[0] = normal[1] = normal[2] = normal[3] = Vector3.right;

                        quadIdx[0] = startIdx + 0;
                        quadIdx[1] = startIdx + 1;
                        quadIdx[2] = startIdx + 2;
                        quadIdx[3] = startIdx + 0;
                        quadIdx[4] = startIdx + 2;
                        quadIdx[5] = startIdx + 3;
                        triangles.AddRange(quadIdx);
                        startIdx += 4;
                    }
                    if ((sideFlag & (int)OpenSide.LEFT) != 0)
                    {
                        quad[0] = cubePos + new Vector3(-0.5f, -0.5f, 0.5f);
                        quad[1] = cubePos + new Vector3(-0.5f, 0.5f, 0.5f);
                        quad[2] = cubePos + new Vector3( -0.5f,  0.5f, -0.5f );
                        quad[3] = cubePos + new Vector3(-0.5f, -0.5f, -0.5f);
                        vertices.AddRange(quad);

                        normal[0] = normal[1] = normal[2] = normal[3] = Vector3.left;

                        quadIdx[0] = startIdx + 0;
                        quadIdx[1] = startIdx + 1;
                        quadIdx[2] = startIdx + 2;
                        quadIdx[3] = startIdx + 0;
                        quadIdx[4] = startIdx + 2;
                        quadIdx[5] = startIdx + 3;
                        triangles.AddRange(quadIdx);
                        startIdx += 4;
                    }
                    if ((sideFlag & (int)OpenSide.UP) != 0)
                    {
                        quad[0] = cubePos + new Vector3( -0.5f, 0.5f, -0.5f );
                        quad[1] = cubePos + new Vector3( -0.5f, 0.5f,  0.5f );
                        quad[2] = cubePos + new Vector3(0.5f, 0.5f, 0.5f);
                        quad[3] = cubePos + new Vector3(0.5f, 0.5f, -0.5f);
                        vertices.AddRange(quad);

                        normal[0] = normal[1] = normal[2] = normal[3] = Vector3.up;

                        quadIdx[0] = startIdx + 0;
                        quadIdx[1] = startIdx + 1;
                        quadIdx[2] = startIdx + 2;
                        quadIdx[3] = startIdx + 0;
                        quadIdx[4] = startIdx + 2;
                        quadIdx[5] = startIdx + 3;
                        triangles.AddRange(quadIdx);
                        startIdx += 4;
                    }
                    if ((sideFlag & (int)OpenSide.DOWN) != 0)
                    {
                        quad[0] = cubePos + new Vector3(-0.5f, -0.5f, 0.5f);
                        quad[1] = cubePos + new Vector3(-0.5f, -0.5f, -0.5f);
                        quad[2] = cubePos + new Vector3(0.5f, -0.5f, -0.5f);
                        quad[3] = cubePos + new Vector3(0.5f, -0.5f, 0.5f);
                        vertices.AddRange(quad);

                        normal[0] = normal[1] = normal[2] = normal[3] = Vector3.down;

                        quadIdx[0] = startIdx + 0;
                        quadIdx[1] = startIdx + 1;
                        quadIdx[2] = startIdx + 2;
                        quadIdx[3] = startIdx + 0;
                        quadIdx[4] = startIdx + 2;
                        quadIdx[5] = startIdx + 3;
                        triangles.AddRange(quadIdx);
                        startIdx += 4;
                    }
                }
        isEmpty = (vertices.Count == 0);

        if (isEmpty)
        {
            gameObject.SetActive(false);
            meshCollider.sharedMesh = null;
        }
            
        else
        {
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.RecalculateNormals();
            meshCollider.sharedMesh = mesh;
        }

    }


}
