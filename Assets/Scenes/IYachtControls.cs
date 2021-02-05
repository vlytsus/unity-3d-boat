using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IYachtControls : MonoBehaviour {
    
    public abstract void rotateHeadSail(int angle);
    public abstract void rotateMainSail(int angle);
    public abstract void rotateRudder(int angle);
    public abstract float getHeadSailLiftCoeficientAtAngle(int angle);
    public abstract float getMainSailLiftCoeficientAtAngle(int angle);
    public abstract float getHeadSailDragCoeficientAtAngle(int angle);
    public abstract float getMainSailDragCoeficientAtAngle(int angle);
    public abstract Vector3 getVelocity();

}
