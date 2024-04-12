using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    public void Activate(GameObject corpse)
    {
        StartCoroutine(AbandonCorpse(corpse));
    }

    IEnumerator AbandonCorpse(GameObject corpse)
    {
        Vector3 dir = (this.transform.position - corpse.transform.forward).normalized;
        while (corpse != null)
        {
            corpse.transform.position += dir * 10f * Time.deltaTime;
            yield return null;
        }
    }
}
