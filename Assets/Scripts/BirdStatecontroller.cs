using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdStateController : MonoBehaviour
{
    public enum BirdState { Idle, Screaming, Angry }
    // 裂纹硬上限：最多3层，不会再多
    private const int MAX_CRACK_LAYERS = 3;
    [Header("状态时间参数")]
    [Tooltip("待机状态最短持续时间")]
    public float minIdleTime = 5f;
    [Tooltip("待机状态最长持续时间")]
    public float maxIdleTime = 15f;
    [Tooltip("尖叫持续多久后进入愤怒")]
    public float screamDuration = 10f;
    [Tooltip("愤怒后延迟多久播放敲击动画")]
    public float peckDelay = 3f;
    [Tooltip("临时鸟飞走后，原鸟等待几秒回原点")]
    public float birdReturnDelay = 2f;
    [Header("抚摸参数")]
    [Tooltip("安抚成功需要的抚摸次数")]
    public int requiredPets;
    [Tooltip("可选：拖入 HandPrefab。留空时保持原逻辑，任何进入鸟触发器的碰撞体都可建立接触。")]
    public Transform handRoot;
    [Tooltip("手指位置至少变化多少才算真的移动")]
    public float fingerPositionThreshold = 0.002f;
    [Tooltip("手指旋转至少变化多少度才算真的移动")]
    public float fingerRotationThreshold = 0.5f;
    [Tooltip("按键与实际手指动作允许相差的时间")]
    public float fingerMovementMemory = 0.35f;
    [Header("原鸟愤怒移动参数")]
    [Tooltip("原鸟移出屏幕的速度")]
    public float angryMoveSpeed = 15f;
    [Tooltip("红温闪烁时长")]
    public float flashDuration = 0.8f;
    [Tooltip("愤怒时小鸟压缩的比例")]
    public float angryScale = 0.7f;
    [Header("临时啄动画设置")]
    [Tooltip("场景里提前摆好的临时鸟（自己调好位置/大小，默认隐藏）")]
    public GameObject tempBird;
    [Tooltip("啄动画循环时长（秒）：啄多久再飞出去")]
    public float peckLoopDuration = 1f;
    [Tooltip("临时鸟飞出屏幕的速度")]
    public float tempBirdFlySpeed = 15f;
    [Header("裂纹设置")]
    [Tooltip("按顺序拖入3层裂纹：浅裂纹→中裂纹→全碎裂纹")]
    public SpriteRenderer[] crackLayers;
    [Header("引用")]
    [Tooltip("残影脚本引用")]
    public AfterImageEffect afterImage;

    // ====================== 音效设置（全Inspector可调）======================
    [Header("音效设置")]
    [Tooltip("待机状态循环音效")]
    public AudioClip idleSound;
    [Tooltip("尖叫状态循环音效")]
    public AudioClip screamSound;
    [Tooltip("啄屏幕状态循环音效")]
    public AudioClip peckSound;
    [Tooltip("屏幕碎裂单次音效（每次出裂纹都会触发）")]
    public AudioClip crackSound;

    [Tooltip("待机状态音量")]
    [Range(0f, 1f)] public float idleVolume = 0.2f;
    [Tooltip("尖叫刚开始的音量")]
    [Range(0f, 1f)] public float screamStartVolume = 0.3f;
    [Tooltip("尖叫结束/飞出屏幕时的最大音量")]
    [Range(0f, 1f)] public float screamMaxVolume = 0.9f;
    [Tooltip("啄屏幕状态音量")]
    [Range(0f, 1f)] public float peckVolume = 1f;
    [Tooltip("碎裂音效音量")]
    [Range(0f, 1f)] public float crackVolume = 1f;

    [Header("愤怒飞出音效设置")]
    [Tooltip("鸟飞出屏幕后，音量淡出总时长（秒）")]
    public float flyOutFadeDuration = 2f;
    [Tooltip("淡出结束后最终音量（0=完全静音）")]
    [Range(0f, 1f)] public float fadeEndVolume = 0f;
    // ==================================================================

    private AudioSource _audio;
    private Coroutine _activeFadeCoroutine;
    // 任务状态标记，默认false，安抚成功后永久为true
    [Header("任务状态")]
    [Tooltip("玩家是否成功通过抚摸安抚过鸟（任务完成标记）")]
    public bool hasCalmedBird = false;
    [Header("任务栏连接")]
    [Tooltip("拖入挂有 TaskChecklistUI 的 TaskCanvas")]
    public TaskChecklistUI taskChecklist;
    [Tooltip("任务栏中摸鸟任务的 Task Id")]
    public string taskId = "bird";
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
    private HashSet<Collider2D> touchingHandColliders = new HashSet<Collider2D>();
    private Transform[] fingerTransforms;
    private Vector3[] previousFingerPositions;
    private Quaternion[] previousFingerRotations;
    private Vector3[] previousFingerScales;
    private FingerController[] detectedFingerControllers;
    private bool[] previousFingerGrabStates;
    private float fingerMovementValidUntil = -1f;
    private bool fingerActionPending = false;
    // 愤怒移动方向
    private Vector2 moveDirection;
    // 临时鸟初始位置缓存
    private Vector3 tempBirdStartPos;
    // 记录当前裂到第几层，永久不重置，满3层就不再加
    private int currentCrackIndex = 0;
    // 单独记录完整的啄屏幕攻击次数
    private int screenPeckCount = 0;
    // 当前运行的愤怒协程，用于中断
    private Coroutine currentAngryCoroutine;
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        startPosition = transform.position;
        originalScale = transform.localScale;

        // 自动初始化音频组件
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.loop = true;

        InitializeFingerTracking();
        // 缓存临时鸟初始位置，默认隐藏
        if (tempBird != null)
        {
            tempBirdStartPos = tempBird.transform.position;
            tempBird.SetActive(false);
        }
        // 初始状态所有裂纹隐藏
        if (crackLayers != null)
        {
            foreach (var crack in crackLayers)
            {
                if (crack != null) crack.enabled = false;
            }
        }
        // 初始待机状态
        EnterIdle();
    }
    void Update()
    {
        DetectActualFingerMovement();
        switch (currentState)
        {
            case BirdState.Idle:
                UpdateIdle();
                break;
            case BirdState.Screaming:
                UpdatePetting();
                if (currentState == BirdState.Screaming)
                {
                    UpdateScreaming();
                }
                break;
            case BirdState.Angry:
                break;
        }
    }

    // 通用音效切换
    void SwitchSound(AudioClip clip, float volume)
    {
        // 切换音效时停止正在运行的淡出，避免冲突
        if (_activeFadeCoroutine != null)
        {
            StopCoroutine(_activeFadeCoroutine);
            _activeFadeCoroutine = null;
        }

        if (_audio == null) return;
        if (clip == null)
        {
            _audio.Stop();
            return;
        }
        if (_audio.clip != clip)
        {
            _audio.Stop();
            _audio.clip = clip;
            _audio.volume = volume;
            _audio.Play();
        }
        else
        {
            _audio.volume = volume;
            if (!_audio.isPlaying) _audio.Play();
        }
    }

    // 平滑音量淡出协程
    IEnumerator FadeOutSound(float startVolume, float targetVolume, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            _audio.volume = Mathf.Lerp(startVolume, targetVolume, progress);
            yield return null;
        }
        _audio.volume = targetVolume;
        if (Mathf.Approximately(targetVolume, 0f)) _audio.Stop();
        _activeFadeCoroutine = null;
    }

    // ========== 待机状态 ==========
    void EnterIdle()
    {
        currentState = BirdState.Idle;
        transform.position = startPosition;
        transform.localScale = originalScale;
        spr.color = Color.white;
        petCount = 0;
        // 中断所有正在运行的愤怒协程
        if (currentAngryCoroutine != null)
        {
            StopCoroutine(currentAngryCoroutine);
            currentAngryCoroutine = null;
        }
        // 隐藏临时鸟
        if (tempBird != null)
        {
            tempBird.SetActive(false);
            tempBird.transform.position = tempBirdStartPos;
        }
        // 启用物理和碰撞
        rb.bodyType = RigidbodyType2D.Dynamic;
        col.enabled = true;
        rb.velocity = Vector2.zero;
        anim.speed = 1;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        anim.Play("BirdResting");
        if (afterImage != null) afterImage.enabled = false;

        SwitchSound(idleSound, idleVolume);
    }
    void UpdateIdle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0) EnterScreaming();
    }
    // ========== 尖叫状态 ==========
    void EnterScreaming()
    {
        currentState = BirdState.Screaming;
        stateTimer = screamDuration;
        petCount = 0;
        fingerPrevState.Clear();
        anim.Play("BirdAngry");
        SwitchSound(screamSound, screamStartVolume);
    }
    void UpdateScreaming()
    {
        stateTimer -= Time.deltaTime;
        // 尖叫阶段音量线性渐增
        if (_audio != null && _audio.clip == screamSound)
        {
            float screamProgress = 1f - (stateTimer / screamDuration);
            _audio.volume = Mathf.Lerp(screamStartVolume, screamMaxVolume, screamProgress);
        }
        if (stateTimer <= 0) currentAngryCoroutine = StartCoroutine(AngrySequence());
    }
    void UpdatePetting()
    {
        if (!birdTouched) return;
        if (fingerActionPending && Time.time <= fingerMovementValidUntil)
        {
            fingerActionPending = false;
            fingerMovementValidUntil = -1f;
            petCount++;
            if (petCount >= requiredPets)
            {
                hasCalmedBird = true;
                if (taskChecklist != null) taskChecklist.CompleteTask(taskId);
                EnterIdle();
            }
        }
    }
    void InitializeFingerTracking()
    {
        if (handRoot != null)
        {
            detectedFingerControllers = handRoot.GetComponentsInChildren<FingerController>(true);
            List<Transform> detectedTransforms = new List<Transform>();
            HashSet<Transform> uniqueTransforms = new HashSet<Transform>();
            foreach (FingerController controller in detectedFingerControllers)
            {
                if (controller == null) continue;
                Transform[] children = controller.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child != null && uniqueTransforms.Add(child)) detectedTransforms.Add(child);
                }
            }
            fingerTransforms = detectedTransforms.ToArray();
        }
        int controllerCount = detectedFingerControllers == null ? 0 : detectedFingerControllers.Length;
        previousFingerGrabStates = new bool[controllerCount];
        for (int i = 0; i < controllerCount; i++)
        {
            if (detectedFingerControllers[i] != null) previousFingerGrabStates[i] = detectedFingerControllers[i].isGrabbing;
        }
        int count = fingerTransforms == null ? 0 : fingerTransforms.Length;
        previousFingerPositions = new Vector3[count];
        previousFingerRotations = new Quaternion[count];
        previousFingerScales = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            Transform finger = fingerTransforms[i];
            if (finger == null) continue;
            previousFingerPositions[i] = finger.localPosition;
            previousFingerRotations[i] = finger.localRotation;
            previousFingerScales[i] = finger.localScale;
        }
    }
    void DetectActualFingerMovement()
    {
        if (fingerTransforms == null) return;
        bool moved = false;
        for (int i = 0; i < fingerTransforms.Length; i++)
        {
            Transform finger = fingerTransforms[i];
            if (finger == null) continue;
            if (Vector3.Distance(finger.localPosition, previousFingerPositions[i]) > fingerPositionThreshold ||
                Quaternion.Angle(finger.localRotation, previousFingerRotations[i]) > fingerRotationThreshold ||
                Vector3.Distance(finger.localScale, previousFingerScales[i]) > fingerPositionThreshold) moved = true;
            previousFingerPositions[i] = finger.localPosition;
            previousFingerRotations[i] = finger.localRotation;
            previousFingerScales[i] = finger.localScale;
        }
        if (detectedFingerControllers != null)
        {
            for (int i = 0; i < detectedFingerControllers.Length; i++)
            {
                FingerController controller = detectedFingerControllers[i];
                if (controller == null) continue;
                bool fingerState = controller.isGrabbing;
                if (fingerState != previousFingerGrabStates[i])
                {
                    moved = true;
                    previousFingerGrabStates[i] = fingerState;
                }
            }
        }
        if (moved && birdTouched && currentState == BirdState.Screaming)
        {
            fingerActionPending = true;
            fingerMovementValidUntil = Time.time + fingerMovementMemory;
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (!IsHandCollider(other)) return;
        touchingHandColliders.Add(other);
        birdTouched = touchingHandColliders.Count > 0;
    }
    void OnTriggerExit2D(Collider2D other)
    {
        touchingHandColliders.Remove(other);
        birdTouched = touchingHandColliders.Count > 0;
        FingerController finger = other.GetComponent<FingerController>();
        if (finger != null && fingerPrevState.ContainsKey(finger)) fingerPrevState.Remove(finger);
    }
    bool IsHandCollider(Collider2D other)
    {
        if (handRoot == null) return true;
        return other.transform == handRoot || other.transform.IsChildOf(handRoot);
    }
    // ========== 愤怒状态协程 ==========
    IEnumerator AngrySequence()
    {
        currentState = BirdState.Angry;
        SwitchSound(screamSound, screamMaxVolume);

        // 1. 红温闪烁 + 压缩
        yield return StartCoroutine(FlashRedAndShrink());
        // 2. 锁定原鸟在愤怒帧
        anim.Play("BirdAngry", 0, 0);
        anim.speed = 0;
        // 3. 原鸟开始飞出屏幕 → 启动音量淡出
        moveDirection = Random.value > 0.5f ? Vector2.right : Vector2.left;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;
        if (afterImage != null) afterImage.enabled = true;

        // 启动平滑淡出：从当前最大音量，按设置时长淡出到目标音量
        if (_activeFadeCoroutine != null) StopCoroutine(_activeFadeCoroutine);
        _activeFadeCoroutine = StartCoroutine(FadeOutSound(screamMaxVolume, fadeEndVolume, flyOutFadeDuration));

        float moveTimer = 0f;
        while (moveTimer < 2f)
        {
            rb.velocity = moveDirection * angryMoveSpeed;
            moveTimer += Time.deltaTime;
            yield return null;
        }
        rb.velocity = Vector2.zero;
        if (afterImage != null) afterImage.enabled = false;
        // 4. 延迟等待啄屏幕
        yield return new WaitForSeconds(peckDelay);

        // 切啄屏幕循环音效
        SwitchSound(peckSound, peckVolume);

        // 激活临时鸟
        if (tempBird != null)
        {
            tempBird.SetActive(true);
            Animator tempAnim = tempBird.GetComponent<Animator>();
            SpriteRenderer tempSpr = tempBird.GetComponent<SpriteRenderer>();
            if (tempSpr != null) tempSpr.color = Color.white;
            if (tempAnim != null)
            {
                tempAnim.speed = 1;
                tempAnim.Play("BirdPeck", 0, 0);
            }
            float peckTimer = 0f;
            bool countedThisAttack = false;
            while (peckTimer < peckLoopDuration)
            {
                if (tempAnim != null)
                {
                    AnimatorStateInfo stateInfo = tempAnim.GetCurrentAnimatorStateInfo(0);
                    if (stateInfo.normalizedTime >= 0.9f && stateInfo.IsName("BirdPeck"))
                    {
                        tempAnim.Play("BirdPeck", 0, 0);
                        if (!countedThisAttack)
                        {
                            countedThisAttack = true;
                            // 点亮当前层裂纹
                            if (crackLayers != null && currentCrackIndex < crackLayers.Length && currentCrackIndex < MAX_CRACK_LAYERS)
                            {
                                if (crackLayers[currentCrackIndex] != null)
                                {
                                    crackLayers[currentCrackIndex].enabled = true;
                                    // ====================== 每次出裂纹/碎裂都触发音效 ======================
                                    if (crackSound != null) _audio.PlayOneShot(crackSound, crackVolume);
                                    // ==================================================================
                                }
                                currentCrackIndex++;
                            }
                            screenPeckCount++;
                            if (screenPeckCount >= 3)
                            {
                                if (taskChecklist != null) taskChecklist.FailGame();
                                _audio?.Stop();
                                yield break;
                            }
                        }
                    }
                }
                peckTimer += Time.deltaTime;
                yield return null;
            }

            // 啄完停音，进入空窗期
            _audio?.Stop();
            float flyTimer = 0f;
            while (flyTimer < 2f)
            {
                tempBird.transform.Translate(Vector2.left * tempBirdFlySpeed * Time.deltaTime);
                flyTimer += Time.deltaTime;
                yield return null;
            }
            tempBird.SetActive(false);
            tempBird.transform.position = tempBirdStartPos;
        }
        // 空窗期等鸟回来
        yield return new WaitForSeconds(birdReturnDelay);
        currentAngryCoroutine = null;
        EnterIdle();
    }
    // 红温闪烁 + 压缩动画
    IEnumerator FlashRedAndShrink()
    {
        float timer = 0f;
        int flashCount = 4;
        for (int i = 0; i < flashCount; i++)
        {
            spr.color = Color.red;
            yield return new WaitForSeconds(flashDuration / flashCount / 2f);
            spr.color = Color.white;
            yield return new WaitForSeconds(flashDuration / flashCount / 2f);
        }
        spr.color = Color.red;
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
