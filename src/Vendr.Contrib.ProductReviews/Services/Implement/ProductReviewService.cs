﻿using System;
using System.Collections.Generic;
using Vendr.Contrib.ProductReviews.Factories;
using Vendr.Contrib.ProductReviews.Persistence.Repositories;
using Vendr.Core;
using Vendr.Core.Persistence;
using Vendr.ProductReviews.Models;

namespace Vendr.ProductReviews.Services.Implement
{
    public sealed class ProductReviewService : IProductReviewService
    {
        private IUnitOfWorkProvider _uowProvider;
        private IProductReviewRepositoryFactory _repositoryFactory;
        //private IRepositoryFactory _repositoryFactory;

        public ProductReviewService(IUnitOfWorkProvider uowProvider, IProductReviewRepositoryFactory repositoryFactory) //IRepositoryFactory repositoryFactory)
        {
            _uowProvider = uowProvider;
            _repositoryFactory = repositoryFactory;
        }

        public ProductReview GetProductReview(Guid id)
        {
            ProductReview productReview;

            using (var uow = _uowProvider.Create())
            using (var repo = _repositoryFactory.CreateProductReviewRepository(uow))
            {
                productReview = repo.Get(id);
                uow.Complete();
            }

            return productReview;
        }

        public IEnumerable<ProductReview> GetProductReviews(Guid[] ids)
        {
            throw new NotImplementedException();
        }

        public void AddProductReview(string productReference, string customerReference, decimal rating, string name, string description)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ProductReview> GetProductReviews(string productReference)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ProductReview> GetProductReviewsForCustomer(string customerReference)
        {
            throw new NotImplementedException();
        }
    }
}