using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ExpressionTester : MonoBehaviour {

    [SerializeField] InputField inputField;
    [SerializeField] Text outputField;

    void Start () {
        outputField.text = GetFunctionHelper();
        

        inputField.onEndEdit.AddListener((input) => {Dictionary<string, float> vars = new Dictionary<string, float>();
            // vars.Add("x", 1);
            // vars.Add("y", 2);
            // vars.Add("z", 3);

            // var outputString = string.Empty;
            // int testCount = 16 * 6;
            // for(int i=0; i<testCount; i++){        //simulating all the matrices
            //     try{
            //         string numberToParse = input + "+" + (testCount - i - 1).ToString();
            //         outputString = StringExpressions.ParseExpression(numberToParse, vars).ToString();
            //     }catch(System.Exception e){
            //         outputString = $"{e.Message}\n{e.StackTrace}";
            //     }
            // }
            // outputField.text = $"{outputString}\n\n{GetFunctionHelper()}\nframe{Time.frameCount}";

            outputField.text = "";
            for(int i=0; i<35; i++){
                string generated = GenerateFloatingPointNumberString();
                if(StringExpressions.TryParseExpression(generated, out var parsed)){
                    outputField.text += $"{generated} -> {parsed.ToString()}";
                }else{
                    outputField.text += $" >>> Error <<< {generated}";
                }
                outputField.text += "\n";
            }
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

    string VarsToNiceString (Dictionary<string, float> variables) {
        int maxVarNameLength = 0;
        string[] varNames = new string[variables.Keys.Count];
        float[] varValues = new float[variables.Values.Count];
        int i = 0;
        foreach(var varName in variables.Keys){
            varNames[i] = varName;
            varValues[i] = variables[varName];
            maxVarNameLength = Mathf.Max(maxVarNameLength, varName.Length);
            i++;
        }
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for(i=0; i<varNames.Length; i++){
            int varNameLength = varNames[i].Length;
            sb.Append(varNames[i]);
            for(int j=0; j<maxVarNameLength-varNameLength; j++){
                sb.Append(" ");
            }
            sb.Append($" \t{varValues[i]}\n");
        }
        return sb.ToString();
    }

    string GenerateTestString (int numberOfOperands, Dictionary<string, float> variables = null, bool whiteSpaceEverywhere = true, int maxFunctionDepth = 3) {
        var output = string.Empty;
        int parenthesisLevel = 0;
        int functionParamsToAdd = 0;
        for(int i=0; i<numberOfOperands; i++){

        }
        return output;
    }

    string GenerateFloatingPointNumberString () {
        var output = Random.Range(-256, +256).ToString();
        if(Random.value > 0.5f) return output;
        output += $".{Random.Range(0, +256)}";
        if(Random.value > 0.5f) return output;
        output += $"E{Random.Range(-6, 6)}";
        return output;
    }
	
}
