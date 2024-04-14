using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbulanceController : MonoBehaviour
{
    public float speed = 40f; // 이동 속도
    private GameObject tempPos;
    private GameObject startPos;
    private GameObject parkPos;
    private GameObject endPos;
    public GameObject paramedicPos;

    public GameObject Paramedic;

    private bool isArrive = false;
    private bool isLeaving = false;

    public List<GameObject> Corpses = new List<GameObject>();

    void Start()
    {
        isArrive = false;
        isLeaving = false;
        tempPos = GameObject.Find("Pos");
        startPos = tempPos.transform.Find("AmbulancePos/startPos").gameObject;
        parkPos = tempPos.transform.Find("AmbulancePos/parkPos").gameObject;
        endPos = tempPos.transform.Find("AmbulancePos/endPos").gameObject;
        this.gameObject.transform.position = startPos.transform.position;
        Paramedic.transform.position = startPos.transform.position;
    }

    public void Report(List<GameObject> Corps)
    {
        if (!isArrive) { StartCoroutine(WaitCar()); }
        Paramedic.GetComponent<ParamedicController>().Report(Corps);
    }

    public void leaveSign() { StartCoroutine(LeaveCar()); }

    IEnumerator WaitCar()
    {
        while (true)
        {
            if (!isLeaving) break;
            yield return null;
        }
        StartCoroutine(ComeCar());
    }

    IEnumerator ComeCar()
    {
        isLeaving = false;
        while (Vector3.Distance(transform.position, parkPos.transform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, parkPos.transform.position, speed * Time.deltaTime);
            yield return null;
        }
        isArrive = true;
        transform.position = parkPos.transform.position;
        Paramedic.transform.position = paramedicPos.transform.position;
        Paramedic.GetComponent<ParamedicController>().StartResolve();
    }

    IEnumerator LeaveCar()
    {
        isArrive = false;
        isLeaving = true;
        while (Vector3.Distance(transform.position, endPos.transform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos.transform.position, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = startPos.transform.position;
        isLeaving = false;
    }
}
