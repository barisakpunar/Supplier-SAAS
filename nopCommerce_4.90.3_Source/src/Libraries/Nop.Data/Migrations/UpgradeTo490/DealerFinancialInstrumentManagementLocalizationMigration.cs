using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 08:00:00", "Seed dealer financial instrument management localization resources")]
public class DealerFinancialInstrumentManagementLocalizationMigration : ForwardOnlyMigration
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
            ["Admin.Customers.DealerFinancialInstruments.Fields.Store"] = "Store",
            ["Admin.Customers.DealerFinancialInstruments.Fields.DueDateFrom"] = "Due date from",
            ["Admin.Customers.DealerFinancialInstruments.Fields.DueDateTo"] = "Due date to",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Dealer"] = "Dealer",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Store"] = "Store",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Customer"] = "Customer",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Type"] = "Type",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Status"] = "Status",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Amount"] = "Amount",
            ["Admin.Customers.DealerFinancialInstruments.Columns.InstrumentNo"] = "Instrument no",
            ["Admin.Customers.DealerFinancialInstruments.Columns.IssueDate"] = "Issue date",
            ["Admin.Customers.DealerFinancialInstruments.Columns.DueDate"] = "Due date",
            ["Admin.Customers.DealerFinancialInstruments.Columns.LinkedCollection"] = "Linked collection",
            ["Admin.Customers.DealerFinancialInstruments.NoData"] = "No financial instruments found.",
            ["Admin.Customers.DealerFinancialInstruments.BackToList"] = "Back to instruments",
            ["Admin.Customers.DealerFinancialInstruments.LinkedCollection.Open"] = "Open linked collection",
            ["Admin.Customers.DealerFinancialInstruments.Actions"] = "Actions",
            ["Admin.Customers.DealerFinancialInstruments.Actions.MarkCollected"] = "Mark as collected",
            ["Admin.Customers.DealerFinancialInstruments.Actions.MarkReturned"] = "Mark as returned",
            ["Admin.Customers.DealerFinancialInstruments.Actions.MarkProtested"] = "Mark as protested"
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Admin.Customers.DealerFinancialInstruments.Fields.Store"] = "Magaza",
            ["Admin.Customers.DealerFinancialInstruments.Fields.DueDateFrom"] = "Vade tarihi baslangic",
            ["Admin.Customers.DealerFinancialInstruments.Fields.DueDateTo"] = "Vade tarihi bitis",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Dealer"] = "Bayi",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Store"] = "Magaza",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Customer"] = "Musteri",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Type"] = "Tur",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Status"] = "Durum",
            ["Admin.Customers.DealerFinancialInstruments.Columns.Amount"] = "Tutar",
            ["Admin.Customers.DealerFinancialInstruments.Columns.InstrumentNo"] = "Belge no",
            ["Admin.Customers.DealerFinancialInstruments.Columns.IssueDate"] = "Belge tarihi",
            ["Admin.Customers.DealerFinancialInstruments.Columns.DueDate"] = "Vade tarihi",
            ["Admin.Customers.DealerFinancialInstruments.Columns.LinkedCollection"] = "Bagli tahsilat",
            ["Admin.Customers.DealerFinancialInstruments.NoData"] = "Finansal belge bulunamadi.",
            ["Admin.Customers.DealerFinancialInstruments.BackToList"] = "Belgelere don",
            ["Admin.Customers.DealerFinancialInstruments.LinkedCollection.Open"] = "Bagli tahsilati ac",
            ["Admin.Customers.DealerFinancialInstruments.Actions"] = "Aksiyonlar",
            ["Admin.Customers.DealerFinancialInstruments.Actions.MarkCollected"] = "Tahsil edildi olarak isaretle",
            ["Admin.Customers.DealerFinancialInstruments.Actions.MarkReturned"] = "Iade edildi olarak isaretle",
            ["Admin.Customers.DealerFinancialInstruments.Actions.MarkProtested"] = "Protesto edildi olarak isaretle"
        });
    }
}
