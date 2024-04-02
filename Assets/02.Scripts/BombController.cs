using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class BombController : MonoBehaviour
{
    //public GameObject ExplosionEffect;
    public Image CountBar;
    private GameObject player;
    private bool isUsable = true;
    float explosionTimer = 10f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void UseBomb()
    {
        if (isUsable)
        {
            isUsable = false;
            StartCoroutine(cUse());
        }
    }

    // 폭발 전 카운트다운
    IEnumerator cUse()
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
        fExplosion();
    }

    // 폭발 범위 함수
    private void fExplosion()
    {
        Debug.Log("폭발");
        int npcLayer = 1 << LayerMask.NameToLayer("NPC");
        int policeLayer = 1 << LayerMask.NameToLayer("POLICE");
        int playerLayer = 1 << LayerMask.NameToLayer("PLAYER");
        int wallLayer = 1 << LayerMask.NameToLayer("WALL");

        //Instantiate(ExplosionEffect, this.transform.position, Quaternion.identity);

        Collider[] targets = Physics.OverlapSphere(transform.position, 10f, (npcLayer | policeLayer));

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, wallLayer))
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

    // 폭발 후 못 쓰게 됨
    private void AfterExplosion()
    {
        // 여기에 뭐... 이미지 바뀌고 하는 거
        Destroy(gameObject);
    }
}
