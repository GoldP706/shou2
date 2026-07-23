using System.Collections;
using UnityEngine;

public class ComputerTask : MonoBehaviour
{
    public bool taskCompleted = false;
    public KeyboardArea keyboardArea;

    public Transform keyPointer;

    public HandControllerNew hand;

    [Header("食指指尖 Transform")]
    public Transform pointerFingerTip;

    [Header("食指 FingerController")]
    public FingerController pointerFinger;

    public float clickDistance = 0.4f;

    [Header("敲键盘音效")]
    public AudioClip typeSfx;
    private AudioSource audioSource;

    [Header("Task")]
    public int minStep = 3;
    public int maxStep = 5;

    [Header("完成任务需要的轮数")]
    public int totalTaskCount = 3;

    [Header("Cooldown")]
    public float cooldown = 8f;

    [Header("任务栏连接")]
    [Tooltip("拖入挂有 TaskChecklistUI 的 TaskCanvas")]
    public TaskChecklistUI taskChecklist;

    [Tooltip("任务栏中打字任务的 Task Id")]
    public string taskId = "typing";

    private int currentStep;
    private int targetStep;

    private int finishedTaskCount = 0;

    private bool taskRunning = false;

    // 记录上一帧食指是否处于弯曲状态，用于边缘检测。
    private bool wasGrabbingLastFrame = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        keyPointer.gameObject.SetActive(false);

        StartCoroutine(Cooldown());
    }

    void Update()
    {
        if (!taskRunning)
            return;

        bool isGrabbingNow = pointerFinger.isGrabbing;

        // 只有从“未弯曲”变成“弯曲”的一帧才点击一次。
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
        // 只有 Type 手势才能触发。
        if (hand.handState != 2)
        {
            return;
        }

        float distance = Vector2.Distance(
            pointerFingerTip.position,
            keyPointer.position
        );

        Debug.Log("距离：" + distance);

        if (distance <= clickDistance)
        {
            // 只有敲对了才播放音效
            if (audioSource != null && typeSfx != null)
            {
                audioSource.PlayOneShot(typeSfx);
            }

            CorrectClick();


        }
    }

    void CorrectClick()
    {
        currentStep++;

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

        // 防止任务开始时手指已经按住而误触发。
        wasGrabbingLastFrame = pointerFinger.isGrabbing;
    }

    void FinishTask()
    {
        taskRunning = false;

        keyPointer.gameObject.SetActive(false);

        finishedTaskCount++;

        if (finishedTaskCount >= totalTaskCount)
        {
            taskCompleted = true;

            // 只有全部三轮完成后，才通知任务栏打勾。
            if (taskChecklist != null)
            {
                taskChecklist.CompleteTask(taskId);
            }

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


