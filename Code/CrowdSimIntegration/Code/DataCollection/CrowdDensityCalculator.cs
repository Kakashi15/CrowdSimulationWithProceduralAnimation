using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.AI;

public class CrowdDensityCalculator : MonoBehaviour
{
    public Transform exitPoint; // The point representing the exit
    public float dataCollectionInterval = 1.0f; // Time interval for data collection (in seconds)
    public string outputFilePath = "CrowdData.csv";

    private float elapsedTime = 0.0f;
    private List<NavMeshAgent> pedestrians = new List<NavMeshAgent>();
    private BoxCollider detectionCollider;
    private const string CSV_HEADER = "Time,Density,FlowRate";

    private int pedestriansExited = 0;

    private void Start()
    {
        // Check if the CSV file exists; if not, write the headers
        string filePath = Path.Combine(Application.persistentDataPath, outputFilePath);
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, CSV_HEADER + "\n");
        }

        detectionCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= dataCollectionInterval)
        {
            float colliderArea = detectionCollider.size.x * detectionCollider.size.z;
            float pedestrianDensity = pedestrians.Count / colliderArea;

            // Calculate the flow rate (persons/min)
            float flowRate = pedestriansExited / (elapsedTime / 60); // elapsedTime is in seconds, so convert to minutes

            // Log the density and flow rate
            Debug.Log($"Density: {pedestrianDensity}, Flow Rate (persons/min): {flowRate}");

            // Save data to CSV
            SaveDataToCSV(Time.time, pedestrianDensity, flowRate);

            // Reset elapsed time and pedestrians exited count
            elapsedTime = 0;
            pedestriansExited = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            NavMeshAgent pedestrian = other.gameObject.transform.parent.GetComponent<NavMeshAgent>();
            if (pedestrians.Contains(pedestrian))
            {
                pedestrians.Remove(pedestrian);
                pedestriansExited++;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            NavMeshAgent pedestrian = other.gameObject.transform.parent.GetComponent<NavMeshAgent>();
            if (!pedestrians.Contains(pedestrian))
            {
                pedestrians.Add(pedestrian);
            }
        }
    }

    private void SaveDataToCSV(float time, float density, float flowRate)
    {
        // Construct the CSV data string
        string csvData = $"{time},{density},{flowRate}\n";
        string filePath = Path.Combine(Application.persistentDataPath, outputFilePath);

        // Append data to the CSV file
        File.AppendAllText(filePath, csvData, Encoding.UTF8);
    }
}
