using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class FootIK : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform leftFootTarget;
    [SerializeField] private Transform rightFootTarget;

    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [SerializeField] private Transform hipTarget;
    public Transform leftFootIKHandler;
    public Transform rightFootIKHandler;

    public AnimationCurve gaitCurve;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Vector3 leftFootOffset;
    [SerializeField] private Vector3 centreOfMassPos;
    [SerializeField] private Vector3 rightfootOffset;
    public Vector3 centreOfMassOffset;
    public Vector3 handOffset;
    public bool changeFeetPos = true;
    public bool alternateLeft = true;

    public float strideDetectionThreshhold = 2f;
    public float speed = 2f;
    public float lerpSPeed = 10f;
    Sachin.PlayerController playerController;
    NavMeshAgent navMeshAgent;

    public Transform position;

    private void Start()
    {
        playerController = FindObjectOfType<Sachin.PlayerController>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Cast a ray from the left foot target towards the ground
        StrideDetection();
        if (navMeshAgent)
            navMeshAgent?.SetDestination(position.position);
    }

    Vector3 footPos = Vector3.one;
    Vector3 leftFootPos = Vector3.one;

    private void OnAnimatorIK(int layerIndex)
    {

        if (changeFeetPos)
        {
            float leftfootStance = 0;
            float rightfootStance = 0;
            if (alternateLeft)
            {
                Ray ray = new Ray(leftFootTarget.position + Vector3.up, Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
                {
                    // Set the left foot position to the hit point on the ground
                    leftFootPos = new Vector3(hit.point.x + leftFootOffset.x, hit.point.y + leftFootOffset.y, hit.point.z + (leftFootOffset.z * Vector3.Dot(leftFootTarget.forward, leftFootIKHandler.forward)));
                    //leftFootIKHandler.position = leftFootPos;
                    leftfootStance = Mathf.Abs(hipTarget.position.y - leftFootIKHandler.position.y);
                }
            }
            else
            {
                Ray ray2 = new Ray(rightFootTarget.position + Vector3.up, Vector3.down);
                RaycastHit hit2;

                if (Physics.Raycast(ray2, out hit2, Mathf.Infinity, groundLayer))
                {
                    // Set the left foot position to the hit point on the ground 
                    footPos = new Vector3(hit2.point.x + rightfootOffset.x, hit2.point.y + rightfootOffset.y, hit2.point.z + (rightfootOffset.z * Vector3.Dot(rightFootTarget.forward, rightFootIKHandler.forward)));
                    rightFootIKHandler.position = footPos;
                    rightfootStance = Mathf.Abs(hipTarget.position.y - rightFootIKHandler.position.y);
                }
            }
            //if (leftfootStance <= 0.3 && rightfootStance <= 0.3)
            //{
            //    transform.position = new Vector3(transform.position.x, transform.position.y + 0.4f, transform.position.z);
            //}
            //else if (leftfootStance >= 0.8 && rightfootStance <= 0.8)
            //{
            //    transform.position = new Vector3(transform.position.x, transform.position.y - 0.4f, transform.position.z);
            //}
            changeFeetPos = false;
        }

        leftFootIKHandler.position = leftFootPos;
        //leftFootIKHandler.position = Vector3.Lerp(leftFootIKHandler.position, leftFootPos, gaitCurve.Evaluate(Time.deltaTime));
        //leftFootIKHandler.position = leftFootPos;
        rightFootIKHandler.position = footPos;

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, transform.rotation);
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIKHandler.position);

        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKRotation(AvatarIKGoal.RightFoot, transform.rotation);
        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIKHandler.position);

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, transform.rotation);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);

        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKRotation(AvatarIKGoal.RightHand, transform.rotation);
        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);

        leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, rightFootIKHandler.position + (transform.right * handOffset.x), lerpSPeed * Time.deltaTime);
        rightHandTarget.position = Vector3.Lerp(rightHandTarget.position, leftFootIKHandler.position + (transform.right * -handOffset.x), lerpSPeed * Time.deltaTime);
    }

    public void StrideDetection()
    {
        Vector3 rayPosition = new Vector3(hipTarget.position.x + centreOfMassOffset.x * playerController.horizontalInput,
            hipTarget.position.y + centreOfMassOffset.y, hipTarget.position.z + centreOfMassOffset.z * playerController.verticalInput);
        Ray ray = new Ray(rayPosition + Vector3.up, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            centreOfMassPos = hit.point;
        }

        if (alternateLeft)
        {
            if (Vector3.Distance(centreOfMassPos, leftFootIKHandler.position) > strideDetectionThreshhold)
            {
                changeFeetPos = true;
                alternateLeft = false;
            }
        }
        else
        {
            if (Vector3.Distance(centreOfMassPos, rightFootIKHandler.position) > strideDetectionThreshhold)
            {
                changeFeetPos = true;
                alternateLeft = true;
            }
        }

    }
}
