using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ExpressionTester : MonoBehaviour {

    [SerializeField] InputField inputField;
    [SerializeField] Text outputField;

    void Start () {
        inputField.onEndEdit.AddListener((input) => {Dictionary<string, float> vars = new Dictionary<string, float>();
            vars.Add("x", 1);
            vars.Add("y", 2);
            vars.Add("z", 3);

            var outputString = string.Empty;
            try{
                // outputString = ExpressionHandler.Debug(input);
                outputString = StringExpressions.ParseExpression(input, vars).ToString();
            }catch(System.Exception e){
                outputString = e.Message;
            }finally{
                outputField.text = $"{outputString}\n{Time.frameCount}";
            }
            // outputString = ExpressionHandler.ParseExpression(input, vars).ToString();

            // string outputString = string.Empty;
            // foreach(var ch in input){
            //     outputString += $"{(int)ch}\n";
            // }
            // outputField.text = outputString;
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
