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

    public void showOptionPanel() { OptionPanel.gameObject.SetActive(true); Time.timeScale = 0f; }
    public void ContinueGame() { OptionPanel.gameObject.SetActive(false); Time.timeScale = 1f; }
    public void RestartGame() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
}
