using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeWin : MonoBehaviour
{
    private AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == ("CoffeeCup"))
        {
            audioSource.Play();
            Debug.Log("Coffee drank Dark Souls text");
        }
    }
}
