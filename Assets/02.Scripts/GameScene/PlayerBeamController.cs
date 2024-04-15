using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBeamController : MonoBehaviour
{
    public static PlayerBeamController Instance;
    public Material LightMaterial;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public void DrawLine(Transform eyepos, GameObject victim)
    {
        StartCoroutine(cDrawLine(eyepos, victim));
    }

    IEnumerator cDrawLine(Transform Eyepos, GameObject Victim)
    {
        GameObject lineObject = new GameObject("LineObject");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = LightMaterial;

        float elapsedTime = 0f;

        while (elapsedTime < 0.5f)
        {
            lineRenderer.SetPosition(0, Eyepos.position);
            lineRenderer.SetPosition(1, Victim.transform.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(lineObject);
    }
}
