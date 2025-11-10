using System;
using System.Collections.Generic;

namespace CoffeeFilter
{
    /*
    program                         → line* EOF
    line                            → declaration | statement
    declaration                     → variable_declaration_statement | function_declaration_statement
    variable_declaration_statement  → "var" IDENTIFIER("=" expression)?";"
    function_declaration_statement  → "fun" function
        function                        → IDENTIFIER "("parameters?")" block_statement
        parameters                      → IDENTIFIER ( "," IDENTIFIER )*
    statement                       → expression_statement | for_statement | if_statement | while_statement | block_statement | return_statement
    for_statement                   → "for" "(" ( variable_declaration_statement | expression_statement | ";" ) expression? ";" expression? ")" statement
    while_statement                 → "while" "(" expression ")" statement
    if_statement                    → "if" "(" expression ")" statement ( "else" statement )?
    block_statement                 → "{" line* "}"
    expression_statement            → expression ";"
    return_statement                → "return" expression?";"

    expression              → assignment
    assignment              → (IDENTIFIER "=" assignment) | logic_or
    logic_or                → logic_and ("or" logic_and)*
    logic_and               → equality ("and" equality)*
    equality                → comparison ( ( "!=" | "==" ) comparison )*
    comparison              → term ( ( ">" | ">=" | "<" | "<=" ) term )*
    term                    → factor ( ( "-" | "+" ) factor )*
    factor                  → unary ( ( "/" | "*" ) unary )*
    unary                   → ( "!" | "-" ) unary | function_call
    function_call           → object_expression | ("(" arguments?")")*
        arguments               → expression ("," expression)*
    object_expression       → "{"declaration*"}" | primary
    primary                 → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER | "[" arguments? "]"
    */
    class Parser<T>
    {
        private class ParseError : Exception { }
        private List<Token> tokens = new List<Token>();
        private int current = 0;
        public Parser(List<Token> tokens)
        {
            //remove all comments from execution
            tokens.RemoveAll((x) => x.token == TokenEnum.COMMENT);
            tokens.RemoveAll((x) => x.token == TokenEnum.WHITE_SPACE);
            tokens.RemoveAll((x) => x.token == TokenEnum.INVALID);
            this.tokens = tokens;
        }

        //Statements
        public List<Statement<T>> Parse()
        {
            List<Statement<T>> statements = new List<Statement<T>>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }
            return statements;
        }
        private Statement<T> Line()
        {
            try
            {
                if (Check(TokenEnum.VAR) || Check(TokenEnum.FUN)) return Declaration();
                return Statement();
            }
            catch (ParseError error)
            {
                synchronize();
                return null;
            }
        }
        private Statement<T> Declaration()
        {
            if (Match(TokenEnum.VAR)) return VariableDeclarationStatement();
            if (Match(TokenEnum.FUN)) return FunctionDeclarationStatement("function");
            return Statement();
        }
        private Statement<T> VariableDeclarationStatement()
        {
            Token name = Consume(TokenEnum.IDENTIFIER, "Expect variable name.");
            Expression<T> initializer = null;
            if (Match(TokenEnum.EQUAL))
            {
                initializer = Expression();
            }
            Consume(TokenEnum.SEMICOLON, "Expect ';' after variable declaration.");
            return new VariableDeclarationStatement<T>(name, initializer);
        }
        private FunctionDeclarationStatement<T> FunctionDeclarationStatement(string kind)
        {
            Token name = Consume(TokenEnum.IDENTIFIER, "Expect " + kind + " name.");
            Consume(TokenEnum.LEFT_PAREN, "Expect '(' after " + kind + " name.");
            List<Token> parameters = new List<Token>();
            if (!Check(TokenEnum.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                        Error(Peek(), "Can't have more than 255 parameters.");
                    parameters.Add(Consume(TokenEnum.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenEnum.COMMA));
            }
            Consume(TokenEnum.RIGHT_PAREN, "Expect ')' after parameters.");
            Consume(TokenEnum.LEFT_BRACE, "Expect '{' before " + kind + " body.");
            List<Statement<T>> body = BlockStatement();
            return new FunctionDeclarationStatement<T>(name, parameters, body);
        }
        private Statement<T> Statement()
        {
            if (Match(TokenEnum.LEFT_BRACE)) return new BlockStatement<T>(BlockStatement());
            if (Match(TokenEnum.IF)) return If();
            if (Match(TokenEnum.WHILE)) return WhileStatement();
            if (Match(TokenEnum.FOR)) return ForStatement();
            if (Match(TokenEnum.RETURN)) return ReturnStatement();
            return ExpressionStatement();
        }
        private Statement<T> ForStatement()
        {
            Consume(TokenEnum.LEFT_PAREN, "Expect '(' after 'for'");
            Statement<T> initializer;
            if (Match(TokenEnum.SEMICOLON))
                initializer = null;
            else if (Match(TokenEnum.VAR))
                initializer = VariableDeclarationStatement();
            else
                initializer = ExpressionStatement();

            Expression<T> condition = null;
            if (!Check(TokenEnum.SEMICOLON))
                condition = Expression();
            Consume(TokenEnum.SEMICOLON, "Expect ';' after loop condition");

            Expression<T> increment = null;
            if (!Check(TokenEnum.RIGHT_PAREN))
                increment = Expression();
            Consume(TokenEnum.RIGHT_PAREN, "Expect ')' after for clauses");

            Statement<T> body = Statement();
            if (increment != null)
            {
                //Add the increment after the body
                List<Statement<T>> statements = new List<Statement<T>>();
                statements.Add(body);
                statements.Add(new ExpressionStatement<T>(increment));
                body = new BlockStatement<T>(statements);
            }

            if (condition == null)
                condition = new LiteralExpression<T>(new BoolVariable(Previous(), true));
            body = new WhileStatement<T>(condition, body);

            if (initializer != null)
            {
                //If we have initializer run that before the body
                List<Statement<T>> ini = new List<Statement<T>>();
                ini.Add(initializer);
                ini.Add(body);
                body = new BlockStatement<T>(ini);
            }
            return body;
        }
        private Statement<T> WhileStatement()
        {
            Consume(TokenEnum.LEFT_PAREN, "Expect '(' after 'while'.");
            Expression<T> condition = Expression();
            Consume(TokenEnum.RIGHT_PAREN, "Expect ')' after while condition.");
            Statement<T> body = Statement();
            return new WhileStatement<T>(condition, body);
        }
        private Statement<T> If()
        {
            Consume(TokenEnum.LEFT_PAREN, "Expect '(' after 'if'.");
            Expression<T> condition = Expression();
            Consume(TokenEnum.RIGHT_PAREN, "Expect ')' after if condition.");
            Statement<T> thenBranch = Statement();
            Statement<T> elseBranch = null;
            if (Match(TokenEnum.ELSE))
            {
                elseBranch = Statement();
            }
            return new IfStatement<T>(condition, thenBranch, elseBranch);
        }
        private List<Statement<T>> BlockStatement()
        {
            List<Statement<T>> statements = new List<Statement<T>>();
            while (!Check(TokenEnum.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Line());
            }
            Consume(TokenEnum.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }
        private Statement<T> ExpressionStatement()
        {
            Expression<T> expression = Expression();
            Consume(TokenEnum.SEMICOLON, "Expect ';' after expression.");
            return new ExpressionStatement<T>(expression);
        }
        private Statement<T> ReturnStatement()
        {
            Token keyword = Previous();
            Expression<T> value = null;
            if (!Check(TokenEnum.SEMICOLON))
                value = Expression();

            Consume(TokenEnum.SEMICOLON, "Expect ';' after return value.");
            return new ReturnStatement<T>(keyword, value);
        }

        //Expressions
        private Expression<T> Expression()
        {
            return Assignment();
        }
        private Expression<T> Assignment()
        {
            Expression<T> expression = LogicOr();
            if (Match(TokenEnum.EQUAL))
            {
                Token equals = Previous();
                Expression<T> value = Assignment();

                if (expression is VariableExpression<T> || expression is AccessExpression<T>)
                {
                    return new AssignExpression<T>(expression, value);
                }
                Error(equals, "Invalid assignment target");
            }
            return expression;
        }
        private Expression<T> LogicOr()
        {
            Expression<T> left = LogicAnd();
            while (Match(TokenEnum.OR))
            {
                Token operatorToken = Previous();
                Expression<T> right = LogicAnd();
                left = new BinaryExpression<T>(left, operatorToken, right);
            }
            return left;
        }
        private Expression<T> LogicAnd()
        {
            Expression<T> left = Equality();
            while (Match(TokenEnum.AND))
            {
                Token operatorToken = Previous();
                Expression<T> right = Equality();
                left = new BinaryExpression<T>(left, operatorToken, right);
            }
            return left;
        }
        private Expression<T> Equality()
        {
            Expression<T> expression = Comparison();
            while (Match(TokenEnum.BANG_EQUAL, TokenEnum.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expression<T> right = Comparison();
                expression = new BinaryExpression<T>(expression, op, right);
            }
            return expression;
        }
        private Expression<T> Comparison()
        {
            Expression<T> left = Term();
            while (Match(TokenEnum.GREATER, TokenEnum.GREATER_EQUAL, TokenEnum.LESSER, TokenEnum.LESSER_EQUAL))
            {
                Token op = Previous();
                Expression<T> right = Term();
                left = new BinaryExpression<T>(left, op, right);
            }
            return left;
        }
        private Expression<T> Term()
        {
            Expression<T> left = Factor();
            while (Match(TokenEnum.MINUS, TokenEnum.PLUS))
            {
                Token op = Previous();
                Expression<T> right = Factor();
                left = new BinaryExpression<T>(left, op, right);
            }
            return left;
        }
        private Expression<T> Factor()
        {
            Expression<T> left = Unary();
            while (Match(TokenEnum.SLASH, TokenEnum.STAR, TokenEnum.PERCENT))
            {
                Token op = Previous();
                Expression<T> right = Unary();
                left = new BinaryExpression<T>(left, op, right);
            }
            return left;
        }
        private Expression<T> Unary()
        {
            if (Match(TokenEnum.BANG, TokenEnum.MINUS))
            {
                Token op = Previous();
                Expression<T> right = FunctionCall();
                return new UnaryExpression<T>(op, right);
            }
            return FunctionCall();
        }
        //Dot access and call should perhaps be the same thing? and squareAccess
        private Expression<T> FunctionCall()
        {
            Expression<T> expression = ObjectExpression();
            while (Match(TokenEnum.LEFT_PAREN, TokenEnum.LEFT_SQUARE, TokenEnum.DOT))
            {
                Token op = Previous();
                if (op.token == TokenEnum.LEFT_PAREN)
                    expression = FinishCall(expression);
                if (op.token == TokenEnum.LEFT_SQUARE)
                {
                    Expression<T> right = Expression();
                    expression = new BinaryExpression<T>(expression, op, right);
                    Consume(TokenEnum.RIGHT_SQUARE, "Expect ']' after List access");
                }
                else if (op.token == TokenEnum.DOT)
                {
                    Token accessToken = Advance();
                    expression = new AccessExpression<T>(expression, op, accessToken);
                }
                else
                    break;
            }
            return expression;
        }
        private Expression<T> FinishCall(Expression<T> callee)
        {
            List<Expression<T>> arguments = new List<Expression<T>>();
            if (!Check(TokenEnum.RIGHT_PAREN))
            {
                do arguments.Add(Expression()); while (Match(TokenEnum.COMMA));
            }
            Token paren = Consume(TokenEnum.RIGHT_PAREN, "Expect ')' after arguments.");
            return new FunctionCallExpression<T>(callee, paren, arguments);
        }
        private Expression<T> ObjectExpression()
        {
            if (Match(TokenEnum.LEFT_BRACE))
            {
                List<Statement<T>> declarations = new List<Statement<T>>();
                while (!Check(TokenEnum.RIGHT_BRACE) && !IsAtEnd())
                {
                    declarations.Add(Declaration());
                }
                Consume(TokenEnum.RIGHT_BRACE, "Expect '}' after Object");
                return new ObjectExpression<T>(declarations);
            }
            return Primary();
        }
        private Expression<T> Primary()
        {
            if (Match(TokenEnum.TRUE))
                return new LiteralExpression<T>(new BoolVariable(Previous(), true));
            if (Match(TokenEnum.FALSE))
                return new LiteralExpression<T>(new BoolVariable(Previous(), false));
            if (Match(TokenEnum.NULL))
                return new LiteralExpression<T>(new NullVariable(Previous()));
            if (Match(TokenEnum.STRING))
                return new LiteralExpression<T>(new StringVariable(Previous(), (string)Previous().Literal));
            if (Match(TokenEnum.NUMBER))
                return new LiteralExpression<T>(new IntVariable(Previous(), (int)Previous().Literal));
            if (Match(TokenEnum.IDENTIFIER))
                return new VariableExpression<T>(Previous());
            if (Match(TokenEnum.LEFT_PAREN))
            {
                Expression<T> right = Expression();
                Consume(TokenEnum.RIGHT_PAREN, "Expected ')' after expression");
                return new GroupingExpression<T>(right);
            }
            if (Match(TokenEnum.LEFT_SQUARE))
            {
                List<Expression<T>> elements = new List<Expression<T>>();
                if (Check(TokenEnum.RIGHT_SQUARE))
                    return new ListLiteralExpression<T>(elements);

                do elements.Add(Expression()); while (Match(TokenEnum.COMMA));
                Consume(TokenEnum.RIGHT_SQUARE, "Expect ']' after List.");
                return new ListLiteralExpression<T>(elements);
            }
            throw Error(Peek(), "Expect expression.");
        }

        //utility
        private bool Match(params TokenEnum[] tokenEnums)
        {
            foreach (TokenEnum tokenEnum in tokenEnums)
            {
                if (Check(tokenEnum))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }
        private bool Check(TokenEnum tokenEnum)
        {
            if (IsAtEnd()) return false;
            return Peek().token == tokenEnum;
        }
        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }
        private bool IsAtEnd()
        {
            return Peek().token == TokenEnum.EOF;
        }
        private Token Peek()
        {
            return tokens[current];
        }
        private Token Previous()
        {
            return tokens[current - 1];
        }
        private Token Consume(TokenEnum tokenEnum, string errorMessage)
        {
            if (Check(tokenEnum)) return Advance();
            throw Error(Peek(), errorMessage);
        }

        //Error handling
        private ParseError Error(Token token, string message)
        {
            CoffeeFilter.Error(token, message);
            return new ParseError();
        }
        private void synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Previous().token == TokenEnum.SEMICOLON) return;
                switch (Peek().token)
                {
                    case TokenEnum.FUN:
                    case TokenEnum.VAR:
                    case TokenEnum.FOR:
                    case TokenEnum.IF:
                    case TokenEnum.WHILE:
                    case TokenEnum.RETURN:
                        return;
                }
                Advance();
            }
        }
    }
}
