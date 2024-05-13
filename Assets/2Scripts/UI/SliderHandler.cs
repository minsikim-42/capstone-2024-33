using UnityEngine;
using UnityEngine.UI;

public class SliderHandler : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetMaxValue(float value)
    {
        slider.maxValue = value; // Slider의 value 설정
    }

    public void SetValue(float value)
    {
        slider.value = value; // Slider의 value 설정
    }

    public float GetValue()
    {
        return slider.value;
    }
}
