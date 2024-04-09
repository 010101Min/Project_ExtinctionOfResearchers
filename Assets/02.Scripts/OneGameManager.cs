using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OneGameManager : MonoBehaviour
{
    public static OneGameManager Instance;

    private bool isGameOver = false;
    private bool isGamePaused = false;
    private bool isGameClear = false;

    public int peopleCount;
    private int targetCount;
    private int peopleKillCount = 0;
    private int targetKillCount = 0;

    private int score = 0;
    private float time = 0;
    private int minutes = 0;
    private int seconds = 0;
    public int timeLimit = 1;

    private int policeCount = 0;

    private GameObject player;
    public GameObject policePrefab;
    public GameObject npcPrefab;
    public GameObject engineer;
    public GameObject paramedic;
    public GameObject teleport;
    public GameObject policeCarPrefab;
    public GameObject CameraPos;

    public List<GameObject> Corpses = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        player = GameObject.Find("Player");

        targetCount = peopleCount / 6;
        for (int i = 0; i < peopleCount; i++)
        {
            GameObject npc = Instantiate(npcPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("NPCS").transform);
            if (i < targetCount) { npc.GetComponent<NPCController>().RandomResearcher(); }
        }
        OneGameUIController.Instance.targetCount(targetCount, 0);
        OneGameUIController.Instance.peopleCount(peopleCount, 0);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) { GamePaused(); }
        if (!isGameOver && !isGameClear && !isGamePaused)
        {
            time += Time.deltaTime;
            minutes = (int)time / 60;
            seconds = (int)time % 60;
            OneGameUIController.Instance.TimeText(minutes, seconds, timeLimit);
        }
    }

    public void GameOver()
    {
        if (!isGameOver && !isGameClear && !isGamePaused)
        {
            Camera.main.gameObject.transform.SetParent(CameraPos.transform);
            Camera.main.gameObject.transform.position = Vector3.zero + 5 * Vector3.up;
            CameraPos.GetComponent<CameraController>().fCameraRotate();
            isGameOver = true;
            OneGameUIController.Instance.showGameOverPanel(score);
        }
    }

    public void GameClear()
    {
        if (!isGameOver && !isGameClear && !isGamePaused)
        {
            Camera.main.gameObject.transform.SetParent(CameraPos.transform);
            Camera.main.gameObject.transform.position = Vector3.zero + 3 * Vector3.up;
            CameraPos.GetComponent<CameraController>().fCameraRotate();
            isGameClear = true;
            int timeBonus = 0;
            if (minutes < timeLimit)
            {
                timeBonus += ((timeLimit - minutes - 1) * 600);
                timeBonus += (60 - seconds) * 10;
            }
            OneGameUIController.Instance.showGameClearPanel(score, timeBonus);

            player.GetComponent<PlayerController>().fHideIcon();
            Destroy(player.gameObject);
        }
    }

    public void GamePaused()
    {
        if (!isGameOver && !isGameClear && !isGamePaused)
        {
            isGamePaused = true;
            Time.timeScale = 0f;
            OneGameUIController.Instance.showOptionPanel();
        }
    }
    public void GameContinued()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
    }

    public void addScore(int addscore)
    {
        if (!isGameOver && !isGameClear && !isGamePaused)
        {
            score += addscore; Debug.Log("���� ����: " + score.ToString());
            OneGameUIController.Instance.ScoreText(score);
        }
    }

    public void killPeopleCount()
    {
        if (!isGameOver && !isGameClear && !isGamePaused)
        {
            peopleKillCount++;
            OneGameUIController.Instance.peopleCount(peopleCount, peopleKillCount);
        }
    }
    public void killTargetCount()
    {
        if (!isGameOver && !isGameClear && !isGamePaused)
        {
            targetKillCount++;
            OneGameUIController.Instance.targetCount(targetCount, targetKillCount);
            if (targetKillCount >= targetCount) { player.GetComponent<PlayerController>().goalAchieved(); }
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