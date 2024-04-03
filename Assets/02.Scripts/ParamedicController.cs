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
    private GameObject ambulance;

    private NavMeshAgent agent;
    private Animator anim;

    public List<GameObject> Corpses = new List<GameObject>();

    //void Start()
    //{
    //    agent = GetComponent<NavMeshAgent>();
    //    agent.speed = 7f;
    //}

    //public void Report(GameObject shortcut)
    //{
    //    ShortCuts.Add(shortcut);
    //    StopCoroutine(cCome());
    //    StopCoroutine(cReturn());
    //    StartCoroutine(cCome());
    //}

    //// Come 상태 구현
    //GameObject findNearestShortcut(List<GameObject> shortcuts)
    //{
    //    GameObject minShortcut = ShortCuts[0];
    //    float minDist = float.MaxValue;
    //    foreach (GameObject shortcut in shortcuts)
    //    {
    //        float distance = Vector3.Distance(this.transform.position, shortcut.GetComponent<ShortCutController>().FindShortest(this.gameObject).transform.position);
    //        if (distance <= minDist) { minShortcut = shortcut; }
    //    }
    //    return minShortcut;
    //}
    //IEnumerator cCome()
    //{
    //    GameObject destShortcut = findNearestShortcut(ShortCuts);
    //    while (true)
    //    {
    //        agent.SetDestination(destShortcut.GetComponent<ShortCutController>().FindShortest(this.gameObject).transform.position);
    //        if ((transform.position - destShortcut.GetComponent<ShortCutController>().FindShortest(this.gameObject).transform.position).magnitude <= 1f) { break; }
    //        yield return null;
    //    }
    //    StartCoroutine(cSabotage(destShortcut));
    //}

    //// Sabotage 상태 구현
    //IEnumerator cSabotage(GameObject shortcut)
    //{
    //    float elapsedTime = 0f;

    //    while (elapsedTime < 5f)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        yield return null;
    //    }
    //    shortcut.GetComponent<ShortCutController>().fDestroy();
    //    removeItems(shortcut);
    //    //if (ShortCuts.Count > 0) { StartCoroutine(cCome()); Debug.Log("남은 지름길 더 있음"); }
    //    //else { StartCoroutine(cReturn()); Debug.Log("남은 지름길 없음"); }
    //}

    //// Return 상태 구현
    //IEnumerator cReturn()
    //{
    //    while (true)
    //    {
    //        agent.SetDestination(home.transform.position);
    //        if (Vector3.Distance(this.gameObject.transform.position, home.transform.position) <= 0.5f) { break; }
    //        yield return null;
    //    }
    //    this.gameObject.transform.position = home.transform.position;
    //}

    //// Wait 상태 구현
    //void fWait()
    //{
    //    StopAllCoroutines();
    //}

    //void removeItems(GameObject item)
    //{
    //    for (int i = ShortCuts.Count - 1; i >= 0; i--)
    //    {
    //        if (ShortCuts[i] == item) { ShortCuts.RemoveAt(i); }
    //    }
    //}
}
