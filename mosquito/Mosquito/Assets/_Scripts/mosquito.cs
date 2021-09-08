using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mosquito : MonoBehaviour {
	[HideInInspector]
	public Vector2 direction;

	private SpriteRenderer selfRenderer;
	private bool isBig, canRotate, isMosq; // isMosq : If this is even a mosquito or not .... isBig : if it is a mosquito then if big or small
	private float currTime, totalTime;
	private Vector2 fromDir, targetDir;
	private int rotCount;

	void Awake(){
		direction = new Vector2();
		selfRenderer = GetComponent<SpriteRenderer>();
	}

	void Update(){
		if(isBig && canRotate){
			currTime += Time.deltaTime;
			if(currTime >= totalTime){
				canRotate = false;
				currTime = totalTime;
				rotCount++;
			}
			float t = currTime / totalTime;
			t = t * t * (3f - 2f * t);
			direction = Vector2.Lerp(fromDir, targetDir, t);
			if(currTime == totalTime && rotCount < 3){
				setRot();
			}
		}
        if (handleLizard.IsLizardActivated && singletonManager.Instance.lizardInstance.Mosquitoe == null && Vector3.Distance(transform.position, singletonManager.Instance.lizardInstance.transform.position) <= handleLizard.attackRange)
        {
            singletonManager.Instance.lizardInstance.Mosquitoe = this;
        }
	}

    public bool IsBig
    {
        get
        {
            return isBig;
        }
    }

    public bool IsMosquito
    {
        get
        {
            return isMosq;
        }
    }

	public void resetMosq(){
		transform.localPosition = Vector2.zero;
		selfRenderer.flipX = false;
		gameObject.SetActive(false);
		isBig = false;
		canRotate = false;
		rotCount = 0;
		if(IsInvoking("setRot")){
			CancelInvoke("setRot");
		}
	}

	public void spawnMosquito(Vector2 spawnPosition, Vector2 moveDirection, bool isMosquito){
		transform.position = spawnPosition;
		direction = moveDirection.normalized;
        isMosq = isMosquito;
		if(direction.x > 0f){
			selfRenderer.flipX = true;
		}
		gameObject.SetActive(true);
	}

	public void spawnMosquito(Vector2 spawnPos, Vector2 moveDir, bool isMosquito, bool isBigMosq){
		spawnMosquito(spawnPos, moveDir, isMosquito);
		isBig = isBigMosq;
		Invoke("setRot", 1f);
	}

	void setRot(){
		fromDir = direction;
		targetDir = Quaternion.Euler(0f,0f,Random.Range(45f, 90f)) * fromDir;
		currTime = 0f;
		totalTime = Random.Range(0.2f, 1f);
		canRotate = true;
	}

}
