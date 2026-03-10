using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-08 19:00:00", "Seed storefront dealer finance localization resources")]
public class DealerFinanceStorefrontLocalizationMigration : ForwardOnlyMigration
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
            ["Account.DealerFinance"] = "Dealer finance",
            ["Account.DealerFinance.Summary"] = "Dealer finance summary",
            ["Checkout.DealerFinanceSummary"] = "Dealer finance summary",
            ["Account.DealerFinance.Fields.Dealer"] = "Dealer",
            ["Account.DealerFinance.Fields.OpenAccountEnabled"] = "Open account",
            ["Account.DealerFinance.Fields.CreditLimit"] = "Credit limit",
            ["Account.DealerFinance.Fields.CurrentDebt"] = "Current debt",
            ["Account.DealerFinance.Fields.AvailableCredit"] = "Available credit",
            ["Account.DealerFinance.Fields.Status.Enabled"] = "Enabled",
            ["Account.DealerFinance.Fields.Status.Disabled"] = "Disabled",
            ["Account.DealerFinance.Transactions"] = "Transactions",
            ["Account.DealerFinance.Transactions.Columns.Date"] = "Date",
            ["Account.DealerFinance.Transactions.Columns.Type"] = "Type",
            ["Account.DealerFinance.Transactions.Columns.Source"] = "Source",
            ["Account.DealerFinance.Transactions.Columns.ReferenceNo"] = "Reference no",
            ["Account.DealerFinance.Transactions.Columns.DocumentNo"] = "Document no",
            ["Account.DealerFinance.Transactions.Columns.DueDate"] = "Due date",
            ["Account.DealerFinance.Transactions.Columns.Debit"] = "Debit",
            ["Account.DealerFinance.Transactions.Columns.Credit"] = "Credit",
            ["Account.DealerFinance.Transactions.Columns.Balance"] = "Balance",
            ["Account.DealerFinance.Transactions.Columns.Note"] = "Note",
            ["Account.DealerFinance.Transactions.NoData"] = "No transactions found.",
            ["Account.DealerFinance.Instruments"] = "Financial instruments",
            ["Account.DealerFinance.Instruments.Columns.Type"] = "Type",
            ["Account.DealerFinance.Instruments.Columns.Status"] = "Status",
            ["Account.DealerFinance.Instruments.Columns.Amount"] = "Amount",
            ["Account.DealerFinance.Instruments.Columns.DocumentNo"] = "Document no",
            ["Account.DealerFinance.Instruments.Columns.IssueDate"] = "Issue date",
            ["Account.DealerFinance.Instruments.Columns.DueDate"] = "Due date",
            ["Account.DealerFinance.Instruments.Columns.Note"] = "Note",
            ["Account.DealerFinance.Instruments.NoData"] = "No financial instruments found."
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Account.DealerFinance"] = "Bayi finans",
            ["Account.DealerFinance.Summary"] = "Bayi finans ozeti",
            ["Checkout.DealerFinanceSummary"] = "Bayi finans ozeti",
            ["Account.DealerFinance.Fields.Dealer"] = "Bayi",
            ["Account.DealerFinance.Fields.OpenAccountEnabled"] = "Acik hesap",
            ["Account.DealerFinance.Fields.CreditLimit"] = "Kredi limiti",
            ["Account.DealerFinance.Fields.CurrentDebt"] = "Guncel borc",
            ["Account.DealerFinance.Fields.AvailableCredit"] = "Kullanilabilir limit",
            ["Account.DealerFinance.Fields.Status.Enabled"] = "Acik",
            ["Account.DealerFinance.Fields.Status.Disabled"] = "Kapali",
            ["Account.DealerFinance.Transactions"] = "Islemler",
            ["Account.DealerFinance.Transactions.Columns.Date"] = "Tarih",
            ["Account.DealerFinance.Transactions.Columns.Type"] = "Tip",
            ["Account.DealerFinance.Transactions.Columns.Source"] = "Kaynak",
            ["Account.DealerFinance.Transactions.Columns.ReferenceNo"] = "Referans no",
            ["Account.DealerFinance.Transactions.Columns.DocumentNo"] = "Belge no",
            ["Account.DealerFinance.Transactions.Columns.DueDate"] = "Vade tarihi",
            ["Account.DealerFinance.Transactions.Columns.Debit"] = "Borc",
            ["Account.DealerFinance.Transactions.Columns.Credit"] = "Alacak",
            ["Account.DealerFinance.Transactions.Columns.Balance"] = "Bakiye",
            ["Account.DealerFinance.Transactions.Columns.Note"] = "Not",
            ["Account.DealerFinance.Transactions.NoData"] = "Islem bulunamadi.",
            ["Account.DealerFinance.Instruments"] = "Finansal belgeler",
            ["Account.DealerFinance.Instruments.Columns.Type"] = "Tur",
            ["Account.DealerFinance.Instruments.Columns.Status"] = "Durum",
            ["Account.DealerFinance.Instruments.Columns.Amount"] = "Tutar",
            ["Account.DealerFinance.Instruments.Columns.DocumentNo"] = "Belge no",
            ["Account.DealerFinance.Instruments.Columns.IssueDate"] = "Belge tarihi",
            ["Account.DealerFinance.Instruments.Columns.DueDate"] = "Vade tarihi",
            ["Account.DealerFinance.Instruments.Columns.Note"] = "Not",
            ["Account.DealerFinance.Instruments.NoData"] = "Finansal belge bulunamadi."
        });
    }
}
