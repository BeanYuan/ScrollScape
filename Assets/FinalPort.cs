using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 当玩家进入本物体的触发器范围时，跳转到 LoadingScene。
/// 把这个脚本挂在 FinalPort 对象上，
/// FinalPort 需要有 BoxCollider2D，并勾选 "Is Trigger"。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FinalPort : MonoBehaviour
{
    [Header("需要检测的玩家 TAG")]
    public string playerTag = "Player";

    [Header("要加载的场景名称")]
    public string loadingSceneName = "LoadingScene";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只触发玩家
        if (other.CompareTag(playerTag))
        {
            // 切场景
            SceneManager.LoadScene(loadingSceneName);
        }
    }
}

