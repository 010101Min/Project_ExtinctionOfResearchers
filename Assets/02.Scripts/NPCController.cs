using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class NPCController : MonoBehaviour
{
    public enum State
    {
        IDLE,
        MOVE,
        REPORT,
        PROVOKED,
        SLEEP,
        DIE,
        HIDE
    }

    public State state = State.MOVE;
    private State oldState = State.IDLE;
    private GameObject Suspect = null;
    public GameObject Corpse = null;

    private bool isPoisoned = false;
    private bool isCarriable = false;
    public bool isDetected = false;
    private bool isRecognized = false;
    private bool isDead = false;
    private bool isArrested = false;
    private bool provokable = true;
    private bool witnessable = true;

    private float fStateTime = 0f;
    private float fPoisoned = 0f;
    private int fProvoke = 0;

    public GameObject player;
    private NavMeshAgent agent;
    private Animator anim;
    public Transform[] pos = new Transform[1];

    private Coroutine idleCoroutine;
    private Coroutine moveCoroutine;
    private Coroutine reportCoroutine;
    private Coroutine provokedCoroutine;
    private Coroutine sleepCoroutine;

    void Start()
    {
        // ������ ���� ����
        // �ú� �Ѿ Ȯ�� ����
        // ���� �ܸ� ����
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        state = State.MOVE;
        RandomProvoked();
    }

    void Update()
    {
        // �þ� Ȯ��
        if (witnessable) CheckVision();
        // �ߵ� ���� Ȯ��

        switch (state)
        {
            case State.IDLE:
                if (idleCoroutine == null) fIdle();
                break;
            case State.MOVE:
                if (moveCoroutine == null) fMove();
                break;
            case State.REPORT:
                if (reportCoroutine == null) fReport(Corpse);
                break;
            case State.PROVOKED:
                if (provokedCoroutine == null) fProvoked();
                break;
            case State.SLEEP:
                if (sleepCoroutine == null) fSleep();
                break;
            default: break;
        }
    }

    // ������ ���� ����
    private void RandomResearcher()
    {

    }
    // �ú� �Ѿ Ȯ�� ����
    private void RandomProvoked()
    {
        int rand = Random.Range(11, 91);
        fProvoke = rand;
    }
    // ���� �ܸ� ���� (���� ��)
    private void RandomAppearance()
    {

    }

    // �þ� Ȯ�� (�ýŸ� ��, �Լ� �ҷ����� �� Witnessable üũ �ʿ�)
    void CheckVision()
    {
        int layer = 1 << LayerMask.NameToLayer("CORPSE");
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, 7f, layer);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            GameObject target = targetsInViewRadius[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < 75f) // �¿�� ���� 75���� �þ߰�
            {
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, ~layer))   // �ýŰ��� �Ÿ� ���̿� ���� ���ع��� ���� (�ý� Ȯ��)
                {
                    Corpse = target;
                    oldState = state;
                    state = State.REPORT;
                }
            }
        }
    }
    
    // ���߽� �ҷ��� �Լ�
    public void CheckProvoked(int percent)
    {
        if (!provokable) { Debug.Log("���� �Ұ�"); return; }
        else
        {
            provokable = false;
            if (percent <= fProvoke)
            {
                Debug.Log("���� ����");
                oldState = state;
                state = State.PROVOKED;
            }
            else { Debug.Log("���� ����"); }
        }
    }

    // ����� �ҷ��� �Լ�
    public void fDead()
    {
        initCoroutine();
        isDead = true;
        agent.enabled = false;
        isCarriable = true;
        witnessable = false;
        gameObject.layer = LayerMask.NameToLayer("CORPSE");
        // ���ӸŴ������� ų �� �ø��� �ʿ�
        state = State.DIE;
    }

    // �ý� �߽߰� �ҷ��� �Լ�
    public void fDetected()
    {
        gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        isCarriable = false;
        gameObject.GetComponent<NPCController>().isDetected = true;
    }

    // �ý� ���н� �ҷ��� �Լ�
    public void fHide()
    {
        agent.enabled = false;
        initCoroutine();
        if (!isDead)
        {
            // ���ӸŴ������� ų �� �ø��� �ʿ�
        }
        Destroy(gameObject);
    }

    // ���ӽ� �ҷ��� �Լ�
    public void fArrested()
    {
        agent.enabled = false;
        witnessable = false;
        fProvoke = 0;
        isCarriable = false;
        isArrested = true;
    }

    // �ڷ�ƾ �ʱ�ȭ �Լ�
    private void initCoroutine()
    {
        StopAllCoroutines();
        idleCoroutine = null;
        moveCoroutine = null;
        reportCoroutine = null;
        provokedCoroutine = null;
        sleepCoroutine = null;
    }

    // �� �ߴ� �Լ�
    private GameObject DrawLine(Vector3 startPoint, Vector3 endPoint)
    {
        GameObject lineObject = new GameObject("LineObject");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material.color = Color.red;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        return lineObject;
    }

    // IDLE ���� ����
    private void fIdle() { initCoroutine(); idleCoroutine = StartCoroutine(cIdle()); }
    IEnumerator cIdle()
    {
        agent.enabled = false;
        fStateTime = Random.Range(5f, 50f);
        yield return new WaitForSeconds(fStateTime);
        
        oldState = state;
        idleCoroutine = null;
        int nextState = Random.Range(1, 101);
        if (nextState <= 80) { state = State.MOVE; }
        else { state = State.SLEEP; }
    }

    // MOVE ���� ����
    private void fMove() { initCoroutine(); moveCoroutine = StartCoroutine(cMove()); }
    IEnumerator cMove()
    {
        agent.enabled = true;
        int movePos = Random.Range(0, pos.Length);
        float variX = Random.Range(-8f, 8f);
        float variZ = Random.Range(-8f, 8f);
        Vector3 dest = new Vector3(pos[movePos].position.x + variX, 0f, pos[movePos].position.z + variZ);

        while ((transform.position - dest).magnitude >= 0.1f) {
            agent.SetDestination(dest);
            yield return null;
        }
        
        oldState = state;
        moveCoroutine = null;
        int nextState = Random.Range(1, 101);
        if (nextState <= 90) { state = State.IDLE; }
        else { state = State.SLEEP; }
    }

    // SLEEP ���� ����
    private void fSleep() { initCoroutine(); sleepCoroutine = StartCoroutine(cSleep()); }
    IEnumerator cSleep()
    {
        agent.enabled = false;
        int tempfProvoke = fProvoke;
        witnessable = false;
        fProvoke = 0;
        isCarriable = true;

        fStateTime = Random.Range(20f, 30f);
        yield return new WaitForSeconds(fStateTime);

        witnessable = true;
        fProvoke = tempfProvoke;
        isCarriable = false;

        oldState = state;
        sleepCoroutine = null;
        int nextState = Random.Range(1, 101);
        if (nextState <= 50) { state = State.MOVE; }
        else { state = State.IDLE; }
    }

    // Report ���� ����
    private void fReport(GameObject corpse) { initCoroutine(); reportCoroutine = StartCoroutine(cReport(corpse)); }
    IEnumerator cReport(GameObject corpse)
    {
        bool tempProvokable = provokable;
        provokable = false;

        // �Ű��� ��ġ ����... ��ȭ��/����
        float minDistance = float.MaxValue;
        GameObject dest = this.gameObject;
        GameObject Police = GameObject.FindGameObjectWithTag("Police");
        if (Police == null) {
            GameObject[] phones = GameObject.FindGameObjectsWithTag("Phone");
            minDistance = float.MaxValue;
            for (int i = 0; i < phones.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, phones[i].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    dest = phones[i];
                }
            }
        }
        else { dest = Police; }
        
        // �ǽ��� ĳ���� ����
        // ���� �ǽ��� ĳ���Ͱ� ���ٸ� �ڱ� �ڽ��� �ǽ� (���� �Ű� �� �Ű��ڿ� ������ ��� �޵���...)
        Collider[] colls = new Collider[13];
        int layer = 1 << LayerMask.NameToLayer("NPC");
        int count = Physics.OverlapSphereNonAlloc(corpse.transform.position, 2f, colls, layer);

        float distToPlayer = Vector3.Distance(corpse.transform.position, player.transform.position);
        Vector3 dirToPlayer = (player.transform.position - corpse.transform.position).normalized;
        bool isPlayerInSight = (Physics.Raycast(transform.position, dirToPlayer, distToPlayer, ~layer)) ? false : true;
        float distToTarget = float.MaxValue;

        if (count <= 0)
        {
            if ((distToPlayer <= 3f) && isPlayerInSight) { Suspect = player; }
            else { Suspect = this.gameObject; }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 dirToTarget = (colls[i].gameObject.transform.position - corpse.transform.position).normalized;
                distToTarget = Vector3.Distance(corpse.transform.position, colls[i].gameObject.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, ~layer))   // NPC-������ ���̿� ���� ����
                {
                    Suspect = colls[i].gameObject;
                }
            }
            if (distToPlayer - 1 >= distToTarget) Suspect = player;
        }
        
        agent.enabled = false;

        // NPC - �ý� - ������ �� �߱�
        GameObject line = DrawLine(this.transform.position, corpse.transform.position);
        GameObject line2 = null;
        if (Suspect != this.gameObject) { line2 = DrawLine(corpse.transform.position, Suspect.transform.position); }
        yield return new WaitForSeconds(0.5f);
        Destroy(line);
        
        if (line2 != null) { Destroy(line2); }
        agent.enabled = true;
        agent.speed = 5.6f;

        while ((transform.position - dest.transform.position).magnitude >= 0.1f)
        {
            agent.SetDestination(dest.transform.position);
            yield return null;
        }

        // ���⿡ ���ӸŴ����� �Ű��ϴ� �ڵ� (�Ű� ���� isDetected üũ)
        // �Ű��� ��ġ�� ���� ���� ������ �� �� �Ű��ϵ���
        if (corpse.gameObject.GetComponent<NPCController>() != null) corpse.gameObject.GetComponent<NPCController>().fDetected();
        if (corpse.gameObject.GetComponent<PoliceController>() != null) corpse.gameObject.GetComponent<PoliceController>().fDetected();
        Debug.Log("Suspect : " + Suspect.gameObject.name + ", Corpse : " + corpse.gameObject.name);

        // ������� �Ӽ� ������
        provokable = tempProvokable;
        agent.speed = 3.5f;
        Corpse = null;
        Suspect = null;
        oldState = state;
        reportCoroutine = null;
        state = State.MOVE;
    }

    // Provoked ���� ����
    private void fProvoked() { initCoroutine(); provokedCoroutine = StartCoroutine(cProvoked()); }
    IEnumerator cProvoked()
    {
        agent.enabled = true;
        agent.speed = 2.5f;
        float elapsedTime = 0f;

        while (elapsedTime < 15f)
        {
            // �Ÿ��� 10 �̻��̸� ���� Ż��
            if ((this.transform.position - player.transform.position).magnitude >= 10f) { break; }

            agent.SetDestination(player.transform.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        oldState = state;
        agent.speed = 3.5f;
        provokedCoroutine = null;
        state = State.MOVE;
    }

    // ���ӽ� �������� ������
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("PoliceCar") && isArrested) { fHide(); }
    }
}
