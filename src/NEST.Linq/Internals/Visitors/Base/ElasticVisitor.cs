using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nest.Linq.Internals.Builders;

namespace Nest.Linq.Internals.Visitors
{

    public abstract class ElasticVisitor<T> : ElasticVisitor
    {
        protected static class MatchQueryDescriptorInfo
        {
            public static Type Type
                => typeof(MatchQueryDescriptor<>).MakeGenericType(typeof(T));

            public static MethodInfo QueryMethod
                => Type.GetMethod(nameof(MatchQueryDescriptor<object>.Query));

            public static string FieldMethodName => nameof(MatchQueryDescriptor<object>.Field);
        }

        protected static class QueryContainerDescriptorInfo
        {
            public static Type Type => typeof(QueryContainerDescriptor<>).MakeGenericType(typeof(T));

            public static MethodInfo MatchMethod => Type.GetMethod(nameof(QueryContainerDescriptor<object>.Match));

            public static MethodInfo PrefixMethod => Type.GetMethod(nameof(QueryContainerDescriptor<object>.Prefix));
        }

        protected static class SortDescriptorInfo
        {
            public static Type Type => typeof(SortDescriptor<>).MakeGenericType(typeof(T));

            public static MethodInfo AscendingMethod => Type.GetMethods().FirstOrDefault(m
                => m.Name == nameof(SortDescriptor<object>.Ascending) && m.IsGenericMethod);

            public static MethodInfo DescendingMethod => Type.GetMethod(nameof(SortDescriptor<object>.Descending));

        }

        protected ElasticVisitor(ISearchDescriptorAccessor accessor) : base(accessor)
        {
        }
    }

    public abstract class ElasticVisitor : ExpressionVisitor, ISearchDescriptorAccessor
    {
        public QueryBuilder Filters { get; }
        public Type SearchDescriptorType { get; }
        public object SearchDescriptorObject { get; protected set; }

        protected ElasticVisitor(ISearchDescriptorAccessor accessor)
        {
            SearchDescriptorType = accessor.SearchDescriptorType;
            SearchDescriptorObject = accessor.SearchDescriptorObject;
            Filters = accessor.Filters;
        }

        public bool IsQuery(Type type)
            => type.IsGenericType
               && (type.GetGenericTypeDefinition() == typeof(ElasticQuery<>)
                   || type.GetGenericTypeDefinition() == typeof(IQueryable<>));
    }
}