using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EngineerController : MonoBehaviour
{
    public enum State
    {
        WAIT,
        COME,
        SABOTAGE,
        RETURN
    }

    public State state = State.WAIT;
    public GameObject home;

    private float fSabotageTime = 15f;

    private NavMeshAgent agent;
    private Animator anim;

    public List<GameObject> ShortCuts = new List<GameObject>();

    void Start()
    {
        this.gameObject.transform.position = home.transform.position;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 7f;
        anim = GetComponentInChildren<Animator>();
        anim.SetBool("Finish", true); anim.SetBool("Destroy", false);
    }

    public void Report(GameObject shortcut)
    {
        ShortCuts.Add(shortcut);
        StopCoroutine(cCome());
        StopCoroutine(cReturn());
        StartCoroutine(cCome());
    }

    // Come 상태 구현
    GameObject findNearestShortcut(List<GameObject> shortcuts)
    {
        GameObject minShortcut = ShortCuts[0];
        float minDist = float.MaxValue;
        foreach (GameObject shortcut in shortcuts)
        {
            float distance = Vector3.Distance(this.transform.position, shortcut.GetComponent<ShortCutController>().FindShortest(this.gameObject).transform.position);
            if (distance <= minDist) { minShortcut = shortcut; }
        }
        return minShortcut;
    }
    IEnumerator cCome()
    {
        anim.SetBool("Finish", false); anim.SetBool("Destroy", false);
        GameObject destShortcut = findNearestShortcut(ShortCuts);
        while (true)
        {
            agent.SetDestination(destShortcut.GetComponent<ShortCutController>().FindShortest(this.gameObject).transform.position);
            if ((transform.position - destShortcut.GetComponent<ShortCutController>().FindShortest(this.gameObject).transform.position).magnitude <= 3f) { break; }
            yield return null;
        }
        StartCoroutine(cSabotage(destShortcut));
    }

    // Sabotage 상태 구현
    IEnumerator cSabotage(GameObject shortcut)
    {
        anim.SetBool("Finish", false); anim.SetBool("Destroy", true);
        float elapsedTime = 0f;

        while (elapsedTime < fSabotageTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        shortcut.GetComponent<ShortCutController>().fDestroy();
        removeItems(shortcut);
        if (ShortCuts.Count > 0) { StartCoroutine(cCome()); }
        else { StartCoroutine(cReturn()); }
    }
    
    // Return 상태 구현
    IEnumerator cReturn()
    {
        anim.SetBool("Finish", false); anim.SetBool("Destroy", false);
        while (true)
        {
            agent.SetDestination(home.transform.position);
            if (Vector3.Distance(this.gameObject.transform.position, home.transform.position) <= 0.5f) { break; }
            yield return null;
        }
        this.gameObject.transform.position = home.transform.position;
        anim.SetBool("Finish", true); anim.SetBool("Destroy", false);
    }

    void removeItems(GameObject item)
    {
        for (int i = ShortCuts.Count - 1; i >= 0; i--)
        {
            if (ShortCuts[i] == item) { ShortCuts.RemoveAt(i); }
        }
    }
}
