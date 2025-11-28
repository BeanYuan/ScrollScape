using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一次性激活按钮：
/// - 玩家用鼠标单击时，启用指定的 GameObject / 组件；
/// - 播放一段点击音效；
/// - 播放完成动画：图标飞向目标并变小后消失；
/// - 动画结束后：本体 SpriteRenderer 隐藏，Collider 停用（不 SetActive(false)）；
/// - 鼠标悬浮时高亮提示。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class OneShotActivator : MonoBehaviour
{
    [Header("要启用的对象（SetActive(true)）")]
    public List<GameObject> targetsToActivate = new List<GameObject>();

    [Header("要启用的组件（enabled = true）")]
    public List<Behaviour> componentsToEnable = new List<Behaviour>();

    [Header("点击音效")]
    public AudioClip clickSfx;                 // 你准备好的音效
    public AudioSource audioSource;            // 建议手动挂在同一物体或父物体上

    [Header("完成动画参数")]
    public float flyDuration = 0.4f;           // 飞行时长
    public float endScaleFactor = 0.2f;        // 变小到原来的多少倍
    public AnimationCurve moveCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("点击后行为")]
    public bool disableColliderAfterClick = true;  // 动画结束后关碰撞
    public bool disableScriptAfterClick = true;    // 动画结束后关脚本（可选）

    [Header("悬浮高亮")]
    public Color hoverColor = new Color(1f, 0.9f, 0.7f, 1f);  // 鼠标悬浮时颜色
    private Color originalColor;
    private bool hasOriginalColor = false;

    private bool hasActivated = false;
    private Collider2D col;
    private SpriteRenderer sr;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            originalColor = sr.color;
            hasOriginalColor = true;
        }

        // 不强制自动添加 AudioSource，但如果本体上有就用它
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void OnMouseDown()
    {
        // 已经激活过了就直接无视
        if (hasActivated) return;

        hasActivated = true;

        // 开启整体流程（带动画）
        StartCoroutine(ActivateRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        // 1) 先激活目标对象 / 组件（逻辑上立刻生效）
        foreach (var go in targetsToActivate)
        {
            if (go != null)
                go.SetActive(true);
        }

        foreach (var comp in componentsToEnable)
        {
            if (comp != null)
                comp.enabled = true;
        }

        // 2) 播放点击音效
        if (audioSource != null && clickSfx != null)
        {
            audioSource.PlayOneShot(clickSfx);
        }

        // 3) 记录本体初始 Transform（位置/缩放）
        Vector3 originalPos = transform.position;
        Vector3 originalScale = transform.localScale;

        // 4) 为每个目标生成一个“飞行体”副本
        List<GameObject> ghosts = new List<GameObject>();
        List<Transform> ghostTargets = new List<Transform>();

        foreach (var go in targetsToActivate)
        {
            if (go == null) continue;

            // 复制本体作为飞行体
            GameObject ghost = Instantiate(
                this.gameObject,
                originalPos,
                Quaternion.identity,
                transform.parent   // 放在同一个父物体下面
            );

            // 清理飞行体上不需要的组件
            var ghostActivator = ghost.GetComponent<OneShotActivator>();
            if (ghostActivator != null) Destroy(ghostActivator); // 防止它也响应点击

            var ghostCollider = ghost.GetComponent<Collider2D>();
            if (ghostCollider != null) ghostCollider.enabled = false; // 不参与碰撞/点击

            // 确保 SpriteRenderer 是开的（显示图标）
            var ghostSr = ghost.GetComponent<SpriteRenderer>();
            if (ghostSr != null) ghostSr.enabled = true;

            ghosts.Add(ghost);
            ghostTargets.Add(go.transform);
        }

        // 5) 点击后本体立即隐藏视觉 & 关闭碰撞（逻辑上仍保留在场景）
        if (sr != null) sr.enabled = false;
        if (disableColliderAfterClick && col != null) col.enabled = false;

        // 6) 执行飞行动画（如果没有目标，就直接结束）
        if (ghosts.Count > 0)
        {
            float timer = 0f;

            while (timer < flyDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / flyDuration);

                float moveT = moveCurve.Evaluate(t);
                float scaleT = scaleCurve.Evaluate(t);

                for (int i = 0; i < ghosts.Count; i++)
                {
                    if (ghosts[i] == null || ghostTargets[i] == null) continue;

                    Vector3 start = originalPos;
                    Vector3 end = ghostTargets[i].position;

                    // 位置插值
                    Vector3 pos = Vector3.Lerp(start, end, moveT);
                    ghosts[i].transform.position = pos;

                    // 缩放插值：从原来大小 → 原来 * endScaleFactor
                    float sLerp = Mathf.Lerp(1f, endScaleFactor, scaleT);
                    ghosts[i].transform.localScale = originalScale * sLerp;
                }

                yield return null;
            }

            // 7) 动画结束：销毁所有飞行体
            for (int i = 0; i < ghosts.Count; i++)
            {
                if (ghosts[i] != null)
                    Destroy(ghosts[i]);
            }
        }

        // 8) 本体 Transform 复原（位置/缩放），方便以后复用或关卡编辑
        transform.position = originalPos;
        transform.localScale = originalScale;

        // 保持 sr.disabled & collider.disabled 的状态

        // 9) 最终可选：关闭脚本（彻底 one-shot）
        if (disableScriptAfterClick)
        {
            enabled = false;
        }
    }

    // ========= 悬浮高亮 =========
    void OnMouseEnter()
    {
        if (hasActivated) return;           // 已经点过就不再高亮
        if (sr != null && hasOriginalColor)
        {
            sr.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        if (sr != null && hasOriginalColor)
        {
            sr.color = originalColor;
        }
    }
}
