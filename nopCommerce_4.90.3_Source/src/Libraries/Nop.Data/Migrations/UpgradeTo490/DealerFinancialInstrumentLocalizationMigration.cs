using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 04:10:00", "Seed dealer financial instrument localization resources")]
public class DealerFinancialInstrumentLocalizationMigration : ForwardOnlyMigration
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
            ["Admin.Customers.DealerFinancialInstruments"] = "Dealer financial instruments",
            ["Admin.Customers.DealerFinancialInstruments.Type.Check"] = "Check",
            ["Admin.Customers.DealerFinancialInstruments.Type.PromissoryNote"] = "Promissory note",
            ["Admin.Customers.DealerFinancialInstruments.Status.Posted"] = "Posted",
            ["Admin.Customers.DealerFinancialInstruments.Status.Collected"] = "Collected",
            ["Admin.Customers.DealerFinancialInstruments.Status.Returned"] = "Returned",
            ["Admin.Customers.DealerFinancialInstruments.Status.Protested"] = "Protested",
            ["Admin.Customers.DealerFinancialInstruments.Status.Cancelled"] = "Cancelled"
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Admin.Customers.DealerFinancialInstruments"] = "Bayi finansal belgeleri",
            ["Admin.Customers.DealerFinancialInstruments.Type.Check"] = "Cek",
            ["Admin.Customers.DealerFinancialInstruments.Type.PromissoryNote"] = "Senet",
            ["Admin.Customers.DealerFinancialInstruments.Status.Posted"] = "Kaydedildi",
            ["Admin.Customers.DealerFinancialInstruments.Status.Collected"] = "Tahsil edildi",
            ["Admin.Customers.DealerFinancialInstruments.Status.Returned"] = "Iade edildi",
            ["Admin.Customers.DealerFinancialInstruments.Status.Protested"] = "Protesto edildi",
            ["Admin.Customers.DealerFinancialInstruments.Status.Cancelled"] = "Iptal edildi"
        });
    }
}
