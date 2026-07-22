using System;
using UnityEngine;
using UnityEngine.Events;

public class TaskChecklistUI : MonoBehaviour
{
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

    [SerializeField] private int completedTaskCount;
    [SerializeField] private bool gameCompleted;

    public int CompletedTaskCount => completedTaskCount;
    public bool GameCompleted => gameCompleted;

    private void Start()
    {
        gameCompleted = false;
        completedTaskCount = 0;

        if (gameCompleteScreen != null)
        {
            gameCompleteScreen.SetActive(false);
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
        if (tasks == null)
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
        if (gameCompleted || tasks == null || tasks.Length == 0)
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

        if (gameCompleteScreen != null)
        {
            gameCompleteScreen.SetActive(true);
        }

        onAllTasksCompleted?.Invoke();
        Debug.Log("All tasks completed. Game complete screen shown.");
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
