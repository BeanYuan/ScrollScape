using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一次性激活按钮：
/// - 玩家用鼠标单击时，启用指定的 GameObject / 组件；
/// - 播放一段点击音效；
/// - 之后这个按钮再被点击不会有任何反应（可选：顺便关闭碰撞器）
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
    public AudioSource audioSource;            // 不指定就自动找自己身上的

    [Header("点击后行为")]
    public bool disableColliderAfterClick = true;  // 防止再次点击
    public bool disableScriptAfterClick = true;    // 彻底关闭脚本

    private bool hasActivated = false;
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (audioSource == null && clickSfx != null)
        {
            // 尝试自动获取/添加一个 AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    void OnMouseDown()
    {
        // 已经激活过了就直接无视
        if (hasActivated) return;

        hasActivated = true;

        // 1) 激活 GameObject
        foreach (var go in targetsToActivate)
        {
            if (go != null)
                go.SetActive(true);
        }

        // 2) 启用组件
        foreach (var comp in componentsToEnable)
        {
            if (comp != null)
                comp.enabled = true;
        }

        // 3) 播放音效
        if (audioSource != null && clickSfx != null)
        {
            audioSource.PlayOneShot(clickSfx);
        }

        // 4) 防止之后继续被点
        if (disableColliderAfterClick && col != null)
        {
            col.enabled = false;
        }

        if (disableScriptAfterClick)
        {
            // 只关自己脚本，不会影响已经启用的对象
            enabled = false;
        }
    }
}

