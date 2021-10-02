using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCore.Toolkit.Extensions
{
    internal static class ExpressionExtensions
    {
        private static MemberExpression GetMemberExpression(this LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Body is MemberExpression memberExpression)
            {
                return memberExpression;
            }

            if (lambdaExpression.Body is UnaryExpression unaryExpression)
            {
                if (unaryExpression.Operand is UnaryExpression innerUnaryExpression)
                {
                    return innerUnaryExpression.Operand as MemberExpression;
                }

                return unaryExpression.Operand as MemberExpression;
            }

            throw new ArgumentException("'lambdaExpression' should be a member expression");
        }

        internal static PropertyInfo GetPropertyInfo(this LambdaExpression lambdaExpression)
        {
            var memberExpression = GetMemberExpression(lambdaExpression);

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("'lambdaExpression' should be a property");
            }

            return propertyInfo;
        }

        internal static Expression RemoveConvert(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }
    }
}