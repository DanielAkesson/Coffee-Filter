using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace CoffeeFilter
{
    public class Tokenizer
    {
        List<TokenDefinition> _tokenDefinitions = new List<TokenDefinition>();
        int currentLine = 1;
        public Tokenizer()
        {
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.COMMENT, "^//[^\n]*\n?"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.COMMENT, "^(/\\*)[\\s\\S]*?(\\*/)"));

            //keywords
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.AND, "^and\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.OR, "^or\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.IF, "^if\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.ELSE, "^else\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.TRUE, "^true\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.FALSE, "^false\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.FUN, "^fun\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.RETURN, "^return\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.FOR, "^for\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.WHILE, "^while\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.NULL, "^null\\b"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.VAR, "^var\\b"));

            //TWO CHAR TOKENS
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.BANG_EQUAL, "^!="));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.EQUAL_EQUAL, "^=="));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.GREATER_EQUAL, "^>="));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.LESSER_EQUAL, "^<="));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.PLUS_EQUAL, "^\\+="));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.MINUS_EQUAL, "^-="));
            //SINGLE CHAR TOKENS
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.EQUAL, "^="));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.GREATER, "^>"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.LESSER, "^<"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.LEFT_PAREN, "^\\("));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.RIGHT_PAREN, "^\\)"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.LEFT_SQUARE, "^\\["));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.RIGHT_SQUARE, "^\\]"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.LEFT_BRACE, "^\\{"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.RIGHT_BRACE, "^\\}"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.COMMA, "^,"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.DOT, "^\\."));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.MINUS, "^-"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.PLUS, "^\\+"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.PERCENT, "^%"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.SEMICOLON, "^;"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.SLASH, "^/"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.STAR, "^\\*"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.BANG, "^!"));

            //Literals
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.IDENTIFIER, "^[a-zA-Z_][\\w]*"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.NUMBER, "^[0-9][0-9]*"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.STRING, "^\"[^\"]*\""));

            //whitespace
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.WHITE_SPACE, "^\\s+"));
            _tokenDefinitions.Add(new TokenDefinition(TokenEnum.INVALID, "(^\\S+\\s)|^\\S+"));
        }
        public List<Token> Tokenize(string lqlText)
        {
            var tokens = new List<Token>();
            string remainingText = lqlText;
            while (!string.IsNullOrWhiteSpace(remainingText))
            {
                var match = FindMatch(remainingText);
                if (match.IsMatch)
                {
                    tokens.Add(new Token(match.Token, match.Value, currentLine));
                    remainingText = match.RemainingText;
                    currentLine += match.Value.Count(x => x.ToString() == "\n");
                }
            }
            tokens.Add(new Token(TokenEnum.EOF, string.Empty, currentLine));
            return tokens;
        }
        private TokenMatch FindMatch(string expression)
        {
            foreach (var tokenDefinition in _tokenDefinitions)
            {
                var match = tokenDefinition.Match(expression);
                if (match.IsMatch)
                    return match;
            }

            return new TokenMatch() { IsMatch = false };
        }
    }
    //Tokenizer
    public enum TokenEnum
    {
        LEFT_PAREN, RIGHT_PAREN, LEFT_SQUARE, RIGHT_SQUARE, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, PERCENT, SEMICOLON, SLASH, STAR, BANG,

        BANG_EQUAL, EQUAL, EQUAL_EQUAL, PLUS_EQUAL, MINUS_EQUAL,
        GREATER, GREATER_EQUAL, LESSER, LESSER_EQUAL,

        IDENTIFIER, STRING, NUMBER,

        INVALID, WHITE_SPACE, COMMENT,

        //keywords
        AND, OR, IF, ELSE, TRUE, FALSE, FUN, RETURN,
        FOR, WHILE, NULL, VAR, EOF
    }
    public class TokenDefinition
    {
        private Regex _regex;
        private readonly TokenEnum _returnsToken;
        public TokenDefinition(TokenEnum returnsToken, string regexPattern)
        {
            _regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            _returnsToken = returnsToken;
        }
        public TokenMatch Match(string inputString)
        {
            var match = _regex.Match(inputString);
            if (match.Success)
            {
                string remainingText = string.Empty;
                if (match.Length != inputString.Length)
                    remainingText = inputString.Substring(match.Length);

                return new TokenMatch()
                {
                    IsMatch = true,
                    RemainingText = remainingText,
                    Token = _returnsToken,
                    Value = match.Value
                };
            }
            else
            {
                return new TokenMatch() { IsMatch = false };
            }

        }
    }
    public class TokenMatch
    {
        public bool IsMatch { get; set; }
        public TokenEnum Token { get; set; }
        public string Value { get; set; }
        public string RemainingText { get; set; }
    }
    public class Token
    {
        public Token(TokenEnum token, string lexeme, int line)
        {
            this.token = token;
            this.lexeme = lexeme;
            this.line = line;
            if (token == TokenEnum.NUMBER)
                Literal = int.Parse(lexeme);
            if (token == TokenEnum.STRING)
                Literal = lexeme.Replace("\"", "");
        }
        public TokenEnum token;
        public string lexeme;
        public int line;
        public object Literal;
    }
}