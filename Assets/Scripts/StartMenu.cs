using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    private float startTimer = 3f;
    void Start()
    {
        
    }

    void Update()
    {
        Debug.Log(startTimer);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "start")
        {   
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        }
    }
}
