using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HandCactusReaction2D : MonoBehaviour
{
    [Header("Knockback")]
    [Min(0f)] public float bounceDistance = 1.2f;
    [Min(0.01f)] public float bounceDuration = 0.16f;

    [Header("Hit colour")]
    public Color hitColor = Color.red;
    [Min(0f)] public float redDuration = 0.35f;

    [Header("Optional")]
    [Tooltip("Drag the script that normally controls the hand here. It will be paused briefly while the hand recoils.")]
    public MonoBehaviour handMovementController;

    private Rigidbody2D body;
    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private Coroutine reactionRoutine;
    private bool resumeMovementController;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
        }
    }

    public void BounceFrom(Vector2 cactusPosition)
    {
        Vector2 direction = body.position - cactusPosition;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.up;
        }

        if (reactionRoutine != null)
        {
            StopCoroutine(reactionRoutine);
        }

        if (handMovementController != null && handMovementController.enabled)
        {
            handMovementController.enabled = false;
            resumeMovementController = true;
        }

        reactionRoutine = StartCoroutine(PlayReaction(direction.normalized));
    }

    private IEnumerator PlayReaction(Vector2 direction)
    {
        SetHandColor(hitColor);

        Vector2 start = body.position;
        Vector2 end = start + direction * bounceDistance;
        float elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / bounceDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            body.MovePosition(Vector2.LerpUnclamped(start, end, easedT));
            yield return new WaitForFixedUpdate();
        }

        body.MovePosition(end);

        float remainingRedTime = Mathf.Max(0f, redDuration - bounceDuration);
        if (remainingRedTime > 0f)
        {
            yield return new WaitForSeconds(remainingRedTime);
        }

        RestoreHandColors();

        if (resumeMovementController && handMovementController != null)
        {
            handMovementController.enabled = true;
            resumeMovementController = false;
        }

        reactionRoutine = null;
    }

    private void SetHandColor(Color color)
    {
        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            spriteRenderer.color = color;
        }
    }

    private void RestoreHandColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = originalColors[i];
            }
        }
    }

    private void OnDisable()
    {
        RestoreHandColors();

        if (resumeMovementController && handMovementController != null)
        {
            handMovementController.enabled = true;
            resumeMovementController = false;
        }
    }
}
