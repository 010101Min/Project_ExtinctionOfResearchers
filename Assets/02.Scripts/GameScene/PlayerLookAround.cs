using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAround : MonoBehaviour
{
    public int turnSpeed;
    Rigidbody rb;

    void Start()
    {
        if (!PlayerPrefs.HasKey("Gamma"))
        {
            PlayerPrefs.SetInt("Gamma", 12);
            turnSpeed = 120;
        }
        else
        {
            turnSpeed = PlayerPrefs.GetInt("Gamma") * 10;
        }

        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        float r = Input.GetAxis("Mouse X");
        float turn = r * turnSpeed * Time.deltaTime;
        rb.rotation = rb.rotation * Quaternion.Euler(0f, turn, 0f);
    }

    public void SensitivityUp()
    {
        turnSpeed += 10;
        PlayerPrefs.SetInt("Gamma", turnSpeed/10);
    }
    public void SensitivityDown()
    {
        turnSpeed -= 10;
        if (turnSpeed <= 20) turnSpeed = 20;
        PlayerPrefs.SetInt("Gamma", turnSpeed/10);
    }
    public int getSensitivity() { return turnSpeed/10; }
}
