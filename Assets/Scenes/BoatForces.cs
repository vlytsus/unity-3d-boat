using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatForces : IYachtControls
{
    public Rigidbody yachtRigidbody;
    public Rigidbody keelRigidbody;
    public GameObject rudder;
    public GameObject waterArround;
    public GameObject underWaterObj;
    public GameObject windObj;
    public GameObject headSail;
    public GameObject mainSail;
    public HudMenu hudMenu;

    private Mesh underWaterMesh;

    public IYachtPhysics yachtPhysics;

    public float windSpeed = 6;

    private float[] headSailAngleToLiftCoeficient;
    private float[] headSailAngleToDragCoeficient;
    private float[] mainSailAngleToLiftCoeficient;
    private float[] mainSailAngleToDragCoeficient;

    const float waterRho = 1030.0f; //salt water density
    const float airRho = 1.2f; //air density

    float mainSailAreaM2; // Width * Height / 2
    float headSailAreaM2; // Width * Height / 2
    float underwaterSurface; //also known as Wetted surface
    float underwaterVolume; //also known as Displacement
    float shipLenght; // Water Line Length
    
    const int maxSailAngle = 80;
    
    void Start() {
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
        //underwaterSurface = CalculateSurfaceArea(underWaterMesh) / 2;
        //underwaterVolume = nderWaterObj.transform.localScale 
        initYachtParameters();
    }

    void initYachtParameters() {
        headSailAngleToLiftCoeficient = yachtPhysics.prepareHeadSailLiftCoefficients();
        headSailAngleToDragCoeficient = yachtPhysics.prepareHeadSailDragCoefficients();
        mainSailAngleToLiftCoeficient = yachtPhysics.prepareMainSailLiftCoefficients();
        mainSailAngleToDragCoeficient = yachtPhysics.prepareMainSailDragCoefficients();
        mainSailAreaM2 = yachtPhysics.getMainSailAreaM2();
        headSailAreaM2 = yachtPhysics.getHeadSailAreaM2();
        underwaterSurface = yachtPhysics.getUnderwaterSurface();
        underwaterVolume = yachtPhysics.getUnderwaterVolume();
        shipLenght = yachtPhysics.getShipLenght();
    }

    public override void rotateHeadSail(int angle){
        rotateSail(headSail, angle);
    }

    public override void rotateMainSail(int angle){
        rotateSail(mainSail, angle);
    }

    public override Vector3 getVelocity(){
        return yachtRigidbody.velocity;
    }

    public override void rotateRudder(int angle){
        if(angle > 0){
            yachtRigidbody.AddForceAtPosition(transform.right * 1000 * getVelocity().magnitude, rudder.transform.position, ForceMode.Force);
        } else if (angle < 0) {
            yachtRigidbody.AddForceAtPosition(transform.right * -1000 * getVelocity().magnitude, rudder.transform.position, ForceMode.Force);
        }
    }

    Vector3 getApparentWindVector() {

        Vector3 windSpeedVector = windObj.transform.forward * windSpeed;
        Vector3 boatSpeedVector = getVelocity();

        // Since Face wind vector is caused by yacht movement it 
        // has the yacht velocity magnitude but opposite direction.
        // So we will just subtract one vector from another
        return windSpeedVector - boatSpeedVector;
    }

    int getSailApparentAngleGrad(GameObject sail, Vector3 apparentWind){
        Vector3 sailVector = sail.transform.forward;
        int apparentWindAngleGrad = (int)Vector3.Angle(sailVector, -apparentWind);
        return apparentWindAngleGrad;
    }

    void FixedUpdate() {
        Vector3 apparentWind = getApparentWindVector();
        hudMenu.onAwaAngleChange(apparentWind);

        addSailForce(headSail, apparentWind, headSailAreaM2);
        addSailForce(mainSail, apparentWind, mainSailAreaM2);

        yachtRigidbody.AddForce(-getVelocity().normalized * calcualteFrictionalForce());
        yachtRigidbody.AddForce(-getVelocity().normalized * calculateResidualForce());

        addHullDragForce();        
        addForceToKeel();
        Debug.DrawRay(transform.position + yachtRigidbody.centerOfMass, getVelocity() * 10, Color.green, 0.0f, false);
    }

    void rotateSail(GameObject sail, int angle) {
        HingeJoint hinge = sail.GetComponent<HingeJoint>();
        
        JointLimits limits = hinge.limits;

        int sailAngle = (int)Vector3.Angle(transform.forward, sail.transform.forward);
        if(angle < 0 && Mathf.Abs(limits.min) < Mathf.Abs(maxSailAngle)){
            limits.min += angle;
            limits.max = limits.min + 30;
        } else if (angle >= 0 && Mathf.Abs(limits.max) < Mathf.Abs(maxSailAngle)) {
            limits.max += angle;
            limits.min = limits.max - 30;
        }
        hinge.limits = limits;
        hinge.useLimits = true;

        /*
        JointSpring hingeSpring = hinge.spring;
        hingeSpring.targetPosition += angle;
        if(Mathf.Abs(hingeSpring.targetPosition) < maxSailAngle){
            hinge.spring = hingeSpring;            
        }*/
    }

    void addSailForce(GameObject sail, Vector3 apparentWind, float sailAreaM2) {           
        Vector3 trueWind = windObj.transform.forward * windSpeed;
        Vector3 sailVector = sail.transform.forward;
        float windVelocity = apparentWind.magnitude;
        int sailAppraentAngleGrad = getSailApparentAngleGrad(sail, apparentWind);
        float liftCoeficient = getHeadSailLiftCoeficientAtAngle(sailAppraentAngleGrad);
        float dragCoeficient = getHeadSailDragCoeficientAtAngle(sailAppraentAngleGrad);

        Vector3 liftForceDirection = calculateLiftDirection(apparentWind, sailVector);       
        Vector3 liftForce = liftForceDirection * calculateSailForce(liftCoeficient, windVelocity, sailAreaM2);
        Vector3 dragForce = apparentWind.normalized * calculateSailForce(dragCoeficient, windVelocity, sailAreaM2);

        
        Rigidbody sailRb = sail.GetComponent<Rigidbody>();       
        sailRb.AddForce(liftForce);
        sailRb.AddForce(dragForce);
        Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, liftForce / 10, Color.blue, 0.0f, false);
        Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, -apparentWind, Color.yellow, 0.0f, false);
        Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, sailVector, Color.yellow, 0.0f, false);
        Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, dragForce / 10, Color.red, 0.0f, false);
    }

    Vector3 calculateLiftDirection(Vector3 apparentWind, Vector3 sailVector){
        float liftAngle = Vector3.SignedAngle(-apparentWind.normalized, sailVector.normalized, Vector3.up);
        if(Mathf.Abs(liftAngle) < 180){
            liftAngle = -90 * Mathf.Sign(liftAngle);
        } else {
            liftAngle = 90 * Mathf.Sign(liftAngle);
        }
        //Debug.Log("liftAngle = " + liftAngle);
        Vector3 liftForceDirection = Quaternion.AngleAxis(liftAngle, Vector3.up) * apparentWind.normalized;
        return liftForceDirection;
    }

    //Keel force is opposite to boat side drag force caused by wind, but keel force is not the same as hull force
    //Beceause keel works kike a wing inder water
    //Keel force has lift effect, thus part of lift has forward vector (because of small side drag)
    void addForceToKeel() {
        float sideSpeed = Vector3.Dot(getVelocity(), transform.right);
        float rightAngle = -90 * Mathf.Sign(sideSpeed);
        Vector3 liftUnderwaterDirection = Quaternion.AngleAxis(rightAngle, Vector3.up) * getVelocity().normalized;
        float antiDargCoeficient = 2; //TODO
        keelRigidbody.AddForce(liftUnderwaterDirection * sideSpeed * sideSpeed * waterRho * antiDargCoeficient);
    }

    // TODO Hull has significant anti drag effect. But I don't have formaula for it yet.
    // It is similar to keel force but has no lift effect and influences whole yacht body
    void addHullDragForce() {
        float sideSpeed = Vector3.Dot(getVelocity(), transform.right);
        float rightAngle = -90 * Mathf.Sign(sideSpeed);
        Vector3 liftUnderwaterDirection = Quaternion.AngleAxis(rightAngle, Vector3.up) * getVelocity().normalized;
        float antiDargCoeficient = 2; //TODO
        yachtRigidbody.AddForce(liftUnderwaterDirection * sideSpeed * sideSpeed * waterRho * antiDargCoeficient);
    }

    float calcualteFrictionalForce() {
        // Ffr = 1/2 * rho * V * V * S * Cf
        float Cf = 0.004f;
        // S = Cws * Sqrt( Vudw * Len )
        float S = 2.6f * Mathf.Sqrt(underwaterVolume * shipLenght);
        float Ffr = 0.5f * waterRho * Mathf.Pow(getVelocity().magnitude, 2) * S * Cf;
        return Ffr;
    }

    float calculateResidualForce() {
        // Fr = 1/2 * rho * V * V * S * Cr
        float Cr = 2 * Mathf.Exp(-3);
        float Fr = 0.5f * waterRho * Mathf.Pow(getVelocity().magnitude, 2) * Cr;
        return Fr;
    }

    float calculateSailForce(float coeficient, float velocity, float sailArea) {
        // Fs = 1/2 * rho * V * V * S * C
        float Fr = 0.5f * airRho * velocity * velocity * sailArea * coeficient;
        return Fr;
    }

    float getCoeficientAtAngle(float[] angleToCoeficient, int angle) {
        float coeficient = 0;
        if(angle < 0){
            coeficient = 0;
        } else if(angle > 180 ){
            coeficient = angleToCoeficient[360 - angle];         
        } else {
            coeficient = angleToCoeficient[angle];
        } 
        return coeficient;
    }

    public override float getHeadSailLiftCoeficientAtAngle(int angle){
        return getCoeficientAtAngle(headSailAngleToLiftCoeficient, angle);
    }
    public override float getMainSailLiftCoeficientAtAngle(int angle){
        return getCoeficientAtAngle(mainSailAngleToLiftCoeficient, angle);
    }
    public override float getHeadSailDragCoeficientAtAngle(int angle){
        return getCoeficientAtAngle(headSailAngleToDragCoeficient, angle);
    }
    public override float getMainSailDragCoeficientAtAngle(int angle){
        return getCoeficientAtAngle(mainSailAngleToDragCoeficient, angle);
    }

    float CalculateSurfaceArea(Mesh mesh) {
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;
        float sum = 0.0f;
        for(int i = 0; i < triangles.Length; i += 3) {
            Vector3 corner = vertices[triangles[i]];
            Vector3 a = vertices[triangles[i + 1]] - corner;
            Vector3 b = vertices[triangles[i + 2]] - corner;

            sum += Vector3.Cross(a, b).magnitude;
        }
        return sum/2.0f;
    }
}
