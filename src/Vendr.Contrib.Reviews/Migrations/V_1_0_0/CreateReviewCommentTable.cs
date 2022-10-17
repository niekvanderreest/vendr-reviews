#if NETFRAMEWORK
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
    public class CreateReviewCommentTable : MigrationBase
    {
        public CreateReviewCommentTable(IMigrationContext context) 
            : base(context) 
        { }


        #if NETFRAMEWORK
        public override void Migrate()
        #else
        protected override void Migrate()
        #endif
        {
            const string commentTableName = Constants.DatabaseSchema.Tables.Comment;
            const string reviewTableName = Constants.DatabaseSchema.Tables.Review;
#if NET6_0_OR_GREATER
            const string storeTableName = Vendr.Persistence.Constants.DatabaseSchema.Tables.Store;
#else
            const string storeTableName = Vendr.Infrastructure.Constants.DatabaseSchema.Tables.Store;
#endif

            if (!TableExists(commentTableName))
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
                Create.Table(commentTableName)
                    .WithColumn("id").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid).PrimaryKey($"PK_{commentTableName}")
                    .WithColumn("storeId").AsGuid().NotNullable().ForeignKey($"FK_{commentTableName}_{storeTableName}", storeTableName, "id")
                    .WithColumn("reviewId").AsGuid().NotNullable().ForeignKey($"FK_{commentTableName}_{reviewTableName}", reviewTableName, "id")
                    .WithColumn("body").AsCustom(nvarcharMaxType).NotNullable()
                    .WithColumn("createDate").AsDateTime().NotNullable()
                    .Do();
            }
        }
    }
}