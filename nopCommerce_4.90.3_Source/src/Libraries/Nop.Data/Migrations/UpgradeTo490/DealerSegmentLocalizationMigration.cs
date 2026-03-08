using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-08 22:00:00", "Seed dealer segment localization resources")]
public class DealerSegmentLocalizationMigration : ForwardOnlyMigration
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
            ["Admin.Customers.DealerSegments"] = "Dealer segments",
            ["Admin.Customers.DealerSegments.AddNew"] = "Add new segment",
            ["Admin.Customers.DealerSegments.Edit"] = "Edit dealer segment",
            ["Admin.Customers.DealerSegments.Info"] = "Segment info",
            ["Admin.Customers.DealerSegments.List.NoData"] = "No dealer segments found.",
            ["Admin.Customers.DealerSegments.List.SearchName"] = "Name / code",
            ["Admin.Customers.DealerSegments.Fields.Name"] = "Name",
            ["Admin.Customers.DealerSegments.Fields.Code"] = "Code",
            ["Admin.Customers.DealerSegments.Fields.Description"] = "Description",
            ["Admin.Customers.DealerSegments.Fields.Store"] = "Store",
            ["Admin.Customers.DealerSegments.Fields.Active"] = "Active",
            ["Admin.Customers.DealerSegments.Fields.DisplayOrder"] = "Display order",
            ["Admin.Customers.DealerSegments.Fields.DealerCount"] = "Dealer count",
            ["Admin.Customers.DealerSegments.Validation.StoreRequired"] = "A valid store is required.",
            ["Admin.Customers.DealerSegments.Validation.NameRequired"] = "Name is required.",
            ["Admin.Customers.DealerSegments.Validation.CodeRequired"] = "Code is required.",
            ["Admin.Customers.DealerSegments.Validation.CodeUnique"] = "Code must be unique within the store.",
            ["Admin.Customers.Dealers.Fields.Segments"] = "Segments",
            ["Admin.Customers.Dealers.Fields.Segments.Hint"] = "Dealer campaign segments.",
            ["Admin.Customers.Dealers.Segments.NoData"] = "No segments available for this store."
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Admin.Customers.DealerSegments"] = "Bayi segmentleri",
            ["Admin.Customers.DealerSegments.AddNew"] = "Yeni segment ekle",
            ["Admin.Customers.DealerSegments.Edit"] = "Bayi segmentini duzenle",
            ["Admin.Customers.DealerSegments.Info"] = "Segment bilgileri",
            ["Admin.Customers.DealerSegments.List.NoData"] = "Bayi segmenti bulunamadi.",
            ["Admin.Customers.DealerSegments.List.SearchName"] = "Ad / kod",
            ["Admin.Customers.DealerSegments.Fields.Name"] = "Ad",
            ["Admin.Customers.DealerSegments.Fields.Code"] = "Kod",
            ["Admin.Customers.DealerSegments.Fields.Description"] = "Aciklama",
            ["Admin.Customers.DealerSegments.Fields.Store"] = "Magaza",
            ["Admin.Customers.DealerSegments.Fields.Active"] = "Aktif",
            ["Admin.Customers.DealerSegments.Fields.DisplayOrder"] = "Gosterim sirasi",
            ["Admin.Customers.DealerSegments.Fields.DealerCount"] = "Bayi sayisi",
            ["Admin.Customers.DealerSegments.Validation.StoreRequired"] = "Gecerli bir magaza gereklidir.",
            ["Admin.Customers.DealerSegments.Validation.NameRequired"] = "Ad zorunludur.",
            ["Admin.Customers.DealerSegments.Validation.CodeRequired"] = "Kod zorunludur.",
            ["Admin.Customers.DealerSegments.Validation.CodeUnique"] = "Kod magazada benzersiz olmalidir.",
            ["Admin.Customers.Dealers.Fields.Segments"] = "Segmentler",
            ["Admin.Customers.Dealers.Fields.Segments.Hint"] = "Bayi kampanya segmentleri.",
            ["Admin.Customers.Dealers.Segments.NoData"] = "Bu magazada segment bulunamadi."
        });
    }
}
