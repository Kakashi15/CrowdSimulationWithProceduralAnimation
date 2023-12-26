using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LegController : MonoBehaviour
{
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform bodyTransformBase;
    [SerializeField] private Leg[] legs;

    [SerializeField] private Transform leftArm;
    [SerializeField] private Vector3 lefArmInitPos;
    [SerializeField] private Vector3 leftArmOffset;

    [SerializeField] private Transform rightArm;
    [SerializeField] private Vector3 rightArmInitPos;
    [SerializeField] private Vector3 rightArmOffset;

    [SerializeField] private Vector3 bodyTransfromInitPos;

    [SerializeField]
    private AnimationCurve armCurve;

    private float maxTipWait = 0.7f;

    [SerializeField] private bool readySwitchOrder = false;
    [SerializeField] private bool stepOrder = true;

    [SerializeField] float bodyHeightBase = 1.3f;
    [SerializeField] float dot;

    private NavMeshAgent navMeshAgent;

    public bool StepOrder
    {
        get { return stepOrder; }
        set
        {
            if (stepOrder != value)
            {
                stepOrder = value;
            }
        }
    }

    private Vector3 bodyPos;
    private Vector3 bodyUp;
    private Vector3 bodyForward;
    private Vector3 bodyRight;
    private Quaternion bodyRotation;

    [SerializeField] public float PosAdjustRatio = 0.1f;
    [SerializeField] float RotAdjustRatio = 0.2f;
    [SerializeField] private float armLerpTime;
    [SerializeField] private float velocityMag;
    [SerializeField] private Vector3 velocityVect;

    private float timer;
    private Vector3 dir;
    private float elapsedTimer;
    public bool rayMod = false;

    private List<Vector3> visitedCorners = new List<Vector3>();
    private int currentCornerIndex = 0;


    private Dictionary<int, bool> cornerDiction;

    SecondOrderDynamics leftArmSecondOrderDynamics;
    SecondOrderDynamics rightArmSecondOrderDynamics;

    [Header("Left Leg Values")]
    [Range(0.0f, 500.0f)]
    public float fl;
    [Range(0f, 5.0f)]
    public float zl;
    [Range(-5f, 5.0f)]
    public float rl;

    [Header("Rigt Leg Values")]
    [Range(0.0f, 500.0f)]
    public float fr;
    [Range(0f, 5.0f)]
    public float zr;
    [Range(-5f, 5.0f)]
    public float rr;


    [Header("Left Arm Values")]
    [Range(0.0f, 500.0f)]
    public float fal;
    [Range(0f, 5.0f)]
    public float zal;
    [Range(-10f, 10.0f)]
    public float ral;

    [Header("Right Arm Values")]
    [Range(0.0f, 500.0f)]
    public float far;
    [Range(0f, 5.0f)]
    public float zar;
    [Range(-10f, 10.0f)]
    public float rar;


    public Vector3 armRestPos;

    private void Start()
    {
        // Start coroutine to adjust body transform
        armRestPos = Vector3.zero;
        lefArmInitPos = leftArm.transform.position;
        rightArmInitPos = rightArm.transform.position;
        bodyTransfromInitPos = bodyTransform.position;
        leftArmSecondOrderDynamics = new SecondOrderDynamics(fal, zal, ral, leftArm.transform.position);
        rightArmSecondOrderDynamics = new SecondOrderDynamics(far, zar, rar, leftArm.transform.position);

        navMeshAgent = bodyTransform.GetComponent<NavMeshAgent>();
        StartCoroutine(AdjustBodyTransform());
    }

    public float lerpValue;
    private void Update()
    {
        if (legs.Length < 2) return;

        leftArmSecondOrderDynamics.ValueUpdate(fal, zal, ral);
        rightArmSecondOrderDynamics.ValueUpdate(far, zar, rar);

        legs[0].dynamicsUpdate(fl, zl, rl);
        legs[1].dynamicsUpdate(fr, zr, rr);
        // If tip is not in current order but it's too far from target position, Switch the order
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i].TipDistance > maxTipWait)
            {
                StepOrder = i % 2 == 0;
                break;
            }
        }

        float vertical = Input.GetAxis("Vertical");

        // Ordering steps
        foreach (Leg leg in legs)
        {
            leg.Movable = StepOrder;
            StepOrder = !StepOrder;
            if (rayMod)
            {
                leg.rayOrigin.transform.localPosition = new Vector3(leg.rayOrigin.transform.localPosition.x,
                    leg.rayOrigin.transform.localPosition.y,
                    (Mathf.Sign(velocityVect.z) * -1) * 0.3f);
            }
        }

        int index = StepOrder ? 0 : 1;

        // If the opposite foot step completes, switch the order to make a new step
        if (readySwitchOrder && !legs[index].Animating)
        {
            StepOrder = !StepOrder;
            readySwitchOrder = false;
            //timer = 0;
        }

        if (!readySwitchOrder && legs[index].Animating)
        {
            readySwitchOrder = true;
        }

        elapsedTimer += Time.deltaTime;

        velocityVect = (bodyTransfromInitPos - bodyTransform.position);

        velocityVect = velocityVect / elapsedTimer;

        velocityMag = velocityVect.magnitude;

        bodyTransfromInitPos = bodyTransform.position;

        if (timer < armLerpTime)
        {
            timer += Time.deltaTime;
            lerpValue = armCurve.Evaluate(timer / armLerpTime);

        }
        else if (velocityMag > 0 && timer >= armLerpTime)
            timer = 0;

        Vector3 leftArmDesPos = legs[1].ikTarget.transform.TransformPoint(leftArmOffset);
        leftArmDesPos.y = leftArmOffset.y;
        Vector3 leftArModPos = leftArmSecondOrderDynamics.Update(Time.deltaTime, leftArmDesPos, Vector3.zero);
        leftArm.transform.position = leftArModPos;

        if (armRestPos != Vector3.zero)
        {
            Vector3 rightArModPos = rightArmSecondOrderDynamics.Update(Time.deltaTime, armRestPos, Vector3.zero);
            rightArm.transform.position = rightArModPos;
        }
        else
        {
            Vector3 rightArmDesPos = legs[0].ikTarget.transform.TransformPoint(rightArmOffset);
            rightArmDesPos.y = rightArmOffset.y;
            Vector3 rightArModPos = rightArmSecondOrderDynamics.Update(Time.deltaTime, rightArmDesPos, Vector3.zero);
            rightArm.transform.position = rightArModPos;
        }

        //rightArm.rotation = Quaternion.identity;

        foreach (Leg leg in legs)
        {
            leg.ikTarget.transform.rotation = bodyTransform.rotation;
        }
        //transform.rotation = bodyTransform.rotation;
    }


    private IEnumerator AdjustBodyTransform()
    {
        while (true)
        {

            Vector3 tipCenter = Vector3.zero;
            bodyUp = Vector3.zero;

            // Collect leg information to calculate body transform
            foreach (Leg leg in legs)
            {
                tipCenter += leg.TipPos;
                bodyUp += leg.TipUpDir + leg.RaycastTipNormal;
            }

            RaycastHit hit;
            if (Physics.Raycast(bodyTransform.position, bodyTransform.up * -1, out hit, 10.0f))
            {
                bodyUp += hit.normal;
            }

            tipCenter /= legs.Length;
            bodyUp.Normalize();

            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                tipCenter = navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance ? navMeshAgent.destination : tipCenter;
                TrackVisitedCorners();
            }
            // Interpolate postition from old to new
            bodyPos = tipCenter + bodyUp * bodyHeightBase;

            bodyTransform.position = Vector3.Lerp(bodyTransform.position, bodyPos, PosAdjustRatio);

            // Calculate new body axis
            bodyRight = Vector3.Cross(bodyUp, bodyTransform.forward);
            bodyForward = Vector3.Cross(bodyRight, bodyUp);

            yield return new WaitForFixedUpdate();
        }
    }

    private void TrackVisitedCorners()
    {
        Vector3[] corners = navMeshAgent.path.corners;

        if (corners.Length > 0)
        {
            if (currentCornerIndex < corners.Length)
            {
                Vector3 currentCorner = corners[currentCornerIndex];

                if (Vector3.Distance(transform.position, currentCorner) < navMeshAgent.stoppingDistance)
                {
                    if (!visitedCorners.Contains(currentCorner))
                    {
                        visitedCorners.Add(currentCorner);
                        currentCornerIndex++;
                    }
                }
            }
        }
    }

    private Vector3 GetNextCornerToRotatoToward()
    {
        Vector3[] allCorners = navMeshAgent.path.corners;

        for (int i = 0; i < allCorners.Length; i++)
        {
            if (!visitedCorners.Contains(allCorners[i]))
            {
                return allCorners[i];
            }
        }
        return bodyUp;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(bodyPos, bodyPos + bodyRight);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(bodyPos, bodyPos + bodyUp);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(bodyPos, bodyPos + bodyForward);
    }
}
