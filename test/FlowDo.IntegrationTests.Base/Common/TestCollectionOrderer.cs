using Xunit;
using Xunit.Abstractions;

namespace FlowDo.IntegrationTests.Base.Common;

public class TestCollectionOrderer : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
    {
        var collections = testCollections.ToList();
        
        // Auth Tests collection'ını önce çalıştır
        var authTests = collections.FirstOrDefault(c => c.DisplayName.Contains("Auth Tests"));
        if (authTests != null)
        {
            yield return authTests;
        }
        
        // Diğer collection'ları çalıştır
        foreach (var collection in collections.Where(c => !c.DisplayName.Contains("Auth Tests")))
        {
            yield return collection;
        }
    }
}

