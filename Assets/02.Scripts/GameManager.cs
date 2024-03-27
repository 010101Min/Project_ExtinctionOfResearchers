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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void Report(GameObject reporter, GameObject corpse, GameObject suspect)
    {
        GameObject police = GameObject.FindGameObjectWithTag("Police");
        if (police != null)
        {
            // 경찰 출동 없이 쫓던 거나 계속 쫓음
            //police.GetComponent<PoliceController>().Report(reporter, corpse, suspect, 10 + (policeCount * 5));
            police.GetComponent<PoliceController>().Report(reporter, corpse, suspect, 10 + (policeCount * 5));
        }
        else
        {
            // 경찰 출동
        }
    }

    public void DestroyShortCut(GameObject shortCut)
    {
        // 엔지니어에게 전달
    }
}