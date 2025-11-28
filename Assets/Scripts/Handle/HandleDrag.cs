using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HandleDrag : MonoBehaviour
{
    public ScrollWindowController window;
    public LayerMask handleLayerMask;

    [Header("滚动音效")]
    public AudioSource audioSource;       // 专门给滚动用的 AudioSource（挂在 handle 上或父物体上）
    public AudioClip dragStartClip;       // 开始拖动时播放一次
    public AudioClip dragLoopClip;        // 拖动过程中循环播放
    public AudioClip dragEndClip;         // 松开时播放一次

    [Header("悬浮高亮")]
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);  // 默认：全白（如果本来是偏灰，就会略微变亮）

    private Camera cam;
    private bool dragging;
    private Vector3 dragOffset;

    private Vector2 handleHalfSize;
    private Collider2D myCollider;

    private Bounds scrollBounds;

    // 高亮相关
    private SpriteRenderer sr;
    private Color originalColor;
    private bool hasOriginalColor = false;

    void Start()
    {
        cam = Camera.main;

        if (window == null)
            window = GetComponentInParent<ScrollWindowController>();

        myCollider = GetComponent<Collider2D>();

        // 获取 handle sprite 半尺寸 + SpriteRenderer
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            handleHalfSize = sr.bounds.extents;
            originalColor = sr.color;
            hasOriginalColor = true;
        }
        else
        {
            handleHalfSize = Vector2.zero;
        }

        // 不自动添加 AudioSource，按你习惯在 Inspector 手动挂
        // if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        if (cam == null || window == null) return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, Mathf.Infinity, handleLayerMask);

        if (hit.collider == myCollider)
        {
            dragging = true;
            dragOffset = transform.position - (Vector3)mouseWorld;
            scrollBounds = window.GetScrollBounds();

            // 👉 开始拖动音效
            if (audioSource != null)
            {
                if (dragStartClip != null)
                    audioSource.PlayOneShot(dragStartClip);

                // 循环滚动音效
                if (dragLoopClip != null)
                {
                    audioSource.clip = dragLoopClip;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
        else
        {
            dragging = false;
        }
    }

    void OnMouseDrag()
    {
        if (!dragging || cam == null || window == null)
            return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        // 先从当前坐标开始，按需要的轴分别更新
        Vector3 targetPos = transform.position;

        // 计算可移动范围（两轴共用）
        float minX = scrollBounds.min.x + handleHalfSize.x;
        float maxX = scrollBounds.max.x - handleHalfSize.x;
        float minY = scrollBounds.min.y + handleHalfSize.y;
        float maxY = scrollBounds.max.y - handleHalfSize.y;

        // 只在父级勾选了 horizontal 时才允许改变 X
        if (window.enableHorizontal)
        {
            float rawX = mouseWorld.x + dragOffset.x;
            targetPos.x = Mathf.Clamp(rawX, minX, maxX);
        }

        // 只在父级勾选了 vertical 时才允许改变 Y
        if (window.enableVertical)
        {
            float rawY = mouseWorld.y + dragOffset.y;
            targetPos.y = Mathf.Clamp(rawY, minY, maxY);
        }

        transform.position = targetPos;

        // 归一化值，供 ScrollWindowController 使用
        float tX = Mathf.InverseLerp(minX, maxX, targetPos.x);
        float tY = Mathf.InverseLerp(minY, maxY, targetPos.y);

        window.SetScroll01(tX, tY);
    }

    void OnMouseUp()
    {
        if (!dragging)
            return;

        dragging = false;

        if (audioSource != null)
        {
            // 停止循环滚动音效
            if (audioSource.loop && audioSource.clip == dragLoopClip)
                audioSource.Stop();

            // 播放结束音效
            if (dragEndClip != null)
                audioSource.PlayOneShot(dragEndClip);
        }
    }

    // ---------- 悬浮高亮逻辑 ----------
    void OnMouseEnter()
    {
        // 鼠标第一次碰到这个 collider 时触发
        if (sr != null && hasOriginalColor)
        {
            sr.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        // 鼠标离开 collider 时恢复原色
        if (sr != null && hasOriginalColor)
        {
            sr.color = originalColor;
        }
    }
}
