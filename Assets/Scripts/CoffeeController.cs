using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeController : MonoBehaviour
{
    public GameObject hand;
    private HandControllerNew handController;

    void Start()
    {
        handController = hand.GetComponent<HandControllerNew>();
    }

    void Update()
    {
        
    }
}
