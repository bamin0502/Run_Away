using GoogleMobileAds.Api;
using System;
using System.Collections;
using UnityEngine;
using GoogleMobileAds;


public class AdMobManager : MonoBehaviour
{
    private BannerView _bannerView;
    private InterstitialAd interstitial;
    public string adUnitId;
    public string adUnitId2;
    private RewardedAd _rewardedAd;
    
    private GameManager gameManager;
    private UiManager uiManager;
    
    public event Action OnUserEarnedReward; // 보상 이벤트
    public event Action OnAdClosed; // 광고 닫힘 이벤트
    public event Action boolCheck;
    public void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>(); 
        uiManager = GameObject.FindGameObjectWithTag("UiManager").GetComponent<UiManager>();
    }
    
    public void Start()
    {
        MobileAds.Initialize(initStatus => { });
        RequestBanner();
        RequestInterstitial();
        RequestRewarded();
    }
    private void MuteAudio()
    {
        MobileAds.SetApplicationMuted(true);
    }

    private void UnmuteAudio()
    {
        MobileAds.SetApplicationMuted(false);
    }
    private void RequestRewarded()
    {
#if UNITY_EDITOR
        string adUnitId2 = "ca-app-pub-3940256099942544/5224354917";
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        string adUnitId2 = "ca-app-pub-2503303900066645/1077238676";
#endif
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("보상형 광고를 로드 중입니다.");

        MuteAudio();
        
        var adRequest = new AdRequest();
        RewardedAd.Load(adUnitId2, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("보상형 광고 로드 실패: " + error); 

                    uiManager.AdsReviveCheckButton.interactable = false;
                    UnmuteAudio();
                    return;
                }
                
                Debug.Log("보상형 광고 로드 완료: " + ad.GetResponseInfo());

                uiManager.AdsReviveCheckButton.interactable = true;
                _rewardedAd = ad;
                
                _rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    OnAdClosed?.Invoke();
                    Invoke("boolCheck", 3f);
                    UnmuteAudio();
                };
                
                
            });
    }

    private void RequestInterstitial()
    {
#if UNITY_EDITOR
        string adUnitId = "ca-app-pub-3940256099942544/1033173712";    
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        string adUnitId = "ca-app-pub-2503303900066645/2326709345";
#endif
        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
            uiManager.AdsPanel.SetActive(false);
        }
#if UNITY_EDITOR
        Debug.Log("전면 광고를 로드 중입니다.");
#endif
        MuteAudio();
        
        var adRequest = new AdRequest();
        InterstitialAd.Load(adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
#if UNITY_EDITOR
                    Debug.LogError("전면 광고 로드 실패: " + error); 
#endif
                    
                    return;
                }
#if UNITY_EDITOR
                Debug.Log("전면 광고 로드 완료: " + ad.GetResponseInfo()); 
#endif
                
                interstitial = ad;
                interstitial.OnAdFullScreenContentClosed += (() =>
                {
                    OnAdClosed?.Invoke();
                    Invoke("boolCheck", 3f);
                    UnmuteAudio();
                });
            });
    }
    
    private void RequestBanner()
    {
#if UNITY_EDITOR
        string adUnitId = "ca-app-pub-3940256099942544/9214589741";   
#endif

        if (_bannerView != null)
        {
            DestroyAd();
        }

        _bannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Top);
    }

    public void DestroyAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
            gameManager.isAd=false;
        }
    }

    public void LoadAd()
    {
        if (_bannerView == null)
        {
            _bannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Top);
        }

        var adRequest = new AdRequest();
        _bannerView.LoadAd(adRequest);
    }

    public void LoadInterstitialAd()
    {
        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }

        var adRequest = new AdRequest();
        InterstitialAd.Load(adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    return;
                }

                interstitial = ad;
               
                
            });
    }
    
    public void ShowInterstitialAd()
    {
        if (interstitial != null && interstitial.CanShowAd())
        {
            gameManager.isAd=true;
            uiManager.AdsPanel.SetActive(true);
            interstitial.Show();
        }
        else
        {
            OnAdClosed?.Invoke();
        }
    }

    public void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();
        RewardedAd.Load(adUnitId2, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    return;
                }

                _rewardedAd = ad;
            });
    }

    public void ShowRewardedAd()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            gameManager.isAd=true;
            uiManager.AdsPanel.SetActive(true);
            _rewardedAd.Show(reward =>
            {
                OnAdClosed?.Invoke();
                Invoke("boolCheck", 3f);
                OnUserEarnedReward?.Invoke();
                UnmuteAudio();
            });
        }
    }
    

    
}
