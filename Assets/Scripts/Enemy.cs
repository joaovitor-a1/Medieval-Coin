using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Coloque este script no objeto do inimigo ou obstáculo.
/// Reinicia a fase atual quando o jogador colide com ele.
/// </summary>
public class Enemy : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o objeto que entrou no Trigger tem a tag "Player"
        if (collision.CompareTag("Player"))
        {
            ReiniciarFase();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Funciona se a colisão física não estiver marcada como Is Trigger
        if (collision.gameObject.CompareTag("Player"))
        {
            ReiniciarFase();
        }
    }

    private void ReiniciarFase()
    {
        // Reinicia a cena ativa no momento
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}