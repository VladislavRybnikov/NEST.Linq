using System;
using System.Linq;
using System.Linq.Expressions;

namespace Nest.Linq.Internals.Visitors
{
    public class WhereVisitor<T> : ElasticVisitor<T>
    {
        private ConstantExpression _constant;
        private MemberExpression _member;

        public WhereVisitor(ISearchDescriptorAccessor accessor) : base(accessor)
        {
            _constant = null;
            _member = null;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var arg = node.Arguments.FirstOrDefault();

            _ = (arg?.Type.Name, node.Method.Name) switch
            {
                (nameof(String), nameof(string.StartsWith)) => VisitStringStartsWith(node, arg),
                (nameof(String), nameof(string.EndsWith)) => VisitStringEndsWith(node, arg),
                ({ } typeName, { } methodName)
                    => throw new NotImplementedException(
                        $"Method {methodName} of type {typeName} is not supported."),
                _ => throw new ArgumentException("Invalid arguments for Where method.")
            };

            return base.VisitMethodCall(node);
        }

        private Expression VisitStringStartsWith(MethodCallExpression node, Expression callArgument)
        {
            var queryParam = Expression.Parameter(QueryContainerDescriptorInfo.Type, "q");

            var matchParam = Expression.Parameter(MatchQueryDescriptorInfo.Type, "m");

            return node;
        }

        private Expression VisitStringEndsWith(MethodCallExpression node, Expression callArgument)
        {
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _member = node;
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _constant = node;
            return base.VisitConstant(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);
            Visit(node.Right);

            if (node.Type == typeof(bool))
            {
                if (Filters.IsEmpty)
                {
                    Filters.InitParameter(QueryContainerDescriptorInfo.Type, "q");
                }

                switch (node.NodeType)
                {
                    case ExpressionType.Equal:
                        Filters.Add(VisitEquality);
                        break;

                    case ExpressionType.AndAlso:
                        Filters.JoinQuery<T>(JoinOperation.AndAlso);
                        break;

                    case ExpressionType.OrElse:
                        Filters.JoinQuery<T>(JoinOperation.OrElse);
                        break;
                }

                return node;
            }

            return node;
        }

        private MethodCallExpression VisitEquality(Expression queryParam)
        {
            if (_member?.Expression is not ParameterExpression lambdaParam) return null;

            var matchParam = Expression.Parameter(MatchQueryDescriptorInfo.Type, "m");

            var callFieldMethod = Expression.Call(matchParam,
                MatchQueryDescriptorInfo.FieldMethodName,
                new[] { _member.Type },
                Expression.Lambda(_member, lambdaParam)
            );

            var callMatchQueryMethod =
                Expression.Call(callFieldMethod, MatchQueryDescriptorInfo.QueryMethod, _constant);

            var callMatchMethod = Expression.Call(queryParam, QueryContainerDescriptorInfo.MatchMethod,
                Expression.Lambda(callMatchQueryMethod, matchParam));

            return callMatchMethod;
        }
    }
}