using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class OneGameManager : MonoBehaviour
{
    public static OneGameManager Instance;

    private bool isGameOver = false;
    private bool isGamePaused = false;
    private bool isGameClear = false;
    private int policeCount = 0;

    public GameObject policePrefab;
    public GameObject engineer;
    public GameObject paramedic;
    public GameObject policeCarPrefab;

    public List<GameObject> Corpses = new List<GameObject>();

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
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            OneGameUIController.Instance.showOptionPanel();
        }
    }

    public void Report(GameObject reporter, GameObject corpse, GameObject suspect)
    {
        GameObject[] polices = GameObject.FindGameObjectsWithTag("Police");
        bool PoliceExist = false;
        if (polices != null)
        {
            // ���� �⵿ ���� �Ѵ� �ų� ��� ����
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
            // ���� �⵿
            GameObject policeCar = Instantiate(policeCarPrefab);
            GameObject police = Instantiate(policePrefab, policeCar.transform.position, Quaternion.identity);
            police.GetComponent<PoliceController>().Report(reporter, corpse, suspect, 15 + (policeCount * 5));
            policeCount++;
        }
    }
    public void plusCorpse(List<GameObject> corpses)
    {
        foreach(GameObject corpse in corpses) { Corpses.Add(corpse); }
        if (corpses.Count >= 3) { ParamedicReport(Corpses); Corpses.Clear(); }
    }

    public void DestroyShortCut(GameObject shortCut)
    {
        // �����Ͼ�� ����
        engineer.GetComponent<EngineerController>().Report(shortCut);
    }

    public void ParamedicReport(List<GameObject> corpses)
    {
        paramedic.GetComponent<ParamedicController>().Report(corpses);
    }
}