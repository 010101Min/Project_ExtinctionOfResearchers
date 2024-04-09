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

    float minScale = 0.1f; // 최소 스케일 값
    float maxScale = 0.75f; // 최대 스케일 값
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

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(chased.gameObject.transform.position);
            bool isInView = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

            // 카메라와 UI 사이에 벽이 있는지 확인
            RaycastHit hit;
            if ((!Physics.Raycast(mainCamera.transform.position, (chased.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer)) && isInView) { handcuff.SetActive(true); }
            else { handcuff.SetActive(false); }
        }
    }

    public void setChased(GameObject chased) { this.chased = chased; }
    public void outChased() { Destroy(this.gameObject); }
}
