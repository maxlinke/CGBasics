using UnityEngine;
using UnityEngine.UI;

public class ImageFlasher : MonoBehaviour {

    private const float flashDuration = 0.333f;

    private Image image;
    private Color flashColor;
    private float currentBlend;
    private float currentFlashSpeedMultiplier = 1f;

    public void Initialize (Image image) {
        this.image = image;
        currentBlend = 0f;
        gameObject.SetActive(false);
    }

    public void UpdateFlashColor (Color newFlashColor) {
        flashColor = newFlashColor;
    }

    public void Flash (float speedMultiplier = 1f) {
        gameObject.SetActive(true);
        image.color = flashColor;
        currentBlend = 1f;
        currentFlashSpeedMultiplier = speedMultiplier;
    }

    void Update () {
        if(currentBlend <= 0){
            gameObject.SetActive(false);
            return;
        }
        image.color = Color.Lerp(Color.clear, flashColor, currentBlend);
        currentBlend -= currentFlashSpeedMultiplier * (Time.deltaTime / flashDuration); 
    }
	
}
