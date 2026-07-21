using System.Collections.Generic;
using UnityEngine;

public class HandControllerNew : MonoBehaviour
{
    public bool grabbing = false;
    public GameObject closestObj;
    public GameObject heldObj;

    private AudioSource audioSource;
    private bool sfxPlayed = false;

    public int handState = 0;
    private bool stateSwitched = false;

    [SerializeField] MouseFollowRandomDrift mouseFollow;
    [SerializeField] FingerController thumb;
    [SerializeField] FingerController pointer;
    [SerializeField] FingerController middle;
    [SerializeField] FingerController ring;
    [SerializeField] FingerController little;

    private float handHeight = 1f;

    // A piece can have several colliders. Count them so exiting just one
    // collider does not make the piece disappear from the nearby list.
    private readonly Dictionary<GameObject, int> nearbyObjects =
        new Dictionary<GameObject, int>();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        UpdateHandState();

        grabbing = thumb.isGrabbing &&
            (pointer.isGrabbing || middle.isGrabbing ||
             ring.isGrabbing || little.isGrabbing);

        if (grabbing)
        {
            // Choose only once. The selected object stays locked until the
            // hand actually opens, even if its collider leaves the palm.
            if (heldObj == null)
            {
                heldObj = FindClosestNearbyObject();
                closestObj = heldObj;

                if (heldObj != null)
                {
                    TetrisSnap2D snap = heldObj.GetComponent<TetrisSnap2D>();
                    if (snap != null)
                    {
                        snap.BeginGrab();
                    }
                }

                if (heldObj != null && !sfxPlayed)
                {
                    if (audioSource != null)
                    {
                        audioSource.Play();
                    }

                    sfxPlayed = true;
                }
            }

            if (heldObj != null)
            {
                TargetJoint2D joint = heldObj.GetComponent<TargetJoint2D>();
                if (joint != null)
                {
                    joint.target = transform.position;
                }
            }
        }
        else
        {
            ReleaseHeldObject();
            closestObj = FindClosestNearbyObject();
        }
    }

    private void UpdateHandState()
    {
        if (Input.mouseScrollDelta.y > 2f && !stateSwitched)
        {
            handState++;
            if (handState > 2) handState = 0;
            stateSwitched = true;
        }

        if (Input.mouseScrollDelta.y < -2f && !stateSwitched)
        {
            handState--;
            if (handState < 0) handState = 2;
            stateSwitched = true;
        }

        if (Input.mouseScrollDelta.y == 0f)
        {
            stateSwitched = false;
        }

        if (Input.GetKey(KeyCode.Mouse0) && handHeight > 0f)
        {
            handHeight -= 0.1f;
        }

        if (Input.GetKey(KeyCode.Mouse1) && handHeight > 1f)
        {
            handHeight += 0.1f;
        }
    }

    private void ReleaseHeldObject()
    {
        if (heldObj == null)
        {
            return;
        }

        TetrisSnap2D snap = heldObj.GetComponent<TetrisSnap2D>();
        if (snap != null)
        {
            snap.TrySnapToGrid();
        }

        heldObj = null;
        sfxPlayed = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject grabbable = FindGrabbableRoot(other);
        if (grabbable == null)
        {
            return;
        }

        if (nearbyObjects.ContainsKey(grabbable))
        {
            nearbyObjects[grabbable]++;
        }
        else
        {
            nearbyObjects.Add(grabbable, 1);
        }

        if (!grabbing && heldObj == null)
        {
            closestObj = FindClosestNearbyObject();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject grabbable = FindGrabbableRoot(other);
        if (grabbable == null || !nearbyObjects.ContainsKey(grabbable))
        {
            return;
        }

        nearbyObjects[grabbable]--;
        if (nearbyObjects[grabbable] <= 0)
        {
            nearbyObjects.Remove(grabbable);
        }

        // Never change heldObj here. It is released only when the hand opens.
        if (!grabbing && heldObj == null)
        {
            closestObj = FindClosestNearbyObject();
        }
    }

    private GameObject FindGrabbableRoot(Collider2D other)
    {
        TargetJoint2D joint = other.GetComponentInParent<TargetJoint2D>();
        GameObject candidate = joint != null ? joint.gameObject : other.gameObject;

        return candidate.CompareTag("CanGrab") ? candidate : null;
    }

    private GameObject FindClosestNearbyObject()
    {
        GameObject nearest = null;
        float nearestDistance = float.PositiveInfinity;

        foreach (KeyValuePair<GameObject, int> pair in nearbyObjects)
        {
            if (pair.Key == null || pair.Value <= 0 ||
                !pair.Key.CompareTag("CanGrab"))
            {
                continue;
            }

            float distance =
                ((Vector2)pair.Key.transform.position -
                 (Vector2)transform.position).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = pair.Key;
            }
        }

        return nearest;
    }
}


