using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 120f;
    Transform tr;

    void Start()
    {
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float r = Input.GetAxis("Mouse X");

        Vector3 moveDirection = new Vector3(x, 0f, z);
        tr.Translate(moveDirection * moveSpeed * Time.deltaTime);
        tr.Rotate(Vector3.up * turnSpeed * Time.deltaTime * r);

        if (Input.GetKeyDown(KeyCode.E)) { killNpc(findNpc()); }
        if (Input.GetKeyDown(KeyCode.Q)) { provokeNpc(findNpc()); }
    }

    GameObject findNpc()
    {
        int layer = 1 << LayerMask.NameToLayer("NPC");
        float minDistance = float.MaxValue;
        Collider[] colls = new Collider[13];
        List<GameObject> targets = new List<GameObject>();
        GameObject target = null;

        // �ൿ ���� ���� ���� npc �ִ��� Ȯ��
        int count = Physics.OverlapSphereNonAlloc(this.transform.position, 2f, colls, 1 << 9);
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = (colls[i].transform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, dir) < 60f)    // ���� �¿� 60���� ���� ���� ����
                {
                    float distToTarget = Vector3.Distance(transform.position, colls[i].transform.position);
                    if (!Physics.Raycast(transform.position, dir, distToTarget, ~layer))   // ���� ���� �÷��̾� ���̿� �� ����
                    {
                        targets.Add(colls[i].gameObject);
                    }
                }
            }
        }

        // �ൿ ���� ���� ���� �ִ� npc�� �� ���� ����� �� ã��
        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                float distance = Vector3.Distance(transform.position, targets[i].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    target = targets[i];
                }
            }
            return target;
        }
        return null;
    }

    void killNpc(GameObject target)
    {
        if (target != null) { target.GetComponent<NPCController>().fDead(); }
        else { Debug.Log("Ÿ�� ����"); }
    }

    void provokeNpc(GameObject target)
    {
        int rand = Random.Range(1, 101);
        if (target != null) { target.GetComponent<NPCController>().CheckProvoked(rand); }
        else { Debug.Log("Ÿ�� ����"); }
    }
}
