using System.Linq.Expressions;

namespace Nest.Linq.Internals.Visitors
{
    public class TakeVisitor : ElasticVisitor
    {
        public TakeVisitor(ISearchDescriptorAccessor accessor) : base(accessor)
        {
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (IsQuery(node.Type)) return node;

            var sizeMethod = SearchDescriptorType.GetMethod(nameof(SearchDescriptor<object>.Size));
            if (sizeMethod is null) return node;

            var sizeMethodCall = Expression.Call(
                Expression.Constant(SearchDescriptorObject), sizeMethod,
                Expression.Convert(node, typeof(int?)));

            SearchDescriptorObject = Expression.Lambda(sizeMethodCall).Compile().DynamicInvoke();

            return node;
        }
    }
}