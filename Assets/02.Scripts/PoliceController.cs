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

    // 상태 바꾸기 함수
    private void ChangeState(State newState)
    {
        Debug.Log($"상태 변경: {state} -> {newState}");
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


    // 신고 받는 함수
    public void Report(GameObject reporter, GameObject corpse, GameObject suspect, int time)
    {
        Debug.Log("신고 들어옴 용의자 : " + suspect.name + ", 시신 : " + corpse);
        initCoroutine();
        chaseTime = time;
        Corpse = corpse;
        Suspect = suspect;
        if (reporter != suspect) { ChangeState(State.CHASE); }
        else { ChangeState(State.RESOLVE); }
    }

    // 시야 확인 (시신만 봄)
    void CheckVision()
    {
        int layer = 1 << LayerMask.NameToLayer("CORPSE");
        Collider[] corpses = Physics.OverlapSphere(transform.position, 10f, layer);

        for (int i = 0; i < corpses.Length; i++)
        {
            GameObject corpse = corpses[i].gameObject;
            Vector3 dirToCorpse = (corpse.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToCorpse) < 90f) // 좌우로 각각 90도가 시야각
            {
                float distToTarget = Vector3.Distance(transform.position, corpse.transform.position);
                if (!Physics.Raycast(transform.position, dirToCorpse, distToTarget, ~layer))   // 시신과의 거리 사이에 뭔가 방해물이 없음 (시신 확인)
                {
                    // 발견되지 않은 시신 위에 경광등 띄우기
                    // 만약 Chase 중이었다면 쫓고 있던 애 계속 쫓음
                    // Chase 중이 아니었다면 용의자 추적
                    if (chaseCoroutine == null) { fFind(corpse); }
                    else
                    {
                        // 만약 누군가를 추적중이었으면
                        if (corpse.CompareTag("NPC")) corpse.gameObject.GetComponent<NPCController>().fDetected();
                        if (corpse.CompareTag("Police")) corpse.gameObject.GetComponent<PoliceController>().fDetected();
                    }
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
        ChangeState(State.DIE);
    }

    // 시신 발견시 불러올 함수
    public void fDetected()
    {
        // 시신 위에 경광등 띄우기
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

    // 시신 수습시 불러올 함수
    public void fResolved()
    {
        agent.enabled = false;
        initCoroutine();
        // 아직 이펙트 없어서 임시로 삭제
        Destroy(gameObject);
    }

    // 코루틴 초기화 함수
    private void initCoroutine()
    {
        StopCoroutine(cReturn());
        StopCoroutine(cResolve(Corpse));
        StopCoroutine(cChase(Suspect, chaseTime));
        resolveCoroutine = null;
        chaseCoroutine = null;
        returnCoroutine = null;
    }

    // 선 긋는 함수
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

    // Find 상태 구현 (용의자 찾기)
    private void fFind(GameObject corpse)
    {
        // 새로운 용의자 추적
        Collider[] colls = new Collider[13];
        int npcLayer = 1 << LayerMask.NameToLayer("NPC");
        int corpseLayer = 1 << LayerMask.NameToLayer("CORPSE");
        int policeLayer = 1 << LayerMask.NameToLayer("POLICE");
        int uninteractableLayer = 1 << LayerMask.NameToLayer("UNINTERACTABLE");

        int count = Physics.OverlapSphereNonAlloc(corpse.transform.position, 3f, colls, npcLayer);

        // 모든 레이어를 합친 후 전체에서 제외
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
                if (!Physics.Raycast(transform.position, dirToTarget, distCurrent, layerMask) && (distCurrent <= distToCorpse))   // NPC-용의자 사이에 벽이 없음
                {
                    distToCorpse = distCurrent;
                    Suspect = colls[i].gameObject;
                }
            }
            if (distToPlayer - 1 >= distToCorpse) Suspect = player;
        }

        // 경찰 - 시신 - 용의자 선 긋기
        fDrawLine(this.transform, corpse.transform);
        if (Suspect != null)
        {
            fDrawLine(corpse.transform, Suspect.transform);
            // 추격 시작
            Report(this.gameObject, corpse, Suspect, chaseTime);
        }
    }
    
    // Resolve 상태 구현 (시신 수습)
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

    // Chase 상태 구현 (용의자에게 이동)
    private void fChase(GameObject suspect, float chaseTime) { chaseCoroutine = StartCoroutine(cChase(suspect, chaseTime)); }
    IEnumerator cChase(GameObject suspect, float chaseTime)
    {
        agent.speed = 10f;
        float elapsedTime = 0f;
        Debug.Log("추격 시작, 추격 대상 : " + suspect.name);

        while (elapsedTime < chaseTime)
        {
            agent.SetDestination(suspect.transform.position);
            if ((transform.position - suspect.transform.position).magnitude <= 2.5f)
            {
                if (suspect.CompareTag("NPC"))
                {
                    suspect.GetComponent<NPCController>().fArrested();
                    Suspect = null;
                    // suspect의 위치 경찰 위로 바꾸기
                    break;
                }
                else
                {
                    // 플레이어가 잡혔을 때
                    Suspect = null;
                    break;
                }
            }
            elapsedTime += Time.deltaTime;
            //yield return null;
        }
        yield return null;
        Debug.Log("추적 종료");
        chaseCoroutine = null;
        agent.speed = 6f;
        ChangeState(State.RETURN);
    }
    
    // Return 상태 구현
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
