using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    public static LineController Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public void DrawLine(GameObject witness, Transform startPoint, Transform endPoint)
    {
        bool isNPC = (witness.GetComponent<NPCController>() != null);
        if (isNPC) { StartCoroutine(npcDrawLine(witness, startPoint, endPoint)); }
        else { StartCoroutine(policeDrawLine(witness, startPoint, endPoint)); }
    }

    IEnumerator npcDrawLine(GameObject witness, Transform startPoint, Transform endPoint)
    {
        GameObject lineObject = new GameObject("LineObject");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material.color = Color.red;

        float elapsedTime = 0f;

        while (elapsedTime < 0.5f)
        {
            if (!witness.GetComponent<NPCController>().isWitnessable()) { break; }
            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, endPoint.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(lineObject);
    }

    IEnumerator policeDrawLine(GameObject witness, Transform startPoint, Transform endPoint)
    {

        GameObject lineObject = new GameObject("LineObject");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material.color = Color.red;

        float elapsedTime = 0f;

        while (elapsedTime < 0.5f)
        {
            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, endPoint.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(lineObject);
    }
}
