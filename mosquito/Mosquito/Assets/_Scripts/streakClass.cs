using UnityEngine;

public class streakClass : MonoBehaviour {
	public Sprite[] icons;
	public Animator origController, shadowController;
	public SpriteRenderer origRenderer, shadowRenderer;

	public void showStreak(Vector3 pos, int iconIndex){
		origRenderer.sprite = icons[iconIndex];
		shadowRenderer.sprite = icons[iconIndex];
		transform.position = pos;
		origController.SetTrigger("shootAnim");
		shadowController.SetTrigger("shootAnim");
		Invoke("resetIcon", 1f);
	}

	void resetIcon(){
		transform.localPosition = Vector3.zero;
	}
}
