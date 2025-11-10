using System.Collections.Generic;
namespace CoffeeFilter {
	public abstract class Statement<T>  {
		public abstract T Accept(StatementVisitor<T> visitor);
	}
	public interface StatementVisitor<T> {
		T VisitBlockStatement(BlockStatement<T> statement);
		T VisitExpressionStatement(ExpressionStatement<T> statement);
		T VisitFunctionDeclarationStatement(FunctionDeclarationStatement<T> statement);
		T VisitIfStatement(IfStatement<T> statement);
		T VisitReturnStatement(ReturnStatement<T> statement);
		T VisitVariableDeclarationStatement(VariableDeclarationStatement<T> statement);
		T VisitWhileStatement(WhileStatement<T> statement);
	}
	public class BlockStatement<T> : Statement<T> {
		public BlockStatement(List<Statement<T>> statements) {
			this.statements = statements;
		}
		public List<Statement<T>> statements;
		public override T Accept(StatementVisitor<T> visitor) {
			return visitor.VisitBlockStatement(this);
		}
	}
	public class ExpressionStatement<T> : Statement<T> {
		public ExpressionStatement(Expression<T> expression) {
			this.expression = expression;
		}
		public Expression<T> expression;
		public override T Accept(StatementVisitor<T> visitor) {
			return visitor.VisitExpressionStatement(this);
		}
	}
	public class FunctionDeclarationStatement<T> : Statement<T> {
		public FunctionDeclarationStatement(Token nameToken, List<Token> parameters, List<Statement<T>> body) {
			this.nameToken = nameToken;
			this.parameters = parameters;
			this.body = body;
		}
		public Token nameToken;
		public List<Token> parameters;
		public List<Statement<T>> body;
		public override T Accept(StatementVisitor<T> visitor) {
			return visitor.VisitFunctionDeclarationStatement(this);
		}
	}
	public class IfStatement<T> : Statement<T> {
		public IfStatement(Expression<T> condition, Statement<T> thenBranch, Statement<T> elseBranch) {
			this.condition = condition;
			this.thenBranch = thenBranch;
			this.elseBranch = elseBranch;
		}
		public Expression<T> condition;
		public Statement<T> thenBranch;
		public Statement<T> elseBranch;
		public override T Accept(StatementVisitor<T> visitor) {
			return visitor.VisitIfStatement(this);
		}
	}
	public class ReturnStatement<T> : Statement<T> {
		public ReturnStatement(Token keywordToken, Expression<T> value) {
			this.keywordToken = keywordToken;
			this.value = value;
		}
		public Token keywordToken;
		public Expression<T> value;
		public override T Accept(StatementVisitor<T> visitor) {
			return visitor.VisitReturnStatement(this);
		}
	}
	public class VariableDeclarationStatement<T> : Statement<T> {
		public VariableDeclarationStatement(Token nameToken, Expression<T> initializer) {
			this.nameToken = nameToken;
			this.initializer = initializer;
		}
		public Token nameToken;
		public Expression<T> initializer;
		public override T Accept(StatementVisitor<T> visitor) {
			return visitor.VisitVariableDeclarationStatement(this);
		}
	}
	public class WhileStatement<T> : Statement<T> {
		public WhileStatement(Expression<T> condition, Statement<T> body) {
			this.condition = condition;
			this.body = body;
		}
		public Expression<T> condition;
		public Statement<T> body;
		public override T Accept(StatementVisitor<T> visitor) {
			return visitor.VisitWhileStatement(this);
		}
	}
}
