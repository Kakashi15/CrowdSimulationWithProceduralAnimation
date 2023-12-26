using UnityEngine;
using UnityEngine.AI;

public class PedestrianDataCollector : MonoBehaviour
{
    public string pedestrianID; // Unique identifier for each pedestrian
    private Transform pedestrianTransform;
    private NavMeshAgent pedestrainNavAgent;
    public bool IsInsideRadius;
    private void Start()
    {
        // Initialize data collection variables
        pedestrianID = gameObject.transform.parent.name;
        pedestrianTransform = GetComponent<Transform>();
        pedestrainNavAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Record data at regular intervals or as needed
        Vector3 currentPosition = pedestrainNavAgent.transform.position;
        Vector3 currentVelocity = pedestrainNavAgent.velocity;

        // Store the data or send it to a data manager for storage
        DataManager.RecordPedestrianData(pedestrianID, currentPosition, currentVelocity, Time.time);
    }
}