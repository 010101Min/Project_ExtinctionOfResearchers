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
        HIDE,
        ARRESTED
    }

    public State state = State.MOVE;
    private GameObject Suspect = null;
    public GameObject Corpse = null;

    public float moveSpeed;
    public float runSpeed;
    public float provokeSpeed;

    public GameObject Icon;
    private GameObject icon = null;

    private bool isTarget = false;
    private bool isPoisoned = false;
    private bool isCarriable = false;
    private bool isDetected = false;
    private bool isDead = false;
    private bool isHidden = false;
    private bool provokable = true;
    private bool witnessable = true;

    private float fStateTime = 0f;
    private float fPoisoned = 0f;
    private int fProvoke = 0;
    private int tempfProvoke = 0;
    private float DeathTimer = 0f;
    public float range = 5f;

    int npcLayer;
    int corpseLayer;
    int wallLayer;

    public GameObject player;
    private NavMeshAgent agent;
    private Animator anim;
    public GameObject[] pos;

    private Coroutine idleCoroutine;
    private Coroutine moveCoroutine;
    private Coroutine reportCoroutine;
    private Coroutine provokedCoroutine;
    private Coroutine sleepCoroutine;

    void Start()
    {
        // 시비에 넘어갈 확률 배정
        // 랜덤 외모 배정
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        state = State.MOVE;
        RandomProvoked();

        npcLayer = 1 << LayerMask.NameToLayer("NPC");
        corpseLayer = 1 << LayerMask.NameToLayer("CORPSE");
        wallLayer = 1 << LayerMask.NameToLayer("WALL");

        StartCoroutine(cSetIcons());

        pos = GameObject.FindGameObjectsWithTag("NPCPos");
        int movePos = Random.Range(0, pos.Length);
        gameObject.transform.position = pos[movePos].transform.position;
    }

    void Update()
    {
        // 시야 확인
        if (witnessable) CheckVision();
        // 중독 상태 확인
        if ((fPoisoned >= 1) && !isDead)
        {
            DeathTimer += Time.deltaTime;
            if (DeathTimer >= 30.0f) { fDead(); OneGameManager.Instance.addScore(50); }
        }

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
    public void RandomResearcher() { isTarget = true; }
    // 시비에 넘어갈 확률 배정
    private void RandomProvoked()
    {
        int rand = Random.Range(11, 91);
        fProvoke = rand;
    }

    // UI 아이콘들용 함수
    IEnumerator cSetIcons()
    {
        icon = Instantiate(Icon, Vector3.zero, Quaternion.identity, GameObject.Find("UICanvas").transform);
        while (true)
        {
            if (icon != null) { icon.GetComponent<NPCStatusIconController>().setNpc(this.gameObject); break; }
            yield return null;
        }
        yield break;
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
                state = State.PROVOKED;
            }
            else { Debug.Log("도발 실패"); }
        }
    }

    // 사망시 불러올 함수
    public void fDead()
    {
        anim.SetTrigger("Die"); anim.SetBool("Carried", false); anim.SetBool("Dead", true);
        initCoroutine();
        isDead = true;
        agent.enabled = false;
        isCarriable = true;
        witnessable = false;
        gameObject.layer = LayerMask.NameToLayer("CORPSE");
        icon.GetComponent<NPCStatusIconController>().showDead();
        // 게임매니저에서 킬 수 올리기 필요
        OneGameManager.Instance.killPeopleCount();
        if (isTarget) OneGameManager.Instance.killTargetCount();
        state = State.DIE;
    }
    public bool fGetDead() { return isDead; }

    // 시야 잃을 시 불러올 함수
    public void fGetBlinded()
    {
        icon.GetComponent<NPCStatusIconController>().showBlinded();
        witnessable = false;
        gameObject.layer = LayerMask.NameToLayer("INVISIBLENPC");
    }
    public void fOutBlinded()
    {
        icon.GetComponent<NPCStatusIconController>().showOutBlinded();
        witnessable = true;
        gameObject.layer = LayerMask.NameToLayer("NPC");
    }

    // 독 사용시 불러올 함수
    public float fGetPoisoned() { return fPoisoned; }
    public void fSetPoisoned(float percent)
    {
        fPoisoned = percent;
        icon.GetComponent<NPCStatusIconController>().showPoisonedPercent(percent);
    }
    public void fIfInPoisoned()
    {
        icon.GetComponent<NPCStatusIconController>().showPoisoned();
        isPoisoned = true;
    }
    public void fIfOutPoisoned() { isPoisoned = false; }
    public bool fIfPoisoned() { return isPoisoned; }

    // 시신 발견시 불러올 함수
    public void fDetected()
    {
        // 시신 위에 경광등 띄우기
        gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        isCarriable = false;
        isDetected = true;
        icon.GetComponent<NPCStatusIconController>().showDetected();
    }
    public bool fGetDetected() { return isDetected; }

    // 시신 은닉시 불러올 함수
    public void fHide()
    {
        if (!isDead) { fDead(); OneGameManager.Instance.addScore(40); }
        isHidden = true;

        gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        StartCoroutine(cHide());
    }
    IEnumerator cHide()
    {
        fHideIcon();
        float elapsedTime = 0f;
        while (elapsedTime < 60f)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(icon.gameObject);
        Destroy(gameObject);
    }
    public bool fGetCarriable() { return isCarriable; }
    public bool fGetHidden() { return isHidden; }
    public void fHideIcon() { Destroy(icon.gameObject); }

    // 시신 수습시 불러올 함수
    public void fResolved()
    {
        initCoroutine();
        fHideIcon();
        this.gameObject.tag = "Uninteractable";
        this.enabled = false;
    }

    // 구속시 불러올 함수
    public void fGetArrested()
    {
        fanim("Carried");
        initCoroutine();
        state = State.ARRESTED;
        agent.enabled = false;
        witnessable = false;
        tempfProvoke = fProvoke;
        fProvoke = 0;
    }
    public void fOutArrested()
    {
        fanim("Walk");
        witnessable = true;
        fProvoke = tempfProvoke;
        isCarriable = false;
        state = State.MOVE;
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

    // 애니메이션 구현
    public void fanim(string sState)
    {
        if (sState == "Idle")
        {
            anim.SetBool("Carried", false);
            anim.SetBool("Sleep", false);
            anim.SetBool("Run", false);
            anim.SetBool("Walk", false);
        }
        else if (sState == "Walk")
        {
            anim.SetBool("Carried", false);
            anim.SetBool("Sleep", false);
            anim.SetBool("Run", false);
            anim.SetBool("Walk", true);
        }
        else if (sState == "Run")
        {
            anim.SetBool("Carried", false);
            anim.SetBool("Sleep", false);
            anim.SetBool("Run", true);
            anim.SetBool("Walk", false);
        }
        else if (sState == "Sleep")
        {
            anim.SetBool("Carried", false);
            anim.SetBool("Sleep", true);
            anim.SetBool("Run", false);
            anim.SetBool("Walk", false);
        }
        else if (sState == "Carried") { anim.SetBool("Carried", true); }
    }
    public void dropCorpse() { anim.SetBool("Carried", false); }

    // IDLE 상태 구현
    private void fIdle() { fanim("Idle"); initCoroutine(); idleCoroutine = StartCoroutine(cIdle()); }
    IEnumerator cIdle()
    {
        agent.enabled = false;
        fStateTime = Random.Range(5f, 50f);
        yield return new WaitForSeconds(fStateTime);

        idleCoroutine = null;
        int nextState = Random.Range(1, 101);
        if (nextState <= 80) { state = State.MOVE; }
        else { state = State.SLEEP; }
    }
    

    // MOVE 상태 구현
    private void fMove() { fanim("Walk"); initCoroutine(); moveCoroutine = StartCoroutine(cMove()); }
    IEnumerator cMove()
    {
        agent.enabled = true;
        agent.speed = moveSpeed;
        int movePos = Random.Range(0, pos.Length);
        float variX = Random.Range(-range, range);
        float variZ = Random.Range(-range, range);
        Vector3 randomRot = new Vector3(0f, Random.Range(-120f, 120f), 0f);
        float angleDifference = float.MaxValue;
        Vector3 dest = new Vector3(pos[movePos].transform.position.x + variX, 0f, pos[movePos].transform.position.z + variZ);

        while ((transform.position - dest).magnitude >= 0.3f)
        {
            agent.SetDestination(dest);
            yield return null;
        }

        fanim("Idle");
        while (angleDifference > 1f)
        {
            angleDifference = Quaternion.Angle(transform.rotation, Quaternion.Euler(randomRot));

            float step = 200f * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(randomRot), step);
            yield return null;
        }

        moveCoroutine = null;
        int nextState = Random.Range(1, 101);
        if (nextState <= 90) { state = State.IDLE; }
        else { state = State.SLEEP; }
    }

    // SLEEP 상태 구현
    private void fSleep() { fanim("Sleep"); initCoroutine(); sleepCoroutine = StartCoroutine(cSleep()); }
    IEnumerator cSleep()
    {
        yield return new WaitForSeconds(2.133f);

        icon.GetComponent<NPCStatusIconController>().showSleeping();

        agent.enabled = false;
        tempfProvoke = fProvoke;
        witnessable = false;
        fProvoke = 0;
        isCarriable = true;

        fStateTime = Random.Range(20f, 30f);
        yield return new WaitForSeconds(fStateTime);

        witnessable = true;
        fProvoke = tempfProvoke;
        isCarriable = false;
        icon.GetComponent<NPCStatusIconController>().showOutSleeping();
        anim.SetBool("Sleep", false);
        yield return new WaitForSeconds(1.67f);

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
        if (Police != null)
        {
            foreach (GameObject police in Police)
            {
                if (!police.GetComponent<PoliceController>().getDead())
                {
                    dest = police;
                    isPoliceExist = true;
                }
            }
        }
        if (isPoliceExist && dest.gameObject.CompareTag("Phone")) { fGiveUp(tempProvokable); }
        return dest;
    }
    private void fCheckDetected(GameObject corpse, GameObject suspect, bool tempProvokable)
    {
        if (corpse != null)
        {
            if (corpse.CompareTag("NPC") && (corpse != null))
            {
                if (corpse.gameObject.GetComponent<NPCController>().fGetDetected())
                {
                    if (!fCheckSuspect(suspect)) { fGiveUp(tempProvokable); }
                }
            }
            else if (corpse.CompareTag("Police") && (corpse != null))
            {
                if (corpse.gameObject.GetComponent<PoliceController>().fGetDetected())
                {
                    if (!fCheckSuspect(suspect)) { fGiveUp(tempProvokable); }
                }
            }
        }
    }
    private void fCheckCorpseExist(GameObject corpse, GameObject suspect, bool tempProvokable)
    {
        if (corpse.CompareTag("NPC"))
        {
            if (corpse.GetComponent<NPCController>().fGetHidden())
            {
                if (!fCheckSuspect(suspect)) { fGiveUp(tempProvokable); }
            }
        }
        else if (corpse.CompareTag("Police"))
        {
            if (corpse.GetComponent<PoliceController>().fGetHidden())
            {
                if (!fCheckSuspect(suspect)) { fGiveUp(tempProvokable); }
            }
        }
    }
    private void fGiveUp(bool tempProvokable)
    {
        provokable = tempProvokable;
        agent.speed = moveSpeed;
        Corpse = null;
        Suspect = null;
        reportCoroutine = null;
        state = State.MOVE;
    }
    private bool fCheckSuspect(GameObject suspect)
    {
        if ((suspect == null) || (suspect.Equals(this.gameObject))) return false;
        if (suspect.Equals(player)) return true;
        else
        {
            if (suspect.GetComponent<NPCController>().fGetDead()) { return false; }
            else { return true; }
        }
    }
    IEnumerator cReport(GameObject corpse)
    {
        bool tempProvokable = provokable;
        provokable = false;

        GameObject dest = null;
        bool isPoliceExist = false;

        float distToPlayer = float.MaxValue;
        bool isPlayerInSight = false;

        // 의심할 캐릭터 결정
        // 만약 의심할 캐릭터가 없다면 자기 자신을 의심 (경찰 신고 시 신고자와 용의자 모두 받도록...)
        Collider[] colls = Physics.OverlapSphere(corpse.transform.position, 5f, npcLayer);

        if (player != null)
        {
            distToPlayer = Vector3.Distance(corpse.transform.position, player.transform.position);
            isPlayerInSight = !(Physics.Raycast(transform.position, (player.transform.position - this.transform.position).normalized, Vector3.Distance(this.transform.position, player.transform.position), wallLayer));
        }
        float distToTarget = 5f;

        if (colls.Length <= 0)
        {
            if ((distToPlayer <= 6f) && isPlayerInSight) { Suspect = player; }
            else { Suspect = this.gameObject; }
        }
        else
        {
            Suspect = this.gameObject;
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
        }

        agent.enabled = false;

        // NPC - 시신 - 용의자 선 긋기
        LineController.Instance.DrawLine(this.gameObject, this.transform, corpse.transform);
        if (Suspect != null) { LineController.Instance.DrawLine(this.gameObject, corpse.transform, Suspect.transform); }
        fanim("Idle");
        yield return new WaitForSeconds(0.5f);

        fanim("Run");
        agent.enabled = true;
        agent.speed = runSpeed;

        while (true)
        {// 만약 신고 하러 가는 도중 그 시신이 사라지면(은닉되면) 용의자만 신고
            fCheckCorpseExist(corpse, Suspect, tempProvokable);

            // 만약 경찰이 있었는데 신고 하러 가는 도중에 사라지면 신고 안 하고 포기
            dest = findPlace(ref isPoliceExist, tempProvokable);
            // 만약 신고 하러 가는 도중 그 시신이 신고받으면 신고 포기
            fCheckDetected(corpse, Suspect, tempProvokable);
            if (state == State.MOVE) yield break;
            if ((transform.position - dest.transform.position).magnitude <= 2.5f) break;
            agent.SetDestination(dest.transform.position);
            yield return null;
        }

        // 신고할 위치에 가장 먼저 도착한 한 명만 신고하도록
        OneGameManager.Instance.Report(this.gameObject, corpse, Suspect);

        // 원래대로 속성 돌리기
        fGiveUp(tempProvokable);
    }

    // Provoked 상태 구현
    private void fProvoked() { fanim("Walk"); initCoroutine(); provokedCoroutine = StartCoroutine(cProvoked()); }
    public bool fGetProvoked() { return provokable; }
    IEnumerator cProvoked()
    {
        agent.enabled = true;
        agent.speed = provokeSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < 15f)
        {
            // 거리가 10 이상이면 루프 탈출
            if ((this.transform.position - player.transform.position).magnitude >= 10f) { break; }

            agent.SetDestination(player.transform.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        agent.speed = moveSpeed;
        provokedCoroutine = null;
        state = State.MOVE;
    }
}