﻿using Vendr.Common.Events;
using Vendr.Core.Adapters;
using Vendr.Core.Services;
using Vendr.Core.Models;

#if NETFRAMEWORK
using Umbraco.Core.Models.PublishedContent;
#else
using Umbraco.Cms.Core.Models.PublishedContent;
#endif

namespace Vendr.Contrib.Reviews.Events.Handlers
{
    public class LogReviewAddedActivity : NotificationEventHandlerBase<ReviewAddedNotification>
    {
        private readonly IActivityLogService _activityLogService;
        private readonly IProductAdapter _productAdapter;
        private readonly IVariationContextAccessor _variationContextAccessor;

        public LogReviewAddedActivity(IActivityLogService activityLogService, IProductAdapter productAdapter, IVariationContextAccessor variationContextAccessor)
        {
            _activityLogService = activityLogService;
            _productAdapter = productAdapter;
            _variationContextAccessor = variationContextAccessor;
        }

        public override void Handle(ReviewAddedNotification evt)
        {
            var culture = _variationContextAccessor.VariationContext.Culture;

            IProductSnapshot snapshot = _productAdapter.GetProductSnapshot(evt.Review.StoreId, evt.Review.ProductReference, culture);

            if (snapshot == null)
                return;

            _activityLogService.LogActivity(evt.Review.StoreId,
                evt.Review.Id, 
                Constants.Entities.EntityTypes.Review,
                "New review added",
                $"vendrreviews/review-edit/{evt.Review.StoreId}_{evt.Review.Id}",
                $"Review submitted from {evt.Review.Name} with a rating of {evt.Review.Rating} for product {snapshot.Sku}",
                evt.Review.CreateDate);
        }
    }
}