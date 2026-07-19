using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyMovement : MonoBehaviour
{
    [Header("Idle Random Drift")]
    public float driftSpeed = 1f;
    public float directionChangeTime = 1.5f;
    public float driftSmoothness = 3f;

    private Vector3 driftDirection;
    private Vector3 targetDriftDirection;

    private float driftTimer;

    private Vector3 lastMousePosition;
    private bool mouseMoving;


    void Start()
    {
        PickNewDriftDirection();
    }


    void Update()
    {
        
        RandomDrift();
        
    }


    /*void FollowMouse()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                Mathf.Abs(mainCamera.transform.position.z)
            )
        );

        mouseWorldPosition.z = transform.position.z;

        transform.position = Vector3.Lerp(
            transform.position,
            mouseWorldPosition,
            followSpeed * Time.deltaTime
        );
    }*/


    void RandomDrift()
    {
        driftTimer += Time.deltaTime;


        // Change direction periodically
        if (driftTimer >= directionChangeTime)
        {
            PickNewDriftDirection();
            driftTimer = 0;
        }


        // Smooth direction change
        driftDirection = Vector3.Lerp(
            driftDirection,
            targetDriftDirection,
            driftSmoothness * Time.deltaTime
        );


        transform.position += driftDirection * driftSpeed * Time.deltaTime;
    }


    void PickNewDriftDirection()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        targetDriftDirection = new Vector3(
            randomDirection.x*2,
            randomDirection.y,
            0
        );
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.gameObject.tag == "border")
        {
            //Debug.Log("hit border");
            targetDriftDirection = new Vector3(0,0,0);
        }
    }
}
