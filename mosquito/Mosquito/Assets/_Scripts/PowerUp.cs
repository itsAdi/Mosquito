using UnityEngine;
using UnityEngine.UI;
using System;
[Serializable]
public class PowerUp {
    public CanvasRenderer Indicator;
    public Button PowerUpButton;
    public CanvasRenderer PowerUpImage;

    public void MakeAbilityAccessible()
    {
        PowerUpButton.interactable = true;
        Indicator.SetAlpha(1f);
        PowerUpImage.SetAlpha(1f);
    }

    public void RestrictAbility()
    {
        PowerUpButton.interactable = false;
        Indicator.SetAlpha(0f);
        PowerUpImage.SetAlpha(0f);
    }
}
