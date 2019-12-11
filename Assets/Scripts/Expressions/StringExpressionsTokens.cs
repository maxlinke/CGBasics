public static partial class StringExpressions {

    private abstract class Token { 

        public abstract bool Equals (char ch);

        public abstract string GetPrintString ();

    }

    private class NumberToken : Token {

        public readonly float value;

        public NumberToken (float value) {
            this.value = value;
        }

        public override bool Equals (char ch) {
            return false;
        }

        public override string GetPrintString () {
            return value.ToString();
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

        public override string GetPrintString () {
            return value.ToString();
        }
    }
	
}
