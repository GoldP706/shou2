using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControllerNew : MonoBehaviour
{
    private bool grabbing = false;
    private GameObject closestObj;

    [SerializeField]FingerController thumb;
    [SerializeField]FingerController pointer;
    [SerializeField]FingerController middle;
    [SerializeField]FingerController ring;
    [SerializeField]FingerController little;

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

        //grabbing
        if(thumb.isGrabbing)
        {
            if(pointer.isGrabbing || middle.isGrabbing || ring.isGrabbing || little.isGrabbing)
            {
                grabbing = true;
                if(closestObj != null)
                {
                    closestObj.transform.position = transform.position;
                }
            }
        }
        else
        {
            grabbing = false;
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
