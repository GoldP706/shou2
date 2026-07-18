using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab : MonoBehaviour
{
    private bool grabCheck = false;
    private bool grabbing = false;
    public Vector3 grabCheckPos;
    private GameObject closestObj;

    private float handHeight = 1f;

    void Start()
    {
        
    }

    void Update()
    {
        //z movement
        if(Input.GetKey(KeyCode.Mouse0)&&handHeight>0f){
            handHeight -= 0.1f;
        }
        
        if(Input.GetKey(KeyCode.Mouse1)&&handHeight>1f){
            handHeight += 0.1f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "CanGrab")
        {
            closestObj = other.gameObject;
        }
    }
}
