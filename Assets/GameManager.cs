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

    public GameObject timerBG;
    public RectTransform timerFill;

    int tapScoreMultiplier = 1;

    enum SplitMode
    {
        Vertical,
        Horizontal
    }

    SplitMode currentSplitMode;

    public int horizontalUnlockScore = 50;

    void Awake()
    {
        Instance = this;

        highScore = PlayerPrefs.GetInt("HIGH_SCORE", 0);
    }

    void Update()
    {
        if (!gameRunning || isPaused) return;

        tapTimer += Time.deltaTime;

        UpdateTimerBar();

        if (tapTimer >= currentTapLimit)
        {
            GameOver(); // too slow
        }
    }

    public void StartGame()
    {
        gameRunning = true;

        tapScoreMultiplier = 1; // reset upgrade

        score = 0;
        tapTimer = 0f;

        timerBG.SetActive(true);
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
        timerBG.SetActive(false);

        AdsManager.Instance.ShowInterstitial();
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

    public void OnSafeTap()
    {
        if (!gameRunning) return;

        tapTimer = 0f;
        // UpdateTimerBar();

        // score++;
        score += tapScoreMultiplier;
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

    void RandomizeZones()
    {
        // Before score 50 → ONLY vertical
        if (score < horizontalUnlockScore)
        {
            Debug.Log("HORIZONTAL MODE UNLOCKED!");
            currentSplitMode = SplitMode.Vertical;
        }
        else
        {
            // After score 50 → vertical OR horizontal
            currentSplitMode = (Random.value > 0.5f)
                ? SplitMode.Vertical
                : SplitMode.Horizontal;
        }

        // Randomly decide which side is SAFE
        bool safeOnPrimarySide = Random.value > 0.5f;

        SetZoneSide(safeZone, safeOnPrimarySide);
        SetZoneSide(redZone, !safeOnPrimarySide);
    }

    void SetZoneSide(RectTransform zone, bool isPrimarySide)
    {
        if (currentSplitMode == SplitMode.Vertical)
        {
            // LEFT / RIGHT
            if (isPrimarySide)
            {
                // LEFT
                zone.anchorMin = new Vector2(0f, 0f);
                zone.anchorMax = new Vector2(0.5f, 1f);
            }
            else
            {
                // RIGHT
                zone.anchorMin = new Vector2(0.5f, 0f);
                zone.anchorMax = new Vector2(1f, 1f);
            }
        }
        else
        {
            // 🔥 HORIZONTAL (TOP / BOTTOM)
            if (isPrimarySide)
            {
                // TOP
                zone.anchorMin = new Vector2(0f, 0.5f);
                zone.anchorMax = new Vector2(1f, 1f);
            }
            else
            {
                // BOTTOM
                zone.anchorMin = new Vector2(0f, 0f);
                zone.anchorMax = new Vector2(1f, 0.5f);
            }
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
        timerBG.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        pauseButton.SetActive(true);   // ✅ SHOW pause
        timerBG.SetActive(true);
    }

    public void QuitGame()
    {
        isPaused = false;
        gameRunning = false;

        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        pauseButton.SetActive(false);  // ❌ HIDE pause

        startPanel.SetActive(true);
        timerBG.SetActive(false);
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

    public void ActivateScoreUpgrade()
    {
        tapScoreMultiplier = 2;
        Debug.Log("Rewarded upgrade activated: +1 extra score per tap");
    }

    public void DebugUpgrade()
    {
        // TEMP – simulate rewarded ad success
        ActivateScoreUpgrade();
    }

}
