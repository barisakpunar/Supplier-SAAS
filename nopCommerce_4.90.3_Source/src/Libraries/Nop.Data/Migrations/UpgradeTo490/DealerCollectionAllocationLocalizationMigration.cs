using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 09:40:00", "Seed dealer collection allocation localization resources")]
public class DealerCollectionAllocationLocalizationMigration : ForwardOnlyMigration
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
            ["Admin.Customers.DealerCollections.Allocations"] = "Allocations",
            ["Admin.Customers.DealerCollections.Allocations.AllocatedAmount"] = "Allocated amount",
            ["Admin.Customers.DealerCollections.Allocations.UnallocatedAmount"] = "Unallocated amount",
            ["Admin.Customers.DealerCollections.Allocations.Columns.DebitSource"] = "Debit source",
            ["Admin.Customers.DealerCollections.Allocations.Columns.Amount"] = "Amount",
            ["Admin.Customers.DealerCollections.Allocations.Columns.CreatedOn"] = "Created on",
            ["Admin.Customers.DealerCollections.Allocations.Columns.Status"] = "Status",
            ["Admin.Customers.DealerCollections.Allocations.Status.Active"] = "Active",
            ["Admin.Customers.DealerCollections.Allocations.Status.Cancelled"] = "Cancelled",
            ["Admin.Customers.DealerCollections.Allocations.NoData"] = "No allocations found.",
            ["Admin.Customers.DealerFinanceAudit.ActionType.CollectionAllocationCreated"] = "Collection allocation created",
            ["Admin.Customers.DealerFinanceAudit.ActionType.CollectionAllocationCancelled"] = "Collection allocation cancelled"
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Admin.Customers.DealerCollections.Allocations"] = "Eslestirmeler",
            ["Admin.Customers.DealerCollections.Allocations.AllocatedAmount"] = "Eslesen tutar",
            ["Admin.Customers.DealerCollections.Allocations.UnallocatedAmount"] = "Eslesmeyen tutar",
            ["Admin.Customers.DealerCollections.Allocations.Columns.DebitSource"] = "Borc kaynagi",
            ["Admin.Customers.DealerCollections.Allocations.Columns.Amount"] = "Tutar",
            ["Admin.Customers.DealerCollections.Allocations.Columns.CreatedOn"] = "Olusturma tarihi",
            ["Admin.Customers.DealerCollections.Allocations.Columns.Status"] = "Durum",
            ["Admin.Customers.DealerCollections.Allocations.Status.Active"] = "Aktif",
            ["Admin.Customers.DealerCollections.Allocations.Status.Cancelled"] = "Iptal edildi",
            ["Admin.Customers.DealerCollections.Allocations.NoData"] = "Eslestirme bulunamadi.",
            ["Admin.Customers.DealerFinanceAudit.ActionType.CollectionAllocationCreated"] = "Tahsilat eslestirmesi olusturuldu",
            ["Admin.Customers.DealerFinanceAudit.ActionType.CollectionAllocationCancelled"] = "Tahsilat eslestirmesi iptal edildi"
        });
    }
}
