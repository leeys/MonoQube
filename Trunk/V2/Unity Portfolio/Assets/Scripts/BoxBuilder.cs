using UnityEngine;
using System.Collections;

public class BoxBuilder : MonoBehaviour {

    private MapGenerator map;
    private Int3 idx;
	// Use this for initialization
	void Start () {
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<MapGenerator>();
    }
	
	// Update is called once per frame
	void Update () {
        Int3 setIdx;

        if (Input.GetMouseButtonDown(0))
        {
            GameObject cube = map.RayCheckCube(Camera.main.transform.position, Camera.main.transform.forward, out setIdx);
            if(cube && cube != this.gameObject)
                map.grubCube(cube);
        }
        else if(Input.GetMouseButtonDown(1))
        {
            GameObject cube = map.RayCheckCube(Camera.main.transform.position, Camera.main.transform.forward, out setIdx);
            if(!map.isOverMap(setIdx))
                map.setCube(setIdx);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 50), idx.ToString());
    }
}
