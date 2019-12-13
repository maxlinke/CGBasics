using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class FoldoutTester : MonoBehaviour {

    [SerializeField] Slider scaleSlider;
    [SerializeField] Slider amountSlider;
    [SerializeField] int minNameLength;
    [SerializeField] int maxNameLength;

    void Update () {
        if(Input.GetKeyDown(KeyCode.Q)){
            var setups = new List<Foldout.ButtonSetup>();
            StringBuilder sb = new StringBuilder();
            for(int i=0; i<amountSlider.value; i++){
                int nameLength = Random.Range(minNameLength, maxNameLength + 1);
                sb.Append($"{i} ");
                for(int j=0; j<nameLength; j++){
                    sb.Append((char)(Random.Range('a', 'z' + 1)));
                }
                setups.Add(new Foldout.ButtonSetup(
                    buttonName: sb.ToString(),
                    buttonHoverMessage: string.Empty,
                    buttonClickAction: null,
                    buttonInteractable: Random.value < 0.5f
                ));
                sb.Clear();
            }
            Foldout.Create(setups, null, scaleSlider.value);
        }
    }
	
}
