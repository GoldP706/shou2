using UnityEngine;
using System.Collections;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

public class HandController : MonoBehaviour {

	private Vector3 mousePosition;
    public float moveSpeed = 0.1f;
    [SerializeField] float upperBound = 0f;
    private SpriteRenderer spr;

    private bool grabCheck = false;
    private bool grabbing = false;
    public Vector3 grabCheckPos;
    private GameObject closestObj;

    private Vector2 shakeAmount;
    private float shakeTimer = 1f;

	void Start () {
        spr = GetComponent<SpriteRenderer>();
	}
	
	void Update () {

        shakeTimer -= Time.deltaTime;
        shakeAmount = new Vector2(transform.position.x + Random.Range(-0.05f, 0.05f), transform.position.y + Random.Range(-0.05f, 0.05f));

/*
        if (shakeTimer <= 0)
        {
            Debug.Log("shake");
            shakeTimer = 1f;
        }*/

        //var pos = new Vector2(transform.position.x + shakeAmount.x, transform.position.y + shakeAmount.y);

        //Follow mouse movement
        if (mousePosition.y < upperBound)
        {
            mousePosition = Input.mousePosition;
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = Vector2.Lerp(transform.position, mousePosition, moveSpeed);
        }
        else
        {
            mousePosition.y = upperBound;
            transform.position = Vector2.Lerp(transform.position, mousePosition, moveSpeed);
        }

        

        //Grab checker
        /*grabCheckPos = new Vector2(transform.position.x, transform.position.y + 0.2);
        grabCheck = Physics2D.OverlapBox(grabCheckPos, 0.3f, 0, LayerMask.GetMask("canGrab"));

        if (grabbing)
        {
            
        }*/

	}
}