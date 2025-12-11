using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EFCore.Toolkit.Utils
{
    internal static class DbHelpers
    {
        /// <summary>
        /// Attempts to convert a lambda expression representing a navigation property
        /// into a dot-separated string path suitable for EF Core string-based Include.
        /// Supports nested collections using Select.
        /// </summary>
        internal static bool TryParsePath(Expression expression, out string? path)
        {
            path = null;

            if (expression == null)
            {
                return false;
            }

            expression = RemoveConvert(expression);

            switch (expression)
            {
                case MemberExpression memberExpression:
                    if (!TryParsePath(memberExpression.Expression, out var parentPath))
                    {
                        return false;
                    }

                    path = string.IsNullOrEmpty(parentPath) ? memberExpression.Member.Name : $"{parentPath}.{memberExpression.Member.Name}";
                    return true;

                case MethodCallExpression methodCallExpression:
                    if (methodCallExpression.Method.Name == "Select" &&
                        methodCallExpression.Arguments.Count == 2)
                    {
                        if (!TryParsePath(methodCallExpression.Arguments[0], out var collectionPath))
                        {
                            return false;
                        }

                        if (methodCallExpression.Arguments[1] is LambdaExpression lambda &&
                            TryParsePath(lambda.Body, out var memberPath))
                        {
                            path = string.IsNullOrEmpty(collectionPath)
                                ? memberPath
                                : $"{collectionPath}.{memberPath}";
                            return true;
                        }

                        return false;
                    }

                    // Unsupported method
                    return false;

                case ParameterExpression:
                    path = null;
                    return true;

                case ConstantExpression:
                    path = null;
                    return true;

                default:
                    // Unsupported expression type
                    return false;
            }
        }

        /// <summary>
        /// Removes any Convert / UnaryExpression wrappers from an expression.
        /// </summary>
        private static Expression RemoveConvert(Expression expression)
        {
            while (expression is UnaryExpression unary &&
                   (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
            {
                expression = unary.Operand;
            }

            return expression;
        }
    }
}
