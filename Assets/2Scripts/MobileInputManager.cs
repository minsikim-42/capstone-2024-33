using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInputManager : MonoBehaviour
{
    Vector3 position;

	[SerializeField] GameObject left;
	[SerializeField] GameObject right;
	[SerializeField] GameObject up;
	[SerializeField] GameObject down;
    [SerializeField] GameObject space;

    int fingerId;
    bool isTouch;

    void Start()
    {

    }

    void Update()
    {

    }
}