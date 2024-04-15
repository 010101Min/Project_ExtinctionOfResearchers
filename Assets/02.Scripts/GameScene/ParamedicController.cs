using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ParamedicController : MonoBehaviour
{
    public GameObject ambulance;

    private NavMeshAgent agent;
    private Animator anim;
    private bool isResolving = false;
    private bool isAble = false;

    public List<GameObject> Corpses = new List<GameObject>();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 7f;
        anim = GetComponentInChildren<Animator>();
        anim.SetBool("Finish", true); anim.SetBool("Resolve", false);
        gameObject.SetActive(false);
    }

    public void Report(List<GameObject> Corps)
    {
        foreach (GameObject corp in Corps) { Corpses.Add(corp); }
        if (!isResolving && isAble)
        {
            StopCoroutine(cCome());
            StopCoroutine(cReturn());
            StartCoroutine(cCome());
        }
    }

    public void StartResolve() { isAble = true; StartCoroutine(cCome()); }

    // Come 상태 구현
    GameObject findNearestCorpse(List<GameObject> corpses)
    {
        if (corpses.Count == 0) { return null; }
        GameObject minCorpse = Corpses[0];
        float minDist = float.MaxValue;
        foreach (GameObject corpse in corpses)
        {
            float distance = Vector3.Distance(this.transform.position, corpse.transform.position);
            if (distance <= minDist) { minCorpse = corpse; }
        }
        return minCorpse;
    }
    IEnumerator cCome()
    {
        anim.SetBool("Finish", false); anim.SetBool("Resolve", false);
        GameObject destCorpse = findNearestCorpse(Corpses);
        while (true)
        {
            if (destCorpse == null) { break; }
            agent.SetDestination(destCorpse.transform.position);
            if ((transform.position - destCorpse.transform.position).magnitude <= 1f) { break; }
            yield return null;
        }
        StartCoroutine(cResolve(destCorpse));
    }

    // Resolve 상태 구현
    IEnumerator cResolve(GameObject corpse)
    {
        anim.SetBool("Finish", false); anim.SetBool("Resolve", true);
        isResolving = true;
        float elapsedTime = 0f;
        Corpses.Remove(corpse);

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (corpse.CompareTag("NPC")) { corpse.GetComponent<NPCController>().fResolved(); }
        else if (corpse.CompareTag("Police")) { corpse.GetComponent<PoliceController>().fResolved(); }
        
        if (Corpses.Count > 0) { StartCoroutine(cCome()); }
        else { StartCoroutine(cReturn()); }
        isResolving = false;
    }

    // Return 상태 구현
    IEnumerator cReturn()
    {
        anim.SetBool("Finish", false); anim.SetBool("Resolve", false);
        while (true)
        {
            agent.SetDestination(ambulance.transform.position);
            if (Vector3.Distance(this.gameObject.transform.position, ambulance.transform.position) <= 3f) { break; }
            yield return null;
        }
        isAble = false;
        this.gameObject.transform.position = ambulance.transform.position;
        anim.SetBool("Finish", true); anim.SetBool("Resolve", false);

        ambulance.GetComponent<AmbulanceController>().leaveSign();
        this.gameObject.SetActive(false);
    }
}
