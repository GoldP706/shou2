using UnityEngine;

// Run after ordinary scene scripts so the start screen remains paused even
// if another script restores Time.timeScale while initializing the scene.
[DefaultExecutionOrder(1000)]
public class StartScreenController : MonoBehaviour
{
    private static bool skipStartScreenOnNextLoad;

    [Header("Start screen")]
    [Tooltip("The Canvas or Panel that contains the start-page image and buttons. Leave empty when this script is attached directly to that object.")]
    public GameObject startScreen;

    [Tooltip("Pause gameplay while the start page is visible.")]
    public bool pauseOnStart = false;

    [Header("Gameplay disabled before start")]
    [Tooltip("Optional but recommended: a parent containing all gameplay objects. Keep Main Camera, HandPrefab, EventSystem, StartCanvas and TransitionCanvas outside it.")]
    public GameObject gameplayRoot;

    [Tooltip("Drag gameplay scripts such as BirdStateController and ComputerTask here. Do not add the hand movement or hand controller scripts.")]
    public Behaviour[] gameplayScriptsToDisable;

    [Header("Hand visible above start page")]
    [Tooltip("Drag the scene HandPrefab here. Its SpriteRenderers are temporarily drawn above the Start Canvas.")]
    public Transform handRoot;

    [Tooltip("Set the Start Canvas Sort Order lower than this value, for example Canvas 200 and hand 500.")]
    public int startHandSortingOrder = 500;

    private SpriteRenderer[] handRenderers;
    private int[] originalHandSortingOrders;

    [Header("Start transition")]
    [Tooltip("CanvasGroup on a separate full-screen black TransitionCanvas.")]
    public CanvasGroup blackFade;

    [Min(0f)] public float snappedHoldDuration = 0.4f;
    [Min(0.01f)] public float fadeToBlackDuration = 0.6f;
    [Min(0.01f)] public float fadeFromBlackDuration = 0.6f;

    private bool transitionStarted;
    private bool skippedForRestart;

    public static void SkipStartScreenOnNextLoad()
    {
        skipStartScreenOnNextLoad = true;
        Time.timeScale = 1f;
    }

    private void Awake()
    {
        if (startScreen == null)
        {
            startScreen = gameObject;
        }

        if (skipStartScreenOnNextLoad)
        {
            skipStartScreenOnNextLoad = false;
            skippedForRestart = true;

            if (blackFade != null)
            {
                blackFade.alpha = 0f;
                blackFade.blocksRaycasts = false;
                blackFade.interactable = false;
                blackFade.gameObject.SetActive(false);
            }

            if (gameplayRoot != null)
            {
                gameplayRoot.SetActive(true);
            }

            SetGameplayScriptsEnabled(true);
            Time.timeScale = 1f;
            startScreen.SetActive(false);
            return;
        }

        startScreen.SetActive(true);

        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(false);
        }

        SetGameplayScriptsEnabled(false);
        PutHandAboveStartScreen();

        if (blackFade != null)
        {
            blackFade.alpha = 0f;
            blackFade.blocksRaycasts = false;
            blackFade.interactable = false;
            blackFade.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (skippedForRestart)
        {
            return;
        }

        // Dragging the Start block uses Rigidbody2D and TargetJoint2D, so the
        // start page must keep time running when Pause On Start is disabled.
        Time.timeScale = pauseOnStart ? 0f : 1f;
    }

    public void StartGame()
    {
        if (!transitionStarted)
        {
            transitionStarted = true;
            StartCoroutine(StartGameTransition());
        }
    }

    private System.Collections.IEnumerator StartGameTransition()
    {
        yield return new WaitForSecondsRealtime(snappedHoldDuration);

        if (blackFade != null)
        {
            blackFade.gameObject.SetActive(true);
            blackFade.blocksRaycasts = true;
            yield return FadeBlack(0f, 1f, fadeToBlackDuration);
        }

        // Switch scenes visually only while the screen is fully black.
        Canvas startCanvas = startScreen != null
            ? startScreen.GetComponent<Canvas>()
            : null;

        if (startCanvas != null)
        {
            startCanvas.enabled = false;
        }

        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(true);
        }

        SetGameplayScriptsEnabled(true);
        RestoreHandSorting();
        ClearStartBlockFromHand();
        Time.timeScale = 1f;

        if (blackFade != null)
        {
            yield return FadeBlack(1f, 0f, fadeFromBlackDuration);
            blackFade.blocksRaycasts = false;
            blackFade.gameObject.SetActive(false);
        }

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeBlack(float from, float to, float duration)
    {
        float elapsed = 0f;
        blackFade.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            blackFade.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        blackFade.alpha = to;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetGameplayScriptsEnabled(bool value)
    {
        if (gameplayScriptsToDisable == null)
        {
            return;
        }

        foreach (Behaviour behaviour in gameplayScriptsToDisable)
        {
            if (behaviour != null)
            {
                behaviour.enabled = value;
            }
        }
    }

    private void PutHandAboveStartScreen()
    {
        if (handRoot == null)
        {
            return;
        }

        handRenderers = handRoot.GetComponentsInChildren<SpriteRenderer>(true);
        originalHandSortingOrders = new int[handRenderers.Length];

        for (int i = 0; i < handRenderers.Length; i++)
        {
            originalHandSortingOrders[i] = handRenderers[i].sortingOrder;
            handRenderers[i].sortingOrder =
                startHandSortingOrder + originalHandSortingOrders[i];
        }
    }

    private void RestoreHandSorting()
    {
        if (handRenderers == null || originalHandSortingOrders == null)
        {
            return;
        }

        for (int i = 0; i < handRenderers.Length; i++)
        {
            if (handRenderers[i] != null)
            {
                handRenderers[i].sortingOrder = originalHandSortingOrders[i];
            }
        }
    }

    private void ClearStartBlockFromHand()
    {
        if (handRoot == null)
        {
            return;
        }

        HandControllerNew handController =
            handRoot.GetComponentInChildren<HandControllerNew>(true);

        if (handController != null)
        {
            handController.closestObj = null;
            handController.heldObj = null;
            handController.grabbing = false;
        }
    }
}

