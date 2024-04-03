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
    public Image CountBar;
    private GameObject player;
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
        CountBar.fillAmount = 1f;
        while (CountBar.fillAmount > 0)
        {
            CountBar.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));
            timer += Time.deltaTime;
            CountBar.fillAmount = Mathf.Lerp(1f, 0f, timer / explosionTimer);
            yield return null;
        }
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
        CountBar.fillAmount = 1f;

        while (CountBar.fillAmount > 0)
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

            CountBar.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));
            timer += Time.deltaTime;
            CountBar.fillAmount = Mathf.Lerp(1f, 0f, timer / blindTimer);
            yield return null;
        }

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
        CountBar.fillAmount = 1f;

        while (CountBar.fillAmount > 0)
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
            CountBar.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));
            timer += Time.deltaTime;
            CountBar.fillAmount = Mathf.Lerp(1f, 0f, timer / poisonTimer);
            yield return null;
        }

        Collider[] poisonednpcs = Physics.OverlapSphere(transform.position, 12f, (npcLayer | invisiblenpcLayer));
        for (int i = 0; i < poisonednpcs.Length; i++) { StopCoroutine(cPoison(poisonednpcs[i].gameObject)); }
    }
    // 독 함수 (개인)
    IEnumerator cPoison(GameObject npc)
    {
        float timer = npc.GetComponent<NPCController>().fGetPoisoned() * poisonedTimer; // 이전 중독 상태에 따라 타이머 시작값 설정
        float poisonedPercent = npc.GetComponent<NPCController>().fGetPoisoned();
        while (poisonedPercent <= 1)
        {
            if (!npc.GetComponent<NPCController>().fIfPoisoned()) yield break;
            timer += Time.deltaTime;
            float normalizedTimer = Mathf.Clamp(timer / poisonedTimer, 0f, 1f); // 타이머 값을 0과 1 사이로 정규화
            float percent = Mathf.Lerp(0f, 1f, normalizedTimer);
            //npc.GetComponent<NPCController>().Poisoned.fillAmount = percent;    // 정규화된 타이머 값을 사용하여 fillAmount 설정
            npc.GetComponent<NPCController>().fSetPoisoned(percent);
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
