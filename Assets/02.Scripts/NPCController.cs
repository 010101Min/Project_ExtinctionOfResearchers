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
        // ������ ���� ����
        // �ú� �Ѿ Ȯ�� ����
        // ���� �ܸ� ����
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

        //deadState.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));
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
                    oldState = state;
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
        //deadState.gameObject.SetActive(true);
        //Dead.gameObject.SetActive(true);
    }

    // �þ� ���� �� �ҷ��� �Լ�
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

    // �ý� �߽߰� �ҷ��� �Լ�
    public void fDetected()
    {
        // �ý� ���� �汤�� ����
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

    // �ý� ������ �ҷ��� �Լ�
    public void fResolved()
    {
        initCoroutine();
        // ���� ����Ʈ ��� �ӽ÷� ����
        //Destroy(gameObject);
    }

    // ���ӽ� �ҷ��� �Լ�
    public void fArrested()
    {
        initCoroutine();
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

        // �ǽ��� ĳ���� ����
        // ���� �ǽ��� ĳ���Ͱ� ���ٸ� null�� �ǽ� (���� �Ű� �� �Ű��ڿ� ������ ��� �޵���...)
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
                if (!Physics.Raycast(transform.position, dirToTarget, distCurrent, wallLayer) && (distCurrent <= distToTarget))   // NPC-������ ���̿� ���� ����
                {
                    distToTarget = distCurrent;
                    Suspect = colls[i].gameObject;
                }
            }
            if (distToPlayer - 1 <= distToTarget) { Suspect = player; }
            if (Suspect.Equals(this.gameObject)) { Suspect = null; }
        }

        agent.enabled = false;

        // NPC - �ý� - ������ �� �߱�
        LineController.Instance.DrawLine(this.gameObject, this.transform, corpse.transform);
        if (Suspect != null) { LineController.Instance.DrawLine(this.gameObject, corpse.transform, Suspect.transform); }
        yield return new WaitForSeconds(0.5f);

        agent.enabled = true;
        agent.speed = 5.6f;

        while (true)
        {
            // ���� ������ �־��µ� �Ű� �Ϸ� ���� ���߿� ������� �Ű� �� �ϰ� ����
            dest = findPlace(ref isPoliceExist, tempProvokable);
            fCheckDetected(corpse, Suspect, tempProvokable);
            if (state == State.MOVE) yield break;
            if ((transform.position - dest.transform.position).magnitude <= 2.5f) break;
            agent.SetDestination(dest.transform.position);
            yield return null;
        }

        // ���⿡ ���ӸŴ����� �Ű��ϴ� �ڵ� (�Ű� ���� isDetected üũ)
        // �Ű��� ��ġ�� ���� ���� ������ �� �� �Ű��ϵ���
        GameManager.Instance.Report(this.gameObject, corpse, Suspect);
        if (corpse.CompareTag("NPC")) corpse.GetComponent<NPCController>().fDetected();
        if (corpse.CompareTag("Police")) corpse.GetComponent<PoliceController>().fDetected();
        //Debug.Log("Suspect : " + Suspect.gameObject.name + ", Corpse : " + corpse.gameObject.name);

        // ������� �Ӽ� ������
        fGiveUp(tempProvokable);
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