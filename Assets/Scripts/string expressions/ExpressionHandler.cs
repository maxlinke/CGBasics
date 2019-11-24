using UnityEngine;
using System.Collections.Generic;

namespace StringExpressions {

    public class ExpressionHandler {

        public static bool TryParseExpression (string inputExpression, out float number, Dictionary<string, float> variables = null) {
            try{
                number = ParseExpression(inputExpression, variables);
                return !(float.IsNaN(number));
            }catch{
                number = float.NaN;
                return false;
            }
        }

        public static string Debug (string inputString, Dictionary<string, float> variables = null) {
            inputString = RemoveAllWhiteSpaces(inputString);
            int testID = 3;
            string output;
            float parsedNumber;
            switch(testID){
                case 0: 
                    RemoveAndGetFunctionParameters(inputString, out var parameters);
                    output = string.Empty;
                    foreach(var parameter in parameters){
                        output += $"{parameter}\n";
                    }
                    break;
                case 1: 
                    ParseAndRemoveIdentifier(inputString, out parsedNumber, variables);
                    output = parsedNumber.ToString();
                    break;
                case 2: 
                    output = string.Empty;
                    foreach(var ch in inputString){
                        output += $"{ch == '('}\t{IsIdentifierChar(ch)}\n";
                    }
                    break;
                case 3:
                    ParseAndRemoveNumber(inputString, out parsedNumber);
                    output = parsedNumber.ToString();
                    break;
                default: 
                    output = "invalid testID";
                    break;
            }
            return output;
        }

        private static float ParseExpression (string inputExpression, Dictionary<string, float> variables) {
            if(inputExpression == null){
                throw new System.NullReferenceException("Input Expression can't be null!");
            }
            inputExpression = RemoveAllWhiteSpaces(inputExpression);
            if(!(inputExpression.Length > 0)){
                throw new System.ArgumentException("Input Expression can't be empty!");
            }
            Stack<Token> tokenStack = new Stack<Token>();
            var processedInput = inputExpression;
            while(processedInput.Length > 0){
                if(processedInput[0] == '('){

                }else if(processedInput[0] == ')'){

                }else{

                }
            }


            return float.NaN;;
        }

        private static string RemoveAllWhiteSpaces (string input) {
            var noWhiteSpace = input.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);     // TODO figure out why Regex.Replace(input, @"s", ""); didn't work...
            var output = string.Empty;
            foreach(var subString in noWhiteSpace){
                output += subString;
            }
            return output;
        }

        private static string ParseAndRemoveOperand (string inputString, out float operandValue, Dictionary<string, float> variables) {
            bool doneWithSign = false;
            float sign = 1;
            int charCounter = 0;
            operandValue = float.NaN;
            foreach(char ch in inputString){
                charCounter ++;
                if(!doneWithSign){
                    if(ch == '-'){
                        sign *= -1;
                        continue;
                    }else if(ch != '+'){
                        doneWithSign = true;
                        inputString = inputString.Substring(charCounter - 1, inputString.Length - charCounter - 1);
                        if(IsNumberChar(ch, true)){
                            inputString = ParseAndRemoveNumber(inputString, out operandValue);
                            break;
                        }else if(IsIdentifierChar(ch)){
                            inputString = ParseAndRemoveIdentifier(inputString, out operandValue, variables);
                            break;
                        }else{
                            throw new System.ArgumentException($"Invalid operand char \"{ch}\"!");
                        }
                    }
                }
            }
            operandValue *= sign;;
            return inputString;
        }

        private static string ParseAndRemoveNumber (string inputString, out float outputNumber) {
            // any + and - should have been removed in the ParseOperand(...)-phase
            if(inputString[0] == '+' || inputString[0] == '-'){
                throw new System.ArgumentException($"Numbers to parse should not have '+' or '-' in the beginning. String: \"{inputString}\"");
            }

            bool pastDecimalPoint = false;
            bool pastExpIndicator = false;
            bool pastExpSign = false;
            int charCounter = 0;
            foreach(char ch in inputString){
                charCounter++;
                if(!pastDecimalPoint){
                    if(IsNumberChar(ch, false)){
                        continue;
                    }else if(ch == '.'){
                        pastDecimalPoint = true;
                        continue;
                    }else if(ValidExpIndicator()){
                        pastDecimalPoint = true;
                        pastExpIndicator = true;
                        continue;
                    }else{
                        charCounter--;
                        break;
                    }
                }else if(!pastExpIndicator){
                    if(IsNumberChar(ch, false)){
                        continue;
                    }else if(ValidExpIndicator()){
                        pastExpIndicator = true;
                        continue;
                    }else{
                        charCounter--;
                        break;
                    }
                }else if(!pastExpSign){
                    if(IsNumberChar(ch, false) || (ch == '+' || ch == '-')){
                        pastExpSign = true;
                        continue;
                    }else{
                        charCounter--;
                        break;
                    }
                }else{
                    if(IsNumberChar(ch, false)){
                        continue;
                    }else{
                        charCounter--;
                        break;
                    }
                }

                bool ValidExpIndicator () {
                    return ((ch == 'e' || ch == 'E') && charCounter > 2);   // to prevent ".Exyz" and "Exyz"
                }
            }

            string numberString = inputString.Substring(0, charCounter);
            if(float.TryParse(numberString, out outputNumber)){
                return inputString.Remove(0, charCounter);                
            }else{
                throw new System.ArgumentException($"Couldn't parse number \"{numberString}\"");
            }

        }

        // variables and functions...
        private static string ParseAndRemoveIdentifier (string inputString, out float outputNumber, Dictionary<string, float> variables) {
            int charCounter = 0;
            foreach(var ch in inputString){
                charCounter++;
                if(IsIdentifierChar(ch)){
                    continue;
                }else{
                    if(ch == '('){
                        var functionName = inputString.Substring(0, charCounter-1);
                        inputString = inputString.Remove(0, charCounter-1);
                        inputString = RemoveAndGetFunctionParameters(inputString, out var parameters);
                        outputNumber = ExecuteFunction(functionName, parameters, variables);
                        return inputString;
                    }else{
                        var variableName = inputString.Substring(0, charCounter-1);
                        inputString = inputString.Remove(0, charCounter-1);
                        outputNumber = GetVariableValue(variableName, variables);
                        return inputString;
                    }
                }
            }
            outputNumber = GetVariableValue(inputString, variables);
            return string.Empty;
        }

        // TODO use extra class with "GetAll" function and each function gets a description (for automatic help generation...)
        private static float ExecuteFunction (string functionName, string[] parameters, Dictionary<string, float> variables) {
            switch(functionName){
                case "pi": 
                    CheckParameterCount(0);
                    return Mathf.PI;
                case "e":
                    CheckParameterCount(0);
                    return (float)System.Math.E;            // interesting that Mathf doesn't have that value...
                // TODO add the rest once i get everything working (needs to use parseexpression for the params...)
                default: 
                    throw new System.ArgumentException($"Unknown function call \"{functionName}\"...");
            }

            void CheckParameterCount (int expectedParameterCount) {
                if(parameters.Length != expectedParameterCount){
                    throw new System.ArgumentException($"Function \"{functionName}\" expected {expectedParameterCount} parameters but got {parameters.Length}!");
                }
            }
        }

        private static float GetVariableValue (string variableName, Dictionary<string, float> variables) {
            if(variables == null){
                throw new System.NullReferenceException($"Requested lookup of variable \"{variableName}\" but variable map was null!");
            }else if(variables.TryGetValue(variableName, out var variableValue)){
                return variableValue;
            }else{
                throw new System.ArgumentException($"Found no variable named \"{variableName}\"!");
            }
        }

        private static string RemoveAndGetFunctionParameters (string inputString, out string[] parameters) {
            if(!(inputString.Length > 0)){
                parameters = new string[0];
                return inputString;
            }
            if(inputString[0] != '('){
                throw new System.ArgumentException($"Parameter string has to start and end with parentheses! Started instead with \"{inputString[0]}\"...");
            }
            int charCounter = 0;
            int parenthesisCounter = 0;
            int currentParameterStart = 1;
            List<string> parameterList = new List<string>();
            foreach(char ch in inputString){
                charCounter++;
                if(ch == '('){
                    parenthesisCounter++;
                }else if(ch == ')'){
                    parenthesisCounter--;
                    int paramLength = ParamStringLength();
                    if(parenthesisCounter == 0 && paramLength > 0){
                        parameterList.Add(inputString.Substring(currentParameterStart, paramLength));
                        break;
                    }
                }else if(ch == ','){
                    if(parenthesisCounter == 1){
                        parameterList.Add(inputString.Substring(currentParameterStart, ParamStringLength()));
                        currentParameterStart = charCounter;
                    }
                }

                int ParamStringLength () {
                    return charCounter - 1 - currentParameterStart;
                }
            }
            if(parenthesisCounter != 0){
                throw new System.ArgumentException($"Paramter list was not complete. {parenthesisCounter} parentheses were missing!");
            }

            parameters = parameterList.ToArray();
            return inputString.Remove(0, charCounter);
        }

        private static bool IsOperatorChar (char inputChar, bool includeParantheses = false) {
            switch(inputChar){
                case '(': return includeParantheses;
                case ')': return includeParantheses;
                case '+': return true;
                case '-': return true;
                case '*': return true;
                case '/': return true;
                default: return false;
            }
        }

        // e and + or - needs special treatment (as in 2.3e-5)...
        private static bool IsNumberChar (char inputChar, bool includeDecimalPoint = false) {
            return ((inputChar >= '0' && inputChar <= '9') || (inputChar == '.' && includeDecimalPoint));
        }

        private static bool IsIdentifierChar (char inputChar) {
            return ((inputChar >= 'a' && inputChar <= 'z') || (inputChar >= 'A' && inputChar <= 'Z'));
        }

        // https://www.geeksforgeeks.org/infix-to-postfix-using-different-precedence-values-for-in-stack-and-out-stack/
        private uint InStackPrecedence (char opChar) {
            switch(opChar){
                case '(': return 0;
                case '+': return 2;
                case '-': return 2;
                case '*': return 4;
                case '/': return 4;
                default: throw new System.ArgumentException($"Invalid Operator \"{opChar}\"!");
            }
        }

        // https://www.geeksforgeeks.org/infix-to-postfix-using-different-precedence-values-for-in-stack-and-out-stack/
        private uint OutStackPrecedence (char opChar) {
            switch(opChar){
                case '(': return 100;
                case '+': return 1;
                case '-': return 1;
                case '*': return 3;
                case '/': return 3;
                default: throw new System.ArgumentException($"Invalid Operator \"{opChar}\"!");
            }
        }

        private abstract class Token { }

        private class NumberToken : Token {

            public readonly float value;

            public NumberToken (float value) {
                this.value = value;
            }
        }

        private class OperatorToken : Token {

            private readonly char value;

            public OperatorToken (char value) {
                if(!IsOperatorChar(value, true)){
                    throw new System.ArgumentException($"Invalid Operator \"{value}\"!");
                }
                this.value = value;
            }
        }
    }
}
