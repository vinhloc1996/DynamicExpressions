﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicExpressions
{
    public static class DynamicExpressions
    {
        private static readonly MethodInfo _containsMethod = typeof(string).GetMethod("Contains"
            , new Type[] { typeof(string) });

        private static readonly MethodInfo _startsWithMethod
            = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethod
            = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

        public static Expression<Func<TEntity, object>> GetPropertyGetter<TEntity>(string property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var param = Expression.Parameter(typeof(TEntity));
            var prop = param.GetNestedProperty(property);
            var convertedProp = Expression.Convert(prop, typeof(object));
            return Expression.Lambda<Func<TEntity, object>>(convertedProp, param);
        }

        public static Func<TEntity, object> GetCompiledPropertyGetter<TEntity>(string property)
        {
            return GetPropertyGetter<TEntity>(property).Compile();
        }

        public static Expression<Func<TEntity, bool>> GetPredicate<TEntity>(string property, FilterOperator op, object value)
        {
            var param = Expression.Parameter(typeof(TEntity));
            return Expression.Lambda<Func<TEntity, bool>>(GetPredicate(param, property, op, value), param);
        }

        public static Func<TEntity, bool> GetCompiledPredicate<TEntity>(string property, FilterOperator op, object value)
        {
            return GetPredicate<TEntity>(property, op, value).Compile();
        }

        internal static Expression GetPredicate(ParameterExpression param, string property, FilterOperator op, object value)
        {
            var constant = Expression.Constant(value);
            var prop = param.GetNestedProperty(property);
            return CreateFilter(prop, op, constant);
        }

        private static Expression CreateFilter(MemberExpression prop, FilterOperator op, ConstantExpression constant)
        {
            return op switch
            {
                FilterOperator.Equals => Expression.Equal(prop, constant),
                FilterOperator.GreaterThan => Expression.GreaterThan(prop, constant),
                FilterOperator.LessThan => Expression.LessThan(prop, constant),
                FilterOperator.Contains => Expression.Call(prop, _containsMethod, constant),
                FilterOperator.StartsWith => Expression.Call(prop, _startsWithMethod, constant),
                FilterOperator.EndsWith => Expression.Call(prop, _endsWithMethod, constant),
                FilterOperator.DoesntEqual => Expression.NotEqual(prop, constant),
                _ => throw new NotImplementedException(),
            };
        }
    }
}