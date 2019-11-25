using UnityEngine;
using System.Collections.Generic;

public static class StringExpressions {
    
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
            // inputString = RemoveAllWhiteSpaces(inputString);
            inputString = inputString.Trim();
            int testID = 4;
            string output;
            string remainder;
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
                    remainder = ParseAndRemoveNumber(inputString, out parsedNumber);
                    output = $"{parsedNumber.ToString()}\n\"{remainder}\"";
                    break;
                case 4: 
                    remainder = ParseAndRemoveOperand(inputString, out parsedNumber, variables);
                    output = $"{parsedNumber.ToString()}\n\"{remainder}\"";
                    break;
                default: 
                    output = "invalid testID";
                    break;
            }
            return output;
        }
    public static float ParseExpression (string inputExpression, Dictionary<string, float> variables) {
        PruneInputExpression();
        var postfix = InfixToPostfix(inputExpression, variables);
        return EvaluatePostfix(postfix);
        void PruneInputExpression () {
            if(inputExpression == null){
                throw new System.NullReferenceException("Input Expression can't be null!");
            }
            // inputExpression = RemoveAllWhiteSpaces(inputExpression);
            inputExpression = inputExpression.Trim();
            if(!(inputExpression.Length > 0)){
                throw new System.ArgumentException("Input Expression can't be empty!");
            }
        }
    }
    private static Queue<Token> InfixToPostfix (string inputExpression, Dictionary<string, float> variables) {
        Stack<Token> tempStack = new Stack<Token>();
        Queue<Token> postfix = new Queue<Token>();
        bool nextPlusOrMinusIsSignInsteadOfOperator = true;
        float tempSign = 1;
        while(inputExpression.Length > 0){
            char ch = inputExpression[0];
            if(IsOperatorChar(ch, true)){
                if(ch == ')'){
                    while(!tempStack.Peek().Equals('(')){
                        postfix.Enqueue(tempStack.Pop());
                        if(!(tempStack.Count > 0)){
                            throw new System.ArgumentOutOfRangeException("Mismatched parenthesis count!");
                        }
                    }
                    tempStack.Pop();
                }else if((ch == '+' || ch == '-') && nextPlusOrMinusIsSignInsteadOfOperator){
                    tempSign *= (ch == '-' ? -1 : 1);
                }else{
                    if(tempStack.Count > 0){
                        if(OutStackPrecedence(ch) > InStackPrecedence(tempStack.Peek())){
                            tempStack.Push(new OperatorToken(ch));
                        }else{
                            while((tempStack.Count > 0) && (OutStackPrecedence(ch) < InStackPrecedence(tempStack.Peek()))){
                                postfix.Enqueue(tempStack.Pop());
                            }
                            tempStack.Push(new OperatorToken(ch));
                        }
                    }else{
                        tempStack.Push(new OperatorToken(ch));
                    }
                    nextPlusOrMinusIsSignInsteadOfOperator = true;
                }
                inputExpression = inputExpression.Substring(1);
            }else{
                inputExpression = ParseAndRemoveOperand(inputExpression, out float parsedOperand, variables);
                parsedOperand *= tempSign;
                tempSign = 1;
                nextPlusOrMinusIsSignInsteadOfOperator = false;
                postfix.Enqueue(new NumberToken(parsedOperand));
            }
            inputExpression = inputExpression.Trim();
        }
        while(tempStack.Count > 0){
            postfix.Enqueue(tempStack.Pop());
        }
        return postfix;
    }
    private static float EvaluatePostfix (Queue<Token> postfix) {
        Stack<Token> tempStack = new Stack<Token>();
        while(postfix.Count > 0){
            var top = postfix.Dequeue();
            if(top is NumberToken){
                tempStack.Push(top);
            }else{
                // float a = (tempStack.Count > 0 ? ((NumberToken)(tempStack.Pop())).value : 0);    //this almost works but "2--2" results in "-4"...
                // float b = (tempStack.Count > 0 ? ((NumberToken)(tempStack.Pop())).value : 0);
                float a = ((NumberToken)(tempStack.Pop())).value;
                float b = ((NumberToken)(tempStack.Pop())).value;
                switch(((OperatorToken)top).value){
                    case '+':
                        tempStack.Push(new NumberToken(b + a));
                        break;
                    case '-':
                        tempStack.Push(new NumberToken(b - a));
                        break;
                    case '*':
                        tempStack.Push(new NumberToken(b * a));
                        break;
                    case '/':
                        tempStack.Push(new NumberToken(b / a));
                        break;
                }
            }
        }
        return ((NumberToken)(tempStack.Pop())).value;
    }
    // private static string RemoveAllWhiteSpaces (string input) {
    //     var noWhiteSpace = input.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);     // TODO figure out why Regex.Replace(input, @"s", ""); didn't work...
    //     var output = string.Empty;
    //     foreach(var subString in noWhiteSpace){
    //         output += subString;
    //     }
    //     return output;
    // }
    private static string ParseAndRemoveOperand (string inputString, out float operandValue, Dictionary<string, float> variables) {
        float sign = 1;
        int charCounter = 0;
        operandValue = float.NaN;
        foreach(char ch in inputString){
            charCounter ++;
            if(ch == '-'){
                sign *= -1;
                continue;
            }else if(ch != '+'){
                inputString = inputString.Remove(0, charCounter - 1);
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
                if(IsNumberChar(ch, false) || ch == '+' || ch == '-'){
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
            case "pi":      return Exec0(() => Mathf.PI);
            case "e":       return Exec0(() => (float)(System.Math.E));            // interesting that Mathf doesn't have that value...
            case "sin":     return Exec1(Mathf.Sin);
            case "cos":     return Exec1(Mathf.Cos);
            case "tan":     return Exec1(Mathf.Tan);
            case "asin":    return Exec1(Mathf.Asin);
            case "acos":    return Exec1(Mathf.Acos);
            case "atan":    return Exec1(Mathf.Atan);
            case "atan2":   return Exec2(Mathf.Atan2);
            case "sqrt":    return Exec1(Mathf.Sqrt);
            case "pow":     return Exec2(Mathf.Pow);
            case "exp":     return Exec1(Mathf.Exp);
            default:        throw new System.ArgumentException($"Unknown function call \"{functionName}\"...");
        }
        void CheckParameterCount (int expectedParameterCount) {
            if(parameters.Length != expectedParameterCount){
                throw new System.ArgumentException($"Function \"{functionName}\" expected {expectedParameterCount} parameters but got {parameters.Length}!");
            }
        }
        float Param (int index) {
            return ParseExpression(parameters[index], variables);
        }
        float Exec0 (System.Func<float> function){
            CheckParameterCount(0);
            return function();
        }
        float Exec1 (System.Func<float, float> function) {
            CheckParameterCount(1);
            return function(Param(0));
        }
        float Exec2 (System.Func<float, float, float> function) {
            CheckParameterCount(2);
            return function(Param(0), Param(1));
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
                if(parenthesisCounter == 0){
                    if(paramLength > 0 || parameterList.Count > 0){
                        parameterList.Add(inputString.Substring(currentParameterStart, paramLength));
                    }
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
    private static uint InStackPrecedence (char opChar) {
        switch(opChar){
            case '(': return 0;
            case '+': return 2;
            case '-': return 2;
            case '*': return 4;
            case '/': return 4;
            default: throw new System.ArgumentException($"Invalid Operator \"{opChar}\"!");
        }
    }
    private static uint InStackPrecedence (Token tok) {
        if(tok is NumberToken){
            return 0;
        }else{
            return InStackPrecedence(((OperatorToken)tok).value);
        }
    }
    // https://www.geeksforgeeks.org/infix-to-postfix-using-different-precedence-values-for-in-stack-and-out-stack/
    private static uint OutStackPrecedence (char opChar) {
        switch(opChar){
            case '(': return 100;
            case '+': return 1;
            case '-': return 1;
            case '*': return 3;
            case '/': return 3;
            default: throw new System.ArgumentException($"Invalid Operator \"{opChar}\"!");
        }
    }
    private static uint OutStackPrecedence (Token tok) {
        if(tok is NumberToken){
            return 0;
        }else{
            return OutStackPrecedence(((OperatorToken)tok).value);
        }
    }
    private abstract class Token { 
        public abstract bool Equals (char ch);
    }
    private class NumberToken : Token {
        public readonly float value;
        public NumberToken (float value) {
            this.value = value;
        }
        public override bool Equals (char ch) {
            return false;
        }
    }
    private class OperatorToken : Token {
        public readonly char value;
        public OperatorToken (char value) {
            if(!IsOperatorChar(value, true)){
                throw new System.ArgumentException($"Invalid Operator \"{value}\"!");
            }
            this.value = value;
        }
        public override bool Equals (char ch) {
            return ch == this.value;
        }
    }
}

