using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortCutController : MonoBehaviour
{
    public GameObject pos1;
    public GameObject pos2;
    private bool reported = false;

    public void UseShortCut(bool isChased, ref GameObject player)
    {
        float dist1 = Vector3.Distance(pos1.transform.position, player.transform.position);
        float dist2 = Vector3.Distance(pos2.transform.position, player.transform.position);

        GameObject usedPos = (dist1 > dist2) ? pos2 : pos1;
        player.transform.position = pos2.transform.position;

        if (isChased && !reported)
        {
            // 게임 매니저에게 이거 부수라고 전달
            reported = true;
        }
    }

    public GameObject FindShortest(GameObject engineer)
    {
        float dist1 = Vector3.Distance(pos1.transform.position, engineer.transform.position);
        float dist2 = Vector3.Distance(pos2.transform.position, engineer.transform.position);

        GameObject usedPos = (dist1 > dist2) ? pos2 : pos1;
        return usedPos;
    }

    public void Destroyed()
    {
        gameObject.tag = "Uninteractable";
    }
}
