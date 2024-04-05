using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCuffIconController : MonoBehaviour
{
    private GameObject chased;

    void Update()
    {
        if (chased != null) { transform.position = Camera.main.WorldToScreenPoint(new Vector3(chased.transform.position.x, chased.transform.position.y + 2f, chased.transform.position.z)); }
    }

    public void setChased(GameObject chased) { this.chased = chased; }
    public void outChased() { Destroy(this.gameObject); }
}
