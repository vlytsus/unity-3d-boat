using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IYachtPhysics : MonoBehaviour {
    
    public abstract float[] prepareHeadSailDragCoefficients();

    public abstract float[] prepareHeadSailLiftCoefficients();
    public abstract float[] prepareMainSailDragCoefficients();

    public abstract float[] prepareMainSailLiftCoefficients();

    public abstract float getMainSailAreaM2();
    public abstract float getHeadSailAreaM2();
    public abstract float getUnderwaterSurface();
    public abstract float getUnderwaterVolume();
    public abstract float getShipLenght();

}
