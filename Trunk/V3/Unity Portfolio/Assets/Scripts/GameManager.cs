using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    [System.Serializable]
    public struct CubePoolInfo
    {
        public GameObject cubeBase;
        public int cubePoolStartSize;
        public int complementSize;
        
    };

    public CubePoolInfo cubePoolInfo;
    // Use this for initialization
    void Start () {
        CubePool.Instance.startSize = cubePoolInfo.cubePoolStartSize;
        CubePool.Instance.complementSize = cubePoolInfo.complementSize;
        CubePool.Instance.baseObject = cubePoolInfo.cubeBase;
        CubePool.Instance.Start();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
