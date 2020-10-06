using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatForces : MonoBehaviour
{
    public float thrust = 1.0f;
    public Rigidbody rigidbody;
    public GameObject whaterArround;
    
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            rigidbody.AddRelativeTorque(new Vector3(0, -0.04f, 0), ForceMode.Impulse);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rigidbody.AddRelativeTorque(new Vector3(0, 0.04f, 0), ForceMode.Impulse);
        }

        if (Input.GetKey(KeyCode.W))
        {
            rigidbody.AddForce(transform.forward * thrust * 5);
        } 
        
        if (Input.GetKey(KeyCode.S))
        {
            rigidbody.AddForce(transform.forward * thrust * -5);
        }

        whaterArround.transform.position = new Vector3(transform.position.x + 12, whaterArround.transform.position.y, transform.position.z + 12);
    
    }

    void FixedUpdate()
    {        
        rigidbody.AddForce(transform.forward * thrust);
    }

}
