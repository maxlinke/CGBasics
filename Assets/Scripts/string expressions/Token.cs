using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StringExpressions {

    public class Token {
        
        public readonly TokenType type;
        public readonly string stringValue;

        public Token (TokenType type, string stringValue) {
            this.type = type;
            this.stringValue = stringValue;
        }
    
    }

}
