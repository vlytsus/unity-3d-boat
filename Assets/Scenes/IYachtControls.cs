using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IYachtControls : MonoBehaviour {
    

    public abstract void rotateHeadSail(int angle);
    public abstract void rotateMainSail(int angle);
    public abstract void rotateRudder(int angle);  

}
