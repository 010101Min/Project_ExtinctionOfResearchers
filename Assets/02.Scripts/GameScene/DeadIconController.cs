using UnityEngine;

public class DeadIconController : MonoBehaviour
{
    public GameObject DeadStatus;
    private GameObject npc;
    private GameObject DeadImage = null;
    private GameObject DetectedImage = null;

    float minScale = 0.1f; // 최소 스케일 값
    float maxScale = 0.75f; // 최대 스케일 값
    float maxDistance = 60f;

    private Camera mainCamera;
    private int wallLayer;

    void Start()
    {
        mainCamera = Camera.main;
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
        DeadImage = transform.Find("Icon_Dead/Viewport/Content/Dead").gameObject;
        DetectedImage = transform.Find("Icon_Dead/Viewport/Content/Detected").gameObject;
        DeadImage.SetActive(false);
        DetectedImage.SetActive(false);
    }

    void Update()
    {
        if (npc != null)
        {
            // 기본적으로 오브젝트 위에 떠있음
            transform.position = Camera.main.WorldToScreenPoint(new Vector3(npc.transform.position.x, npc.transform.position.y + 1f, npc.transform.position.z));

            float distance = Vector3.Distance(npc.gameObject.transform.position, mainCamera.transform.position);
            float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
            this.gameObject.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

            if (distance > 100f) { DeadStatus.SetActive(false); return; }

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(npc.transform.position);
            bool isInView = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

            // 카메라와 UI 사이에 벽이 있는지 확인
            RaycastHit hit;
            if ((!Physics.Raycast(mainCamera.transform.position, (npc.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer)) && isInView) { DeadStatus.SetActive(true); }
            else { DeadStatus.SetActive(false); }
        }
    }

    public void setNpc(GameObject npc) { this.npc = npc; }

    public void showDead() { DeadImage.SetActive(true); DetectedImage.SetActive(false); }
    public void showDetected() { DeadImage.SetActive(false); DetectedImage.SetActive(true); }
}
