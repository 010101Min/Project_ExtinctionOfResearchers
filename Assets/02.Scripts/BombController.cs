using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BombController : MonoBehaviour
{
    //public GameObject ExplosionEffect;
    public Image defaultBombIcon;
    public Image poisonBombIcon;
    public Image fireBombIcon;
    private GameObject player;
    float minScale = 0.1f; // 최소 스케일 값
    float maxScale = 0.5f; // 최대 스케일 값
    float maxDistance = 60f;

    private Camera mainCamera;
    private int wallLayer;

    private bool isUsable = true;
    float explosionTimer = 10f;
    float blindTimer = 20f;
    float poisonTimer = 20f;
    float poisonedTimer = 15f;
    public State state;

    public enum State
    {
        BOMB,
        FIREEXTINGUISHER,
        POISON
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mainCamera = Camera.main;
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
    }

    public void UseBomb()
    {
        if (isUsable)
        {
            if (state == State.BOMB)
            {
                AfterUse();
                StartCoroutine(cBombUse());
            }
            else if (state == State.FIREEXTINGUISHER)
            {
                AfterUse();
                Debug.Log("소화기 작동");
                StartCoroutine(cFireUse());
            }
            else
            {
                AfterUse();
                Debug.Log("독 작동");
                StartCoroutine(cPoisonUse());
            }
        }
    }

    // 공통 함수
    // 폭발 후 못 쓰게 됨
    private void AfterExplosion()
    {
        // 여기에 뭐... 이미지 바뀌고 하는 거
        Destroy(gameObject);
    }
    // 못 쓰게 할 함수
    private void AfterUse()
    {
        isUsable = false;
        this.gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
    }

    // 폭발물 함수
    // 폭발 전 카운트다운
    IEnumerator cBombUse()
    {
        float timer = 0f;
        Debug.Log("함정 작동");
        Image bombIcon = Instantiate(defaultBombIcon, Vector3.zero, Quaternion.identity, GameObject.Find("UICanvas").transform);
        bombIcon.fillAmount = 1f;
        while (bombIcon.fillAmount > 0)
        {
            bombIcon.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));
            timer += Time.deltaTime;
            bombIcon.fillAmount = Mathf.Lerp(1f, 0f, timer / explosionTimer);

            float distance = Vector3.Distance(this.gameObject.transform.position, mainCamera.transform.position);
            RaycastHit hit;
            // 거리가 100 이상이거나 사이에 벽이 있으면
            if ((distance > 100f) || (Physics.Raycast(mainCamera.transform.position, (this.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer))) { bombIcon.gameObject.SetActive(false); }
            else
            {
                bombIcon.gameObject.SetActive(true);
                float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
                bombIcon.gameObject.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);
            }

            yield return null;
        }
        Destroy(bombIcon.gameObject);
        // 폭발
        fBombExplosion();
    }
    // 폭발 범위 함수
    private void fBombExplosion()
    {
        Debug.Log("폭발");
        int npcLayer = 1 << LayerMask.NameToLayer("NPC");
        int policeLayer = 1 << LayerMask.NameToLayer("POLICE");
        int wallLayer = 1 << LayerMask.NameToLayer("WALL");

        //Instantiate(ExplosionEffect, this.transform.position, Quaternion.identity);

        Collider[] targets = Physics.OverlapSphere(transform.position, 10f, (npcLayer | policeLayer));

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, wallLayer) && (distToPlayer <= 10f))
        {
            Debug.Log("플레이어 사망");
        }

        if (targets.Length == 0) { Debug.Log("아무도 없음"); }
        
        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            float distToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, wallLayer))   // 타겟과의 거리 사이에 벽 없음 (폭발 영향 확인)
            {
                if (target.CompareTag("NPC")) { target.GetComponent<NPCController>().fDead(); }
                if (target.CompareTag("Police")) { target.GetComponent<PoliceController>().fDead();}
            }
        }
        AfterExplosion();
    }

    // 소화기 함수
    IEnumerator cFireUse()
    {
        float timer = 0f;
        int npcLayer = 1 << LayerMask.NameToLayer("NPC");
        int invisiblenpcLayer = 1 << LayerMask.NameToLayer("INVISIBLENPC");
        int corpseLayer = 1 << LayerMask.NameToLayer("CORPSE");
        int invisiblecorpseLayer = 1 << LayerMask.NameToLayer("INVISIBLECORPSE");
        int wallLayer = 1 << LayerMask.NameToLayer("WALL");
        Image fireIcon = Instantiate(fireBombIcon, Vector3.zero, Quaternion.identity, GameObject.Find("UICanvas").transform);
        fireIcon.fillAmount = 1f;

        while (fireIcon.fillAmount > 0)
        {
            // 살아있는 npc부터 찾기
            Collider[] targets1 = Physics.OverlapSphere(transform.position, 12f, (npcLayer | invisiblenpcLayer));
            for (int i = 0; i < targets1.Length; i++)
            {
                GameObject target = targets1[i].gameObject;
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, wallLayer) && (distToTarget <= 10f)) { target.GetComponent<NPCController>().fGetBlinded(); }    // NPC 실명
                else { target.GetComponent<NPCController>().fOutBlinded(); }    // NPC가 실명 상태 회복
            }

            // 시신들 처리
            Collider[] targets2 = Physics.OverlapSphere(transform.position, 10f, corpseLayer);
            for (int i = 0; i < targets2.Length; i++)
            {
                GameObject target = targets2[i].gameObject;
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, wallLayer)) { target.gameObject.layer = LayerMask.NameToLayer("INVISIBLECORPSE"); }
            }

            fireIcon.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));

            float distance = Vector3.Distance(this.gameObject.transform.position, mainCamera.transform.position);
            RaycastHit hit;
            // 거리가 100 이상이거나 사이에 벽이 있으면
            if ((distance > 100f) || (Physics.Raycast(mainCamera.transform.position, (this.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer))) { fireIcon.gameObject.SetActive(false); }
            else
            {
                fireIcon.gameObject.SetActive(true);
                float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
                fireIcon.gameObject.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);
            }

            timer += Time.deltaTime;
            fireIcon.fillAmount = Mathf.Lerp(1f, 0f, timer / blindTimer);
            yield return null;
        }

        Destroy(fireIcon.gameObject);
        Collider[] blindnpcs = Physics.OverlapSphere(transform.position, 12f, invisiblenpcLayer);
        for (int i = 0; i < blindnpcs.Length; i++) { blindnpcs[i].GetComponent<NPCController>().fOutBlinded(); }    // NPC가 실명 상태 회복
        Collider[] blindcorpses = Physics.OverlapSphere(transform.position, 10f, invisiblecorpseLayer);
        for (int i = 0; i < blindcorpses.Length; i++) { blindcorpses[i].gameObject.layer = LayerMask.NameToLayer("CORPSE"); }
    }

    // 독 함수
    IEnumerator cPoisonUse()
    {
        Dictionary<GameObject, bool> poisonDictionary = new Dictionary<GameObject, bool>();
        
        float timer = 0f;
        int npcLayer = 1 << LayerMask.NameToLayer("NPC");
        int invisiblenpcLayer = 1 << LayerMask.NameToLayer("INVISIBLENPC");
        int wallLayer = 1 << LayerMask.NameToLayer("WALL");
        Image poisonIcon = Instantiate(poisonBombIcon, Vector3.zero, Quaternion.identity, GameObject.Find("UICanvas").transform);
        poisonIcon.fillAmount = 1f;

        while (poisonIcon.fillAmount > 0)
        {
            Collider[] targets1 = Physics.OverlapSphere(transform.position, 12f, (npcLayer | invisiblenpcLayer));
            for (int i = 0; i < targets1.Length; i++)
            {
                GameObject target = targets1[i].gameObject;
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, wallLayer) && (distToTarget <= 10f))    // NPC 중독
                {
                    if (!poisonDictionary.ContainsKey(target)) { poisonDictionary.Add(target, false); }
                    if (!poisonDictionary[target])
                    {
                        poisonDictionary[target] = true;
                        target.GetComponent<NPCController>().fIfInPoisoned();
                        StartCoroutine(cPoison(target.gameObject));
                    }
                }
                else    // NPC 중독 회복
                {
                    if (poisonDictionary.ContainsKey(target))
                    {
                        target.GetComponent<NPCController>().fIfOutPoisoned();
                        poisonDictionary.Remove(target);
                    }
                }
            }
            poisonIcon.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));
            float distance = Vector3.Distance(this.gameObject.transform.position, mainCamera.transform.position);
            RaycastHit hit;
            // 거리가 100 이상이거나 사이에 벽이 있으면
            if ((distance > 100f) || (Physics.Raycast(mainCamera.transform.position, (this.transform.position - mainCamera.transform.position).normalized, out hit, distance, wallLayer))) { poisonIcon.gameObject.SetActive(false); }
            else
            {
                poisonIcon.gameObject.SetActive(true);
                float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
                poisonIcon.gameObject.transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);
            }

            timer += Time.deltaTime;
            poisonIcon.fillAmount = Mathf.Lerp(1f, 0f, timer / poisonTimer);
            yield return null;
        }

        Destroy(poisonIcon.gameObject);
        Collider[] poisonednpcs = Physics.OverlapSphere(transform.position, 12f, (npcLayer | invisiblenpcLayer));
        for (int i = 0; i < poisonednpcs.Length; i++) { StopCoroutine(cPoison(poisonednpcs[i].gameObject)); }
    }
    // 독 함수 (개인)
    IEnumerator cPoison(GameObject npc)
    {
        float timer = 0f;
        float poisonedPercent = npc.GetComponent<NPCController>().fGetPoisoned();

        while (true)
        {
            if (!npc.GetComponent<NPCController>().fIfPoisoned()) yield break;
            timer += Time.deltaTime;
            float normalizedTimer = Mathf.Clamp(timer / ((1 - poisonedPercent) * poisonedTimer), 0f, 1f);
            float percent = Mathf.Lerp(poisonedPercent, 1f, normalizedTimer);
            npc.GetComponent<NPCController>().fSetPoisoned(percent);
            if (percent >= 1) yield break;
            yield return null;
        }
    }


    // 폭발 범위 함수
    private void fFireExplosion()
    {
        Debug.Log("폭발");
        

        //Instantiate(ExplosionEffect, this.transform.position, Quaternion.identity);

        
        AfterExplosion();
    }
}
