using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeCupBehavior : MonoBehaviour
{
    public GameObject hand;
    private HandControllerNew handController;
    private Transform tr;
    [SerializeField]GameObject coffee;
    private SpriteRenderer coffeeSpr;
    private AudioSource audioSource;
    public bool spilled = false;
    private bool sfxPlayed = false;
    [SerializeField] float killTimer = 3f;

    void Start()
    {
        handController = hand.GetComponent<HandControllerNew>();
        tr = GetComponent<Transform>();
        coffeeSpr = coffee.GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(handController.handState == 1){
            gameObject.tag = "CanGrab";
        }
        else{
            gameObject.tag = "Untagged";
        }

        if(tr.rotation.eulerAngles.z > 90f || tr.rotation.eulerAngles.z < -90f){
            coffeeSpr.enabled = false;
            spilled = true;
        }
        if(spilled){
            if(!sfxPlayed){
                audioSource.Play();
                sfxPlayed = true;
            }
            killTimer -= Time.deltaTime;
            if(killTimer <= 0f){
            }
        }
        
    }
}
