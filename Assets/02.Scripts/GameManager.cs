using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private bool isGameOver = false;
    private bool isGamePaused = false;
    private bool isGameClear = false;
    private int policeCount = 0;

    public GameObject policePrefab;

    private void Awake()
    {
        if (GameManager.instance == null)
            GameManager.instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Report(GameObject reporter, GameObject suspect)
    {
        GameObject police = GameObject.FindGameObjectWithTag("Police");
        if (police != null)
        {
            // ���� �⵿ ���� �Ѵ� �ų� ��� ����
        }
        else
        {
            // ���� �⵿
        }
    }
}
