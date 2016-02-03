using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public struct Int3
{
    public Int3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public int x, y, z;

    public override string ToString()
    {
        return string.Format("({0}, {1}, {2})", x, y, z);
    }
}

public class MapGenerator : MonoBehaviour {
    public Vector3 size = new Vector3(8, 8, 8);
    public GameObject Cube;
    private GameObject[,,] map;

    int nowCubeNum;
    private string nowCubeNumString;

    // Use this for initialization
    void Start() {

        map = new GameObject[(int)size.x, (int)size.y * 2, (int)size.z];
        Load();
    }

    // Update is called once per frame
    void Update() {

    }

    //MapLoad
    void Load()
    {
        //int count = 0;
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.z; z++)
                {
                    GameObject gameObj = GameObject.Instantiate(Cube);
                    gameObj.transform.position = new Vector3(-size.x * 0.5f + x, -y, -size.z * 0.5f + z);
                    gameObj.transform.rotation = Quaternion.identity;
                    gameObj.SetActive(false);
                    map[x, (int)(size.y) + y, z] = gameObj;
                }
        ActiveCalculate();
    }
    void ActiveCalculate()
    {
        nowCubeNum = 0;

        for (int y = 0; y < map.GetLength(1); y++)
            for (int x = 0; x < map.GetLength(0); x++)
                for (int z = 0; z < map.GetLength(2); z++)
                    if (map[x, y, z] != null)
                    {
                        nowCubeNum++;
                        map[x, y, z].SetActive(tileEnable(x, y, z));
                    }

        nowCubeNumString = nowCubeNum.ToString();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 50), nowCubeNumString);
    }

    //get Method
    bool tileEnable(int x, int y, int z)
    {
        if (!hasCube(x + 1, y, z))
            return true;
        if (!hasCube(x - 1, y, z))
            return true;
        if (!hasCube(x, y + 1, z))
            return true;
        if (!hasCube(x, y - 1, z))
            return true;
        if (!hasCube(x, y, z + 1))
            return true;
        if (!hasCube(x, y, z - 1))
            return true;
        return false;
    }
    bool hasCube(int x, int y, int z)
    {
        if (x >= 0 && x < map.GetLength(0)
            && y >= 0 && y < map.GetLength(1)
            && z >= 0 && z < map.GetLength(2))
            return (map[x, y, z] != null);
        else return true;
    }
    Int3 PositionToIndex(Vector3 position)
    {
        return new Int3(Mathf.RoundToInt(position.x + (size.x * 0.5f)),
            Mathf.RoundToInt(size.y - position.y),
            Mathf.RoundToInt(position.z + (size.z * 0.5f)));
    }
    Int3 CubeToIndex(GameObject cube)
    {
        return new Int3(Mathf.RoundToInt(cube.transform.position.x + (size.x * 0.5f)),
            Mathf.RoundToInt(size.y - cube.transform.position.y),
            Mathf.RoundToInt(cube.transform.position.z + (size.z * 0.5f)));
    }
    /*
    GameObject rayCheckCube(Vector3 eye, Vector3 dir)
    {
        Vector3 deltaVector = (eye + (dir * 4)) - eye;
        float min = Mathf.Min(Mathf.Min(deltaVector.x, deltaVector.y), deltaVector.z);
        if (min == deltaVector.x)
        {

        }

        
        //for (int x = 0; x < Mathf.FloorToInt(deltaVector.x); x++)
        //    for (int y = 0; y < Mathf.FloorToInt(deltaVector.y); y++)
        //        for (int z = 0; z < Mathf.FloorToInt(deltaVector.z); z++)
        //        {
        //            map[x, y, z] = ;
        //        }

    }
    */
}

