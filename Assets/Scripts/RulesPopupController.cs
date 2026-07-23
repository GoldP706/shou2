using UnityEngine;

public class RulesPopupController : MonoBehaviour
{
    [Header("操作介绍页面")]
    [Tooltip("直接拖入整个 RulesCanvas。脚本不要挂在 RulesCanvas 上。")]
    public GameObject rulesCanvas;

    [Tooltip("打开操作介绍时暂停游戏。")]
    public bool pauseWhileOpen = true;

    [Header("正式游戏")]
    [Tooltip("可选：拖入 GameplayRoot，可防止在封面和开场介绍中按 Tab。")]
    public GameObject gameplayRoot;

    [Header("运行状态（只读）")]
    [SerializeField] private bool isOpen;

    private float timeScaleBeforeOpening = 1f;

    private void Awake()
    {
        isOpen = false;

        if (rulesCanvas == gameObject)
        {
            Debug.LogError(
                "RulesPopupController 不要挂在 RulesCanvas 上。" +
                "请把脚本挂在 SceneManager 上，然后把整个 RulesCanvas 拖入脚本。",
                this
            );
            rulesCanvas = null;
            return;
        }

        PrepareRulesCanvas();

        if (rulesCanvas != null)
        {
            rulesCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameplayRoot != null && !gameplayRoot.activeInHierarchy)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleRules();
        }
    }

    public void ToggleRules()
    {
        if (isOpen)
        {
            CloseRules();
        }
        else
        {
            OpenRules();
        }
    }

    public void OpenRules()
    {
        if (isOpen || rulesCanvas == null)
        {
            return;
        }

        timeScaleBeforeOpening = Time.timeScale;
        isOpen = true;

        rulesCanvas.SetActive(true);
        PrepareRulesCanvas();

        if (pauseWhileOpen)
        {
            Time.timeScale = 0.0f;
        }
    }

    public void CloseRules()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        if (rulesCanvas != null)
        {
            rulesCanvas.SetActive(false);
        }

        if (pauseWhileOpen)
        {
            Time.timeScale = timeScaleBeforeOpening;
        }
    }

    private void PrepareRulesCanvas()
    {
        if (rulesCanvas == null)
        {
            return;
        }

        Canvas canvas = rulesCanvas.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = rulesCanvas.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
        canvas.enabled = true;

        CanvasGroup[] canvasGroups =
            rulesCanvas.GetComponentsInChildren<CanvasGroup>(true);

        foreach (CanvasGroup group in canvasGroups)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }

    private void OnDisable()
    {
        if (isOpen && pauseWhileOpen)
        {
            Time.timeScale = timeScaleBeforeOpening;
        }
    }
}

