using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawnOnClick : MonoBehaviour
{
    public GameObject prefabToSpawn; // Drag and drop your prefab in the Unity Inspector.

    public LayerMask groundLayer;

    public ObjectSelector objectSelector;

    void Update()
    {
        // Cast a ray from the mouse position into the scene
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // Get the position of the ray hit point
            Vector3 worldPosition = hit.point;


            // Check for mouse button click to spawn the actual prefab
            if (Input.GetKeyDown(KeyCode.Space)) // Change 0 to the desired button index (0 for left mouse button).
            {
                // Spawn the specified prefab at the ray hit point
                CrowdEntity entity = Instantiate(prefabToSpawn, worldPosition, Quaternion.identity).GetComponentInChildren<CrowdEntity>();
                objectSelector.selectableObjects.Add(entity);
            }
        }
    }
}

