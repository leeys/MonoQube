using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubePool
{
    private static CubePool sInstance;

    public static CubePool Instance
    {
        get {
            if (sInstance == null)
            {
                sInstance = new CubePool();
            }
            return sInstance;
        }
    }
    public GameObject baseObject { get; set; }
    public int startSize { get; set; }
    public int complementSize { get; set; }

    private int totalSize;
    private Queue<GameObject> q_ObjectPool = new Queue<GameObject>();

    private CubePool()
    {}

    public void Start()
    {
        Complement(startSize);
    }

    public GameObject ActiveObject()
    {
        if (q_ObjectPool.Count == 0)
            Complement();

        GameObject cube = q_ObjectPool.Dequeue();
        cube.SetActive(true);
        return cube;
    }

    public void DisableObject(GameObject cubeObj)
    {
        if (cubeObj == null) return;
        cubeObj.SetActive(false);
        q_ObjectPool.Enqueue(cubeObj);
    }

    public void Complement(int size = 0)
    {
        if (size <= 0)
            size = this.complementSize;
        totalSize += size;
        for (int i = 0; i < size; i++)
        {
            GameObject newGameObject = GameObject.Instantiate(baseObject);
            newGameObject.SetActive(false);
            q_ObjectPool.Enqueue(newGameObject);
        }    
    }
}
