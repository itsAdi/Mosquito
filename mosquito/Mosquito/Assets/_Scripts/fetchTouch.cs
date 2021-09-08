using UnityEngine;
using UnityEngine.EventSystems;

public class fetchTouch : MonoBehaviour, IPointerDownHandler {
	public handleGameplay hG;
	public UIInput UIInpt;
	public void OnPointerDown(PointerEventData e){
		if(!singletonManager.Instance.gameOver){
			if(!UIInpt.gameStarted){
				UIInpt.playGame();
			}else{
				hG.setTouchPos(e.position);
			}
		}
	}
}
