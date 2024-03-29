using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BombController : MonoBehaviour
{
    //public GameObject ExplosionEffect;
    public Slider CountBar;
    private bool isUsable = true;
    float explosionTimer = 10f;

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
        CountBar.value = 1f;
        while (CountBar.value > 0)
        {
            CountBar.transform.position = Camera.main.WorldToScreenPoint(new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1f, this.gameObject.transform.position.z));
            timer += Time.deltaTime;
            CountBar.value = Mathf.Lerp(1f, 0f, timer / explosionTimer);
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
        Collider[] targets = Physics.OverlapSphere(transform.position, 10f, (npcLayer | policeLayer | playerLayer));
        if (targets.Length == 0) { Debug.Log("�ƹ��� ����"); }
        
        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            float distToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, wallLayer))   // Ÿ�ٰ��� �Ÿ� ���̿� �� ���� (���� ���� Ȯ��)
            {
                if (target.CompareTag("NPC")) { target.GetComponent<NPCController>().fDead(); }
                //if (target.CompareTag("Police")) { target.GetComponent<PoliceController>().fDead(); Debug.Log("���� ���"); }
                if (target.CompareTag("Police")) { Debug.Log("���� ���"); }
                // �÷��̾��Ͻ� �÷��̾ ���
                if (target.CompareTag("Player")) { Debug.Log("�÷��̾� ���"); }
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
