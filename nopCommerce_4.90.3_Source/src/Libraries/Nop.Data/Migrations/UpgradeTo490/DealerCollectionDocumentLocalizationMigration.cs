using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 03:10:00", "Seed dealer collection document localization resources")]
public class DealerCollectionDocumentLocalizationMigration : ForwardOnlyMigration
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
            ["Admin.Customers.DealerCollections.Columns.DocumentNo"] = "Document no",
            ["Admin.Customers.DealerCollections.Columns.DueDate"] = "Due date",
            ["Admin.Customers.DealerCollections.Fields.DocumentNo"] = "Document no",
            ["Admin.Customers.DealerCollections.Fields.IssueDate"] = "Issue date",
            ["Admin.Customers.DealerCollections.Fields.DueDate"] = "Due date",
            ["Admin.Customers.DealerCollections.Validation.DueDateBeforeIssueDate"] = "Due date cannot be earlier than issue date."
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Admin.Customers.DealerCollections.Columns.DocumentNo"] = "Belge no",
            ["Admin.Customers.DealerCollections.Columns.DueDate"] = "Vade tarihi",
            ["Admin.Customers.DealerCollections.Fields.DocumentNo"] = "Belge no",
            ["Admin.Customers.DealerCollections.Fields.IssueDate"] = "Belge tarihi",
            ["Admin.Customers.DealerCollections.Fields.DueDate"] = "Vade tarihi",
            ["Admin.Customers.DealerCollections.Validation.DueDateBeforeIssueDate"] = "Vade tarihi belge tarihinden once olamaz."
        });
    }
}
