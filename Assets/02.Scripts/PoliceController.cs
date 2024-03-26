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

    // 신고 받는 함수
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

    // 시야 확인 (시신만 봄)
    void CheckVision()
    {
        int layer = 1 << LayerMask.NameToLayer("CORPSE");
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, 10f, layer);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            GameObject target = targetsInViewRadius[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < 90f) // 좌우로 각각 90도가 시야각
            {
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, ~layer))   // 시신과의 거리 사이에 뭔가 방해물이 없음 (시신 확인)
                {
                    // 시신 위에 경광등 띄우기                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
                }
            }
        }
    }

    // 사망시 불러올 함수
    public void fDead()
    {
        initCoroutine();
        isDead = true;
        agent.enabled = false;
        isCarriable = true;
        gameObject.layer = LayerMask.NameToLayer("CORPSE");
        state = State.DIE;
    }

    // 시신 발견시 불러올 함수
    public void fDetected()
    {
        gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        isCarriable = false;
        gameObject.GetComponent<PoliceController>().isDetected = true;
    }

    // 시신 은닉시 불러올 함수
    public void fHide()
    {
        agent.enabled = false;
        initCoroutine();
        Destroy(gameObject);
    }

    // 코루틴 초기화 함수
    private void initCoroutine()
    {
        StopAllCoroutines();
        resolveCoroutine = null;
        chaseCoroutine = null;
        returnCoroutine = null;
}

    // Resolve 상태 구현 (시신 수습)
    // Chase 상태 구현 (용의자에게 이동)
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
                    // suspect의 위치 경찰 위로 바꾸기
                    break;
                }
                else
                {
                    // 플레이어가 잡혔을 때
                }
            }
            agent.SetDestination(suspect.transform.position);
            elapsedTime += Time.deltaTime;
        }
        state = State.RETURN;
        yield return null;
    }
    
    // Return 상태 구현
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
