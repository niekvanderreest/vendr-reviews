using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using Vendr.Common.Models;
using Vendr.Contrib.Reviews.Models;
using Vendr.Contrib.Reviews.Persistence.Dtos;
using Vendr.Contrib.Reviews.Persistence.Factories;
using Vendr.Common;

#if NETFRAMEWORK
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
#elif NET5_0_OR_GREATER
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;
#endif

#if NET6_0_OR_GREATER
using Vendr.Persistence;
#else
using Vendr.Infrastructure;
#endif

namespace Vendr.Contrib.Reviews.Persistence.Repositories.Implement
{
    internal class ReviewRepository : RepositoryBase, IReviewRepository
    {

        private readonly ISqlContext _sqlSyntax;
#if NET6_0_OR_GREATER
        private readonly IUnitOfWorkProvider _uowProvider;
        private readonly INPocoDatabaseProvider _dbProvider;
        public ReviewRepository(INPocoDatabaseProvider dbProvider, ISqlContext sqlSyntax)
#else
        private readonly IDatabaseUnitOfWork _uow;
        public ReviewRepository(IDatabaseUnitOfWork uow, ISqlContext sqlSyntax)
#endif
        {
#if NET6_0_OR_GREATER
            _dbProvider = dbProvider;
#else
            _uow = uow;
#endif
            _sqlSyntax = sqlSyntax;
        }


        private IDatabase GetDatabase()
        {
#if NET6_0_OR_GREATER
            return _dbProvider.GetDatabase();
#else
            return GetDatabase();
#endif
        }


        protected Sql<ISqlContext> Sql() => _sqlSyntax.Sql();
        protected ISqlSyntaxProvider SqlSyntax => _sqlSyntax.SqlSyntax;

        public Review GetReview(Guid id)
            => GetReviews(new[] { id }).FirstOrDefault();

        public IEnumerable<Review> GetReviews(Guid[] ids)
        {
            Sql<ISqlContext> sql = Sql();

            sql.Select("*")
                .From<ReviewDto>()
                .LeftJoin<CommentDto>().On<CommentDto, ReviewDto>((comment, review) => comment.ReviewId == review.Id)
                .WhereIn<ReviewDto>(x => x.Id, ids);

            var data = GetDatabase().FetchOneToMany<ReviewDto>(x => x.Comments, sql);

            return data.Select(EntityFactory.BuildEntity).ToList();
        }

        public PagedResult<Review> SearchReviews(Guid storeId, string searchTerm = null, string[] productReferences = null, string[] customerReferences = null, ReviewStatus[] statuses = null, decimal[] ratings = null, DateTime? startDate = null, DateTime? endDate = null, long pageNumber = 1, long pageSize = 50)
        {
            productReferences = productReferences ?? new string[0];
            customerReferences = customerReferences ?? new string[0];
            statuses = statuses ?? new ReviewStatus[0];
            ratings = ratings ?? new decimal[0];

            Sql<ISqlContext> sql = Sql();

            sql.Select("*")
                .From<ReviewDto>()
                .Where<ReviewDto>(x => x.StoreId == storeId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                //sql.Where<ReviewDto>(x =>
                //    x.Title.Contains(searchTerm) ||
                //    x.Name.Contains(searchTerm) ||
                //    x.Email.Contains(searchTerm) ||
                //    x.Body.Contains(searchTerm)
                //);

                sql.Where($"( upper({ReviewDto.TableName}.{SqlSyntax.GetQuotedColumnName("title")}) LIKE upper(@term) " +
                    $"OR upper({ReviewDto.TableName}.{SqlSyntax.GetQuotedColumnName("name")}) LIKE upper(@term) " +
                    $"OR upper({ReviewDto.TableName}.{SqlSyntax.GetQuotedColumnName("email")}) LIKE upper(@term) " +
                    $"OR upper(convert(nvarchar(4000), {ReviewDto.TableName}.{SqlSyntax.GetQuotedColumnName("body")})) LIKE upper(@term))", new { term = $"%{searchTerm}%" });
            }

            if (productReferences.Length > 0)
            {
                sql.WhereIn<ReviewDto>(x => x.ProductReference, productReferences);
            }

            if (customerReferences.Length > 0)
            {
                sql.WhereIn<ReviewDto>(x => x.CustomerReference, customerReferences);
            }

            if (statuses.Length > 0)
            {
                sql.WhereIn<ReviewDto>(x => x.Status, statuses.Select(x => (int)x));
            }

            if (ratings.Length > 0)
            {
                sql.WhereIn<ReviewDto>(x => x.Rating, ratings);
            }

            if (startDate != null && startDate >= DateTime.MinValue)
            {
                sql.Where<ReviewDto>(x => x.CreateDate >= startDate.Value);
            }

            if (endDate != null && endDate <= DateTime.MaxValue)
            {
                sql.Where<ReviewDto>(x => x.CreateDate <= endDate.Value);
            }

            sql.OrderByDescending<ReviewDto>(x => x.CreateDate);

            var page = GetDatabase().Page<ReviewDto>(pageNumber, pageSize, sql);

            return new PagedResult<Review>(page.TotalItems, page.CurrentPage, page.ItemsPerPage)
            {
                Items = page.Items.Select(EntityFactory.BuildEntity).ToList()
            };
        }

        public decimal GetAverageRatingForProduct(Guid storeId, string productReference)
        {
            return GetDatabase().ExecuteScalar<decimal>($"SELECT AVG(rating) FROM {ReviewDto.TableName} WHERE storeId = @0 AND productReference = @1 AND status = @2", storeId, productReference, (int)ReviewStatus.Approved);
        }

        public Review SaveReview(Review review)
        {
            var dto = EntityFactory.BuildDto(review);

            dto.Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;

            GetDatabase().Save(dto);

            return EntityFactory.BuildEntity(dto);
        }

        public void DeleteReview(Guid id)
        {
            GetDatabase().Delete<CommentDto>("WHERE reviewId = @0", id);
            GetDatabase().Delete<ReviewDto>("WHERE id = @0", id);
        }

        public Review ChangeReviewStatus(Guid id, ReviewStatus status)
        {
            var review = GetDatabase().SingleById<ReviewDto>(id);

            review.Status = (int)status;

            GetDatabase().Update(review);

            return EntityFactory.BuildEntity(review);
        }

        public Comment SaveComment(Comment comment)
        {
            var dto = EntityFactory.BuildDto(comment);

            dto.Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;

            var entry = GetDatabase().SingleOrDefaultById<CommentDto>(dto.Id);
            if (entry == null)
            {
                dto.CreateDate = dto.CreateDate == DateTime.MinValue ? DateTime.UtcNow : dto.CreateDate;
            }
            else
            {
                dto.CreateDate = entry.CreateDate;
            }

            GetDatabase().Save(dto);

            return EntityFactory.BuildEntity(dto);
        }

        public IEnumerable<Comment> GetComments(Guid storeId, Guid reviewId)
            => GetComments(storeId, new[] { reviewId });

        public IEnumerable<Comment> GetComments(Guid storeId, Guid[] reviewIds)
        {
            Sql<ISqlContext> sql = Sql();

            sql.Select("*")
                .From<CommentDto>()
                .Where<CommentDto>(x => x.StoreId == storeId)
                .WhereIn<CommentDto>(x => x.ReviewId, reviewIds);

            return GetDatabase().Fetch<CommentDto>(sql).Select(EntityFactory.BuildEntity).ToList();
        }

        public void DeleteComment(Guid id)
        {
            GetDatabase().Delete<CommentDto>("WHERE id = @0", id);
        }
    }
}