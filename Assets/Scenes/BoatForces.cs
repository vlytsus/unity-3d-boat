using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatForces : MonoBehaviour
{
    public float thrust = 0.0f;
    public float windAngle = 0.0f;
    public float angleOfAttack = 0.0f;
    public Rigidbody boatRigidbody;
    public GameObject rudder;
    public GameObject waterArround;
    public GameObject underWaterObj;
    public GameObject windObj;

    public Vector3 directionVector;
    public Vector3 transformVector;

    private Mesh underWaterMesh;
    public float underwaterSurface = 18.0f;
    public float underwaterVolume = 8.0f; 
    public float shipLenght = 8.0f; 

    public float[] angleToForce;
    
    void Start()
    {
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
        //underwaterSurface = CalculateSurfaceArea(underWaterMesh) / 2;
        //underwaterVolume = nderWaterObj.transform.localScale 
        initAngleForces();
        
    } 

    void initAngleForces(){
        angleToForce = new float[100];
        for(int i= 10; i < 100; i++) {
            float force = calcForce4(i);
            Debug.Log("Result for " + i + "=" + force);
            angleToForce[i] = force;
        }
    }

    // Calculate forces using Excel calculated polynomial trend
    // training data
    // angle: 10,  20,  30,  40,  50, 60, 70, 80, 90, 100
    // force: 16, 130, 144, 145, 124, 96, 72, 50, 28, 14
    // Polynome: -2.36451E-05*X4 + 0.006606935*X3 - 0.659066142*X2 + 25.33863636*X - 173.9166667
    float calcForce4(float x){
        double y =
            -0.236451049  * (double) Mathf.Pow(x, 4) / Mathf.Pow(10, 4)
            + 6.606934732 * (double) Mathf.Pow(x, 3) / Mathf.Pow(10, 3)
            -65.90661422  * (double) Mathf.Pow(x, 2) / Mathf.Pow(10, 2)
            + 253.3863636 * (double) Mathf.Pow(x, 1) / Mathf.Pow(10, 1)
            -173.9166667;
        return (float)y;
    }

    float getForceAtAngle(int angle)
    {
        float force = 0;
        if(angle < 10){
            force = 0;
        } else if(angle > 90 && angle < 270 ){
            force = angleToForce[99];
        } else if(angle >= 270 ){
            force = angleToForce[360 - angle];         
        } else {
            force = angleToForce[angle];
        } 

        Debug.Log("Result angle = " + angle + " force = " + force);
        return force;
    }   

    void Update()
    {
        //boatRigidbody.AddForceAtPosition(transform.right * 400, rudder.transform.position, ForceMode.Force);
        //boatRigidbody.AddForce(transform.forward * 10000);

        if (Input.GetKey(KeyCode.A))
        {
            //boatRigidbody.AddRelativeTorque(new Vector3(0, -100, 0), ForceMode.Impulse);
            boatRigidbody.AddForceAtPosition(transform.right * 3000, rudder.transform.position, ForceMode.Force);
        }

        if (Input.GetKey(KeyCode.D))
        {
            //boatRigidbody.AddRelativeTorque(new Vector3(0, 100, 0), ForceMode.Impulse);
            boatRigidbody.AddForceAtPosition(-transform.right * 3000, rudder.transform.position, ForceMode.Force);
        }

        if (Input.GetKey(KeyCode.W))
        {
            boatRigidbody.AddForce(transform.forward * 10000);
        } 
        
        if (Input.GetKey(KeyCode.S))
        {
            boatRigidbody.AddForce(transform.forward * -10000);
        }

        waterArround.transform.position = new Vector3(transform.position.x + 12, waterArround.transform.position.y, transform.position.z + 12);
    
    }

    void FixedUpdate()
    {        
        boatRigidbody.AddForce(transform.forward * getForceAtAngle((int)transform.eulerAngles.y) * 10);
        boatRigidbody.AddForce(-boatRigidbody.velocity.normalized * calcualteFrictionalForce());
        boatRigidbody.AddForce(-boatRigidbody.velocity.normalized * calculateResidualForce());
        
        var sideSpeed = Vector3.Dot(boatRigidbody.velocity, transform.right);
        var vsVector = Vector3.forward;
        if(sideSpeed > 0.0f){
            vsVector = -transform.right * sideSpeed * sideSpeed;
        } else {
            vsVector = transform.right * sideSpeed * sideSpeed;
        }
        
        //Debug.Log("sideSpeed=" + sideSpeed);
        directionVector = boatRigidbody.velocity.normalized;
        boatRigidbody.AddForce(vsVector * 800);

        Debug.DrawRay(transform.position + boatRigidbody.centerOfMass, windObj.transform.forward * 100, Color.green, 0.0f, false);
        Debug.DrawRay(transform.position + boatRigidbody.centerOfMass, vsVector * 10, Color.green, 0.0f, false);
        Debug.DrawRay(transform.position + boatRigidbody.centerOfMass, boatRigidbody.velocity * 10, Color.red, 0.0f, false);
    }

    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
		Vector3 perp = Vector3.Cross(fwd, targetDir);
		float dir = Vector3.Dot(perp, up);
		
		if (dir > 0f) {
			return 1f;
		} else if (dir < 0f) {
			return -1f;
		} else {
			return 0f;
		}
	}

    float calcualteFrictionalForce(){
        // Ffr = 1/2 * rho * V * V * S * Cf
        float Cf = 0.004f;
        // S = Cws * Sqrt( Vudw * Len )
        float S = 2.6f * Mathf.Sqrt(underwaterVolume * shipLenght);
        float Ffr = 0.5f * 1000 * Mathf.Pow(boatRigidbody.velocity.magnitude, 2) * S * Cf;
        return Ffr;
    }

    float calculateResidualForce(){
        // Fr = 1/2 * rho * V * V * S * Cr
        float Cr = 2 * Mathf.Exp(-3);
        float Fr = 0.5f * 1000 * Mathf.Pow(boatRigidbody.velocity.magnitude, 2) * Cr;
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
