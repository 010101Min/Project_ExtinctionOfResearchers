using UnityEngine;

public class DeadIconController : MonoBehaviour
{
    private GameObject npc;
    private GameObject DeadImage = null;
    private GameObject DetectedImage = null;

    float minScale = 0.5f; // �ּ� ������ ��
    float maxScale = 1.0f; // �ִ� ������ ��
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
            // �⺻������ ������Ʈ ���� ������
            transform.position = Camera.main.WorldToScreenPoint(new Vector3(npc.transform.position.x, npc.transform.position.y + 1f, npc.transform.position.z));

            // ī�޶���� �Ÿ��� ���� ������ ����
            float distance = Vector3.Distance(npc.gameObject.transform.position, mainCamera.transform.position);
            float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
            this.gameObject.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

            // ī�޶�� UI ���̿� ���� �ִ��� Ȯ��
            RaycastHit hit;
            if (Physics.Raycast(mainCamera.transform.position, (npc.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer))
            {
                // ���� �������� ��� UI ��� ����
                Debug.Log("���� ������ ����");
                this.gameObject.SetActive(false);
            }
            else
            {
                // ���� �������� �ʾ��� ��� UI ��� ����
                Debug.Log("���� �� ������ ����");
                this.gameObject.SetActive(true);
            }
        }
    }

    public void setNpc(GameObject npc) { this.npc = npc; }

    public void showDead() { DeadImage.SetActive(true); DetectedImage.SetActive(false); }
    public void showDetected() { DeadImage.SetActive(false); DetectedImage.SetActive(true); }
}
