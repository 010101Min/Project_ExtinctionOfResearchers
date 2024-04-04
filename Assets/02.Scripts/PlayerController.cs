using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 120f;
    public Transform carryPos;
    public Image HandCuff;
    public Image Stamina_Box;
    public Image Stamina_Bar;
    private Coroutine refillStaminaCoroutine;
    private bool isChased = false;
    private bool isCarrying;
    public bool isRunning;
    private GameObject carryingBody = null;

    GameObject nearestNPC = null;
    GameObject nearestBomb = null;
    GameObject nearestWindow = null;
    GameObject nearestShortcut = null;
    GameObject nearestCarriable = null;
    Transform tr;

    int visiblenpcLayer;
    int invisiblenpcLayer;
    int npcLayer;
    int bombLayer;
    int windowLayer;
    int shortcutLayer;
    int visiblecorpseLayer;
    int invisiblecorpseLayer;
    int corpseLayer;
    int uninteractableLayer;

    void Start()
    {
        tr = GetComponent<Transform>();

        visiblenpcLayer = 1 << LayerMask.NameToLayer("NPC");
        invisiblenpcLayer = 1 << LayerMask.NameToLayer("INVISIBLENPC");
        npcLayer = (visiblenpcLayer | invisiblenpcLayer);

        visiblecorpseLayer = 1 << LayerMask.NameToLayer("CORPSE");
        invisiblecorpseLayer = 1 << LayerMask.NameToLayer("INVISIBLECORPSE");
        corpseLayer = (visiblecorpseLayer | invisiblecorpseLayer);

        bombLayer = 1 << LayerMask.NameToLayer("BOMB");
        windowLayer = 1 << LayerMask.NameToLayer("WINDOW");
        shortcutLayer = 1 << LayerMask.NameToLayer("SHORTCUT");
        
        uninteractableLayer = 1 << LayerMask.NameToLayer("UNINTERACTABLE");

        isCarrying = false;

        HandCuff.enabled = false;
        Stamina_Bar.fillAmount = 1f;
        Stamina_Box.enabled = false;
        Stamina_Bar.enabled = false;
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float r = Input.GetAxis("Mouse X");

        if (Input.GetKey(KeyCode.LeftShift) && !(x == 0 && z == 0) && !isRunning && (Stamina_Bar.fillAmount >= 0.2f))
        {
            startRun();
        }
        else if (!Input.GetKey(KeyCode.LeftShift) || (x == 0 && z == 0)) { isRunning = false; }

        Vector3 moveDirection = new Vector3(x, 0f, z);
        tr.Translate(moveDirection * moveSpeed * Time.deltaTime);
        tr.Rotate(Vector3.up * turnSpeed * Time.deltaTime * r);

        findObject(out nearestNPC, out nearestBomb, out nearestWindow, out nearestShortcut, out nearestCarriable);

        if (nearestNPC != null)
        {
            // E, Q 키 작동 가능하다고 UI에 띄우기
            
        }
        if (nearestBomb != null)
        {
            // 함정 작동 가능하다고 UI에 띄우기
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (nearestBomb.GetComponent<BombController>() != null) { nearestBomb.GetComponent<BombController>().UseBomb(); }
            }
            
        }
        if (nearestShortcut != null)
        {
            // 지름길 작동 가능하다고 UI에 띄우기
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Transform newTrans = nearestShortcut.GetComponent<ShortCutController>().UseShortCut(this.gameObject);
                if (newTrans != null)
                {
                    this.gameObject.transform.position = newTrans.position;
                    this.gameObject.transform.rotation = newTrans.rotation;
                }
            }
        }
        if ((nearestCarriable != null) && (!isCarrying) && (carryingBody == null))  // 들 수 있는 게 근처에 있고 지금 들고 있는 게 없으면
        {
            // 캐릭터 운반 가능하다고 UI에 띄우기
            if (Input.GetKeyUp(KeyCode.R))
            {
                isCarrying = true;
                carryingBody = nearestCarriable;
                StartCoroutine(cCarryBody(carryingBody));
            }
        }
        else if (isCarrying && (carryingBody != null))
        {
            if (Input.GetKeyUp(KeyCode.R)) { dropBody(carryingBody); }
        }
        if (nearestWindow != null)
        {
            // 은닉처 작동 가능하다고 UI에 띄우기
            if (isCarrying && (Input.GetKeyUp(KeyCode.Space)))
            {
                hideBody(carryingBody);
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) { killNpc(nearestNPC); }
        if (Input.GetKeyDown(KeyCode.Q)) { provokeNpc(nearestNPC); }
    }

    void findObject(out GameObject nearestNPC, out GameObject nearestBomb, out GameObject nearestWindow, out GameObject nearestShortcut, out GameObject nearestCarriable)
    {
        Collider[] colls = Physics.OverlapSphere(this.transform.position, 3f, (bombLayer | windowLayer | shortcutLayer));   // 탐지해야 할 것 : 함정, 은닉처, 지름길
        List<GameObject> bombs = new List<GameObject>();
        List<GameObject> windows = new List<GameObject>();
        List<GameObject> shortcuts = new List<GameObject>();

        if (colls.Length > 0)
        {
            for (int i = 0; i < colls.Length; i++)
            {
                Vector3 dir = (colls[i].transform.position - transform.position).normalized;

                float distToTarget = Vector3.Distance(transform.position, colls[i].transform.position);
                if (!Physics.Raycast(transform.position, dir, distToTarget, ~(npcLayer | corpseLayer | uninteractableLayer | bombLayer | windowLayer | shortcutLayer)))   // 공격 대상과 플레이어 사이에 있어도 되는 것: 다른 npc, 시신(corpse, uninteractable)
                {
                    if (colls[i].CompareTag("Bomb")) { bombs.Add(colls[i].gameObject); }
                    if (colls[i].CompareTag("Window")) { windows.Add(colls[i].gameObject); }
                    if (colls[i].CompareTag("ShortCut")) { shortcuts.Add(colls[i].gameObject); }
                }
            }
        }
        // 행동 가능 범위 내에 있는 npc, 함정, 은닉처, 지름길들 중 가장 가까운 애 찾기
        nearestBomb = findNearest(bombs);
        nearestWindow = findNearest(windows);
        nearestShortcut = findNearest(shortcuts);
        nearestNPC = findNpc();
        nearestCarriable = findCarriable();
    }

    GameObject findNpc()
    {
        Collider[] colls = Physics.OverlapSphere(this.transform.position, 3f, npcLayer);   // 탐지해야 할 것 : NPC
        List<GameObject> npcs = new List<GameObject>();

        if (colls.Length > 0)
        {
            for (int i = 0; i < colls.Length; i++)
            {
                Vector3 dir = (colls[i].transform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, dir) < 75f)    // 전방 좌우 75도가 공격 가능 범위
                {
                    float distToTarget = Vector3.Distance(transform.position, colls[i].transform.position);
                    if (!Physics.Raycast(transform.position, dir, distToTarget, ~(npcLayer | corpseLayer | uninteractableLayer)))   // 공격 대상과 플레이어 사이에 있어도 되는 것: 다른 npc, 시신(corpse, uninteractable)
                    {
                        if (colls[i].CompareTag("NPC")) { if (!colls[i].gameObject.GetComponent<NPCController>().fGetDead()) npcs.Add(colls[i].gameObject); }
                    }
                }
            }
        }
        return(findNearest(npcs));
    }
    GameObject findCarriable()
    {
        Collider[] colls = Physics.OverlapSphere(this.transform.position, 3f, (npcLayer | corpseLayer));   // 탐지해야 할 것 : NPC, 시신(미신고)
        List<GameObject> bodies = new List<GameObject>();

        if (colls.Length > 0)
        {
            for (int i = 0; i < colls.Length; i++)
            {
                Vector3 dir = (colls[i].transform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, dir) < 75f)
                {
                    float distToTarget = Vector3.Distance(transform.position, colls[i].transform.position);
                    if (!Physics.Raycast(transform.position, dir, distToTarget, ~(npcLayer | corpseLayer | uninteractableLayer)))   // 공격 대상과 플레이어 사이에 있어도 되는 것: 다른 npc, 시신(corpse, uninteractable)
                    {
                        if ((colls[i].CompareTag("NPC") && colls[i].gameObject.GetComponent<NPCController>().fGetCarriable()) || (colls[i].CompareTag("Police") && colls[i].gameObject.GetComponent<PoliceController>().fGetCarriable()))
                        {
                            bodies.Add(colls[i].gameObject);
                        }
                    }
                }
            }
        }
        return (findNearest(bodies));
    }
    GameObject findNearest(List<GameObject> lists)
    {
        float minDistance = float.MaxValue;
        GameObject nearestItem = null;
        if (lists.Count > 0)
        {
            nearestItem = lists[0];
            for (int i = 0; i < lists.Count; i++)
            {
                float distance = Vector3.Distance(transform.position, lists[i].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestItem = lists[i];
                }
            }
            if (nearestItem.CompareTag("ShortCut")) { nearestItem = nearestItem.transform.parent.gameObject; }
        }
        return nearestItem;
    }

    IEnumerator cRun()
    {
        moveSpeed *= 1.6f;

        Stamina_Box.enabled = true;
        Stamina_Bar.enabled = true;

        float spare = Stamina_Bar.fillAmount; // 스태미너 남은 값부터 시작
        float timer = 0f;

        while (true)
        {
            if (Stamina_Bar.fillAmount <= 0) break;
            if (!isRunning) break;
            timer += Time.deltaTime;
            float normalizedTimer = Mathf.Clamp(timer / (spare * 5f), 0f, 1f); // 타이머 값을 0과 1 사이로 정규화
            Stamina_Bar.fillAmount = Mathf.Lerp(spare, 0f, normalizedTimer);
            yield return null;
        }
        Debug.Log("cRun 에서 while (true) 루프 탈출");
        refillStaminaCoroutine = StartCoroutine(cRefillStamina());
    }
    IEnumerator cRefillStamina()
    {
        moveSpeed *= 0.625f;
        isRunning = false;

        float spare = Stamina_Bar.fillAmount; // 스태미너 남은 값부터 시작
        float timer = 0f;

        while (true)
        {
            if (Stamina_Bar.fillAmount >= 1) break;
            if (isRunning) yield break;
            timer += Time.deltaTime;
            float normalizedTimer = Mathf.Clamp(timer / ((1 - spare) * 12f), 0f, 1f); // 타이머 값을 0과 1 사이로 정규화
            Stamina_Bar.fillAmount = Mathf.Lerp(spare, 1f, normalizedTimer);
            yield return null;
        }

        Stamina_Box.enabled = false;
        Stamina_Bar.enabled = false;
        refillStaminaCoroutine = null;
    }
    void startRun() { isRunning = true; StartCoroutine(cRun()); }

    void killNpc(GameObject target)
    {
        if (target != null) { target.GetComponent<NPCController>().fDead(); }
        else { Debug.Log("타겟 없음"); }
    }
    void provokeNpc(GameObject target)
    {
        int rand = Random.Range(1, 101);
        if (target != null) { target.GetComponent<NPCController>().CheckProvoked(rand); }
        else { Debug.Log("타겟 없음"); }
    }
    IEnumerator cCarryBody(GameObject body)
    {
        moveSpeed *= 0.625f;
        while (true)
        {
            if (!isCarrying) { yield break; }
            if (body.CompareTag("NPC"))
            {
                if (body.GetComponent<NPCController>().fGetCarriable()) { body.transform.position = carryPos.position; }
                else { dropBody(body); yield break; }
            }
            else if (body.CompareTag("Police"))
            {
                if (body.GetComponent<PoliceController>().fGetCarriable()) { body.transform.position = carryPos.position; }
                else { dropBody(body); yield break; }
            }
            yield return null;
        }
    }
    void dropBody(GameObject body)
    {
        moveSpeed *= 1.6f;
        body.transform.position = new Vector3(body.transform.position.x, 0f, body.transform.position.z);
        isCarrying = false;
        carryingBody = null;
    }
    void hideBody(GameObject body)
    {
        moveSpeed *= 1.6f;
        isCarrying = false;
        carryingBody = null;
        if (body.CompareTag("NPC"))
        {
            body.GetComponent<NPCController>().fHide();
        }
        else if (body.CompareTag("Police"))
        {
            body.GetComponent<PoliceController>().fHide();
        }
    }

    public void inChased()
    {
        isChased = true;
        HandCuff.enabled = true;
    }
    public void outChased()
    {
        isChased = false;
        HandCuff.enabled = false;
    }
    public bool getChased() { return isChased; }
}