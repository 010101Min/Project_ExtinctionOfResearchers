using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class OneGameUIController : MonoBehaviour
{
    public static OneGameUIController Instance;

    public Text timeText;
    public Text scoreText;
    public Text researcherText;
    public Text allText;

    public Text AttackText;
    public Text ProvokeText;
    public Text CarryText;
    public Text DropText;
    public Text BombText;
    public Text FireText;
    public Text PoisonText;
    public Text ShortcutText;
    public Text AbandonText;
    public Text TeleportText;

    public GameObject OptionPanel;
    public Text OptionSensText;
    public GameObject GameOverPanel;
    public GameObject GameClearPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        timeText.color = Color.yellow;
        Clearall();
        ContinueGame();
    }

    // 좌상단 Text 함수들
    public void TimeText(int minutes, int seconds, int timeLimit)
    {
        timeText.text = string.Format("TIME: {0}:{1:00}", minutes, seconds);
        if (minutes >= timeLimit) { timeText.color = Color.white; }
    }
    public void ScoreText(int score)
    {
        scoreText.text = "SCORE: " + score.ToString();
    }
    public void targetCount(int target, int killtarget)
    {
        researcherText.text = "KILL TARGET: " + killtarget.ToString() + " / " + target.ToString();
        if (target == killtarget) { researcherText.color = Color.yellow; }
    }
    public void peopleCount(int all, int killall)
    {
        allText.text = "KILL ALL: " + killall.ToString() + " / " + all.ToString();
        if (all == killall) { allText.color = Color.yellow; }
    }

    // 각종 버튼 함수들
    public void ContinueGame() { OptionPanel.gameObject.SetActive(false); }
    public void RestartGame() { OneGameManager.Instance.GameContinued(); SceneManager.LoadScene(SceneManager.GetActiveScene().name); }


    // OptionPanel 함수
    public void showOptionPanel() { UpdateSensitivity(); OptionPanel.gameObject.SetActive(true); }
    
    // GameOverPanel 함수
    public void showGameOverPanel(int score)
    {
        GameOverPanel.gameObject.SetActive(true);
        Text scoreText = GameOverPanel.transform.Find("ScoreText").gameObject.GetComponent<Text>();
        scoreText.text = "SCORE: " + score.ToString();
    }

    // GameClearPanel 함수
    public void showGameClearPanel(int score, int bonus)
    {
        GameClearPanel.gameObject.SetActive(true);
        Text scoreText = GameClearPanel.transform.Find("ScoreText").gameObject.GetComponent<Text>();
        if (bonus == 0) { scoreText.text = "SCORE: " + score.ToString(); }
        else { scoreText.text = $"SCORE: {score} + {bonus} = {score + bonus}"; }
    }

    private void UpdateSensitivity() { OptionSensText.text = ((int)OneGameManager.Instance.SensitivityShow()/10).ToString(); }
    public void PushSensUpBtn() { OneGameManager.Instance.SensitivityUp(); UpdateSensitivity(); }
    public void PushSensDownBtn() { OneGameManager.Instance.SensitivityDown(); UpdateSensitivity(); }


    public void InAttack() { AttackText.gameObject.SetActive(true); }
    public void InProvoke() { ProvokeText.gameObject.SetActive(true); }
    public void InCarry() { CarryText.gameObject.SetActive(true); }
    public void InDrop() { DropText.gameObject.SetActive(true); }
    public void InBomb() { BombText.gameObject.SetActive(true); }
    public void InFire() { FireText.gameObject.SetActive(true); }
    public void InPoison() { PoisonText.gameObject.SetActive(true); }
    public void InShortcut() { ShortcutText.gameObject.SetActive(true); }
    public void InAbandon() { AbandonText.gameObject.SetActive(true); }
    public void InTeleport() { TeleportText.gameObject.SetActive(true); }

    public void Clearall()
    {
        AttackText.gameObject.SetActive(false);
        ProvokeText.gameObject.SetActive(false);
        CarryText.gameObject.SetActive(false);
        DropText.gameObject.SetActive(false);
        BombText.gameObject.SetActive(false);
        FireText.gameObject.SetActive(false);
        ShortcutText.gameObject.SetActive(false);
        PoisonText.gameObject.SetActive(false);
        AbandonText.gameObject.SetActive(false);
        TeleportText.gameObject.SetActive(false);
    }
}
