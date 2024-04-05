using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCStatusIconController : MonoBehaviour
{
    public GameObject AliveStatusPrefab;
    public GameObject DeadStatusPrefab;
    private GameObject aliveStatus;
    private GameObject deadStatus;

    private GameObject npc;

    private GameObject DeadImage = null;
    private GameObject DetectedImage = null;
    private GameObject PoisonedImage = null;
    private GameObject BlindedImage = null;
    private GameObject SleepingImage = null;

    float minScale = 0.5f; // �ּ� ������ ��
    float maxScale = 1.0f; // �ִ� ������ ��
    float maxDistance = 20f;

    private Camera mainCamera;
    private int wallLayer;

    void Start()
    {
        mainCamera = Camera.main;
        wallLayer = 1 << LayerMask.NameToLayer("WALL");

        aliveStatus = Instantiate(AliveStatusPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("UICanvas").transform);
        deadStatus = Instantiate(DeadStatusPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("UICanvas").transform);

        PoisonedImage = aliveStatus.transform.Find("Viewport/Content/Poisoned").gameObject;
        BlindedImage = aliveStatus.transform.Find("Viewport/Content/Blinded").gameObject;
        SleepingImage = aliveStatus.transform.Find("Viewport/Content/Sleeping").gameObject;
        DeadImage = deadStatus.transform.Find("Viewport/Content/Dead").gameObject;
        DetectedImage = deadStatus.transform.Find("Viewport/Content/Detected").gameObject;

        PoisonedImage.SetActive(false);
        BlindedImage.SetActive(false);
        SleepingImage.SetActive(false);
        DeadImage.SetActive(false);
        DetectedImage.SetActive(false);

        aliveStatus.SetActive(false);
        deadStatus.SetActive(false);
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
                aliveStatus.SetActive(false);
                deadStatus.SetActive(false);
            }
            else
            {
                // ���� �������� �ʾ��� ��� UI ��� ����
                Debug.Log("���� �� ������ ����");
                aliveStatus.SetActive(true);
                deadStatus.SetActive(true);
            }
        }
    }

    public void setNpc(GameObject npc) { this.npc = npc; }

    public void showDead()
    {
        aliveStatus.SetActive(false);
        deadStatus.SetActive(true);
        DeadImage.SetActive(true);
        DetectedImage.SetActive(false);
    }
    public void showDetected()
    {
        showDead();
        DeadImage.SetActive(false);
        DetectedImage.SetActive(true);
    }

    public void showPoisoned()
    {
        if (!aliveStatus.activeSelf) { aliveStatus.SetActive(true); }
        PoisonedImage.SetActive(true);
    }
    public void showPoisonedPercent(float percent) { PoisonedImage.GetComponent<Image>().fillAmount = percent; }
    public void showBlinded()
    {
        if (!aliveStatus.activeSelf) { aliveStatus.SetActive(true); }
        BlindedImage.SetActive(true);
    }
    public void showOutBlinded() { BlindedImage.SetActive(false); }
    public void showSleeping()
    {
        if (!aliveStatus.activeSelf) { aliveStatus.SetActive(true); }
        SleepingImage.SetActive(true);
    }
    public void showOutSleeping() { SleepingImage.SetActive(false); }
}
