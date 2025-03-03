using UnityEngine;
using GoogleMobileAds.Api;

public class AdsControler : MonoBehaviour
{
    public static AdsControler instance;
    private InterstitialAd _interstitialAd;
    private string _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        MobileAds.Initialize(initStatus => { });
    }
    private void Start()
    {
        LoadInterstitialAd();
    }
    private void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }
        var request = new AdRequest();

        InterstitialAd.Load(_interstitialAdUnitId, request, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.Log("Interstitial ad failed to load with error: " + error);
                return;
            }
            _interstitialAd = ad;
        });
    }
    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial ad is not ready yet.");
        }
    }
}
