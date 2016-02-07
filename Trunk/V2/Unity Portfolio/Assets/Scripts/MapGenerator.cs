using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum CubeInfo { None, Grass, Rock, };

/*
public class QuadNode
{
    public enum NodePosition { LT = 0, RT, LB, RB, CT };
    public QuadNode(QuadNode parent, NodePosition nodePosition, Vector3 centerPos, Vector3 size, ref CubeInfo[,,] mapInfo)
    {
        this.parent = parent;
        
        if (size.x > 4 && size.z > 4)
        {
            child = new QuadNode[4];
            Vector3 posLT = centerPosition + new Vector3(-size.x * 0.25f, 0, size.z * 0.25f);
            Vector3 posRT = centerPosition + new Vector3(size.x * 0.25f, 0, size.z * 0.25f);
            Vector3 posLB = centerPosition + new Vector3(-size.x * 0.25f, 0, -size.z * 0.25f);
            Vector3 posRB = centerPosition + new Vector3(size.x * 0.25f, 0, -size.z * 0.25f);

            Vector3 newSize = new Vector3(size.x * 0.5f, size.y, size.z * 0.5f);
            child[(int)NodePosition.LT] = new QuadNode(this, NodePosition.LT, posLT, newSize,ref mapInfo);
            child[(int)NodePosition.RT] = new QuadNode(this, NodePosition.RT, posRT, newSize,ref mapInfo);
            child[(int)NodePosition.LB] = new QuadNode(this, NodePosition.LB, posLB, newSize,ref mapInfo);
            child[(int)NodePosition.RB] = new QuadNode(this, NodePosition.RB, posRB, newSize,ref mapInfo);
        }
        else
        {
            cubeNodes = new Vector3(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.x)); 
        }
    }

    private QuadNode parent;
    private QuadNode[] child;

    private Vector3 centerPosition;
    private Vector3 size;

    private bool isOn;
    private GameObject[] cubeNodes;
}

public class CubeQuadTree
{
    public CubeQuadTree(CubeInfo[,,] mapinfo)
    {
        root = new QuadNode(null, QuadNode.NodePosition.CT, new Vector3(0, 0, 0), new Vector3(mapinfo.GetLength(0), mapinfo.GetLength(1), mapinfo.GetLength(2)), ref mapinfo);
    }
    QuadNode root;
}*/

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
    
    public struct CubeState
    {
        public GameObject cube;
        public bool isEnable;
    };

    public class Area
    {
        public Area(Vector3 center, Vector3 areaSize, Vector3 halfSize, ref CubeState[,,] mapState, MapGenerator map)
        {
            this.map = map;
            this.mapState = mapState;
            this.center = center;
            min = center - areaSize;
            max = center + areaSize;
            halfMin = map.PositionToIndex(center - halfSize);
            halfMin.y = 0;
            halfMax = map.PositionToIndex(center + halfSize);
            halfMax.y = mapState.GetLength(1);
        }

        public bool AreaCheck(Vector3 position)
        {
            if (position.x > min.x && max.x > position.x && position.z > min.z && max.z > position.z)
            {
                if (!isDrawing)
                {
                    //추가
                    for (int x = halfMin.x; x < halfMax.x; x++)
                        for (int y = halfMin.y; y < halfMax.y; y++)
                            for (int z = halfMin.z; z < halfMax.z; z++)
                            {
                                if (mapState[x, y, z].isEnable)
                                {
                                    if (mapState[x, y, z].cube == null)
                                    {
                                        GameObject gameObj = GameObject.Instantiate(map.CubeInstance);
                                        gameObj.transform.position = map.IndexToPosition(new Int3(x, y, z));
                                        gameObj.transform.rotation = Quaternion.identity;
                                        mapState[x, y, z].cube = gameObj;
                                    }
                                }
                            }
                    isDrawing = true;
                    return true;
                }
            }
            else if (isDrawing)
            {
                //삭제
                for (int x = halfMin.x; x < halfMax.x; x++)
                    for (int y = halfMin.y; y < halfMax.y; y++)
                        for (int z = halfMin.z; z < halfMax.z; z++)
                        {
                            if (mapState[x, y, z].cube != null)
                            {
                                GameObject.Destroy(mapState[x, y, z].cube);
                                mapState[x, y, z].cube = null;
                            }
                        }
                isDrawing = false;
                return true;
            }

            return false;
        }

        private bool isDrawing;

        private Vector3 center;
        private Vector3 min;
        private Vector3 max;
        private Int3 halfMin;
        private Int3 halfMax;
        private CubeState[,,] mapState;
        private MapGenerator map;
    }

    public Transform player;

    public Vector3 size = new Vector3(8, 8, 8);
    public GameObject CubeInstance;

    private CubeInfo[,,] mapinfo;
    private CubeState[,,] map;
    private Area[] area;
    private List<GameObject> l_CollisionCube = new List<GameObject>();
    int layerFlag;

    int nowCubeNum;

    // Use this for initialization
    void Start() {
        layerFlag = 8;
        
        Load();

        StartCoroutine("AreaCulling");
    }

    // Update is called once per frame
    void Update(){

    }

    IEnumerator AreaCulling()
    {
        while (true)
        {
            int count = 0;
            for (int i = 0; i < area.Length; i++)
            {
                if (area[i].AreaCheck(player.position))
                    count++;
                if (count > 8)
                {
                    count = 0;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield return new WaitForSeconds(0.33f);
        }
    }

    //MapLoad
    void Load()
    {
        map = new CubeState[(int)size.x, (int)size.y * 2, (int)size.z];
        mapinfo = new CubeInfo[(int)size.x, (int)size.y * 2, (int)size.z];

        int cutSize = 8;
        int farSize = 48;

        int areaNumber = Mathf.RoundToInt(size.x * size.z / (cutSize * cutSize));
        area = new Area[areaNumber];

        int halfY = (int)size.y;
        int divideX = (int)size.x / cutSize;
        int divideZ = (int)size.z / cutSize;

        for (int x = 0; x < divideX; x++)
        {
            for (int z = 0; z < divideZ; z++)
            {
                area[(x * divideZ) + z] = new Area(new Vector3(4,0,4) + IndexToPosition(new Int3(x * cutSize, halfY, z * cutSize)), new Vector3(farSize, farSize, farSize), new Vector3(cutSize * 0.5f, cutSize * 0.5f, cutSize * 0.5f), ref map, this);
            }
        }

        //int count = 0;
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.z; z++)
                {
                    mapinfo[x, (int)(size.y) + y, z] = CubeInfo.Grass;
                }
        ActiveCalculate();
    }
    void ActiveCalculate()
    {
        for (int x = 0; x < map.GetLength(0); x++)
            for (int z = 0; z < map.GetLength(2); z++)
                for (int y = 0; y < map.GetLength(1); y++)
                    if (hasCubeInfo(x, (int)(size.y) + y, z))
                    {
                        if (tileEnable(x, (int)(size.y) + y, z))
                        {
                            if (Vector3.Distance(player.position, IndexToPosition(new Int3(x, y, z))) < 48)
                            {
                                GameObject gameObj = GameObject.Instantiate(CubeInstance);
                                gameObj.transform.position = IndexToPosition(new Int3(x, (int)(size.y) + y, z));
                                gameObj.transform.rotation = Quaternion.identity;
                                map[x, (int)(size.y) + y, z].cube = gameObj;
                            }
                            map[x, (int)(size.y) + y, z].isEnable = true;
                        }
                    }
    }

    void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 200, 50), nowCubeNumString);
    }

    //큐브가 그려져야되는지
    public bool tileEnable(int x, int y, int z)
    {
        if (!isOverMap(x + 1, y, z))
            if (!hasCubeInfo(x + 1, y, z))
                return true;
        if (!isOverMap(x - 1, y, z))
            if (!hasCubeInfo(x - 1, y, z))
                return true;
        if (!isOverMap(x , y + 1, z))
            if (!hasCubeInfo(x, y + 1, z))
                return true;
        if (!isOverMap(x, y - 1, z))
            if (!hasCubeInfo(x, y - 1, z))
                return true;
        if (!isOverMap(x, y, z + 1))
            if (!hasCubeInfo(x, y, z + 1))
                return true;
        if (!isOverMap(x, y, z - 1))
            if (!hasCubeInfo(x, y, z - 1))
                return true;
        return false;
    }
    public bool tileEnable(Int3 index)
    {
        return tileEnable(index.x, index.y, index.z);
    }

    //큐브가 맵정보상으로 존재하는지
    public bool hasCubeInfo(int x, int y, int z)
    {
        if (x >= 0 && x < map.GetLength(0)
            && y >= 0 && y < map.GetLength(1)
            && z >= 0 && z < map.GetLength(2))
            return (mapinfo[x, y, z] != CubeInfo.None);
        else return false;
    }
    public bool hasCubeInfo(Int3 index)
    {
        return hasCubeInfo(index.x, index.y, index.z);
    }

    //실질적으로 그리는 맵에서 오브젝트로 할당되있는지
    public bool hasObject(int x, int y, int z)
    {
        if (x >= 0 && x < map.GetLength(0)
            && y >= 0 && y < map.GetLength(1)
            && z >= 0 && z < map.GetLength(2))
                return map[x, y, z].cube != null;
        return false;
    }
    public bool hasObject(Int3 index)
    {
        return hasObject(index.x, index.y, index.z);
    }

    //인덱스가 맵밖으로 나갔는지
    public bool isOverMap(int x, int y, int z) {
        return x < 0 || x >= map.GetLength(0)
|| y < 0 || y >= map.GetLength(1)
|| z < 0 || z >= map.GetLength(2);
    }
    public bool isOverMap(Int3 idx)
    {
        return isOverMap(idx.x, idx.y, idx.z);
    }
    
    //큐브 오브젝트 들고옴
    public GameObject getCube(Int3 idx)
    {
        if (hasCubeInfo(idx) == false) return null;

        if (!hasObject(idx))
        {
            GameObject gameObj = GameObject.Instantiate(CubeInstance);
            gameObj.transform.position = IndexToPosition(idx);
            gameObj.transform.rotation = Quaternion.identity;
            map[idx.x, idx.y, idx.z].cube = gameObj;

            return gameObj;
        }
        else
            return map[idx.x, idx.y, idx.z].cube;
    }

    public Int3 PositionToIndex(Vector3 position)
    {
        return new Int3(Mathf.FloorToInt(position.x + (size.x * 0.5f)),
            Mathf.FloorToInt(size.y - position.y),
            Mathf.FloorToInt(position.z + (size.z * 0.5f)));
    }
    public Vector3 IndexToPosition(Int3 index)
    {
        return new Vector3(-size.x * 0.5f + index.x, (int)(size.y) - index.y, -size.z * 0.5f + index.z);
    }
    public Int3 CubeToIndex(GameObject cube)
    {
        return PositionToIndex(cube.transform.position);
    }

    public GameObject RayCheckCube(Vector3 eye, Vector3 dir, out Int3 setIdx)
    {
        /*
        Vector3 deltaVector = dir.normalized * 5;
        Int3 deltaIdx = new Int3(Mathf.CeilToInt(deltaVector.x), Mathf.CeilToInt(deltaVector.y), Mathf.CeilToInt(deltaVector.z));
        Int3 sign = new Int3(Math.Sign(deltaIdx.x), Math.Sign(deltaIdx.y), Math.Sign(deltaIdx.z));

        deltaIdx = new Int3(Math.Abs(deltaIdx.x), Math.Abs(deltaIdx.y), Math.Abs(deltaIdx.z));

        for (int x = 0; x <= deltaIdx.x; x++)
            for (int y = 0; y <= deltaIdx.y; y++)
                for (int z = 0; z <= deltaIdx.z; z++)
                {
                    Int3 idx = PositionToIndex(eye + new Vector3(x * sign.x, y * sign.y, z * sign.z));
                    
                    if (isOverMap(idx))
                        continue;
                    if (hasCubeInfo(idx))
                    {
                        Debug.Log("asdf");
                        GameObject cube = getCube(idx);
                        cube.layer = layerFlag;
                        l_CollisionCube.Add(cube);
                    }
                }*/
         
        Ray ray = new Ray(eye, dir.normalized);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 10/*, layerFlag*/);

        //for (int i = 0; i < l_CollisionCube.Count; i++)
        //{
        //    l_CollisionCube[i].layer = 0;
        //}
        //l_CollisionCube.Clear();

        if (hit.collider != null)
        {
            //forward
            if (Mathf.Approximately(hit.normal.x, Vector3.forward.x) && Mathf.Approximately(hit.normal.y, Vector3.forward.y) && Mathf.Approximately(hit.normal.z, Vector3.forward.z))
            {
                Int3 idx = CubeToIndex(hit.collider.gameObject);
                idx.z++;
                setIdx = idx;
            }
            //up
            else if (Mathf.Approximately(hit.normal.x, Vector3.up.x) && Mathf.Approximately(hit.normal.y, Vector3.up.y) && Mathf.Approximately(hit.normal.z, Vector3.up.z))
            {
                Int3 idx = CubeToIndex(hit.collider.gameObject);
                idx.y--;
                setIdx = idx;
            }
            //right
            else if (Mathf.Approximately(hit.normal.x, Vector3.right.x) && Mathf.Approximately(hit.normal.y, Vector3.right.y) && Mathf.Approximately(hit.normal.z, Vector3.right.z))
            {
                Int3 idx = CubeToIndex(hit.collider.gameObject);
                idx.x++;
                setIdx = idx;
            }
            //back
            else if (Mathf.Approximately(hit.normal.x, Vector3.back.x) && Mathf.Approximately(hit.normal.y, Vector3.back.y) && Mathf.Approximately(hit.normal.z, Vector3.back.z))
            {
                Int3 idx = CubeToIndex(hit.collider.gameObject);
                idx.z--;
                setIdx = idx;
            }
            //down
            else if (Mathf.Approximately(hit.normal.x, Vector3.down.x) && Mathf.Approximately(hit.normal.y, Vector3.down.y) && Mathf.Approximately(hit.normal.z, Vector3.down.z))
            {
                Int3 idx = CubeToIndex(hit.collider.gameObject);
                idx.y++;
                setIdx = idx;
            }
            //left
            else
            {
                Int3 idx = CubeToIndex(hit.collider.gameObject);
                idx.x--;
                setIdx = idx;
            }
            return hit.collider.gameObject;
        }

        setIdx = new Int3(-1, -1, -1);

        return null;
    }

    public void RenewalAdjacentCubeObject(Int3 cubeIdx)
    {
        Int3 up = cubeIdx; up.y++;
        Int3 forward = cubeIdx; forward.z++;
        Int3 right = cubeIdx; right.x++;
        Int3 down = cubeIdx; down.y--;
        Int3 back = cubeIdx; back.z--;
        Int3 left = cubeIdx; left.x--;

        bool tempEnable;
        if (hasCubeInfo(up))
        {
            tempEnable = tileEnable(up);
            getCube(up).SetActive(tempEnable);
            map[up.x, up.y, up.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(forward))
        {
            tempEnable = tileEnable(forward);
            getCube(forward).SetActive(tileEnable(forward));
            map[forward.x, forward.y, forward.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(right))
        {
            tempEnable = tileEnable(right);
            getCube(right).SetActive(tileEnable(right));
            map[right.x, right.y, right.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(down))
        {
            tempEnable = tileEnable(down);
            getCube(down).SetActive(tileEnable(down));
            map[down.x, down.y, down.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(back))
        {
            tempEnable = tileEnable(back);
            getCube(back).SetActive(tileEnable(back));
            map[back.x, back.y, back.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(left))
        {
            tempEnable = tileEnable(left);
            getCube(left).SetActive(tileEnable(left));
            map[left.x, left.y, left.z].isEnable = tempEnable;
        }
    }

    //큐브 설치
    public void setCube(Int3 cubeIdx)
    {
        GameObject obj = GameObject.Instantiate(CubeInstance);
        obj.transform.position = IndexToPosition(cubeIdx);
        map[cubeIdx.x, cubeIdx.y, cubeIdx.z].cube = obj;
        map[cubeIdx.x, cubeIdx.y, cubeIdx.z].isEnable = true;
        mapinfo[cubeIdx.x, cubeIdx.y, cubeIdx.z] = CubeInfo.Grass;

        RenewalAdjacentCubeObject(cubeIdx);
    }

    //큐브 파기
    public void grubCube(GameObject cube)
    {
        Int3 idx = CubeToIndex(cube);
        GameObject.Destroy(cube);
        map[idx.x, idx.y, idx.z].cube = null;
        map[idx.x, idx.y, idx.z].isEnable = false;
        mapinfo[idx.x, idx.y, idx.z] = CubeInfo.None;

        RenewalAdjacentCubeObject(idx);
    }
    
}