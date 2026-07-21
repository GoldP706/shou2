using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlySting : MonoBehaviour
{
    public float stingWaitTimer; // time until next sting
    public float stingWaitTimerMax;
    public float stingingTimer; // time until sting is completed
    public float stingingTimeMax;
    private int elbowCount = 0;
    private bool isStinging = false;
    [SerializeField]SpriteRenderer stingWarningSpr;
    [SerializeField]FlyMovement flyMovement;

    void Start()
    {
        stingWaitTimer = stingWaitTimerMax;
    }

    void Update()
    {
        if(!flyMovement.isDead){
            stingWaitTimer -= Time.deltaTime;
            if(stingWaitTimer<=0){
                Sting();
                stingWaitTimer = stingWaitTimerMax;
            }
        }
        if(isStinging){
            stingingTimer += Time.deltaTime;

        }

        if(Input.GetKeyDown(KeyCode.Mouse0) && isStinging){
            elbowCount += 1;
        }
    }

    void Sting(){
        isStinging = true;
    }
}
