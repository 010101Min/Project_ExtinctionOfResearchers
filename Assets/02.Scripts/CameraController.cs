using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    public float rotateSpeed = 10f;

    public void fCameraRotate() { StartCoroutine(CameraRotate()); }
    IEnumerator CameraRotate()
    {
        while (true)
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
