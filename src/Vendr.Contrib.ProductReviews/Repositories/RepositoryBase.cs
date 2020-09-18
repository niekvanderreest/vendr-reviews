﻿using Vendr.Core.Persistence.Repositories;

namespace Vendr.Contrib.ProductReviews.Repositories
{
    internal abstract class RepositoryBase : IRepository
    {
        public virtual void Dispose()
        {
            // Dispose of any resources
        }
    }
}