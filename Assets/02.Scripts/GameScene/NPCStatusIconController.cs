using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class NPCStatusIconController : MonoBehaviour
{
    public GameObject AliveStatus;
    public GameObject DeadStatus;

    private GameObject npc;

    private GameObject DeadImage = null;
    private GameObject DetectedImage = null;
    private GameObject PoisonedImage = null;
    private GameObject BlindedImage = null;
    private GameObject SleepingImage = null;

    float minScale = 0.1f; // �ּ� ������ ��
    float maxScale = 0.75f; // �ִ� ������ ��
    float maxDistance = 60f;

    private Camera mainCamera;
    private int wallLayer;

    void Start()
    {
        mainCamera = Camera.main;
        wallLayer = 1 << LayerMask.NameToLayer("WALL");

        PoisonedImage = AliveStatus.transform.Find("Viewport/Content/Poisoned").gameObject;
        BlindedImage = AliveStatus.transform.Find("Viewport/Content/Blinded").gameObject;
        SleepingImage = AliveStatus.transform.Find("Viewport/Content/Sleeping").gameObject;
        DeadImage = DeadStatus.transform.Find("Viewport/Content/Dead").gameObject;
        DetectedImage = DeadStatus.transform.Find("Viewport/Content/Detected").gameObject;

        PoisonedImage.SetActive(false);
        BlindedImage.SetActive(false);
        SleepingImage.SetActive(false);
        DeadImage.SetActive(false);
        DetectedImage.SetActive(false);

        AliveStatus.SetActive(false);
        DeadStatus.SetActive(false);
    }

    void Update()
    {
        if (npc != null)
        {
            // �⺻������ ������Ʈ ���� ������
            transform.position = Camera.main.WorldToScreenPoint(npc.transform.position + Vector3.up * 1.2f);

            float distance = Vector3.Distance(npc.gameObject.transform.position, mainCamera.transform.position);
            float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
            this.gameObject.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

            if (distance > 100f) { AliveStatus.SetActive(false); DeadStatus.SetActive(false); return; }

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(npc.transform.position);
            bool isInView = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

            // ī�޶�� UI ���̿� ���� �ִ��� Ȯ��
            RaycastHit hit;
            if ((!Physics.Raycast(mainCamera.transform.position, (npc.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer)) && isInView)
            {
                // ���� �������� �ʾ��� ��� UI ��� ����
                AliveStatus.SetActive(true);
                DeadStatus.SetActive(true);
            }
            else
            {
                // ���� �������� ��� UI ��� ����
                AliveStatus.SetActive(false);
                DeadStatus.SetActive(false);
            }
        }
        else { Destroy(gameObject); }
    }

    public void setNpc(GameObject npc) { this.npc = npc; }

    public void showDead()
    {
        PoisonedImage.SetActive(false);
        BlindedImage.SetActive(false);
        SleepingImage.SetActive(false);
        DeadImage.SetActive(true);
        DetectedImage.SetActive(false);
    }
    public void showDetected()
    {
        PoisonedImage.SetActive(false);
        BlindedImage.SetActive(false);
        SleepingImage.SetActive(false);
        DeadImage.SetActive(false);
        DetectedImage.SetActive(true);
    }

    public void showPoisoned() { PoisonedImage.SetActive(true); }
    public void showPoisonedPercent(float percent) { PoisonedImage.GetComponent<Image>().fillAmount = percent; }
    public void showBlinded() { BlindedImage.SetActive(true); }
    public void showOutBlinded() { BlindedImage.SetActive(false); }
    public void showSleeping() { SleepingImage.SetActive(true); }
    public void showOutSleeping() { SleepingImage.SetActive(false); }
}
