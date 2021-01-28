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

    private int minimalDistance = 5;
    
    void Start() {
        secondYacht = cloneBoat(yachtPrefab, "SecondYacht");
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

        moveWaterAreaArroundShip();
    }

    BoatForces cloneBoat(BoatForces sourceBoat, string name){
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().enabled = false;
        BoatForces newBoat = Instantiate(sourceBoat, new Vector3(0, 0, 10), Quaternion.identity);        
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().enabled = true;
        newBoat.windObj = windObj;
        newBoat.tag = name;
        return newBoat; 
    }

    private void followTarget() {
        Vector3 yachtApparentWind = yachtControls.getApparentWindVector();
        
        /*
        int windToBoatAngle = (int)Vector3.Angle(yachtPrefab.transform.forward, yachtApparentWind);
        if(windToBoatAngle < -40){
            limits.min += windToBoatAngle;
            limits.max = limits.min + 30;
        } else if (windToBoatAngle > 40) {
            limits.max += windToBoatAngle;
            limits.min = limits.max - 30;
        }*/

        float angle = calcAngleToWaypoint();
        if(angle > 10){
            yachtControls.rotateRudder(1);
        } else if(angle < -10){
            yachtControls.rotateRudder(-1);
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
        if(distance < minimalDistance ){
            goNextWaypoint();
        }
        return target[waypoint].transform.position - yachtPrefab.transform.position;
    }

    private void goNextWaypoint(){
        if(waypoint < target.Length-1){
            waypoint++;
        } else {
            waypoint = 0;
        }
    }

    private void moveWaterAreaArroundShip() {
        if(waterArround != null) {
            waterArround.transform.position = new Vector3(yachtPrefab.transform.position.x + 12, waterArround.transform.position.y, yachtPrefab.transform.position.z + 12);
        }
    }
}
