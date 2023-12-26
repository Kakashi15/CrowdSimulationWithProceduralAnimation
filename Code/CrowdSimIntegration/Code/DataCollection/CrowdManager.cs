using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.AI;

public class CrowdManager : MonoBehaviour
{
    public Transform exitPoint1;
    public Transform exitPoint2;
    public Collider exitCollider;

    private List<Transform> crowdEntities;
    private int density;
    private int exitCount1;
    private int exitCount2;
    private List<string> dataRows = new List<string>();
    private float timeElapsed = 0f;

    private void Start()
    {
        crowdEntities = new List<Transform>();

        // Find all entities in the scene with NavMeshAgent components.
        NavMeshAgent[] agents = FindObjectsOfType<NavMeshAgent>();
        foreach (NavMeshAgent agent in agents)
        {
            crowdEntities.Add(agent.transform);
        }
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        // Calculate the density by counting entities within the exit collider.
        density = CountEntitiesInCollider(exitCollider);

        // Track the number of entities passing through each exit point.
        exitCount1 = CountEntitiesInExitPoint(exitPoint1);
        exitCount2 = CountEntitiesInExitPoint(exitPoint2);

        // Store the data row with time, density, exitCount1, and exitCount2.
        string dataRow = $"{timeElapsed},{density},{exitCount1},{exitCount2}";
        dataRows.Add(dataRow);
    }

    private int CountEntitiesInCollider(Collider collider)
    {
        int count = 0;
        foreach (Transform entity in crowdEntities)
        {
            if (collider.bounds.Contains(entity.position))
            {
                count++;
            }
        }
        return count;
    }

    private int CountEntitiesInExitPoint(Transform exitPoint)
    {
        int count = 0;
        foreach (Transform entity in crowdEntities)
        {
            if (Vector3.Distance(entity.position, exitPoint.position) < 1.0f) // Adjust the distance threshold as needed
            {
                count++;
            }
        }
        return count;
    }

    // At the end of your simulation or when needed, you can call this method to save the data to a CSV file.
    public void SaveDataToCSV()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "simulation_data2.csv");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Time,Density,ExitCount1,ExitCount2");

            foreach (string dataRow in dataRows)
            {
                writer.WriteLine(dataRow);
            }
        }
    }
}
