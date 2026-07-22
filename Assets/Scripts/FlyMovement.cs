using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyMovement : MonoBehaviour
{
    [Header("Random Drift")]
    public float driftSpeed = 1f;
    public float directionChangeTime = 1.5f;
    public float driftSmoothness = 3f;

    // 离屏幕中心多远就开始往回飞
    public float safeDistance = 3f;

    private Vector3 driftDirection;
    private Vector3 targetDriftDirection;

    private float driftTimer;

    // 是否正在飞回屏幕的路
    private bool returningToScreen = false;

    //check if dead
    public bool isDead = false;

    // ==================== 新增战斗相关状态 ====================
    public enum FlyState { RandomDrift, Chasing, KnockedBack, Stunned }
    [HideInInspector] public FlyState currentState = FlyState.RandomDrift;
    [HideInInspector] public Transform chaseTarget; // 要追的叮点（由FlyManager赋值）

    [Header("Combat Settings")]
    [Tooltip("被肘击后飞出去的距离")] public float knockbackDistance = 2f;
    [Tooltip("被肘击后晕多久才会继续追")] public float stunTime = 1f;
    [Tooltip("追人的速度倍率")] public float chaseSpeedMultiplier = 1.5f;
    [Tooltip("被打飞的速度倍率")] public float knockbackSpeedMultiplier = 2f;

    private Vector3 knockbackDirection;
    private float currentStunTimer;
    private float knockbackTraveled;
    // ==========================================================

    void Start()
    {
        PickNewDriftDirection();
        driftDirection = targetDriftDirection;
    }

    void Update()
    {
        if (isDead) return; // 死了就不动
        RandomDrift();
    }

    void RandomDrift()
    {
        //--------------------------
        // 回家状态（优先级最高，任何状态下撞边都先回屏幕）
        //--------------------------
        if (returningToScreen)
        {
            // 朝屏幕中心的方向
            targetDriftDirection = (Vector3.zero - transform.position).normalized;

            driftDirection = Vector3.Lerp(
                driftDirection,
                targetDriftDirection,
                driftSmoothness * Time.deltaTime
            );

            transform.position += driftDirection * driftSpeed * Time.deltaTime;

            // 已经回到安全范围
            if (Vector3.Distance(transform.position, Vector3.zero) <= safeDistance)
            {
                returningToScreen = false;
                driftTimer = 0;
                PickNewDriftDirection();
                // 回家后如果还在追人就继续追，否则随机飘
                currentState = chaseTarget != null ? FlyState.Chasing : FlyState.RandomDrift;
            }

            return;
        }

        //--------------------------
        // 被打飞状态
        //--------------------------
        if (currentState == FlyState.KnockedBack)
        {
            float moveStep = driftSpeed * knockbackSpeedMultiplier * Time.deltaTime;
            transform.position += knockbackDirection * moveStep;
            knockbackTraveled += moveStep;

            // 飞够距离了就进入眩晕
            if (knockbackTraveled >= knockbackDistance)
            {
                currentState = FlyState.Stunned;
                currentStunTimer = stunTime;
            }
            return;
        }

        //--------------------------
        // 眩晕状态
        //--------------------------
        if (currentState == FlyState.Stunned)
        {
            currentStunTimer -= Time.deltaTime;
            if (currentStunTimer <= 0)
            {
                // 晕完了：有追击目标就继续追，没有就回去随机飘
                currentState = chaseTarget != null ? FlyState.Chasing : FlyState.RandomDrift;
            }
            return;
        }

        //--------------------------
        // 追击叮点状态
        //--------------------------
        if (currentState == FlyState.Chasing && chaseTarget != null)
        {
            targetDriftDirection = (chaseTarget.position - transform.position).normalized;
            driftDirection = Vector3.Lerp(
                driftDirection,
                targetDriftDirection,
                driftSmoothness * 2f * Time.deltaTime // 追人的时候转向更灵
            );
            transform.position += driftDirection * driftSpeed * chaseSpeedMultiplier * Time.deltaTime;
            return;
        }

        //--------------------------
        // 正常随机飘（你原来的逻辑，完全没改）
        //--------------------------
        driftTimer += Time.deltaTime;

        if (driftTimer >= directionChangeTime)
        {
            PickNewDriftDirection();
            driftTimer = 0;
        }

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
            randomDirection.x * 2f, // 左右飞的概率更高，符合苍蝇/蚊子习性
            randomDirection.y,
            0
        ).normalized;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("border"))
        {
            returningToScreen = true;
        }
    }

    // ==================== 公共方法 ====================
    /// <summary>
    /// 被肘击 hit 的时候调用，传入打飞的方向
    /// </summary>
    public void GetKnockedBack(Vector3 hitDirection)
    {
        knockbackDirection = hitDirection.normalized;
        knockbackTraveled = 0f;
        currentState = FlyState.KnockedBack;
    }

    /// <summary>
    /// 停止追击，回到随机飘状态
    /// </summary>
    public void StopChasing()
    {
        chaseTarget = null;
        currentState = FlyState.RandomDrift;
    }

    /// <summary>
    /// 开始追击指定目标
    /// </summary>
    public void StartChasing(Transform target)
    {
        chaseTarget = target;
        currentState = FlyState.Chasing;
    }

    /// <summary>
    /// 强制触发回屏幕（给管理器调用，解决生成在屏幕外飞不进来的问题）
    /// </summary>
    public void TriggerReturnScreen()
    {
        returningToScreen = true;
    }
}