namespace StringExpressions {

    public class ExpressionHandler {

        public bool TryEvaluateAsNumber (string expression, out float number) {
            bool tokenized = Tokenizer.TryTokenize(expression, out Token[] tokens);
            if(!tokenized){
                number = float.NaN;
                return false;
            }
            number = float.NaN;
            return false;
            // have infix tokens, convert to postfix first...   
        }
        
    }
	
}
