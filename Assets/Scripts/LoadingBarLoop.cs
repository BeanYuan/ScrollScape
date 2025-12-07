using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class LoadingBarLoop : MonoBehaviour
{
    public enum LoadingMode
    {
        TwoPhase,   // 阶段1 + 阶段2 + 渐出切场景
        SinglePhase // 只有阶段1，然后渐出切场景
    }

    [Header("加载模式")]
    public LoadingMode mode = LoadingMode.TwoPhase;

    [Header("滚动条移动参数")]
    public float speed = 3f;
    public float leftEdge = -3f;
    public float rightEdge = 3f;

    [Header("Loading 背景图切换（仅 TwoPhase 使用）")]
    public SpriteRenderer loadingSpriteRenderer;
    public Sprite secondLoadingSprite;

    [Header("时间参数")]
    public float timeBeforeSwap = 3f;      // TwoPhase：阶段1时长；SinglePhase：整体 loading 时长
    public float timeAfterSwap = 2f;       // 仅 TwoPhase：阶段2时长（切图后到开始淡出的时间）

    [Header("进入游戏场景")]
    public string targetSceneName = "GameScene";

    [Header("转场淡出设置")]
    public SpriteRenderer fadeOverlay;     // 铺满屏幕的黑色 SpriteRenderer，初始 alpha=0
    public float fadeDuration = 1f;

    [Header("音效设置")]
    public AudioSource audioSource;        // 挂在 Loading 场景的某个物体上（建议同物体）
    public AudioClip phase1Clip;           // 阶段1的 BGM / 循环音效
    public AudioClip phase2Clip;           // 阶段2的“中病毒 duang”音效（仅 TwoPhase 用）
    public bool phase1Loop = true;
    public bool phase2Loop = true;         // 如果只想放一遍，就设为 false

    private SpriteRenderer barRenderer;
    private float halfWidth;

    private float elapsed = 0f;
    private bool swapped = false;
    private bool sceneTriggered = false;
    private bool isMoving = true;

    private void Awake()
    {
        barRenderer = GetComponent<SpriteRenderer>();
        if (barRenderer != null)
            halfWidth = barRenderer.bounds.extents.x;

        if (loadingSpriteRenderer == null)
            Debug.LogWarning("LoadingBarLoop: 没有指定 loadingSpriteRenderer。", this);

        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // 进入场景就播阶段1音效
        PlayPhase1Audio();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        HandleTimeline();

        if (isMoving)
        {
            MoveBar();
        }
    }

    private void HandleTimeline()
    {
        switch (mode)
        {
            case LoadingMode.TwoPhase:
                HandleTimelineTwoPhase();
                break;

            case LoadingMode.SinglePhase:
                HandleTimelineSinglePhase();
                break;
        }
    }

    // ============ 模式 A：TwoPhase ============
    private void HandleTimelineTwoPhase()
    {
        // 阶段切换：到时间 -> 换图 + 换音效 + 停止 Bar
        if (!swapped && elapsed >= timeBeforeSwap)
        {
            swapped = true;
            isMoving = false;

            // 换 loading 图
            if (loadingSpriteRenderer != null && secondLoadingSprite != null)
                loadingSpriteRenderer.sprite = secondLoadingSprite;

            // 切到第二阶段音效
            PlayPhase2Audio();
        }

        // 准备转场：第二阶段结束 -> 开始淡出并切场景
        if (swapped && !sceneTriggered && elapsed >= timeBeforeSwap + timeAfterSwap)
        {
            sceneTriggered = true;
            StartCoroutine(FadeOutAndLoad());
        }
    }

    // ============ 模式 B：SinglePhase ============
    private void HandleTimelineSinglePhase()
    {
        // 只有阶段 1：bar 一直动，只有 phase1 音效
        // 到 timeBeforeSwap 之后，直接开始淡出切场景
        if (!sceneTriggered && elapsed >= timeBeforeSwap)
        {
            sceneTriggered = true;

            // 这里可以选择是否停掉 bar 的移动
            isMoving = false;

            // 不切图、不播第二段音效，直接淡出
            StartCoroutine(FadeOutAndLoad());
        }
    }

    // ============ Bar 移动 ============
    private void MoveBar()
    {
        Vector3 localPos = transform.localPosition;
        localPos.x += speed * Time.deltaTime;

        float leftOfBar = localPos.x - halfWidth;
        float rightOfBar = localPos.x + halfWidth;

        // 超出右侧边界 -> 从左边重新出现
        if (leftOfBar > rightEdge)
        {
            localPos.x = leftEdge - halfWidth;
        }

        transform.localPosition = localPos;
    }

    // ==================  音效相关  ==================

    private void PlayPhase1Audio()
    {
        if (audioSource == null || phase1Clip == null) return;

        audioSource.Stop();
        audioSource.clip = phase1Clip;
        audioSource.loop = phase1Loop;
        audioSource.Play();
    }

    private void PlayPhase2Audio()
    {
        if (audioSource == null || phase2Clip == null) return;

        audioSource.Stop();
        audioSource.clip = phase2Clip;
        audioSource.loop = phase2Loop;
        audioSource.Play();
    }

    // ==================  淡出 + 切场景  ==================

    private IEnumerator FadeOutAndLoad()
    {
        float startVolume = (audioSource != null) ? audioSource.volume : 1f;

        // 1）淡出到黑，同时把音量也淡出
        if (fadeOverlay != null && fadeDuration > 0f)
        {
            Color c = fadeOverlay.color;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeDuration);

                // 画面淡出
                c.a = k;
                fadeOverlay.color = c;

                // 音量淡出
                if (audioSource != null)
                    audioSource.volume = Mathf.Lerp(startVolume, 0f, k);

                yield return null;
            }
        }

        // 2）彻底停音
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = startVolume;   // 重置音量，避免下个场景重用时为 0
        }

        // 3）真正切场景
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("LoadingBarLoop: targetSceneName 为空，无法加载场景。", this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Transform parent = transform.parent;
        float y = (parent != null) ? transform.localPosition.y : transform.position.y;

        Vector3 left = (parent != null)
            ? parent.TransformPoint(new Vector3(leftEdge, y, 0))
            : new Vector3(leftEdge, y, 0);

        Vector3 right = (parent != null)
            ? parent.TransformPoint(new Vector3(rightEdge, y, 0))
            : new Vector3(rightEdge, y, 0);

        Gizmos.DrawLine(left + Vector3.down * 0.1f, left + Vector3.up * 0.1f);
        Gizmos.DrawLine(right + Vector3.down * 0.1f, right + Vector3.up * 0.1f);
    }
}
