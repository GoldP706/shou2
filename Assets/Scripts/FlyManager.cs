using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyManager : MonoBehaviour
{
    private float stingWaitTimer;
    public float stingWaitTimerMax = 20f;
    private float stingingTimer;
    public float stingingTimeMax;

    private float respawnTimer = 3f;
    [SerializeField] bool canRespawn = false;
    private int elbowCount = 0;
    private bool isStinging = false;

    [SerializeField] SpriteRenderer stingWarningSpr;
    [SerializeField] FlyMovement flyMovement;
    [SerializeField] GameObject flyPrefab;
    [SerializeField] GameObject currentFly;

    [Header("References - 拖对应的组件")]
    public Transform hand; // 直接拖你的手对象就行
    public MouseFollowRandomDrift mouseFollow; // 拖你手上的MouseFollowRandomDrift组件
    public Transform stingPoint;
    public Collider2D elbowHitbox;
    public GameObject bitePrefab;
    public Transform handVisual;

    [Header("Settings")]
    public float idleThreshold = 0.3f;
    public float idleTriggerTime = 2f;
    public float abandonDistance = 0.5f;
    public float warningDuration = 0.5f;
    public float biteStayTime = 5f;
    public float shakeDuration = 5f;
    public float shakeStrength = 0.1f;

    private enum StingState { Idle, Chasing, Warning }
    private StingState currentStingState = StingState.Idle;

    private float idleTimer;
    private float warningTimer;
    private float shakeTimer;
    private Vector3 chaseStartHandPos;
    private bool wasElbowingLastFrame;
    private Transform shakeTarget;
    private Vector3 handVisualOriginalLocalPos;

    void Start()
    {
        stingWaitTimer = stingWaitTimerMax;
        currentFly = GameObject.FindWithTag("Fly");
        if (currentFly != null)
            flyMovement = currentFly.GetComponent<FlyMovement>();

        shakeTarget = handVisual != null ? handVisual : hand;
        if (handVisual != null)
            handVisualOriginalLocalPos = handVisual.localPosition;

        if (stingWarningSpr != null) stingWarningSpr.enabled = false;
        if (elbowHitbox != null) elbowHitbox.enabled = false;
        chaseStartHandPos = hand.position;
    }

    void Update()
    {
        if (hand == null || mouseFollow == null) return;

        if (canRespawn)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                Spawn();
                respawnTimer = 3f;
            }
        }

        if (currentFly != null && flyMovement != null && !flyMovement.isDead)
        {
            if (currentStingState == StingState.Idle)
                stingWaitTimer -= Time.deltaTime;
        }
        if (currentFly == null || (flyMovement != null && flyMovement.isDead))
        {
            canRespawn = true;
        }
        if (isStinging)
        {
            stingingTimer += Time.deltaTime;
        }

        if (flyMovement == null || flyMovement.isDead || currentFly == null)
        {
            ResetStingState();
            return;
        }

        bool isElbowingNow = !mouseFollow.enabled;

        if (isElbowingNow && !wasElbowingLastFrame)
        {
            if (elbowHitbox != null) elbowHitbox.enabled = true;
        }
        if (!isElbowingNow && wasElbowingLastFrame)
        {
            if (elbowHitbox != null) elbowHitbox.enabled = false;
            chaseStartHandPos = hand.position;
        }
        wasElbowingLastFrame = isElbowingNow;

        if (isElbowingNow && elbowHitbox != null)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(elbowHitbox.bounds.center, elbowHitbox.bounds.size, 0);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Fly"))
                {
                    elbowCount++;
                    Vector3 knockDir = (flyMovement.transform.position - hand.position).normalized;
                    flyMovement.GetKnockedBack(knockDir);
                    if (currentStingState == StingState.Warning)
                    {
                        currentStingState = StingState.Chasing;
                        if (stingWarningSpr != null) stingWarningSpr.enabled = false;
                        warningTimer = 0;
                    }
                    chaseStartHandPos = hand.position;
                    break;
                }
            }
        }

        if (currentStingState == StingState.Idle)
        {
            float handMoveDist = Vector3.Distance(hand.position, chaseStartHandPos);
            if (handMoveDist < idleThreshold)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= idleTriggerTime)
                {
                    StartChasing();
                }
            }
            else
            {
                idleTimer = 0;
                chaseStartHandPos = hand.position;
            }

            if (stingWaitTimer <= 0)
            {
                StartChasing();
                stingWaitTimer = stingWaitTimerMax;
            }
        }

        switch (currentStingState)
        {
            case StingState.Chasing:
                if (!isElbowingNow && Vector3.Distance(hand.position, chaseStartHandPos) > abandonDistance)
                {
                    ResetStingState();
                    stingWaitTimer = stingWaitTimerMax;
                    break;
                }
                if (Vector3.Distance(flyMovement.transform.position, stingPoint.position) < 0.2f)
                {
                    currentStingState = StingState.Warning;
                    warningTimer = warningDuration;
                    if (stingWarningSpr != null) stingWarningSpr.enabled = true;
                }
                break;

            case StingState.Warning:
                if (!isElbowingNow && Vector3.Distance(hand.position, chaseStartHandPos) > abandonDistance)
                {
                    ResetStingState();
                    stingWaitTimer = stingWaitTimerMax;
                    break;
                }
                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0)
                {
                    StingSuccess();
                }
                break;
        }

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        if (shakeTarget == null) return;

        if (shakeTimer > 0)
        {
            if (handVisual != null)
            {
                shakeTarget.localPosition = handVisualOriginalLocalPos + (Vector3)Random.insideUnitCircle * shakeStrength;
            }
        }
        else
        {
            if (handVisual != null)
                shakeTarget.localPosition = handVisualOriginalLocalPos;
        }
    }

    void Spawn()
    {
        var spawn = new Vector3(15f, Random.Range(10, -10), 0f);
        GameObject Fly = Instantiate(flyPrefab, spawn, Quaternion.identity);
        currentFly = Fly;
        flyMovement = Fly.GetComponent<FlyMovement>();
        canRespawn = false;

        flyMovement.chaseTarget = stingPoint;
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(Fly.transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            flyMovement.TriggerReturnScreen();
        }
        ResetStingState();
        stingWaitTimer = stingWaitTimerMax;
        isStinging = false;
        stingingTimer = 0;
    }

    void StartChasing()
    {
        currentStingState = StingState.Chasing;
        chaseStartHandPos = hand.position;
        flyMovement.StartChasing(stingPoint);
        idleTimer = 0;
    }

    void ResetStingState()
    {
        currentStingState = StingState.Idle;
        if (flyMovement != null) flyMovement.StopChasing();
        if (stingWarningSpr != null) stingWarningSpr.enabled = false;
        if (elbowHitbox != null) elbowHitbox.enabled = false;
        warningTimer = 0;
        idleTimer = 0;
        wasElbowingLastFrame = false;
        isStinging = false;
        stingingTimer = 0;
    }

    void StingSuccess()
    {
        if (bitePrefab != null && stingPoint != null)
        {
            GameObject bite = Instantiate(bitePrefab, stingPoint.position, Quaternion.identity, hand);
            Destroy(bite, biteStayTime);
        }
        shakeTimer = shakeDuration;
        ResetStingState();
        stingWaitTimer = stingWaitTimerMax;
    }
}