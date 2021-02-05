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

        rotateToOptimalSailAngle(headSail, apparentWind, headSailAreaM2);
        rotateToOptimalSailAngle(mainSail, apparentWind, mainSailAreaM2);

        addSailForce(headSail, apparentWind, headSailAreaM2);
        addSailForce(mainSail, apparentWind, mainSailAreaM2);

        yachtRigidbody.AddForce(-getVelocity().normalized * calcualteFrictionalForce());
        yachtRigidbody.AddForce(-getVelocity().normalized * calculateResidualForce());

        addHullDragForce();        
        addForceToKeel();
        Debug.DrawRay(transform.position + yachtRigidbody.centerOfMass, getVelocity() * 10, Color.green, 0.0f, false);
    }

    void rotateSail(GameObject sail, int angle) {
        int minLimit = getSailJointMinLimit(sail);
        int maxLimit = getSailJointMaxLimit(sail);

        if(angle < 0 && Mathf.Abs(minLimit) < Mathf.Abs(maxSailAngle)){
            minLimit += angle;
            maxLimit = minLimit + 30;
        } else if (angle >= 0 && Mathf.Abs(maxLimit) < Mathf.Abs(maxSailAngle)) {
            maxLimit += angle;
            minLimit = maxLimit - 30;
        }
        setSailJointLimits(sail, minLimit, maxLimit);
    }

    int getSailJointMaxLimit(GameObject sail) {
        return (int)sail.GetComponent<HingeJoint>().limits.max;
    }

    int getSailJointMinLimit(GameObject sail) {
        return (int)sail.GetComponent<HingeJoint>().limits.min;
    }

    void setSailJointLimits(GameObject sail, int angle) {
        setSailJointLimits(sail, angle-0.1f, angle+0.1f);
    }

    void setSailJointLimits(GameObject sail, float minAngle, float maxAngle) {
        HingeJoint hinge = sail.GetComponent<HingeJoint>();        
        JointLimits limits = hinge.limits;
        limits.max = maxAngle;
        limits.min = minAngle;
        hinge.limits = limits;
        hinge.useLimits = true;
    }

    void addSailForce(GameObject sail, Vector3 apparentWind, float sailAreaM2) {
        Vector3 sailVector = sail.transform.forward;
        float windVelocity = apparentWind.magnitude;
        
        int sailAppraentAngleGrad = getSailApparentAngleGrad(sail, apparentWind);
        float liftCoeficient = getHeadSailLiftCoeficientAtAngle(sailAppraentAngleGrad);
        float dragCoeficient = getHeadSailDragCoeficientAtAngle(sailAppraentAngleGrad);
        Vector3 liftForceDirection = calculateLiftDirection(apparentWind, sailVector);
        Vector3 liftForce = liftForceDirection * calculateSailForce(liftCoeficient, windVelocity, sailAreaM2);
        Vector3 dragForce = apparentWind.normalized * calculateSailForce(dragCoeficient, windVelocity, sailAreaM2);
        Vector3 resultForce = liftForce + dragForce;
        
        Rigidbody sailRb = sail.GetComponent<Rigidbody>();       
        sailRb.AddForce(resultForce);
        //Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, liftForce / 10, Color.blue, 0.0f, false);
        //Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, -apparentWind, Color.yellow, 0.0f, false);
        //Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, sailVector*10, Color.yellow, 0.0f, false);
        //Debug.DrawRay(sail.transform.position + sailRb.centerOfMass, dragForce / 10, Color.red, 0.0f, false);
    }

    void rotateToOptimalSailAngle(GameObject sail, Vector3 apparentWind, float sailAreaM2){
        float maxProjection = 0;
        float currentProjection = 0;
        int resultAngle = 0;
        float windVelocity = apparentWind.magnitude;
        int sailAngleGrad = -(int)Vector3.SignedAngle(sail.transform.forward, transform.forward, Vector3.up);          
        int resultApa = 0;

        for(int i = -maxSailAngle+20; i < maxSailAngle-20; i+=3) {
            
            Vector3 sailVector = rotateVectorByDegree(transform.forward, i);
            int sailAppraentAngleGrad = (int)Vector3.Angle(sailVector, -apparentWind);
            float liftCoeficient = getHeadSailLiftCoeficientAtAngle(sailAppraentAngleGrad);
            float dragCoeficient = getHeadSailDragCoeficientAtAngle(sailAppraentAngleGrad);            
            Vector3 liftForceDirection = calculateLiftDirection(apparentWind, sailVector);
            Vector3 liftForce = liftForceDirection * calculateSailForce(liftCoeficient, windVelocity, sailAreaM2);
            Vector3 dragForce = apparentWind.normalized * calculateSailForce(dragCoeficient, windVelocity, sailAreaM2);
            Vector3 resultForce = liftForce + dragForce;

            int resultMagnitude = (int)Vector3.Dot(resultForce, transform.forward);
            if(resultMagnitude >  maxProjection){
                maxProjection = resultMagnitude;
                resultAngle = i;
                resultApa = sailAppraentAngleGrad;
            }           
        }

        setSailJointLimits(sail, resultAngle);
        Debug.Log ("resultAngle=" +resultAngle+ " sailAngleGrad=" + sailAngleGrad + " maxProjection=" + maxProjection);
    }

    Vector3 calculateLiftDirection(Vector3 apparentWind, Vector3 sailVector){
        float liftAngle = Vector3.SignedAngle(-apparentWind.normalized, sailVector.normalized, Vector3.up);
        if(Mathf.Abs(liftAngle) < 180){
            //Mathf.Sign(x) == 1 when x is positive or zero, -1 when x is negative.
            liftAngle = -90 * Mathf.Sign(liftAngle);
        } else {
            liftAngle = 90 * Mathf.Sign(liftAngle);
        }
        //lift angle is always 90 degree to the apparent wind, but has left or right direction
        Vector3 liftForceDirection = rotateVectorByDegree(apparentWind, liftAngle);
        //So, we are getting liftForceDirection vector by rotating apparent wind to 90 degree left or right
        return liftForceDirection;
    }

    Vector3 rotateVectorByDegree(Vector3 origVector, float degree) {
        Vector3 rotatedVector = Quaternion.AngleAxis(degree, Vector3.up) * origVector.normalized;
        return rotatedVector;
    }

    //Keel force is opposite to boat side drag force caused by wind, but keel force is not the same as hull force
    //Beceause keel works kike a wing inder water
    //Keel force has also lift effect, thus part of lift has forward vector (because of small side drag)
    void addForceToKeel() {
        float sideSpeed = Vector3.Dot(getVelocity(), transform.right);
        float rightAngle = -90 * Mathf.Sign(sideSpeed);
        Vector3 liftUnderwaterDirection = rotateVectorByDegree(getVelocity(), rightAngle);
        float antiDargCoeficient = 2f; //TODO
        keelRigidbody.AddForce(liftUnderwaterDirection * sideSpeed * sideSpeed * waterRho * antiDargCoeficient);
    }

    // TODO Hull has significant anti drag effect. But I don't have formaula for it yet.
    // It is similar to keel force but has no lift effect and influences whole yacht body
    void addHullDragForce() {
        float sideSpeed = Vector3.Dot(getVelocity(), transform.right);
        float dargCoeficient = 0.25f; //TODO
        yachtRigidbody.AddForce(-getVelocity().normalized * sideSpeed * sideSpeed * waterRho * dargCoeficient);
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
