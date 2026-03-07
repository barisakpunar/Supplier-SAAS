using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data.Mapping;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 01:00:00", "Add source fields to DealerTransaction")]
public class DealerTransactionSourceMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        var transactionTableName = nameof(DealerTransaction);

        if (!Schema.Table(transactionTableName).Exists())
            return;

        if (!Schema.Table(transactionTableName).Column(nameof(DealerTransaction.SourceTypeId)).Exists())
            Alter.Table(transactionTableName)
                .AddColumn(nameof(DealerTransaction.SourceTypeId)).AsInt32().NotNullable().WithDefaultValue((int)DealerTransactionSourceType.None).Indexed();

        if (!Schema.Table(transactionTableName).Column(nameof(DealerTransaction.SourceId)).Exists())
            Alter.Table(transactionTableName)
                .AddColumn(nameof(DealerTransaction.SourceId)).AsInt32().Nullable().Indexed();

        if (!Schema.Table(transactionTableName).Column(nameof(DealerTransaction.ReferenceNo)).Exists())
            Alter.Table(transactionTableName)
                .AddColumn(nameof(DealerTransaction.ReferenceNo)).AsString(400).Nullable();

        var orderTableName = NameCompatibilityManager.GetTableName(typeof(Order));
        Execute.Sql($"""
            UPDATE "public"."{transactionTableName}" AS dt
            SET "SourceTypeId" = {(int)DealerTransactionSourceType.Order},
                "SourceId" = dt."OrderId",
                "ReferenceNo" = COALESCE(dt."ReferenceNo", o."CustomOrderNumber")
            FROM "public"."{orderTableName}" AS o
            WHERE dt."OrderId" IS NOT NULL
              AND o."Id" = dt."OrderId"
              AND dt."SourceTypeId" = {(int)DealerTransactionSourceType.None};
            """);

        if (Schema.Table(nameof(DealerCollection)).Exists())
        {
            Execute.Sql($"""
                UPDATE "public"."{transactionTableName}" AS dt
                SET "SourceTypeId" = {(int)DealerTransactionSourceType.Collection},
                    "SourceId" = dc."Id",
                    "ReferenceNo" = COALESCE(dt."ReferenceNo", dc."ReferenceNo")
                FROM "public"."{nameof(DealerCollection)}" AS dc
                WHERE (dc."DealerTransactionId" = dt."Id" OR dc."CancelledDealerTransactionId" = dt."Id")
                  AND dt."SourceTypeId" IN ({(int)DealerTransactionSourceType.None}, {(int)DealerTransactionSourceType.Collection});
                """);
        }

        Execute.Sql($"""
            UPDATE "public"."{transactionTableName}"
            SET "SourceTypeId" = {(int)DealerTransactionSourceType.ManualAdjustment}
            WHERE "SourceTypeId" = {(int)DealerTransactionSourceType.None}
              AND "TransactionTypeId" IN ({(int)DealerTransactionType.ManualDebitAdjustment}, {(int)DealerTransactionType.ManualCreditAdjustment});
            """);
    }
}
