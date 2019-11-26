using System.Collections.Generic;
using UnityEngine;

public static partial class StringExpressions {

    public static class Functions {

        static Dictionary<string, Function> functions;

        static Functions () {
            functions = new Dictionary<string, Function>();
            AddFunc(new Function0("pi", "Shorthand for 3.1415...", () => Mathf.PI));
            AddFunc(new Function0("e", "Euler's number 2.71828...", () => (float)(System.Math.E)));
            AddFunc(new Function1("sin", $"The sine of angle {IndexToVariableName(0)} in radians", Mathf.Sin));
            AddFunc(new Function1("cos", $"The cosine of angle {IndexToVariableName(0)} in radians", Mathf.Cos));
            AddFunc(new Function1("tan", $"The tangent of angle {IndexToVariableName(0)} in radians", Mathf.Tan));
            AddFunc(new Function1("asin", $"The arc-sine of {IndexToVariableName(0)}, the angle in radians whose sine is {IndexToVariableName(0)}", Mathf.Asin));
            AddFunc(new Function1("acos", $"The arc-cosine of {IndexToVariableName(0)}, the angle in radians whose cosine is {IndexToVariableName(0)}", Mathf.Acos));
            AddFunc(new Function1("atan", $"The arc-tangent of {IndexToVariableName(0)}, the angle in radians whose tangent is {IndexToVariableName(0)}", Mathf.Atan));
            AddFunc(new Function2("atan2", $"The fancier arc-tangent of {IndexToVariableName(0)}/{IndexToVariableName(1)}", Mathf.Atan2));
            AddFunc(new Function1("sqrt", $"The square root of {IndexToVariableName(0)}", Mathf.Sqrt));
            AddFunc(new Function2("pow", $"{IndexToVariableName(0)} raised to power {IndexToVariableName(1)}", Mathf.Pow));
            AddFunc(new Function1("exp", $"e raised to {IndexToVariableName(0)}", Mathf.Exp));
            AddFunc(new Function0("deg2rad", "Degrees to radians conversion multiplier", () => Mathf.Deg2Rad));
            AddFunc(new Function0("rad2deg", "Radians to degrees conversion multiplier", () => Mathf.Rad2Deg));
            AddFunc(new Function1("sindeg", $"The sine of angle {IndexToVariableName(0)} in degrees", (x) => Mathf.Sin(Mathf.Deg2Rad * x)));
            AddFunc(new Function1("cosdeg", $"The cosine of angle {IndexToVariableName(0)} in degrees", (x) => Mathf.Cos(Mathf.Deg2Rad * x)));
            AddFunc(new Function1("tandeg", $"The tangent of angle {IndexToVariableName(0)} in degrees", (x) => Mathf.Tan(Mathf.Deg2Rad * x)));

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

        public static char IndexToVariableName (int varIndex) {
            int output = 'x' + varIndex;
            if(output > 'z'){
                output -= 'z';
            }
            return (char)output;
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
