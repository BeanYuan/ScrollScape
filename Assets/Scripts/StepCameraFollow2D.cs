using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 分屏式摄像机：
/// - 玩家走出当前屏幕时，更新目标屏幕中心；
/// - 摄像机与背景都平滑移动到目标位置；
/// - 背景每次移动为背景宽/高的 1/4，实现简单视差。
/// </summary>
[RequireComponent(typeof(Camera))]
public class StepCameraFollow2D : MonoBehaviour
{
    [Header("目标")]
    public Transform player;          // 拖 Character 进来

    [Header("背景")]
    public Transform background;      // 拖 background_1 进来
    [Range(0f, 1f)]
    public float backgroundStepRatio = 0.25f; // 背景每步 = 背景图尺寸 * 比例

    [Header("平滑参数")]
    public float smoothTime = 0.35f;  // 越大越慢、越柔和

    private Camera cam;
    private float halfWidth;
    private float halfHeight;

    // 当前“屏幕格子”的中心（逻辑坐标）
    private Vector3 regionCenter;

    // 摄像机/背景的平滑目标
    private Vector3 camTargetPos;
    private Vector3 bgTargetPos;

    // SmoothDamp 需要的速度缓存
    private Vector3 camVel;
    private Vector3 bgVel;

    // 背景每步世界位移
    private float bgStepX;
    private float bgStepY;

    void Awake()
    {
        cam = GetComponent<Camera>();

        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        // 初始屏幕中心 = 当前相机位置
        regionCenter = transform.position;
        camTargetPos = transform.position;

        if (background != null)
        {
            bgTargetPos = background.position;

            var sr = background.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Vector2 bgSize = sr.bounds.size;
                bgStepX = bgSize.x * backgroundStepRatio;
                bgStepY = bgSize.y * backgroundStepRatio;
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        // 每帧根据相机当前参数算可视范围
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        // ---------- 1. 先判断玩家是否跑出了当前“屏幕格子” ----------
        bool regionChanged = false;
        Vector3 newRegionCenter = regionCenter;
        Vector3 newBgTargetPos = bgTargetPos;

        float left = regionCenter.x - halfWidth;
        float right = regionCenter.x + halfWidth;
        float bottom = regionCenter.y - halfHeight;
        float top = regionCenter.y + halfHeight;

        // 水平方向
        if (player.position.x > right)
        {
            newRegionCenter.x += halfWidth * 2f; // 下一格往右
            newBgTargetPos.x += bgStepX;
            regionChanged = true;
        }
        else if (player.position.x < left)
        {
            newRegionCenter.x -= halfWidth * 2f; // 下一格往左
            newBgTargetPos.x -= bgStepX;
            regionChanged = true;
        }

        // 垂直方向（允许对角线一次移动一格）
        if (player.position.y > top)
        {
            newRegionCenter.y += halfHeight * 2f; // 下一格往上
            newBgTargetPos.y += bgStepY;
            regionChanged = true;
        }
        else if (player.position.y < bottom)
        {
            newRegionCenter.y -= halfHeight * 2f; // 下一格往下
            newBgTargetPos.y -= bgStepY;
            regionChanged = true;
        }

        if (regionChanged)
        {
            regionCenter = newRegionCenter;

            // 新的相机目标位置（z 保持不变）
            camTargetPos = new Vector3(
                regionCenter.x,
                regionCenter.y,
                transform.position.z
            );

            // 新的背景目标位置
            if (background != null)
                bgTargetPos = new Vector3(
                    newBgTargetPos.x,
                    newBgTargetPos.y,
                    background.position.z
                );
        }

        // ---------- 2. 再把相机/背景平滑地移动到目标点 ----------
        transform.position = Vector3.SmoothDamp(
            transform.position,
            camTargetPos,
            ref camVel,
            smoothTime
        );

        if (background != null)
        {
            background.position = Vector3.SmoothDamp(
                background.position,
                bgTargetPos,
                ref bgVel,
                smoothTime
            );
        }
    }
}


