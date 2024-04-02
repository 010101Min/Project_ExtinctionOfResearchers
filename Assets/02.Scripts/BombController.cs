using System;
using System.Collections;
using System.Collections.Generic;
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
            }
        }
    }

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
            CountBar.fillAmount = Mathf.Lerp(1f, 0f, timer / 30f);
            yield return null;
        }

        Collider[] blindnpcs = Physics.OverlapSphere(transform.position, 12f, invisiblenpcLayer);
        for (int i = 0; i < blindnpcs.Length; i++) { blindnpcs[i].GetComponent<NPCController>().fOutBlinded(); }    // NPC가 실명 상태 회복
        Collider[] blindcorpses = Physics.OverlapSphere(transform.position, 10f, invisiblecorpseLayer);
        for (int i = 0; i < blindcorpses.Length; i++) { blindcorpses[i].gameObject.layer = LayerMask.NameToLayer("CORPSE"); }
    }

    // 폭발 범위 함수
    private void fFireExplosion()
    {
        Debug.Log("폭발");
        

        //Instantiate(ExplosionEffect, this.transform.position, Quaternion.identity);

        
        AfterExplosion();
    }
}
