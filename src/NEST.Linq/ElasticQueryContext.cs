using System;
using System.Linq.Expressions;
using Nest.Linq.Internals.Visitors;

namespace Nest.Linq
{
    public class ElasticQueryContext<T>
    {
        internal IElasticClient Client { get; }

        public ElasticQueryContext(IElasticClient client)
        {
            Client = client;
        }

        public object Execute(Expression expression, bool isEnumerable = false)
        {
            if (!typeof(T).IsClass) throw new InvalidOperationException();

            var visitor = new ElasticQueryVisitor<T>();
            visitor.Visit(expression);
            var filter = visitor.Filters.Build<T>();

            return filter?.Invoke(visitor.SearchDescriptorObject) ?? visitor.SearchDescriptorObject;
        }
    }
}