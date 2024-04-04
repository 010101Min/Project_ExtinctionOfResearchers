using UnityEngine;

public class DeadIconController : MonoBehaviour
{
    private GameObject npc;
    private GameObject DeadImage = null;
    private GameObject DetectedImage = null;

    void Start()
    {
        DeadImage = transform.Find("Viewport/Content/Dead").gameObject;
        DetectedImage = transform.Find("Viewport/Content/Detected").gameObject;
        DeadImage.SetActive(false);
        DetectedImage.SetActive(false);
    }

    void Update()
    {
        if (npc != null) { transform.position = Camera.main.WorldToScreenPoint(new Vector3(npc.transform.position.x, npc.transform.position.y + 1.2f, npc.transform.position.z)); }
    }

    public void setNpc(GameObject npc) { this.npc = npc; }

    public void showDead() { DeadImage.SetActive(true); DetectedImage.SetActive(false); }
    public void showDetected() { DeadImage.SetActive(false); DetectedImage.SetActive(true); }
}
