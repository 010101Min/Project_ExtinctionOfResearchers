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
                Debug.Log("��ȭ�� �۵�");
                StartCoroutine(cFireUse());
            }
            else
            {
                AfterUse();
                Debug.Log("�� �۵�");
                StartCoroutine(cPoisonUse());
            }
        }
    }

    // ���� �Լ�
    // ���� �� �� ���� ��
    private void AfterExplosion()
    {
        // ���⿡ ��... �̹��� �ٲ�� �ϴ� ��
        Destroy(gameObject);
    }
    // �� ���� �� �Լ�
    private void AfterUse()
    {
        isUsable = false;
        this.gameObject.layer = LayerMask.NameToLayer("UNINTERACTABLE");
    }

    // ���߹� �Լ�
    // ���� �� ī��Ʈ�ٿ�
    IEnumerator cBombUse()
    {
        float timer = 0f;
        Debug.Log("���� �۵�");
        CountBar.fillAmount = 1f;
        while (CountBar.fillAmount > 0)
        {
            CountBar.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));
            timer += Time.deltaTime;
            CountBar.fillAmount = Mathf.Lerp(1f, 0f, timer / explosionTimer);
            yield return null;
        }
        // ����
        fBombExplosion();
    }
    // ���� ���� �Լ�
    private void fBombExplosion()
    {
        Debug.Log("����");
        int npcLayer = 1 << LayerMask.NameToLayer("NPC");
        int policeLayer = 1 << LayerMask.NameToLayer("POLICE");
        int wallLayer = 1 << LayerMask.NameToLayer("WALL");

        //Instantiate(ExplosionEffect, this.transform.position, Quaternion.identity);

        Collider[] targets = Physics.OverlapSphere(transform.position, 10f, (npcLayer | policeLayer));

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, wallLayer) && (distToPlayer <= 10f))
        {
            Debug.Log("�÷��̾� ���");
        }

        if (targets.Length == 0) { Debug.Log("�ƹ��� ����"); }
        
        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            float distToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, wallLayer))   // Ÿ�ٰ��� �Ÿ� ���̿� �� ���� (���� ���� Ȯ��)
            {
                if (target.CompareTag("NPC")) { target.GetComponent<NPCController>().fDead(); }
                if (target.CompareTag("Police")) { target.GetComponent<PoliceController>().fDead();}
            }
        }
        AfterExplosion();
    }

    // ��ȭ�� �Լ�
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
            // ����ִ� npc���� ã��
            Collider[] targets1 = Physics.OverlapSphere(transform.position, 12f, (npcLayer | invisiblenpcLayer));
            for (int i = 0; i < targets1.Length; i++)
            {
                GameObject target = targets1[i].gameObject;
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, wallLayer) && (distToTarget <= 10f)) { target.GetComponent<NPCController>().fGetBlinded(); }    // NPC �Ǹ�
                else { target.GetComponent<NPCController>().fOutBlinded(); }    // NPC�� �Ǹ� ���� ȸ��
            }

            // �ýŵ� ó��
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
        for (int i = 0; i < blindnpcs.Length; i++) { blindnpcs[i].GetComponent<NPCController>().fOutBlinded(); }    // NPC�� �Ǹ� ���� ȸ��
        Collider[] blindcorpses = Physics.OverlapSphere(transform.position, 10f, invisiblecorpseLayer);
        for (int i = 0; i < blindcorpses.Length; i++) { blindcorpses[i].gameObject.layer = LayerMask.NameToLayer("CORPSE"); }
    }

    // �� �Լ�
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
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, wallLayer) && (distToTarget <= 10f))    // NPC �ߵ�
                {
                    if (!poisonDictionary.ContainsKey(target)) { poisonDictionary.Add(target, false); }
                    if (!poisonDictionary[target])
                    {
                        poisonDictionary[target] = true;
                        target.GetComponent<NPCController>().fIfInPoisoned();
                        StartCoroutine(cPoison(target.gameObject));
                    }
                }
                else    // NPC �ߵ� ȸ��
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
    // �� �Լ� (����)
    IEnumerator cPoison(GameObject npc)
    {
        float timer = npc.GetComponent<NPCController>().fGetPoisoned() * poisonedTimer; // ���� �ߵ� ���¿� ���� Ÿ�̸� ���۰� ����
        float poisonedPercent = npc.GetComponent<NPCController>().fGetPoisoned();
        while (poisonedPercent <= 1)
        {
            if (!npc.GetComponent<NPCController>().fIfPoisoned()) yield break;
            timer += Time.deltaTime;
            float normalizedTimer = Mathf.Clamp(timer / poisonedTimer, 0f, 1f); // Ÿ�̸� ���� 0�� 1 ���̷� ����ȭ
            float percent = Mathf.Lerp(0f, 1f, normalizedTimer);
            //npc.GetComponent<NPCController>().Poisoned.fillAmount = percent;    // ����ȭ�� Ÿ�̸� ���� ����Ͽ� fillAmount ����
            npc.GetComponent<NPCController>().fSetPoisoned(percent);
            yield return null;
        }
    }


    // ���� ���� �Լ�
    private void fFireExplosion()
    {
        Debug.Log("����");
        

        //Instantiate(ExplosionEffect, this.transform.position, Quaternion.identity);

        
        AfterExplosion();
    }
}
