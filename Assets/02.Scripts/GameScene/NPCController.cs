using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

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
        // �ú� �Ѿ Ȯ�� ����
        // ���� �ܸ� ����
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
        // �þ� Ȯ��
        if (witnessable) CheckVision();
        // �ߵ� ���� Ȯ��
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

    // ������ ���� ����
    public void RandomResearcher() { isTarget = true; }
    // �ú� �Ѿ Ȯ�� ����
    private void RandomProvoked()
    {
        int rand = Random.Range(11, 91);
        fProvoke = rand;
    }

    // UI �����ܵ�� �Լ�
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

    // �þ� Ȯ�� (�ýŸ� ��, �Լ� �ҷ����� �� Witnessable üũ �ʿ�)
    void CheckVision()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, 7f, corpseLayer);

        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - this.transform.position).normalized;

            if (Vector3.Angle(this.transform.forward, dirToTarget) < 75f) // �¿�� ���� 75���� �þ߰�
            {
                float distToTarget = Vector3.Distance(this.transform.position, target.transform.position);
                if (!Physics.Raycast(this.transform.position, dirToTarget, distToTarget, wallLayer)) // �ýŰ��� �Ÿ� ���̿� ���� ���ع��� ���� (�ý� Ȯ��)
                {
                    Corpse = target;
                    state = State.REPORT;
                }
            }
        }
    }
    public bool isWitnessable() { return witnessable; }

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
                state = State.PROVOKED;
            }
            else { Debug.Log("���� ����"); }
        }
    }

    // ����� �ҷ��� �Լ�
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
        // ���ӸŴ������� ų �� �ø��� �ʿ�
        OneGameManager.Instance.killPeopleCount();
        if (isTarget) OneGameManager.Instance.killTargetCount();
        state = State.DIE;
    }
    public bool fGetDead() { return isDead; }

    // �þ� ���� �� �ҷ��� �Լ�
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

    // �� ���� �ҷ��� �Լ�
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

    // �ý� �߽߰� �ҷ��� �Լ�
    public void fDetected()
    {
        // �ý� ���� �汤�� ����
        gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
        isCarriable = false;
        isDetected = true;
        icon.GetComponent<NPCStatusIconController>().showDetected();
    }
    public bool fGetDetected() { return isDetected; }

    // �ý� ���н� �ҷ��� �Լ�
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

    // �ý� ������ �ҷ��� �Լ�
    public void fResolved()
    {
        initCoroutine();
        fHideIcon();
        this.gameObject.tag = "Uninteractable";
        this.enabled = false;
    }

    // ���ӽ� �ҷ��� �Լ�
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

    // �ִϸ��̼� ����
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

    // IDLE ���� ����
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
    

    // MOVE ���� ����
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

    // SLEEP ���� ����
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

    // Report ���� ����
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

        // �ǽ��� ĳ���� ����
        // ���� �ǽ��� ĳ���Ͱ� ���ٸ� �ڱ� �ڽ��� �ǽ� (���� �Ű� �� �Ű��ڿ� ������ ��� �޵���...)
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
                if (!Physics.Raycast(transform.position, dirToTarget, distCurrent, wallLayer) && (distCurrent <= distToTarget))   // NPC-������ ���̿� ���� ����
                {
                    distToTarget = distCurrent;
                    Suspect = colls[i].gameObject;
                }
            }
            if (distToPlayer - 1 <= distToTarget) { Suspect = player; }
        }

        agent.enabled = false;

        // NPC - �ý� - ������ �� �߱�
        LineController.Instance.DrawLine(this.gameObject, this.transform, corpse.transform);
        if (Suspect != null) { LineController.Instance.DrawLine(this.gameObject, corpse.transform, Suspect.transform); }
        fanim("Idle");
        yield return new WaitForSeconds(0.5f);

        fanim("Run");
        agent.enabled = true;
        agent.speed = runSpeed;

        while (true)
        {// ���� �Ű� �Ϸ� ���� ���� �� �ý��� �������(���еǸ�) �����ڸ� �Ű�
            fCheckCorpseExist(corpse, Suspect, tempProvokable);

            // ���� ������ �־��µ� �Ű� �Ϸ� ���� ���߿� ������� �Ű� �� �ϰ� ����
            dest = findPlace(ref isPoliceExist, tempProvokable);
            // ���� �Ű� �Ϸ� ���� ���� �� �ý��� �Ű������ �Ű� ����
            fCheckDetected(corpse, Suspect, tempProvokable);
            if (state == State.MOVE) yield break;
            if ((transform.position - dest.transform.position).magnitude <= 2.5f) break;
            agent.SetDestination(dest.transform.position);
            yield return null;
        }

        // �Ű��� ��ġ�� ���� ���� ������ �� �� �Ű��ϵ���
        OneGameManager.Instance.Report(this.gameObject, corpse, Suspect);

        // ������� �Ӽ� ������
        fGiveUp(tempProvokable);
    }

    // Provoked ���� ����
    private void fProvoked() { fanim("Walk"); initCoroutine(); provokedCoroutine = StartCoroutine(cProvoked()); }
    public bool fGetProvoked() { return provokable; }
    IEnumerator cProvoked()
    {
        agent.enabled = true;
        agent.speed = provokeSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < 15f)
        {
            // �Ÿ��� 10 �̻��̸� ���� Ż��
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