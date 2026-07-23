using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmSpriteController : MonoBehaviour
{
    [SerializeField]HandControllerNew handController;
    public Sprite flat;
    public Sprite side;
    public Sprite type;
    public Sprite flatElbow;
    public Sprite sideElbow;
    public Sprite typeElbow;

    private SpriteRenderer spr;
    private bool elbowing = false;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if(handController.handState == 0)//flat state
        {
            if(!elbowing){
                spr.sprite = flat;
            }
            else{
                spr.sprite = flatElbow;
            }
            spr.sortingOrder = 54;
        }
        else if(handController.handState == 1)//side state
        {
            if(!elbowing){
                spr.sprite = side;
            }
            else{
                spr.sprite = sideElbow;
            }
            spr.sortingOrder = 50;
        }
        else if(handController.handState == 2)//type state
        {
            if(!elbowing){
                spr.sprite = type;
            }
            else{
                spr.sprite = typeElbow;
            }
            spr.sortingOrder = 50;
        }

        if(Input.GetKeyDown(KeyCode.Mouse1)){
            elbowing = true;
        }
        if(Input.GetKeyUp(KeyCode.Mouse1)){
            elbowing = false;
        }
    }
}

