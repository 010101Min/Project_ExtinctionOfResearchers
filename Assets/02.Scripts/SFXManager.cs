using UnityEngine;
using UnityEngine.SceneManagement;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioClip Laser;
    public AudioClip Provoke;

    public AudioClip Throw;
    public AudioClip Bomb;
    public AudioClip Fireextinguisher;
    public AudioClip Poison;
    public AudioClip Shortcut;

    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else if (instance != this) { Destroy(gameObject); }

        audioSource = GetComponent<AudioSource>();

        if (!PlayerPrefs.HasKey("SFX"))
        {
            PlayerPrefs.SetFloat("SFX", 0.5f);
            PlayerPrefs.Save();
        }
        else
        {
            audioSource.volume = PlayerPrefs.GetFloat("SFX");
        }
    }

    private void PlayAudio(AudioClip clipName)
    {
        audioSource.PlayOneShot(clipName);
    }

    public void PlayLaser() => PlayAudio(Laser);
    public void PlayProvoke() => PlayAudio(Provoke);
    public void PlayThrow() => PlayAudio(Throw);
    public void PlayBomb() => PlayAudio(Bomb);
    public void PlayFireextinguisher() => PlayAudio(Fireextinguisher);
    public void PlayPoison() => PlayAudio(Poison);
    public void PlayShortcut() => PlayAudio(Shortcut);


    public void SetVolume(float value) => audioSource.volume = value;
    public void SetDefaultVolume() => audioSource.volume = PlayerPrefs.GetFloat("SFX");
    public float GetVolume() => audioSource.volume;

    public void SetSave()
    {
        PlayerPrefs.SetFloat("SFX", audioSource.volume);
        PlayerPrefs.Save();
    }
}
