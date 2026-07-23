using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeWin : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private CoffeeCupBehavior coffeeCupBehavior;

    [Header("任务栏连接")]
    [Tooltip("拖入挂有 TaskChecklistUI 的 TaskCanvas")]
    [SerializeField] private TaskChecklistUI taskChecklist;

    [Tooltip("任务栏中喝咖啡任务的 Task Id")]
    [SerializeField] private string taskId = "coffee";

    [Header("任务状态")]
    public bool taskCompleted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (taskChecklist == null)
        {
            taskChecklist = FindObjectOfType<TaskChecklistUI>();
        }
    }

    void Update()
    {
        // 只要喝咖啡的 AudioSource 开始播放，就判定任务成功。
        if (!taskCompleted && audioSource != null && audioSource.isPlaying)
        {
            CompleteCoffeeTask();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (taskCompleted)
        {
            return;
        }

        if (other.gameObject.name == "CoffeeCup" && !coffeeCupBehavior.spilled)
        {
            audioSource.Play();
            Debug.Log("Coffee drank Dark Souls text");

            // 声音开始播放的同一刻完成任务，避免漏掉很短的音效。
            CompleteCoffeeTask();
        }
    }

    void CompleteCoffeeTask()
    {
        taskCompleted = true;

        if (taskChecklist != null)
        {
            taskChecklist.CompleteTask(taskId);
        }
    }
}

