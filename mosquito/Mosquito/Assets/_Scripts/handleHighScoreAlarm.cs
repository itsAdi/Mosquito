using UnityEngine;

public class handleHighScoreAlarm : MonoBehaviour {
	public Animator bigAnim, smallAnim;
	public AudioSource trumpet;

	public void shootAlarm(){
		smallAnim.SetTrigger("shootAnim");
		bigAnim.SetTrigger("shootAnim");
		trumpet.Play();
		Invoke("disableObj", 3f);
	}

	void disableObj(){
		gameObject.SetActive(false);
	}
}
