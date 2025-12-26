using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI scoreText;
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    public RectTransform safeZone;
    public RectTransform redZone;

    float tapTimer;
    public float tapLimit = 3f; // 1 second

    public float startTapLimit = 10f;  // 10 seconds at start
    public float minTapLimit = 2f;     // never below 2 seconds

    float currentTapLimit;

    // float score;
    int score;

    bool gameRunning;

    int highScore;

    public TextMeshProUGUI highScoreText; // optional on UI

    bool isPaused;

    public GameObject pausePanel;

    public GameObject pauseButton;

    public RectTransform timerFill;


    void Awake()
    {
        Instance = this;

        highScore = PlayerPrefs.GetInt("HIGH_SCORE", 0);
    }

    // void Update()
    // {
    //     if (!gameRunning) return;

    //     score += Time.deltaTime;
    //     scoreText.text = "Score: " + Mathf.FloorToInt(score);
    // }

    // void Update()
    // {
    //     if (!gameRunning) return;

    //     tapTimer += Time.deltaTime;

    //     // player is too slow
    //     if (tapTimer >= tapLimit)
    //     {
    //         GameOver();
    //         return;
    //     }

    //     scoreText.text = "Score: " + Mathf.FloorToInt(score);
    // }

    // void Update()
    // {
    //     if (!gameRunning) return;

    //     tapTimer += Time.deltaTime;

    //     if (tapTimer >= tapLimit)
    //     {
    //         GameOver(); // player too slow
    //     }
    // }

    void Update()
    {
        // if (!gameRunning) return;
        if (!gameRunning || isPaused) return;

        tapTimer += Time.deltaTime;

        UpdateTimerBar();

        if (tapTimer >= currentTapLimit)
        {
            GameOver(); // too slow
        }
    }


    // public void StartGame()
    // {
    //     score = 0;
    //     gameRunning = true;
    //     startPanel.SetActive(false);
    //     gameOverPanel.SetActive(false);
    // }

    // public void StartGame()
    // {
    //     gameRunning = true;

    //     score = 0;
    //     tapTimer = 0f;
    //     tapLimit = 1.2f;   // reset difficulty

    //     scoreText.text = "Score: 0";

    //     startPanel.SetActive(false);
    //     gameOverPanel.SetActive(false);
    // }

    // public void StartGame()
    // {
    //     gameRunning = true;

    //     score = 0;
    //     tapTimer = 0f;
    //     tapLimit = 1f;

    //     scoreText.text = "Score: 0";

    //     startPanel.SetActive(false);
    //     gameOverPanel.SetActive(false);

    //     RandomizeZones();
    // }

    public void StartGame()
    {
        gameRunning = true;

        score = 0;
        tapTimer = 0f;
        UpdateTimerBar();

        currentTapLimit = startTapLimit;

        scoreText.text = "Score: 0";

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        highScoreText.text = "Best: " + FormatScore(highScore);

        pauseButton.SetActive(true); 

        RandomizeZones();
    }




    public void GameOver()
    {
        gameRunning = false;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HIGH_SCORE", highScore);
            PlayerPrefs.Save();
        }

        highScoreText.text = "Best: " + FormatScore(highScore);

        finalScoreText.text = "Score: " + FormatScore(Mathf.FloorToInt(score));
        gameOverPanel.SetActive(true);

        pauseButton.SetActive(false);  // ❌ HIDE pause
    }

    public void SafeTap()
    {
        tapTimer = 0f;
        UpdateTimerBar();

        score += 1;
        MoveRedToRandomPosition();

        // increase difficulty slowly
        tapLimit = Mathf.Max(0.4f, tapLimit - 0.02f);
    }

    void MoveRedToRandomPosition()
    {
        RectTransform red = redZone;
        float x = Random.Range(-Screen.width / 2 + 200, Screen.width / 2 - 200);
        float y = Random.Range(-Screen.height / 2 + 200, Screen.height / 2 - 200);

        red.anchoredPosition = new Vector2(x, y);
    }

    public bool IsGameRunning()
    {
        return gameRunning;
    }

    // public void OnSafeTap()
    // {
    //     if (!gameRunning) return;

    //     score++;
    //     scoreText.text = "Score: " + score;

    //     tapTimer = 0f;

    //     // increase difficulty
    //     tapLimit = Mathf.Max(0.4f, tapLimit - 0.03f);

    //     RandomizeZones();
    // }

    public void OnSafeTap()
    {
        if (!gameRunning) return;

        tapTimer = 0f;
        // UpdateTimerBar();

        score++;
        scoreText.text = "Score: " + score;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HIGH_SCORE", highScore);
            PlayerPrefs.Save();
        }

        highScoreText.text = "Best: " + FormatScore(highScore);

        tapTimer = 0f;

        UpdateTapLimit();

        RandomizeZones();
    }


    // void RandomizeZones()
    // {
    //     bool redOnLeft = Random.value > 0.5f;

    //     if (redOnLeft)
    //     {
    //         redZone.anchoredPosition = new Vector2(-Screen.width / 4f, 0);
    //         safeZone.anchoredPosition = new Vector2(Screen.width / 4f, 0);
    //     }
    //     else
    //     {
    //         redZone.anchoredPosition = new Vector2(Screen.width / 4f, 0);
    //         safeZone.anchoredPosition = new Vector2(-Screen.width / 4f, 0);
    //     }
    // }

    void RandomizeZones()
    {
        bool redOnLeft = Random.value > 0.5f;

        // place red zone
        SetZoneSide(redZone, redOnLeft);
        // place safe zone on opposite side
        SetZoneSide(safeZone, !redOnLeft);
    }


    void SetZoneSide(RectTransform zone, bool leftSide)
    {
        if (leftSide)
        {
            zone.anchorMin = new Vector2(0f, 0f);
            zone.anchorMax = new Vector2(0.5f, 1f);
        }
        else
        {
            zone.anchorMin = new Vector2(0.5f, 0f);
            zone.anchorMax = new Vector2(1f, 1f);
        }

        zone.offsetMin = Vector2.zero;
        zone.offsetMax = Vector2.zero;
    }

    void UpdateTapLimit()
    {
        // Reduce time every score
        // Example: after 0 score → 10s
        // after 20 score → ~2s

        // Special hard mode after score 100
        if (score >= 100)
        {
            currentTapLimit = 1f; // force 1 second
            return;
        }

        float reductionPerPoint = (startTapLimit - minTapLimit) / 20f;

        currentTapLimit = startTapLimit - (score * reductionPerPoint);

        currentTapLimit = Mathf.Clamp(currentTapLimit, minTapLimit, startTapLimit);

        Debug.Log("Current Tap Time: " + currentTapLimit);
    }

    string FormatScore(int value)
    {
        if (value >= 1_000_000_000)
            return (value / 1_000_000_000f).ToString("0.#") + "B";
        if (value >= 1_000_000)
            return (value / 1_000_000f).ToString("0.#") + "M";
        if (value >= 1_000)
            return (value / 1_000f).ToString("0.#") + "K";

        return value.ToString();
    }

    public void PauseGame()
    {
        if (!gameRunning) return;

        isPaused = true;
        pausePanel.SetActive(true);

        pauseButton.SetActive(false);  // ❌ HIDE pause
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        pauseButton.SetActive(true);   // ✅ SHOW pause
    }

    public void QuitGame()
    {
        isPaused = false;
        gameRunning = false;

        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        pauseButton.SetActive(false);  // ❌ HIDE pause

        startPanel.SetActive(true);
    }

    void UpdateTimerBar()
    {
        float remaining = Mathf.Clamp01(1f - (tapTimer / currentTapLimit));

        timerFill.localScale = new Vector3(remaining, 1f, 1f);

        // Color feedback
        var img = timerFill.GetComponent<UnityEngine.UI.Image>();

        if (remaining > 0.5f)
            img.color = Color.green;
        else if (remaining > 0.25f)
            img.color = Color.yellow;
        else
            img.color = Color.red;
    }



}
