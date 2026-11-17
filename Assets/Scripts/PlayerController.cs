using UnityEngine;

/// <summary>
/// Movimento de plataforma 2D com caminhada/corrida, pulo único, escalada,
/// gerenciamento de áudio robusto e sistema de morte por Trigger e Colisão Física.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8.5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Áudio Clips")]
    [SerializeField] private AudioClip jumpSound;     // Ficheiro de som do pulo
    [SerializeField] private AudioClip footstepSound; // Ficheiro de som dos passos

    [Header("Escalada")]
    [SerializeField] private float climbSpeed = 4f;

    [Header("Verificação de Chão")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Componentes de áudio atribuídos automaticamente pelo script
    private AudioSource stepsAudioSource;
    private AudioSource jumpAudioSource;

    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isRunning;

    private bool isLadder;
    private bool isClimbing;
    private float originalGravity;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;
        originalGravity = rb.gravityScale;

        // Procura todos os AudioSource presentes no Player
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            stepsAudioSource = sources[0];
            jumpAudioSource = sources[1];
        }
        else if (sources.Length == 1)
        {
            stepsAudioSource = sources[0];
            jumpAudioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            stepsAudioSource = gameObject.AddComponent<AudioSource>();
            jumpAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (isDead) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift);

        // Atualiza a animação
        if (animator != null)
        {
            animator.SetFloat("velocidade", Mathf.Abs(horizontalInput));
        }

        // Vira o sprite
        if (spriteRenderer != null && horizontalInput != 0f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
        }

        // Entra na escalada
        if (isLadder && Mathf.Abs(verticalInput) > 0f)
        {
            isClimbing = true;
        }

        // Pulo único
        if (Input.GetButtonDown("Jump") && isGrounded && !isClimbing && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // Toca o pulo no canal exclusivo de efeitos
            if (jumpAudioSource != null && jumpSound != null)
            {
                jumpAudioSource.PlayOneShot(jumpSound);
            }

            // Cancela os passos imediatamente
            if (stepsAudioSource != null && stepsAudioSource.isPlaying)
            {
                stepsAudioSource.Stop();
            }

            isGrounded = false;
        }

        // Gestão do som dos passos
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        if (stepsAudioSource == null || footstepSound == null) return;

        bool isMovingOnGround = Mathf.Abs(horizontalInput) > 0.1f && isGrounded && !isClimbing && !isDead;

        if (isMovingOnGround)
        {
            if (!stepsAudioSource.isPlaying)
            {
                stepsAudioSource.clip = footstepSound;
                stepsAudioSource.loop = true;
                stepsAudioSource.Play();
            }
        }
        else
        {
            if (stepsAudioSource.isPlaying)
            {
                stepsAudioSource.Stop();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // Atualiza a verificação de chão
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;
        }

        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(horizontalInput * walkSpeed, verticalInput * climbSpeed);
        }
        else
        {
            rb.gravityScale = originalGravity;

            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
        }
    }

    // Detecção para gatilhos sem colisão física (Is Trigger marcado, ex: DeathZone)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Ladder"))
        {
            isLadder = true;
        }

        if (collision.CompareTag("Hazard"))
        {
            Die();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isLadder = false;
            isClimbing = false;
        }
    }

    // Detecção para colisão física sólida (Is Trigger desmarcado, ex: Inimigos e Espinhos)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Hazard"))
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Interrompe o som de passos imediatamente
        if (stepsAudioSource != null && stepsAudioSource.isPlaying)
        {
            stepsAudioSource.Stop();
        }

        // Zera o movimento físico
        rb.linearVelocity = Vector2.zero;

        // Desconta vida e aciona reinício/Game Over no GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}