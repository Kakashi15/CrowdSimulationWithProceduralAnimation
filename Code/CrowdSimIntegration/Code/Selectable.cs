using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    public Color selectedColor = Color.green;
    public Color deselectedColor = Color.white;
    public MeshRenderer objectRenderer;
    public List<MeshRenderer> allMeshRenderers = new List<MeshRenderer>();
    private bool isSelected = false;
    private bool colorSet = false;
    public void Awake()
    {
        Deselect();
    }

    public void Select()
    {
        isSelected = true;
        foreach (MeshRenderer mesh in allMeshRenderers)
        {
            mesh.material.color = selectedColor;
        }
        //objectRenderer.material.color = selectedColor;
    }
    private void FixedUpdate()
    {
        if (isSelected)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                foreach (MeshRenderer mesh in allMeshRenderers)
                {
                    mesh.material.color = Color.red;
                    deselectedColor = Color.red;
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                foreach (MeshRenderer mesh in allMeshRenderers)
                {
                    mesh.material.color = Color.blue;
                    deselectedColor = Color.blue;
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                foreach (MeshRenderer mesh in allMeshRenderers)
                {
                    mesh.material.color = Color.black;
                    deselectedColor = Color.black;
                }
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                foreach (MeshRenderer mesh in allMeshRenderers)
                {
                    mesh.material.color = Color.yellow;
                    deselectedColor = Color.yellow;
                }
            }
        }

    }
    private void Update()
    {

    }

    public void Deselect()
    {
        isSelected = false;
        foreach (MeshRenderer mesh in allMeshRenderers)
        {
            mesh.material.color = deselectedColor;
        }
        //objectRenderer.material.color = deselectedColor;
    }

    public bool IsSelected()
    {
        return isSelected;
    }

}
