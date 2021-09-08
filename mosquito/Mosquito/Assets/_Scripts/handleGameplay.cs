using UnityEngine;
using System.Collections;

public class handleGameplay : MonoBehaviour
{
    public UIInput uiI;
    public GameObject leftCurtain, rightCurtain;
    public CanvasRenderer waspHitPanel;
    public AudioSource mosquitoSound;
    public AudioSource[] mosqTapSounds;
    public streakClass streakInstance;
    public GameObject wasp, waspDead, waspShocked, streakMosq, highScoreAlarmPanelObj;
    public GameObject[] smallMosq, bigMosq, smallDead, bigDead, smallShocked, bigShocked;

    private float currtime, totaltime = 1f, BTCurrtime, BTTotalTime = 1f, WHCurrTime, WHTotalTime = 1f;
    private bool takeInput, enableWasp, shockActivated, touchRecieved, highScoreDetected, canSpawnLizard;
    private Vector2[] posBounds;
    private mosquito[] smallMosqClass, bigMosqClass;
    private deadMosquito[] smallDeadClass, bigDeadClass;
    private shockedMosquito[] smallShockedClass, bigShockedClass;
    private deadMosquito waspDeadClass;
    private shockedMosquito waspShockedClass;
    private mosquito waspClass, streakMosqClass;
    private Vector2 touchPos;
    private int aliveMosqCount, streakIconIndex = -1;
    private handleHighScoreAlarm hHsa;

    private struct posDir
    {
        public Vector2 posVec;
        public Vector2 dirVec;
    }

    void Start()
    {
        singletonManager.Instance.hGP = null;
        singletonManager.Instance.hGP = this;
        hHsa = highScoreAlarmPanelObj.GetComponent<handleHighScoreAlarm>();
        highScoreAlarmPanelObj.SetActive(false);
        BTCurrtime = BTTotalTime;
        WHCurrTime = WHTotalTime;
        calculatePosBound();
        currtime = totaltime;
        if (singletonManager.Instance.gameResumed)
        {
            leftCurtain.transform.position = new Vector3(-15f, 0f, 0f);
            rightCurtain.transform.position = new Vector3(15f, 0f, 0f);
        }
        smallMosqClass = new mosquito[smallMosq.Length];
        bigMosqClass = new mosquito[bigMosq.Length];
        smallDeadClass = new deadMosquito[smallMosq.Length];
        bigDeadClass = new deadMosquito[bigMosq.Length];
        smallShockedClass = new shockedMosquito[smallShocked.Length];
        bigShockedClass = new shockedMosquito[bigShocked.Length];

        waspDeadClass = waspDead.GetComponent<deadMosquito>();
        waspClass = wasp.GetComponent<mosquito>();
        waspShockedClass = waspShocked.GetComponent<shockedMosquito>();
        wasp.SetActive(false);
        waspDead.SetActive(false);
        waspShocked.SetActive(false);

        streakMosqClass = streakMosq.GetComponent<mosquito>();
        streakMosq.SetActive(false);
        touchPos = new Vector2();
        for (int i = 0; i < smallMosq.Length; i++)
        {
            smallMosqClass[i] = smallMosq[i].GetComponent<mosquito>();
            smallDeadClass[i] = smallDead[i].GetComponent<deadMosquito>();
            smallShockedClass[i] = smallShocked[i].GetComponent<shockedMosquito>();
            smallMosq[i].SetActive(false);
            smallDead[i].SetActive(false);
            smallShocked[i].SetActive(false);
        }
        for (int j = 0; j < bigMosq.Length; j++)
        {
            bigMosqClass[j] = bigMosq[j].GetComponent<mosquito>();
            bigDeadClass[j] = bigDead[j].GetComponent<deadMosquito>();
            bigShockedClass[j] = bigShocked[j].GetComponent<shockedMosquito>();
            bigDead[j].SetActive(false);
            bigMosq[j].SetActive(false);
            bigShocked[j].SetActive(false);
        }
    }

    void Update()
    {
        if (takeInput)
        {
            if (canSpawnLizard)
            {
                if (touchRecieved)
                {
                    canSpawnLizard = false;
                    StartCoroutine(singletonManager.Instance.lizardInstance.enableLizard(touchPos));
                }
            }
            else
            {
                if (touchRecieved || shockActivated)
                {
                    int shockedIndex = 0;
                    bool playSquash = false;
                    for (int j = 0; j < smallMosq.Length; j++)
                    {
                        if (smallMosq[j].activeSelf)
                        {
                            if (touchRecieved)
                            {
                                if (Vector2.Distance(smallMosq[j].transform.position, touchPos) < 1f)
                                {
                                    int deadIndex = 0;
                                    while (deadIndex < smallDead.Length)
                                    {
                                        if (!smallDead[deadIndex].activeSelf)
                                        {
                                            smallDead[deadIndex].SetActive(true);
                                            smallDeadClass[deadIndex].makeMosquitoDead(smallMosq[j].transform.position, smallMosqClass[j].direction.x > 0 ? true : false);
                                            deadIndex = smallMosq.Length;
                                        }
                                        deadIndex++;
                                    }
                                    smallMosqClass[j].resetMosq();
                                    singletonManager.Instance.currentScore += 1 * singletonManager.Instance.scoreMultiplier;
                                    playSquash = true;
                                }
                            }
                            else if (shockActivated)
                            {
                                smallShocked[shockedIndex].SetActive(true);
                                smallShockedClass[shockedIndex].showShocked(smallMosq[j].transform.position, smallMosqClass[j].direction.x > 0 ? true : false);
                                shockedIndex++;
                                smallMosqClass[j].resetMosq();
                                singletonManager.Instance.currentScore += 1 * singletonManager.Instance.scoreMultiplier;
                            }
                        }
                    }
                    shockedIndex = 0;
                    for (int j = 0; j < bigMosq.Length; j++)
                    {
                        if (bigMosq[j].activeSelf)
                        {
                            if (touchRecieved)
                            {
                                if (Vector2.Distance(bigMosq[j].transform.position, touchPos) < 1f)
                                {
                                    int deadIndex = 0;
                                    while (deadIndex < bigDead.Length)
                                    {
                                        if (!bigDead[deadIndex].activeSelf)
                                        {
                                            bigDead[deadIndex].SetActive(true);
                                            bigDeadClass[deadIndex].makeMosquitoDead(bigMosq[j].transform.position, bigMosqClass[j].direction.x > 0 ? true : false);
                                            deadIndex = bigMosq.Length;
                                        }
                                        deadIndex++;
                                    }
                                    uiI.handleShockBarMultiply();
                                    bigMosqClass[j].resetMosq();
                                    singletonManager.Instance.currentScore += 5 * singletonManager.Instance.scoreMultiplier;
                                    playSquash = true;
                                }
                            }
                            else if (shockActivated)
                            {
                                bigShocked[shockedIndex].SetActive(true);
                                bigShockedClass[shockedIndex].showShocked(bigMosq[j].transform.position, bigMosqClass[j].direction.x > 0f ? true : false);
                                shockedIndex++;
                                bigMosqClass[j].resetMosq();
                                singletonManager.Instance.currentScore += 5 * singletonManager.Instance.scoreMultiplier;
                            }
                        }
                    }
                    shockedIndex = 0;
                    if (wasp.activeSelf)
                    {
                        if (touchRecieved)
                        {
                            if (Vector2.Distance(wasp.transform.position, touchPos) < 1f)
                            {
                                waspDead.SetActive(true);
                                waspDeadClass.makeMosquitoDead(wasp.transform.position, waspClass.direction.x > 0f ? true : false);
                                waspClass.resetMosq();
                                playSquash = true;
                                aliveMosqCount++;
                                if (aliveMosqCount == 5)
                                {
                                    singletonManager.Instance.gameOver = true;
                                    mosquitoSound.Stop();
                                    takeInput = false;
                                    singletonManager.Instance.updateGameCount();
                                }
                                uiI.processLife();
                                WHCurrTime = 0f;
                            }
                        }
                        else if (shockActivated)
                        {
                            waspShocked.SetActive(true);
                            waspShockedClass.showShocked(wasp.transform.position, waspClass.direction.x > 0f ? true : false);
                            waspClass.resetMosq();
                        }
                    }

                    if (streakMosq.activeSelf)
                    {
                        if (Vector2.Distance(streakMosq.transform.position, touchPos) < 1f)
                        {
                            streakIconIndex++;
                            streakIconIndex = Mathf.Clamp(streakIconIndex, -1, 3);
                            streakInstance.showStreak(streakMosq.transform.position, streakIconIndex);
                            streakMosqClass.resetMosq();
                            singletonManager.Instance.scoreMultiplier = Mathf.Clamp(++singletonManager.Instance.scoreMultiplier, 1, 5);
                            singletonManager.Instance.streakMosqSpeed += 2f;
                            singletonManager.Instance.streakMosqSpeed = Mathf.Clamp(singletonManager.Instance.streakMosqSpeed, 5f, 12f);
                            playSquashSound(streakIconIndex + 1);
                            BTCurrtime = 0f;
                            Invoke("spawnStreak", 1f);
                        }
                    }
                    if (singletonManager.Instance.currentScore > 50 && !enableWasp)
                    {
                        enableWasp = true;
                        StartCoroutine(spawnEnemy());
                    }
                    if (singletonManager.Instance.currentScore >= singletonManager.Instance.scoreThreshold && singletonManager.Instance.scoreThreshold > 0)
                    {
                        singletonManager.Instance.scoreThreshold += singletonManager.Instance.scoreThreshold;
                        singletonManager.Instance.smallMosqRate -= 0.5f;
                        if (singletonManager.Instance.smallMosqRate <= 0.7f)
                        {
                            singletonManager.Instance.smallMosqRate = 0.7f;
                            singletonManager.Instance.scoreThreshold = 0;
                        }
                        singletonManager.Instance.smallMosqSpeed++;
                        singletonManager.Instance.smallMosqSpeed = Mathf.Clamp(singletonManager.Instance.smallMosqSpeed, 5f, 8f);
                    }
                    if (playSquash)
                    {
                        playSquashSound(0);
                        BTCurrtime = 0f;
                    }
                    touchRecieved = false;
                    shockActivated = false;
                }
            }
        }
        if (currtime < totaltime && !singletonManager.Instance.gameResumed)
        {
            currtime += Time.deltaTime;
            if (currtime >= totaltime)
            {
                currtime = totaltime;
                resumeGame();
            }
            float d = currtime / totaltime;
            d *= d;
            leftCurtain.transform.position = Vector3.Lerp(new Vector3(-5.5f, 0f, 0f), new Vector3(-15f, 0f, 0f), d);
            rightCurtain.transform.position = Vector3.Lerp(new Vector3(5.5f, 0f, 0f), new Vector3(15f, 0f, 0f), d);
        }
        if (BTCurrtime < BTTotalTime && singletonManager.Instance.soundVolume == 1f)
        {
            BTCurrtime += Time.deltaTime;
            if (BTCurrtime >= BTTotalTime)
            {
                BTCurrtime = BTTotalTime;
            }
            float t = BTCurrtime / BTTotalTime;
            t *= t;
            mosquitoSound.volume = Mathf.Lerp(0f, 0.3f, t);
        }
        if (WHCurrTime < WHTotalTime)
        {
            WHCurrTime += Time.deltaTime;
            if (WHCurrTime >= WHTotalTime)
            {
                WHCurrTime = WHTotalTime;
            }
            float c = WHCurrTime / WHTotalTime;
            c *= c;
            waspHitPanel.SetAlpha(Mathf.Lerp(255f, 1f, c));
        }
        for (int i = 0; i < smallMosq.Length; i++)
        {
            if (smallMosq[i].activeSelf)
            {
                smallMosq[i].transform.position += (Vector3)smallMosqClass[i].direction * singletonManager.Instance.smallMosqSpeed * Time.deltaTime;
                if (smallMosq[i].transform.position.x < posBounds[0].x || smallMosq[i].transform.position.x > posBounds[1].x || smallMosq[i].transform.position.y < posBounds[0].y || smallMosq[i].transform.position.y > posBounds[1].y)
                {
                    smallMosqClass[i].resetMosq();
                    aliveMosqCount++;
                    if (aliveMosqCount == 5)
                    {
                        singletonManager.Instance.gameOver = true;
                        takeInput = false;
                        mosquitoSound.Stop();
                        singletonManager.Instance.updateGameCount();
                    }
                    uiI.processLife();
                }
            }
        }
        for (int i = 0; i < bigMosq.Length; i++)
        {
            if (bigMosq[i].activeSelf)
            {
                bigMosq[i].transform.position += (Vector3)bigMosqClass[i].direction * 5f * Time.deltaTime;
                if (bigMosq[i].transform.position.x < posBounds[0].x || bigMosq[i].transform.position.x > posBounds[1].x || bigMosq[i].transform.position.y < posBounds[0].y || bigMosq[i].transform.position.y > posBounds[1].y)
                {
                    bigMosqClass[i].resetMosq();
                    aliveMosqCount++;
                    if (aliveMosqCount == 5)
                    {
                        singletonManager.Instance.gameOver = true;
                        mosquitoSound.Stop();
                        takeInput = false;
                        singletonManager.Instance.updateGameCount();
                    }
                    uiI.processLife();
                }
            }
        }
        if (wasp.activeSelf)
        {
            wasp.transform.position += (Vector3)waspClass.direction * 7f * Time.deltaTime;
            if (wasp.transform.position.x < posBounds[0].x || wasp.transform.position.x > posBounds[1].x || wasp.transform.position.y < posBounds[0].y || wasp.transform.position.y > posBounds[1].y)
            {
                waspClass.resetMosq();
            }
        }

        if (streakMosq.activeSelf)
        {
            streakMosq.transform.position += (Vector3)streakMosqClass.direction * singletonManager.Instance.streakMosqSpeed * Time.deltaTime;
            if (streakMosq.transform.position.x < posBounds[0].x || streakMosq.transform.position.x > posBounds[1].x || streakMosq.transform.position.y < posBounds[0].y || streakMosq.transform.position.y > posBounds[1].y)
            {
                streakMosqClass.resetMosq();
                singletonManager.Instance.streakMosqSpeed = 5f;
                streakIconIndex = -1;
                singletonManager.Instance.scoreMultiplier = 1;
                StartCoroutine(initiateStreak());
            }
        }
        if (!highScoreDetected && singletonManager.Instance.bestScore != 0 && singletonManager.Instance.currentScore > singletonManager.Instance.bestScore)
        {
            highScoreDetected = true;
            highScoreAlarmPanelObj.SetActive(true);
            hHsa.shootAlarm();
        }
    }

    public void detectGameStart()
    {
        if (!singletonManager.Instance.gameResumed)
        {
            currtime = 0f;
        }
        else
        {
            resumeGame();
        }
        mosquitoSound.volume = singletonManager.Instance.soundVolume == 1f ? 0.3f : 0f;
    }

    void calculatePosBound()
    {
        float scrAspect = (float)Screen.width / (float)Screen.height;
        float camHeight = Camera.main.orthographicSize * 2f;
        Bounds tempBound = new Bounds(Camera.main.transform.position, new Vector3(camHeight * scrAspect, camHeight, 0f));
        posBounds = new Vector2[2];
        posBounds[1] = new Vector2();
        posBounds[1].x = tempBound.max.x * 1.2f;
        posBounds[1].y = tempBound.max.y * 1.2f;
        posBounds[0] = new Vector2();
        posBounds[0].x = posBounds[1].x * -1f;
        posBounds[0].y = posBounds[1].y * -1f;
    }

    private IEnumerator spawnSmallMosquito()
    {
        while (!singletonManager.Instance.gameOver)
        {
            int mosqIndex = 0;
            while (mosqIndex < smallMosq.Length)
            {
                if (!smallMosq[mosqIndex].activeSelf)
                {
                    posDir tempStruct = calculatePosDir();
                    smallMosqClass[mosqIndex].spawnMosquito(tempStruct.posVec, tempStruct.dirVec, true);
                    mosqIndex = smallMosq.Length;
                }
                mosqIndex++;
            }
            yield return new WaitForSeconds(singletonManager.Instance.smallMosqRate);
        }
    }

    private IEnumerator spawnBigMosquito()
    {
        while (!singletonManager.Instance.gameOver)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 10f));
            int mosqIndex = 0;
            while (mosqIndex < bigMosq.Length)
            {
                if (!bigMosq[mosqIndex].activeSelf)
                {
                    posDir tempStruct = calculatePosDir();
                    bigMosqClass[mosqIndex].spawnMosquito(tempStruct.posVec, tempStruct.dirVec, true, true);
                    mosqIndex = bigMosq.Length;
                }
                mosqIndex++;
            }
        }
    }

    private IEnumerator spawnEnemy()
    {
        while (!singletonManager.Instance.gameOver)
        {
            float spawnTime = Random.Range(5f, 10f);
            yield return new WaitForSeconds(spawnTime);
            posDir tempStruct = calculatePosDir();
            waspClass.spawnMosquito(tempStruct.posVec, tempStruct.dirVec, false);
        }
    }

    private IEnumerator initiateStreak()
    {
        yield return new WaitForSeconds(20f);
        if (!singletonManager.Instance.gameOver)
        {
            spawnStreak();
        }
    }

    private void spawnStreak()
    {
        posDir tempStruct = calculatePosDir();
        streakMosqClass.spawnMosquito(tempStruct.posVec, tempStruct.dirVec, false); // making it false so lizard cant increase score while eating streak mosq
    }

    private posDir calculatePosDir()
    {
        posDir retData;
        float xORy = UnityEngine.Random.Range(1, 10);
        float posX, posY;
        if (xORy % 2f == 0f)
        {
            posX = UnityEngine.Random.Range(posBounds[0].x, posBounds[1].x);
            posY = posBounds[UnityEngine.Random.Range(0, 1)].y;
        }
        else
        {
            posY = UnityEngine.Random.Range(posBounds[0].y, posBounds[1].y);
            posX = posBounds[UnityEngine.Random.Range(0, 1)].x;
        }
        Vector2 posToSpawn = new Vector2(posX, posY);
        Vector2 dirToMove = posToSpawn * -1f;
        if (xORy % 2f == 0f)
        {
            dirToMove.x += UnityEngine.Random.Range(-3f, 3f);
        }
        else
        {
            dirToMove.y += UnityEngine.Random.Range(-3f, 3f);
        }
        retData.posVec = posToSpawn;
        retData.dirVec = dirToMove;
        return retData;
    }

    public void setTouchPos(Vector2 pos)
    {
        touchPos = Camera.main.ScreenToWorldPoint(pos);
        touchRecieved = true;
    }

    public void initShock()
    {
        shockActivated = true;
    }

    public void enableLizardSpawn()
    {
        canSpawnLizard = true;
    }

    public void resumeGame()
    {
        takeInput = true;
        StartCoroutine(spawnSmallMosquito());
        StartCoroutine(spawnBigMosquito());
        StartCoroutine(initiateStreak());
        BTCurrtime = singletonManager.Instance.soundVolume == 1f ? 0f : BTTotalTime;
        mosquitoSound.Play();
        if (singletonManager.Instance.gameResumed)
        {
            mosquitoSound.volume = singletonManager.Instance.soundVolume == 1f ? 0.3f : 0f;
            singletonManager.Instance.gameResumed = false;
        }
    }

    public void playSquashSound(int clipIndex)
    {
        if (mosqTapSounds[clipIndex].isPlaying)
        {
            mosqTapSounds[clipIndex].Stop();
        }
        mosqTapSounds[clipIndex].Play();
    }
}
