using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool isGameOver = false;
    private bool isGamePaused = false;
    private bool isGameClear = false;
    private int policeCount = 0;

    public GameObject policePrefab;
    public GameObject engineer;
    public GameObject policeCarPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {
        engineer = GameObject.Find("Engineer");
    }

    void Update()
    {

    }

    public void Report(GameObject reporter, GameObject corpse, GameObject suspect)
    {
        GameObject[] polices = GameObject.FindGameObjectsWithTag("Police");
        bool PoliceExist = false;
        if (polices != null)
        {
            // 경찰 출동 없이 쫓던 거나 계속 쫓음
            //police.GetComponent<PoliceController>().Report(reporter, corpse, suspect, 10 + (policeCount * 5));
            foreach (GameObject police in polices)
            {
                if (!police.GetComponent<PoliceController>().getDead())
                {
                    police.GetComponent<PoliceController>().Report(reporter, corpse, suspect, 15 + (policeCount * 5));
                    PoliceExist = true;
                }
            }
        }
        if (!PoliceExist)
        {
            // 경찰 출동
            GameObject policeCar = Instantiate(policeCarPrefab);
            GameObject police = Instantiate(policePrefab, policeCar.transform.position, Quaternion.identity);
            police.GetComponent<PoliceController>().Report(reporter, corpse, suspect, 15 + (policeCount * 5));
            policeCount++;
        }
    }

    public void DestroyShortCut(GameObject shortCut)
    {
        // 엔지니어에게 전달
        engineer.GetComponent<EngineerController>().Report(shortCut);
    }
}