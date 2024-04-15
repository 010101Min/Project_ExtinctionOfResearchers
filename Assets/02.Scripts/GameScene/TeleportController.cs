using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportController : MonoBehaviour
{
    private void Start()
    {
        this.gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
    }

    public void SetUsable()
    {
        this.gameObject.layer = LayerMask.NameToLayer("TELEPORT");
    }
}
