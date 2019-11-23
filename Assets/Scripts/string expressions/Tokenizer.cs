using System.Collections.Generic;

namespace StringExpressions {
    
    public static class Tokenizer {

        public static bool TryTokenize (string input, out Token[] tokens) {
            // split around spaces, operators, commas (NOT DOTS!!!) but accept float values (1e-3) (or dont... 1^-3...)
            if(input == null){
                tokens = null;
                return false;
            }
            List<Token> tokenList = new List<Token>();
            string[] whiteSpaceSplit = input.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

            bool unfinishedToken = false;
            string  tokenString = string.Empty;
            TokenType currentType = TokenType.NUMBER;   // just a default init. value doesn't matter...
            foreach(var possibleToken in whiteSpaceSplit){
                // read character by character
                // current token type (plus "none", when we are definitely finished)
                foreach(char c in possibleToken.ToCharArray()){




                }
            }            

            tokens = null;
            return false;
            
            bool IsNumber (char c) {
                switch(c){
                    case '0': return true;
                    case '1': return true;
                    case '2': return true;
                    case '3': return true;
                    case '4': return true;
                    case '5': return true;
                    case '6': return true;
                    case '7': return true;
                    case '8': return true;
                    case '9': return true;
                    case '.': return true;  // for "0.1", ".1", "4." and more...
                    default: return false;
                }
            }

        }

    }
	
}
