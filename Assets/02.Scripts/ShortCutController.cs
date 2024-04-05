using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortCutController : MonoBehaviour
{
    public GameObject pos1;
    public GameObject pos2;
    public bool isUsable = true;
    private bool reported = false;

    public Transform UseShortCut(GameObject player)
    {
        bool isChased = player.GetComponent<PlayerController>().getChased();
        float dist1 = Vector3.Distance(pos1.transform.position, player.transform.position);
        float dist2 = Vector3.Distance(pos2.transform.position, player.transform.position);

        GameObject destPos = (dist1 > dist2) ? pos1 : pos2;
        
        if (player.GetComponent<PlayerController>().getChased() && !reported)
        {
            OneGameManager.Instance.DestroyShortCut(this.gameObject);
            reported = true;
        }

        return destPos.transform;
    }

    public GameObject FindShortest(GameObject engineer)
    {
        float dist1 = Vector3.Distance(pos1.transform.position, engineer.transform.position);
        float dist2 = Vector3.Distance(pos2.transform.position, engineer.transform.position);

        GameObject usedPos = (dist1 > dist2) ? pos2 : pos1;
        return usedPos;
    }

    public void fDestroy()
    {
        pos1.gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        pos1.gameObject.tag = "Uninteractable";
        pos2.gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        pos2.gameObject.tag = "Uninteractable";
        isUsable = false;
    }
}
