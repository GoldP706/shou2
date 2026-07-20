using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeWin : MonoBehaviour
{

    void Start()
    {
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == ("CoffeeCup"))
        {
            Debug.Log("Coffee drank Dark Souls text");
        }
    }
}
