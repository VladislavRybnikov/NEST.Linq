using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Nest;

namespace Nest.Linq.Internals.Builders
{
    public class QueryBuilder
    {
        private readonly ISearchDescriptorAccessor _searchDescriptorAccessor;
        private readonly Queue<Func<Expression, MethodCallExpression>> _filters;

        public QueryBuilder(ISearchDescriptorAccessor searchDescriptorAccessor)
        {
            _searchDescriptorAccessor = searchDescriptorAccessor;
            _filters = new();
        }

        public bool IsEmpty => _filters.Count == 0;

        public ParameterExpression Parameter { get; private set; }

        public string ParameterName { get; private set; }

        public QueryBuilder InitParameter(Type type, string parameterName)
        {
            if (!string.IsNullOrEmpty(ParameterName))
            {
                return this;
            }

            ParameterName = parameterName;
            Parameter = Expression.Parameter(type, ParameterName);
            return this;
        }

        public QueryBuilder Add(Func<Expression, MethodCallExpression> filter)
        {
            _filters.Enqueue(filter);
            return this;
        }

        public QueryBuilder JoinQuery<T>(JoinOperation operation)
        {
            return operation switch
            {
                JoinOperation.AndAlso => JoinWithAndAlso<T>(),
                JoinOperation.OrElse => JoinWithOrElse<T>(),
                _ => throw new ArgumentException("Invalid filter join operation")
            };
        }

        private QueryBuilder JoinWithAndAlso<T>()
        {
            if (_filters.Count <= 1) return this;

            var queryContainerType = typeof(QueryContainerDescriptor<>).MakeGenericType(typeof(T));

            var funcType = typeof(Func<,>).MakeGenericType(queryContainerType, typeof(QueryContainer));
            var funcEnumerableType = typeof(IEnumerable<>).MakeGenericType(funcType);

            var boolQueryType = typeof(BoolQueryDescriptor<>).MakeGenericType(typeof(T));
            var boolMethod = queryContainerType.GetMethod(nameof(QueryContainerDescriptor<object>.Bool));
            var mustMethod = boolQueryType.GetMethod(nameof(BoolQueryDescriptor<object>.Must), new[]
            {
                funcEnumerableType
            });

            var lambdas = new List<LambdaExpression>();
            while (_filters.TryDequeue(out var filter))
            {
                var param = Expression.Parameter(queryContainerType, "must");
                lambdas.Add(Expression.Lambda(filter(param), param));
            }

            var lambdaParam = Expression.NewArrayInit(funcType, lambdas);

            if (boolMethod != null && mustMethod != null)
            {
                Add(param =>
                {
                    var boolParam = Expression.Parameter(boolQueryType, "bool");
                    return Expression.Call(param, boolMethod,
                        Expression.Lambda(Expression.Call(
                            boolParam, mustMethod, lambdaParam), boolParam));
                });
            }

            return this;
        }

        private QueryBuilder JoinWithOrElse<T>()
        {
            var filters = new List<Func<Expression, MethodCallExpression>>();
            while (_filters.TryDequeue(out var filter))
            {
                filters.Add(filter);
            }

            // TODO:

            return this;
        }

        public Func<object, object> Build<T>()
        {
            if (!IsEmpty && Parameter != null)
            {
                var queryMethod = _searchDescriptorAccessor.SearchDescriptorType
                    .GetMethod(nameof(SearchDescriptor<object>.Query));

                Expression expression = Parameter;

                JoinQuery<T>(JoinOperation.AndAlso);
                while (_filters.TryDequeue(out var filter))
                {
                    expression = filter(expression);
                }

                if (expression is MethodCallExpression methodCall)
                {
                    Debug.WriteLine(expression.ToString());

                    var lambdaExpr = Expression.Lambda(methodCall, Parameter);
                    var lambda = lambdaExpr.Compile();

                    return obj => queryMethod?.Invoke(obj, new object[] { lambda });
                }
            }

            return null;
        }
    }
}