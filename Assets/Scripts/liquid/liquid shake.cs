using UnityEngine;

public class LiquidInertia : MonoBehaviour
{
    public Transform cupTransform;   // 拖入杯子父物体
    public float followSpeed = 8f;   // 跟随速度，越小越慢越晃
    public float maxOffset = 0.3f;   // 最大偏移量，防止液体跑出杯子

    private Vector3 targetPos;
    private Vector3 velocity;

    void LateUpdate()
    {
        // 目标位置 = 杯子位置
        targetPos = cupTransform.position;

        // 液体平滑跟随杯子（有延迟）
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            1f / followSpeed
        );

        // 限制偏移量，不让液体跑出杯子太多
        Vector3 offset = transform.position - targetPos;
        offset = Vector3.ClampMagnitude(offset, maxOffset);
        transform.position = targetPos + offset;

        // 把偏移量传给Shader，用来扭曲液面
        GetComponent<SpriteRenderer>().material.SetFloat(
            "_OffsetX", offset.x
        );
        GetComponent<SpriteRenderer>().material.SetFloat(
            "_OffsetY", offset.y
        );
    }
}