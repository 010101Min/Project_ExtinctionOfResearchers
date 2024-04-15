using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempNPCMoveController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public GameObject Appearance;

    void Start()
    {
        moveSpeed = Random.Range(0.5f, 8f);
        Appearance.transform.rotation = this.gameObject.transform.rotation;
        gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
    }
    void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        if (transform.position.x <= -15f) { Destroy(this.gameObject); }
    }
}
