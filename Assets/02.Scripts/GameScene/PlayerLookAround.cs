using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAround : MonoBehaviour
{
    public int turnSpeed = 120;
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

    public void SensitivityUp() { turnSpeed += 10; }
    public void SensitivityDown() { turnSpeed -= 10; }
    public int getSensitivity() { return turnSpeed; }
}
