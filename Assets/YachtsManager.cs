using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YachtsManager : MonoBehaviour
{
    public BoatForces yachtPrefab;
    public GameObject windObj;

    void Start() {
        BoatForces boatForces = Instantiate(yachtPrefab, new Vector3(0, 0, 10), Quaternion.identity);
        boatForces.windObj = windObj;
    }

}
