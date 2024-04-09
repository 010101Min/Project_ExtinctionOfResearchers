using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void SelectMapBtn() { SceneManager.LoadScene("SelectMapScene"); }
    public void GameStartBtn() { SceneManager.LoadScene("GameScene"); }
    public void MainMenuBtn() { SceneManager.LoadScene("MainScene"); }
    public void OptionBtn() { SceneManager.LoadScene("OptionScene"); }
    public void QuitBtn() { Application.Quit(); }
}
