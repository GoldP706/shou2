using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControllerNew : MonoBehaviour
{
    public bool grabbing = false;
    public GameObject closestObj;
    public GameObject heldObj;

    [SerializeField]FingerController thumb;
    [SerializeField]FingerController pointer;
    [SerializeField]FingerController middle;
    [SerializeField]FingerController ring;
    [SerializeField]FingerController little;

    private float handHeight = 1f;

    //private bool holdingObj = false;

    void Start()
    {
        
    }

    void Update()
    {
        Debug.Log(grabbing);
        //z movement 
        if(Input.GetKey(KeyCode.Mouse0)&&handHeight>0f){
            handHeight -= 0.1f;
        }
        
        if(Input.GetKey(KeyCode.Mouse1)&&handHeight>1f){
            handHeight += 0.1f;
        }

        //grab and move
        if(thumb.isGrabbing)
        {
            if(pointer.isGrabbing || middle.isGrabbing || ring.isGrabbing || little.isGrabbing)
            {
                grabbing = true;
            }
            else{grabbing = false;}
        }
        else{grabbing = false;}
        
        if(closestObj != null && grabbing)
        {
            heldObj = closestObj;
            heldObj.GetComponent<TargetJoint2D>().target = transform.position;
        }
        else{
            heldObj = null;
        }  

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "CanGrab" && closestObj == null && grabbing == false)
        {
            closestObj = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == "CanGrab")
        {
            closestObj = null;
        }
    }
}
