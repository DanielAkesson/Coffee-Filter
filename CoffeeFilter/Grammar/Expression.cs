using System.Collections.Generic;
namespace CoffeeFilter {
	public abstract class Expression<T>  {
		public abstract T Accept(ExpressionVisitor<T> visitor);
	}
	public interface ExpressionVisitor<T> {
		T VisitAssignExpression(AssignExpression<T> expression);
		T VisitBinaryExpression(BinaryExpression<T> expression);
		T VisitAccessExpression(AccessExpression<T> expression);
		T VisitFunctionCallExpression(FunctionCallExpression<T> expression);
		T VisitGroupingExpression(GroupingExpression<T> expression);
		T VisitLiteralExpression(LiteralExpression<T> expression);
		T VisitListLiteralExpression(ListLiteralExpression<T> expression);
		T VisitUnaryExpression(UnaryExpression<T> expression);
		T VisitVariableExpression(VariableExpression<T> expression);
		T VisitObjectExpression(ObjectExpression<T> expression);
	}
	public class AssignExpression<T> : Expression<T> {
		public AssignExpression(Expression<T> variable, Expression<T> value) {
			this.variable = variable;
			this.value = value;
		}
		public Expression<T> variable;
		public Expression<T> value;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitAssignExpression(this);
		}
	}
	public class BinaryExpression<T> : Expression<T> {
		public BinaryExpression(Expression<T> left, Token operationToken, Expression<T> right) {
			this.left = left;
			this.operationToken = operationToken;
			this.right = right;
		}
		public Expression<T> left;
		public Token operationToken;
		public Expression<T> right;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitBinaryExpression(this);
		}
	}
	public class AccessExpression<T> : Expression<T> {
		public AccessExpression(Expression<T> left, Token operationToken, Token accessToken) {
			this.left = left;
			this.operationToken = operationToken;
			this.accessToken = accessToken;
		}
		public Expression<T> left;
		public Token operationToken;
		public Token accessToken;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitAccessExpression(this);
		}
	}
	public class FunctionCallExpression<T> : Expression<T> {
		public FunctionCallExpression(Expression<T> callee, Token parenToken, List<Expression<T>> arguments) {
			this.callee = callee;
			this.parenToken = parenToken;
			this.arguments = arguments;
		}
		public Expression<T> callee;
		public Token parenToken;
		public List<Expression<T>> arguments;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitFunctionCallExpression(this);
		}
	}
	public class GroupingExpression<T> : Expression<T> {
		public GroupingExpression(Expression<T> expression) {
			this.expression = expression;
		}
		public Expression<T> expression;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitGroupingExpression(this);
		}
	}
	public class LiteralExpression<T> : Expression<T> {
		public LiteralExpression(Variable value) {
			this.value = value;
		}
		public Variable value;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitLiteralExpression(this);
		}
	}
	public class ListLiteralExpression<T> : Expression<T> {
		public ListLiteralExpression(List<Expression<T>> elements) {
			this.elements = elements;
		}
		public List<Expression<T>> elements;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitListLiteralExpression(this);
		}
	}
	public class UnaryExpression<T> : Expression<T> {
		public UnaryExpression(Token operationToken, Expression<T> right) {
			this.operationToken = operationToken;
			this.right = right;
		}
		public Token operationToken;
		public Expression<T> right;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitUnaryExpression(this);
		}
	}
	public class VariableExpression<T> : Expression<T> {
		public VariableExpression(Token nameToken) {
			this.nameToken = nameToken;
		}
		public Token nameToken;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitVariableExpression(this);
		}
	}
	public class ObjectExpression<T> : Expression<T> {
		public ObjectExpression(List<Statement<T>> declarations) {
			this.declarations = declarations;
		}
		public List<Statement<T>> declarations;
		public override T Accept(ExpressionVisitor<T> visitor) {
			return visitor.VisitObjectExpression(this);
		}
	}
}
