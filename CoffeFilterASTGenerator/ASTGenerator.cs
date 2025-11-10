using System.Collections.Generic;
using System.IO;
using System.Text;

public class ASTGenerator
{
    const string GRAMMAR_PATH = "../../../../CoffeeFilter/Grammar/";
    public void CreateAST()
    {
        string[] expresion = {
            "Assign         : Expression<T> variable, Expression<T> value",
            "Binary         : Expression<T> left, Token operationToken, Expression<T> right",
            "Access         : Expression<T> left, Token operationToken, Token accessToken",
            "FunctionCall   : Expression<T> callee, Token parenToken, List<Expression<T>> arguments",
            "Grouping       : Expression<T> expression",
            "Literal        : Variable value",
            "ListLiteral    : List<Expression<T>> elements",
            "Unary          : Token operationToken, Expression<T> right",
            "Variable       : Token nameToken",
            "Object         : List<Statement<T>> declarations"
        };
        defineAST(GRAMMAR_PATH, "Expression", new List<string>(expresion));
        string[] statement = {
            "Block                  : List<Statement<T>> statements",
            "Expression             : Expression<T> expression",
            "FunctionDeclaration    : Token nameToken, List<Token> parameters, List<Statement<T>> body",
            "If                     : Expression<T> condition, Statement<T> thenBranch," + " Statement<T> elseBranch",
            "Return                 : Token keywordToken, Expression<T> value",
            "VariableDeclaration    : Token nameToken, Expression<T> initializer",
            "While                  : Expression<T> condition, Statement<T> body",
        };
        defineAST(GRAMMAR_PATH, "Statement", new List<string>(statement));
    }
    private void defineAST(string out_dir, string baseName, List<string> types)
    {
        string path = out_dir + "/" + baseName + ".cs";
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("namespace CoffeeFilter {");
        sb.AppendLine("\tpublic abstract class " + baseName + "<T> " + " {");
        sb.AppendLine("\t\tpublic abstract T Accept(" + baseName + "Visitor<T> visitor);");
        sb.AppendLine("\t}");

        defineVisitor(sb, baseName, types);
        foreach (string type in types)
        {
            string className = type.Split(':')[0].Trim();
            string fields = type.Split(':')[1].Trim();
            defineType(sb, baseName, className, fields);
        }
        sb.AppendLine("}");//close namespace
        File.WriteAllText(path, sb.ToString());
    }
    private void defineVisitor(StringBuilder sb, string baseName, List<string> types)
    {
        sb.AppendLine("\tpublic interface " + baseName + "Visitor<T> {");
        foreach (string type in types)
        {
            string typeName = type.Split(':')[0].Trim();
            sb.AppendLine("\t\tT Visit" + typeName + baseName + "(" + typeName + baseName + "<T> " + baseName.ToLower() + ");");
        }
        sb.AppendLine("\t}");
    }
    private void defineType(StringBuilder sb, string baseName, string className, string fields)
    {
        //definition
        sb.AppendLine("\tpublic class " + className + baseName + "<T>" + " : " + baseName + "<T> {");
        
        //constructor
        sb.AppendLine($"\t\tpublic {className + baseName}({fields})" + " {");

        //store params in fields
        fields = fields.Replace(", ", ",");
        string[] fieldList = fields.Split(',');
        foreach (string field in fieldList)
        {
            string name = field.Split(' ')[1].Trim();
            sb.AppendLine("\t\t\tthis." + name + " = " + name + ";");
        }
        sb.AppendLine("\t\t}");

        //Fields
        foreach (string field in fieldList)
        {
            sb.AppendLine("\t\tpublic " + field + ";");
        }

        //Implement interface
        sb.AppendLine("\t\tpublic override T Accept(" + baseName + "Visitor<T> visitor) {");
        sb.AppendLine("\t\t\treturn visitor.Visit" + className + baseName + "(this);");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
    }
}
