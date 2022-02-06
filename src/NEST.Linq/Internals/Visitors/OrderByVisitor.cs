using System.Linq.Expressions;

namespace Nest.Linq.Internals.Visitors
{
    public class OrderByVisitor<T> : ElasticVisitor<T>
    {
        private readonly bool _descending;

        public OrderByVisitor(ISearchDescriptorAccessor accessor, bool descending = false) : base(accessor)
        {
            _descending = @descending;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var sortMethod = SearchDescriptorType.GetMethod(nameof(SearchDescriptor<object>.Sort));
            var sortingTypeMethod = _descending
                    ? SortDescriptorInfo.DescendingMethod
                    : SortDescriptorInfo.AscendingMethod;

            sortingTypeMethod = sortingTypeMethod?.MakeGenericMethod(node.Type);

            if (sortMethod is null || sortingTypeMethod is null) return node;
            if (node.Expression is not ParameterExpression nodeParam) return node;

            var sortDescriptorParam = Expression.Parameter(SortDescriptorInfo.Type, "sort");

            var typeParam = Expression.Parameter(typeof(T), nodeParam.Name);
            var fieldSelector = Expression.Lambda(
                Expression.MakeMemberAccess(typeParam, node.Member), typeParam);

            var sortExpression = Expression.Call(sortDescriptorParam, sortingTypeMethod, fieldSelector);
            var sortLambda = Expression.Lambda(sortExpression, sortDescriptorParam);

            var sortMethodCall = Expression.Call(
                Expression.Constant(SearchDescriptorObject), sortMethod, sortLambda);

            SearchDescriptorObject = Expression.Lambda(sortMethodCall).Compile().DynamicInvoke();

            return base.VisitMember(node);
        }
    }
}