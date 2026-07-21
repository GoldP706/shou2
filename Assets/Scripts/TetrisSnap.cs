
using UnityEngine;

public class TetrisSnap2D : MonoBehaviour
{
    [Header("Fixed target (recommended for this puzzle)")]
    [Tooltip("If assigned, the piece snaps directly to this exact position instead of using the grid calculation.")]
    public Transform fixedTarget;

    [Tooltip("Hold the piece perfectly in place after snapping. It will automatically unlock when grabbed again.")]
    public bool lockAfterFixedSnap = true;

    [Header("4 x 5 board")]
    [Tooltip("Place an empty object at the inside bottom-left corner of the wooden frame.")]
    public Transform gridOrigin;

    [Min(0.01f)] public float cellSize = 1f;
    [Min(1)] public int columns = 4;
    [Min(1)] public int rows = 5;

    [Header("Snapping")]
    [Tooltip("The piece only snaps when it is this close to the board.")]
    [Min(0f)] public float snapRange = 1.5f;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Collider2D[] pieceColliders;
    private TargetJoint2D targetJoint;
    private RigidbodyType2D originalBodyType;
    private RigidbodyConstraints2D originalConstraints;
    private bool isFixedSnapped;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        pieceColliders = GetComponentsInChildren<Collider2D>();
        targetJoint = GetComponent<TargetJoint2D>();

        if (body != null)
        {
            originalBodyType = body.bodyType;
            originalConstraints = body.constraints;
        }
    }

    public void BeginGrab()
    {
        if (!isFixedSnapped || body == null)
        {
            return;
        }

        body.bodyType = originalBodyType;
        body.constraints = originalConstraints;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;

        if (targetJoint != null)
        {
            targetJoint.enabled = true;
            targetJoint.target = body.position;
        }

        body.WakeUp();
        isFixedSnapped = false;
    }

    public void TrySnapToGrid()
    {
        if (fixedTarget != null)
        {
            TrySnapToFixedTarget();
            return;
        }

        if (gridOrigin == null || body == null || spriteRenderer == null)
        {
            return;
        }

        Bounds bounds = spriteRenderer.bounds;
        Vector2 origin = gridOrigin.position;
        Vector2 oldPosition = body.position;

        int pieceColumns = Mathf.Max(1, Mathf.RoundToInt(bounds.size.x / cellSize));
        int pieceRows = Mathf.Max(1, Mathf.RoundToInt(bounds.size.y / cellSize));

        if (pieceColumns > columns || pieceRows > rows)
        {
            return;
        }

        float snappedMinX = origin.x +
            Mathf.Round((bounds.min.x - origin.x) / cellSize) * cellSize;
        float snappedMinY = origin.y +
            Mathf.Round((bounds.min.y - origin.y) / cellSize) * cellSize;

        snappedMinX = Mathf.Clamp(
            snappedMinX,
            origin.x,
            origin.x + (columns - pieceColumns) * cellSize);

        snappedMinY = Mathf.Clamp(
            snappedMinY,
            origin.y,
            origin.y + (rows - pieceRows) * cellSize);

        Vector2 movement = new Vector2(
            snappedMinX - bounds.min.x,
            snappedMinY - bounds.min.y);

        Vector2 snappedPosition = oldPosition + movement;

        if (Vector2.Distance(oldPosition, snappedPosition) > snapRange)
        {
            return;
        }

        body.position = snappedPosition;
        Physics2D.SyncTransforms();

        if (OverlapsAnotherPiece())
        {
            body.position = oldPosition;
            Physics2D.SyncTransforms();
            UpdateJointTarget(oldPosition);
            return;
        }

        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        UpdateJointTarget(snappedPosition);
    }

    private void TrySnapToFixedTarget()
    {
        if (body == null)
        {
            return;
        }

        Vector2 targetPosition = fixedTarget.position;
        if (Vector2.Distance(body.position, targetPosition) > snapRange)
        {
            return;
        }

        body.position = targetPosition;
        body.rotation = fixedTarget.eulerAngles.z;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        UpdateJointTarget(targetPosition);
        Physics2D.SyncTransforms();

        if (!lockAfterFixedSnap)
        {
            isFixedSnapped = false;
            return;
        }

        body.constraints = RigidbodyConstraints2D.FreezeAll;
        isFixedSnapped = true;

        if (targetJoint != null)
        {
            targetJoint.enabled = false;
        }
    }

    private bool OverlapsAnotherPiece()
    {
        TetrisSnap2D[] allPieces = FindObjectsOfType<TetrisSnap2D>();

        foreach (TetrisSnap2D otherPiece in allPieces)
        {
            if (otherPiece == this)
            {
                continue;
            }

            Collider2D[] otherColliders =
                otherPiece.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D ownCollider in pieceColliders)
            {
                if (ownCollider == null || ownCollider.isTrigger)
                {
                    continue;
                }

                foreach (Collider2D otherCollider in otherColliders)
                {
                    if (otherCollider == null || otherCollider.isTrigger)
                    {
                        continue;
                    }

                    if (ownCollider.Distance(otherCollider).isOverlapped)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void UpdateJointTarget(Vector2 position)
    {
        if (targetJoint != null)
        {
            targetJoint.target = position;
        }
    }
}

