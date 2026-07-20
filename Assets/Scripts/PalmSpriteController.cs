using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmSpriteController : MonoBehaviour
{
    [SerializeField]HandControllerNew handController;
    public Sprite flat;
    public Sprite side;

    private SpriteRenderer spr;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if(handController.handState == 0)//flat state
        {
            spr.sprite = flat;
        }
        else if(handController.handState == 1)//side state
        {
            spr.sprite = side;
        }
    }
}
