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

    bool isHorizontalShown;
    bool is20LevelShown;
    bool is40LevelShown;
    bool is80LevelShown;
    bool is200LevelShown;
    bool is400LevelShown;
    bool is900LevelShown;

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

    enum ZoneLayoutMode
    {
        Split,
        CenterSafe,
        CenterRed
    }

    ZoneLayoutMode currentLayoutMode;


    public int horizontalUnlockScore = 50;

    public GameObject rewardSuccessText;
    public TextMeshProUGUI rewardSuccessTextText;

    public GameObject levelTextObj;
    public TextMeshProUGUI levelText;

    public TextMeshProUGUI safeHintText;

    public Image safeZoneImage;
    public Image redZoneImage;

    bool isCenterModeShown;

    enum LevelMessageType
    {
        Info,
        Speed,
        Unlock,
        Warning
    }

    void Awake()
    {
        Instance = this;

        highScore = PlayerPrefs.GetInt("HIGH_SCORE", 0);

        // 🔥 HOME SCREEN INIT
        highScoreText.text = "Best: " + FormatScore(highScore);
        highScoreText.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!gameRunning || isPaused) return;

        tapTimer += Time.deltaTime;

        UpdateTimerBar();
        UpdateSafeZoneCountdown();

        if (tapTimer >= currentTapLimit)
        {
            GameOver(); // too slow
        }
    }

    public void StartGame()
    {
        SetZoneRaycast(true);

        isHorizontalShown = false;
        is900LevelShown = false;
        is400LevelShown = false;
        is200LevelShown = false;
        is80LevelShown = false;
        is40LevelShown = false;
        is20LevelShown = false;
        isCenterModeShown = false;

        gameRunning = true;

        tapScoreMultiplier = 1; // reset upgrade

        score = 0;
        tapTimer = 0f;

        timerBG.SetActive(true);
        UpdateTimerBar();

        currentTapLimit = startTapLimit;

        scoreText.text = "Score: 0";
        levelText.text = "Level";

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        highScoreText.text = "Best: " + FormatScore(highScore);

        pauseButton.SetActive(true); 
        rewardSuccessText.SetActive(false);

        safeHintText.gameObject.SetActive(true);
        UpdateSafeZoneCountdown();

        RandomizeZones();
    }

    public void GameOver()
    {
        gameRunning = false;

        SetZoneRaycast(false); // 🔥 important

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

        safeHintText.gameObject.SetActive(false);

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
        UpdateSafeZoneCountdown();
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

    // void RandomizeZones()
    // {
    //     // Before score 50 → ONLY vertical
    //     if (score < horizontalUnlockScore)
    //     {
    //         currentSplitMode = SplitMode.Vertical;
    //     }
    //     else
    //     {
    //         // After score 50 → vertical OR horizontal
    //         if (!isHorizontalShown) {
    //             isHorizontalShown = true;
    //             levelText.text = "HORIZONTAL MODE UNLOCKED!"; 
    //             levelTextObj.SetActive(true);
    //             Invoke(nameof(HideLevelText), 2f);
    //         }
    //         currentSplitMode = (Random.value > 0.5f)
    //             ? SplitMode.Vertical
    //             : SplitMode.Horizontal;
    //     }

    //     // Randomly decide which side is SAFE
    //     bool safeOnPrimarySide = Random.value > 0.5f;

    //     SetZoneSide(safeZone, safeOnPrimarySide);
    //     SetZoneSide(redZone, !safeOnPrimarySide);
    // }

    void RandomizeZones()
    {
        // 🔥 RESET FIRST (THIS FIXES FULL SCREEN ISSUE)
        ResetRect(safeZone);
        ResetRect(redZone);
        
        // -------------------------
        // 1️⃣ Decide split mode
        // -------------------------
        if (score < horizontalUnlockScore)
        {
            currentSplitMode = SplitMode.Vertical;
        }
        else
        {
            if (!isHorizontalShown)
            {
                isHorizontalShown = true;
                // levelText.text = "HORIZONTAL MODE UNLOCKED!";
                // levelTextObj.SetActive(true);
                // Invoke(nameof(HideLevelText), 2f);
                ShowLevelMessage("HORIZONTAL MODE UNLOCKED!", LevelMessageType.Unlock);
            }

            currentSplitMode = (Random.value > 0.5f)
                ? SplitMode.Vertical
                : SplitMode.Horizontal;
        }

        // -------------------------
        // 2️⃣ Decide layout mode
        // -------------------------
        if (score < 100)
        {
            currentLayoutMode = ZoneLayoutMode.Split;
        }
        else
        {
            // 🔥 Center mode unlock info
            if (score >= 100 && !isCenterModeShown)
            {
                isCenterModeShown = true;

                // levelText.text = "CENTER ZONE MODE UNLOCKED!";
                // levelTextObj.SetActive(true);
                // Invoke(nameof(HideLevelText), 2f);
                ShowLevelMessage("CENTER ZONE MODE UNLOCKED!", LevelMessageType.Unlock);
            }

            int rand = Random.Range(0, 100);

            if (rand < 60)
                currentLayoutMode = ZoneLayoutMode.Split;       // 60%
            else if (rand < 80)
                currentLayoutMode = ZoneLayoutMode.CenterSafe;  // 20%
            else
                currentLayoutMode = ZoneLayoutMode.CenterRed;   // 20%
        }

        // -------------------------
        // 3️⃣ Apply layout
        // -------------------------
        if (currentLayoutMode == ZoneLayoutMode.Split)
        {

            bool safeOnPrimarySide = Random.value > 0.5f;

            // ResetRect(safeZone);
            // ResetRect(redZone);

            SetZoneSide(safeZone, safeOnPrimarySide);
            SetZoneSide(redZone, !safeOnPrimarySide);

            safeHintText.text = "TOUCH HERE";
            HintTextCenter();
        }
        else if (currentLayoutMode == ZoneLayoutMode.CenterSafe)
        {
            ApplyCenterSafeLayout();
        }
        else
        {
            ApplyCenterRedLayout();
        }
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

        if (score >= 20 && score < 40) {
            currentTapLimit = 9f; // force 1.5 second
            if (!is20LevelShown) {
                is20LevelShown = true;

                // levelText.text = "Tap Timer Set To 9 Seconds!"; 
                // levelTextObj.SetActive(true);
                // Invoke(nameof(HideLevelText), 2f);
                ShowLevelMessage("TAP TIMER: 9 SECONDS", LevelMessageType.Speed);
            }
            return;
        }
        if (score >= 40 && score < 80)
        {
            currentTapLimit = 7.5f; // force 1.5 second
            if (!is40LevelShown) {
                is40LevelShown = true;

                // levelText.text = "Tap Timer Set To 7.5 Seconds!"; 
                // levelTextObj.SetActive(true);
                // Invoke(nameof(HideLevelText), 2f);
                ShowLevelMessage("TAP TIMER: 7.5 SECONDS", LevelMessageType.Speed);
            }
            return;
        }
        // Special hard mode after score 100
        if (score >= 80 && score < 200)
        {
            currentTapLimit = 5f; // force 1.5 second
            if (!is80LevelShown) {
                is80LevelShown = true;

                // levelText.text = "Tap Timer Set To 5 Seconds!"; 
                // levelTextObj.SetActive(true);
                // Invoke(nameof(HideLevelText), 2f);
                ShowLevelMessage("TAP TIMER: 5 SECONDS", LevelMessageType.Speed);
            }
            return;
        }

        if (score >= 200 && score < 400)
        {
            currentTapLimit = 2.5f; // force 2.5 second
            if (!is200LevelShown) {
                is200LevelShown = true;

                // levelText.text = "Tap Timer Set To 2.5 Seconds!"; 
                // levelTextObj.SetActive(true);
                // Invoke(nameof(HideLevelText), 2f);
                ShowLevelMessage("TAP TIMER: 2.5 SECONDS", LevelMessageType.Speed);
            }
            return;
        }

        if (score >= 400 && score < 900)
        {
            currentTapLimit = 1f; // force 1 second

            if (!is400LevelShown) {
                is400LevelShown = true;
                // levelText.text = "Tap Timer Set To 1 Seconds!"; 
                // levelTextObj.SetActive(true);
                // Invoke(nameof(HideLevelText), 2f);
                ShowLevelMessage("TAP TIMER: 1 SECONDS", LevelMessageType.Speed);
            }
            return;
        }

        if (score >= 900)
        {
            if (!is900LevelShown) {
                is900LevelShown = true;

                levelText.text = "Tap Timer Set To 0.5 Seconds!"; 
                levelTextObj.SetActive(true);
                Invoke(nameof(HideLevelText), 2f);
            }
            currentTapLimit = 0.5f; // force 0.5 second
            return;
        }

        // float reductionPerPoint = (startTapLimit - minTapLimit) / 20f;

        // currentTapLimit = startTapLimit - (score * reductionPerPoint);

        // currentTapLimit = Mathf.Clamp(currentTapLimit, minTapLimit, startTapLimit);

        currentTapLimit = startTapLimit;

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
        safeHintText.text = "PAUSED";
        pausePanel.SetActive(true);

        pauseButton.SetActive(false);  // ❌ HIDE pause
        timerBG.SetActive(false);
        rewardSuccessText.SetActive(false);

        // 🔥 IMPORTANT
        SetZoneRaycast(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        pauseButton.SetActive(true);   // ✅ SHOW pause
        timerBG.SetActive(true);

        safeHintText.gameObject.SetActive(true);
        UpdateSafeZoneCountdown();

        // 🔥 IMPORTANT
        SetZoneRaycast(true);
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
        // tapScoreMultiplier = 2;
        Debug.Log("Rewarded upgrade activated: +1 extra score per tap");
    }

    public void DebugUpgrade()
    {
        // TEMP – simulate rewarded ad success
        ActivateScoreUpgrade();
    }

    public void ShowRewardSuccess()
    {
        tapScoreMultiplier += 1;
        rewardSuccessTextText.text = $"Upgrade Activated! +{tapScoreMultiplier}";
        rewardSuccessText.SetActive(true);
        Invoke(nameof(HideRewardSuccess), 2f);
    }

    void HideRewardSuccess()
    {
        rewardSuccessText.SetActive(false);
    }

    void HideLevelText()
    {
        levelTextObj.SetActive(false);
    }

    void UpdateSafeZoneCountdown()
    {
        float remainingTime = Mathf.Max(0f, currentTapLimit - tapTimer);

        // Show 1 decimal for speed feel
        safeHintText.text = $"TOUCH HERE\n{remainingTime:0.0}s";
    }

    // void ResetRect(RectTransform rect)
    // {
    //     rect.anchorMin = Vector2.zero;
    //     rect.anchorMax = Vector2.one;
    //     rect.offsetMin = Vector2.zero;
    //     rect.offsetMax = Vector2.zero;
    // }

    void ResetRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }


    void ApplyCenterSafeLayout()
    {
        ResetRect(safeZone);
        ResetRect(redZone);

        // Safe in center
        safeZone.anchorMin = safeZone.anchorMax = new Vector2(0.5f, 0.5f);
        safeZone.sizeDelta = new Vector2(400, 400);
        safeZone.anchoredPosition = Vector2.zero;

        // Red fills background
        redZone.anchorMin = Vector2.zero;
        redZone.anchorMax = Vector2.one;

        // 🔥 IMPORTANT: bring safe on top
        // safeZone.SetAsLastSibling();
        safeZone.transform.SetSiblingIndex(
            safeZone.transform.parent.childCount - 1
        );

        safeHintText.text = "TOUCH CENTER!";
        HintTextCenter();

    }

    void ApplyCenterRedLayout()
    {
        ResetRect(safeZone);
        ResetRect(redZone);

        // Red in center
        redZone.anchorMin = redZone.anchorMax = new Vector2(0.5f, 0.5f);
        redZone.sizeDelta = new Vector2(400, 400);
        redZone.anchoredPosition = Vector2.zero;

        // Safe fills background
        safeZone.anchorMin = Vector2.zero;
        safeZone.anchorMax = Vector2.one;

        // 🔥 IMPORTANT: bring red on top
        // redZone.SetAsLastSibling();
        redZone.transform.SetSiblingIndex(
            redZone.transform.parent.childCount - 1
        );

        safeHintText.text = "AVOID CENTER!";
        HintTextTop();   // 🔥 THIS is what you were missing
    }

    void SetZoneRaycast(bool enabled)
    {
        safeZoneImage.raycastTarget = enabled;
        redZoneImage.raycastTarget = enabled;
    }

    void HintTextCenter()
    {
        RectTransform rt = safeHintText.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }

    void HintTextTop()
    {
        RectTransform rt = safeHintText.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -250f); // move down from top
    }

    public void ContinueFromRewardAd()
    {
        Debug.Log("Continuing game from rewarded ad");

        // Hide GameOver UI
        gameOverPanel.SetActive(false);

        // Resume state
        gameRunning = true;
        isPaused = false;

        // Reset timer ONLY
        tapTimer = 0f;
        UpdateTimerBar();
        UpdateSafeZoneCountdown();

        // UI restore
        pauseButton.SetActive(true);
        timerBG.SetActive(true);
        safeHintText.gameObject.SetActive(true);

        // Allow clicks again
        SetZoneRaycast(true);

        // IMPORTANT: Do NOT reset score, layout, or difficulty
    }

    public Image levelBG;

    void ShowLevelMessage(string message, LevelMessageType type)
    {
        Debug.Log("ShowLevelMessage CALLED");
        levelText.text = message;

        switch (type)
        {
            case LevelMessageType.Info:
                levelText.color = Color.white;
                levelBG.color = new Color(0, 0, 0, 0.7f);
                break;

            case LevelMessageType.Speed:
                levelText.color = Color.yellow;
                levelBG.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
                break;

            case LevelMessageType.Unlock:
                levelText.color = Color.cyan;
                levelBG.color = new Color(0f, 0.2f, 0.3f, 0.9f);
                break;

            case LevelMessageType.Warning:
                levelText.color = Color.red;
                levelBG.color = new Color(0.3f, 0f, 0f, 0.9f);
                break;
        }

        levelTextObj.SetActive(true);
        CancelInvoke(nameof(HideLevelText));
        Invoke(nameof(HideLevelText), 2f);
    }

}
