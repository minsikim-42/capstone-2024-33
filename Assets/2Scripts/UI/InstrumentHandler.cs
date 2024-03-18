using TMPro;
using UnityEngine;

public class InstrumentHandler : MonoBehaviour
{
    [SerializeField] private Transform line; // 라인
    [SerializeField] private TextMeshProUGUI text; // 값 텍스트

    private int direction; // 방향
    
    public void SetHorizontal(float value, int direction)
    {
        // direction에 따라 값과 텍스트를 변경
        if (direction == 1)
        {
            value = -value; // 방향이 1이면 값 부호 변경
            
            text.text = value.ToString("F0"); // 텍스트 변경
        }
        else
        {
            text.text = (-value).ToString("F0"); // 텍스트 변경
        }
        
        line.localRotation = Quaternion.Euler(0, 0, value); // 라인의 회전값 변경
    }

    public void SetAngle(float value, int direction)
    {
        line.localScale = new Vector3(direction, 1, 1); // 방향에 따라 라인의 스케일 변경
        line.localRotation = Quaternion.Euler(0, 0, value * direction); // 라인의 회전값 변경

        this.direction = direction; // 방향 설정
        
        angleVector = line.right; // 라인의 오른쪽 벡터를 angleVector에 저장
        
        if (direction == -1)
        {
            angleVector.y = -angleVector.y; // 방향이 -1이면 angleVector의 y값 부호 변경
            
            // 각도 z값이 180보다 크면 360에서 빼고, 아니면 부호 변경해서 angle에 저장
            if (line.eulerAngles.z > 180) 
                angle = 360 - line.eulerAngles.z;
            else
                angle = -line.eulerAngles.z;
        }
        else
        {
            angle = line.eulerAngles.z; // 각도 z값을 angle에 저장
        }

        text.text = value.ToString("F0"); // 텍스트 변경
    }

    private float angle; // 각도
    
    public float GetAngle()
    {
        return angle;
    }

    private Vector3 angleVector; // 각도 벡터
    
    public Vector3 GetAngleVector()
    {
        // 방향에 따라 angleVector의 x값 부호 변경
        if (direction == -1)
        {
            angleVector.x = -angleVector.x;
            
            return angleVector;
        }
        else
        {
            return angleVector;
        }
    }
}
