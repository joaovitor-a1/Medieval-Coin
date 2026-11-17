using UnityEngine;

/// <summary>
/// Moeda coletável animada. 
/// 
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    public static int Remaining { get; set; }

    [SerializeField] private int value = 1;
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private AudioClip collectSound;

    private Vector3 startPos;
    private float timeOffset;
    private bool isCollected = false;

    private void Awake()
    {
        // Garante que o Collider2D seja Is Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        startPos = transform.position;
        timeOffset = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        // Efeito de flutuação suave (senoidal)
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed + timeOffset) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || !other.CompareTag("Player")) return;

        isCollected = true;

        // Toca o som de coleta caso configurado
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Adiciona a pontuação no GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(value);
        }

        // Desativa a moeda imediatamente (mais seguro que Destroy para a contagem)
        gameObject.SetActive(false);
    }
}