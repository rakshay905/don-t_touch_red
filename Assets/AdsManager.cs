using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    // ==================================================
    // 🔴 TEST AD IDS (REPLACE WITH REAL ON RELEASE)
    // ==================================================
    private const string BANNER_ID = "ca-app-pub-4847526487101723/2714739692";
    private const string INTERSTITIAL_ID = "ca-app-pub-4847526487101723/8468084662";
    private const string REWARDED_ID = "ca-app-pub-4847526487101723/3694447745";

    BannerView bannerView;
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;

    int gameOverCounter = 0;

    // ==================================================
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==================================================
    void Start()
    {
        MobileAds.Initialize(status => { });

        RequestBanner();
        LoadInterstitial();
        LoadRewarded();
    }

    // ==================================================
    // 📢 BANNER
    // ==================================================
    void RequestBanner()
    {
        bannerView = new BannerView(
            BANNER_ID,
            AdSize.Banner,
            AdPosition.Bottom
        );

        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
    }

    public void ShowBanner()
    {
        if (bannerView != null)
            bannerView.Show();
    }

    public void HideBanner()
    {
        if (bannerView != null)
            bannerView.Hide();
    }

    // ==================================================
    // 📺 INTERSTITIAL
    // ==================================================
    void LoadInterstitial()
    {
        InterstitialAd.Load(
            INTERSTITIAL_ID,
            new AdRequest(),
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.Log("Interstitial load failed");
                    return;
                }

                interstitialAd = ad;
            }
        );
    }

    public void ShowInterstitial()
    {
        gameOverCounter++;

        // Show every 3rd game over
        if (gameOverCounter % 3 != 0) return;

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
            LoadInterstitial();
        }
    }

    // ==================================================
    // 🎁 REWARDED (UPGRADE)
    // ==================================================
    void LoadRewarded()
    {
        RewardedAd.Load(
            REWARDED_ID,
            new AdRequest(),
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.Log("Rewarded load failed");
                    return;
                }

                rewardedAd = ad;
            }
        );
    }

    // public void ShowRewarded()
    // {
    //     if (rewardedAd != null && rewardedAd.CanShowAd())
    //     {
    //         rewardedAd.Show(reward =>
    //         {
    //             Debug.Log("Reward earned");
    //             GameManager.Instance.ActivateScoreUpgrade();
    //         });

    //         LoadRewarded();
    //     }
    //     else
    //     {
    //         Debug.Log("Rewarded not ready");
    //     }
    // }

    // public void ShowRewarded()
    // {
    //     if (rewardedAd != null && rewardedAd.CanShowAd())
    //     {
    //         rewardedAd.Show(reward =>
    //         {
    //             // ✅ THIS IS CALLED ONLY IF REWARD IS GRANTED
    //             Debug.Log("Reward granted by AdMob");

    //             GameManager.Instance.ActivateScoreUpgrade();
    //         });

    //         // Preload next rewarded ad
    //         LoadRewarded();
    //     }
    //     else
    //     {
    //         Debug.Log("Rewarded ad not ready");
    //     }
    // }

    bool rewardGranted = false;

    public void ShowRewarded()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardGranted = false;

            // Reward callback
            rewardedAd.Show(reward =>
            {
                rewardGranted = true;
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    GameManager.Instance.ActivateScoreUpgrade();
                });
            });

            // Ad closed callback
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                // Return to pause panel UI
                // GameManager.Instance.PauseGame();

                // Show success ONLY if reward was earned
                if (rewardGranted)
                {
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        GameManager.Instance.ShowRewardSuccess();
                    });
                }

                // Preload next ad
                LoadRewarded();
            };
        }
        else
        {
            Debug.Log("Rewarded ad not ready");
        }
    }


}