using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjCollideSFX : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip collideSFX;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }
}
