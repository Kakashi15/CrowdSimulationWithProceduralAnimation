using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.AI;

public class NavMeshAgentStats : MonoBehaviour
{
    public Transform finalDestination;  // The final destination for the NavMeshAgent
    public float dataCollectionInterval = 1.0f; // Time interval for data collection (in seconds)
    public string outputFilePath = "AgentStats.csv";

    private NavMeshAgent agent;
    private float elapsedTime = 0.0f;
    private float startTime = 0.0f;
    private float totalDistance = 0.0f;
    private bool destinationReached = false;

    // Headers for the CSV file
    private const string CSV_HEADER = "Time,TimeToReachDestination,TotalDistance";

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Check if the CSV file exists; if not, write the headers
        string filePath = Path.Combine(Application.persistentDataPath, outputFilePath);
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, CSV_HEADER + "\n");
        }
    }

    private void Update()
    {
        // Check if the agent has reached the final destination
        if (!destinationReached && agent.remainingDistance <= agent.stoppingDistance)
        {
            destinationReached = true;
            float timeToReachDestination = Time.time - startTime;

            // Log the statistics
            Debug.Log($"Time to Reach Destination: {timeToReachDestination} seconds");
            Debug.Log($"Total Distance Covered: {totalDistance} units");

            // Save data to CSV
            SaveDataToCSV(Time.time, timeToReachDestination, totalDistance);

            // Reset for the next calculation
            elapsedTime = 0.0f;
        }

        elapsedTime += Time.deltaTime;

        // Record the start time when the agent starts moving
        if (!destinationReached && agent.velocity.magnitude > 0.01f)
        {
            startTime = Time.time;
        }

        // Calculate the total distance covered by the agent
        if (!destinationReached)
        {
            totalDistance += Vector3.Distance(transform.position, transform.position + agent.velocity * Time.deltaTime);
        }
    }

    private void SaveDataToCSV(float time, float timeToReachDestination, float totalDistance)
    {
        // Construct the CSV data string
        string csvData = $"{time},{timeToReachDestination},{totalDistance}\n";
        string filePath = Path.Combine(Application.persistentDataPath, outputFilePath);

        // Append data to the CSV file
        File.AppendAllText(filePath, csvData, Encoding.UTF8);
    }
}
