using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CrowdEntityAlt : Selectable
{
    public float movementSpeed = 1;
    public float turningSpeed = 30;

    // internal private class vars
    [SerializeField] private bool isMoving = false;
    private bool isTurning = false;
    private Vector3 recalibratedWayPoint;
    private NavMeshPath path;
    private int currentPathCorner = 0;
    private Quaternion currentRotateTo;
    private Vector3 currentRotateDir;
    private Vector3 groundDestination;
    private Rigidbody rb;
    public bool moveForward;
    public Vector3 TargetPosition;
    [SerializeField] private Transform targetPos;
    [SerializeField] private float standingForce;
    [SerializeField] private float pushForce;
    [SerializeField] private float raycastDistance = 2.0f;

    [SerializeField] private Transform leftShoulder;
    [SerializeField] private Transform rightShoulder;

    [SerializeField] private float stabilizationStrength = 10f; // Adjust the strength of stabilization

    [SerializeField] private float targetHeight = 1.0f; // Desired height above the ground
    [SerializeField] private float forceStrength = 10.0f; // Strength of the upward force

    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
    }

    private void Update()
    {

        
    }

    private void FixedUpdate()
    {

        if (isMoving)
        {
            // account for any turning needed
            if (isTurning)
            {
                currentRotateTo.x = transform.rotation.x;
                currentRotateTo.z = transform.rotation.z;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, currentRotateTo, Time.deltaTime * turningSpeed);
                if (Vector3.Angle(transform.forward, currentRotateDir) < 1) isTurning = false;
            }

            // applying force gives a natural feel to the rolling movement 
            recalibratedWayPoint.y = transform.position.y;
            Vector3 tmpDir = (recalibratedWayPoint - transform.position).normalized;
            //tmpDir.y = 0;
            rb.AddForce(tmpDir * movementSpeed * Time.deltaTime, ForceMode.Force);

            // check to see if you got to your latest waypoint
            if (Vector3.Distance(recalibratedWayPoint, transform.position) < 1)
            {
                isTurning = false;
                currentPathCorner++;
                if (currentPathCorner >= path.corners.Length)
                {
                    // you have arrived at the destination
                    isMoving = false;
                }
                else
                {
                    // recalibrate the y coordinate to account for the difference between the piece's centerpoint
                    // and the ground's elevation
                    recalibratedWayPoint = path.corners[currentPathCorner];
                    recalibratedWayPoint.y = transform.position.y;
                    isTurning = true;
                    currentRotateDir = (recalibratedWayPoint - transform.position).normalized;
                    currentRotateTo = Quaternion.LookRotation(currentRotateDir);
                }
            }
        }

        // Calculate the torque to keep the Rigidbody upright
        Vector3 torque = CalculateStabilizationTorque();

        // Apply the torque to the Rigidbody
        rb.AddTorque(torque, ForceMode.Force);

        //StandingSetup();

        // Calculate the distance between the Rigidbody's position and the ground
        float distanceToGround = CalculateDistanceToGround();

        // Calculate the force needed to keep the Rigidbody off the ground
        float forceMagnitude = CalculateForceMagnitude(distanceToGround);

        // Apply the upward force to the Rigidbody
        Vector3 upwardForce = Vector3.up * forceMagnitude;
        rb.AddForce(upwardForce, ForceMode.Force);
        PeerDetection();

    }

    public void StandingSetup()
    {
        rb.AddRelativeForce(transform.GetChild(0).up * standingForce * Time.deltaTime, ForceMode.Force);
    }

    public void setMovementDestination(Vector3 tmpDest)
    {
        groundDestination = tmpDest;
        groundDestination.y = 2;
        currentPathCorner = 1;
        path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, groundDestination, NavMesh.AllAreas, path);
        // sometimes path winds up having 1 or less corners - skip this setting event if that's the case
        if (path.corners.Length > 1)
        {
            isMoving = true;
            isTurning = true;
            // recalibrate the y coordinate to account for the difference between the piece's centerpoint
            // and the ground's elevation
            recalibratedWayPoint = path.corners[currentPathCorner];
            //recalibratedWayPoint.y = transform.position.y;
            currentRotateDir = (recalibratedWayPoint - transform.position).normalized;
            currentRotateTo = Quaternion.LookRotation(currentRotateDir);
        }
    }

    private Vector3 CalculateStabilizationTorque()
    {
        // Calculate the angle between the current up direction and the desired up direction (e.g., world up)
        Quaternion desiredRotation = Quaternion.FromToRotation(transform.up, Vector3.up);
        float angle = 0f;
        Vector3 axis = Vector3.zero;
        desiredRotation.ToAngleAxis(out angle, out axis);

        // Calculate the torque needed to stabilize the Rigidbody
        Vector3 torque = axis.normalized * angle * stabilizationStrength;

        return torque;
    }

    private float CalculateDistanceToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.GetChild(0).position, Vector3.down, out hit, Mathf.Infinity, 6))
        {
            return hit.distance;
        }
        return 0.0f;
    }

    private float CalculateForceMagnitude(float distanceToGround)
    {
        // Calculate the difference between the current height and the target height
        float heightDifference = targetHeight - distanceToGround;

        // Calculate the force needed to counteract gravity and keep the Rigidbody off the ground
        float forceMagnitude = rb.mass * Physics.gravity.magnitude + heightDifference * forceStrength;

        return forceMagnitude;
    }

    private void PeerDetection()
    {
        RaycastHit hit1;
        RaycastHit hit2;

        if (Physics.Raycast(leftShoulder.position, transform.forward, out hit1, raycastDistance))
        {
            //if (hit1.collider.gameObject.CompareTag("Pushable"))
            {
                Rigidbody hitRigidbody = hit1.collider.gameObject.GetComponent<Rigidbody>();
                if (hitRigidbody != null)
                {
                    Vector3 pushDirection = (hitRigidbody.transform.position - transform.position).normalized;
                    hitRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                }
            }
        }

        if (Physics.Raycast(rightShoulder.position, transform.forward, out hit2, raycastDistance))
        {
            //if (hit2.collider.gameObject.CompareTag("Pushable"))
            {
                Rigidbody hitRigidbody = hit2.collider.gameObject.GetComponent<Rigidbody>();
                if (hitRigidbody != null)
                {
                    Vector3 pushDirection = (hitRigidbody.transform.position - transform.position).normalized;
                    hitRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                }
            }
        }

    }
}
