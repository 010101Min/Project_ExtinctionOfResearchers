using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class HandCuffIconController : MonoBehaviour
{
    private GameObject chased;
    private Camera mainCamera;
    private int wallLayer;
    private GameObject handcuff = null;

    float minScale = 0.1f; // �ּ� ������ ��
    float maxScale = 0.75f; // �ִ� ������ ��
    float maxDistance = 60f;

    private void Start()
    {
        mainCamera = Camera.main;
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
        handcuff = transform.Find("HandCuff").gameObject;
    }

    void Update()
    {
        if (chased != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(new Vector3(chased.transform.position.x, chased.transform.position.y + 2f, chased.transform.position.z));

            float distance = Vector3.Distance(chased.gameObject.transform.position, mainCamera.transform.position);
            float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
            this.gameObject.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

            if (distance > 100f) { handcuff.SetActive(false); return; }

            // ī�޶�� UI ���̿� ���� �ִ��� Ȯ��
            RaycastHit hit;
            if (Physics.Raycast(mainCamera.transform.position, (chased.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer)) { handcuff.SetActive(false); }
            else { handcuff.SetActive(true); }
        }
    }

    public void setChased(GameObject chased) { this.chased = chased; }
    public void outChased() { Destroy(this.gameObject); }
}
