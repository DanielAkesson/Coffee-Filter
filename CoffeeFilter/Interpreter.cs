using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeFilter
{
    class RuntimeError : Exception
    {
        public Token token;
        public RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
    class Return : Exception
    {
        public Variable value;
        public Return(Variable value) : base(null)
        {
            this.value = value;
        }
    }
    public interface Callable
    {
        int Arity();
        Variable Call(Interpreter interpreter, List<Variable> arguments);
    }
    public class Function : Callable
    {
        private FunctionDeclarationStatement<Variable> declaration;
        private Environment closure;
        public Function(FunctionDeclarationStatement<Variable> declaration, Environment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }
        public int Arity()
        {
            return declaration.parameters.Count;
        }
        public Variable Call(Interpreter interpreter, List<Variable> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration.parameters.Count; i++)
            {
                environment.Define(declaration.parameters[i], arguments[i]);
            }
            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.value;
            }
            return null;
        }
        public override string ToString()
        {
            return "<fn " + declaration.nameToken.lexeme + ">";
        }
    }
    //standard function
    class Clock : Callable
    {
        public int Arity()
        {
            return 0;
        }
        public Variable Call(Interpreter interpreter, List<Variable> arguments)
        {
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return new IntVariable((int)milliseconds);
        }
        public override string ToString() { return "<native fn>"; }
    }
    class Print : Callable
    {
        public int Arity()
        {
            return 1;
        }
        public Variable Call(Interpreter interpreter, List<Variable> arguments)
        {
            Console.WriteLine(arguments[0].GetValue().ToString());
            return null;
        }
        public override string ToString() { return "<native fn>"; }
    }
    public class Interpreter : ExpressionVisitor<Variable>, StatementVisitor<Variable>
    {
        public Environment globals = new Environment();
        private Environment environment;
        public Interpreter()
        {
            environment = globals;
            globals.Define("clock", new FuncVariable(new Clock()));
            globals.Define("print", new FuncVariable(new Print()));
        }

        //ExpressionVisitor
        public void Interpret(List<Statement<Variable>> statements)
        {
            try
            {
                foreach (Statement<Variable> statement in statements)
                    Execute(statement);
            }
            catch (RuntimeError error)
            {
                CoffeeFilter.RuntimeError(error);
            }
        }
        public Variable VisitBinaryExpression(BinaryExpression<Variable> expression)
        {
            Variable left = Evaluate(expression.left);
            Variable right = Evaluate(expression.right);

            switch (expression.operationToken.token)
            {
                case TokenEnum.MINUS:
                    CheckOperandIsNumber(expression.operationToken, left.GetValue(), right.GetValue());
                    return new IntVariable(expression.operationToken, (int)left.GetValue() - (int)right.GetValue());
                case TokenEnum.SLASH:
                    CheckOperandIsNumber(expression.operationToken, left.GetValue(), right.GetValue());
                    return new IntVariable(expression.operationToken, (int)left.GetValue() / (int)right.GetValue());
                case TokenEnum.STAR:
                    CheckOperandIsNumber(expression.operationToken, left.GetValue(), right.GetValue());
                    return new IntVariable(expression.operationToken, (int)left.GetValue() * (int)right.GetValue());
                case TokenEnum.GREATER:
                    CheckOperandIsNumber(expression.operationToken, left.GetValue(), right.GetValue());
                    return new BoolVariable(expression.operationToken, (int)left.GetValue() > (int)right.GetValue());
                case TokenEnum.GREATER_EQUAL:
                    CheckOperandIsNumber(expression.operationToken, left.GetValue(), right.GetValue());
                    return new BoolVariable(expression.operationToken, (int)left.GetValue() >= (int)right.GetValue());
                case TokenEnum.LESSER:
                    CheckOperandIsNumber(expression.operationToken, left.GetValue(), right.GetValue());
                    return new BoolVariable(expression.operationToken, (int)left.GetValue() < (int)right.GetValue());
                case TokenEnum.LESSER_EQUAL:
                    CheckOperandIsNumber(expression.operationToken, left.GetValue(), right.GetValue());
                    return new BoolVariable(expression.operationToken, (int)left.GetValue() <= (int)right.GetValue());
                case TokenEnum.PERCENT:
                    CheckOperandIsNumber(expression.operationToken, left.GetValue(), right.GetValue());
                    return new IntVariable(expression.operationToken, (int)left.GetValue() % (int)right.GetValue());
                case TokenEnum.BANG_EQUAL:
                    return new BoolVariable(expression.operationToken, !IsEqual(left, right));
                case TokenEnum.EQUAL_EQUAL:
                    return new BoolVariable(expression.operationToken, IsEqual(left, right));
                case TokenEnum.LEFT_SQUARE:
                    return left.SquareAccess(expression.operationToken, right);
                case TokenEnum.OR:
                    //list or
                    if (left is ListVariable && right is ListVariable)
                    {
                        List<Variable> list = new List<Variable>();
                        foreach (Variable leftV in left.GetValue() as List<Variable>)
                            if (!list.Contains(leftV))
                                list.Add(leftV);
                        foreach (Variable rightV in right.GetValue() as List<Variable>)
                            if (!list.Contains(rightV))
                                list.Add(rightV);
                        return new ListVariable(expression.operationToken, list);
                    }
                    if (left is ListVariable)
                    {
                        List<Variable> list = new List<Variable>();
                        foreach (Variable leftV in left.GetValue() as List<Variable>)
                            if (!list.Contains(leftV))
                                list.Add(leftV);
                        if (!list.Contains(right))
                            list.Add(right);
                        return new ListVariable(list);
                    }
                    if (right is ListVariable)
                    {
                        List<Variable> list = new List<Variable>();
                        foreach (Variable rightV in right.GetValue() as List<Variable>)
                            if (!list.Contains(rightV))
                                list.Add(rightV);
                        if (!list.Contains(left))
                            list.Add(left);
                        return new ListVariable(expression.operationToken, list);
                    }
                    return new BoolVariable(expression.operationToken, IsTruthy(left) || IsTruthy(right));
                case TokenEnum.AND:
                    //list and [1,2,3,4] and [1,4] = [1,4]
                    if (left is ListVariable && right is ListVariable)
                    {
                        List<Variable> list = new List<Variable>();
                        foreach (Variable leftV in left.GetValue() as List<Variable>)
                            if (!(right.GetValue() as List<Variable>).Contains(leftV))
                                list.Add(leftV);
                        return new ListVariable(expression.operationToken, list);
                    }
                    if (left is ListVariable)
                    {
                        List<Variable> list = new List<Variable>();
                        if (!(left.GetValue() as List<Variable>).Contains(right))
                            list.Add(right);
                        return new ListVariable(expression.operationToken, list);
                    }
                    if (right is ListVariable)
                    {
                        List<Variable> list = new List<Variable>();
                        if (!(right.GetValue() as List<Variable>).Contains(left))
                            list.Add(left);
                        return new ListVariable(list);
                    }
                    return new BoolVariable(expression.operationToken, IsTruthy(left) && IsTruthy(right));

                case TokenEnum.PLUS: //unique since both strings and numbers can use the + operator
                    //addition
                    if (left is IntVariable && right is IntVariable)
                        return new IntVariable(expression.operationToken, (int)left.GetValue() + (int)right.GetValue());
                    //string concatenate
                    if (left is StringVariable && right.GetValue() is string)
                        return new StringVariable(expression.operationToken, (string)left.GetValue() + (string)right.GetValue());
                    if (left is StringVariable)
                        return new StringVariable(expression.operationToken, (string)left.GetValue() + Stringify(right));
                    if (right is StringVariable)
                        return new StringVariable(expression.operationToken, Stringify(left) + (string)right.GetValue());
                    //list concatenate
                    if (left is ListVariable && right is ListVariable)
                    {
                        List<Variable> list = new List<Variable>();
                        list.AddRange(left.GetValue() as List<Variable>);
                        list.AddRange(right.GetValue() as List<Variable>);
                        return new ListVariable(expression.operationToken, list);
                    }
                    if (left is ListVariable)
                    {
                        List<Variable> list = new List<Variable>();
                        list.AddRange(left.GetValue() as List<Variable>);
                        list.Add(right);
                        return new ListVariable(expression.operationToken, list);
                    }
                    if (right is ListVariable)
                    {
                        List<Variable> list = new List<Variable>();
                        list.Add(left);
                        list.AddRange(right.GetValue() as List<Variable>);
                        return new ListVariable(expression.operationToken, list);
                    }
                    throw new RuntimeError(expression.operationToken, "Operands must be numbers or strings");
            }
            return null;
        }
        public Variable VisitGroupingExpression(GroupingExpression<Variable> expression)
        {
            return Evaluate(expression.expression);
        }
        public Variable VisitLiteralExpression(LiteralExpression<Variable> expression)
        {
            return expression.value;
        }
        public Variable VisitUnaryExpression(UnaryExpression<Variable> expression)
        {
            Variable right = Evaluate(expression.right);

            switch (expression.operationToken.token)
            {
                case TokenEnum.BANG:
                    return new BoolVariable(expression.operationToken, !IsTruthy(right));
                case TokenEnum.MINUS:
                    CheckOperandIsNumber(expression.operationToken, right.GetValue());
                    return new IntVariable(expression.operationToken, -(int)right.GetValue());
            }
            return null;
        }
        public Variable VisitAccessExpression(AccessExpression<Variable> expression)
        {
            if (expression.operationToken.token == TokenEnum.DOT)
                return Evaluate(expression.left).DotAccess(expression.operationToken, expression.accessToken);
            return null;
        }
        public Variable VisitVariableExpression(VariableExpression<Variable> expression)
        {
            return environment.Get(expression.nameToken);
        }
        public Variable VisitAssignExpression(AssignExpression<Variable> expression)
        {
            Variable value = Evaluate(expression.value);
            Variable toAssign = Evaluate(expression.variable);
            toAssign.SetValue(value.GetValue());
            return value;
        }
        public Variable VisitFunctionCallExpression(FunctionCallExpression<Variable> expression)
        {
            Variable callee = Evaluate(expression.callee);
            List<Variable> arguments = new List<Variable>();
            foreach (Expression<Variable> argument in expression.arguments)
                arguments.Add(Evaluate(argument));

            //trying to call a non function like 8() or "hej"()
            if (!(callee.GetValue() is Callable))
                throw new RuntimeError(expression.parenToken, "Can only call functions.");

            Callable function = (Callable)callee.GetValue();

            //does argument amount match declared function?
            if (arguments.Count != function.Arity())
                throw new RuntimeError(expression.parenToken, "Expected " + function.Arity() + " arguments but got " + arguments.Count() + ".");

            //All is in order call the function
            return function.Call(this, arguments);
        }
        public Variable VisitObjectExpression(ObjectExpression<Variable> expression)
        {
            Dictionary<string, Variable> value = new Dictionary<string, Variable>();
            ObjectExpression<Variable> objExpr = (expression as ObjectExpression<Variable>);

            foreach (Statement<Variable> statement in objExpr.declarations)
            {
                if (statement is FunctionDeclarationStatement<Variable>)
                {
                    FunctionDeclarationStatement<Variable> funDec = (statement as FunctionDeclarationStatement<Variable>);
                    Function function = new Function(funDec, environment);
                    value.Add(funDec.nameToken.lexeme, new FuncVariable(funDec.nameToken, function));
                }
                else
                {
                    VariableDeclarationStatement<Variable> varDec = (statement as VariableDeclarationStatement<Variable>);
                    Variable member = null;
                    if (varDec.initializer != null)
                        member = Evaluate(varDec.initializer);
                    value.Add(varDec.nameToken.lexeme, member);
                }
            }
            return new ObjectVariable(new Token(TokenEnum.NULL, "", 0), value);
        }
        public Variable VisitListLiteralExpression(ListLiteralExpression<Variable> expression)
        {
            List<Variable> value = new List<Variable>();
            foreach (Expression<Variable> element in expression.elements)
                value.Add(Evaluate(element));
            return new ListVariable(value);
        }

        //StatementVisitor
        public Variable VisitExpressionStatement(ExpressionStatement<Variable> statement)
        {
            Evaluate(statement.expression);
            return null;
        }
        public Variable VisitVariableDeclarationStatement(VariableDeclarationStatement<Variable> statement)
        {
            Variable value = null;
            if (statement.initializer != null)
                value = Evaluate(statement.initializer);
            value.Name = statement.nameToken;
            environment.Define(statement.nameToken, value);
            return null;
        }
        public Variable VisitBlockStatement(BlockStatement<Variable> statement)
        {
            ExecuteBlock(statement.statements, new Environment(environment));
            return null;
        }
        public Variable VisitIfStatement(IfStatement<Variable> statement)
        {
            if (IsTruthy(Evaluate(statement.condition)))
                Execute(statement.thenBranch);
            else if (statement.elseBranch != null)
                Execute(statement.elseBranch);
            return null;
        }
        public Variable VisitWhileStatement(WhileStatement<Variable> statement)
        {
            while (IsTruthy(Evaluate(statement.condition)))
                Execute(statement.body);
            return null;
        }
        public Variable VisitFunctionDeclarationStatement(FunctionDeclarationStatement<Variable> statement)
        {
            Function function = new Function(statement, environment);
            environment.Define(statement.nameToken.lexeme, new FuncVariable(statement.nameToken, function));
            return null;
        }
        public Variable VisitReturnStatement(ReturnStatement<Variable> statement)
        {
            Variable value = null;
            if (statement.value != null) value = Evaluate(statement.value);
            throw new Return(value);
        }

        //utility
        private Variable Evaluate(Expression<Variable> expression)
        {
            return expression.Accept(this);
        }
        private void Execute(Statement<Variable> statement)
        {
            statement.Accept(this);
        }
        public void ExecuteBlock(List<Statement<Variable>> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;
                foreach (Statement<Variable> statement in statements)
                    Execute(statement);
            }
            finally
            {
                this.environment = previous;
            }
        }
        private bool IsTruthy(Variable obj)
        {
            if (obj.GetValue() == null) return false;
            if (obj.GetValue() is bool) return (bool)obj.GetValue();
            if (obj.GetValue() is int) return (int)obj.GetValue() != 0;
            return true;
        }
        private bool IsEqual(Variable a, Variable b)
        {
            if (a.GetValue() == null && b.GetValue() == null) return true;
            if (a.GetValue() == null) return false;
            return a.GetValue().Equals(b.GetValue());
        }
        private void CheckOperandIsNumber(Token token, params object[] operands)
        {
            foreach (object op in operands)
                if (!(op is int))
                    throw new RuntimeError(token, $"Operand is not a Number it is {op.GetType().Name}");
        }
        private string Stringify(Variable obj)
        {
            if (obj.GetValue() is int || obj.GetValue() is bool)
            {
                return obj.GetValue() + "";
            }
            return obj.ToString();
        }
    }
}
