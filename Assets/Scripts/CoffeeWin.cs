using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeWin : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField]CoffeeCupBehavior coffeeCupBehavior;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == ("CoffeeCup")&&!coffeeCupBehavior.spilled)
        {
            audioSource.Play();
            Debug.Log("Coffee drank Dark Souls text");
        }
    }
}
