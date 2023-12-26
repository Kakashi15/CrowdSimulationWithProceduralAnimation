using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class DataManager : MonoBehaviour
{
    private static Dictionary<string, List<PedestrianData>> pedestrianData = new Dictionary<string, List<PedestrianData>>();

    public static void RecordPedestrianData(string pedestrianID, Vector3 position, Vector3 velocity, float timestamp)
    {
        // Create a new PedestrianData object and add it to the list
        PedestrianData dataPoint = new PedestrianData(position, velocity, timestamp);

        if (!pedestrianData.ContainsKey(pedestrianID))
        {
            pedestrianData[pedestrianID] = new List<PedestrianData>();
        }

        pedestrianData[pedestrianID].Add(dataPoint);
    }

    // Add a method to save data to a CSV file

    public static void SavePedestrianDataToCSV()
    {
        string fileName = GenerateFileName(); // Generate a unique filename

        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write the header row
            writer.WriteLine("PedestrianID,PositionX,PositionY,PositionZ,VelocityX,VelocityY,VelocityZ,Timestamp");

            foreach (var entry in pedestrianData)
            {
                string pedestrianID = entry.Key;
                List<PedestrianData> dataPoints = entry.Value;

                foreach (PedestrianData dataPoint in dataPoints)
                {
                    // Write data for each pedestrian
                    string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                        pedestrianID,
                        dataPoint.Position.x,
                        dataPoint.Position.y,
                        dataPoint.Position.z,
                        dataPoint.Velocity.x,
                        dataPoint.Velocity.y,
                        dataPoint.Velocity.z,
                        dataPoint.Timestamp);

                    writer.WriteLine(line);
                }
            }
        }
    }

    private static string GenerateFileName()
    {
        // Generate a unique filename based on a timestamp
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        return "PedestrianData_" + timestamp + ".csv";
    }
}

public class PedestrianData
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Timestamp;

    public PedestrianData(Vector3 pedPos, Vector3 pedVel, float t)
    {
        Position = pedPos;
        Velocity = pedVel;
        Timestamp = t;
    }
}