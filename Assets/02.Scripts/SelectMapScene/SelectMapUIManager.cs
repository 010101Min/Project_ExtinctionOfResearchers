using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectMapUIManager : MonoBehaviour
{
    public void GameStartBtn() { SceneManager.LoadScene("GameScene"); BGMManager.instance.PlayGameBGM(); }
    public void MainMenuBtn() { Cursor.lockState = CursorLockMode.None; SceneManager.LoadScene("MainScene"); BGMManager.instance.PlayMainBGM(); }
}
