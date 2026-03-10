using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-10 12:10:00", "Seed discount single-store policy localization resources")]
public class DiscountSingleStorePolicyLocalizationMigration : ForwardOnlyMigration
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
            ["Admin.Promotions.Discounts.Validation.SingleStoreOnly"] = "Discounts must be limited to a single store.",
            ["Admin.Promotions.Discounts.Validation.LegacyMultiStoreWarning"] = "This discount is currently mapped to multiple stores. Saving it will keep only the selected store.",
            ["Plugins.DiscountRules.DealerSegments.Fields.StoreScope.Required"] = "This discount must be limited to a single store before configuring a dealer segment requirement.",
            ["Plugins.DiscountRules.DealerSegments.Fields.StoreScope.SingleStoreRequired"] = "This discount must be limited to a single store before configuring a dealer segment requirement."
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Admin.Promotions.Discounts.Validation.SingleStoreOnly"] = "Indirimler tek bir magazaya bagli olmalidir.",
            ["Admin.Promotions.Discounts.Validation.LegacyMultiStoreWarning"] = "Bu indirim su anda birden fazla magazaya bagli. Kaydederseniz sadece secili magaza korunur.",
            ["Plugins.DiscountRules.DealerSegments.Fields.StoreScope.Required"] = "Bayi segmenti kosulu tanimlamadan once bu indirim tek bir magazaya bagli olmalidir.",
            ["Plugins.DiscountRules.DealerSegments.Fields.StoreScope.SingleStoreRequired"] = "Bayi segmenti kosulu tanimlamadan once bu indirim tek bir magazaya bagli olmalidir."
        });
    }
}
