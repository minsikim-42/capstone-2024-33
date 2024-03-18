using UnityEngine;

public class PowerSlider : SliderHandler
{
    [SerializeField] private RectTransform previousPower;
    [SerializeField] private RectTransform fillRect;
    
    public void SetPreviousPower()
    {
        // 이전 파워의 x값을 fillRect의 width값으로 변경
        var temp = previousPower.anchoredPosition;
            temp.x = fillRect.rect.width;
        previousPower.anchoredPosition = temp;
    }
}
