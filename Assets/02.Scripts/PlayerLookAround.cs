using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAround : MonoBehaviour
{
    public float turnSpeed = 120f;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        float r = Input.GetAxis("Mouse X");
        float turn = r * turnSpeed * Time.deltaTime;
        rb.rotation = rb.rotation * Quaternion.Euler(0f, turn, 0f);
    }
}
