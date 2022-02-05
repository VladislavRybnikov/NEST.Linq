using System;
using System.Linq;
using System.Linq.Expressions;
using Nest.Linq.Internals.Builders;

namespace Nest.Linq.Internals.Visitors
{
    public class ElasticQueryVisitor<T> : ElasticVisitor
    {
        public ElasticQueryVisitor() : base(new InternalSearchDescriptorAccessor())
        {
        }

        public ElasticQueryVisitor(ISearchDescriptorAccessor accessor) : base(accessor)
        {
        }

        protected ElasticVisitor ResolveVisitor(string methodName) =>
            methodName switch
            {
                nameof(Queryable.Skip) => new SkipVisitor(this),
                nameof(Queryable.Take) => new TakeVisitor(this),
                nameof(Queryable.Where) => new WhereVisitor<T>(this),
                nameof(Queryable.OrderBy) => new OrderByVisitor<T>(this),
                nameof(Queryable.OrderByDescending) => new OrderByVisitor<T>(this, true),
                _ => throw new NotImplementedException($"{methodName} is not supported")
            };

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var arg = node.Arguments.FirstOrDefault(e => !IsQuery(e.Type));

            var visitor = ResolveVisitor(node.Method.Name);
            visitor.Visit(arg);

            foreach (var mArg in node.Arguments)
            {
                if (mArg is MethodCallExpression methodCallExpression
                    && methodCallExpression.Type.Assembly == typeof(IQueryable).Assembly)
                {
                    VisitMethodCall(methodCallExpression);
                }
            }

            return node;
        }

        private class InternalSearchDescriptorAccessor : ISearchDescriptorAccessor
        {
            public QueryBuilder Filters { get; }
            public Type SearchDescriptorType { get; }
            public object SearchDescriptorObject { get; }

            public InternalSearchDescriptorAccessor()
            {
                SearchDescriptorType = typeof(SearchDescriptor<>).MakeGenericType(typeof(T));
                SearchDescriptorObject = Activator.CreateInstance(SearchDescriptorType);
                Filters = new QueryBuilder(this);
            }
        }
    }
}