public static partial class StringExpressions {

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
