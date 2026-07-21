using System;
using UnityEngine;

public class TaskChecklistUI : MonoBehaviour
{
    [Serializable]
    public class TaskEntry
    {
        [Tooltip("A unique ID, for example: tetris")]
        public string taskId;

        [Tooltip("The tick/checkmark object shown when this task is complete.")]
        public GameObject checkMark;

        [Tooltip("Optional object shown before completion, such as an empty box.")]
        public GameObject incompleteMark;
    }

    public TaskEntry[] tasks;

    private void Start()
    {
        foreach (TaskEntry task in tasks)
        {
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
        foreach (TaskEntry task in tasks)
        {
            if (!string.Equals(task.taskId, taskId,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (task.checkMark != null)
            {
                task.checkMark.SetActive(true);
            }

            if (task.incompleteMark != null)
            {
                task.incompleteMark.SetActive(false);
            }

            Debug.Log("Task completed: " + taskId);
            return;
        }

        Debug.LogWarning("Task ID was not found in TaskChecklistUI: " + taskId);
    }

    public void ResetTask(string taskId)
    {
        foreach (TaskEntry task in tasks)
        {
            if (!string.Equals(task.taskId, taskId,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
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
}
