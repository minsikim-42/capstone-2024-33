using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInputManager : MonoBehaviour
{
    Vector3 position;

	[SerializeField] GameObject left; // 왼쪽 버튼
	[SerializeField] GameObject right; // 오른쪽 버튼
	[SerializeField] GameObject up; // 위 버튼
	[SerializeField] GameObject down; // 아래 버튼
    [SerializeField] GameObject space; // 스페이스 버튼

    int fingerId;
    bool isTouch;

    bool isLeft;
    bool isRight;
    bool isUp;
    bool isDown;

    void Start()
    {

    }

    void Update()
    {
        isLeft = false;
        isRight = false;
        isUp = false;
        isDown = false;

        for (int i=0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            switch (touch.phase)
            {
                case TouchPhase.Began: // at first
                    
                    break;
                case TouchPhase.Moved: // moving
                    if (fingerId == touch.fingerId)
                    {
                        
                    }
                    break;
                case TouchPhase.Ended: // del
                    if (fingerId == touch.fingerId)
                    {
                        fingerId = -1;
                    }
                    break;
            }
        }
    }
}