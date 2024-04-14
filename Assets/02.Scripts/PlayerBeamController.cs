using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBeamController : MonoBehaviour
{
    public static PlayerBeamController Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public void DrawLine(Transform eyepos, GameObject victim)
    {
        Debug.Log("���� ����");
        StartCoroutine(cDrawLine(eyepos, victim));
    }

    IEnumerator cDrawLine(Transform Eyepos, GameObject Victim)
    {
        Debug.Log("�ڷ�ƾ ����");
        GameObject lineObject = new GameObject("LineObject");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material.color = Color.yellow;

        float elapsedTime = 0f;

        while (elapsedTime < 0.5f)
        {
            Debug.Log("���� ���� ��");
            lineRenderer.SetPosition(0, Eyepos.position);
            lineRenderer.SetPosition(1, Victim.transform.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(lineObject);
    }
}
