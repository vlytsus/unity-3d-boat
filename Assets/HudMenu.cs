using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudMenu : MonoBehaviour
{
    public GameObject boat;
    public GameObject wind;
    public Text status;
    public RectTransform twaIndicator;
    public RectTransform awaIndicator;

    protected Vector3 awaVector;
    protected Vector3 headSailVector;
    protected Vector3 mainSailVector;
    

    // Update is called once per frame
    void Update()
    {
        Rigidbody bootRb = boat.GetComponent<Rigidbody>();
        status.text = "Speed: " + System.Math.Round(bootRb.velocity.magnitude, 2);
        
        Vector3 indDirection = new Vector3();
        indDirection.z = boat.transform.eulerAngles.y - wind.transform.eulerAngles.y;
        twaIndicator.localEulerAngles = indDirection;

        int apparentWindAngleGrad = (int)Vector3.Angle(boat.transform.forward, -awaVector);
        if(indDirection.z < 180){
            apparentWindAngleGrad = 180 - apparentWindAngleGrad;
        } else {
            apparentWindAngleGrad = 180 + apparentWindAngleGrad;
        }
        awaIndicator.eulerAngles = new Vector3(0, 0, apparentWindAngleGrad);
    }

    public void onAwaAngleChange(Vector3 vector) {
        awaVector = vector;
    }

    public void onHeadSailAngleChange(Vector3 vector) {
        headSailVector = vector;
    }

    public void onMainSailAngleChange(Vector3 vector) {
        mainSailVector = vector;
    }
}
