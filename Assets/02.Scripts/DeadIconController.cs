using UnityEngine;

public class DeadIconController : MonoBehaviour
{
    private GameObject npc;
    private GameObject DeadImage = null;
    private GameObject DetectedImage = null;

    float minScale = 0.5f; // 최소 스케일 값
    float maxScale = 1.0f; // 최대 스케일 값
    float maxDistance = 20f;

    private Camera mainCamera;
    private int wallLayer;

    void Start()
    {
        mainCamera = Camera.main;
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
        DeadImage = transform.Find("Viewport/Content/Dead").gameObject;
        DetectedImage = transform.Find("Viewport/Content/Detected").gameObject;
        DeadImage.SetActive(false);
        DetectedImage.SetActive(false);
    }

    void Update()
    {
        if (npc != null)
        {
            // 기본적으로 오브젝트 위에 떠있음
            transform.position = Camera.main.WorldToScreenPoint(new Vector3(npc.transform.position.x, npc.transform.position.y + 1f, npc.transform.position.z));

            // 카메라와의 거리에 따라 스케일 조정
            float distance = Vector3.Distance(npc.gameObject.transform.position, mainCamera.transform.position);
            float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
            this.gameObject.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

            // 카메라와 UI 사이에 벽이 있는지 확인
            RaycastHit hit;
            if (Physics.Raycast(mainCamera.transform.position, (npc.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer))
            {
                // 벽에 가려졌을 경우 UI 요소 숨김
                Debug.Log("벽에 가려짐 판정");
                this.gameObject.SetActive(false);
            }
            else
            {
                // 벽에 가려지지 않았을 경우 UI 요소 보임
                Debug.Log("벽에 안 가려짐 판정");
                this.gameObject.SetActive(true);
            }
        }
    }

    public void setNpc(GameObject npc) { this.npc = npc; }

    public void showDead() { DeadImage.SetActive(true); DetectedImage.SetActive(false); }
    public void showDetected() { DeadImage.SetActive(false); DetectedImage.SetActive(true); }
}
