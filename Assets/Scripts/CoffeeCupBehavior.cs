using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeCupBehavior : MonoBehaviour
{
    public GameObject hand;
    private HandControllerNew handController;
    private Transform tr;

    void Start()
    {
        handController = hand.GetComponent<HandControllerNew>();
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        Debug.Log("Hand state: " + handController.handState);
        if(handController.handState == 1){
            gameObject.tag = "CanGrab";
        }
        else{
            gameObject.tag = "Untagged";
        }
    }
}
