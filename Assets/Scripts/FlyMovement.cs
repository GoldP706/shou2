using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyMovement : MonoBehaviour
{
    [Header("Random Drift")]
    public float driftSpeed = 1f;
    public float directionChangeTime = 1.5f;
    public float driftSmoothness = 3f;

    // ���ص��������Ķ��ٺ�ָ��������
    public float safeDistance = 3f;

    private Vector3 driftDirection;
    private Vector3 targetDriftDirection;

    private float driftTimer;

    // �Ƿ����ڷ�����Ļ
    private bool returningToScreen = false;

    //check if dead
    public bool isDead = false;
    private GameObject hand;
    [SerializeField] HandControllerNew handController;

    private Rigidbody2D rb;

    void Start()
    {
        PickNewDriftDirection();
        driftDirection = targetDriftDirection;
        hand = GameObject.Find("HandPrefab");
        handController = hand.GetComponent<HandControllerNew>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //Debug.Log(isDead);
        RandomDrift();
        if(isDead){
            driftSpeed = 0;
            rb.gravityScale = 3;
        }
    }

    void RandomDrift()
    {
        //--------------------------
        // ����״̬
        //--------------------------
        if (returningToScreen)
        {
            // ����Ļ���ķ�
            targetDriftDirection = (Vector3.zero - transform.position).normalized;

            driftDirection = Vector3.Lerp(
                driftDirection,
                targetDriftDirection,
                driftSmoothness * Time.deltaTime
            );

            transform.position += driftDirection * driftSpeed * Time.deltaTime;

            // �Ѿ��ص���ȫ����
            if (Vector3.Distance(transform.position, Vector3.zero) <= safeDistance)
            {
                returningToScreen = false;
                driftTimer = 0;
                PickNewDriftDirection();
            }

            return;
        }

        //--------------------------
        // ���Ư��
        //--------------------------
        driftTimer += Time.deltaTime;

        if (driftTimer >= directionChangeTime)
        {
            PickNewDriftDirection();
            driftTimer = 0;
        }

        driftDirection = Vector3.Lerp(
            driftDirection,
            targetDriftDirection,
            driftSmoothness * Time.deltaTime
        );

        transform.position += driftDirection * driftSpeed * Time.deltaTime;
    }

    void PickNewDriftDirection()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        targetDriftDirection = new Vector3(
            randomDirection.x * 2f,
            randomDirection.y,
            0
        ).normalized;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("border"))
        {
            returningToScreen = true;
        }
    }

//catch fly
    void OnTriggerStay2D(Collider2D other){
        if (other.CompareTag("catchCollider") && handController.handState == 0){
            if(Input.GetKey(KeyCode.Space)){
                if(Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.R)||Input.GetKeyDown(KeyCode.T)){                    isDead = true;
                    isDead = true;
                    //Destroy(gameObject);
                }
            }
        }
        if (other.CompareTag("border"))
        {
            returningToScreen = true;
        }
    }
}