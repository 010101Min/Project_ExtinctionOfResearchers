using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class NPCHandcuffController : MonoBehaviour
{
    public static NPCHandcuffController Instance;

    public GameObject Suspect = null;
    public GameObject HandcuffPrefab;
    private GameObject Handcuff;
    private bool temp = false;

    float minScale = 0.1f;
    float maxScale = 0.75f;
    float maxDistance = 60f;
    private Camera mainCamera;
    private int wallLayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {
        Handcuff = Instantiate(HandcuffPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("UICanvas").transform);
        Handcuff.SetActive(false);
        mainCamera = Camera.main;
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
    }

    public void DrawHandcuff(GameObject suspect)
    {
        Suspect = suspect;
        StartCoroutine(cHandcuff());
    }

    IEnumerator cHandcuff()
    {
        offHandcuff();
        temp = true;

        Handcuff.SetActive(true);
        while (temp)
        {
            if (Suspect == null) break;
            Handcuff.transform.position = Camera.main.WorldToScreenPoint(Suspect.transform.position + Vector3.up * 2f);

            float distance = Vector3.Distance(Suspect.gameObject.transform.position, mainCamera.transform.position);
            float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
            Handcuff.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

            if (distance > 100f) { Handcuff.SetActive(false); }
            else
            {
                Vector3 viewportPos = mainCamera.WorldToViewportPoint(Suspect.transform.position);
                bool isInView = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

                RaycastHit hit;
                if ((!Physics.Raycast(mainCamera.transform.position, (Suspect.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer)) && isInView) { Handcuff.SetActive(true); }
                else { Handcuff.SetActive(false); }
            }
            yield return null;
        }
        Handcuff.SetActive(false);
    }

    public void offHandcuff() { temp = false; Handcuff.SetActive(false); }
}
