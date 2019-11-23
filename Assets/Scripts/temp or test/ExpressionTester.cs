using UnityEngine;
using UnityEngine.UI;

public class ExpressionTester : MonoBehaviour {

    [SerializeField] InputField inputField;
    [SerializeField] Text outputField;

    void Start () {
        inputField.onEndEdit.AddListener((input) => {
            outputField.text = input;
        });
    }
	
}
