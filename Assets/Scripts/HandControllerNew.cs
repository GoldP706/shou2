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

    private bool holdingObj = false;

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

        //grab and move
        if(thumb.isGrabbing && !holdingObj)
        {
            if(pointer.isGrabbing || middle.isGrabbing || ring.isGrabbing || little.isGrabbing)
            {
                grabbing = true;
                if(closestObj != null)
                {
                    closestObj.GetComponent<TargetJoint2D>().target = transform.position;
                    holdingObj = true;
                }
            }
        }
        else
        {
            grabbing = false;
            holdingObj = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "CanGrab" && closestObj == null)
        {
            closestObj = other.gameObject;
        }
    }
}
