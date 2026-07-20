using UnityEngine;

public class FingerController : MonoBehaviour
{
    [SerializeField] KeyCode key;

    [SerializeField]HandControllerNew handController;

    public Sprite grab;
    public Sprite open;
    public Sprite sideGrab;
    public Sprite sideOpen;

    private Sprite openSprite;
    private Sprite grabSprite;


    [SerializeField] float holdTimerMax = 2f;
    float holdTimer = 0f;

    private SpriteRenderer spr;

    private bool Active;

    public bool isGrabbing;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();

    }

    void Update()
    {
        //detect hand state, set sprite to match state
        if(handController.handState == 0)//flat state
        {
            openSprite = open;
            grabSprite = grab;
        }
        else if(handController.handState == 1)//side state
        {
            openSprite = sideOpen;
            grabSprite = sideGrab;
        }

        if(Input.GetKey(KeyCode.RightShift))
        {
            Active = true;
        }
        else{
            Active = false;
        }

        if (Input.GetKey(key)&& Active && holdTimer<=holdTimerMax)
        {
            holdTimer -= Time.deltaTime;
            spr.sprite = grabSprite;
            isGrabbing = true;
        }
        else
        {
            spr.sprite = openSprite;
            holdTimer = holdTimerMax;
            isGrabbing = false;
        }
    }
}
