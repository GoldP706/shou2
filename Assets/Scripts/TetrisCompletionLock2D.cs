using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TetrisCompletionLock2D : MonoBehaviour
{
    [Header("Pieces")]
    [Tooltip("Automatically find all active TetrisSnap2D pieces in the scene.")]
    public bool autoFindPieces = true;

    [Min(1)] public int requiredPieceCount = 6;

    [Tooltip("You may also assign the six pieces manually.")]
    public TetrisSnap2D[] pieces;

    [Header("Runtime status (read while playing)")]
    [SerializeField] private int snappedPieceCount;
    [SerializeField] private bool isCompleted;

    [Header("Completion")]
    public UnityEvent onCompleted;

    [Header("Computer taskbar")]
    public TaskChecklistUI taskChecklist;
    public string taskId = "tetris";

    public bool IsCompleted => isCompleted;

    private bool missingPiecesWarningShown;

    private void Start()
    {
        if (autoFindPieces || pieces == null || pieces.Length == 0)
        {
            RefreshPieces();
        }
    }

    private void Update()
    {
        if (isCompleted)
        {
            return;
        }

        if (autoFindPieces &&
            (pieces == null || pieces.Length != requiredPieceCount || HasNullPiece()))
        {
            RefreshPieces();
        }

        snappedPieceCount = CountUniqueSnappedPieces();

        if (pieces == null || pieces.Length < requiredPieceCount)
        {
            if (!missingPiecesWarningShown)
            {
                Debug.LogWarning(
                    "Tetris completion detector found " +
                    (pieces == null ? 0 : pieces.Length) +
                    " pieces, but requires " + requiredPieceCount + ".");
                missingPiecesWarningShown = true;
            }

            return;
        }

        if (snappedPieceCount < requiredPieceCount)
        {
            return;
        }

        CompletePuzzle();
    }

    [ContextMenu("Refresh Pieces")]
    public void RefreshPieces()
    {
        pieces = FindObjectsOfType<TetrisSnap2D>();
        missingPiecesWarningShown = false;
    }

    private int CountUniqueSnappedPieces()
    {
        if (pieces == null)
        {
            return 0;
        }

        HashSet<TetrisSnap2D> uniqueSnappedPieces =
            new HashSet<TetrisSnap2D>();

        foreach (TetrisSnap2D piece in pieces)
        {
            if (piece != null && piece.IsFixedSnapped)
            {
                uniqueSnappedPieces.Add(piece);
            }
        }

        return uniqueSnappedPieces.Count;
    }

    private bool HasNullPiece()
    {
        if (pieces == null)
        {
            return true;
        }

        foreach (TetrisSnap2D piece in pieces)
        {
            if (piece == null)
            {
                return true;
            }
        }

        return false;
    }

    private void CompletePuzzle()
    {
        isCompleted = true;

        HashSet<TetrisSnap2D> lockedPieces = new HashSet<TetrisSnap2D>();
        foreach (TetrisSnap2D piece in pieces)
        {
            if (piece != null && lockedPieces.Add(piece))
            {
                piece.PermanentlyLock();
            }
        }

        if (taskChecklist != null)
        {
            taskChecklist.CompleteTask(taskId);
        }

        onCompleted?.Invoke();
        Debug.Log("Tetris puzzle completed and permanently locked.");
    }
}
