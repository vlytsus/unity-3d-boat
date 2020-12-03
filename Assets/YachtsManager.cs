using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YachtsManager : MonoBehaviour
{
    public BoatForces yachtPrefab;
    public GameObject windObj;

    public IYachtControls yachtControls;

    void Start() {
        //BoatForces boatForces = Instantiate(yachtPrefab, new Vector3(0, 0, 10), Quaternion.identity);
        //boatForces.windObj = windObj;
    }

    void Update() {
        
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

}
