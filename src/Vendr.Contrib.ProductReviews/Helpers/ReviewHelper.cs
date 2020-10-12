﻿using Vendr.Contrib.ProductReviews.Enums;

namespace Vendr.Contrib.ProductReviews.Helpers
{
    internal static class ReviewHelper
    {
        public static string GetStatusColor(ReviewStatus status)
        {
            var color = "black";

            switch (status)
            {
                case ReviewStatus.Pending:
                    color = "light-blue";
                    break;
                case ReviewStatus.Approved:
                    color = "green";
                    break;
                case ReviewStatus.Declined:
                    color = "grey";
                    break;
            }

            return color;
        }
    }
}