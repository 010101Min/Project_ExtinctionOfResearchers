using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowController : MonoBehaviour
{
    public Image binPrefab;
    private Image binIcon;
    private Camera mainCamera;
    private int wallLayer;

    private bool showCross = false;

    float minScale = 0.1f;
    float maxScale = 0.5f;
    float maxDistance = 60f;

    private void Start()
    {
        mainCamera = Camera.main;
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
        binIcon = Instantiate(binPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("UICanvas").transform);
        binIcon.gameObject.SetActive(false);
    }

    public void Activate(GameObject corpse)
    {
        StartCoroutine(AbandonCorpse(corpse));
    }

    IEnumerator AbandonCorpse(GameObject corpse)
    {
        Vector3 dir = (this.transform.position - corpse.transform.forward).normalized;
        if (corpse.CompareTag("NPC")) { corpse.GetComponent<NPCController>().dropCorpse(); }
        else { corpse.GetComponent<PoliceController>().dropCorpse(); }
        while (corpse != null)
        {
            corpse.transform.position += dir * 10f * Time.deltaTime;
            yield return null;
        }
    }

    public void showCrossHair() { if (!showCross) { showCross = true; StartCoroutine(cCrossHair()); } }
    public void hideCrossHair() { showCross = false; }
    IEnumerator cCrossHair()
    {
        binIcon.gameObject.SetActive(true);
        while (true)
        {
            if (!showCross) break;
            binIcon.transform.position = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);

            float distance = Vector3.Distance(this.gameObject.transform.position, mainCamera.transform.position);
            RaycastHit hit;
            // 거리가 100 이상이거나 사이에 벽이 있으면
            if ((distance > 100f) || (Physics.Raycast(mainCamera.transform.position, (this.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer))) { binIcon.gameObject.SetActive(false); }
            else
            {
                binIcon.gameObject.SetActive(true);
                float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
                binIcon.gameObject.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);
            }
            yield return null;
        }
        binIcon.gameObject.SetActive(false);
    }
}
