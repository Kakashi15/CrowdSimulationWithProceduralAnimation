using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Linq;

public class EntityManager : MonoBehaviour
{
    ObjectSelector objectSelector;
    CrowdEntity[] crowdEntity;
    [SerializeField] private Slider minValSlider; // Reference to the UI Slider for minVal
    [SerializeField] private Slider maxValSlider; // Reference to the UI Slider for maxVal

    [SerializeField] private Slider LegValueZetaMin;
    [SerializeField] private Slider LegValueZetaMax;

    [SerializeField] private Slider armValueZetaMin;
    [SerializeField] private Slider armValueZetaMax;

    [SerializeField] private Slider LegValueRangeMin;
    [SerializeField] private Slider LegValueRangeMax;

    [SerializeField] private Slider armValueRangeMin;
    [SerializeField] private Slider armValueRangeMax;

    [SerializeField] private Slider posAdjustValue;

    [SerializeField] private bool showVision;
    private float minVal;
    private float maxVal;

    void Start()
    {
        objectSelector = FindObjectOfType<ObjectSelector>();
        crowdEntity = objectSelector.selectableObjects.ToArray();

        // Initialize minVal and maxVal with slider values
        minVal = minValSlider.value;
        maxVal = maxValSlider.value;

        foreach (CrowdEntity entity in crowdEntity)
        {
            NavMeshAgent agent = entity.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                float randomSpeed = Random.Range(minVal, maxVal);
                agent.speed = randomSpeed;
            }
        }

        UpdateAgentValues(); // Call the function to set initial agent values
    }

    // Function to update minVal, maxVal, and agent values
    public void UpdateAgentValues()
    {
        // Update minVal and maxVal with slider values
        minVal = minValSlider.value;
        maxVal = maxValSlider.value;
        crowdEntity = objectSelector.selectableObjects.FindAll((x) => x.IsSelected()).ToArray();

        foreach (CrowdEntity entity in crowdEntity)
        {
            NavMeshAgent agent = entity.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                float randomSpeed = Random.Range(minVal, maxVal);
                agent.speed = randomSpeed;
            }

            LegController legController = entity.legController;
            if (legController != null)
            {
                if (agent.speed > 0)
                    legController.PosAdjustRatio = agent.speed / 100;
                float randomZetaValLeft = Random.Range(LegValueZetaMin.value, LegValueZetaMax.value);
                legController.zl = randomZetaValLeft;

                float randomRangeValLeft = Random.Range(LegValueRangeMin.value, LegValueRangeMax.value);
                legController.rl = randomRangeValLeft;

                float randomArmZetaValue = Random.Range(armValueZetaMin.value, armValueZetaMax.value);
                legController.zr = randomArmZetaValue;

                float randomArmRandomValue = Random.Range(armValueRangeMin.value, armValueRangeMax.value);
                legController.rr = randomArmRandomValue;
            }
        }
    }

    // Attach this function to UI slider OnValueChanged events in the inspector
    public void OnMinValSliderChanged()
    {
        UpdateAgentValues();
    }

    // Attach this function to UI slider OnValueChanged events in the inspector
    public void OnMaxValSliderChanged()
    {
        UpdateAgentValues();
    }

    // Update is called once per frame
    void Update()
    {
        // Your other update logic here
    }
}
