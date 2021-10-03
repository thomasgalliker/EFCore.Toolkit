using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCore.Toolkit.Utils
{
    internal static class ReflectionHelper
    {
        private static MethodInfo GetGenericMethod(Expression<Action> expression, Type genericType)
        {
            var genericMethodDefinition = GetMethod(expression).GetGenericMethodDefinition();
            var methodDefinition = genericMethodDefinition.MakeGenericMethod(genericType);
            return methodDefinition;
        }

        private static MethodInfo GetGenericMethod<T>(Expression<Func<T>> expression, Type genericType)
        {
            var genericMethodDefinition = GetMethod(expression).GetGenericMethodDefinition();
            var methodDefinition = genericMethodDefinition.MakeGenericMethod(genericType);
            return methodDefinition;
        }

        internal static object InvokeGenericMethod(object target, Expression<Action> expression, Type genericType, object[] parameters = null)
        {
            if (parameters == null)
            {
                parameters = new object[] { };
            }

            var result = GetGenericMethod(expression, genericType).Invoke(target, parameters);

            return result;
        }

        internal static object InvokeGenericMethod<T>(object target, Expression<Func<T>> expression, Type genericType, object[] parameters = null)
        {
            if (parameters == null)
            {
                parameters = new object[] { };
            }

            var result = GetGenericMethod(expression, genericType).Invoke(target, parameters);

            return result;
        }

        private static MethodInfo GetMethod(Expression<Action> expression)
        {
            var callExpression = (MethodCallExpression)expression.Body;
            return callExpression.Method;
        }

        private static MethodInfo GetMethod<T>(Expression<Func<T>> expression)
        {
            var callExpression = (MethodCallExpression)expression.Body;
            return callExpression.Method;
        }

        internal static object GetPropertyValue(this object sourceObject, string propertyName)
        {
            return sourceObject.GetType().GetTypeInfo().GetDeclaredProperty(propertyName).GetValue(sourceObject, null);
        }

        internal static void SetPropertyValue(this object sourceObject, string propertyName, object value)
        {
            sourceObject.GetType().GetTypeInfo().GetDeclaredProperty(propertyName).SetValue(sourceObject, value);
        }

        internal static string GetMemberName<T>(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentException("The expression cannot be null.");
            }

            return GetMemberName(expression.Body);
        }

        private static string GetMemberName(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression is MemberExpression memberExpression)
            {
                // Reference type property or field
                return memberExpression.Member.Name;
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                // Reference type method
                return methodCallExpression.Method.Name;
            }

            if (expression is UnaryExpression unaryExpression)
            {
                // Property, field of method returning value type
                return GetMemberName(unaryExpression);
            }

            if (expression is LambdaExpression lambdaExpression)
            {
                return GetMemberName(lambdaExpression.Body);
            }

            throw new ArgumentException("Invalid expression");
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression methodCallExpression)
            {
                var methodExpression = methodCallExpression;
                return methodExpression.Method.Name;
            }

            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }
    }
}