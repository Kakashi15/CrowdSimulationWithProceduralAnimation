using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Color minColor = Color.green;
    public Color maxColor = Color.red;
    public float maxSpeed = 10.0f; // Adjust the maximum speed of the entity
    public int maxPoints = 50; // Adjust the maximum number of points in the line

    private NavMeshAgent navMeshAgent;
    private List<Vector3> linePoints = new List<Vector3>();

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
        }
    }

    private void Update()
    {
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        // Get the entity's velocity and normalize it
        Vector3 velocity = navMeshAgent.velocity;
        float speed = Mathf.Clamp01(velocity.magnitude / navMeshAgent.speed);

        // Calculate the color based on speed
        Color currentColor = Color.Lerp(maxColor, minColor, speed);

        // Update the color of the Line Renderer
        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;

        // Calculate the new end position of the Line Renderer
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - velocity.normalized * navMeshAgent.radius;

        // Raycast down to find the ground height
        RaycastHit hit;
        if (Physics.Raycast(endPos, Vector3.down, out hit, Mathf.Infinity, navMeshAgent.areaMask))
        {
            endPos = hit.point;
        }

        // Add the new point to the linePoints list
        linePoints.Insert(0, endPos);

        startPos.y = endPos.y;

        // Limit the number of points in the list
        if (linePoints.Count > maxPoints)
        {
            linePoints.RemoveAt(linePoints.Count - 1);
        }

        // Set the Line Renderer positions using the linePoints list
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }
}
