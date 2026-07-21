using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillCollider : MonoBehaviour
{
    [SerializeField] AudioClip coffeeCrash;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other){
        Destroy(other.gameObject);
        if(other.gameObject.name == "CoffeeCup"){
            audioSource.Play();
        }
    }
}