using Vendr.Common;
using Vendr.Contrib.Reviews.Persistence.Repositories;
using Vendr.Contrib.Reviews.Persistence.Repositories.Implement;
using Vendr.Infrastructure;

#if NETFRAMEWORK
using Umbraco.Core.Scoping;
#elif NET5_0
using Umbraco.Cms.Core.Scoping;
#elif NET6_0_OR_GREATER
using Umbraco.Cms.Infrastructure.Scoping;
using Vendr.Persistence;
#endif

namespace Vendr.Contrib.Reviews.Persistence
{
    public class ReviewRepositoryFactory : IReviewRepositoryFactory
    {
        //private readonly IScopeAccessor _scopeAccessor;
        private readonly IScopeProvider _scopeProvider;

#if NET6_0_OR_GREATER
        private readonly INPocoDatabaseProvider _dbProvider;
        public ReviewRepositoryFactory(IScopeProvider scopeProvider, INPocoDatabaseProvider dbProvider)
#else 
        public ReviewRepositoryFactory(IScopeProvider scopeProvider)
#endif
        {
            _scopeProvider = scopeProvider;
#if NET6_0
            _dbProvider = dbProvider;
#endif
        }

        public IReviewRepository CreateReviewRepository(IUnitOfWork uow)
        {
#if NET6_0
            return new ReviewRepository(_dbProvider, _scopeProvider.SqlContext);
#else
            return new ReviewRepository((IDatabaseUnitOfWork)uow, _scopeProvider.SqlContext);
#endif
        }

    }
}