using UnityEngine;
using UnityEngine.UI;

public class AliveIconController : MonoBehaviour
{
    private GameObject npc;
    private GameObject PoisonedImage = null;
    private GameObject BlindedImage = null;
    private GameObject SleepingImage = null;

    void Start()
    {
        PoisonedImage = transform.Find("Viewport/Content/Poisoned").gameObject;
        BlindedImage = transform.Find("Viewport/Content/Blinded").gameObject;
        SleepingImage = transform.Find("Viewport/Content/Sleeping").gameObject;
        PoisonedImage.SetActive(false);
        BlindedImage.SetActive(false);
        SleepingImage.SetActive(false);
    }

    void Update()
    {
        if (npc != null) { transform.position = Camera.main.WorldToScreenPoint(new Vector3(npc.transform.position.x, npc.transform.position.y + 1.2f, npc.transform.position.z)); }
    }

    public void setNpc(GameObject npc) { this.npc = npc; }
    public void fInvisible()
    {
        PoisonedImage.SetActive(false);
        BlindedImage.SetActive(false);
        SleepingImage.SetActive(false);
    }

    public void showPoisoned() { PoisonedImage.SetActive(true); }
    public void showPoisonedPercent(float percent) { PoisonedImage.GetComponent<Image>().fillAmount = percent; }
    public void showBlinded() { BlindedImage.SetActive(true); }
    public void showOutBlinded() { BlindedImage.SetActive(false); }
    public void showSleeping() { SleepingImage.SetActive(true); }
    public void showOutSleeping() { SleepingImage.SetActive(false); }
}
