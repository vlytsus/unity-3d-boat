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
    public RectTransform dirIndicator;
    public RectTransform speedIndicator;

    protected Vector3 awaVector;
    protected Vector3 headSailVector;
    protected Vector3 mainSailVector;
    

    // Update is called once per frame
    void Update()
    {
        Rigidbody bootRb = boat.GetComponent<Rigidbody>();
        status.text = ""+System.Math.Round(bootRb.velocity.magnitude, 2);        
        
        Vector3 indDirection = new Vector3();
        indDirection.z = boat.transform.eulerAngles.y - wind.transform.eulerAngles.y;
        twaIndicator.localEulerAngles = indDirection;

        int apparentWindAngleGrad = (int)Vector3.Angle(boat.transform.forward, -awaVector);
        int directionAngle = (int)Vector3.Angle(boat.transform.forward, bootRb.velocity);
        
        if(indDirection.z < 180){
            apparentWindAngleGrad = 180 - apparentWindAngleGrad;
        } else {
            apparentWindAngleGrad = 180 + apparentWindAngleGrad;
            directionAngle = 360 - directionAngle;
        }
        dirIndicator.eulerAngles = new Vector3(0, 0, directionAngle);
        awaIndicator.eulerAngles = new Vector3(0, 0, apparentWindAngleGrad);
        speedIndicator.eulerAngles = new Vector3(0, 0, 120 - bootRb.velocity.magnitude*24);
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
