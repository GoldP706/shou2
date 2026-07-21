using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControllerNew : MonoBehaviour
{
    public bool grabbing = false;
    public GameObject closestObj;
    public GameObject heldObj;

    private AudioSource audioSource;
    private bool sfxPlayed = false;

    public int handState = 0; //0 = flat, 1 = side, 2 = type
    private bool stateSwitched = false;

    [SerializeField]MouseFollowRandomDrift mouseFollow;

    [SerializeField]FingerController thumb;
    [SerializeField]FingerController pointer;
    [SerializeField]FingerController middle;
    [SerializeField]FingerController ring;
    [SerializeField]FingerController little;

    private float handHeight = 1f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        //Debug.Log(stateSwitched);
        //switch hand state
        if(Input.mouseScrollDelta.y > 2f && !stateSwitched){
            handState += 1;
            if(handState>2){handState = 0;}
            stateSwitched = true;
        }
        if(Input.mouseScrollDelta.y < -2f && !stateSwitched){
            handState -= 1;
            if(handState<0){handState = 2;}
            stateSwitched = true;
        }
        if(Input.mouseScrollDelta.y == 0f){
            stateSwitched = false;
        }

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
            sfxPlayed = false;
        }  

        if(heldObj != null && !sfxPlayed){
            audioSource.Play();
            sfxPlayed = true;
        }

        //elbow movement
        if(Input.GetKeyDown(KeyCode.Mouse1)){
            

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
