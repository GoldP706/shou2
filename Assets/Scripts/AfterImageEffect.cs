using System.Collections;
using UnityEngine;

public class AfterImageEffect : MonoBehaviour
{
    [Tooltip("残影生成间隔")]
    public float spawnInterval = 0.3f;
    [Tooltip("残影淡出时间")]
    public float fadeDuration = 0.5f;
    [Tooltip("残影颜色（建议半透明）")]
    public Color afterImageColor = new Color(1f, 1f, 1f, 0.5f);

    private SpriteRenderer spr;
    private float spawnTimer;

    void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
        enabled = false; // 默认关闭，愤怒时由主脚本开启
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnAfterImage();
        }
    }

    void SpawnAfterImage()
    {
        GameObject img = new GameObject("AfterImage");
        img.transform.position = transform.position;
        img.transform.rotation = transform.rotation;
        img.transform.localScale = transform.localScale;

        SpriteRenderer imgSpr = img.AddComponent<SpriteRenderer>();
        imgSpr.sprite = spr.sprite;
        imgSpr.color = afterImageColor;
        imgSpr.sortingLayerID = spr.sortingLayerID;
        imgSpr.sortingOrder = spr.sortingOrder - 1;

        StartCoroutine(FadeOutAndDestroy(img, imgSpr));
    }

    IEnumerator FadeOutAndDestroy(GameObject obj, SpriteRenderer imgSpr)
    {
        float timer = 0f;
        Color startColor = imgSpr.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, timer / fadeDuration);
            imgSpr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(obj);
    }
}