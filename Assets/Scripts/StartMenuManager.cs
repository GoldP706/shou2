using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    [SerializeField]GameObject start;
    void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject == start){
            SceneManager.LoadScene("SampleScene");
        }
    }
}
