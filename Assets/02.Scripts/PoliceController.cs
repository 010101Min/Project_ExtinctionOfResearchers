using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    public State state;
    NavMeshAgent agent;
    private GameObject Suspect;
    private GameObject Corpse;
    private GameObject policeCar;
    private GameObject player;
    private int chaseTime = 0;

    private bool isDead = false;
    private bool isDetected = false;
    private bool isCarriable = false;

    private Coroutine resolveCoroutine;
    private Coroutine chaseCoroutine;
    private Coroutine returnCoroutine;

    void Start()
    {
        ChangeState(State.WAIT);
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 6f;
        policeCar = GameObject.FindGameObjectWithTag("PoliceCar");
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (!isDead) CheckVision();
    }

    // ���� �ٲٱ� �Լ�
    private void ChangeState(State newState)
    {
        Debug.Log($"���� ����: {state} -> {newState}");
        initCoroutine();
        state = newState;

        switch (newState)
        {
            case State.RESOLVE:
                fResolve(Corpse);
                break;
            case State.CHASE:
                fChase(Suspect, chaseTime);
                break;
            case State.RETURN:
                fReturn();
                break;
            default: break;
        }
    }


    // �Ű� �޴� �Լ�
    public void Report(GameObject reporter, GameObject corpse, GameObject suspect, int time)
    {
        Debug.Log("�Ű� ���� ������ : " + suspect.name + ", �ý� : " + corpse);
        initCoroutine();
        chaseTime = time;
        Corpse = corpse;
        Suspect = suspect;
        if (reporter != suspect) { ChangeState(State.CHASE); }
        else { ChangeState(State.RESOLVE); }
    }

    // �þ� Ȯ�� (�ýŸ� ��)
    void CheckVision()
    {
        int layer = 1 << LayerMask.NameToLayer("CORPSE");
        Collider[] corpses = Physics.OverlapSphere(transform.position, 10f, layer);

        for (int i = 0; i < corpses.Length; i++)
        {
            GameObject corpse = corpses[i].gameObject;
            Vector3 dirToCorpse = (corpse.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToCorpse) < 90f) // �¿�� ���� 90���� �þ߰�
            {
                float distToTarget = Vector3.Distance(transform.position, corpse.transform.position);
                if (!Physics.Raycast(transform.position, dirToCorpse, distToTarget, ~layer))   // �ýŰ��� �Ÿ� ���̿� ���� ���ع��� ���� (�ý� Ȯ��)
                {
                    // �߰ߵ��� ���� �ý� ���� �汤�� ����
                    // ���� Chase ���̾��ٸ� �Ѱ� �ִ� �� ��� ����
                    // Chase ���� �ƴϾ��ٸ� ������ ����
                    if (chaseCoroutine == null) { fFind(corpse); }
                    else
                    {
                        // ���� �������� �������̾�����
                        if (corpse.CompareTag("NPC")) corpse.gameObject.GetComponent<NPCController>().fDetected();
                        if (corpse.CompareTag("Police")) corpse.gameObject.GetComponent<PoliceController>().fDetected();
                    }
                }
            }
        }
    }

    // ����� �ҷ��� �Լ�
    public void fDead()
    {
        initCoroutine();
        isDead = true;
        agent.enabled = false;
        isCarriable = true;
        gameObject.layer = LayerMask.NameToLayer("CORPSE");
        ChangeState(State.DIE);
    }

    // �ý� �߽߰� �ҷ��� �Լ�
    public void fDetected()
    {
        // �ý� ���� �汤�� ����
        gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        isCarriable = false;
        gameObject.GetComponent<PoliceController>().isDetected = true;
    }

    // �ý� ���н� �ҷ��� �Լ�
    public void fHide()
    {
        agent.enabled = false;
        initCoroutine();
        Destroy(gameObject);
    }

    // �ý� ������ �ҷ��� �Լ�
    public void fResolved()
    {
        agent.enabled = false;
        initCoroutine();
        // ���� ����Ʈ ��� �ӽ÷� ����
        Destroy(gameObject);
    }

    // �ڷ�ƾ �ʱ�ȭ �Լ�
    private void initCoroutine()
    {
        StopCoroutine(cReturn());
        StopCoroutine(cResolve(Corpse));
        StopCoroutine(cChase(Suspect, chaseTime));
        resolveCoroutine = null;
        chaseCoroutine = null;
        returnCoroutine = null;
    }

    // �� �ߴ� �Լ�
    private void fDrawLine(Transform startPoint, Transform endPoint) { StartCoroutine(cDrawLine(startPoint, endPoint)); }
    IEnumerator cDrawLine(Transform startPoint, Transform endPoint)
    {
        GameObject lineObject = new GameObject("LineObject");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material.color = Color.red;

        float elapsedTime = 0f;

        while (elapsedTime < 0.5f)
        {
            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, endPoint.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(lineObject);
    }

    // Find ���� ���� (������ ã��)
    private void fFind(GameObject corpse)
    {
        // ���ο� ������ ����
        Collider[] colls = new Collider[13];
        int npcLayer = 1 << LayerMask.NameToLayer("NPC");
        int corpseLayer = 1 << LayerMask.NameToLayer("CORPSE");
        int policeLayer = 1 << LayerMask.NameToLayer("POLICE");
        int uninteractableLayer = 1 << LayerMask.NameToLayer("UNINTERACTABLE");

        int count = Physics.OverlapSphereNonAlloc(corpse.transform.position, 3f, colls, npcLayer);

        // ��� ���̾ ��ģ �� ��ü���� ����
        int layerMask = ~(npcLayer | corpseLayer | policeLayer | uninteractableLayer);

        float distToPlayer = Vector3.Distance(corpse.transform.position, player.transform.position);
        Vector3 dirToPlayer = (player.transform.position - corpse.transform.position).normalized;
        bool isPlayerInSight = !(Physics.Raycast(transform.position, dirToPlayer, distToPlayer, layerMask));
        float distToCorpse = float.MaxValue;

        if (count <= 0)
        {
            if ((distToPlayer <= 4f) && isPlayerInSight) { Suspect = player; }
            else { Suspect = null; }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 dirToTarget = (colls[i].gameObject.transform.position - corpse.transform.position).normalized;
                float distCurrent = Vector3.Distance(corpse.transform.position, colls[i].gameObject.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distCurrent, layerMask) && (distCurrent <= distToCorpse))   // NPC-������ ���̿� ���� ����
                {
                    distToCorpse = distCurrent;
                    Suspect = colls[i].gameObject;
                }
            }
            if (distToPlayer - 1 >= distToCorpse) Suspect = player;
        }

        // ���� - �ý� - ������ �� �߱�
        fDrawLine(this.transform, corpse.transform);
        if (Suspect != null)
        {
            fDrawLine(corpse.transform, Suspect.transform);
            // �߰� ����
            Report(this.gameObject, corpse, Suspect, chaseTime);
        }
    }
    
    // Resolve ���� ���� (�ý� ����)
    private void fResolve(GameObject corpse) { resolveCoroutine = StartCoroutine(cResolve(corpse)); }
    IEnumerator cResolve(GameObject corpse)
    {
        agent.speed = 6f;
        while (true)
        {
            if ((transform.position - corpse.transform.position).magnitude <= 2f) break;
            agent.SetDestination(corpse.transform.position);
            yield return null;
        }
        if (corpse.CompareTag("NPC")) corpse.GetComponent<NPCController>().fResolved();
        if (corpse.CompareTag("Police")) corpse.GetComponent<PoliceController>().fResolved();
        resolveCoroutine = null;
        ChangeState(State.RETURN);
    }

    // Chase ���� ���� (�����ڿ��� �̵�)
    private void fChase(GameObject suspect, float chaseTime) { chaseCoroutine = StartCoroutine(cChase(suspect, chaseTime)); }
    IEnumerator cChase(GameObject suspect, float chaseTime)
    {
        agent.speed = 10f;
        float elapsedTime = 0f;
        Debug.Log("�߰� ����, �߰� ��� : " + suspect.name);

        while (elapsedTime < chaseTime)
        {
            agent.SetDestination(suspect.transform.position);
            if ((transform.position - suspect.transform.position).magnitude <= 2.5f)
            {
                if (suspect.CompareTag("NPC"))
                {
                    suspect.GetComponent<NPCController>().fArrested();
                    Suspect = null;
                    // suspect�� ��ġ ���� ���� �ٲٱ�
                    break;
                }
                else
                {
                    // �÷��̾ ������ ��
                    Suspect = null;
                    break;
                }
            }
            elapsedTime += Time.deltaTime;
            //yield return null;
        }
        yield return null;
        Debug.Log("���� ����");
        chaseCoroutine = null;
        agent.speed = 6f;
        ChangeState(State.RETURN);
    }
    
    // Return ���� ����
    private void fReturn() { returnCoroutine = StartCoroutine(cReturn()); }
    IEnumerator cReturn()
    {
        Suspect = null;
        Corpse = null;
        agent.speed = 6f;
        while ((transform.position - policeCar.transform.position).magnitude >= 0.1f)
        {
            agent.SetDestination(policeCar.transform.position);
            yield return null;
        }
        transform.position = policeCar.transform.position;
        returnCoroutine = null;
        //Destroy(this);
    }
}
