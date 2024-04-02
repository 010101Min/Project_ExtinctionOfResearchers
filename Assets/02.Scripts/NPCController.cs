using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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

    //public ScrollRect aliveState;
    //public Image Poisoned;
    //public Image Blinded;
    //public Image Sleeping;
    //public ScrollRect deadState;
    //public Image Dead;
    //public Image Detected;

    private bool isPoisoned = false;
    private bool isCarriable = false;
    public bool isDetected = false;
    private bool isRecognized = false;
    public bool isDead = false;
    private bool isArrested = false;
    private bool provokable = true;
    private bool witnessable = true;

    private float fStateTime = 0f;
    private float fPoisoned = 0f;
    private int fProvoke = 0;

    int npcLayer;
    int corpseLayer;
    int wallLayer;

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

        npcLayer = 1 << LayerMask.NameToLayer("NPC");
        corpseLayer = 1 << LayerMask.NameToLayer("CORPSE");
        wallLayer = 1 << LayerMask.NameToLayer("WALL");

        //aliveState.gameObject.SetActive(false);
        //Poisoned.gameObject.SetActive(false);
        //Blinded.gameObject.SetActive(false);
        //Sleeping.gameObject.SetActive(false);
        //deadState.gameObject.SetActive(false);
        //Dead.gameObject.SetActive(false);
        //Detected.gameObject.SetActive(false);
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

        //deadState.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));
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
        Collider[] targets = Physics.OverlapSphere(transform.position, 7f, corpseLayer);

        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - this.transform.position).normalized;

            if (Vector3.Angle(this.transform.forward, dirToTarget) < 75f) // 좌우로 각각 75도가 시야각
            {
                float distToTarget = Vector3.Distance(this.transform.position, target.transform.position);
                if (!Physics.Raycast(this.transform.position, dirToTarget, distToTarget, wallLayer)) // 시신과의 거리 사이에 뭔가 방해물이 없음 (시신 확인)
                {
                    Corpse = target;
                    oldState = state;
                    state = State.REPORT;
                }
            }
        }
    }
    public bool isWitnessable() { return witnessable; }

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
        //deadState.gameObject.SetActive(true);
        //Dead.gameObject.SetActive(true);
    }

    // 시야 잃을 시 불러올 함수
    public void fGetBlinded()
    {
        witnessable = false;
        gameObject.layer = LayerMask.NameToLayer("INVISIBLENPC");
    }
    public void fOutBlinded()
    {
        witnessable = true;
        gameObject.layer = LayerMask.NameToLayer("NPC");
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
        //Destroy(gameObject);
    }

    // 구속시 불러올 함수
    public void fArrested()
    {
        initCoroutine();
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

        while ((transform.position - dest).magnitude >= 0.1f)
        {
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
    private GameObject findPlace(ref bool isPoliceExist, bool tempProvokable)
    {
        float minDistance = float.MaxValue;
        GameObject dest = this.gameObject;
        GameObject[] Police = GameObject.FindGameObjectsWithTag("Police");
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
        if (Police == null)
        {
            if (isPoliceExist) { fGiveUp(tempProvokable); }
        }
        else
        {
            foreach (GameObject police in Police) { if (!police.GetComponent<PoliceController>().getDead()) dest = police; }
            isPoliceExist = true;
        }
        return dest;
    }
    private void fCheckDetected(GameObject corpse, GameObject suspect, bool tempProvokable)
    {
        if (corpse.CompareTag("NPC"))
        {
            if (corpse.gameObject.GetComponent<NPCController>().isDetected)
            {
                if (suspect == null) { fGiveUp(tempProvokable); }
            }
        }
        if (corpse.CompareTag("Police"))
        {
            if(corpse.gameObject.GetComponent<PoliceController>().isDetected)
            {
                if (suspect == null) { fGiveUp(tempProvokable); }
            }
        }
    }
    private void fGiveUp(bool tempProvokable)
    {
        provokable = tempProvokable;
        agent.speed = 3.5f;
        Corpse = null;
        Suspect = null;
        oldState = state;
        reportCoroutine = null;
        state = State.MOVE;
    }
    IEnumerator cReport(GameObject corpse)
    {
        bool tempProvokable = provokable;
        provokable = false;

        GameObject dest = null;
        bool isPoliceExist = false;

        // 의심할 캐릭터 결정
        // 만약 의심할 캐릭터가 없다면 null을 의심 (경찰 신고 시 신고자와 용의자 모두 받도록...)
        Collider[] colls = Physics.OverlapSphere(corpse.transform.position, 5f, npcLayer);

        float distToPlayer = Vector3.Distance(corpse.transform.position, player.transform.position);
        bool isPlayerInSight = !(Physics.Raycast(transform.position, (player.transform.position - this.transform.position).normalized, Vector3.Distance(this.transform.position, player.transform.position), wallLayer));
        float distToTarget = 5f;

        if (colls.Length <= 0)
        {
            if ((distToPlayer <= 6f) && isPlayerInSight) { Suspect = player; }
            else { Suspect = null; }
        }
        else
        {
            for (int i = 0; i < colls.Length; i++)
            {
                Vector3 dirToTarget = (colls[i].gameObject.transform.position - corpse.transform.position).normalized;
                float distCurrent = Vector3.Distance(corpse.transform.position, colls[i].gameObject.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distCurrent, wallLayer) && (distCurrent <= distToTarget))   // NPC-용의자 사이에 벽이 없음
                {
                    distToTarget = distCurrent;
                    Suspect = colls[i].gameObject;
                }
            }
            if (distToPlayer - 1 <= distToTarget) { Suspect = player; }
            if (Suspect.Equals(this.gameObject)) { Suspect = null; }
        }

        agent.enabled = false;

        // NPC - 시신 - 용의자 선 긋기
        LineController.Instance.DrawLine(this.gameObject, this.transform, corpse.transform);
        if (Suspect != null) { LineController.Instance.DrawLine(this.gameObject, corpse.transform, Suspect.transform); }
        yield return new WaitForSeconds(0.5f);

        agent.enabled = true;
        agent.speed = 5.6f;

        while (true)
        {
            // 만약 경찰이 있었는데 신고 하러 가는 도중에 사라지면 신고 안 하고 포기
            dest = findPlace(ref isPoliceExist, tempProvokable);
            fCheckDetected(corpse, Suspect, tempProvokable);
            if (state == State.MOVE) yield break;
            if ((transform.position - dest.transform.position).magnitude <= 2.5f) break;
            agent.SetDestination(dest.transform.position);
            yield return null;
        }

        // 여기에 게임매니저로 신고하는 코드 (신고 직전 isDetected 체크)
        // 신고할 위치에 가장 먼저 도착한 한 명만 신고하도록
        GameManager.Instance.Report(this.gameObject, corpse, Suspect);
        if (corpse.CompareTag("NPC")) corpse.GetComponent<NPCController>().fDetected();
        if (corpse.CompareTag("Police")) corpse.GetComponent<PoliceController>().fDetected();
        //Debug.Log("Suspect : " + Suspect.gameObject.name + ", Corpse : " + corpse.gameObject.name);

        // 원래대로 속성 돌리기
        fGiveUp(tempProvokable);
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