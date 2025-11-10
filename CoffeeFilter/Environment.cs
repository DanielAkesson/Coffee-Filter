using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeFilter
{
    public enum Type
    {
        Int,
        Bool,
        Func,
        String,
        List,
        Object,
        Null,
    }
    public class Variable
    {
        public Token Name;
        public Type Type;
        private object Value;

        public Variable(object value, Type type)
        {
            Name = new Token(TokenEnum.NULL, "", -1);
            Value = value;
            Type = type;
        }
        public Variable(Token name, object value, Type type)
        {
            Name = name;
            Value = value;
            Type = type;
        }
        public object GetValue()
        {
            return Value;
        }
        public void SetValue(object value)
        {
            Value = value;
        }
        public virtual Variable DotAccess(Token operationToken, Token index)
        {
            throw new RuntimeError(operationToken, $"{Type.ToString()} does not support . access");
        }
        public virtual Variable SquareAccess(Token operationToken, Variable index)
        {
            throw new RuntimeError(operationToken, $"{Type.ToString()} does not support [] access");
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Variable))
                return false;
            return Value.Equals((obj as Variable).Value);
        }
    }
    public class IntVariable : Variable
    {
        public IntVariable(int value) : base(value, Type.Int) { }
        public IntVariable(Token name, int value) : base(name, value, Type.Int) { }
    }
    public class BoolVariable : Variable
    {
        public BoolVariable(bool value) : base(value, Type.Bool) { }
        public BoolVariable(Token name, bool value) : base(name, value, Type.Bool) { }
    }
    public class StringVariable : Variable
    {
        public StringVariable(string value) : base(value, Type.String) { }
        public StringVariable(Token name, string value) : base(name, value, Type.String) { }
        public override Variable DotAccess(Token operationToken, Token index)
        {
            switch (index.lexeme)
            {
                case "size":
                    return new IntVariable(operationToken, (GetValue() as string).Length);
            }
            throw new RuntimeError(operationToken, $"Undefined variable {index.lexeme} in {Name.lexeme}");
        }
    }
    public class FuncVariable : Variable
    {
        public FuncVariable(Callable value) : base(value, Type.Func) { }
        public FuncVariable(Token name, Callable value) : base(name, value, Type.Func) { }
    }
    public class ListVariable : Variable
    {
        public ListVariable(List<Variable> value) : base(value, Type.List) { }
        public ListVariable(Token name, List<Variable> value) : base(name, value, Type.List) { }
        public override Variable DotAccess(Token operationToken, Token index)
        {
            //List
            switch (index.lexeme)
            {
                case "size":
                    return new IntVariable((GetValue() as List<Variable>).Count);
                case "add":
                    return new FuncVariable(new ListAdd(GetValue() as List<Variable>));
                case "add_range":
                    return new FuncVariable(new ListAddRange(GetValue() as List<Variable>));
                case "insert":
                    return new FuncVariable(new ListInsert(GetValue() as List<Variable>));
                case "remove":
                    return new FuncVariable(new ListRemove(GetValue() as List<Variable>));

            }
            throw new RuntimeError(index, $"Undefined variable {index.lexeme} in {Name.lexeme}");
        }
        public override Variable SquareAccess(Token operationToken, Variable index)
        {
            if (!(index is IntVariable))
                throw new RuntimeError(operationToken, $"Unable to index {Name.lexeme}, {index.GetValue()} need to be of integer type");
            if ((int)index.GetValue() >= (GetValue() as List<Variable>).Count || (int)index.GetValue() < 0)
                throw new RuntimeError(operationToken, $"Index out of bounds {Name.lexeme} index {(int)index.GetValue()}");
            return (GetValue() as List<Variable>)[(int)index.GetValue()];
            throw new RuntimeError(operationToken, $"Undefined variable {index.GetValue()} in {Name.lexeme}");
        }
        class ListAdd : Callable
        {
            public ListAdd(List<Variable> me)
            {
                this.me = me;
            }
            private List<Variable> me = null;
            public int Arity()
            {
                return 1;
            }
            public Variable Call(Interpreter interpreter, List<Variable> arguments)
            {
                me.Add(arguments[0]);
                return null;
            }
            public override string ToString() { return "<native fn>"; }
        }
        class ListAddRange : Callable
        {
            public ListAddRange(List<Variable> me)
            {
                this.me = me;
            }
            private List<Variable> me = null;
            public int Arity()
            {
                return 1;
            }
            public Variable Call(Interpreter interpreter, List<Variable> arguments)
            {
                me.AddRange(arguments[0].GetValue() as List<Variable>);
                return null;
            }
            public override string ToString() { return "<native fn>"; }
        }
        class ListInsert : Callable
        {
            public ListInsert(List<Variable> me)
            {
                this.me = me;
            }
            private List<Variable> me = null;
            public int Arity()
            {
                return 2;
            }
            public Variable Call(Interpreter interpreter, List<Variable> arguments)
            {
                me.Insert((int)arguments[0].GetValue(), arguments[1]);
                return null;
            }
            public override string ToString() { return "<native fn>"; }
        }
        class ListRemove : Callable
        {
            public ListRemove(List<Variable> me)
            {
                this.me = me;
            }
            private List<Variable> me = null;
            public int Arity()
            {
                return 1;
            }
            public Variable Call(Interpreter interpreter, List<Variable> arguments)
            {
                me.Remove(arguments[0]);
                return null;
            }
            public override string ToString() { return "<native fn>"; }
        }
    }
    public class ObjectVariable : Variable
    {
        public ObjectVariable(Dictionary<string, Variable> value) : base(value, Type.Object) { }
        public ObjectVariable(Token name, Dictionary<string, Variable> value) : base(name, value, Type.Object) { }
        public override Variable DotAccess(Token operationToken, Token index)
        {
            if (GetValue() is Dictionary<string, Variable>)
            {
                if ((GetValue() as Dictionary<string, Variable>).ContainsKey(index.lexeme))
                    return (GetValue() as Dictionary<string, Variable>)[index.lexeme];
            }
            throw new RuntimeError(operationToken, $"Undefined variable {index.lexeme} in {Name.lexeme}");
        }
        public override Variable SquareAccess(Token operationToken, Variable index)
        {
            if (!(index is IntVariable))
                throw new RuntimeError(operationToken, $"Unable to index {Name.lexeme}, {index.GetValue()} need to be of integer type");
            if ((int)index.GetValue() >= (GetValue() as List<Variable>).Count || (int)index.GetValue() < 0)
                throw new RuntimeError(operationToken, $"Index out of bounds {Name.lexeme} index {(int)index.GetValue()}");
            return (GetValue() as List<Variable>)[(int)index.GetValue()];
            throw new RuntimeError(operationToken, $"Undefined variable {index.GetValue()} in {Name.lexeme}");
        }
    }
    public class NullVariable : Variable
    {
        public NullVariable() : base(null, Type.Null) { }
        public NullVariable(Token name) : base(name, null, Type.Null) { }
    }
    public class Environment
    {
        private Environment enclosing;
        private Dictionary<string, Variable> values = new Dictionary<string, Variable>();
        public Environment()
        {
            enclosing = null;
        }
        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }
        public void Define(string name, Variable value)
        {
            Define(new Token(TokenEnum.NULL, name, -1), value);
        }
        public void Define(Token token, Variable value)
        {
            values.Add(token.lexeme, value);
        }
        public Variable Get(Token name)
        {
            if (values.ContainsKey(name.lexeme))
                return values[name.lexeme];
            if (enclosing != null)
                return enclosing.Get(name);
            throw new RuntimeError(name, $"Undefined variable to retrieve {name.lexeme}.");
        }
    }
}