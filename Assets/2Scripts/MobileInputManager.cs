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

    }
}