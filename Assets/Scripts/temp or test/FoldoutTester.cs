using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Text;

public class FoldoutTester : MonoBehaviour, IPointerClickHandler, IScrollHandler {

    const int minNameLength = 1;
    const int maxNameLength = 20;

    const float minScale = 0.2f;
    const float maxScale = 5f;

    const int minAmount = 0;
    const int maxAmount = 100;

    const float interactableChance = 0.8f;

    [SerializeField] TextMeshProUGUI leftText;
    [SerializeField] TextMeshProUGUI rightText;

    float scale;
    float amount;
    string[] loremIpsum;
    bool scaleIsLeft;

    void Awake () {
        string loremIpsumText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. ";
        loremIpsumText = loremIpsumText.Replace(".", "");
        loremIpsumText = loremIpsumText.Replace(",", "");
        loremIpsumText = loremIpsumText.ToLower();
        loremIpsum = loremIpsumText.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
        scale = 1;
        amount = 5;
        scaleIsLeft = true;
    }

    void Update () {
        string scaleText = $"scale:\n\n{scale}";
        string amountText = $"amount:\n\n{(int)amount}";
        leftText.text = scaleIsLeft ? scaleText : amountText;
        rightText.text = scaleIsLeft ? amountText : scaleText;
    }

    public void OnPointerClick (PointerEventData eventData) {
        var setups = new List<Foldout.ButtonSetup>();
        StringBuilder sb = new StringBuilder();
        for(int i=0; i<((int)amount); i++){
            int nameLength = Random.Range(minNameLength, maxNameLength+1);
            sb.Append($"{i} ");
            for(int j=0; j<nameLength; j++){
                sb.Append($"{loremIpsum[Random.Range(0, loremIpsum.Length)]} ");
            }
            setups.Add(new Foldout.ButtonSetup(
                buttonName: sb.ToString(),
                buttonHoverMessage: string.Empty,
                buttonClickAction: null,
                buttonInteractable: Random.value < interactableChance
            ));
            sb.Clear();
        }
        Foldout.Create(setups, null, scale);
    }

    public void OnScroll (PointerEventData eventData) {
        bool pointerIsLeft = (eventData.position.x < (Screen.width / 2));
        bool increaseScale = (pointerIsLeft && scaleIsLeft) || (!pointerIsLeft && !scaleIsLeft);
        float scaledDelta = (eventData.scrollDelta.y * InputSystem.shiftCtrlMultiplier);
        if(increaseScale){
            scale = Mathf.Clamp(scale + (0.1f * scaledDelta), minScale, maxScale);
        }else{
            amount = (Mathf.Clamp(amount + scaledDelta, minAmount, maxAmount));
        }
    }

}
