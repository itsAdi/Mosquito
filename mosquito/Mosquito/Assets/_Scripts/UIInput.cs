using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIInput : MonoBehaviour
{
    public handleGameplay hG;
    public Text scoreText, topperText, gameOverScore, gameOverBest, resumeCounter;
    public GameObject mainPanelObj, topperPanelObject, gameOverPanelObj;
    public AudioSource bgMusic, shockAudio;
    public Sprite[] musicSprite;
    public Image musicRenderer, shockBarMask, topperImage;
    public Image[] lifeRenderer;
    public Sprite enabledLife;
    public Button rewardVideoBtn;
    public Animator LizardHintAnim;
    [Header("Background window gameobject")]
    public Material windowObj;
    public SpriteRenderer windowRenderer;
    public Sprite[] windowSprites;
    [Header("Power up resources")]
    public PowerUp[] PowerUps; // 0 for lizard and 1 for shock
    [HideInInspector]
    public bool gameStarted;

    private int lifeIndex;
    private float currTime, totalTime;
    private Sprite topperSprite;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (!singletonManager.Instance.gameOver)
        {
            scoreText.text = singletonManager.Instance.currentScore.ToString();
            if (currTime < totalTime && !handleLizard.IsLizardActivated)
            {
                currTime += Time.deltaTime;
                if (currTime >= totalTime)
                {
                    shockBarMask.fillAmount = Mathf.Clamp((shockBarMask.fillAmount + 0.02f), 0f, 1f);
                    if (shockBarMask.fillAmount == 1f)
                    {
                        PowerUps[1].MakeAbilityAccessible();
                    }
                    else if (shockBarMask.fillAmount >= 0.5f)
                    {
                        if (!PowerUps[0].PowerUpButton.IsInteractable())
                        {
                            PowerUps[0].MakeAbilityAccessible();
                        }
                        currTime = 0f;
                    }
                    else
                    {
                        currTime = 0f;
                    }
                }
            }
        }
    }

    void Start()
    {
        int bgIndex = Random.Range(1, 10);
        windowRenderer.sprite = bgIndex % 2 == 0 ? windowSprites[0] : windowSprites[1];
        PowerUps[0].RestrictAbility();
        PowerUps[1].RestrictAbility();
        topperPanelObject.SetActive(false);
        gameOverBest.text = singletonManager.Instance.bestScore.ToString();
        gameOverPanelObj.SetActive(false);
        resumeCounter.gameObject.SetActive(false);
        toggleMusic();
        totalTime = 0.7f;
        currTime = totalTime;
        if (!singletonManager.Instance.gameOver)
        {
            singletonManager.Instance.detectLogin(detectLogin);
        }
        if (singletonManager.Instance.gameResumed)
        {
            StartCoroutine(handleResumeCounter());
        }
        else
        {
            singletonManager.Instance.smallMosqRate = 3f;
            singletonManager.Instance.smallMosqSpeed = 5f;
            singletonManager.Instance.scoreThreshold = 10;
            singletonManager.Instance.streakMosqSpeed = 5f;
            singletonManager.Instance.scoreMultiplier = 1;
            singletonManager.Instance.currentScore = 0;
        }
        if (singletonManager.Instance.gameOver)
        {
            singletonManager.Instance.gameOver = false;
            mainPanelObj.SetActive(false);
            if (!singletonManager.Instance.gameResumed)
            {
                Invoke("playGame", 0.3f);
            }
        }
    }

    public void playGame()
    {
        if (!singletonManager.Instance.gameOver)
        {
            gameStarted = true;
            currTime = 0f;
            if (mainPanelObj.activeSelf)
            {
                mainPanelObj.SetActive(false);
            }
            if (topperPanelObject.activeSelf)
            {
                topperPanelObject.SetActive(false);
            }
            hG.detectGameStart();
        }
    }

    public void restartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void handleMusic()
    {
        singletonManager.Instance.soundVolume = singletonManager.Instance.soundVolume == 1f ? 0f : 1f;
        toggleMusic();
    }

    public void activateShock()
    {
        if (!singletonManager.Instance.gameOver)
        {
            PowerUps[1].RestrictAbility(); // Shock button used so disable shock button graphics
            PowerUps[0].RestrictAbility(); // also disable lizard button graphics
            currTime = 0f;
            shockBarMask.fillAmount = 0f;
            shockAudio.Play();
            StartCoroutine(handleWindowShock());
        }
    }

    public void activateLizard()
    {
        PowerUps[0].RestrictAbility();
        PowerUps[1].RestrictAbility();
        currTime = 0f;
        shockBarMask.fillAmount = 0f;
        if (singletonManager.Instance.ValidateLizardHint())
        {
            LizardHintAnim.SetTrigger("comeIn");
        }
        singletonManager.Instance.hGP.enableLizardSpawn();
    }

    public void showLeaderboard()
    {
        singletonManager.Instance.showLeaderboard();
    }

    public void openLink(string linkUrl)
    {
        Application.OpenURL(linkUrl);
    }

    public void processLife()
    {
        lifeRenderer[lifeIndex].sprite = enabledLife;
        lifeIndex = Mathf.Clamp(++lifeIndex, 0, lifeRenderer.Length - 1);
        if (singletonManager.Instance.gameOver && !gameOverPanelObj.activeInHierarchy)
        {
            gameOverScore.text = scoreText.text;
            gameOverPanelObj.SetActive(true);
            rewardVideoBtn.gameObject.SetActive(singletonManager.Instance.isRewardReady());
            rewardVideoBtn.onClick.RemoveAllListeners();
            rewardVideoBtn.onClick.AddListener(showRewardVideo);
            singletonManager.Instance.writeBest();
        }
    }

    public void handleShockBarMultiply()
    {
        shockBarMask.fillAmount = Mathf.Clamp(shockBarMask.fillAmount + 0.05f, 0f, 1f);
        if (shockBarMask.fillAmount == 1f)
        {
            currTime = totalTime;
            PowerUps[1].PowerUpButton.interactable = true;
            PowerUps[1].Indicator.SetAlpha(1f);
            PowerUps[1].PowerUpImage.SetAlpha(1f);
        }
    }

    void showRewardVideo()
    {
        singletonManager.Instance.showReward(rewardCallback);
    }

    void rewardCallback(bool scs)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => restartGame());
    }

    void toggleMusic()
    {
        if (singletonManager.Instance.soundVolume == 1f)
        {
            musicRenderer.sprite = musicSprite[0];
            bgMusic.volume = 0.2f;
        }
        else
        {
            musicRenderer.sprite = musicSprite[1];
            bgMusic.volume = 0f;
        }
    }

    void TogglePowerUp(int index)
    {
        PowerUps[index].Indicator.SetAlpha(PowerUps[index].PowerUpButton.IsInteractable() ? 0f : 1f);
        PowerUps[index].PowerUpImage.SetAlpha(PowerUps[index].PowerUpButton.IsInteractable() ? 0f : 1f);
        PowerUps[index].PowerUpButton.interactable = !PowerUps[index].PowerUpButton.IsInteractable();
    }

    void detectLogin(bool result)
    {
        if (result)
        {
            singletonManager.Instance.fetchTopperData(showLeaderboardTopper);
        }
        else
        {
            Debug.Log("Login Failed");
        }
    }

    void showLeaderboardTopper(singletonManager.topperData data)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(enumerateTopperData(data));
    }

    IEnumerator enumerateTopperData(singletonManager.topperData tData)
    {
        topperText.text = tData.displayName + "\n" + tData.score;
        Texture2D imageTex = new Texture2D(96, 96, TextureFormat.RGB24, false);
        WWW imageUrl = new WWW(tData.displayPicture);
        yield return imageUrl;
        if (!string.IsNullOrEmpty(imageUrl.error))
        {
            topperSprite = null;
        }
        else
        {
            if (imageUrl.isDone)
            {
                imageUrl.LoadImageIntoTexture(imageTex);
                topperSprite = Sprite.Create(imageTex, new Rect(0f, 0f, 96f, 96f), new Vector2(0.5f, 0.5f));
                topperImage.sprite = topperSprite;
                if (!gameStarted)
                {
                    topperPanelObject.SetActive(true);
                }
            }
            else
            {
                topperSprite = null;
            }
        }
    }

    IEnumerator handleWindowShock()
    {
        int stepCount = 0;
        while (stepCount < 4)
        {
            windowObj.SetFloat("_divisor", 3f);
            yield return new WaitForSeconds(0.05f);
            windowObj.SetFloat("_divisor", 1f);
            yield return new WaitForSeconds(0.05f);
            stepCount++;
        }
    }

    IEnumerator handleResumeCounter()
    {
        int tempCount = 3;
        resumeCounter.gameObject.SetActive(true);
        while (tempCount > 0)
        {
            yield return new WaitForSeconds(1f);
            tempCount--;
            resumeCounter.text = tempCount.ToString();
        }
        resumeCounter.gameObject.SetActive(false);
        playGame();
    }
}
