using UnityEngine;
using UnityEngine.UI;
using StringExpressions;
using System.Data;
using System.Collections.Generic;

public class ExpressionTester : MonoBehaviour {

    [SerializeField] InputField inputField;
    [SerializeField] Text outputField;

    void Start () {
        // int i = -1;
        // char c = (char)i;
        // string s = c.ToString();
        // outputField.text = s;

        inputField.onEndEdit.AddListener((input) => {
            // if(Tokenizer.TryTokenize(input, out Token[] tokens)){
            //     foreach(var token in tokens){
            //         outputField.text += $"{token.stringValue}\n";
            //     }
            // }else{
            //     outputField.text = "Error";
            // }

            // var outputString = string.Empty;
            // try{
            //     var result = new DataTable().Compute(input, null);
            //     outputString = $"{result} ({result.GetType()})\n";
            // }catch(System.Exception e){
            //     outputString = e.ToString();
            // }finally{
            //     outputField.text = outputString;
            // }

            // outputField.text = input;

            if(float.TryParse(input, out float val)){
                outputField.text = val.ToString();
            }else{
                outputField.text = "Error";
            }
        });
    }

    // second comment at https://stackoverflow.com/questions/3422673/how-to-evaluate-a-math-expression-given-in-string-form
    // is somewhat useful i guess

    bool TryParseExpression (string inputExpression, out float parsedValue, Dictionary<string, float> variables = null) {
        parsedValue = float.NaN;
        return false;
    }

    string RemoveAllWhiteSpaces (string input) {
        var noWhiteSpace = input.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);     // TODO figure out why Regex.Replace(input, @"s", ""); didn't work...
        var output = string.Empty;
        foreach(var subString in noWhiteSpace){
            output += subString;
        }
        return output;
    }
	
}
