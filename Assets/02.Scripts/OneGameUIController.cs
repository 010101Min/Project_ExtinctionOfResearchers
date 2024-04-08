using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OneGameUIController : MonoBehaviour
{
    public static OneGameUIController Instance;

    public Text AttackText;
    public Text ProvokeText;
    public Text CarryText;
    public Text DropText;
    public Text BombText;
    public Text FireText;
    public Text PoisonText;
    public Text ShortcutText;
    public Text AbandonText;

    public GameObject OptionPanel;
    public GameObject GameOverPanel;
    public GameObject GameClearPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        Clearall();
        ContinueGame();
    }

    // 각종 버튼 함수들
    public void ContinueGame() { OptionPanel.gameObject.SetActive(false); }
    public void RestartGame() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }


    // OptionPanel 함수
    public void showOptionPanel() { OptionPanel.gameObject.SetActive(true); }
    
    // GameOverPanel 함수
    public void showGameOverPanel(int score)
    {
        GameOverPanel.gameObject.SetActive(true);
        Text scoreText = GameOverPanel.transform.Find("ScoreText").gameObject.GetComponent<Text>();
        scoreText.text = "SCORE: " + score.ToString();
    }

    // GameClearPanel 함수
    public void showGameClearPanel(int score)
    {
        GameClearPanel.gameObject.SetActive(true);
        Text scoreText = GameClearPanel.transform.Find("ScoreText").gameObject.GetComponent<Text>();
        scoreText.text = "SCORE: " + score.ToString();
    }

    public void InAttack() { AttackText.gameObject.SetActive(true); }
    public void InProvoke() { ProvokeText.gameObject.SetActive(true); }
    public void InCarry() { CarryText.gameObject.SetActive(true); }
    public void InDrop() { DropText.gameObject.SetActive(true); }
    public void InBomb() { BombText.gameObject.SetActive(true); }
    public void InFire() { FireText.gameObject.SetActive(true); }
    public void InPoison() { PoisonText.gameObject.SetActive(true); }
    public void InShortcut() { ShortcutText.gameObject.SetActive(true); }
    public void InAbandon() { AbandonText.gameObject.SetActive(true); }

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
    }
}
