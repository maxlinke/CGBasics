using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ExpressionTester : MonoBehaviour {

    [SerializeField] InputField inputField;
    [SerializeField] Text outputField;

    void Start () {
        outputField.text = GetFunctionHelper();

        inputField.onEndEdit.AddListener((input) => {Dictionary<string, float> vars = new Dictionary<string, float>();
            vars.Add("x", 1);
            vars.Add("y", 2);
            vars.Add("z", 3);

            var outputString = string.Empty;
            try{
                outputString = StringExpressions.ParseExpression(input, vars).ToString();
            }catch(System.Exception e){
                outputString = e.Message;
            }finally{
                outputField.text = $"{outputString}\n\n{GetFunctionHelper()}\n\nframe{Time.frameCount}";
            }
            // outputString = ExpressionHandler.ParseExpression(input, vars).ToString();
        });
    }

    string GetFunctionHelper () {
        var funcs = StringExpressions.Functions.GetAllFunctions();
        int maxCallLength = 0;
        string[] calls = new string[funcs.Length];
        string[] descs = new string[funcs.Length];
        for(int i=0; i<funcs.Length; i++){
            calls[i] = funcs[i].exampleCall;
            descs[i] = funcs[i].description;
            maxCallLength = Mathf.Max(maxCallLength, calls[i].Length);
        }
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for(int i=0; i<calls.Length; i++){
            int callLength = calls[i].Length;
            sb.Append(calls[i]);
            for(int j=0; j<maxCallLength-callLength; j++){
                sb.Append(" ");
            }
            sb.Append($" \t{descs[i]}\n");
        }
        return sb.ToString();
    }
	
}
