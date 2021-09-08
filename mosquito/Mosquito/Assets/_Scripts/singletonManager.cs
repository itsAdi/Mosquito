using UnityEngine;
using google.service.game;
using admob;
using MiniJSON;
using System.Collections.Generic;
using System.IO;

public class singletonManager : MonoBehaviour
{
    public struct topperData
    {
        public string displayName;
        public string score;
        public string displayPicture;
    }
    public delegate void fetchTopperDataDel(topperData data);
    protected fetchTopperDataDel fetchTopperDataRef;
    public delegate void detectLoginScsDel(bool result);
    protected detectLoginScsDel detectLoginScsRef;

    public delegate void detectRewardDel(bool result);
    protected detectRewardDel detectRewardRef;

    public static singletonManager Instance;
    [HideInInspector]
    public float soundVolume;
    [HideInInspector]
    public int bestScore;
    [HideInInspector]
    public int currentScore;
    [HideInInspector]
    public float smallMosqRate;
    [HideInInspector]
    public float smallMosqSpeed;
    [HideInInspector]
    public float streakMosqSpeed;
    [HideInInspector]
    public int scoreMultiplier;
    [HideInInspector]
    public int scoreThreshold;
    [HideInInspector]
    public bool gameOver;
    [HideInInspector]
    public bool gameResumed;

    [HideInInspector]
    public handleGameplay hGP = null;
    [HideInInspector]
    public handleLizard lizardInstance = null;

    private int gameCount = 3, LizardHintCounts = 0;
    private bool adCompleted;

    void Awake()
    {
        soundVolume = 1f;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GoogleGame.Instance().gameEventHandler += OnGameEvent;
        GoogleGame.Instance().login(true, false);
        Admob.Instance().initAdmob("", "ca-app-pub-8347424776413444/4177619326");
        Admob.Instance().loadInterstitial();
        Admob.Instance().loadRewardedVideo("ca-app-pub-8347424776413444/3034031809");
        Admob.Instance().interstitialEventHandler += OnInterstitialEvent;
        Admob.Instance().rewardedVideoEventHandler += OnRewardEvent;
        try
        {
            if (!File.Exists(Application.persistentDataPath + "/KS_gameData.json"))
            {
                File.Create(Application.persistentDataPath + "/KS_gameData.json").Dispose();
                File.WriteAllText(Application.persistentDataPath + "/KS_gameData.json", "{\"best\":0, \"LizardHintCounts\":0}");
                bestScore = 0;
            }
            else
            {
                string fetchedData = File.ReadAllText(Application.persistentDataPath + "/KS_gameData.json");
                var best = Json.Deserialize(fetchedData) as Dictionary<string, object>;
                int.TryParse(best["best"].ToString(), out bestScore);
                int.TryParse(best["LizardHintCounts"].ToString(), out LizardHintCounts);
            }
        }
        catch
        {
            Debug.LogError("Assessing JSON failed");
            throw;
        }
    }

    public void showLeaderboard()
    {
        if (GoogleGame.Instance().isConnected())
        {
            GoogleGame.Instance().showLeaderboard("CgkIpamRuLIKEAIQAA");
        }
    }

    public void updateGameCount()
    {
        gameCount++;
        if (gameCount >= 3)
        {
            if (Admob.Instance().isInterstitialReady())
            {
                Admob.Instance().showInterstitial();
            }
        }
    }

    public void detectLogin(detectLoginScsDel callBack)
    {
        detectLoginScsRef = callBack;
        if (GoogleGame.Instance().isConnected())
        {
            detectLoginScsRef(true);
        }
        else
        {
            GoogleGame.Instance().login(true, false);
        }
    }

    public void fetchTopperData(fetchTopperDataDel callBack)
    {
        if (GoogleGame.Instance().isConnected())
        {
            fetchTopperDataRef = callBack;
            GoogleGame.Instance().loadTopLeaderboardScores("CgkIpamRuLIKEAIQAA", GameConst.TIME_SPAN_ALL_TIME, GameConst.COLLECTION_PUBLIC, 1, true);
        }
    }

    public void showReward(detectRewardDel callBack)
    {
        detectRewardRef = callBack;
        if (Admob.Instance().isRewardedVideoReady())
        {
            Admob.Instance().showRewardedVideo();
        }
        else
        {
            detectRewardRef(false);
        }
    }

    public void writeBest()
    {
        if (currentScore > bestScore)
        {
            try
            {
                File.WriteAllText(Application.persistentDataPath + "/KS_gameData.json", "{\"best\":" + currentScore.ToString() + ", \"LizardHintCounts\": " + LizardHintCounts.ToString() + "}");
            }
            catch (System.Exception)
            {
                Debug.LogError("Data file could be updated on disk");
                throw;
            }
            bestScore = currentScore;
            if (GoogleGame.Instance().isConnected())
            {
                GoogleGame.Instance().submitLeaderboardScore("CgkIpamRuLIKEAIQAA", (long)currentScore);
            }
        }
    }

    public bool isRewardReady()
    {
        if (Admob.Instance().isRewardedVideoReady())
        {
            return true;
        }
        return false;
    }

    public bool ValidateLizardHint()
    {
        if(LizardHintCounts < 2)
        {
            LizardHintCounts++;
            try
            {
                File.WriteAllText(Application.persistentDataPath + "/KS_gameData.json", "{\"best\":" + bestScore.ToString() + ", \"LizardHintCounts\": " + LizardHintCounts.ToString() + "}");
            }
            catch (System.Exception)
            {
                Debug.LogError("Data file could be updated on disk");
                throw;
            }
            return true;
        }
        return false;
    }

    // ***************************** Google Game methods ***************************

    void OnGameEvent(int resultCode, string eventName, string msg)
    {
        if (resultCode == GameConst.RESULT_OK)
        {
            if (eventName.Equals(GameEvent.onConnectSuccess))
            {
                detectLoginScsRef(true);
            }
            if (eventName.Equals(GameEvent.onLoadLeaderboardTopScores))
            {
                topperData tData;
                var dict = Json.Deserialize(msg) as Dictionary<string, object>;
                var scoreDict = ((List<object>)dict["scores"])[0] as Dictionary<string, object>;
                var scoreHolderDict = scoreDict["scoreHolder"] as Dictionary<string, object>;
                tData.displayName = scoreDict["scoreHolderDisplayName"].ToString();
                tData.score = scoreDict["displayScore"].ToString();
                tData.displayPicture = scoreHolderDict["iconImageUrl"].ToString();
                fetchTopperDataRef(tData);
            }
        }
        else
        {
            detectLoginScsRef(false);
        }
    }

    // ***************************** Admob methods *********************************
    void OnInterstitialEvent(string evnt, string msg)
    {
        if (string.Equals(evnt, AdmobEvent.onAdFailedToLoad))
        {
            Admob.Instance().loadInterstitial();
        }
        else if (string.Equals(evnt, AdmobEvent.onAdClosed))
        {
            Admob.Instance().loadInterstitial();
            gameCount = 0;
        }
    }

    void OnRewardEvent(string evnt, string msg)
    {
        if (string.Equals(evnt, AdmobEvent.onAdFailedToLoad))
        {
            Admob.Instance().loadRewardedVideo("ca-app-pub-8347424776413444/3034031809");
            detectRewardRef(false);
        }
        else if (string.Equals(evnt, AdmobEvent.onRewarded))
        {
            adCompleted = true;
            gameResumed = true;
        }

        if (string.Equals(evnt, AdmobEvent.onAdClosed))
        {
            if (adCompleted)
            {
                adCompleted = false;
                detectRewardRef(true);
            }
            else
            {
                detectRewardRef(false);
            }
            Admob.Instance().loadRewardedVideo("ca-app-pub-8347424776413444/3034031809");
        }
    }
}
