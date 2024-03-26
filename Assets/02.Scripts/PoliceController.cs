using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PoliceController : MonoBehaviour
{
    public enum State
    {
        RESOLVE,
        CHASE,
        RETURN,
        DIE,
        RECOGNIZED,
        HIDE
    }

    private State state;
    NavMeshAgent agent;
    private GameObject suspect;
    private GameObject policeCar;
    private int chaseTime = 0;

    private bool isDead = false;
    private bool isDetected = false;
    private bool isCarriable = false;

    private Coroutine resolveCoroutine;
    private Coroutine chaseCoroutine;
    private Coroutine returnCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 5f;
        policeCar = GameObject.FindGameObjectWithTag("PoliceCar");
    }

    void Update()
    {
        if (!isDead) CheckVision();

        switch (state)
        {
            case State.RESOLVE:
                //if (resolveCoroutine == null) fResolve();
                break;
            case State.CHASE:
                if (chaseCoroutine == null) fChase(suspect, chaseTime);
                break;
            case State.RETURN:
                if (returnCoroutine == null) fReturn();
                break;
            default: break;
        }
    }

    // �Ű� �޴� �Լ�
    public void Report(GameObject reporter, GameObject suspect, int count)
    {
        if (reporter != null)
        {
            initCoroutine();
            chaseTime = 10 + (count * 5);
            state = State.CHASE;
        }
        else
        {
            state = State.RESOLVE;
        }
    }

    // �þ� Ȯ�� (�ýŸ� ��)
    void CheckVision()
    {
        int layer = 1 << LayerMask.NameToLayer("CORPSE");
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, 10f, layer);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            GameObject target = targetsInViewRadius[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < 90f) // �¿�� ���� 90���� �þ߰�
            {
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, ~layer))   // �ýŰ��� �Ÿ� ���̿� ���� ���ع��� ���� (�ý� Ȯ��)
                {
                    // �ý� ���� �汤�� ����                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
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
        state = State.DIE;
    }

    // �ý� �߽߰� �ҷ��� �Լ�
    public void fDetected()
    {
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

    // �ڷ�ƾ �ʱ�ȭ �Լ�
    private void initCoroutine()
    {
        StopAllCoroutines();
        resolveCoroutine = null;
        chaseCoroutine = null;
        returnCoroutine = null;
}

    // Resolve ���� ���� (�ý� ����)
    // Chase ���� ���� (�����ڿ��� �̵�)
    private void fChase(GameObject suspect, float chaseTime)
    {

    }
    IEnumerator cChase(GameObject suspect, float chaseTime)
    {
        agent.speed = 10f;
        float elapsedTime = 0f;

        while (elapsedTime < chaseTime)
        {
            if ((transform.position - suspect.transform.position).magnitude <= 0.1f)
            {
                if (suspect.CompareTag("NPC"))
                {
                    suspect.GetComponent<NPCController>().fArrested();
                    // suspect�� ��ġ ���� ���� �ٲٱ�
                    break;
                }
                else
                {
                    // �÷��̾ ������ ��
                }
            }
            agent.SetDestination(suspect.transform.position);
            elapsedTime += Time.deltaTime;
        }
        state = State.RETURN;
        yield return null;
    }
    
    // Return ���� ����
    private void fReturn()
    {
        
    }
    IEnumerator cReturn()
    {
        agent.speed = 5f;
        while ((transform.position - policeCar.transform.position).magnitude >= 0.1f)
        {
            agent.SetDestination(policeCar.transform.position);
            yield return null;
        }
        transform.position = policeCar.transform.position;
        Destroy(this);
    }

}
