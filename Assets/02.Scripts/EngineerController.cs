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

    private float fSabotageTime = 0f;

    public GameObject player;
    private NavMeshAgent agent;
    private Animator anim;
    public Transform[] pos = new Transform[2];

    private List<GameObject> ShortCuts = new List<GameObject>();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 7f;
    }

    // COME 상태 구현
    IEnumerator cCome()
    {
        while (ShortCuts.Count >= 1)
        {
            GameObject minShortcut = ShortCuts[0].GetComponent<ShortCutController>().FindShortest(this.gameObject);
            float minDist = float.MaxValue;
            foreach (GameObject shortcut in ShortCuts)
            {
                float distance = Vector3.Distance(this.transform.position, minShortcut.transform.position);
                if (distance <= minDist) { minShortcut = shortcut; }
            }
            while (true)
            {
                agent.SetDestination(minShortcut.transform.position);
                if ((transform.position - minShortcut.transform.position).magnitude <= 1f)
                {
                    //ShortCuts.Remove();
                    agent.SetDestination(this.transform.position);
                    break;
                }
                yield return null;
            }
        }
    }
}
