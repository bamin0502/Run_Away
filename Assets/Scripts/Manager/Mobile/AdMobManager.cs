using GoogleMobileAds.Api;
using System;
using UnityEngine;
using GoogleMobileAds;

public class AdMobManager : MonoBehaviour
{
    private BannerView _bannerView;
    private InterstitialAd interstitial;
    public string adUnitId;
    private RewardedAd _rewardedAd;

    public event Action OnUserEarnedReward; // 보상 이벤트
    
    public void Start()
    {
        MobileAds.Initialize(initStatus => { });
        RequestBanner();
        RequestInterstitial();
        RequestRewarded();
    }

    private void RequestRewarded()
    {
        string adUnitId = "ca-app-pub-3940256099942544/5224354917";

        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("보상형 광고를 로드 중입니다.");
        var adRequest = new AdRequest();
        RewardedAd.Load(adUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("보상형 광고 로드 실패: " + error);
                    return;
                }

                Debug.Log("보상형 광고 로드 완료: " + ad.GetResponseInfo());
                _rewardedAd = ad;
            });
    }

    private void RequestInterstitial()
    {
        string adUnitId = "ca-app-pub-3940256099942544/1033173712";

        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }

        Debug.Log("전면 광고를 로드 중입니다.");
        var adRequest = new AdRequest();
        InterstitialAd.Load(adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("전면 광고 로드 실패: " + error);
                    return;
                }

                Debug.Log("전면 광고 로드 완료: " + ad.GetResponseInfo());
                interstitial = ad;

            });
    }

    private void RequestBanner()
    {
        string adUnitId = "ca-app-pub-3940256099942544/9214589741";

        if (_bannerView != null)
        {
            DestroyAd();
        }

        _bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
    }

    public void DestroyAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    public void LoadAd()
    {
        if (_bannerView == null)
        {
            _bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
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
            interstitial.Show();
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
        RewardedAd.Load(adUnitId, adRequest,
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
            _rewardedAd.Show(reward =>
            {
                OnUserEarnedReward?.Invoke();
            });
        }
    }
    

    
}
