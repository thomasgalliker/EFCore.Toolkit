using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EFCore.Toolkit.Extensions
{
    internal static class TypeExtensions
    {
        /// <summary>
        ///     Gets the raw entity type without dynamic proxy type.
        /// </summary>
        public static Type GetEntityType(this EntityEntry entry)
        {
            var entityType = entry.Entity.GetType();
            if (entityType.Namespace == "System.Data.Entity.DynamicProxies")
            {
                entityType = entityType.GetTypeInfo().BaseType;
            }

            return entityType;
        }

        /// <summary>
        ///     Safely casts the specified object to the type specified through <typeparamref name="TTo" />.
        /// </summary>
        /// <remarks>
        ///     Has been introduced to allow casting objects without breaking the fluent API.
        /// </remarks>
        /// <typeparam name="TTo"></typeparam>
        public static TTo As<TTo>(this object subject)
        {
            if (subject is TTo to)
            {
                return to;
            }

            return default(TTo);
        }

        internal static string GetFormattedName(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsGenericType())
            {
                return type.Name;
            }

            return $"{type.Name.Substring(0, type.Name.IndexOf('`'))}<{string.Join(", ", type.GenericTypeArguments.Select(t => t.GetFormattedName()))}>";
        }

        internal static string GetFormattedFullname(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsGenericType())
            {
                return type.ToString();
            }

            return $"{type.Namespace}.{type.Name.Substring(0, type.Name.IndexOf('`'))}<{string.Join(", ", type.GenericTypeArguments.Select(t => t.GetFormattedFullname()))}>";
        }

        /// <summary>
        ///     Finds the best matching constructor for given type <paramref name="type" />.
        /// </summary>
        internal static ConstructorInfoAndParameters GetMatchingConstructor(this Type type, params object[] args)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var constructors = type.GetTypeInfo().DeclaredConstructors.ToList();
            foreach (var constructor in constructors)
            {
                var allMatched = false;
                var ctorParameters = constructor.GetParameters();
                if (args.Length >= ctorParameters.Count(p => p.HasDefaultValue == false) && args.Length <= ctorParameters.Length)
                {
                    for (var ctorParameterIndex = 0; ctorParameterIndex < ctorParameters.Length; ctorParameterIndex++)
                    {
                        var ctorParameter = ctorParameters[ctorParameterIndex];
                        if (ctorParameterIndex >= args.Length)
                        {
                            var argsList = args.ToList();
                            argsList.Add(ctorParameter.DefaultValue);
                            args = argsList.ToArray();
                        }

                        var arg = args[ctorParameterIndex];
                        if (arg != null)
                        {
                            var argType = arg.GetType().GetTypeInfo();
                            var isEqualType = argType == ctorParameter.ParameterType.GetTypeInfo();


                            var interfaces = ctorParameter.ParameterType.GetTypeInfo().ImplementedInterfaces;
                            var isAnyAssignable = interfaces.Any(i => i.GetTypeInfo().IsAssignableFrom(argType));
                            var isInstanceOfType = ctorParameter.ParameterType.GetTypeInfo().IsAssignableFrom(argType);
                            if (isEqualType || isAnyAssignable || isInstanceOfType)
                            {
                                allMatched = true;
                            }
                            else
                            {
                                allMatched = false;
                                break;
                            }
                        }
                        else
                        {
                            if (!ctorParameter.ParameterType.GetTypeInfo().IsValueType)
                            {
                                allMatched = true;
                            }
                            else
                            {
                                allMatched = false;
                                break;
                            }
                        }
                    }
                }

                if (allMatched)
                {
                    return new ConstructorInfoAndParameters(constructor, args);
                }
            }

            var typeName = type.GetFormattedName();
            var exceptionStringBuilder = new StringBuilder();
            var argTypes = args.Where(d => d != null).Select(d => d.GetType()).ToArray();
            var definedParameters = string.Join(", ", argTypes.Select(p => p.GetFormattedName()));
            exceptionStringBuilder.AppendLine(
                definedParameters.Length == 0
                    ? $"{typeName} does not have a constructor with no parameters."
                    : $"{typeName} does not have a constructor with parameter{(definedParameters.Length > 1 ? "s" : "")} ({definedParameters}).");

            if (constructors.Any())
            {
                exceptionStringBuilder.AppendLine();
                exceptionStringBuilder.AppendLine("Use one of the following constructors:");
                foreach (var constructor in constructors)
                {
                    var parameters = $"{string.Join(", ", constructor.GetParameters().Select(p => $"{p.ParameterType} {p.Name}"))}";
                    exceptionStringBuilder.AppendLine($"{typeName}({parameters})");
                }
            }

            var exceptionMessage = exceptionStringBuilder.ToString();
            throw new InvalidOperationException(exceptionMessage);
        }
    }
}