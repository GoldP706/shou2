using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CactusHazard2D : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandCactusReaction2D hand = other.GetComponentInParent<HandCactusReaction2D>();

        if (hand != null)
        {
            hand.BounceFrom(transform.position);
        }
    }
}
