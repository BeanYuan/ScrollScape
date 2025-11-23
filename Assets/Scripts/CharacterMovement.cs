using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;

    [Header("跳跃参数")]
    public float jumpForce = 12f;
    public float maxJumpHoldTime = 0.2f;

    [Header("落地检测（矩形方式）")]
    public LayerMask groundLayer;
    public float groundBoxHeight = 0.1f;
    public float groundBoxOffset = 0.05f;
    public float groundBoxWidthScale = 0.9f;

    [Header("连跳手感优化")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;
    public float maxFallSpeed = -25f;

    [Header("可选：角色朝向")]
    public Transform spriteTransform;

    [Header("音效")]
    public AudioClip jumpSfx;            // 跳跃音效
    public AudioSource audioSource;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private float moveInput;

    private bool isGrounded;
    private bool isJumping;
    private float jumpHoldCounter;

    private float coyoteCounter;
    private float jumpBufferCounter;

    private Animator anim;

    private Transform currentPlatform;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (spriteTransform == null)
        {
            Transform t = transform.Find("Sprite");
            if (t != null) spriteTransform = t;
        }

        if (spriteTransform != null)
        {
            anim = spriteTransform.GetComponent<Animator>();
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // ---- 落地检测 ----
        CheckGroundedBox();

        // ---- 更新土狼时间 ----
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // ---- 跳跃缓冲 ----
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // ---- 起跳 ----
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            isJumping = true;
            jumpHoldCounter = maxJumpHoldTime;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;

            if (audioSource != null && jumpSfx != null)
            {
                audioSource.PlayOneShot(jumpSfx);
            }
        }

        // ---- 长按跳 ----
        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpHoldCounter > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpHoldCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        // ---- 松开跳键降低上升速度 ----
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
            if (rb.velocity.y > 0f)
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // ---- 限制最大下落速度 ----
        if (rb.velocity.y < maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);

        // ---- ★ 新增：驱动动画（Idle/Walk）----
        if (anim != null)
        {
            // speed = 横向移动速度的绝对值
            anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        }

        HandleFlip();
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    // ---- 矩形脚底检测 ----
    void CheckGroundedBox()
    {
        if (col == null)
        {
            isGrounded = false;
            DetachFromPlatform();  // 没碰撞体就保险起见解掉父子
            return;
        }

        Bounds bounds = col.bounds;

        Vector2 boxCenter = new Vector2(
            bounds.center.x,
            bounds.min.y - groundBoxOffset
        );

        Vector2 boxSize = new Vector2(
            bounds.size.x * groundBoxWidthScale,
            groundBoxHeight
        );

        bool wasGrounded = isGrounded;

        // ★ 关键：拿到脚下命中的 collider
        Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);
        isGrounded = (hit != null);

        // ---- 处理移动平台父子关系 ----
        UpdatePlatformParent(hit);

        // 刚刚落地瞬间：刷新土狼时间，让连跳绝对不卡
        if (isGrounded && !wasGrounded && rb.velocity.y <= 0f)
        {
            coyoteCounter = coyoteTime;
            isJumping = false;
        }
    }

    // 根据脚下的 collider 决定是否挂到移动平台上
    void UpdatePlatformParent(Collider2D groundHit)
    {
        Transform newPlatform = null;

        // 只有在“真的落地”的时候才考虑平台，否则在空中不挂任何平台
        if (isGrounded && groundHit != null && groundHit.CompareTag("MovingPlatform"))
        {
            newPlatform = groundHit.transform;
        }

        if (newPlatform == currentPlatform)
        {
            // 脚下平台没变，不用处理
            return;
        }

        // 从旧平台下来：取消父子关系
        if (currentPlatform != null && (newPlatform == null || newPlatform != currentPlatform))
        {
            DetachFromPlatform();
        }

        // 上了新平台：挂上去
        if (newPlatform != null)
        {
            currentPlatform = newPlatform;
            // 保持当前世界坐标不变，只改变 parent
            transform.SetParent(currentPlatform, true);
        }
    }

    // 强制解除平台父子关系
    void DetachFromPlatform()
    {
        if (currentPlatform != null)
        {
            transform.SetParent(null, true);
            currentPlatform = null;
        }
    }


    void HandleFlip()
    {
        if (spriteTransform == null) return;

        if (moveInput > 0.01f)
            spriteTransform.localScale = new Vector3(-2f, spriteTransform.localScale.y, spriteTransform.localScale.z);
        else if (moveInput < -0.01f)
            spriteTransform.localScale = new Vector3(2f, spriteTransform.localScale.y, spriteTransform.localScale.z);
    }

    void OnDrawGizmosSelected()
    {
        if (col == null) return;

        Bounds bounds = col.bounds;

        Vector2 boxCenter = new Vector2(
            bounds.center.x,
            bounds.min.y - groundBoxOffset
        );

        Vector2 boxSize = new Vector2(
            bounds.size.x * groundBoxWidthScale,
            groundBoxHeight
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
