using UnityEngine;

public class FingerController : MonoBehaviour
{
    [SerializeField] KeyCode key;

    [SerializeField] Sprite grab;
    [SerializeField] Sprite open;

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
            spr.sprite = grab;
            isGrabbing = true;
        }
        else
        {
            spr.sprite = open;
            holdTimer = holdTimerMax;
            isGrabbing = false;
        }
    }
}
