using UnityEngine;

/// <summary>
/// Controla o comportamento de perseguição simples do inimigo em relação ao jogador.
/// </summary>
public class EnemyFollow : MonoBehaviour
{
    [Header("Configurações de Perseguição")]
    [SerializeField] private float speed = 3f;           // Velocidade de movimento do inimigo
    [SerializeField] private float detectionRange = 7f;   // Distância máxima para detectar o jogador
    [SerializeField] private float stopDistance = 0.5f;   // Distância mínima do jogador para parar

    private Transform player;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Localiza o objeto do Player na cena pela Tag
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Calcula a distância atual até o jogador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Persegue o jogador apenas se estiver dentro do raio de detecção e além da distância de parada
        if (distanceToPlayer <= detectionRange && distanceToPlayer > stopDistance)
        {
            // Move o inimigo na direção do jogador
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime
            );

            // Vira o sprite na direção em que está caminhando
            if (spriteRenderer != null)
            {
                if (player.position.x < transform.position.x)
                {
                    spriteRenderer.flipX = true;  // Olhando para a esquerda
                }
                else if (player.position.x > transform.position.x)
                {
                    spriteRenderer.flipX = false; // Olhando para a direita
                }
            }
        }
    }

    // Desenha o raio de detecção no editor para facilitar o ajuste visual
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}