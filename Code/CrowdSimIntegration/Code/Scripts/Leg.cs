using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leg : MonoBehaviour
{
    // Self-explanatory variable names
    private LegController legController;

    [SerializeField] private Transform bodyTransform;
    [SerializeField] public Transform rayOrigin;
    public GameObject ikTarget;

    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private AnimationCurve heightCurve;

    [SerializeField] private float tipMaxHeight = 0.2f;
    private float tipAnimationTime = 0.15f;
    private float tipAnimationFrameTime = 1 / 60.0f;

    public float ikOffset = 1.0f;
    private float tipMoveDist = 0.55f;
    private float maxRayDist = 7.0f;
    private float tipPassOver = 0.55f / 2.0f;

    public Vector3 TipPos { get; private set; }
    public Vector3 TipUpDir { get; private set; }
    public Vector3 RaycastTipPos { get; private set; }
    public Vector3 RaycastTipNormal { get; private set; }

    public bool Animating = false;
    public bool Movable = false;
    public float TipDistance;

    public LayerMask layer;
    public float hieghtCurveTime;

    SecondOrderDynamics secondOrderDynamics;

    [Range(0.0f, 500.0f)]
    public float f;
    [Range(-5.0f, 5.0f)]
    public float z;
    [Range(-10f, 10.0f)]
    public float r;

    private void Awake()
    {
        legController = GetComponentInParent<LegController>();

        transform.parent = bodyTransform;
        rayOrigin.parent = bodyTransform;
        TipPos = ikTarget.transform.position;
    }

    private void Start()
    {
        secondOrderDynamics = new SecondOrderDynamics(f, z, r, TipPos);
        UpdateIKTargetTransform();
    }

    public void dynamicsUpdate(float freq,float zeta, float reta)
    {
        f = freq;
        z = zeta;
        r = reta;
    }

    private void Update()
    {
        RaycastHit hit;
        secondOrderDynamics.ValueUpdate(f, z, r);
        // Calculate the tip target position
        if (Physics.Raycast(rayOrigin.position, bodyTransform.up.normalized * -1, out hit, maxRayDist, layer))
        {
            RaycastTipPos = hit.point;
            RaycastTipNormal = hit.normal;
        }
        TipDistance = (RaycastTipPos - TipPos).magnitude;

        hieghtCurveTime += (Input.GetAxis("Vertical") * 0.01f);

        // If the distance gets too far, animate and move the tip to new position
        if (!Animating && (TipDistance > tipMoveDist && Movable))
        {
            StartCoroutine(AnimateLeg());
        }
    }

    private IEnumerator AnimateLeg()
    {
        Animating = true;

        float timer = 0.0f;
        float animTime;

        Vector3 startingTipPos = TipPos;
        Vector3 tipDirVec = RaycastTipPos - TipPos;
        tipDirVec += tipDirVec.normalized * tipPassOver;

        Vector3 right = Vector3.Cross(bodyTransform.up, tipDirVec.normalized).normalized;
        TipUpDir = Vector3.Cross(tipDirVec.normalized, right);

        while (timer < tipAnimationTime + tipAnimationFrameTime)
        {
            animTime = speedCurve.Evaluate(timer / tipAnimationTime);

            // If the target is keep moving, apply acceleration to correct the end point
            float tipAcceleration = Mathf.Max((RaycastTipPos - startingTipPos).magnitude / tipDirVec.magnitude, 1.0f);

            TipPos = startingTipPos + tipDirVec * tipAcceleration * animTime; // Forward direction of tip vector
            TipPos += TipUpDir * heightCurve.Evaluate(animTime) * tipMaxHeight; // Upward direction of tip vector

            UpdateIKTargetTransform();

            timer += tipAnimationFrameTime;

            yield return new WaitForSeconds(tipAnimationFrameTime);
        }

        Animating = false;
    }

    public Quaternion ikTargetRotationOffset;
    private void UpdateIKTargetTransform()
    {

        // Update leg ik target transform depend on tip information
        ikTarget.transform.position = TipPos + bodyTransform.up.normalized * ikOffset;
        Vector3 newPos = secondOrderDynamics.Update(Time.deltaTime, ikTarget.transform.position, Vector3.zero);
        newPos.y = TipPos.y + bodyTransform.up.normalized.y * ikOffset;
        ikTarget.transform.position = newPos;
        //ikTarget.transform.rotation = Quaternion.LookRotation(TipPos - ikTarget.transform.position, RaycastTipNormal) * ikTargetRotationOffset;/* * Quaternion.Euler(90, 0, 0)*/;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(RaycastTipPos, 0.1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(TipPos, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(TipPos, RaycastTipPos);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(ikTarget.transform.position, 0.1f);
    }
}
