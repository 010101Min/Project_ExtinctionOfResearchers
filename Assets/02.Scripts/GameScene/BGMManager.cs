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

    // ���� �ε�� �� ȣ��Ǵ� �޼���
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        if (scene.name == "GameScene") // 'd' ������ ��ȯ�� ��
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
