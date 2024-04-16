using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void SelectMapBtn() { Cursor.lockState = CursorLockMode.None; SceneManager.LoadScene("SelectMapScene"); }
    public void GameStartBtn() { SceneManager.LoadScene("GameScene"); }
    public void MainMenuBtn() { Cursor.lockState = CursorLockMode.None; SceneManager.LoadScene("MainScene"); }
    public void OptionBtn() { Cursor.lockState = CursorLockMode.None; SceneManager.LoadScene("OptionScene"); }
    public void QuitBtn() { Application.Quit(); }
}
