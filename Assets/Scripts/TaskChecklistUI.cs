using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TaskChecklistUI : MonoBehaviour
{
    public AudioClip winAudio;
    public AudioClip loseAudio;
    public AudioSource bgm;
    [Serializable]
    public class TaskEntry
    {
        [Tooltip("Unique ID: tetris, coffee, bird, or typing")]
        public string taskId;

        public GameObject checkMark;
        public GameObject incompleteMark;

        [Tooltip("Keep the original empty box visible behind the green completion mark.")]
        public bool keepIncompleteMarkWhenCompleted;

        [HideInInspector] public bool completed;
    }

    [Header("Four tasks")]
    public TaskEntry[] tasks;

    [Header("Game complete")]
    [Tooltip("A separate full-screen Canvas or Panel shown after every task is complete.")]
    public GameObject gameCompleteScreen;

    [Tooltip("Optional sound, animation, or other response.")]
    public UnityEvent onAllTasksCompleted;

    [Tooltip("Pause gameplay after the victory screen appears.")]
    public bool pauseGameOnComplete = true;

    [Tooltip("Seconds to wait after all four tasks complete before showing the victory screen.")]
    [Min(0f)] public float victoryDelay = 2.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip taskCompleteClip;

    [Header("Game failure")]
    [Tooltip("A separate full-screen Canvas or Panel shown after the bird pecks the screen three times.")]
    public GameObject gameFailureScreen;

    [Tooltip("Seconds to wait after the third peck before showing the failure screen.")]
    [Min(0f)] public float failureDelay = 1.5f;

    [SerializeField] private int completedTaskCount;
    [SerializeField] private bool gameCompleted;
    [SerializeField] private bool gameFailed;

    public int CompletedTaskCount => completedTaskCount;
    public bool GameCompleted => gameCompleted;
    public bool GameFailed => gameFailed;

    private void Start()
    {
        // Scene reload does not always restore the global time scale.
        // Make sure ESC retry starts the game normally after win or failure.
        Time.timeScale = 1f;

        gameCompleted = false;
        gameFailed = false;
        completedTaskCount = 0;

        if (gameCompleteScreen != null)
        {
            gameCompleteScreen.SetActive(false);
        }

        if (gameFailureScreen != null)
        {
            gameFailureScreen.SetActive(false);
        }

        if (tasks == null)
        {
            return;
        }

        foreach (TaskEntry task in tasks)
        {
            if (task == null)
            {
                continue;
            }

            task.completed = false;

            if (task.checkMark != null)
            {
                task.checkMark.SetActive(false);
            }

            if (task.incompleteMark != null)
            {
                task.incompleteMark.SetActive(true);
            }
        }
    }

    public void CompleteTask(string taskId)
    {
        if (tasks == null || gameFailed)
        {
            return;
        }

        foreach (TaskEntry task in tasks)
        {
            if (task == null ||
                !string.Equals(task.taskId, taskId,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!task.completed)
            {
                task.completed = true;
                completedTaskCount++;
            }

            if (task.checkMark != null)
            {
                task.checkMark.SetActive(true);
            }

            if (task.incompleteMark != null &&
                !task.keepIncompleteMarkWhenCompleted)
            {
                task.incompleteMark.SetActive(false);
            }

            Debug.Log("Task completed: " + taskId);
            // 播放完成音效
            if (audioSource != null && taskCompleteClip != null)
            {
                audioSource.PlayOneShot(taskCompleteClip);
            }

            CheckForGameComplete();
            return;
        }

        Debug.LogWarning(
            "Task ID was not found in TaskChecklistUI: " + taskId);
    }

    public void ResetTask(string taskId)
    {
        if (tasks == null || gameCompleted)
        {
            return;
        }

        foreach (TaskEntry task in tasks)
        {
            if (task == null ||
                !string.Equals(task.taskId, taskId,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (task.completed)
            {
                task.completed = false;
                completedTaskCount = Mathf.Max(0, completedTaskCount - 1);
            }

            if (task.checkMark != null)
            {
                task.checkMark.SetActive(false);
            }

            if (task.incompleteMark != null)
            {
                task.incompleteMark.SetActive(true);
            }

            return;
        }
    }

    public void CompleteTetrisTask() => CompleteTask("tetris");
    public void CompleteCoffeeTask() => CompleteTask("coffee");
    public void CompleteBirdTask() => CompleteTask("bird");
    public void CompleteTypingTask() => CompleteTask("typing");

    private void CheckForGameComplete()
    {
        if (gameCompleted || gameFailed || tasks == null || tasks.Length == 0)
        {
            return;
        }

        foreach (TaskEntry task in tasks)
        {
            if (task == null || !task.completed)
            {
                return;
            }
        }

        gameCompleted = true;

        StartCoroutine(ShowVictoryAfterDelay());
        Debug.Log("All tasks completed. Victory screen will appear after " + victoryDelay + " seconds.");
    }

    private IEnumerator ShowVictoryAfterDelay()
    {
        yield return new WaitForSecondsRealtime(victoryDelay);

        if (gameCompleteScreen != null)
        {
            gameCompleteScreen.SetActive(true);
            bgm.clip = winAudio;
            bgm.loop = false;
        }

        onAllTasksCompleted?.Invoke();

        if (pauseGameOnComplete)
        {
            Time.timeScale = 0f;
        }

        Debug.Log("All tasks completed. Game complete screen shown.");
    }

    public void FailGame()
    {
        if (gameCompleted || gameFailed)
        {
            return;
        }

        gameFailed = true;

        // Prevent a delayed victory screen from appearing after failure.
        StopAllCoroutines();

        if (gameCompleteScreen != null)
        {
            gameCompleteScreen.SetActive(false);
        }

        StartCoroutine(ShowFailureAfterDelay());
        Debug.Log("Game failed. Failure screen will appear after " + failureDelay + " seconds.");
    }

    private IEnumerator ShowFailureAfterDelay()
    {
        yield return new WaitForSecondsRealtime(failureDelay);

        if (gameFailureScreen != null)
        {
            gameFailureScreen.SetActive(true);
            bgm.clip = loseAudio;
            bgm.loop = false;
        }
        else
        {
            Debug.LogWarning("Game failed, but Game Failure Screen is not assigned.");
        }

        Time.timeScale = 0f;
        Debug.Log("Game failed: the bird pecked the screen three times.");
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

    [ContextMenu("TEST - Complete All Tasks (Play Mode Only)")]
    private void CompleteAllTasksForTesting()
    {
        if (!Application.isPlaying || tasks == null)
        {
            Debug.LogWarning("Run this test only while the game is playing.");
            return;
        }

        foreach (TaskEntry task in tasks)
        {
            if (task != null && !string.IsNullOrWhiteSpace(task.taskId))
            {
                CompleteTask(task.taskId);
            }
        }
    }
}