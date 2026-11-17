using UnityEngine;

/// <summary>
/// Faz o inimigo patrulhar de um lado para o outro.
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float speed = 2f;         // Velocidade do inimigo
    [SerializeField] private float moveDistance = 3f;  // Distância máxima de patrulha

    private Vector3 startPos;
    private bool movingRight = true;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Limites de patrulha com base na posição inicial
        float rightLimit = startPos.x + moveDistance;
        float leftLimit = startPos.x - moveDistance;

        if (movingRight)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);

            if (transform.position.x >= rightLimit)
            {
                movingRight = false;
                Flip();
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);

            if (transform.position.x <= leftLimit)
            {
                movingRight = true;
                Flip();
            }
        }
    }

    private void Flip()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !movingRight;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha a linha da área de patrulha no Editor
        Vector3 origin = Application.isPlaying ? startPos : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin + Vector3.left * moveDistance, origin + Vector3.right * moveDistance);
    }
}