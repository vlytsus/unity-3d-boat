using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudMenu : MonoBehaviour
{
    public GameObject boat;
    public Text status;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody bootRb = boat.GetComponent<Rigidbody>();
        status.text = "Speed: " + System.Math.Round(bootRb.velocity.magnitude, 2);
    }
}
