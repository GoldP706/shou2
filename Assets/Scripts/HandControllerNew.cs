using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControllerNew : MonoBehaviour
{
    public bool grabbing = false;
    public GameObject closestObj;
    public GameObject heldObj;

    [SerializeField] FingerController thumb;
    [SerializeField] FingerController pointer;
    [SerializeField] FingerController middle;
    [SerializeField] FingerController ring;
    [SerializeField] FingerController little;

    // ===== 涟漪相关参数 =====
    [SerializeField] GameObject ripplePrefab;       // 涟漪预制体
    [SerializeField] float minSpeed = 0.5f;         // 触发涟漪的最小速度（低于这个不生成）
    [SerializeField] float maxSpeed = 5f;           // 速度上限（用于归一化强度）
    [SerializeField] float rippleInterval = 0.15f;  // 涟漪生成间隔（秒，避免太密）
    [SerializeField] float minRippleScale = 0.5f;   // 最小涟漪缩放
    [SerializeField] float maxRippleScale = 2f;     // 最大涟漪缩放

    private float handHeight = 1f;

    // ===== 涟漪内部变量 =====
    private Vector3 lastObjPosition;
    private float rippleTimer;
    private bool wasHolding = false;

    //private bool holdingObj = false;

    void Start()
    {

    }

    void Update()
    {
        Debug.Log(grabbing);
        //z movement 
        if (Input.GetKey(KeyCode.Mouse0) && handHeight > 0f)
        {
            handHeight -= 0.1f;
        }

        if (Input.GetKey(KeyCode.Mouse1) && handHeight > 1f)
        {
            handHeight += 0.1f;
        }

        //grab and move
        if (thumb.isGrabbing)
        {
            if (pointer.isGrabbing || middle.isGrabbing || ring.isGrabbing || little.isGrabbing)
            {
                grabbing = true;
            }
            else { grabbing = false; }
        }
        else { grabbing = false; }

        if (closestObj != null && grabbing)
        {
            heldObj = closestObj;
            heldObj.GetComponent<TargetJoint2D>().target = transform.position;
        }
        else
        {
            heldObj = null;
        }

        // ===== 速度驱动涟漪 =====
        if (heldObj != null)
        {
            // 刚抓住的第一帧只记录位置，不计算速度
            if (!wasHolding)
            {
                lastObjPosition = heldObj.transform.position;
                rippleTimer = 0f;
            }
            else
            {
                // 计算当前帧移动速度
                float distance = Vector3.Distance(heldObj.transform.position, lastObjPosition);
                float speed = distance / Time.deltaTime;

                // 速度超过阈值才考虑生成涟漪
                if (speed >= minSpeed)
                {
                    rippleTimer += Time.deltaTime;
                    if (rippleTimer >= rippleInterval)
                    {
                        // 根据速度计算涟漪强度（0~1归一化）
                        float intensity = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
                        float scale = Mathf.Lerp(minRippleScale, maxRippleScale, intensity);

                        SpawnRipple(heldObj.transform.position, scale);
                        rippleTimer = 0f;
                    }
                }
                else
                {
                    // 速度不够时重置计时
                    rippleTimer = rippleInterval;
                }

                lastObjPosition = heldObj.transform.position;
            }
        }

        wasHolding = heldObj != null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "CanGrab" && closestObj == null && grabbing == false)
        {
            closestObj = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "CanGrab")
        {
            closestObj = null;
        }
    }

    // ===== 生成涟漪（带强度缩放）=====
    void SpawnRipple(Vector3 position, float scale)
    {
        if (ripplePrefab != null)
        {
            GameObject ripple = Instantiate(
                ripplePrefab,
                new Vector3(position.x, position.y, 0),
                Quaternion.identity
            );
            ripple.transform.localScale = Vector3.one * scale;
        }
    }
}