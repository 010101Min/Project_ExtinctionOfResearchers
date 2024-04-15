using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNPC : MonoBehaviour
{
    public GameObject NPCPrefab;

    public float curTime = 0f;
    public int coolTime = 1;

    void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > coolTime)
        {
            curTime = 0f;
            MakeEnemy();
        }
    }

    void MakeEnemy()
    {
        float x = 12f;
        float y = Random.Range(-2f, 5f);
        float z = Random.Range(-5f, 0f);

        float rotx = Random.Range(-90f, 90f);
        float roty = Random.Range(-90f, 90f);
        float rotz = Random.Range(-90f, 90f);
        GameObject enemy = Instantiate(NPCPrefab, new Vector3(x, y, z), Quaternion.Euler(new Vector3(rotx, roty, rotz)));
    }
}
