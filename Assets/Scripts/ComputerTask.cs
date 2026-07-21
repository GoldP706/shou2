using System.Collections;
using UnityEngine;

public class ComputerTask : MonoBehaviour

{
    public bool taskCompleted = false;
    public KeyboardArea keyboardArea;

    public Transform keyPointer;

    public HandControllerNew hand;

    [Header("��ʳָ�� Transform ��Ϊָ������")]
    public Transform pointerFingerTip;

    [Header("ʳָ�����ж�������ʳָ�� FingerController��")]
    public FingerController pointerFinger;

    public float clickDistance = 0.4f;

    [Header("Task")]
    public int minStep = 3;
    public int maxStep = 5;

    [Header("���������")]
    public int totalTaskCount = 3;

    [Header("Cooldown")]
    public float cooldown = 8f;

    private int currentStep;
    private int targetStep;

    private int finishedTaskCount = 0;

    private bool taskRunning = false;

    // ��¼��һ֡ʳָ�Ƿ�������״̬�����ڱ�Ե��⣨ֻ��"������ȥ"��һ֡��һ�ε����
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

        

        // ֻ�д�"û��"���"����"����һ֡�Ŵ���һ�� TryClick
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
        // ֻ�� Type ���Ʋ��ܴ���
        if (hand.handState != 2)
        {
            Debug.Log("���� Type ����");
            return;
        }

        float distance = Vector2.Distance(
            pointerFingerTip.position,
            keyPointer.position
        );

        Debug.Log("���룺" + distance);

        if (distance <= clickDistance)
        {
            Debug.Log("����ɹ���");
            CorrectClick();
        }
        else
        {
            Debug.Log("������");
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

        // ���ñ�Ե���״̬����������һ��ʼ����Ϊ����԰�ס��һ�ζ������󴥷�
        wasGrabbingLastFrame = pointerFinger.isGrabbing;

        Debug.Log("Computer Task Start, ������Ҫ��� " + targetStep + " ��");
    }

    void FinishTask()
    {
        Debug.Log("Computer Task Finish");

        taskRunning = false;

        keyPointer.gameObject.SetActive(false);

        finishedTaskCount++;

        Debug.Log("����ɵ������� " + finishedTaskCount + " / " + totalTaskCount);

        if (finishedTaskCount >= totalTaskCount)
        {
            taskCompleted = true;
            Debug.Log("��������ȫ����ɣ����ټ���");
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

