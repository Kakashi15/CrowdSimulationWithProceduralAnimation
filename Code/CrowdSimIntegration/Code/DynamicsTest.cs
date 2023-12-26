using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicsTest : MonoBehaviour
{
    public Transform followTarget;

    public Vector3 offset;
    [Range(0.0f, 10.0f)]
    public float f;
    [Range(0.0f, 1.0f)]
    public float z;
    [Range(-10f, 10.0f)]
    public float r;

    private Vector3 lastPosition;
    private float lastTime;

    public SecondOrderDynamics secondOrderDynamics;
    private void Start()
    {
        lastPosition = followTarget.position;
        lastTime = Time.time;
        secondOrderDynamics = new SecondOrderDynamics(f, z, r, followTarget.position);
    }

    private void Update()
    {
        float deltaTime = Time.time - lastTime;
        secondOrderDynamics.ValueUpdate(f, z, r);
        // Calculate position change
        Vector3 positionChange = followTarget.position - lastPosition;

        // Calculate velocity using position change and time passed
        Vector3 velocity = positionChange / Time.deltaTime;

        Vector3 newPos = secondOrderDynamics.Update(Time.deltaTime, followTarget.position, Vector3.zero);

        transform.position = newPos + offset;
        // Update last position and time
        lastPosition = followTarget.position;
        lastTime = Time.time;
    }


}
