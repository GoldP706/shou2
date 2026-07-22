using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StartDropZone2D : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag ImageStart, the movable Start block, here.")]
    public GameObject startBlock;

    [Tooltip("Drag StartCanvas, which contains StartScreenController, here.")]
    public StartScreenController startScreenController;

    [Tooltip("Optional exact snap position. Leave empty to use the center of this box.")]
    public Transform snapPoint;

    [Header("Settings")]
    [Tooltip("Require the whole Start block collider to be inside the target collider before snapping.")]
    public bool requireFullyInside = true;

    [Tooltip("Optional additional center-distance requirement. Set to 0 to use only the full-inside check.")]
    [Min(0f)] public float requiredCenterDistance = 0f;

    private bool started;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStartGame(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryStartGame(other);
    }

    private void TryStartGame(Collider2D other)
    {
        if (started || startBlock == null || startScreenController == null)
        {
            return;
        }

        GameObject enteredObject = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        bool isStartBlock = enteredObject == startBlock ||
                            enteredObject.transform.IsChildOf(startBlock.transform) ||
                            startBlock.transform.IsChildOf(enteredObject.transform);

        if (!isStartBlock)
        {
            return;
        }

        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (requireFullyInside && !IsFullyInside(other.bounds, zoneCollider.bounds))
        {
            return;
        }

        Vector3 targetPosition = snapPoint != null
            ? snapPoint.position
            : transform.position;

        if (requiredCenterDistance > 0f &&
            Vector2.Distance(startBlock.transform.position, targetPosition) > requiredCenterDistance)
        {
            return;
        }

        started = true;
        startBlock.transform.position = targetPosition;

        Rigidbody2D body = startBlock.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        TargetJoint2D targetJoint = startBlock.GetComponent<TargetJoint2D>();
        if (targetJoint != null)
        {
            targetJoint.enabled = false;
        }

        Collider2D[] blockColliders = startBlock.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D blockCollider in blockColliders)
        {
            blockCollider.enabled = false;
        }

        startScreenController.StartGame();
    }

    private bool IsFullyInside(Bounds item, Bounds zone)
    {
        return item.min.x >= zone.min.x &&
               item.max.x <= zone.max.x &&
               item.min.y >= zone.min.y &&
               item.max.y <= zone.max.y;
    }
}
