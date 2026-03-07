using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 08:40:00", "Seed dealer finance audit localization resources")]
public class DealerFinanceAuditLocalizationMigration : ForwardOnlyMigration
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
            ["Admin.Customers.DealerFinanceAudit.Timeline"] = "Audit timeline",
            ["Admin.Customers.DealerFinanceAudit.NoData"] = "No audit records found.",
            ["Admin.Customers.DealerFinanceAudit.Columns.PerformedOn"] = "Performed on",
            ["Admin.Customers.DealerFinanceAudit.Columns.EntityType"] = "Entity",
            ["Admin.Customers.DealerFinanceAudit.Columns.ActionType"] = "Action",
            ["Admin.Customers.DealerFinanceAudit.Columns.StatusTransition"] = "Status transition",
            ["Admin.Customers.DealerFinanceAudit.Columns.PerformedBy"] = "Performed by",
            ["Admin.Customers.DealerFinanceAudit.Columns.Note"] = "Note",
            ["Admin.Customers.DealerFinanceAudit.EntityType.Collection"] = "Collection",
            ["Admin.Customers.DealerFinanceAudit.EntityType.FinancialInstrument"] = "Financial instrument",
            ["Admin.Customers.DealerFinanceAudit.ActionType.CollectionCreated"] = "Collection created",
            ["Admin.Customers.DealerFinanceAudit.ActionType.CollectionCancelled"] = "Collection cancelled",
            ["Admin.Customers.DealerFinanceAudit.ActionType.FinancialInstrumentCreated"] = "Financial instrument created",
            ["Admin.Customers.DealerFinanceAudit.ActionType.FinancialInstrumentStatusChanged"] = "Financial instrument status changed"
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Admin.Customers.DealerFinanceAudit.Timeline"] = "Denetim gecmisi",
            ["Admin.Customers.DealerFinanceAudit.NoData"] = "Denetim kaydi bulunamadi.",
            ["Admin.Customers.DealerFinanceAudit.Columns.PerformedOn"] = "Islem tarihi",
            ["Admin.Customers.DealerFinanceAudit.Columns.EntityType"] = "Varlik",
            ["Admin.Customers.DealerFinanceAudit.Columns.ActionType"] = "Aksiyon",
            ["Admin.Customers.DealerFinanceAudit.Columns.StatusTransition"] = "Durum gecisi",
            ["Admin.Customers.DealerFinanceAudit.Columns.PerformedBy"] = "Islemi yapan",
            ["Admin.Customers.DealerFinanceAudit.Columns.Note"] = "Not",
            ["Admin.Customers.DealerFinanceAudit.EntityType.Collection"] = "Tahsilat",
            ["Admin.Customers.DealerFinanceAudit.EntityType.FinancialInstrument"] = "Finansal belge",
            ["Admin.Customers.DealerFinanceAudit.ActionType.CollectionCreated"] = "Tahsilat olusturuldu",
            ["Admin.Customers.DealerFinanceAudit.ActionType.CollectionCancelled"] = "Tahsilat iptal edildi",
            ["Admin.Customers.DealerFinanceAudit.ActionType.FinancialInstrumentCreated"] = "Finansal belge olusturuldu",
            ["Admin.Customers.DealerFinanceAudit.ActionType.FinancialInstrumentStatusChanged"] = "Finansal belge durumu degisti"
        });
    }
}
