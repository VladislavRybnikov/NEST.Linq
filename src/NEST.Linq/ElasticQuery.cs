using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nest;

namespace Nest.Linq
{
    public class ElasticQuery<T> : IQueryable<T>, IOrderedQueryable<T>
    {
        private readonly ElasticQueryContext<T> _context;

        public ElasticQuery(ElasticQueryContext<T> context)
        {
            _context = context;
            Provider = new ElasticQueryProvider<T>(context);
            Expression = Expression.Constant(this);
        }

        public ElasticQuery(ElasticQueryProvider<T> provider, Expression expression)
        {
            _context = provider.Context;
            Provider = provider;
            Expression = expression;
        }

        public Task<ISearchResponse<TSearch>> SearchInternalAsync<TSearch>(
            Func<SearchDescriptor<TSearch>, ISearchRequest> searchFunc = null)
            where TSearch : class
        {
            var searchDescriptor = Provider.Execute<SearchDescriptor<TSearch>>(Expression);
            var request = searchFunc?.Invoke(searchDescriptor) ?? searchDescriptor;
            return _context.Client.SearchAsync<TSearch>(request);
        }

        public ISearchResponse<TSearch> SearchInternal<TSearch>(
            Func<SearchDescriptor<TSearch>, ISearchRequest> searchFunc = null)
            where TSearch : class
        {
            var searchDescriptor = Provider.Execute<SearchDescriptor<TSearch>>(Expression);
            var request = searchFunc?.Invoke(searchDescriptor) ?? searchDescriptor;
            return _context.Client.Search<TSearch>(request);
        }

        public IEnumerator<T> GetEnumerator() => Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Provider.Execute<IEnumerable>(Expression).GetEnumerator();

        public Type ElementType => typeof(T);

        public Expression Expression { get; private set; }

        public IQueryProvider Provider { get; private set; }
    }
}