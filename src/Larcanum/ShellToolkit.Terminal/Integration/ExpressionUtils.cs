using System.Linq.Expressions;
using System.Reflection;

namespace Larcanum.ShellToolkit.Terminal.Integration;

public static class ExpressionUtils
{
    public static PropertyInfo GetPropertyInfo<T, TValue>(Expression<Func<T, TValue>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression memberExpr)
        {
            throw new ArgumentException("Expression must refer to a property", nameof(propertyExpression));
        }

        if (memberExpr.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Expression must refer to a property, not a field", nameof(propertyExpression));
        }

        if (!propertyInfo.CanWrite)
        {
            throw new InvalidOperationException($"Property '{propertyInfo.Name}' does not have a setter.");
        }

        return propertyInfo;
    }
}
