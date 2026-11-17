using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla pontuação, vidas, vitória, tela de Game Over, transição de fases, 
/// reinício e música de fundo contínua entre as cenas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Música de Fundo (BGM)")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.4f;

    [Header("UI (TextMeshPro)")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winText;          // Texto dentro do painel de vitória
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;     // Texto dentro do painel de Game Over

    [Header("Configurações de Vidas")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int firstStageIndex = 0;

    [Header("Configurações de Transição")]
    [SerializeField] private float delayParaProximaFase = 2f;
    [SerializeField] private float delayParaGameOver = 2f;

    private AudioSource bgmAudioSource;
    private int score;
    private int totalCoins;
    private int currentLives;
    private bool won;
    private bool isGameOver;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            bgmAudioSource = gameObject.AddComponent<AudioSource>();
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentLives = maxLives;
    }

    private void Start()
    {
        TocarMusicaDeFundo();
        InicializarFase();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InicializarFase();
    }

    private void TocarMusicaDeFundo()
    {
        if (backgroundMusic != null && bgmAudioSource != null)
        {
            if (!bgmAudioSource.isPlaying)
            {
                bgmAudioSource.clip = backgroundMusic;
                bgmAudioSource.volume = musicVolume;
                bgmAudioSource.Play();
            }
        }
    }

    private void InicializarFase()
    {
        Time.timeScale = 1f;
        won = false;
        isGameOver = false;
        score = 0;

        BuscarReferenciasUI();

        totalCoins = FindObjectsByType<Coin>().Length;

        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateUI();
    }

    private void BuscarReferenciasUI()
    {
        if (livesText == null)
        {
            GameObject livesObj = GameObject.Find("LivesText");
            if (livesObj != null) livesText = livesObj.GetComponent<TMP_Text>();
        }

        if (scoreText == null)
        {
            GameObject scoreObj = GameObject.Find("ScoreText");
            if (scoreObj != null) scoreText = scoreObj.GetComponent<TMP_Text>();
        }

        if (winPanel == null) winPanel = GameObject.Find("WinPanel");
        if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");

        if (winText == null && winPanel != null)
        {
            winText = winPanel.GetComponentInChildren<TMP_Text>();
        }

        if (gameOverText == null && gameOverPanel != null)
        {
            gameOverText = gameOverPanel.GetComponentInChildren<TMP_Text>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void AddScore(int amount)
    {
        if (won || isGameOver) return;

        score += amount;
        UpdateUI();

        if (!won && score >= totalCoins && totalCoins > 0)
        {
            Win();
        }
    }

    public void TakeDamage(int damageAmount = 1)
    {
        if (won || isGameOver) return;

        currentLives -= damageAmount;
        UpdateUI();

        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        if (gameOverText != null)
        {
            gameOverText.text = "VOCÊ PERDEU!";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        StartCoroutine(RestartFromFirstStageRoutine());
    }

    private IEnumerator RestartFromFirstStageRoutine()
    {
        yield return new WaitForSeconds(delayParaGameOver);

        currentLives = maxLives;
        SceneManager.LoadScene(firstStageIndex);
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Moedas: {score}/{totalCoins}";
        }

        if (livesText != null)
        {
            livesText.text = $"Vidas: {currentLives}";
        }
    }

    private void Win()
    {
        won = true;

        int proximaCenaIndex = SceneManager.GetActiveScene().buildIndex + 1;
        bool eAUltimaFase = proximaCenaIndex >= SceneManager.sceneCountInBuildSettings;

        // Personaliza a mensagem se for a última fase ou transição normal
        if (winText != null)
        {
            if (eAUltimaFase)
            {
                winText.text = "Você terminou o jogo";
            }
            else
            {
                winText.text = "Fase Concluída!";
            }
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        StartCoroutine(CarregarProximaFaseRoutine(proximaCenaIndex, eAUltimaFase));
    }

    private IEnumerator CarregarProximaFaseRoutine(int proximaCenaIndex, bool eAUltimaFase)
    {
        yield return new WaitForSeconds(delayParaProximaFase);

        if (!eAUltimaFase)
        {
            SceneManager.LoadScene(proximaCenaIndex);
        }
        else
        {
            Debug.Log("Jogo finalizado com sucesso!");
        }
    }
}