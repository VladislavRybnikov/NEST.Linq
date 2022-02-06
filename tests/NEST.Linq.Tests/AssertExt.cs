using System;
using Moq;
using Nest;

namespace NEST.Linq.Tests
{
    public static class AssertExt
    {
        public static void CompareQueries<T>(
            Func<IElasticClient, ISearchResponse<T>> expected,
            Func<IElasticClient, ISearchResponse<T>> actual,
            Action<ISearchRequest, ISearchRequest> assertion) where T : class
        {
            var (expectedClient, searchQueryExpected) = PrepareSearchDescriptor<T>();
            var (actualClient, searchQueryActual) = PrepareSearchDescriptor<T>();

            _ = expected(expectedClient);
            _ = actual(actualClient);

            assertion(searchQueryExpected.Value, searchQueryActual.Value);
        }

        private static (IElasticClient, Container<ISearchRequest>) PrepareSearchDescriptor<T>() where T : class
        {
            var container = new Container<ISearchRequest>
            {
                Value = new SearchDescriptor<T>()
            };

            var clientMockExpected = new Mock<IElasticClient>();

            clientMockExpected.Setup(m => m
                .Search(It.IsAny<Func<SearchDescriptor<T>, ISearchRequest>>()))
                .Returns<Func<SearchDescriptor<T>, ISearchRequest>>(
                f
                    =>
                {
                    f(container.Value as SearchDescriptor<T>);
                    return null;
                });

            clientMockExpected.Setup(m => m
                .Search<T>(It.IsAny<ISearchRequest>()))
                .Returns<ISearchRequest>(
                f
                    =>
                {
                    container.Value = f;
                    return null;
                });

            var clientExpeted = clientMockExpected.Object;
            return (clientExpeted, container);
        }

        private class Container<T>
        {
            public T Value { get; set; }
        }
    }
}
