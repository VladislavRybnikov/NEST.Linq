using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nest.Linq
{
    public class ElasticQueryProvider<T> : IQueryProvider
    {
        internal ElasticQueryContext<T> Context { get; }

        public ElasticQueryProvider(ElasticQueryContext<T> context)
        {
            Context = context;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetElementType() ?? throw new TypeLoadException();
            try
            {
                return
                    (IQueryable)Activator.CreateInstance(typeof(ElasticQuery<>).
                        MakeGenericType(elementType), this, expression);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null) throw e.InnerException;
                else throw;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var provider = new ElasticQueryProvider<TElement>(
                new ElasticQueryContext<TElement>(Context.Client));

            return new ElasticQuery<TElement>(provider, expression);
        }

        public object? Execute(Expression expression) => Context.Execute(expression, false);

        public TResult Execute<TResult>(Expression expression) =>
            (TResult)Context.Execute(expression);
    }
}