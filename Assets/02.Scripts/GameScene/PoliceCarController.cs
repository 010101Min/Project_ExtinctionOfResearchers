using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class PoliceCarController : MonoBehaviour
{
    public float speed = 300f; // 이동 속도
    private GameObject tempPos;
    private GameObject startPos;
    private GameObject parkPos;
    private GameObject endPos;
    private GameObject policePos;
    public GameObject PolicePrefab;
    private GameObject police;

    private GameObject reporter;
    private GameObject corpse;
    private GameObject suspect;
    private int time;
    private bool isLeaving = false;

    void Start()
    {
        tempPos = GameObject.Find("Pos");
        startPos = tempPos.transform.Find("PolicePos/startPos").gameObject;
        parkPos = tempPos.transform.Find("PolicePos/parkPos").gameObject;
        endPos = tempPos.transform.Find("PolicePos/endPos").gameObject;
        this.gameObject.transform.position = startPos.transform.position;
        policePos = this.gameObject.transform.Find("PolicePos").gameObject;
        StartCoroutine(ComeCar());
    }

    

    public void GetReport(GameObject tempReporter, GameObject tempCorpse, GameObject tempSuspect, int tempTime)
    {
        this.reporter = tempReporter;
        this.corpse = tempCorpse;
        this.suspect = tempSuspect;
        this.time = tempTime;
    }

    public void leaveSign() { StartCoroutine(LeaveCar()); }
    public bool getLeave() { return isLeaving; }

    IEnumerator ComeCar()
    {
        while (Vector3.Distance(transform.position, parkPos.transform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, parkPos.transform.position, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = parkPos.transform.position;
        police = Instantiate(PolicePrefab, policePos.transform.position, Quaternion.identity);
        police.GetComponent<PoliceController>().Report(reporter, corpse, suspect, time);
    }

    IEnumerator LeaveCar()
    {
        isLeaving = true;
        while (Vector3.Distance(transform.position, endPos.transform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos.transform.position, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = endPos.transform.position;
        Destroy(this.gameObject);
    }
}
