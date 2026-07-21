using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyMovement : MonoBehaviour
{
    [Header("Random Drift")]
    public float driftSpeed = 1f;
       // 返回画面的速度
    public float directionChangeTime = 1.5f;
    public float driftSmoothness = 3f;

    // 返回到距离中心多少后恢复随机飞行
    public float safeDistance = 3f;

    private Vector3 driftDirection;
    private Vector3 targetDriftDirection;

    private float driftTimer;

    // 是否正在返回屏幕
    private bool returningToScreen = false;

    void Start()
    {
        PickNewDriftDirection();
        driftDirection = targetDriftDirection;
    }

    void Update()
    {
        RandomDrift();
    }

    void RandomDrift()
    {
        //--------------------------
        // 返回状态
        //--------------------------
        if (returningToScreen)
        {
            // 朝屏幕中心飞
            targetDriftDirection = (Vector3.zero - transform.position).normalized;

            driftDirection = Vector3.Lerp(
                driftDirection,
                targetDriftDirection,
                driftSmoothness * Time.deltaTime
            );

            transform.position += driftDirection * driftSpeed * Time.deltaTime;

            // 已经回到安全区域
            if (Vector3.Distance(transform.position, Vector3.zero) <= safeDistance)
            {
                returningToScreen = false;
                driftTimer = 0;
                PickNewDriftDirection();
            }

            return;
        }

        //--------------------------
        // 随机漂浮
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
            randomDirection.x * 2f,
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
}