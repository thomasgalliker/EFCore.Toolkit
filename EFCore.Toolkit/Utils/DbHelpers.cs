using System.Linq.Expressions;
using EFCore.Toolkit.Extensions;

namespace EFCore.Toolkit.Utils
{
    internal static class DbHelpers
    {
        internal static bool TryParsePath(Expression expression, out string path)
        {
            path = null;
            Expression expression1 = expression.RemoveConvert();
            if (expression1 is MemberExpression memberExpression)
            {
                var name = memberExpression.Member.Name;
                if (!TryParsePath(memberExpression.Expression, out var path1))
                {
                    return false;
                }
                path = path1 == null ? name : path1 + "." + name;
            }
            else if (expression1 is MethodCallExpression methodCallExpression)
            {

                if (methodCallExpression.Method.Name == "Select" &&
                    methodCallExpression.Arguments.Count == 2 &&
                    TryParsePath(methodCallExpression.Arguments[0], out var path1)
                    && path1 != null)
                {
                    if (methodCallExpression.Arguments[1] is LambdaExpression lambdaExpression &&
                        TryParsePath(lambdaExpression.Body, out var path2) && path2 != null)
                    {
                        path = path1 + "." + path2;
                        return true;
                    }
                }
                //if (methodCallExpression.Method.Name == "As" &&
                //    methodCallExpression.Arguments.Count == 1)
                //{
                //    var asType = methodCallExpression.Type;
                //    if (asType != null)
                //    {
                //        path = asType.Name;
                //        return true;
                //    }

                //    string path2;
                //    if (TryParsePath(methodCallExpression, out path2) && path2 != null)
                //    {
                //        path = path2;
                //        return true;
                //    }
                //}
                return false;
            }
            return true;
        }
    }
}