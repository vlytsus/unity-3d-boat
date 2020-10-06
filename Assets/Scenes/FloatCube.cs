using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatCube : MonoBehaviour
{
    public Rigidbody rigidbody;
    public float depthBeforeSubmerged = 1f;
    public float displacementAmount = 3f;
    public float waweHeight = 0f;

    void FixedUpdate()
    {
        waweHeight = WaweManager.GetWaweHeight(transform.position.x);
        if(transform.position.y < waweHeight)
        {
            float displacementMultiplier = Mathf.Clamp01((waweHeight - transform.position.y) / depthBeforeSubmerged ) * displacementAmount;
            rigidbody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration);
        }

    }

}
