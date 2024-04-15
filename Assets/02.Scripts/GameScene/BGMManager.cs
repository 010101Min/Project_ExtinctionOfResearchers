using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else if (instance != this) { Destroy(gameObject); }
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }

    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    // 씬이 로드될 때 호출되는 메서드
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        if (scene.name == "GameScene") // 'd' 씬으로 전환할 때
        {
            GetComponent<AudioSource>().Stop();
        }
        else
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
