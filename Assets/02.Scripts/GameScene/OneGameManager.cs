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

    public int policeCount = 0;

    private GameObject player;
    public GameObject policePrefab;
    public GameObject npcPrefab;
    public GameObject engineer;
    public GameObject ambulance;
    public GameObject teleport;
    public GameObject policeCarPrefab;
    public GameObject CameraPos;

    private GameObject[] bombs;
    private GameObject[] windows;

    public List<GameObject> Corpses = new List<GameObject>();

    public GameObject teleportEffect;

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
        bombs = GameObject.FindGameObjectsWithTag("Bomb");
        windows = GameObject.FindGameObjectsWithTag("Window");
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (OneGameUIController.Instance.isOptionPanelOpen())
            {
                GameContinued();
                OneGameUIController.Instance.ContinueGame();
            }
            else { GamePaused(); }
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            foreach (GameObject bomb in bombs) { if (bomb != null) bomb.GetComponent<BombController>().showCrossHair(); }
            foreach (GameObject window in windows) { if (window != null) window.GetComponent<WindowController>().showCrossHair(); }
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            foreach (GameObject bomb in bombs) { if (bomb != null) bomb.GetComponent<BombController>().hideCrossHair(); }
            foreach (GameObject window in windows) { if (window != null) window.GetComponent<WindowController>().hideCrossHair(); }
        }
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
            Camera.main.gameObject.transform.position = CameraPos.transform.position;
            Camera.main.gameObject.transform.SetParent(CameraPos.transform);
            CameraPos.GetComponent<CameraController>().fCameraRotate();
            isGameOver = true;
            OneGameUIController.Instance.hideTeleportPanel();
            OneGameUIController.Instance.showGameOverPanel(score);
        }
    }

    public void GameClear()
    {
        if (!isGameOver && !isGameClear && !isGamePaused)
        {
            Camera.main.gameObject.transform.position = CameraPos.transform.position;
            Camera.main.gameObject.transform.SetParent(CameraPos.transform);
            CameraPos.GetComponent<CameraController>().fCameraRotate();
            isGameClear = true;
            int timeBonus = 0;
            if (minutes < timeLimit)
            {
                timeBonus += ((timeLimit - minutes - 1) * 600);
                timeBonus += (60 - seconds) * 10;
            }
            OneGameUIController.Instance.hideTeleportPanel();
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
            score += addscore;
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
            if (targetKillCount >= targetCount)
            {
                GameObject teleport = GameObject.FindGameObjectWithTag("Teleport");
                player.GetComponent<PlayerController>().goalAchieved();
                OneGameUIController.Instance.showTeleportPanel();
                Instantiate(teleportEffect, new Vector3(teleport.transform.position.x, -0.5f, teleport.transform.position.z), Quaternion.identity);
            }
        }
    }

    public void Report(GameObject reporter, GameObject corpse, GameObject suspect)
    {
        if (corpse != null)
        {
            if (corpse.CompareTag("NPC")) { if (!corpse.GetComponent<NPCController>().fGetHidden()) { corpse.GetComponent<NPCController>().fDetected(); } }
            if (corpse.CompareTag("Police")) { if (!corpse.GetComponent<PoliceController>().fGetHidden()) { corpse.GetComponent<PoliceController>().fDetected(); } }
        }
        GameObject[] polices = GameObject.FindGameObjectsWithTag("Police");
        GameObject policeCar = GameObject.FindGameObjectWithTag("PoliceCar");
        bool PoliceExist = false;
        if (polices != null)
        {
            foreach (GameObject police in polices)
            {
                if (!police.GetComponent<PoliceController>().getDead())
                {
                    police.GetComponent<PoliceController>().Report(reporter, corpse, suspect, 15 + (policeCount * 5));
                    PoliceExist = true;
                }
            }
        }
        // 만약 살아있는 경찰이 없고, 경찰차도 없다면
        if (!PoliceExist && (policeCar == null || policeCar.GetComponent<PoliceCarController>().getLeave()))
        {
            // 경찰 출동
            GameObject policecar = Instantiate(policeCarPrefab);
            policecar.GetComponent<PoliceCarController>().GetReport(reporter, corpse, suspect, 15 + (policeCount * 5));
            policeCount++;
        }
        else if (!PoliceExist && (policeCar != null && !policeCar.GetComponent<PoliceCarController>().getLeave())) { StartCoroutine(WaitForPolice(reporter, corpse, suspect)); }
    }
    IEnumerator WaitForPolice(GameObject tempReporter, GameObject tempCorpse, GameObject tempSuspect)
    {
        // 만약 경찰은 없는데 경찰차는 있는 상태라면 경찰 들어올 때까지 대기하다가 넣어주기
        while (true)
        {
            GameObject[] polices = GameObject.FindGameObjectsWithTag("Police");
            if (polices != null)
            {
                foreach (GameObject police in polices)
                {
                    if (!police.GetComponent<PoliceController>().getDead())
                    {
                        police.GetComponent<PoliceController>().Report(tempReporter, tempCorpse, tempSuspect, 15 + (policeCount * 5));
                        yield break;
                    }
                }
            }
            yield return null;
        }
    }
    public void plusCorpse(List<GameObject> corpses)
    {
        foreach(GameObject corpse in corpses) { Corpses.Add(corpse); }
        if (Corpses.Count >= 3) { ParamedicReport(Corpses); Corpses.Clear(); }
    }

    public void DestroyShortCut(GameObject shortCut)
    {
        // 엔지니어에게 전달
        engineer.GetComponent<EngineerController>().Report(shortCut);
    }

    public void ParamedicReport(List<GameObject> corpses)
    {
        ambulance.GetComponent<AmbulanceController>().Report(corpses);
    }

    // 마우스 감도
    public void SensitivityUp() { player.GetComponent<PlayerLookAround>().SensitivityUp(); }
    public void SensitivityDown() { player.GetComponent<PlayerLookAround>().SensitivityDown(); }
    public float SensitivityShow() { return player.GetComponent<PlayerLookAround>().getSensitivity(); }
}