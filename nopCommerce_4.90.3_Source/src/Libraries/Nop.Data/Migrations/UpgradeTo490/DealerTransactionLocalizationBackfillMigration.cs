using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 15:00:00", "Backfill dealer transaction localization resources")]
public class DealerTransactionLocalizationBackfillMigration : ForwardOnlyMigration
{
    protected virtual string EscapeSql(string value)
    {
        return (value ?? string.Empty).Replace("'", "''");
    }

    protected virtual void UpsertResources(string languageCulture, IDictionary<string, string> resources)
    {
        if (!Schema.Table(nameof(Language)).Exists() || !Schema.Table(nameof(LocaleStringResource)).Exists() || resources.Count == 0)
            return;

        var values = string.Join(",\n", resources.Select(resource =>
            $"('{EscapeSql(resource.Key)}', '{EscapeSql(resource.Value)}')"));

        Execute.Sql($"""
            WITH resources("ResourceName", "ResourceValue") AS (
                VALUES
                {values}
            )
            UPDATE "public"."{nameof(LocaleStringResource)}" AS locale
            SET "ResourceValue" = resource."ResourceValue"
            FROM resources AS resource
            INNER JOIN "public"."{nameof(Language)}" AS language
                ON language."LanguageCulture" = '{EscapeSql(languageCulture)}'
            WHERE locale."LanguageId" = language."Id"
              AND locale."ResourceName" = resource."ResourceName";
            """);

        Execute.Sql($"""
            WITH resources("ResourceName", "ResourceValue") AS (
                VALUES
                {values}
            )
            INSERT INTO "public"."{nameof(LocaleStringResource)}" ("LanguageId", "ResourceName", "ResourceValue")
            SELECT language."Id", resource."ResourceName", resource."ResourceValue"
            FROM resources AS resource
            INNER JOIN "public"."{nameof(Language)}" AS language
                ON language."LanguageCulture" = '{EscapeSql(languageCulture)}'
            WHERE NOT EXISTS (
                SELECT 1
                FROM "public"."{nameof(LocaleStringResource)}" AS locale
                WHERE locale."LanguageId" = language."Id"
                  AND locale."ResourceName" = resource."ResourceName"
            );
            """);
    }

    public override void Up()
    {
        UpsertResources("en-US", new Dictionary<string, string>
        {
            ["Admin.Customers.Dealers.Transactions"] = "Dealer transactions",
            ["Admin.Customers.Dealers.Transactions.Columns.CreatedOnUtc"] = "Created on (UTC)",
            ["Admin.Customers.Dealers.Transactions.Columns.Dealer"] = "Dealer",
            ["Admin.Customers.Dealers.Transactions.Columns.Store"] = "Store",
            ["Admin.Customers.Dealers.Transactions.Columns.Type"] = "Type",
            ["Admin.Customers.Dealers.Transactions.Columns.Source"] = "Source",
            ["Admin.Customers.Dealers.Transactions.Columns.Debit"] = "Debit",
            ["Admin.Customers.Dealers.Transactions.Columns.Credit"] = "Credit",
            ["Admin.Customers.Dealers.Transactions.Columns.RunningBalance"] = "Running balance",
            ["Admin.Customers.Dealers.Transactions.Columns.Order"] = "Order",
            ["Admin.Customers.Dealers.Transactions.Columns.Customer"] = "Customer",
            ["Admin.Customers.Dealers.Transactions.Columns.Note"] = "Note",
            ["Admin.Customers.Dealers.Transactions.NoData"] = "No transactions found.",
            ["Admin.Customers.Dealers.Transactions.OpenDealer"] = "Open dealer",
            ["Admin.Customers.Dealers.Transactions.StatementDealer"] = "Dealer",
            ["Admin.Customers.Dealers.Transactions.StatementModeHint.Text"] = "Select a dealer to switch to statement mode.",
            ["Admin.Customers.Dealers.Transactions.Summary"] = "Summary",
            ["Admin.Customers.Dealers.Transactions.Summary.TotalDebit"] = "Total debit",
            ["Admin.Customers.Dealers.Transactions.Summary.TotalCredit"] = "Total credit",
            ["Admin.Customers.Dealers.Transactions.Summary.NetBalance"] = "Net balance",
            ["Admin.Customers.Dealers.Transactions.Summary.OpeningBalance"] = "Opening balance",
            ["Admin.Customers.Dealers.Transactions.Summary.ClosingBalance"] = "Closing balance",
            ["Admin.Customers.Dealers.Transactions.DealerFallback"] = "Deleted dealer",
            ["Admin.Customers.Dealers.Transactions.Direction.Debit"] = "Debit",
            ["Admin.Customers.Dealers.Transactions.Direction.Credit"] = "Credit",
            ["Admin.Customers.Dealers.Transactions.Direction.Unknown"] = "Unknown",
            ["Admin.Customers.Dealers.Transactions.Type.OpenAccountOrder"] = "Open account order",
            ["Admin.Customers.Dealers.Transactions.Type.OpenAccountCollection"] = "Open account collection",
            ["Admin.Customers.Dealers.Transactions.Type.ManualDebitAdjustment"] = "Manual debit adjustment",
            ["Admin.Customers.Dealers.Transactions.Type.ManualCreditAdjustment"] = "Manual credit adjustment",
            ["Admin.Customers.Dealers.Transactions.Type.Unknown"] = "Unknown"
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Admin.Customers.Dealers.Transactions"] = "Bayi işlemleri",
            ["Admin.Customers.Dealers.Transactions.Columns.CreatedOnUtc"] = "Oluşturulma (UTC)",
            ["Admin.Customers.Dealers.Transactions.Columns.Dealer"] = "Bayi",
            ["Admin.Customers.Dealers.Transactions.Columns.Store"] = "Mağaza",
            ["Admin.Customers.Dealers.Transactions.Columns.Type"] = "Tip",
            ["Admin.Customers.Dealers.Transactions.Columns.Source"] = "Kaynak",
            ["Admin.Customers.Dealers.Transactions.Columns.Debit"] = "Borç",
            ["Admin.Customers.Dealers.Transactions.Columns.Credit"] = "Alacak",
            ["Admin.Customers.Dealers.Transactions.Columns.RunningBalance"] = "Bakiye",
            ["Admin.Customers.Dealers.Transactions.Columns.Order"] = "Sipariş",
            ["Admin.Customers.Dealers.Transactions.Columns.Customer"] = "Müşteri",
            ["Admin.Customers.Dealers.Transactions.Columns.Note"] = "Not",
            ["Admin.Customers.Dealers.Transactions.NoData"] = "İşlem bulunamadı.",
            ["Admin.Customers.Dealers.Transactions.OpenDealer"] = "Bayiyi aç",
            ["Admin.Customers.Dealers.Transactions.StatementDealer"] = "Bayi",
            ["Admin.Customers.Dealers.Transactions.StatementModeHint.Text"] = "Ekstre moduna geçmek için bir bayi seçin.",
            ["Admin.Customers.Dealers.Transactions.Summary"] = "Özet",
            ["Admin.Customers.Dealers.Transactions.Summary.TotalDebit"] = "Toplam borç",
            ["Admin.Customers.Dealers.Transactions.Summary.TotalCredit"] = "Toplam alacak",
            ["Admin.Customers.Dealers.Transactions.Summary.NetBalance"] = "Net bakiye",
            ["Admin.Customers.Dealers.Transactions.Summary.OpeningBalance"] = "Açılış bakiyesi",
            ["Admin.Customers.Dealers.Transactions.Summary.ClosingBalance"] = "Kapanış bakiyesi",
            ["Admin.Customers.Dealers.Transactions.DealerFallback"] = "Silinmiş bayi",
            ["Admin.Customers.Dealers.Transactions.Direction.Debit"] = "Borç",
            ["Admin.Customers.Dealers.Transactions.Direction.Credit"] = "Alacak",
            ["Admin.Customers.Dealers.Transactions.Direction.Unknown"] = "Bilinmiyor",
            ["Admin.Customers.Dealers.Transactions.Type.OpenAccountOrder"] = "Açık hesap siparişi",
            ["Admin.Customers.Dealers.Transactions.Type.OpenAccountCollection"] = "Açık hesap tahsilatı",
            ["Admin.Customers.Dealers.Transactions.Type.ManualDebitAdjustment"] = "Manuel borç düzeltmesi",
            ["Admin.Customers.Dealers.Transactions.Type.ManualCreditAdjustment"] = "Manuel alacak düzeltmesi",
            ["Admin.Customers.Dealers.Transactions.Type.Unknown"] = "Bilinmiyor"
        });
    }
}
