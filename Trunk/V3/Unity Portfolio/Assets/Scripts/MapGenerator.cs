using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CubeInfo : byte { None = 0, Grass, Rock, };

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
[System.Serializable]
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

    public static bool operator ==(Int3 a, Int3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }
    public static bool operator !=(Int3 a, Int3 b)
    {
        return a.x != b.x || a.y != b.y || a.z != b.z;
    }

    public static Int3 operator -(Int3 a, Int3 b)
    {
        return new Int3(a.x - b.x,
                        a.y - b.y,
                        a.z - b.z);
    }
    public static Int3 operator +(Int3 a, Int3 b)
    {
        return new Int3(a.x + b.x,
                        a.y + b.y,
                        a.z + b.z);
    }
    public static Int3 operator *(Int3 a, float b)
    {
        return new Int3((int)(a.x * b),
                        (int)(a.y * b),
                        (int)(a.z * b));
    }
    public static Int3 operator /(Int3 a, float b)
    {
        return new Int3((int)(a.x / b),
                        (int)(a.y / b),
                        (int)(a.z / b));
    }

    public static int LengthSq(Int3 v)
    {
        return (v.x * v.x) + (v.y * v.y) + (v.z * v.z);
    }
    public static float Length(Int3 v)
    {
        return Mathf.Sqrt((v.x * v.x) + (v.y * v.y) + (v.z * v.z));
    }
}

//큐브, 지역
public enum OpenSide { FORWARD = 1, BACK = 2, UP = 4, DOWN = 8, RIGHT = 16, LEFT = 32 };

public class MapGenerator : MonoBehaviour
{
    
    public struct CubeState
    {
        //Flag
        public int isEnable;
        public CubeInfo mapinfo;
    };

    public class Area
    {
        public CubeArea cubeArea = null;
        public static int divideSize = 16;
        public static int farSize = 128;

        public Area(Vector3 center, Vector3 areaSize, Vector3 halfSize, MapGenerator map)
        {
            this.map = map;
            this.center = center;
            min = center - areaSize;
            max = center + areaSize;
            halfMin = map.PositionToIndex(center - halfSize);
            halfMax = map.PositionToIndex(center + halfSize);

            int halfMinY = halfMin.y;
            halfMin.y = halfMax.y;
            halfMax.y = halfMinY;

            cubeArea = CubePool.Instance.ActiveObject().GetComponent<CubeArea>();
        }

        public void Draw(bool setDraw)
        {
            if (setDraw)
            {
                if (!cubeArea.gameObject.activeSelf)
                {
                    cubeArea.gameObject.SetActive(true);
                }
            }
            else if (cubeArea.gameObject.activeSelf)
            {
                cubeArea.gameObject.SetActive(false);
            }
        }

        public bool OpenCheck()
        {
            int oldFlag = openFlag;
            openFlag = 0;

            for (int x = halfMin.x; x < halfMax.x; x++)
                for (int y = halfMin.y; y < halfMax.y; y++)
                {
                    if (map.map[x, y, halfMax.z - 1].mapinfo == CubeInfo.None)
                    {
                        openFlag |= (int)OpenSide.FORWARD;
                        goto endForward;
                    }
                }
            endForward:

            for (int x = halfMin.x; x < halfMax.x; x++)
                for (int y = halfMin.y; y < halfMax.y; y++)
                {
                    if (map.map[x, y, halfMin.z].mapinfo == CubeInfo.None)
                    {
                        openFlag |= (int)OpenSide.BACK;
                        goto endBack;
                    }
                }
            endBack:

            for (int x = halfMin.x; x < halfMax.x; x++)
                for (int z = halfMin.z; z < halfMax.z; z++)
                {
                    if (map.map[x, halfMax.y - 1, z].mapinfo == CubeInfo.None)
                    {
                        openFlag |= (int)OpenSide.DOWN;
                        goto endDown;
                    }
                }
            endDown:

            for (int x = halfMin.x; x < halfMax.x; x++)
                for (int z = halfMin.z; z < halfMax.z; z++)
                {
                    if (map.map[x, halfMin.y, z].mapinfo == CubeInfo.None)
                    {
                        openFlag |= (int)OpenSide.UP;
                        goto endUp;
                    }
                }
            endUp:

            for (int y = halfMin.y; y < halfMax.y; y++)
                for (int z = halfMin.z; z < halfMax.z; z++)
                {
                    if (map.map[halfMax.x - 1, y, z].mapinfo == CubeInfo.None)
                    {
                        openFlag |= (int)OpenSide.RIGHT;
                        goto endRight;
                    }
                }
            endRight:

            for (int y = halfMin.y; y < halfMax.y; y++)
                for (int z = halfMin.z; z < halfMax.z; z++)
                {
                    if (map.map[halfMin.x, y, z].mapinfo == CubeInfo.None)
                    {
                        openFlag |= (int)OpenSide.LEFT;
                        goto endLeft;
                    }
                }
            endLeft:;

            return (oldFlag == openFlag);
        }
        

        public Vector3 center;
        public Vector3 min;
        public Vector3 max;
        public Int3 halfMin;
        public Int3 halfMax;
        public MapGenerator map;

        public int openFlag { get; private set; }
        public bool isIn;
    }

    public Transform player;
    [Range(0, 100)]
    public float randomFillPercent;

    public Int3 size = new Int3(128, 32, 128);
    public GameObject CubeInstance;


    public CubeState[,,] map { get; private set; }
    private Area[,,] area;

    private System.Random pseudoRandom;
    //private List<GameObject> l_CollisionCube = new List<GameObject>();
    int layerFlag;

    int nowCubeNum;

    // Use this for initialization
    void Start()
    {
        layerFlag = 8;

        Load();

        StartCoroutine("AreaCulling");
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator AreaCulling()
    {
        int areaFarSq = (Area.farSize / Area.divideSize);
        areaFarSq *= areaFarSq;

        Int3 oldAreaIdx = new Int3(-999, -999, -999);

        
        while (true)
        {
            Int3 nowAreaIdx = PositionToAreaIndex(player.position);
            if (nowAreaIdx != oldAreaIdx)
            {
                for (int y = 0; y < area.GetLength(1); y++)
                {
                    for (int x = 0; x < area.GetLength(0); x++)
                    {
                        for (int z = 0; z < area.GetLength(2); z++)
                        {
                            Int3 areaToNowArea = nowAreaIdx - new Int3(x, y, z);
                            int lengthSq = Int3.LengthSq(areaToNowArea);
                            if ((area[x, y, z].isIn = (lengthSq < areaFarSq)) == false)
                            {
                                area[x, y, z].Draw(false);
                            }
                        }
                    }
                }
                OpenAreaCheck(nowAreaIdx);
                oldAreaIdx = nowAreaIdx;
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }
    }

    void OpenAreaCheck(Int3 nowAreaIdx)
    {
        //인접한 구역 탐색
        Queue<Int3> stackData = new Queue<Int3>();
        if (hasAreaInfo(nowAreaIdx))
            stackData.Enqueue(nowAreaIdx);
        while (stackData.Count != 0)
        {
            Int3 areaIdx = stackData.Dequeue();
            if (area[areaIdx.x, areaIdx.y, areaIdx.z].isIn)
            {
                area[areaIdx.x, areaIdx.y, areaIdx.z].isIn = false;
                area[areaIdx.x, areaIdx.y, areaIdx.z].Draw(true);

                Int3 adjacentIdx;

                //Forward
                if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.FORWARD) != 0)
                {
                    adjacentIdx = areaIdx + new Int3(0, 0, 1);
                    if (hasAreaInfo(adjacentIdx))
                    {
                        if (area[adjacentIdx.x, adjacentIdx.y, adjacentIdx.z].isIn)
                        {
                            stackData.Enqueue(adjacentIdx);
                        }
                    }
                }
                //Back
                if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.BACK) != 0)
                {
                    adjacentIdx = areaIdx + new Int3(0, 0, -1);
                    if (hasAreaInfo(adjacentIdx))
                    {
                        if (area[adjacentIdx.x, adjacentIdx.y, adjacentIdx.z].isIn)
                        {
                            stackData.Enqueue(adjacentIdx);
                        }
                    }
                }
                //Up
                if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.UP) != 0)
                {
                    adjacentIdx = areaIdx + new Int3(0, -1, 0);
                    if (hasAreaInfo(adjacentIdx))
                    {
                        if (area[adjacentIdx.x, adjacentIdx.y, adjacentIdx.z].isIn)
                        {
                            stackData.Enqueue(adjacentIdx);
                        }
                    }
                }
                //Down
                if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.DOWN) != 0)
                {
                    adjacentIdx = areaIdx + new Int3(0, 1, 0);
                    if (hasAreaInfo(adjacentIdx))
                    {
                        if (area[adjacentIdx.x, adjacentIdx.y, adjacentIdx.z].isIn)
                        {
                            stackData.Enqueue(adjacentIdx);
                        }
                    }
                }
                //Right
                if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.RIGHT) != 0)
                {
                    adjacentIdx = areaIdx + new Int3(1, 0, 0);
                    if (hasAreaInfo(adjacentIdx))
                    {
                        if (area[adjacentIdx.x, adjacentIdx.y, adjacentIdx.z].isIn)
                        {
                            stackData.Enqueue(adjacentIdx);
                        }
                    }
                }
                //Left
                if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.LEFT) != 0)
                {
                    adjacentIdx = areaIdx + new Int3(-1, 0, 0);
                    if (hasAreaInfo(adjacentIdx))
                    {
                        if (area[adjacentIdx.x, adjacentIdx.y, adjacentIdx.z].isIn)
                        {
                            stackData.Enqueue(adjacentIdx);
                        }
                    }
                }
            }
        }
        int areaFar = (Area.farSize / Area.divideSize);
        Int3 minIdx = nowAreaIdx - new Int3(areaFar, areaFar, areaFar);
        minIdx.x = Mathf.Max(minIdx.x, 0);
        minIdx.y = Mathf.Max(minIdx.y, 0);
        minIdx.z = Mathf.Max(minIdx.z, 0);

        Int3 maxIdx = nowAreaIdx + new Int3(areaFar, areaFar, areaFar);
        maxIdx.x = Mathf.Min(maxIdx.x, area.GetLength(0));
        maxIdx.y = Mathf.Min(maxIdx.y, area.GetLength(1));
        maxIdx.z = Mathf.Min(maxIdx.z, area.GetLength(2));

        int x = 0, y = 0, z = 0;
            for (x = minIdx.x; x < maxIdx.x; x++)
            {
                for (y = minIdx.y; y < maxIdx.y; y++)
                {
                    for (z = minIdx.z; z < maxIdx.z; z++)
                    {
                        if (area[x, y, z].isIn)
                        {
                            area[x, y, z].Draw(false);
                        }
                    }
                }
            }
    }
    
    //MapLoad
    void Load()
    {
        Allocate();
        MakeRandomMap();
        ActiveCalculate();
    }

    void RefreshCulling()
    {

    }

    //메모리 할당
    void Allocate()
    {
        map = new CubeState[(int)size.x, (int)size.y * 2, (int)size.z];

        Area.divideSize = 16;
        Area.farSize = 256;

        int divideX = (int)size.x / Area.divideSize;
        int divideY = (int)(size.y * 2) / Area.divideSize;
        int divideZ = (int)size.z / Area.divideSize;

        area = new Area[divideX, divideY, divideZ];


        for (int x = 0; x < divideX; x++)
        {
            for (int y = 0; y < divideY; y++)
            {
                for (int z = 0; z < divideZ; z++)
                {
                    area[x, y, z] = new Area(new Vector3(Area.divideSize * 0.5f, -Area.divideSize * 0.5f, Area.divideSize * 0.5f) + IndexToPosition(new Int3(x * Area.divideSize, y * Area.divideSize, z * Area.divideSize)), new Vector3(Area.farSize, Area.farSize, Area.farSize), new Vector3(Area.divideSize * 0.5f, Area.divideSize * 0.5f, Area.divideSize * 0.5f), this);
                }
            }
        }
    }

    //랜덤맵 생성
    void MakeRandomMap()
    {
        pseudoRandom = new System.Random(0);
        
        //지하 높이
        int groundHeight = (int)size.y + 1;
        //토양 두께
        int soilHeight = 1;
        int skyHeight = map.GetLength(1) - groundHeight;
        
        //지하 동굴 생성
        for (int y = 0; y < groundHeight; y++)
            for (int x = 0; x < map.GetLength(0); x++)
                for (int z = 0; z < map.GetLength(2); z++)
                {
                    if (x == 0 || x == (map.GetLength(0) - 1) ||
                       y == 0 || y == groundHeight - 1 ||
                       z == 0 || z == (map.GetLength(2) - 1))
                        map[x, skyHeight + y, z].mapinfo = CubeInfo.Grass;
                    else
                        map[x, skyHeight + y, z].mapinfo = (pseudoRandom.Next(0, 100) < randomFillPercent) ? CubeInfo.Grass : CubeInfo.None;
                }
        for (int i = 0; i < 3; i++)
        {
            SmoothMap(skyHeight);
        }

        int groundToHeight = Mathf.Min(groundHeight + 1 + soilHeight, map.GetLength(1));
        for (int y = groundHeight + 1; y < groundToHeight; y++)
            for (int x = 0; x < map.GetLength(0); x++)
                for (int z = 0; z < map.GetLength(2); z++)
                {
                    map[x, y, z].mapinfo = CubeInfo.Grass;
                }

        ////산 만들기
        MakeRandomMountain(groundHeight + soilHeight, 0.1f, 1, 8, 3);

        
    }

    void MakeRandomMountain(int startHeight, float onePerHorizontalArea, int minHeight, int maxHeight, int avgRipple)
    {
        
        int randHeight = startHeight - pseudoRandom.Next(minHeight, maxHeight);

        for (int x = 0; x < map.GetLength(0); x++)
            for (int z = 0; z < map.GetLength(2); z++)
            {
                for (int y = randHeight; y < startHeight; y++)
                {
                    map[x, y, z].mapinfo = CubeInfo.Grass;
                }
                randHeight = startHeight - pseudoRandom.Next(minHeight, maxHeight);
            }
    }

    void SmoothMap(int skyHeight)
    {
        CubeInfo[,,] temp = new CubeInfo[map.GetLength(0), map.GetLength(1), map.GetLength(2)];

        for (int x = 1; x < map.GetLength(0) - 1; x++)
        {
            for (int y = skyHeight + 1; y < map.GetLength(1) - 1; y++)
            {
                for (int z = 1; z < map.GetLength(2) - 1; z++)
                {
                    int neighbourWallTiles = GetSurroundingCubeCount(x, y, z);

                    if (neighbourWallTiles > 13)
                        temp[x, y, z] = CubeInfo.Grass;
                    else if (neighbourWallTiles < 13)
                        temp[x, y, z] = CubeInfo.None;
                }

            }
        }
        for (int x = 1; x < map.GetLength(0) - 1; x++)
        {
            for (int y = skyHeight + 1; y < map.GetLength(1) - 1; y++)
            {
                for (int z = 1; z < map.GetLength(2) - 1; z++)
                {
                    map[x, y, z].mapinfo = temp[x, y, z];
                }
            }
        }
    }

    //큐브 컬링 계산
    void ActiveCalculate()
    {
        for (int x = 0; x < map.GetLength(0); x++)
            for (int z = 0; z < map.GetLength(2); z++)
                for (int y = 0; y < map.GetLength(1); y++)
                    if (hasCubeInfo(x, y, z))
                    {
                        map[x, y, z].isEnable = tileEnable(x, y, z);
                    }

        for (int x = 0; x < area.GetLength(0); x++)
            for (int y = 0; y < area.GetLength(1); y++)
                for (int z = 0; z < area.GetLength(2); z++)
                {
                    area[x, y, z].cubeArea.GenerateMesh(area[x, y, z]);
                    area[x, y, z].OpenCheck();
                }
    }
    int GetSurroundingCubeCount(int ix, int iy, int iz)
    {
        int count = 0;
        for (int x = ix - 1; x <= ix + 1; x++)
            for (int y = iy - 1; y <= iy + 1; y++)
                for (int z = iz - 1; z <= iz + 1; z++)
                {
                    if (ix != x || iy != y || iz != z)
                    {
                        if (hasCubeInfo(x, y, z))
                        {
                            count++;
                        }
                    }
                }

        return count;
    }

    void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 200, 50), nowCubeNumString);
    }

    //큐브가 그려져야되는지
    public int tileEnable(int x, int y, int z)
    {
        int openFlag = 0;
        /*
        if (!isOverMap(x + 1, y, z))
            if (!hasCubeInfo(x + 1, y, z))
                return true;
        if (!isOverMap(x - 1, y, z))
            if (!hasCubeInfo(x - 1, y, z))
                return true;
        if (!isOverMap(x, y + 1, z))
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
        */
        if (!hasCubeInfo(x + 1, y, z))
            openFlag |= (int)OpenSide.RIGHT;
        if (!hasCubeInfo(x - 1, y, z))
            openFlag |= (int)OpenSide.LEFT;
        if (!hasCubeInfo(x, y + 1, z))
            openFlag |= (int)OpenSide.DOWN;
        if (!hasCubeInfo(x, y - 1, z))
            openFlag |= (int)OpenSide.UP;
        if (!hasCubeInfo(x, y, z + 1))
            openFlag |= (int)OpenSide.FORWARD;
        if (!hasCubeInfo(x, y, z - 1))
            openFlag |= (int)OpenSide.BACK;

        return openFlag;
    }
    public int tileEnable(Int3 index)
    {
        return tileEnable(index.x, index.y, index.z);
    }

    //큐브가 맵정보상으로 존재하는지
    public bool hasCubeInfo(int x, int y, int z)
    {
        if (x >= 0 && x < map.GetLength(0)
            && y >= 0 && y < map.GetLength(1)
            && z >= 0 && z < map.GetLength(2))
                return (map[x, y, z].mapinfo != CubeInfo.None);
        else return false;
    }
    public bool hasCubeInfo(Int3 index)
    {
        return hasCubeInfo(index.x, index.y, index.z);
    }

    //인덱스가 맵밖으로 나갔는지
    public bool isOverMap(int x, int y, int z)
    {
        return x < 0 || x >= map.GetLength(0)
|| y < 0 || y >= map.GetLength(1)
|| z < 0 || z >= map.GetLength(2);
    }
    public bool isOverMap(Int3 idx)
    {
        return isOverMap(idx.x, idx.y, idx.z);
    }
    

    public Int3 PositionToIndex(Vector3 position)
    {
        return new Int3(Mathf.RoundToInt(position.x + (size.x * 0.5f)),
            Mathf.RoundToInt(size.y - position.y),
            Mathf.RoundToInt(position.z + (size.z * 0.5f)));
    }
    public Vector3 IndexToPosition(Int3 index)
    {
        return new Vector3(-size.x * 0.5f + index.x, (int)(size.y) - index.y, -size.z * 0.5f + index.z);
    }
    public Int3 CubeToIndex(GameObject cube)
    {
        return PositionToIndex(cube.transform.position);
    }

    //Area 인덱스 관련
    public Int3 PositionToAreaIndex(Vector3 position)
    {
        Int3 positionIdx = PositionToIndex(position);
        return positionIdx /= Area.divideSize;
    }

    public bool hasAreaInfo(int x, int y, int z)
    {
        return (x >= 0 && x < area.GetLength(0)
            && y >= 0 && y < area.GetLength(1)
            && z >= 0 && z < area.GetLength(2));
    }
    public bool hasAreaInfo(Int3 index)
    {
        return hasAreaInfo(index.x, index.y, index.z);
    }
    
    public Int3 RayCheckCube(Vector3 eye, Vector3 dir, out Int3 setIdx)
    {

        Ray ray = new Ray(eye, dir.normalized);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 3);

        Int3 hitIdx;

        if (hit.collider != null)
        {
            //forward
            if (Mathf.Approximately(hit.normal.x, Vector3.forward.x) && Mathf.Approximately(hit.normal.y, Vector3.forward.y) && Mathf.Approximately(hit.normal.z, Vector3.forward.z))
            {
                Int3 idx = hitIdx = PositionToIndex(hit.point + new Vector3(0, 0, -0.5f));
                idx.z++;
                setIdx = idx;
            }
            //up
            else if (Mathf.Approximately(hit.normal.x, Vector3.up.x) && Mathf.Approximately(hit.normal.y, Vector3.up.y) && Mathf.Approximately(hit.normal.z, Vector3.up.z))
            {
                Int3 idx = hitIdx = PositionToIndex(hit.point + new Vector3(0, -0.5f, 0));
                idx.y--;
                setIdx = idx;
            }
            //right
            else if (Mathf.Approximately(hit.normal.x, Vector3.right.x) && Mathf.Approximately(hit.normal.y, Vector3.right.y) && Mathf.Approximately(hit.normal.z, Vector3.right.z))
            {
                Int3 idx = hitIdx = PositionToIndex(hit.point + new Vector3(-0.5f, 0, 0));
                idx.x++;
                setIdx = idx;
            }
            //back
            else if (Mathf.Approximately(hit.normal.x, Vector3.back.x) && Mathf.Approximately(hit.normal.y, Vector3.back.y) && Mathf.Approximately(hit.normal.z, Vector3.back.z))
            {
                Int3 idx = hitIdx = PositionToIndex(hit.point + new Vector3(0, 0, 0.5f));
                idx.z--;
                setIdx = idx;
            }
            //down
            else if (Mathf.Approximately(hit.normal.x, Vector3.down.x) && Mathf.Approximately(hit.normal.y, Vector3.down.y) && Mathf.Approximately(hit.normal.z, Vector3.down.z))
            {
                Int3 idx = hitIdx = PositionToIndex(hit.point + new Vector3(0, 0.5f, 0));
                idx.y++;
                setIdx = idx;
            }
            //left
            else
            {
                Int3 idx = hitIdx = PositionToIndex(hit.point + new Vector3(0.5f, 0, 0));
                idx.x--;
                setIdx = idx;
            }
            return hitIdx;
        }

        setIdx = new Int3(-1, -1, -1);

        return new Int3(-1,-1,-1);
    }

    public void RenewalAdjacentCubeObject(Int3 cubeIdx)
    {
        Int3 up = cubeIdx; up.y--;
        Int3 forward = cubeIdx; forward.z++;
        Int3 right = cubeIdx; right.x++;
        Int3 down = cubeIdx; down.y++;
        Int3 back = cubeIdx; back.z--;
        Int3 left = cubeIdx; left.x--;

        int tempEnable;
        if (hasCubeInfo(up))
        {
            tempEnable = tileEnable(up);
            map[up.x, up.y, up.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(forward))
        {
            tempEnable = tileEnable(forward);
            map[forward.x, forward.y, forward.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(right))
        {
            tempEnable = tileEnable(right);
            map[right.x, right.y, right.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(down))
        {
            tempEnable = tileEnable(down);
            map[down.x, down.y, down.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(back))
        {
            tempEnable = tileEnable(back);
            map[back.x, back.y, back.z].isEnable = tempEnable;
        }
        if (hasCubeInfo(left))
        {
            tempEnable = tileEnable(left);
            map[left.x, left.y, left.z].isEnable = tempEnable;
        }
    }

    //큐브 설치
    public void setCube(Int3 cubeIdx)
    {
        map[cubeIdx.x, cubeIdx.y, cubeIdx.z].isEnable = tileEnable(cubeIdx);
        map[cubeIdx.x, cubeIdx.y, cubeIdx.z].mapinfo = CubeInfo.Grass;

        if (!isOverMap(cubeIdx))
        {
            RenewalAdjacentCubeObject(cubeIdx);

            Int3 areaIdx = PositionToAreaIndex(IndexToPosition(cubeIdx));
            area[areaIdx.x, areaIdx.y, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y, areaIdx.z]);

            int oldOpenFlag = area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag;
            area[areaIdx.x, areaIdx.y, areaIdx.z].OpenCheck();
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.FORWARD) != 0 && areaIdx.z + 1 < area.GetLength(2))
            {
                area[areaIdx.x, areaIdx.y, areaIdx.z + 1].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y, areaIdx.z + 1]);
                area[areaIdx.x, areaIdx.y, areaIdx.z + 1].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.BACK) != 0 && areaIdx.z - 1 >= 0)
            {
                area[areaIdx.x, areaIdx.y, areaIdx.z - 1].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y, areaIdx.z - 1]);
                area[areaIdx.x, areaIdx.y, areaIdx.z - 1].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.RIGHT) != 0 && areaIdx.x + 1 < area.GetLength(0))
            {
                area[areaIdx.x + 1, areaIdx.y, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x + 1, areaIdx.y, areaIdx.z]);
                area[areaIdx.x + 1, areaIdx.y, areaIdx.z].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.LEFT) != 0 && areaIdx.x - 1 >= 0)
            {
                area[areaIdx.x - 1, areaIdx.y, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x - 1, areaIdx.y, areaIdx.z]);
                area[areaIdx.x - 1, areaIdx.y, areaIdx.z].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.UP) != 0 && areaIdx.y - 1 >= 0)
            {
                area[areaIdx.x, areaIdx.y - 1, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y - 1, areaIdx.z]);
                area[areaIdx.x, areaIdx.y - 1, areaIdx.z].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.DOWN) != 0 && areaIdx.y + 1 < area.GetLength(1))
            {
                area[areaIdx.x, areaIdx.y + 1, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y + 1, areaIdx.z]);
                area[areaIdx.x, areaIdx.y + 1, areaIdx.z].cubeArea.gameObject.SetActive(true);
            }
        }
    }

    //큐브 파기
    public void grubCube(Int3 cubeIdx)
    {
        map[cubeIdx.x, cubeIdx.y, cubeIdx.z].isEnable = 0;
        map[cubeIdx.x, cubeIdx.y, cubeIdx.z].mapinfo = CubeInfo.None;

        if (!isOverMap(cubeIdx))
        {
            RenewalAdjacentCubeObject(cubeIdx);

            Int3 areaIdx = PositionToAreaIndex(IndexToPosition(cubeIdx));
            area[areaIdx.x, areaIdx.y, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y, areaIdx.z]);



            area[areaIdx.x, areaIdx.y, areaIdx.z].OpenCheck();

            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.FORWARD) != 0 && areaIdx.z + 1 < area.GetLength(2))
            {
                area[areaIdx.x, areaIdx.y, areaIdx.z + 1].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y, areaIdx.z + 1]);
                area[areaIdx.x, areaIdx.y, areaIdx.z + 1].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.BACK) != 0 && areaIdx.z - 1 >= 0)
            {
                area[areaIdx.x, areaIdx.y, areaIdx.z - 1].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y, areaIdx.z - 1]);
                area[areaIdx.x, areaIdx.y, areaIdx.z - 1].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.RIGHT) != 0 && areaIdx.x + 1 < area.GetLength(0))
            {
                area[areaIdx.x + 1, areaIdx.y, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x + 1, areaIdx.y, areaIdx.z]);
                area[areaIdx.x + 1, areaIdx.y, areaIdx.z].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.LEFT) != 0 && areaIdx.x - 1 >= 0)
            {
                area[areaIdx.x - 1, areaIdx.y, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x - 1, areaIdx.y, areaIdx.z]);
                area[areaIdx.x - 1, areaIdx.y, areaIdx.z].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.UP) != 0 && areaIdx.y - 1 >= 0)
            {
                area[areaIdx.x, areaIdx.y - 1, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y - 1, areaIdx.z]);
                area[areaIdx.x, areaIdx.y - 1, areaIdx.z].cubeArea.gameObject.SetActive(true);
            }
            if ((area[areaIdx.x, areaIdx.y, areaIdx.z].openFlag & (int)OpenSide.DOWN) != 0 && areaIdx.y + 1 < area.GetLength(1))
            {
                area[areaIdx.x, areaIdx.y + 1, areaIdx.z].cubeArea.GenerateMesh(area[areaIdx.x, areaIdx.y + 1, areaIdx.z]);
                area[areaIdx.x, areaIdx.y + 1, areaIdx.z].cubeArea.gameObject.SetActive(true);
            }
        }
    }

}