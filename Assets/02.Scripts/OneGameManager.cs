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

    private int score = 0;
    private float time = 0;
    private int minutes = 0;
    private int seconds = 0;
    public int timeLimit = 5;

    private int policeCount = 0;

    public Text timeText;
    public Text scoreText;
    public Text resercherText;
    public Text allText;

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

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) { GamePaused(); }
        if (!isGameOver && !isGameClear && !isGamePaused)
        {
            time += Time.deltaTime;
            minutes = (int)time / 60;
            seconds = (int)time % 60;
            if (minutes <= timeLimit) { timeText.color = Color.yellow; }
            else { timeText.color = Color.white; }
            timeText.text = string.Format("TIME: {0}:{1:00}", (int)time / 60, (int)time % 60);
        }
        scoreText.text = "SCORE: " + score.ToString();
    }

    public void GameOver()
    {
        isGameOver = true;
        OneGameUIController.Instance.showGameOverPanel(score);
    }

    public void GameClear()
    {
        isGameClear = true;
        if (minutes <= timeLimit)
        {
            score += ((timeLimit - minutes - 1) * 600);
            score += (60 - seconds) * 10;
        }
        OneGameUIController.Instance.showGameClearPanel(score);
    }

    public void GamePaused()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
        OneGameUIController.Instance.showOptionPanel();
    }
    public void GameContinued()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
    }

    public void addScore(int score) { this.score += score; }

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
    public void plusCorpse(List<GameObject> corpses)
    {
        foreach(GameObject corpse in corpses) { Corpses.Add(corpse); }
        if (corpses.Count >= 3) { ParamedicReport(Corpses); Corpses.Clear(); }
    }

    public void DestroyShortCut(GameObject shortCut)
    {
        // 엔지니어에게 전달
        engineer.GetComponent<EngineerController>().Report(shortCut);
    }

    public void ParamedicReport(List<GameObject> corpses)
    {
        paramedic.GetComponent<ParamedicController>().Report(corpses);
    }
}