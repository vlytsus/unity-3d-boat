using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IYachtConfigFactory : MonoBehaviour {
    public abstract float[] prepareSailDragCoefficients();

    public abstract float[] prepareSailLiftCoefficients();

    public abstract float getMainSailAreaM2();
    public abstract float getHeadSailAreaM2();
    public abstract float getUnderwaterSurface();
    public abstract float getUnderwaterVolume();
    public abstract float getShipLenght();

}
