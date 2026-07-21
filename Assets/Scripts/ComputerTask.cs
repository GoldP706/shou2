using System.Collections;
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

    [Header("Task")]
    public int minStep = 3;
    public int maxStep = 5;

    [Header("总任务次数")]
    public int totalTaskCount = 3;

    [Header("Cooldown")]
    public float cooldown = 8f;

    private int currentStep;
    private int targetStep;

    private int finishedTaskCount = 0;

    private bool taskRunning = false;

    // 记录上一帧食指是否处于弯曲状态，用于边缘检测（只在"刚弯下去"那一帧算一次点击）
    private bool wasGrabbingLastFrame = false;

    void Start()
    {
        keyPointer.gameObject.SetActive(false);

        StartCoroutine(Cooldown());
    }

    void Update()
    {
        if (!taskRunning)
            return;

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
            CorrectClick();
        }
        else
        {
            Debug.Log("点歪了");
        }
    }

    void CorrectClick()
    {
        currentStep++;

        Debug.Log("Correct : " + currentStep + "/" + targetStep);

        if (currentStep >= targetStep)
        {
            FinishTask();
        }
        else
        {
            MovePointer();
        }
    }

    void StartTask()
    {
        taskRunning = true;

        currentStep = 0;

        targetStep = Random.Range(minStep, maxStep + 1);

        keyPointer.gameObject.SetActive(true);

        MovePointer();

        // 重置边缘检测状态，避免任务一开始就因为玩家仍按住上一次动作而误触发
        wasGrabbingLastFrame = pointerFinger.isGrabbing;

        Debug.Log("Computer Task Start, 本轮需要点击 " + targetStep + " 次");
    }

    void FinishTask()
    {
        Debug.Log("Computer Task Finish");

        taskRunning = false;

        keyPointer.gameObject.SetActive(false);

        finishedTaskCount++;

        Debug.Log("已完成电脑任务 " + finishedTaskCount + " / " + totalTaskCount);

        if (finishedTaskCount >= totalTaskCount)
        {
            Debug.Log("电脑任务全部完成，不再继续");
            return;
        }

        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);

        StartTask();
    }
}

