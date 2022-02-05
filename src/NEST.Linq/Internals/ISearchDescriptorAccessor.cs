using System;
using Nest.Linq.Internals.Builders;

namespace Nest.Linq.Internals
{
    public interface ISearchDescriptorAccessor
    {
        QueryBuilder Filters { get; }
        Type SearchDescriptorType { get; }
        object SearchDescriptorObject { get; }
    }
}