using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SliderValueDisplay : MonoBehaviour
{
    Slider slider;
    TextMeshProUGUI textMeshProUGUI;
    void Start()
    {
        slider = GetComponentInParent<Slider>();
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        textMeshProUGUI.SetText(slider.value.ToString("0"));
    }
}
