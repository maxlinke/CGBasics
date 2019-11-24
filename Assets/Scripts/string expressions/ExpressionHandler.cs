using System.Collections.Generic;

namespace StringExpressions {

    public class ExpressionHandler {

        public static bool TryEvaluateAsNumber (string inputExpression, out float number, Dictionary<string, float> variables = null) {
            if(inputExpression == null){
                number = float.NaN;
                return false;
            }
            inputExpression = RemoveAllWhiteSpaces(inputExpression);
            if(!(inputExpression.Length > 0)){
                number = float.NaN;
                return false;
            }
            Stack<Token> tokenStack = new Stack<Token>();
            var processedInput = inputExpression;
            while(processedInput.Length > 0){
                if(processedInput[0] == '('){

                }else if(processedInput[0] == ')'){

                }else{

                }
            }


            number = float.NaN;
            return false;
        }

        // TODO use this for testing
        public static string Debug (string inputString, Dictionary<string, float> variables = null) {
            var asdf = RemoveAndGetFunctionParameters(inputString, out var parameters, variables);
            var output = string.Empty;
            foreach(var parameter in parameters){
                output += $"{parameter}\n";
            }
            return output;
        }

        private static string RemoveAllWhiteSpaces (string input) {
            var noWhiteSpace = input.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);     // TODO figure out why Regex.Replace(input, @"s", ""); didn't work...
            var output = string.Empty;
            foreach(var subString in noWhiteSpace){
                output += subString;
            }
            return output;
        }

        private static string RemoveAndGetFirstOperandValue (string inputString, out float operandValue, Dictionary<string, float> variables) {
            bool doneWithSign = false;
            float sign = 1;
            int totalCharCounter = 0;
            int startChar = -1;
            bool parsingNumber = false;
            bool parsingIdentifier = false;
            string processedString = inputString;
            foreach(char ch in inputString){
                totalCharCounter ++;
                if(!doneWithSign){
                    if(ch == '-'){
                        sign *= -1;
                        continue;
                    }else if(ch != '+'){
                        doneWithSign = true;
                        startChar = totalCharCounter - 1;
                        if(IsNumberChar(ch)){
                            parsingNumber = true;
                        }else if(IsIdentifierChar(ch)){
                            parsingIdentifier = true;
                        }else{
                            throw new System.ArgumentException($"Invalid operand char \"{ch}\"!");
                        }
                    }
                }
                if(parsingNumber){

                }else if(parsingIdentifier){

                }else{
                    throw new System.Exception("This part of the code should NEVER be reached. Something went wrong!");
                }
            }
            operandValue = float.NaN;
            return inputString.Remove(0, totalCharCounter);
        }

        private static string ParseAndRemoveNumber (string inputString, out float outputNumber) {
            outputNumber = float.NaN;
            return null;
        }

        // variables and functions...
        private static string ParseAndRemoveIdentifier (string inputString, out float outputNumber, Dictionary<string, float> variables) {
            bool isVariable = false;
            bool isFunction = false;
            int charCounter = 0;
            string processedInput = inputString;
            foreach(var ch in inputString){
                charCounter++;
                if(IsIdentifierChar(ch)){
                    continue;
                }else{
                    if(ch == '('){
                        isFunction = true;
                        var functionName = inputString.Substring(0, charCounter-1);
                        processedInput = inputString.Remove(0, charCounter-1);
                        // get parameter array
                    }else{

                    }
                }
            }

            //for compiler
            outputNumber = float.NaN;
            return null;
        }

        private static string RemoveAndGetFunctionParameters (string inputString, out string[] parameters, Dictionary<string, float> variables) {
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
                    if(parenthesisCounter == 0){
                        parameterList.Add(inputString.Substring(currentParameterStart, charCounter - 1 - currentParameterStart));
                        break;
                    }
                }else if(ch == ','){
                    if(parenthesisCounter == 1){
                        parameterList.Add(inputString.Substring(currentParameterStart, charCounter - 1 - currentParameterStart));
                        currentParameterStart = charCounter;
                    }
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
        private static bool IsNumberChar (char inputChar) {
            return (inputChar >= '0' && inputChar <= '9' || inputChar == '.');
        }

        private static bool IsIdentifierChar (char inputChar) {
            return ((inputChar >= 'a' && inputChar <= 'z') || (inputChar >= 'A' || inputChar <= 'Z'));
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
