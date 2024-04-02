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

    // ���� �� ī��Ʈ�ٿ�
    IEnumerator cUse()
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
        fExplosion();
    }

    // ���� ���� �Լ�
    private void fExplosion()
    {
        Debug.Log("����");
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

    // ���� �� �� ���� ��
    private void AfterExplosion()
    {
        // ���⿡ ��... �̹��� �ٲ�� �ϴ� ��
        Destroy(gameObject);
    }
}
