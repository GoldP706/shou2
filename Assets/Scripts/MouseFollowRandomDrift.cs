using UnityEngine;

public class MouseFollowRandomDrift : MonoBehaviour
{
    [Header("Mouse Follow")]
    public Camera mainCamera;
    public float followSpeed = 10f;

    [Header("Idle Random Drift")]
    public float driftSpeed = 1f;
    public float directionChangeTime = 1.5f;
    public float driftSmoothness = 3f;

    private Vector3 driftDirection;
    private Vector3 targetDriftDirection;

    private float driftTimer;

    private Vector3 lastMousePosition;
    private bool mouseMoving;

    private float clampMouseYMin = -4.5f;
    private float clampMouseYMax = 3.3f;
    private float clampMouseXMin = -7f;
    private float clampMouseXMax = 8f;



    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        lastMousePosition = Input.mousePosition;

        PickNewDriftDirection();
    }


    void Update()
    {
        DetectMouseMovement();

        if (mouseMoving)
        {
            FollowMouse();
        }
        else
        {
            RandomDrift();
        }
    }


    void FollowMouse()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                Mathf.Abs(mainCamera.transform.position.z)
            )
        );

        mouseWorldPosition.z = transform.position.z;

        var clampedMouseWorldPosition = mouseWorldPosition;
        clampedMouseWorldPosition.y = Mathf.Clamp(mouseWorldPosition.y, clampMouseYMin, clampMouseYMax);
        clampedMouseWorldPosition.x = Mathf.Clamp(mouseWorldPosition.x, clampMouseXMin, clampMouseXMax);

        transform.position = Vector3.Lerp(
            transform.position,
            clampedMouseWorldPosition,
            followSpeed * Time.deltaTime
        );
    }


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
            randomDirection.x,
            randomDirection.y,
            0
        );
    }


    void DetectMouseMovement()
    {
        float distance = Vector3.Distance(
            Input.mousePosition,
            lastMousePosition
        );

        mouseMoving = distance > 0.01f;

        lastMousePosition = Input.mousePosition;
    }
}