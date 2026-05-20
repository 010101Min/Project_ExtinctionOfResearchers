using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    public AudioClip mainBGM;
    public AudioClip gameBGM;

    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else if (instance != this) { Destroy(gameObject); }

        audioSource = GetComponent<AudioSource>();

        if (!PlayerPrefs.HasKey("BGM"))
        {
            PlayerPrefs.SetFloat("BGM", 0.5f);
            PlayerPrefs.Save();
        }
        else
        {
            audioSource.volume = PlayerPrefs.GetFloat("BGM");
        }

        PlayMainBGM();
    }

    public void PlayMainBGM() => PlayBGM(mainBGM);
    public void PlayGameBGM() => PlayBGM(gameBGM);

    public void PlayBGM(AudioClip clipName)
    {
        if (audioSource.clip == clipName)
            return;

        audioSource.Stop();
        audioSource.clip = clipName;
        audioSource.Play();
    }

    public void SetVolume(float value) => audioSource.volume = value;
    public void SetDefaultVolume() => audioSource.volume = PlayerPrefs.GetFloat("BGM");
    public float GetVolume() => audioSource.volume;

    public void SetSave()
    {
        PlayerPrefs.SetFloat("BGM", audioSource.volume);
        PlayerPrefs.Save();
    }

    public void SetPitchHigh() => audioSource.pitch = 1.5f;
    public void SetPitchNormal() => audioSource.pitch = 1.0f;
}
