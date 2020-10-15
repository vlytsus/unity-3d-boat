using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatForces : MonoBehaviour
{
    public float thrust = 0.0f;
    public Rigidbody rigidbody;
    public GameObject waterArround;
    public GameObject underWaterObj;

    private Mesh underWaterMesh;
    public float underwaterSurface = 18.0f;
    public float underwaterVolume = 8.0f; 
    public float shipLenght = 8.0f; 
    
    void Start()
    {
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
        //underwaterSurface = CalculateSurfaceArea(underWaterMesh) / 2;
        //underwaterVolume = nderWaterObj.transform.localScale 
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            rigidbody.AddRelativeTorque(new Vector3(0, -100, 0), ForceMode.Impulse);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rigidbody.AddRelativeTorque(new Vector3(0, 100, 0), ForceMode.Impulse);
        }

        if (Input.GetKey(KeyCode.W))
        {
            rigidbody.AddForce(transform.forward * 10000);
        } 
        
        if (Input.GetKey(KeyCode.S))
        {
            rigidbody.AddForce(transform.forward * -10000);
        }

        waterArround.transform.position = new Vector3(transform.position.x + 12, waterArround.transform.position.y, transform.position.z + 12);
    
    }

    void FixedUpdate()
    {        
        //rigidbody.AddForce(transform.forward * thrust);

        

        rigidbody.AddForce(-rigidbody.velocity.normalized * calcualteFrictionalForce());
        rigidbody.AddForce(-rigidbody.velocity.normalized * calculateResidualForce());
    }

    float calcualteFrictionalForce(){
        // Ffr = 1/2 * rho * V * V * S * Cf
        float Cf = 0.004f;
        // S = Cws * Sqrt( Vudw * Len )
        float S = 2.6f * Mathf.Sqrt(underwaterVolume * shipLenght);
        float Ffr = 0.5f * 1000 * Mathf.Pow(rigidbody.velocity.magnitude, 2) * S * Cf;
        return Ffr;
    }

    float calculateResidualForce(){
        // Fr = 1/2 * rho * V * V * S * Cr
        float Cr = 2 * Mathf.Exp(-3);
        float Fr = 0.5f * 1000 * Mathf.Pow(rigidbody.velocity.magnitude, 2) * Cr;
        return Fr;
    }


    float CalculateSurfaceArea(Mesh mesh) {
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;

        double sum = 0.0;

        for(int i = 0; i < triangles.Length; i += 3) {
            Vector3 corner = vertices[triangles[i]];
            Vector3 a = vertices[triangles[i + 1]] - corner;
            Vector3 b = vertices[triangles[i + 2]] - corner;

            sum += Vector3.Cross(a, b).magnitude;
        }

        return (float)(sum/2.0);
    }

}
