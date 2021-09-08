using UnityEngine;

public class shockedMosquito : MonoBehaviour {
	public SpriteRenderer selfRenderer;
	public Animator selfAnim;
	public void showShocked(Vector2 pos, bool flippedX){
		selfRenderer.flipX = flippedX;
		transform.position = pos;
		selfAnim.SetTrigger("shootAnim");
		Invoke("reset", 0.9f);
	}

	void reset(){
		selfRenderer.flipX = false;
		transform.localPosition = Vector2.zero;
	}
}
