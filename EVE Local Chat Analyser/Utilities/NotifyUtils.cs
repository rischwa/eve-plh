using System;
using System.Linq.Expressions;

namespace EveLocalChatAnalyser.Utilities
{
    public static class NotifyUtils
    {
        public static string GetPropertyName<TIn, TOut>(Expression<Func<TIn, TOut>> action)
        {
            var expression = (MemberExpression) action.Body;
            return expression.Member.Name;
        }
    }
}
