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
    public Transform[] pos = new Transform[2];

    private Coroutine idleCoroutine;
    private Coroutine moveCoroutine;
    private Coroutine reportCoroutine;
    private Coroutine provokedCoroutine;
    private Coroutine sleepCoroutine;

    void Start()
    {
        // 연구원 여부 배정
        // 시비에 넘어갈 확률 배정
        // 랜덤 외모 배정
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        state = State.MOVE;
        RandomProvoked();
    }

    void Update()
    {
        // 시야 확인
        if (witnessable) CheckVision();
        // 중독 상태 확인

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

    // 연구원 여부 배정
    private void RandomResearcher()
    {

    }
    // 시비에 넘어갈 확률 배정
    private void RandomProvoked()
    {
        int rand = Random.Range(11, 91);
        fProvoke = rand;
    }
    // 랜덤 외모 배정 (구현 전)
    private void RandomAppearance()
    {

    }

    // 시야 확인 (시신만 봄, 함수 불러오기 전 Witnessable 체크 필요)
    void CheckVision()
    {
        int layer = 1 << LayerMask.NameToLayer("CORPSE");
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, 7f, layer);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            GameObject target = targetsInViewRadius[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < 75f) // 좌우로 각각 75도가 시야각
            {
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, ~layer))   // 시신과의 거리 사이에 뭔가 방해물이 없음 (시신 확인)
                {
                    Corpse = target;
                    oldState = state;
                    state = State.REPORT;
                }
            }
        }
    }
    
    // 도발시 불러올 함수
    public void CheckProvoked(int percent)
    {
        if (!provokable) { Debug.Log("도발 불가"); return; }
        else
        {
            provokable = false;
            if (percent <= fProvoke)
            {
                Debug.Log("도발 성공");
                oldState = state;
                state = State.PROVOKED;
            }
            else { Debug.Log("도발 실패"); }
        }
    }

    // 사망시 불러올 함수
    public void fDead()
    {
        initCoroutine();
        isDead = true;
        agent.enabled = false;
        isCarriable = true;
        witnessable = false;
        gameObject.layer = LayerMask.NameToLayer("CORPSE");
        // 게임매니저에서 킬 수 올리기 필요
        state = State.DIE;
    }

    // 시신 발견시 불러올 함수
    public void fDetected()
    {
        // 시신 위에 경광등 띄우기
        gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        isCarriable = false;
        gameObject.GetComponent<NPCController>().isDetected = true;
    }

    // 시신 은닉시 불러올 함수
    public void fHide()
    {
        agent.enabled = false;
        initCoroutine();
        if (!isDead)
        {
            // 게임매니저에서 킬 수 올리기 필요
        }
        Destroy(gameObject);
    }

    // 시신 수습시 불러올 함수
    public void fResolved()
    {
        initCoroutine();
        // 아직 이펙트 없어서 임시로 삭제
        Destroy(gameObject);
    }

    // 구속시 불러올 함수
    public void fArrested()
    {
        agent.enabled = false;
        witnessable = false;
        fProvoke = 0;
        isCarriable = false;
        isArrested = true;
    }

    // 코루틴 초기화 함수
    private void initCoroutine()
    {
        StopAllCoroutines();
        idleCoroutine = null;
        moveCoroutine = null;
        reportCoroutine = null;
        provokedCoroutine = null;
        sleepCoroutine = null;
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

    // IDLE 상태 구현
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

    // MOVE 상태 구현
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

    // SLEEP 상태 구현
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

    // Report 상태 구현
    private void fReport(GameObject corpse) { initCoroutine(); reportCoroutine = StartCoroutine(cReport(corpse)); }
    // 신고할 위치 찾는 함수
    private GameObject findPlace()
    {
        float minDistance = float.MaxValue;
        GameObject dest = this.gameObject;
        GameObject Police = GameObject.FindGameObjectWithTag("Police");
        if (Police == null)
        {
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
        return dest;
    }
    IEnumerator cReport(GameObject corpse)
    {
        bool tempProvokable = provokable;
        provokable = false;

        // 신고할 위치 결정
        GameObject dest = findPlace();
        
        // 의심할 캐릭터 결정
        // 만약 의심할 캐릭터가 없다면 자기 자신을 의심 (경찰 신고 시 신고자와 용의자 모두 받도록...)
        Collider[] colls = new Collider[13];
        int layer = 1 << LayerMask.NameToLayer("NPC");
        int count = Physics.OverlapSphereNonAlloc(corpse.transform.position, 3f, colls, layer);

        // 각 레이어의 마스크 생성
        int npcLayer = 1 << LayerMask.NameToLayer("NPC");
        int corpseLayer = 1 << LayerMask.NameToLayer("CORPSE");
        int policeLayer = 1 << LayerMask.NameToLayer("POLICE");
        int uninteractableLayer = 1 << LayerMask.NameToLayer("UNINTERACTABLE");

        // 모든 레이어를 합친 후 전체에서 제외
        int layerMask = ~(npcLayer | corpseLayer | policeLayer | uninteractableLayer);

        float distToPlayer = Vector3.Distance(corpse.transform.position, player.transform.position);
        Vector3 dirToPlayer = (player.transform.position - corpse.transform.position).normalized;
        bool isPlayerInSight = !(Physics.Raycast(transform.position, dirToPlayer, distToPlayer, layerMask));
        float distToTarget = float.MaxValue;

        if (count <= 0)
        {
            if ((distToPlayer <= 4f) && isPlayerInSight) { Suspect = player; }
            else { Suspect = this.gameObject; }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 dirToTarget = (colls[i].gameObject.transform.position - corpse.transform.position).normalized;
                float distCurrent = Vector3.Distance(corpse.transform.position, colls[i].gameObject.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distCurrent, layerMask) && (distCurrent <= distToTarget))   // NPC-용의자 사이에 벽이 없음
                {
                    distToTarget = distCurrent;
                    Suspect = colls[i].gameObject;
                }
            }
            if (distToPlayer - 1 >= distToTarget) Suspect = player;
        }
        
        agent.enabled = false;

        // NPC - 시신 - 용의자 선 긋기
        fDrawLine(this.transform, corpse.transform);
        if (Suspect != this.gameObject) { fDrawLine(corpse.transform, Suspect.transform); }
        yield return new WaitForSeconds(0.5f);
        
        agent.enabled = true;
        agent.speed = 5.6f;

        while (true)
        {
            if (dest == null) dest = findPlace();
            if ((transform.position - dest.transform.position).magnitude <= 2.5f) break;
            agent.SetDestination(dest.transform.position);
            yield return null;
        }

        // 여기에 게임매니저로 신고하는 코드 (신고 직전 isDetected 체크)
        // 신고할 위치에 가장 먼저 도착한 한 명만 신고하도록
        GameManager.Instance.Report(this.gameObject, corpse.gameObject, Suspect.gameObject);
        if (corpse.CompareTag("NPC")) corpse.gameObject.GetComponent<NPCController>().fDetected();
        if (corpse.CompareTag("Police")) corpse.gameObject.GetComponent<PoliceController>().fDetected();
        Debug.Log("Suspect : " + Suspect.gameObject.name + ", Corpse : " + corpse.gameObject.name);

        // 원래대로 속성 돌리기
        provokable = tempProvokable;
        agent.speed = 3.5f;
        Corpse = null;
        Suspect = null;
        oldState = state;
        reportCoroutine = null;
        state = State.MOVE;
    }

    // Provoked 상태 구현
    private void fProvoked() { initCoroutine(); provokedCoroutine = StartCoroutine(cProvoked()); }
    IEnumerator cProvoked()
    {
        agent.enabled = true;
        agent.speed = 2.5f;
        float elapsedTime = 0f;

        while (elapsedTime < 15f)
        {
            // 거리가 10 이상이면 루프 탈출
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

    // 구속시 경찰차에 닿으면
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("PoliceCar") && isArrested) { fHide(); }
    }
}
