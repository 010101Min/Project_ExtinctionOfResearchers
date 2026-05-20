using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Scrollbar BGM;
    public Scrollbar SFX;
    public Text Gamma;

    public GameObject MainPanel;
    public GameObject OptionPanel;

    public void SelectMapBtn() { Cursor.lockState = CursorLockMode.None; SceneManager.LoadScene("SelectMapScene"); BGMManager.instance.PlayMainBGM(); }
    public void GameStartBtn() { SceneManager.LoadScene("GameScene"); BGMManager.instance.PlayGameBGM(); }
    public void MainMenuBtn() { Cursor.lockState = CursorLockMode.None; SceneManager.LoadScene("MainScene"); BGMManager.instance.PlayMainBGM(); }
    public void OptionBtn() => OpenOptionPanel();
    public void QuitBtn() => Application.Quit();

    public void SaveBtn() => SaveOptions();
    public void OptionQuitBtn() => CloseOptionPanel();
    public void SaveAndQuitBtn()
    {
        SaveOptions();
        CloseOptionPanel();
    }

    private void Start()
    {
        BGM.value = BGMManager.instance.GetVolume();
        SFX.value = SFXManager.instance.GetVolume();
        if (!PlayerPrefs.HasKey("Gamma")) { PlayerPrefs.SetInt("Gamma", 12); Gamma.text = "12"; }
        else Gamma.text = PlayerPrefs.GetInt("Gamma").ToString();
    }

    private void OpenOptionPanel()
    {
        MainPanel.SetActive(false);
        OptionPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }
    public void SaveOptions()
    {
        SFXManager.instance.SetSave();
        BGMManager.instance.SetSave();
    }
    public void CloseOptionPanel()
    {
        BGMManager.instance.SetDefaultVolume();
        SFXManager.instance.SetDefaultVolume();
        BGM.value = PlayerPrefs.GetFloat("BGM");
        SFX.value = PlayerPrefs.GetFloat("SFX");

        MainPanel.SetActive(true);
        OptionPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
    }

    public void GammaUpBtn() {
        int now = PlayerPrefs.GetInt("Gamma");
        PlayerPrefs.SetInt("Gamma", now + 1);
        Gamma.text = (now + 1).ToString();
    }
    public void GammaDownBtn()
    {
        int now = PlayerPrefs.GetInt("Gamma");
        if (now <= 2) return;
        PlayerPrefs.SetInt("Gamma", now - 1);
        Gamma.text = (now - 1).ToString();
    }

    public void SetBGMVolume(float value) { BGMManager.instance.SetVolume(value); }
    public void SetSFXVolume(float value) { SFXManager.instance.SetVolume(value); }
}
