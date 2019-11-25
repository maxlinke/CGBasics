using UnityEngine;
using System.Collections.Generic;

public static class StringExpressions {

    // ((4 - pow(2, 3) + 1) * -sqrt(3*3+4*4)) / 2
    // should return 7.5 (and it CURRENTLY does)
    // just for future tests...
    
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
        // switch(functionName){
        //     case "pi":      return Exec0(() => Mathf.PI);
        //     case "e":       return Exec0(() => (float)(System.Math.E));            // interesting that Mathf doesn't have that value...
        //     case "sin":     return Exec1(Mathf.Sin);
        //     case "cos":     return Exec1(Mathf.Cos);
        //     case "tan":     return Exec1(Mathf.Tan);
        //     case "asin":    return Exec1(Mathf.Asin);
        //     case "acos":    return Exec1(Mathf.Acos);
        //     case "atan":    return Exec1(Mathf.Atan);
        //     case "atan2":   return Exec2(Mathf.Atan2);
        //     case "sqrt":    return Exec1(Mathf.Sqrt);
        //     case "pow":     return Exec2(Mathf.Pow);
        //     case "exp":     return Exec1(Mathf.Exp);
        //     default:        throw new System.ArgumentException($"Unknown function call \"{functionName}\"...");
        // }
        // void CheckParameterCount (int expectedParameterCount) {
        //     if(parameters.Length != expectedParameterCount){
        //         throw new System.ArgumentException($"Function \"{functionName}\" expected {expectedParameterCount} parameters but got {parameters.Length}!");
        //     }
        // }
        // float Param (int index) {
        //     return ParseExpression(parameters[index], variables);
        // }
        // float Exec0 (System.Func<float> function){
        //     CheckParameterCount(0);
        //     return function();
        // }
        // float Exec1 (System.Func<float, float> function) {
        //     CheckParameterCount(1);
        //     return function(Param(0));
        // }
        // float Exec2 (System.Func<float, float, float> function) {
        //     CheckParameterCount(2);
        //     return function(Param(0), Param(1));
        // }
        if(Functions.TryGetFunction(functionName, out var funcToExecute)){
            float[] floatParams = new float[parameters.Length];
            for(int i=0; i<floatParams.Length; i++){
                floatParams[i] = ParseExpression(parameters[i], variables);
            }
            return funcToExecute.Execute(floatParams);
        }else{
            throw new System.ArgumentException($"Unknown function call \"{functionName}\"...");
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

    public static class Functions {

        static Dictionary<string, Function> functions;

        static Functions () {
            functions = new Dictionary<string, Function>();
            AddFunc(new Function0("pi", "Shorthand for 3.1415...", () => Mathf.PI));
            AddFunc(new Function0("e", "Euler's number 2.71828...", () => (float)(System.Math.E)));
            AddFunc(new Function1("sin", $"The sine of angle {Function.IndexToVariableName(0)} in radians", Mathf.Sin));
            AddFunc(new Function1("cos", $"The cosine of angle {Function.IndexToVariableName(0)} in radians", Mathf.Cos));
            AddFunc(new Function1("tan", $"The tangent of angle {Function.IndexToVariableName(0)} in radians", Mathf.Tan));
            AddFunc(new Function1("asin", $"The arc-sine of {Function.IndexToVariableName(0)}, the angle in radians whose sine is {Function.IndexToVariableName(0)}", Mathf.Asin));
            AddFunc(new Function1("acos", $"The arc-cosine of {Function.IndexToVariableName(0)}, the angle in radians whose cosine is {Function.IndexToVariableName(0)}", Mathf.Acos));
            AddFunc(new Function1("atan", $"The arc-tangent of {Function.IndexToVariableName(0)}, the angle in radians whose tangent is {Function.IndexToVariableName(0)}", Mathf.Atan));
            AddFunc(new Function2("atan2", $"The fancier arc-tangent of {Function.IndexToVariableName(0)}/{Function.IndexToVariableName(1)}", Mathf.Atan2));
            AddFunc(new Function1("sqrt", $"The square root of {Function.IndexToVariableName(0)}", Mathf.Sqrt));
            AddFunc(new Function2("pow", $"{Function.IndexToVariableName(0)} raised to power {Function.IndexToVariableName(1)}", Mathf.Pow));
            AddFunc(new Function1("exp", $"e raised to {Function.IndexToVariableName(0)}", Mathf.Exp));
            AddFunc(new Function0("deg2rad", "Degrees to radians conversion multiplier", () => Mathf.Deg2Rad));
            AddFunc(new Function0("rad2deg", "Radians to degrees conversion multiplier", () => Mathf.Rad2Deg));
            AddFunc(new Function1("sindeg", $"The sine of angle {Function.IndexToVariableName(0)} in degrees", (x) => Mathf.Sin(Mathf.Deg2Rad * x)));
            AddFunc(new Function1("cosdeg", $"The cosine of angle {Function.IndexToVariableName(0)} in degrees", (x) => Mathf.Cos(Mathf.Deg2Rad * x)));
            AddFunc(new Function1("tandeg", $"The tangent of angle {Function.IndexToVariableName(0)} in degrees", (x) => Mathf.Tan(Mathf.Deg2Rad * x)));

            void AddFunc (Function funcToAdd) {
                functions.Add(funcToAdd.functionName, funcToAdd);
            }
        }

        public static float ExecuteFunction (string functionName, float[] parameterArray) {
            if(functions.TryGetValue(functionName, out var function)){
                return function.Execute(parameterArray);
            }
            throw new System.MissingMemberException($"There is no function named \"{functionName}\"");
        }

        public static bool TryGetFunction (string functionName, out Function outputFunction) {
            if(functions.TryGetValue(functionName, out outputFunction)){
                return true;
            }
            outputFunction = null;
            return false;
        }

        public static Function[] GetAllFunctions () {
            var outputList = new List<Function>();
            foreach(var f in functions.Values){
                outputList.Add(f);
            }
            return outputList.ToArray();
        }

        public abstract class Function {

            public readonly string functionName;
            public readonly string description;
            public readonly string exampleCall;
            public readonly int paramNumber;
            public abstract float Execute (float[] inputParameters);

            protected Function (string name, string desc, int paramNumber) {
                this.functionName = name;
                this.description = desc;
                this.paramNumber = paramNumber;
                this.exampleCall = GenerateExampleCall(paramNumber);
            }

            public static char IndexToVariableName (int varIndex) {
                return (char)('a' + varIndex);
            }

            protected string GenerateExampleCall (int paramNumber){
                string paramString = "(";
                for(int i=0; i<paramNumber; i++){
                    paramString += IndexToVariableName(i);
                    if(i+1 < paramNumber){
                        paramString += ", ";
                    }
                }
                paramString += ")";
                return functionName + paramString;
            }

            protected void CheckParameterCount (float[] paramArray, int expectedParameterCount) {
                if(paramArray.Length != expectedParameterCount){
                    throw new System.ArgumentException($"Function \"{functionName}\" expected {expectedParameterCount} parameters but got {paramArray.Length}!");
                }
            }
        }

        public class Function0 : Function {

            private System.Func<float> realFunction;

            public Function0 (string name, string desc, System.Func<float> realFunction) : base(name, desc, 0) {
                this.realFunction = realFunction;
            }

            public override float Execute (float[] inputParameters) {
                CheckParameterCount(inputParameters, paramNumber);
                return realFunction();
            }
        }

        public class Function1 : Function {

            private System.Func<float, float> realFunction;

            public Function1 (string name, string desc, System.Func<float, float> realFunction) : base(name, desc, 1) {
                this.realFunction = realFunction;
            }

            public override float Execute (float[] inputParameters) {
                CheckParameterCount(inputParameters, paramNumber);
                return realFunction(inputParameters[0]);
            }
        }

        public class Function2 : Function {

            private System.Func<float, float, float> realFunction;

            public Function2 (string name, string desc, System.Func<float, float, float> realFunction) : base(name, desc, 2) {
                this.realFunction = realFunction;
            }

            public override float Execute (float[] inputParameters) {
                CheckParameterCount(inputParameters, paramNumber);
                return realFunction(inputParameters[0], inputParameters[1]);
            }
        }

    }
}

