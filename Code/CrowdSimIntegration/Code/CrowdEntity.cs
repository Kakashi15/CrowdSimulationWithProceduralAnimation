using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CrowdEntity : Selectable
{
    public Vector3 TargetPosition; // The position the NavMesh Agent should move towards.
    [SerializeField] private Transform navmshTargetTransform;
    [SerializeField] private Transform leftRay;
    [SerializeField] private Transform rightRay;

    [SerializeField] private float shoulderDetectionRadius = 4f;
    [SerializeField] private float rorationSpeed = 4f;
    [SerializeField] private float rotationAmount = 4f;
    [SerializeField] private float moveForwardDistance = 50f;
    [SerializeField] private float aggressivePushForce = 2f;

    [SerializeField] private bool aggressive;
    [SerializeField] private bool aggressiveAllowRotation;
    [SerializeField] private bool evassive;

    private Vector3 initEulerAngle;
    private Vector3 initPos;

    private NavMeshAgent navMeshAgent;

    [SerializeField] private float timer;
    [SerializeField] private float waitTime = 5f; // 5 seconds
    private float originalSpeed = 1;

    public bool moveForward = false;

    private List<Vector3> pathWaypoints = new List<Vector3>();
    [SerializeField] private bool drawPath;
    private LineRenderer lineRenderer;

    private FieldOfView fieldOfView;

    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightLegPos;
    [SerializeField] private Vector3 leftArmOffset;

    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftLegPos;
    [SerializeField] private Vector3 rightArmOffset;
    [SerializeField] public LegController legController;

    [SerializeField] LayerMask obstacleMask;
    [SerializeField] LayerMask entityMask;
    private void Start()
    {
        fieldOfView = GetComponent<FieldOfView>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        initEulerAngle = transform.eulerAngles;
        initPos = transform.position;
        //navMeshAgent.speed = originalSpeed;
        if (drawPath)
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
        }

    }

    private void Update()
    {
        if (navMeshAgent.enabled)
        {
            if (Input.GetKeyDown(KeyCode.R) && IsSelected())
            {
                aggressive = true;
            }

            if (Input.GetKeyDown(KeyCode.Y) && IsSelected())
            {
                evassive = true;
            }


            // Move the agent to the target position.
            if (moveForward)
                TargetPosition = initPos + transform.forward * moveForwardDistance;

            if (fieldOfView.pathCorrection == Vector3.zero || fieldOfView.pathCorrection == Vector3.up)
                navMeshAgent.SetDestination(TargetPosition);
            else
            {
                navMeshAgent.SetDestination(fieldOfView.pathCorrection);
                if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
                    fieldOfView.pathCorrection = Vector3.up;
            }


            if (navMeshAgent.velocity.magnitude < navMeshAgent.speed / 2 && navMeshAgent.remainingDistance > 1)
            {
                timer += Time.deltaTime;
            }

            if (timer > waitTime)
            {
                //aggressive = true;
                //deselectedColor = Color.red;
                timer = 0;
            }



            if (navMeshAgent.remainingDistance < 1)
            {
                // Reset the NavMeshAgent speed to the original value

            }

            pathWaypoints.Clear();
            foreach (Vector3 waypoint in navMeshAgent.path.corners)
            {
                pathWaypoints.Add(waypoint);
            }

            // Update the Line Renderer
            if (drawPath)
                UpdateLineRenderer();

        }

        //ShoulderRotation();
        if (aggressive)
        {
            deselectedColor = Color.red;
            AggressivePush();
        }

        if (evassive)
        {
            deselectedColor = Color.yellow;
            ShoulderRotation();
        }
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = pathWaypoints.Count; // Set the number of line positions

        for (int i = 0; i < pathWaypoints.Count; i++)
        {
            lineRenderer.SetPosition(i, pathWaypoints[i]); // Set each line position
        }
    }

    private void AggressivePush()
    {
        //check if there's an entity infront of you using raycast
        //check how far it is
        //if its close enough (0.2f) change modify its navmeshagent velocity to offset it left of right side of the yourself

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f, entityMask))
        {
            // Check if the hit object has a NavMeshAgent
            NavMeshAgent targetAgent = hit.collider.gameObject.transform.parent.GetComponent<NavMeshAgent>();
            if (targetAgent != null)
            {
                // Calculate the distance between the two agents
                float distance = Vector3.Distance(transform.position, targetAgent.transform.position);

                // If the distance is close enough, offset the target's velocity
                if (distance < 1.2f)
                {
                    legController.armRestPos = hit.point;
                    Vector3 pushDirection = Vector3.Cross(Vector3.up, transform.forward).normalized;
                    int randomVal = Random.Range(0, 2);
                    Vector3 pushForceVector;
                    if (Physics.Raycast(transform.position, pushDirection, out RaycastHit hit2, 1.2f, obstacleMask))
                    {
                        pushForceVector = pushDirection * 3;
                    }
                    else
                    {
                        pushForceVector = pushDirection * -3f;
                    }
                    //pushForceVector = pushDirection * 3;
                    targetAgent.velocity = pushForceVector;
                    if (aggressiveAllowRotation)
                        transform.LookAt(targetAgent.transform);
                }
                else
                {
                    legController.armRestPos = Vector3.zero;
                }
            }
            else
            {
                legController.armRestPos = Vector3.zero;
            }
        }

    }

    private void ShoulderRotation()
    {
        if (Physics.Raycast(leftRay.position, transform.forward, out RaycastHit hit, shoulderDetectionRadius))
        {
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - rotationAmount, transform.eulerAngles.z), rorationSpeed * Time.deltaTime);
        }

        if (Physics.Raycast(rightRay.position, transform.forward, out RaycastHit hit2, shoulderDetectionRadius))
        {
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationAmount, transform.eulerAngles.z), rorationSpeed * Time.deltaTime);
        }

        Debug.DrawLine(leftRay.position, leftRay.position + transform.forward * shoulderDetectionRadius, Color.red);
        Debug.DrawLine(rightRay.position, rightRay.position + transform.forward * shoulderDetectionRadius, Color.red);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < pathWaypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(pathWaypoints[i], pathWaypoints[i + 1]);
        }
    }
}
