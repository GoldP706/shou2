using UnityEngine;

public class ComputerTask : MonoBehaviour

{
    public KeyboardArea keyboardArea;

    public Transform keyPointer;

    public HandControllerNew hand;

    [Header("用食指的 Transform 作为指尖点击点")]
    public Transform pointerFingerTip;

    [Header("食指弯曲判定（来自食指的 FingerController）")]
    public FingerController pointerFinger;

    public float clickDistance = 0.4f;

    // 记录上一帧食指是否处于弯曲状态，用于边缘检测（只在"刚弯下去"那一帧算一次点击）
    private bool wasGrabbingLastFrame = false;

    void Start()
    {
        MovePointer();
    }

    void Update()
    {
        // 按空格换位置（测试）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MovePointer();
        }

        bool isGrabbingNow = pointerFinger.isGrabbing;

        // 只有从"没弯"变成"弯了"的那一帧才触发一次 TryClick
        if (isGrabbingNow && !wasGrabbingLastFrame)
        {
            TryClick();
        }

        wasGrabbingLastFrame = isGrabbingNow;
    }

    void MovePointer()
    {
        keyPointer.position = keyboardArea.GetRandomPoint();
    }
  

    void TryClick()
    {
        Debug.Log("11");
        // 只有 Type 手势才能打字
        if (hand.handState != 2)
        {
            Debug.Log("不是 Type 手势");
            return;
        }

        float distance = Vector2.Distance(
            pointerFingerTip.position,
            keyPointer.position
        );

        Debug.Log("距离：" + distance);

        if (distance <= clickDistance)
        {
            Debug.Log("点击成功！");
            MovePointer();
        }
        else
        {
            Debug.Log("点歪了");
        }
    }
}
