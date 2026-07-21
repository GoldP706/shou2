using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyManager : MonoBehaviour
{
    private float stingWaitTimer; // time until next sting
    public float stingWaitTimerMax;
    private float stingingTimer; // time until sting is completed
    public float stingingTimeMax;

    private float respawnTimer = 3f;
    [SerializeField] bool canRespawn = false;
    private int elbowCount = 0;
    private bool isStinging = false;

    [SerializeField]SpriteRenderer stingWarningSpr;
    [SerializeField]FlyMovement flyMovement;

    [SerializeField]GameObject flyPrefab;
    [SerializeField] GameObject currentFly;

    void Start()
    {
        stingWaitTimer = stingWaitTimerMax;
        flyMovement = flyPrefab.GetComponent<FlyMovement>();
        currentFly = GameObject.FindWithTag("Fly");
    }

    void Update()
    {
        if(canRespawn){
            respawnTimer -= Time.deltaTime;
            if(respawnTimer <= 0){
                Spawn();
                respawnTimer = 3f;
            }
        }

        if(!flyMovement.isDead){
            stingWaitTimer -= Time.deltaTime;
            if(stingWaitTimer<=0){
                Sting();
                stingWaitTimer = stingWaitTimerMax;
            }
        }
        if(currentFly == null){
            canRespawn = true;
        }

        if(isStinging){
            stingingTimer += Time.deltaTime;

        }

        if(Input.GetKeyDown(KeyCode.Mouse0) && isStinging){
            elbowCount += 1;
        }
    }

    void Sting(){
        isStinging = true;
    }

    void Spawn(){
        var spawn = new Vector3(15f,Random.Range(10,-10),0f);
        GameObject Fly = Instantiate(flyPrefab,spawn,Quaternion.identity);
        currentFly = Fly;
        flyMovement = flyPrefab.GetComponent<FlyMovement>();
        canRespawn = false;
    }
}
