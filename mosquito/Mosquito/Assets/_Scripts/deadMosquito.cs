using UnityEngine;

public class deadMosquito : MonoBehaviour {
	public Sprite nextStageSprite;

	private SpriteRenderer selfRenderer;
	private Sprite initSprite;

	void Awake(){
		selfRenderer = GetComponent<SpriteRenderer>();
		initSprite = selfRenderer.sprite;
	}

	public void makeMosquitoDead(Vector2 pos, bool xFlipped){
		transform.position = pos;
		selfRenderer.flipX = xFlipped;
		Invoke("changeSprite", 0.3f);
	}

	void changeSprite(){
		selfRenderer.sprite = nextStageSprite;
		Invoke("resetObject", 1f);
	}

	void resetObject(){
		transform.localPosition = Vector2.zero;
		selfRenderer.sprite = initSprite;
		selfRenderer.flipX = false;
		gameObject.SetActive(false);
	}
}
