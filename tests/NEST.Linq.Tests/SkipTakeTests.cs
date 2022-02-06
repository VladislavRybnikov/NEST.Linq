using System.Linq;
using Nest;
using Nest.Linq;
using Xunit;

namespace NEST.Linq.Tests
{
    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class SkipTakeTests
    {
        [Theory]
        [InlineData(5, 3)]
        [InlineData(null, 4)]
        [InlineData(6, null)]
        public void SkipTakeTest(int? skip, int? take)
        {

            ISearchResponse<User> QueryableQuery(IElasticClient client)
            {
                var q = client.AsQueryable<User>();

                if (skip is { } s)
                {
                    q = q.Skip(s);
                }

                if (take is { } t)
                {
                    q = q.Take(t);
                }

                return q.Search();
            }

            ISearchResponse<User> NestQuery(IElasticClient client)
            {
                return client.Search<User>(search =>
                {
                    if (skip is { } s)
                    {
                        search = search.From(s);
                    }

                    if (take is { } t)
                    {
                        search = search.Size(t);
                    }

                    return search;
                });
            }

            AssertExt.CompareQueries(NestQuery, QueryableQuery,
                (expected, actual) =>
                {
                    Assert.Equal(expected.From, actual.From);
                    Assert.Equal(expected.Size, actual.Size);
                });
        }
    }
}
