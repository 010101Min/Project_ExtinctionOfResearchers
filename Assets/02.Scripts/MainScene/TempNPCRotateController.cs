using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempNPCRotateController : MonoBehaviour
{
    public float rotationSpeed = 50f;
    private Vector3 randomDirection;

    void Start()
    {
        rotationSpeed = Random.Range(30f, 180f);
        randomDirection = Random.insideUnitSphere.normalized;
    }

    void Update()
    {
        float rotationAngle = rotationSpeed * Time.deltaTime;
        transform.Rotate(randomDirection, rotationAngle);
    }
}