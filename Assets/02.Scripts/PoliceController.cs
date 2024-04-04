using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PoliceController : MonoBehaviour
{
    public enum State
    {
        WAIT,
        RESOLVE,
        FIND,
        CHASE,
        RETURN,
        DIE,
        RECOGNIZED,
        HIDE
    }

    private State oldState;
    public State state;
    NavMeshAgent agent;
    private GameObject Suspect;
    public List<GameObject> Corpse = new List<GameObject>();
    private GameObject policeCar;
    private GameObject player;
    public ScrollRect deadStatePrefab;
    private ScrollRect deadStateImage = null;

    private int chaseTime = 0;
    private bool isDead = false;
    public bool isDetected = false;
    private bool isCarriable = false;
    private bool isHidden = false;

    int npcLayer;
    int corpseLayer;
    int wallLayer;

    bool chaseCoroutine = false;
    bool resolveCoroutine = false;
    bool returnCoroutine = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 6f;
        policeCar = GameObject.FindGameObjectWithTag("PoliceCar");
        player = GameObject.FindGameObjectWithTag("Player");

        npcLayer = 1 << LayerMask.NameToLayer("NPC");
        corpseLayer = 1 << LayerMask.NameToLayer("CORPSE");
        wallLayer = 1 << LayerMask.NameToLayer("WALL");

        StartCoroutine(cSetDeadIcons());
        deadStateImage.enabled = false;
    }

    void Update()
    {
        if (!isDead) CheckVision();

        switch (state)
        {
            case State.RETURN:
                if (!returnCoroutine) StartCoroutine(cReturn());
                break;
            case State.RESOLVE:
                if (!resolveCoroutine) StartCoroutine(cResolve());
                break;
            case State.CHASE:
                StopCoroutine(cReturn());
                StopCoroutine(cResolve());
                if (!chaseCoroutine) StartCoroutine(cChase());
                break;
            default: break;
        }
    }

    // ���� �ٲٴ� �Լ�
    void ChangeState(State newState)
    {
        initCoroutine();
        oldState = state;
        state = newState;

        switch (state)
        {
            case State.RESOLVE:
                if (!resolveCoroutine && state != State.RESOLVE)
                {
                    StopCoroutine(cChase());
                    StopCoroutine(cReturn());
                    StartCoroutine(cResolve());
                }
                break;
            case State.CHASE:
                if (!chaseCoroutine && state != State.CHASE)
                {
                    StopCoroutine(cResolve());
                    StopCoroutine(cReturn());
                    StartCoroutine(cChase());
                }
                break;
            case State.RETURN:
                if (!returnCoroutine && state != State.RETURN)
                {
                    StopCoroutine(cResolve());
                    StopCoroutine(cChase());
                    StartCoroutine(cReturn());
                }
                break;
            default:
                break;
        }
    }

    void initCoroutine()
    {
        StopCoroutine(cChase());
        StopCoroutine(cResolve());
        StopCoroutine(cReturn());
        chaseCoroutine = false;
        resolveCoroutine = false;
        returnCoroutine = false;
    }

    // UI ��� �����ܿ� �Լ�
    IEnumerator cSetDeadIcons()
    {
        deadStateImage = Instantiate(deadStatePrefab, Vector3.zero, Quaternion.identity, GameObject.Find("UICanvas").transform);
        while (true)
        {
            if (deadStateImage != null) { deadStateImage.GetComponent<DeadIconController>().setNpc(this.gameObject); break; }
            yield return null;
        }
        yield break;
    }
    // ����� �ҷ��� �Լ�
    public void fDead()
    {
        initCoroutine();
        player.GetComponent<PlayerController>().outChased();
        isDead = true;
        agent.enabled = false;
        isCarriable = true;
        if (!deadStateImage.gameObject.activeSelf) { deadStateImage.enabled = true; }
        deadStateImage.GetComponent<DeadIconController>().showDead();
        gameObject.layer = LayerMask.NameToLayer("CORPSE");
        state = State.DIE;
    }
    public bool getDead() { return isDead; }
    // �ý� �߽߰� �ҷ��� �Լ�
    public void fDetected()
    {
        if (!deadStateImage.gameObject.activeSelf) { deadStateImage.enabled = true; }
        deadStateImage.GetComponent<DeadIconController>().showDetected();
        gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        isCarriable = false;
        isDetected = true;
    }
    public bool fGetDetected() { return isDetected; }
    // �ý� ���н� �ҷ��� �Լ�
    public void fHide()
    {
        agent.enabled = false;
        isHidden = true;
        gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        initCoroutine();
        if (!isDead)
        {
            // ���ӸŴ������� ų �� �ø��� �ʿ�
        }
        StartCoroutine(cHide());
    }
    IEnumerator cHide()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 60f)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(deadStateImage.gameObject);
        Destroy(gameObject);
    }
    public bool fGetCarriable() { return isCarriable; }
    public bool fGetHidden() { return isHidden; }
    // �ý� ������ �ҷ��� �Լ�
    public void fResolved()
    {
        initCoroutine();
        this.gameObject.tag = "Uninteractable";
        this.enabled = false;
    }

    // �Ű� �Լ�
    public void Report(GameObject reporter, GameObject corpse, GameObject suspect, int time)
    {
        if (corpse != null)
        {
            if (corpse.CompareTag("NPC")) corpse.GetComponent<NPCController>().fDetected();
            if (corpse.CompareTag("Police")) corpse.GetComponent<PoliceController>().fDetected();
            Corpse.Add(corpse);
        }
        chaseTime = time;
        if (suspect == reporter)
        {
            Debug.Log("�Ű� ���� �Ű��� : " + reporter.name + ", ������ ����, �ý� : " + corpse);
            if (!((state == State.RETURN) && (oldState == State.CHASE))) ChangeState(State.RESOLVE);
        }
        else
        {
            if (corpse != null) { Debug.Log("�Ű� ���� �Ű��� : " + reporter.name + ", ������ : " + suspect.name + ", �ý� : " + corpse.name); }
            else { Debug.Log("�Ű� ���� �Ű��� : " + reporter.name + ", ������ : " + suspect.name + ", �ý� : �����"); }
            StopCoroutine(cChase());
            Suspect = suspect;
            ChangeState(State.CHASE);
        }
    }

    // �þ� üũ �Լ�
    void CheckVision()
    {
        Collider[] corpses = Physics.OverlapSphere(transform.position, 10f, corpseLayer);

        for (int i = 0; i < corpses.Length; i++)
        {
            GameObject tempCorpse = corpses[i].gameObject;
            Vector3 dirToCorpse = (tempCorpse.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToCorpse) <= 90f)
            {
                float distToCorpse = Vector3.Distance(transform.position, tempCorpse.transform.position);
                if (!Physics.Raycast(transform.position, dirToCorpse, distToCorpse, wallLayer))
                {
                    if (tempCorpse.CompareTag("NPC")) tempCorpse.GetComponent<NPCController>().fDetected();
                    if (tempCorpse.CompareTag("Police")) tempCorpse.GetComponent<PoliceController>().fDetected();
                    Debug.Log("�ý� ��� " + tempCorpse.name);
                    Corpse.Add(tempCorpse);
                    // Chase ���̾��ٸ� Ÿ�� �ٲ��� �ʰ� ��� �߰�, Chase ���� �ƴϾ��ٸ� ������ ����
                    if (state != State.CHASE) { StartCoroutine(FindSuspect(tempCorpse)); }
                }
            }
        }
    }
    // ������ ã�� �Լ�
    IEnumerator FindSuspect(GameObject corpse)
    {
        Debug.Log("������ ã�� ����");
        GameObject tempSuspect = this.gameObject;
        Collider[] suspects = Physics.OverlapSphere(corpse.transform.position, 8f, npcLayer);

        float distToPlayer = Vector3.Distance(corpse.transform.position, player.transform.position);
        bool isPlayerInSight = !(Physics.Raycast(transform.position, (player.transform.position - this.transform.position).normalized, Vector3.Distance(this.transform.position, player.transform.position), wallLayer));
        float minDist = 8f;

        if (suspects.Length <= 0) { if ((distToPlayer <= 9f) && (isPlayerInSight)) { tempSuspect = player; } }
        else
        {
            for (int i = 0; i < suspects.Length; i++)
            {
                Vector3 dirToSuspect = (suspects[i].gameObject.transform.position - corpse.transform.position).normalized;
                
                float distToSuspect = Vector3.Distance(corpse.transform.position, suspects[i].gameObject.transform.position);
                if (!Physics.Raycast(corpse.transform.position, dirToSuspect, distToSuspect, wallLayer) && (distToSuspect <= minDist))
                {
                    minDist = distToSuspect;
                    tempSuspect = suspects[i].gameObject;
                }
            }
            if (distToPlayer - 2 <= minDist) tempSuspect = player;
            Debug.Log("������ : " + tempSuspect.name);
        }

        LineController.Instance.DrawLine(this.gameObject, this.transform, corpse.transform);
        if (tempSuspect != this.gameObject)
        {
            LineController.Instance.DrawLine(this.gameObject, corpse.transform, tempSuspect.transform);
            Report(this.gameObject, corpse, tempSuspect, chaseTime);
        }
        else { Debug.Log("������ �� ã��"); }
        yield return null;
    }

    // Chase ���� ����
    IEnumerator cChase()
    {
        StopCoroutine(cResolve());
        StopCoroutine(cReturn());
        if (chaseCoroutine) yield break;
        chaseCoroutine = true;
        resolveCoroutine = false;
        returnCoroutine = false;
        yield return null;
        agent.speed = 10f;
        float elapsedTime = 0f;

        Debug.Log("�߰� ���� / �߰� ��� : " + Suspect.name);
        if (Suspect.Equals(player)) { player.GetComponent<PlayerController>().inChased(); }
        else
        {
            player.GetComponent<PlayerController>().outChased();
            Suspect.GetComponent<NPCController>().sadfasdsaf
        }

        while ((elapsedTime < chaseTime) && (Suspect != null))
        {
            if (Suspect.CompareTag("NPC")) { if (Suspect.GetComponent<NPCController>().fGetDead()) break; }
            agent.SetDestination(Suspect.transform.position);
            if ((transform.position - Suspect.transform.position).magnitude <= 2.5f)
            {
                if (Suspect.CompareTag("NPC")) { Suspect.GetComponent<NPCController>().fArrested(); }
                else
                {
                    // �÷��̾ ������ �� �Լ� ���� �ʿ�
                }
                break;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        player.GetComponent<PlayerController>().outChased();
        Suspect = null;
        Debug.Log("�߰� ����");
        agent.speed = 6f;
        ChangeState(State.RETURN);
    }
    // Resolve ���� ����
    IEnumerator cResolve()
    {
        if (resolveCoroutine) yield break;
        chaseCoroutine = false;
        resolveCoroutine = true;
        returnCoroutine = false;
        agent.speed = 6f;
        while (Corpse.Count >= 1)
        {
            GameObject minCorpse = Corpse[0];
            float minDist = float.MaxValue;
            foreach (GameObject corpse in Corpse)
            {
                float distance = Vector3.Distance(this.transform.position, corpse.transform.position);
                if (distance <= minDist) { minCorpse = corpse; }
            }
            while (true)
            {
                agent.SetDestination(minCorpse.transform.position);
                if ((transform.position - minCorpse.transform.position).magnitude <= 2f)
                {
                    if (minCorpse.CompareTag("NPC")) minCorpse.GetComponent<NPCController>().fResolved();
                    if (minCorpse.CompareTag("Police")) minCorpse.GetComponent<PoliceController>().fResolved();
                    Corpse.Remove(minCorpse);
                    agent.SetDestination(this.transform.position);
                    break;
                }
                if ((state == State.CHASE) || (state == State.WAIT) || (state == State.RETURN)) { yield break; }
                yield return null;
            }
        }
        if (Suspect == player) { player.GetComponent<PlayerController>().outChased(); }
        ChangeState(State.RETURN);
    }
    // Return ���� ����
    IEnumerator cReturn()
    {
        if (returnCoroutine) yield break;
        chaseCoroutine = false;
        resolveCoroutine = false;
        returnCoroutine = true;
        agent.speed = 5f;
        while ((transform.position - policeCar.transform.position).magnitude >= 1f)
        {
            agent.SetDestination(policeCar.transform.position);
            if (((state == State.WAIT) || (state == State.RESOLVE)) && (oldState != State.CHASE)) { yield break; }
            if (state == State.CHASE) { yield break; }
            yield return null;
        }
        transform.position = policeCar.transform.position;
        GameManager.Instance.plusCorpse(Corpse);
        cWait();
    }
    // Wait ���� ���� (�ӽ�)
    void cWait()
    {
        initCoroutine();
        ChangeState(State.WAIT);
        chaseCoroutine = false;
        resolveCoroutine = false;
        returnCoroutine = false;
        Destroy(this.gameObject);
    }
}