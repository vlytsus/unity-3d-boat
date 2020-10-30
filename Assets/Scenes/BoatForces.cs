using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatForces : MonoBehaviour
{
    public Rigidbody boatRigidbody;
    public Rigidbody keelRigidbody;
    public GameObject rudder;
    public GameObject waterArround;
    public GameObject underWaterObj;
    public GameObject windObj;
    public GameObject headSail;
    public GameObject mainSail;

    private Mesh underWaterMesh;

    public float[] angleToLiftCoeficient;
    public float[] angleToDragCoeficient;

    public float windSpeed = 6;

    private static float waterRho = 1030.0f; //salt water density

    // TODO Experimental constants goes here
    const float mainSailAreaM2 = 20.0f; // Width * Height / 2
    const float headSailAreaM2 = 15.0f; // Width * Height / 2
    const float underwaterSurface = 18.5f; //also known as Wetted surface
    const float underwaterVolume = 3.3f; //also known as Displacement
    const float shipLenght = 8.3f; // Water Line Length
    const float liftCoeficientExperimentalFix = 800f;
    const float dragCoeficientExperimentalFix = 800f;

    const int maxSailAngle = 70;
    
    void Start() {
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
        //underwaterSurface = CalculateSurfaceArea(underWaterMesh) / 2;
        //underwaterVolume = nderWaterObj.transform.localScale 
        initAngleForces();  
    }

    void Update() {
        //boatRigidbody.AddForceAtPosition(transform.right * 400, rudder.transform.position, ForceMode.Force);
        //boatRigidbody.AddForce(transform.forward * 10000);

        if (Input.GetKey(KeyCode.A)){
            //boatRigidbody.AddRelativeTorque(new Vector3(0, -100, 0), ForceMode.Impulse);
            boatRigidbody.AddForceAtPosition(transform.right * 3000, rudder.transform.position, ForceMode.Force);
        }

        if (Input.GetKey(KeyCode.D)) {
            //boatRigidbody.AddRelativeTorque(new Vector3(0, 100, 0), ForceMode.Impulse);
            boatRigidbody.AddForceAtPosition(-transform.right * 3000, rudder.transform.position, ForceMode.Force);
        }

        if (Input.GetKey(KeyCode.Q)) {
            rotateSail(headSail, 1);
        }

        if (Input.GetKey(KeyCode.E)) {
            rotateSail(headSail, -1);
        }

        if (Input.GetKey(KeyCode.Z)) {
            rotateSail(mainSail, 1);
        }

        if (Input.GetKey(KeyCode.C)) {
            rotateSail(mainSail, -1);
        }

        if (Input.GetKey(KeyCode.W)) {
            boatRigidbody.AddForce(transform.forward * 10000);
        } 
        
        if (Input.GetKey(KeyCode.S)) {
            boatRigidbody.AddForce(transform.forward * -10000);
        }

        moveWaterAreaArroundShip();    
    }

    void FixedUpdate() {
        addLiftForceToSail(headSail, headSailAreaM2);
        addDragForceToSail(headSail, headSailAreaM2);
        addLiftForceToSail(mainSail, mainSailAreaM2);
        addDragForceToSail(mainSail, mainSailAreaM2);

        boatRigidbody.AddForce(-boatRigidbody.velocity.normalized * calcualteFrictionalForce());
        boatRigidbody.AddForce(-boatRigidbody.velocity.normalized * calculateResidualForce());

        addHullDragForce();        
        addForceToKeel();

        //Debug.DrawRay(transform.position + boatRigidbody.centerOfMass, windObj.transform.forward * 100, Color.green, 0.0f, false);
        //Debug.DrawRay(transform.position + boatRigidbody.centerOfMass, vsVector * 10, Color.green, 0.0f, false);
        Debug.DrawRay(transform.position + boatRigidbody.centerOfMass, boatRigidbody.velocity * 10, Color.green, 0.0f, false);
    }

    void rotateSail(GameObject sail, int angle) {
        HingeJoint hinge = sail.GetComponent<HingeJoint>();
        JointSpring hingeSpring = hinge.spring;
        hingeSpring.targetPosition += angle;
        if(Mathf.Abs(hingeSpring.targetPosition) < maxSailAngle){
            hinge.spring = hingeSpring;
        }
    }

    void moveWaterAreaArroundShip() {
        waterArround.transform.position = new Vector3(transform.position.x + 12, waterArround.transform.position.y, transform.position.z + 12);
    }

    void addLiftForceToSail(GameObject sail, float sailAreaM2) {           
        Vector3 trueWind = windObj.transform.forward * windSpeed;
        Vector3 apparentWind = calculateApparentWindVector(trueWind, boatRigidbody.velocity);
        Vector3 sailVector = sail.transform.forward;

        int apparentWindAngleGrad = (int)Vector3.Angle(sailVector, -apparentWind);
        float liftCoeficient = getCoeficientAtAngle(angleToLiftCoeficient, apparentWindAngleGrad);
        float windVelocity = apparentWind.magnitude;

        Vector3 liftForceDirection = calculateLiftDirection(apparentWind, sailVector);       
        Vector3 liftForce = liftForceDirection * calculateSailForce(liftCoeficient, windVelocity, sailAreaM2);
        
        Rigidbody sailRb = sail.GetComponent<Rigidbody>();       
        sailRb.AddForce(liftForce);
        Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, liftForce / 10, Color.blue, 0.0f, false);
    }

    void addDragForceToSail(GameObject sail, float sailAreaM2) {  
        
        Vector3 trueWind = windObj.transform.forward * windSpeed;
        Vector3 apparentWind = calculateApparentWindVector(trueWind, boatRigidbody.velocity);
        Vector3 sailVector = sail.transform.forward;

        int apparentWindAngleGrad = (int)Vector3.Angle(sailVector, -apparentWind);
        float dragCoeficient = getCoeficientAtAngle(angleToDragCoeficient, apparentWindAngleGrad);
        float windVelocity = apparentWind.magnitude;
        
        Vector3 dragForce = apparentWind.normalized * calculateSailForce(dragCoeficient, windVelocity, sailAreaM2);

        Rigidbody sailRb = sail.GetComponent<Rigidbody>();
        sailRb.AddForce(dragForce);
        Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, dragForce / 10, Color.red, 0.0f, false);
    }

    Vector3 calculateLiftDirection(Vector3 apparentWind, Vector3 sailVector){
        float liftAngle = Vector3.SignedAngle(-apparentWind.normalized, sailVector.normalized, Vector3.up);
        if(Mathf.Abs(liftAngle) < 90){
            liftAngle = -90 * Mathf.Sign(liftAngle);
        } else {
            liftAngle = 90 * Mathf.Sign(liftAngle);
        }
        Debug.Log("liftAngle = " + liftAngle);
        
        Vector3 liftForceDirection = Quaternion.AngleAxis(liftAngle, Vector3.up) * apparentWind.normalized;
        return liftForceDirection;
    }

    Vector3 calculateApparentWindVector(Vector3 windSpeedVector, Vector3 boatSpeedVector) {
        // Since Face wind vector is caused by yacht movement it 
        // has the yacht velocity magnitude but opposite direction.
        // So we will just subtract one vector from another
        return windSpeedVector - boatSpeedVector;
    }

    //Keel force is opposite to boat side drag force caused by wind, but keel force is not the same as hull force
    //Beceause keel works kike a wing inder water
    //Keel force has lift effect, thus part of lift has forward vector (because of small side drag)
    void addForceToKeel() {
        float sideSpeed = Vector3.Dot(boatRigidbody.velocity, transform.right);
        float rightAngle = -90 * Mathf.Sign(sideSpeed);
        Vector3 liftUnderwaterDirection = Quaternion.AngleAxis(rightAngle, Vector3.up) * boatRigidbody.velocity.normalized;
        float antiDargCoeficient = 2; //TODO
        keelRigidbody.AddForce(liftUnderwaterDirection * sideSpeed * sideSpeed * waterRho * antiDargCoeficient);
    }

    // TODO Hull has significant anti drag effect. But I don't have formaula for it yet.
    // It is similar to keel force but has no lift effect and influences whole yacht body
    void addHullDragForce() {
        float sideSpeed = Vector3.Dot(boatRigidbody.velocity, transform.right);
        float rightAngle = -90 * Mathf.Sign(sideSpeed);
        Vector3 liftUnderwaterDirection = Quaternion.AngleAxis(rightAngle, Vector3.up) * boatRigidbody.velocity.normalized;
        float antiDargCoeficient = 2; //TODO
        boatRigidbody.AddForce(liftUnderwaterDirection * sideSpeed * sideSpeed * waterRho * antiDargCoeficient);
    }

    void initAngleForces() {
        angleToLiftCoeficient = new float[100];
        angleToDragCoeficient = new float[100];
        for(int i= 10; i < 100; i++) {
            float liftCoeficient = calcLiftCoeficient4(i);
            float dragCoeficient = calcDragCoeficient4(i);
            angleToLiftCoeficient[i] = liftCoeficient / liftCoeficientExperimentalFix; //TODO
            angleToDragCoeficient[i] = dragCoeficient / dragCoeficientExperimentalFix; //TODO
            //Debug.Log("Drag result for " + i + " lift=" + liftCoeficient + " drag=" + dragCoeficient);
        }
    }

    float calcualteFrictionalForce() {
        // Ffr = 1/2 * rho * V * V * S * Cf
        float Cf = 0.004f;
        // S = Cws * Sqrt( Vudw * Len )
        float S = 2.6f * Mathf.Sqrt(underwaterVolume * shipLenght);
        float Ffr = 0.5f * waterRho * Mathf.Pow(boatRigidbody.velocity.magnitude, 2) * S * Cf;
        return Ffr;
    }

    float calculateResidualForce() {
        // Fr = 1/2 * rho * V * V * S * Cr
        float Cr = 2 * Mathf.Exp(-3);
        float Fr = 0.5f * waterRho * Mathf.Pow(boatRigidbody.velocity.magnitude, 2) * Cr;
        return Fr;
    }

    float calculateSailForce(float coeficient, float velocity, float sailArea) {
        // Fs = 1/2 * rho * V * V * S * C
        float Fr = 0.5f * waterRho * velocity * velocity * sailArea * coeficient;
        return Fr;
    }

    // Calculate forces using Excel calculated polynomial trend
    // training data
    // angle:        10, 20, 30,   40,   50,   60,  70,  80,  90,  100
    // coefficient: 1.6, 13, 14.4, 14.5, 12.4, 9.6, 7.2, 5.0, 2.8, 1.4
    // Polynome: -2.36451E-05*X4 + 0.006606935*X3 - 0.659066142*X2 + 25.33863636*X - 173.9166667
    float calcLiftCoeficient4(float x) {
        double y =
            -0.00236451049  * (double) Mathf.Pow(x, 4) / Mathf.Pow(10, 4)
            + 0.06606934732 * (double) Mathf.Pow(x, 3) / Mathf.Pow(10, 3)
            -0.6590661422  * (double) Mathf.Pow(x, 2) / Mathf.Pow(10, 2)
            + 2.533863636 * (double) Mathf.Pow(x, 1) / Mathf.Pow(10, 1)
            -1.7391666;
        return (float)y;
    }

    // angle:         10,    20,   30,  40,  50,   60,  70,  80,   90,  100
    // coefficient: 0.15, 0.174, 0.22, 0.3, 0.4, 0.55, 0.7, 0.94, 1.28, 1.6
    // Polynome: 1.16861E-06*X3 + 1.35431E-05*X2 + 0.001794328*X + 0.127066667
    float calcDragCoeficient4(float x) {
        double y =
            + 0.001168609 * (double) Mathf.Pow(x, 3) / Mathf.Pow(10, 3)
            + 0.013543124 * (double) Mathf.Pow(x, 2) / Mathf.Pow(10, 2)
            + 0.017943279 * (double) Mathf.Pow(x, 1) / Mathf.Pow(10, 1)
            + 0.127066667;
        return (float)y;
    }

    float getCoeficientAtAngle(float[] angleToCoeficient, int angle) {
        float coeficient = 0;
        if(angle < 10){
            coeficient = 0;
        } else if(angle > 90 && angle < 270 ){
            coeficient = angleToCoeficient[99];
        } else if(angle >= 270 ){
            coeficient = angleToCoeficient[360 - angle];         
        } else {
            coeficient = angleToCoeficient[angle];
        } 
        return coeficient;
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
