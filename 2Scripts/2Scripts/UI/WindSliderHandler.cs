using UnityEngine;

public class WindSliderHandler : MonoBehaviour
{
    private Transform windSlider; // windSlider의 Transform
    
    private void Awake()
    {
        windSlider = transform; // windSlider의 Transform 할당
    }

    public void SetWind(float wind)
    {
        windSlider.localScale = new Vector3(wind, 1, 1); // windSlider의 Scale을 변경하여 바람의 세기를 표현
    }
}
