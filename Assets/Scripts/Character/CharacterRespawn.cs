using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRespawn : MonoBehaviour
{
    [Header("死亡音效")]
    public AudioClip deathSfx;                 // 死亡音效
    public AudioSource audioSource;            // 角色身上的 AudioSource（可选）

    public Vector3 currentCheckpoint;   // 当前存档点
    private Vector3 initialPosition;    // 初始出生点

    private void Awake()
    {
        initialPosition = transform.position;
        currentCheckpoint = initialPosition;

        // 自动获取角色身上的 AudioSource（如果有）
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void Die()
    {
        // 播放死亡音效
        PlayDeathSound();

        // 重生
        transform.position = currentCheckpoint;

        // 清空速度
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
    }

    private void PlayDeathSound()
    {
        if (deathSfx == null)
            return;

        // 优先使用角色已有的 AudioSource
        if (audioSource != null)
        {
            audioSource.PlayOneShot(deathSfx);
        }
        else
        {
            // 如果角色身上没挂 AudioSource，则临时创建一个，不会干扰角色
            AudioSource.PlayClipAtPoint(deathSfx, transform.position);
        }
    }

    public void SetCheckpoint(Vector3 pos)
    {
        currentCheckpoint = pos;
        Debug.Log("Checkpoint 更新为：" + pos);
    }
}
