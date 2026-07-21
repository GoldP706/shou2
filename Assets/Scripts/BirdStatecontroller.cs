using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdStateController : MonoBehaviour
{
    public enum BirdState { Idle, Screaming, Angry }

    [Header("状态时间参数")]
    [Tooltip("待机状态最短持续时间")]
    public float minIdleTime = 5f;
    [Tooltip("待机状态最长持续时间")]
    public float maxIdleTime = 15f;
    [Tooltip("尖叫持续多久后进入愤怒")]
    public float screamDuration = 10f;
    [Tooltip("愤怒后延迟多久播放敲击动画")]
    public float peckDelay = 3f;

    [Header("抚摸参数")]
    [Tooltip("安抚成功需要的抚摸次数")]
    public int requiredPets = 10;

    [Header("愤怒移动参数")]
    [Tooltip("移出屏幕的速度")]
    public float angryMoveSpeed = 15f;
    [Tooltip("红温闪烁时长")]
    public float flashDuration = 0.8f;
    [Tooltip("愤怒时小鸟压缩的比例")]
    public float angryScale = 0.7f;

    [Header("引用")]
    [Tooltip("屏幕裂痕的GameObject（带Animator）")]
    public GameObject crackScreen;
    [Tooltip("残影脚本引用")]
    public AfterImageEffect afterImage;

    // 状态
    public BirdState currentState { get; private set; }
    private Vector3 startPosition;
    private Vector3 originalScale;
    private SpriteRenderer spr;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;

    // 计时
    private float stateTimer;
    private float idleTimer;

    // 抚摸计数
    private int petCount;
    private Dictionary<FingerController, bool> fingerPrevState = new Dictionary<FingerController, bool>();
    private bool birdTouched = false;

    // 愤怒移动方向
    private Vector2 moveDirection;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        startPosition = transform.position;
        originalScale = transform.localScale;

        // 初始待机状态
        EnterIdle();
    }

    void Update()
    {
        switch (currentState)
        {
            case BirdState.Idle:
                UpdateIdle();
                break;
            case BirdState.Screaming:
                UpdateScreaming();
                break;
            case BirdState.Angry:
                // 愤怒状态主要用协程控制
                break;
        }
    }

    // ========== 待机状态 ==========
    void EnterIdle()
    {
        currentState = BirdState.Idle;
        transform.position = startPosition;
        transform.localScale = originalScale;
        spr.color = Color.white;
        petCount = 0;

        // 【修改点1】回到待机自动隐藏屏幕裂痕
        if (crackScreen != null) crackScreen.SetActive(false);

        // 启用物理和碰撞
        rb.bodyType = RigidbodyType2D.Dynamic;
        col.enabled = true;

        // 随机待机时间
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        anim.Play("BirdResting");

        if (afterImage != null) afterImage.enabled = false;
    }

    void UpdateIdle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            EnterScreaming();
        }
    }

    // ========== 尖叫状态 ==========
    void EnterScreaming()
    {
        currentState = BirdState.Screaming;
        stateTimer = screamDuration;
        petCount = 0;
        fingerPrevState.Clear();
        // 【修改点2】和你合并后的动画名统一，删除了BirdScreaming状态的引用
        anim.Play("BirdAngry");
    }

    void UpdateScreaming()
    {
        stateTimer -= Time.deltaTime;

        // 超时 → 愤怒
        if (stateTimer <= 0)
        {
            StartCoroutine(AngrySequence());
        }
    }

    // 检测抚摸（手指弯曲上升沿）
    void OnTriggerStay2D(Collider2D other)
    {
        if (currentState != BirdState.Screaming) return;

        FingerController finger = other.GetComponent<FingerController>();
        if (finger != null)
        {
            bool prevGrab = fingerPrevState.ContainsKey(finger) ? fingerPrevState[finger] : false;
            bool currentGrab = finger.isGrabbing;

            // 从松开到按下 = 一次抚摸
            if (!prevGrab && currentGrab)
            {
                petCount++;
                Debug.Log("抚摸次数: " + petCount + "/" + requiredPets);

                if (petCount >= requiredPets)
                {
                    EnterIdle();
                    return;
                }
            }

            fingerPrevState[finger] = currentGrab;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        FingerController finger = other.GetComponent<FingerController>();
        if (finger != null && fingerPrevState.ContainsKey(finger))
        {
            fingerPrevState.Remove(finger);
        }
    }

    // ========== 愤怒状态协程 ==========
    IEnumerator AngrySequence()
    {
        currentState = BirdState.Angry;

        // 1. 红温闪烁 + 压缩
        yield return StartCoroutine(FlashRedAndShrink());

        // 2. 锁定当前帧Sprite（停在愤怒第一帧）
        anim.Play("BirdAngry", 0, 0);
        anim.speed = 0; // 冻结动画

        // 3. 随机方向移出屏幕
        moveDirection = Random.value > 0.5f ? Vector2.right : Vector2.left;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;

        if (afterImage != null) afterImage.enabled = true;

        float moveTimer = 0f;
        while (moveTimer < 2f) // 最多移动2秒，足够出屏
        {
            rb.velocity = moveDirection * angryMoveSpeed;
            moveTimer += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        if (afterImage != null) afterImage.enabled = false;

        // 4. 延迟3秒
        yield return new WaitForSeconds(peckDelay);

        // 【调试用】运行到这里Console会打印，没打印就是前面飞的环节卡了
        Debug.Log("=== 开始播放啄屏幕动画 ===");

        // 5. 播放敲击屏幕动画 + 裂痕
        anim.Play("BirdPeck");
        anim.speed = 1;

        if (crackScreen != null)
        {
            crackScreen.SetActive(true);
            // 【修改点3】裂痕动画每次从头播放，和啄屏幕同步
            Animator crackAnim = crackScreen.GetComponent<Animator>();
            if (crackAnim != null) crackAnim.Play("CrackAppear", 0, 0);
        }

        // 等待敲击动画播完（假设动画时长约1.5秒，可调整）
        yield return new WaitForSeconds(1.5f);

        // 6. 回到待机
        EnterIdle();
    }

    // 红温闪烁 + 压缩动画
    IEnumerator FlashRedAndShrink()
    {
        float timer = 0f;
        int flashCount = 4; // 闪烁次数

        for (int i = 0; i < flashCount; i++)
        {
            spr.color = Color.red;
            yield return new WaitForSeconds(flashDuration / flashCount / 2f);
            spr.color = Color.white;
            yield return new WaitForSeconds(flashDuration / flashCount / 2f);
        }

        spr.color = Color.red;

        // 压缩Sprite
        timer = 0f;
        float shrinkDuration = 0.3f;
        Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * angryScale, originalScale.z);

        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / shrinkDuration);
            yield return null;
        }
    }
}