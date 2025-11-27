using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CloseWindowButton : MonoBehaviour
{
    [Header("要关闭的窗口根节点（默认为父节点）")]
    public GameObject windowRoot;

    [Header("点击音效（AudioSource 必须挂在 windowRoot 上）")]
    public AudioClip clickSfx;

    private AudioSource windowAudio;   // 从 windowRoot 获取
    private bool clicked = false;

    void Awake()
    {
        // 默认窗口 = 父级
        if (windowRoot == null && transform.parent != null)
            windowRoot = transform.parent.gameObject;

        // 使用 windowRoot 上已有的 AudioSource，不再自动添加！
        if (windowRoot != null)
            windowAudio = windowRoot.GetComponent<AudioSource>();

        if (windowAudio == null && clickSfx != null)
            Debug.LogWarning("CloseWindowButton: windowRoot 上没有 AudioSource，但你设置了 clickSfx。音效将无法播放。", this);
    }

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;

        // 播放窗口自己的音效
        if (windowAudio != null && clickSfx != null)
            windowAudio.PlayOneShot(clickSfx);

        HideWindow(windowRoot);
    }

    /// <summary>
    /// 隐藏窗口（不 SetActive(false)）
    /// </summary>
    void HideWindow(GameObject root)
    {
        if (root == null) return;

        root.SetActive(false);
    }


    // GameObject 不 SetActive(false)，所以 windowAudio 还能继续播放
}
