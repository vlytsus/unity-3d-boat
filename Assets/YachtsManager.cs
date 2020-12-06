using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YachtsManager : MonoBehaviour
{
    public BoatForces yachtPrefab;
    public GameObject windObj;
    public GameObject waterArround;
    public IYachtControls yachtControls;
    public BoatForces secondYacht;

    public BoatForces anotherYacht;
    public GameObject mainYacht;
    public GameObject[] target;

    private int waypoint = 0;
    

    void Start() {
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().enabled = false;
        secondYacht = Instantiate(yachtPrefab, new Vector3(0, 0, 10), Quaternion.identity);
        secondYacht.tag = "SecondYacht";
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().enabled = true;
        secondYacht.windObj = windObj;
    }

    void followTarget(){        
        float angle = calcAngleToWaypoint();
        if(angle > 10){
            yachtControls.rotateRudder(1);
        } else if(angle < -10){
            yachtControls.rotateRudder(-1);
        }        
    }

    void Update() {

        followTarget();
        
        if (Input.GetKey(KeyCode.A)){
            yachtControls.rotateRudder(1);
        }

        if (Input.GetKey(KeyCode.D)) {
            yachtControls.rotateRudder(-1);
        }

        if (Input.GetKey(KeyCode.Q)) {
            yachtControls.rotateHeadSail(1);
        }

        if (Input.GetKey(KeyCode.E)) {
            yachtControls.rotateHeadSail(-1);
        }

        if (Input.GetKey(KeyCode.Z)) {
            yachtControls.rotateMainSail(1);
        }

        if (Input.GetKey(KeyCode.C)) {
            yachtControls.rotateMainSail(-1);
        }
    }

    private int calcAngleToWaypoint() { 
        Vector3 targetDir = targetDirection();
        int angle = (int)Vector3.SignedAngle(targetDir, yachtPrefab.transform.forward, Vector3.up);
        //Debug.Log("angle = " + angle);
        return angle;
    }
    private Vector3 targetDirection() {
        float distance = Vector3.Distance (target[waypoint].transform.position, yachtPrefab.transform.position);
        if(distance < 5 ){
            if(waypoint < target.Length-1){
                waypoint++;
            } else {
               waypoint = 0;
            }
        }
        return target[waypoint].transform.position - yachtPrefab.transform.position;
    }
}
