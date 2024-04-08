using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAround : MonoBehaviour
{
    public float turnSpeed = 120f;
    Transform tr;

    void Start()
    {
        tr = GetComponent<Transform>();
    }
    
    void Update()
    {
        float r = Input.GetAxis("Mouse X");
        tr.Rotate(Vector3.up * turnSpeed * Time.deltaTime * r);
    }
}
