﻿#if NETFRAMEWORK
using Umbraco.Core.Migrations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;
#else
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
#endif

namespace Vendr.Contrib.Reviews.Migrations.V_1_0_0
{
    public class CreateReviewTable : MigrationBase
    {
        public CreateReviewTable(IMigrationContext context) 
            : base(context) 
        { }

        #if NETFRAMEWORK
        public override void Migrate()
        #else
        protected override void Migrate()
        #endif
        {
            const string reviewTableName = Constants.DatabaseSchema.Tables.Review;
#if NET6_0_OR_GREATER
            const string storeTableName = Vendr.Persistence.Constants.DatabaseSchema.Tables.Store;
#else
            const string storeTableName = Vendr.Infrastructure.Constants.DatabaseSchema.Tables.Store;
#endif


            if (!TableExists(reviewTableName))
            {

#if NETFRAMEWORK
                var nvarcharMaxType = SqlSyntax is SqlCeSyntaxProvider
                    ? "NTEXT"
                    : "NVARCHAR(MAX)";
#elif NET5_0
                var nvarcharMaxType = DatabaseType is NPoco.DatabaseTypes.SqlServerCEDatabaseType
                    ? "NTEXT"
                    : "NVARCHAR(MAX)";
#elif NET6_0_OR_GREATER
                var nvarcharMaxType = DatabaseType is NPoco.DatabaseTypes.SqlServerCEDatabaseType
                    ? "NTEXT"
                    : DatabaseType is NPoco.DatabaseTypes.SQLiteDatabaseType 
                    ? "TEXT"
                    : "NVARCHAR(MAX)";
#endif

                // Create table
                Create.Table(reviewTableName)
                    .WithColumn("id").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid).PrimaryKey($"PK_{reviewTableName}")
                    .WithColumn("storeId").AsGuid().NotNullable().ForeignKey($"FK_{reviewTableName}_{storeTableName}", storeTableName, "id")
                    .WithColumn("productReference").AsString(255).NotNullable()
                    .WithColumn("customerReference").AsString(255).Nullable()
                    .WithColumn("rating").AsDecimal(2, 1).NotNullable()
                    .WithColumn("title").AsString(255).NotNullable()
                    .WithColumn("email").AsString(255).NotNullable()
                    .WithColumn("name").AsString(255).NotNullable()
                    .WithColumn("body").AsCustom(nvarcharMaxType).NotNullable()
                    .WithColumn("verifiedBuyer").AsBoolean().NotNullable()
                    .WithColumn("recommendProduct").AsBoolean().Nullable()
                    .WithColumn("status").AsInt32().NotNullable()
                    .WithColumn("createDate").AsDateTime().NotNullable()
                    .WithColumn("updateDate").AsDateTime().NotNullable()
                    .Do();
            }
        }
    }
}