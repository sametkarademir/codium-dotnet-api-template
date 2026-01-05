using Xunit;

namespace Codium.Template.IntegrationTests.Base.Common
{
    [CollectionDefinition("Auth Tests", DisableParallelization = true)]
    public class AuthTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
    {
    }

    [CollectionDefinition("Feature Tests", DisableParallelization = false)]
    public class FeatureTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
    {
    }
}