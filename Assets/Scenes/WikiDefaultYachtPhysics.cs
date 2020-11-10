using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WikiDefaultYachtPhysics : IYachtPhysics {

    const float mainSailAreaM2 = 20.0f; // Width * Height / 2
    const float headSailAreaM2 = 15.0f; // Width * Height / 2
    const float underwaterSurface = 18.5f; //also known as Wetted surface
    const float underwaterVolume = 3.3f; //also known as Displacement
    const float shipLenght = 8.3f; // Water Line Length

    public override float getMainSailAreaM2() {
        return mainSailAreaM2;
    }
    public override float getHeadSailAreaM2() {
        return headSailAreaM2;
    }
    public override float getUnderwaterSurface() {
        return underwaterSurface;
    }
    public override float getUnderwaterVolume() {
        return underwaterVolume;
    }
    public override float getShipLenght() {
        return shipLenght;
    }

    public override float[] prepareHeadSailDragCoefficients() {
        return prepareMainSailDragCoefficients();
    }

    public override float[] prepareHeadSailLiftCoefficients() {
        return prepareMainSailLiftCoefficients();
    }

    public override float[] prepareMainSailDragCoefficients() {
        float[] coeficientAtAngle = new float[100];
        for(int i= 10; i < 100; i++) {
            coeficientAtAngle[i] = calcDragCoeficient4(i);
            //Debug.Log("Drag result for " + i + " lift=" + liftCoeficient + " drag=" + dragCoeficient);
        }
        return coeficientAtAngle;
    }
    public override float[] prepareMainSailLiftCoefficients() {
        float[] coeficientAtAngle = new float[100];
        for(int i= 10; i < 100; i++) {
            coeficientAtAngle[i] = calcLiftCoeficient4(i);
            //Debug.Log("Drag result for " + i + " lift=" + liftCoeficient + " drag=" + dragCoeficient);
        }
        return coeficientAtAngle;
    }

    // Calculate forces using Excel calculated polynomial trend
    // training data
    // angle:        10, 20, 30,   40,   50,   60,  70,  80,  90,  100
    // coefficient: 1.6, 13, 14.4, 14.5, 12.4, 9.6, 7.2, 5.0, 2.8, 1.4
    // Polynome: -2.36451E-05*X4 + 0.006606935*X3 - 0.659066142*X2 + 25.33863636*X - 173.9166667
    float calcLiftCoeficient4(float x) {
        float y =
            -0.00236451049f  * Mathf.Pow(x, 4) / Mathf.Pow(10, 4)
            + 0.06606934732f * Mathf.Pow(x, 3) / Mathf.Pow(10, 3)
            -0.6590661422f  * Mathf.Pow(x, 2) / Mathf.Pow(10, 2)
            + 2.533863636f * Mathf.Pow(x, 1) / Mathf.Pow(10, 1)
            -1.7391666f;
        return y;
    }

    // Calculate forces using Excel calculated polynomial trend
    // training data
    // angle:         10,    20,   30,  40,  50,   60,  70,  80,   90,  100
    // coefficient: 0.15, 0.174, 0.22, 0.3, 0.4, 0.55, 0.7, 0.94, 1.28, 1.6
    // Polynome: 1.16861E-06*X3 + 1.35431E-05*X2 + 0.001794328*X + 0.127066667
    float calcDragCoeficient4(float x) {
        float y =
            + 0.001168609f * Mathf.Pow(x, 3) / Mathf.Pow(10, 3)
            + 0.013543124f * Mathf.Pow(x, 2) / Mathf.Pow(10, 2)
            + 0.017943279f * Mathf.Pow(x, 1) / Mathf.Pow(10, 1)
            + 0.127066667f;
        return y;
    }
}
