using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectSelector : MonoBehaviour
{
    public List<CrowdEntity> selectableObjects;
    public List<CrowdEntityAlt> selectableObjectsAlt;
    private bool objectsSelected = false;
    private bool objectCanMove = false;
    public Vector3[] targetPosition;
    public Vector3 oneTargetPos;
    public float desitinationPositionGeneratorRadius = 3f;
    public bool useAltCrowdEnity;

    private Vector3 mousePosition1;
    private Vector3 mousePosition2Update;
    private Vector3 mousePosition2;

    [SerializeField] bool moveForwardOnly = false;

    [SerializeField] Slider destinationPointSlider;

    Rect selectionRect;
    Texture2D selectionTexture;
    Color selectionColor;
    bool drawSelectionBox;
    private void Start()
    {
        selectableObjects = Resources.FindObjectsOfTypeAll<CrowdEntity>().ToList();

        foreach (CrowdEntity selectable in selectableObjects)
        {
            selectable.moveForward = moveForwardOnly;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePosition1 = Input.mousePosition;
            objectsSelected = false;
            drawSelectionBox = true;


            if (!UIClick())
                DeselectAll();
        }

        if (Input.GetMouseButton(0))
        {
            mousePosition2Update = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            mousePosition2 = Input.mousePosition;
            drawSelectionBox = false;
            SelectObjects();
        }

        if (objectsSelected && Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                Tree tree = hit.transform.GetComponent<Tree>();
                if (tree == null)
                    targetPosition = GenerateRandomPositionsInCircle(hit.point, desitinationPositionGeneratorRadius, selectableObjects.Count((x) => x.IsSelected()));
                oneTargetPos = hit.point;
            }
            objectCanMove = true;

            List<CrowdEntity> selectedCrowdEntity = selectableObjects.Where((x) => x.IsSelected()).ToList();
        }

        if (objectCanMove && targetPosition != null)
        {
            MoveAllSelectedObjects();
        }
        if (mousePosition1 != mousePosition2Update)
            selectionRect = GetSelectionRect(mousePosition1, mousePosition2Update);

        desitinationPositionGeneratorRadius = destinationPointSlider.value;
    }

    private bool UIClick()
    {
        // Create a pointer event data with the current mouse position
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // Create a list to store the results of the raycast
        List<RaycastResult> results = new List<RaycastResult>();

        // Raycast to find UI elements at the click position
        EventSystem.current.RaycastAll(eventData, results);

        // Check if any UI elements were hit
        if (results.Count > 0)
        {
            // Handle UI click
            return true;
        }
        else
        {
            // Handle non-UI click
            return false;
        }
    }

    public static Vector3[] GenerateRandomPositionsInCircle(Vector3 targetPos, float radius, int posCount)
    {
        Vector3[] positions = new Vector3[posCount];

        for (int i = 0; i < posCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2); // Random angle in radians
            float distance = Random.Range(0f, radius);

            // Calculate position using polar coordinates
            float x = targetPos.x + distance * Mathf.Cos(angle);
            float z = targetPos.z + distance * Mathf.Sin(angle);

            positions[i] = new Vector3(x, targetPos.y, z);
        }

        return positions;
    }

    void DeselectAll()
    {
        if (useAltCrowdEnity)
        {
            foreach (CrowdEntityAlt selectable in selectableObjectsAlt)
            {
                selectable.Deselect();
                objectsSelected = false;
                objectCanMove = false;
                //targetPosition = Vector3.negativeInfinity;
            }
        }
        else
        {
            foreach (CrowdEntity selectable in selectableObjects)
            {
                selectable.Deselect();
                objectsSelected = false;
                objectCanMove = false;
                //targetPosition = Vector3.negativeInfinity;
            }
        }
    }

    void SelectObjects()
    {
        Bounds selectionBounds = GetViewportBounds(mousePosition1, mousePosition2);
        if (useAltCrowdEnity)
        {
            foreach (CrowdEntityAlt selectable in selectableObjectsAlt)
            {
                if (selectionBounds.Contains(Camera.main.WorldToViewportPoint(selectable.transform.position)))
                {
                    selectable.GetComponent<CrowdEntityAlt>().Select();
                    objectsSelected = true;
                }
            }
        }
        else
        {
            foreach (CrowdEntity selectable in selectableObjects)
            {
                if (selectionBounds.Contains(Camera.main.WorldToViewportPoint(selectable.transform.position)))
                {
                    selectable.GetComponent<CrowdEntity>().Select();
                    objectsSelected = true;
                }
            }
        }


        //RaycastHit hit;
        //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
        //{
        //    if (hit.transform.gameObject.GetComponent<CrowdEntity>() != null)
        //    {
        //        hit.transform.gameObject.GetComponent<CrowdEntity>().Select();
        //        objectsSelected = true;
        //    }
        //    //navMeshAgent.SetDestination(hit.point);
        //}

    }

    void OnMouseDown()
    {

    }

    void MoveAllSelectedObjects()
    {
        if (useAltCrowdEnity)
        {
            foreach (CrowdEntityAlt selectable in selectableObjectsAlt)
            {

                if (selectable.GetComponent<CrowdEntityAlt>().IsSelected())
                {
                    selectable.GetComponent<CrowdEntityAlt>().setMovementDestination(targetPosition[0]);
                }
            }
        }

        else
        {
            List<CrowdEntity> selectedCrowdEntity = selectableObjects.Where((x) => x.IsSelected()).ToList();

            for (int i = 0; i < selectedCrowdEntity.Count; i++)
            {
                int positionIndex = i > targetPosition.Length - 1 ? 0 : i;
                selectedCrowdEntity[i].GetComponent<CrowdEntity>().TargetPosition = targetPosition[positionIndex];
            }

        }
    }

    void MoveObjects()
    {
        foreach (CrowdEntity selectable in selectableObjects)
        {
            if (selectable.GetComponent<Renderer>().material.color == Color.green)
            {
                selectable.GetComponent<NavMeshAgent>().SetDestination(targetPosition[0]);
                //selectable.transform.position = Vector3.MoveTowards(selectable.transform.position, targetPosition, Time.deltaTime * 5f);
            }
        }
    }

    Bounds GetViewportBounds(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        Vector3 lowerLeft = Camera.main.ScreenToViewportPoint(screenPosition1);
        Vector3 upperRight = Camera.main.ScreenToViewportPoint(screenPosition2);

        Vector3 min = Vector3.Min(lowerLeft, upperRight);
        Vector3 max = Vector3.Max(lowerLeft, upperRight);

        min.z = Camera.main.nearClipPlane;
        max.z = Camera.main.farClipPlane;

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);

        return bounds;
    }

    private void OnGUI()
    {
        // Create a Rect from the mouse positions


        // Draw the selection rectangle
        //DrawSelectionRect(selectionRect);
    }

    private Rect GetSelectionRect(Vector3 startPosition, Vector3 endPosition)
    {
        // Calculate the top-left position and size of the rectangle
        Vector3 topLeft = Vector3.Min(startPosition, endPosition);
        Vector3 bottomRight = Vector3.Max(startPosition, endPosition);
        Vector3 size = bottomRight - topLeft;

        // Create a Rect from the calculated values
        return new Rect(topLeft.x, Screen.height - topLeft.y, size.x, -size.y);
    }

    private void DrawSelectionRect(Rect rect)
    {
        // Set the color and transparency for the selection rectangle
        selectionColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);
        if (drawSelectionBox && mousePosition1 != mousePosition2Update)
        {
            selectionTexture = new Texture2D(1, 1);
            selectionTexture.SetPixel(0, 0, selectionColor);
            selectionTexture.Apply();
            GUI.DrawTexture(rect, selectionTexture);
        }
        else
        {
            rect = new Rect(0, 0, 0, 0);
            selectionTexture = new Texture2D(0, 0);
            selectionTexture.SetPixel(0, 0, selectionColor);
            selectionTexture.Apply();
            GUI.DrawTexture(rect, selectionTexture);
        }
    }
}
