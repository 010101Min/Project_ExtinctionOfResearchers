using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ParamedicController : MonoBehaviour
{
    public enum State
    {
        WAIT,
        COME,
        RESOLVE,
        RETURN
    }

    public State state = State.WAIT;
    public GameObject ambulance;

    private NavMeshAgent agent;
    private Animator anim;

    public List<GameObject> Corpses = new List<GameObject>();

    void Start()
    {
        this.gameObject.transform.position = ambulance.transform.position;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 7f;
    }

    public void Report(List<GameObject> Corps)
    {
        foreach (GameObject corp in Corps) { Corpses.Add(corp); }
        StopCoroutine(cCome());
        StopCoroutine(cReturn());
        StartCoroutine(cCome());
    }

    // Come 상태 구현
    GameObject findNearestCorpse(List<GameObject> corpses)
    {
        if (corpses.Count == 0) { return null; }
        GameObject minCorpse = Corpses[0];
        float minDist = float.MaxValue;
        foreach (GameObject corpse in corpses)
        {
            //if (corpses.Count == 0) break;
            //if (corpse == null) { corpses.Remove(corpse); break; }
            //if (corpse.CompareTag("NPC") && corpse.GetComponent<NPCController>().fGetHidden()) { corpses.Remove(corpse); break; }
            //else if (corpse.CompareTag("Police") && corpse.GetComponent<PoliceController>().fGetHidden()) { corpses.Remove(corpse); break; }
            float distance = Vector3.Distance(this.transform.position, corpse.transform.position);
            if (distance <= minDist) { minCorpse = corpse; }
        }
        return minCorpse;
    }
    IEnumerator cCome()
    {
        GameObject destCorpse = findNearestCorpse(Corpses);
        while (true)
        {
            if (destCorpse == null) break;
            agent.SetDestination(destCorpse.transform.position);
            if ((transform.position - destCorpse.transform.position).magnitude <= 1f) { break; }
            yield return null;
        }
        StartCoroutine(cResolve(destCorpse));
    }

    // Resolve 상태 구현
    IEnumerator cResolve(GameObject corpse)
    {
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (corpse.CompareTag("NPC")) { corpse.GetComponent<NPCController>().fResolved(); }
        else if (corpse.CompareTag("Police")) { corpse.GetComponent<PoliceController>().fResolved(); }
        removeItems(corpse);
        if (Corpses.Count > 0) { StartCoroutine(cCome()); }
        else { StartCoroutine(cReturn()); }
    }

    // Return 상태 구현
    IEnumerator cReturn()
    {
        while (true)
        {
            agent.SetDestination(ambulance.transform.position);
            if (Vector3.Distance(this.gameObject.transform.position, ambulance.transform.position) <= 0.5f) { break; }
            yield return null;
        }
        this.gameObject.transform.position = ambulance.transform.position;
    }

    // Wait 상태 구현
    void fWait()
    {
        StopAllCoroutines();
    }

    void removeItems(GameObject item)
    {
        for (int i = Corpses.Count - 1; i >= 0; i--)
        {
            if (Corpses[i] == item) { Corpses.RemoveAt(i); }
        }
    }
}
