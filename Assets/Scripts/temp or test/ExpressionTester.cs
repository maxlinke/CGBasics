using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ExpressionTester : MonoBehaviour {

    [SerializeField] InputField inputField;
    [SerializeField] Button testButton;
    [SerializeField] Text outputField;

    void Start () {
        outputField.text = GetFunctionHelper();

        inputField.onEndEdit.AddListener((input) => {
            Dictionary<string, float> vars = new Dictionary<string, float>();
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

            var outputString = string.Empty;
            try{
                outputString = StringExpressions.ParseExpression(input, vars).ToString();
            }catch(System.Exception e){
                outputString = e.ToString();
            }finally{
                outputField.text = $"{outputString}\n\nf{Time.frameCount}";
            }

            // outputField.text = "";
            // for(int i=0; i<35; i++){
            //     // string generated = GenerateFloatingPointNumberString();
            //     // if(StringExpressions.TryParseExpression(generated, out var parsed)){
            //     //     outputField.text += $"{generated} -> {parsed.ToString()}";
            //     // }else{
            //     //     outputField.text += $" >>> Error <<< {generated}";
            //     // }
            //     outputField.text += GenerateTestString(4, whiteSpaceEverywhere : false);
            //     outputField.text += "\n";
            // }
        });

        testButton.onClick.AddListener(() => {
            Test(
                testNumber: 10000,
                generateInput: () => GenerateTestString(4),
                out var errorInputs,
                NaNIsError: false
            );
            if(errorInputs.Length > 0){
                var errorMsg = $"{errorInputs.Length} errors!";
                foreach(var errorInput in errorInputs){
                    errorMsg += $"\n{errorInput}";
                }
                Debug.LogError(errorMsg);
            }else{
                Debug.Log("no issues");
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

    void Test (int testNumber, System.Func<string> generateInput, out string[] errorInputs, bool NaNIsError = true) {
        List<string> errorList = new List<string>();
        for(int i=0; i<testNumber; i++){
            var generated = generateInput();
            try{
                var parsed = StringExpressions.ParseExpression(generated, null);
                if(float.IsNaN(parsed) && NaNIsError){
                    errorList.Add(generated);
                }
            }catch(System.Exception){
                errorList.Add(generated);
            }
        }
        errorInputs = errorList.ToArray();
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
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        float parenthesisProbability = 0.333f;
        float functionProbability = 0.2f;
        float variableProbability = 0.2f;
        int parenthesisLevel = 0;
        string[] operands = new string[]{"+", "-", "*", "/"};
        // var allFunctions = StringExpressions.Functions.GetAllFunctions();
        // List<string> forbiddenFunctions = new List<string>(){"asin", "acos", "atan", "atan", "sqrt"};     //all the functions that can return NaN
        // List<StringExpressions.Functions.Function> allowedFunctionList = new List<StringExpressions.Functions.Function>();
        // foreach(var function in allFunctions){
        //     if(!forbiddenFunctions.Contains(function.functionName)){
        //         allowedFunctionList.Add(function);
        //     } 
        // }
        // var functions = allowedFunctionList.ToArray();
        var functions = StringExpressions.Functions.GetAllFunctions();
        for(int i=0; i<numberOfOperands; i++){
            if(Random.value < parenthesisProbability){
                sb.Append("(");
                parenthesisLevel++;
                MaybeAddWhiteSpace();
            }
            int switchInput = Random.Range(0, 2 - (maxFunctionDepth > 0 ? 0 : 1) + (variables != null ? 1 : 0));
            if(switchInput == 1){
                if(Random.value > functionProbability){
                    switchInput = 0;
                }
            }else if(switchInput == 2){
                if(Random.value > variableProbability){
                    switchInput = 0;
                }
            }
            switch(switchInput){
                case 0:
                    sb.Append(GenerateFloatingPointNumberString());
                    break;
                case 1: 
                    var function = functions.Random();
                    sb.Append(function.functionName);
                    sb.Append("(");
                    for(int j=0; j<function.paramNumber; j++){
                        sb.Append(GenerateTestString(Mathf.Max(numberOfOperands / 2, 1), variables, whiteSpaceEverywhere, maxFunctionDepth-1));
                        if(j+1 < function.paramNumber){
                            sb.Append(",");
                            MaybeAddWhiteSpace();
                        }
                    }
                    sb.Append(")");
                    break;
                case 2:
                    sb.Append(variables.RandomKey());
                    break;
                default:
                    throw new System.NotImplementedException("This is a programmer error!");
            }
            MaybeAddWhiteSpace();
            if(Random.value < parenthesisProbability && parenthesisLevel > 0){
                sb.Append(")");
                parenthesisLevel--;
                MaybeAddWhiteSpace();
            }
            if(i+1 < numberOfOperands){
                sb.Append(operands.Random());
                MaybeAddWhiteSpace();
            }
        }
        while(parenthesisLevel>0){
            sb.Append(")");
            parenthesisLevel--;
        }
        return sb.ToString();

        void MaybeAddWhiteSpace () {
            if(whiteSpaceEverywhere) sb.Append(" ");
        }
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
