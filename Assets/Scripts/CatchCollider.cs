using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchCollider : MonoBehaviour
{
    [SerializeField] HandControllerNew handController;
    void Start()
    {
        
    }

    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "fly" && handController.handState == 0)
        {
            if(thumb.isGrabbing)
            {   
                if(pointer.isGrabbing || middle.isGrabbing || ring.isGrabbing || little.isGrabbing)
                {

                }
            }
        }
    }
}
