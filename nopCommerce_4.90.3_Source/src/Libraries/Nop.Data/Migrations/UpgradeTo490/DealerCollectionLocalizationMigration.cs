using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Localization;

namespace Nop.Data.Migrations.UpgradeTo490;

[NopSchemaMigration("2026-03-07 02:00:00", "Seed dealer collection localization resources")]
public class DealerCollectionLocalizationMigration : ForwardOnlyMigration
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

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        UpsertResources("en-US", new Dictionary<string, string>
        {
            ["Admin.Customers.DealerCollections"] = "Dealer collections",
            ["Admin.Customers.DealerCollections.AddNew"] = "Add new collection",
            ["Admin.Customers.DealerCollections.Audit"] = "Audit",
            ["Admin.Customers.DealerCollections.Cancel"] = "Cancel collection",
            ["Admin.Customers.DealerCollections.Cancellation"] = "Cancellation",
            ["Admin.Customers.DealerCollections.Columns.Amount"] = "Amount",
            ["Admin.Customers.DealerCollections.Columns.CollectionDate"] = "Collection date",
            ["Admin.Customers.DealerCollections.Columns.Customer"] = "Customer",
            ["Admin.Customers.DealerCollections.Columns.Dealer"] = "Dealer",
            ["Admin.Customers.DealerCollections.Columns.Method"] = "Method",
            ["Admin.Customers.DealerCollections.Columns.Note"] = "Note",
            ["Admin.Customers.DealerCollections.Columns.ReferenceNo"] = "Reference no",
            ["Admin.Customers.DealerCollections.Columns.Status"] = "Status",
            ["Admin.Customers.DealerCollections.Columns.Store"] = "Store",
            ["Admin.Customers.DealerCollections.Details"] = "Collection details",
            ["Admin.Customers.DealerCollections.Fields.Amount"] = "Amount",
            ["Admin.Customers.DealerCollections.Fields.CancelledBy"] = "Cancelled by",
            ["Admin.Customers.DealerCollections.Fields.CancelledDealerTransaction"] = "Cancellation transaction",
            ["Admin.Customers.DealerCollections.Fields.CancelledDealerTransactionDate"] = "Cancellation transaction date",
            ["Admin.Customers.DealerCollections.Fields.CancelledOnUtc"] = "Cancelled on",
            ["Admin.Customers.DealerCollections.Fields.CollectionDate"] = "Collection date",
            ["Admin.Customers.DealerCollections.Fields.CollectionDateFrom"] = "Collection date from",
            ["Admin.Customers.DealerCollections.Fields.CollectionDateTo"] = "Collection date to",
            ["Admin.Customers.DealerCollections.Fields.CollectionMethod"] = "Collection method",
            ["Admin.Customers.DealerCollections.Fields.CreatedBy"] = "Created by",
            ["Admin.Customers.DealerCollections.Fields.CreatedOnUtc"] = "Created on",
            ["Admin.Customers.DealerCollections.Fields.Customer"] = "Customer",
            ["Admin.Customers.DealerCollections.Fields.Dealer"] = "Dealer",
            ["Admin.Customers.DealerCollections.Fields.DealerTransaction"] = "Dealer transaction",
            ["Admin.Customers.DealerCollections.Fields.DealerTransactionDate"] = "Dealer transaction date",
            ["Admin.Customers.DealerCollections.Fields.Note"] = "Note",
            ["Admin.Customers.DealerCollections.Fields.ReferenceNo"] = "Reference no",
            ["Admin.Customers.DealerCollections.Fields.Status"] = "Status",
            ["Admin.Customers.DealerCollections.Fields.Store"] = "Store",
            ["Admin.Customers.DealerCollections.Fields.UpdatedOnUtc"] = "Updated on",
            ["Admin.Customers.DealerCollections.Info"] = "Collection information",
            ["Admin.Customers.DealerCollections.Method.BankTransfer"] = "Bank transfer",
            ["Admin.Customers.DealerCollections.Method.Cash"] = "Cash",
            ["Admin.Customers.DealerCollections.Method.Check"] = "Check",
            ["Admin.Customers.DealerCollections.Method.CreditCard"] = "Credit card",
            ["Admin.Customers.DealerCollections.Method.Other"] = "Other",
            ["Admin.Customers.DealerCollections.Method.PromissoryNote"] = "Promissory note",
            ["Admin.Customers.DealerCollections.NoData"] = "No collections found.",
            ["Admin.Customers.DealerCollections.Status.Cancelled"] = "Cancelled",
            ["Admin.Customers.DealerCollections.Status.Posted"] = "Posted",
            ["Admin.Customers.DealerCollections.Validation.AmountRange"] = "Amount must be between 0.0001 and 99999999999999.9999.",
            ["Admin.Customers.DealerCollections.Validation.AmountScale"] = "Amount can contain up to 4 decimal digits.",
            ["Admin.Customers.DealerCollections.Validation.CustomerInvalid"] = "The selected customer is not mapped to the dealer.",
            ["Admin.Customers.DealerCollections.Validation.DealerRequired"] = "A valid dealer is required.",
            ["Admin.Customers.DealerCollections.Validation.MethodRequired"] = "A valid collection method is required.",
            ["Admin.Customers.Dealers.Transactions.Source.Collection"] = "Collection #{0}",
            ["Admin.Customers.Dealers.Transactions.Source.ManualAdjustment"] = "Manual adjustment",
            ["Admin.Customers.Dealers.Transactions.Source.Order"] = "Order #{0}",
            ["Admin.Customers.Dealers.Transactions.Source.OrderByReference"] = "Order #{0}"
        });

        UpsertResources("tr-TR", new Dictionary<string, string>
        {
            ["Admin.Customers.DealerCollections"] = "Bayi tahsilatlari",
            ["Admin.Customers.DealerCollections.AddNew"] = "Yeni tahsilat ekle",
            ["Admin.Customers.DealerCollections.Audit"] = "Kayit bilgileri",
            ["Admin.Customers.DealerCollections.Cancel"] = "Tahsilati iptal et",
            ["Admin.Customers.DealerCollections.Cancellation"] = "Iptal bilgileri",
            ["Admin.Customers.DealerCollections.Columns.Amount"] = "Tutar",
            ["Admin.Customers.DealerCollections.Columns.CollectionDate"] = "Tahsilat tarihi",
            ["Admin.Customers.DealerCollections.Columns.Customer"] = "Musteri",
            ["Admin.Customers.DealerCollections.Columns.Dealer"] = "Bayi",
            ["Admin.Customers.DealerCollections.Columns.Method"] = "Yontem",
            ["Admin.Customers.DealerCollections.Columns.Note"] = "Not",
            ["Admin.Customers.DealerCollections.Columns.ReferenceNo"] = "Referans no",
            ["Admin.Customers.DealerCollections.Columns.Status"] = "Durum",
            ["Admin.Customers.DealerCollections.Columns.Store"] = "Magaza",
            ["Admin.Customers.DealerCollections.Details"] = "Tahsilat detayi",
            ["Admin.Customers.DealerCollections.Fields.Amount"] = "Tutar",
            ["Admin.Customers.DealerCollections.Fields.CancelledBy"] = "Iptal eden",
            ["Admin.Customers.DealerCollections.Fields.CancelledDealerTransaction"] = "Iptal hareketi",
            ["Admin.Customers.DealerCollections.Fields.CancelledDealerTransactionDate"] = "Iptal hareket tarihi",
            ["Admin.Customers.DealerCollections.Fields.CancelledOnUtc"] = "Iptal tarihi",
            ["Admin.Customers.DealerCollections.Fields.CollectionDate"] = "Tahsilat tarihi",
            ["Admin.Customers.DealerCollections.Fields.CollectionDateFrom"] = "Tahsilat baslangic tarihi",
            ["Admin.Customers.DealerCollections.Fields.CollectionDateTo"] = "Tahsilat bitis tarihi",
            ["Admin.Customers.DealerCollections.Fields.CollectionMethod"] = "Tahsilat yontemi",
            ["Admin.Customers.DealerCollections.Fields.CreatedBy"] = "Olusturan",
            ["Admin.Customers.DealerCollections.Fields.CreatedOnUtc"] = "Olusturma tarihi",
            ["Admin.Customers.DealerCollections.Fields.Customer"] = "Musteri",
            ["Admin.Customers.DealerCollections.Fields.Dealer"] = "Bayi",
            ["Admin.Customers.DealerCollections.Fields.DealerTransaction"] = "Bayi hareketi",
            ["Admin.Customers.DealerCollections.Fields.DealerTransactionDate"] = "Bayi hareket tarihi",
            ["Admin.Customers.DealerCollections.Fields.Note"] = "Not",
            ["Admin.Customers.DealerCollections.Fields.ReferenceNo"] = "Referans no",
            ["Admin.Customers.DealerCollections.Fields.Status"] = "Durum",
            ["Admin.Customers.DealerCollections.Fields.Store"] = "Magaza",
            ["Admin.Customers.DealerCollections.Fields.UpdatedOnUtc"] = "Guncelleme tarihi",
            ["Admin.Customers.DealerCollections.Info"] = "Tahsilat bilgileri",
            ["Admin.Customers.DealerCollections.Method.BankTransfer"] = "Havale/EFT",
            ["Admin.Customers.DealerCollections.Method.Cash"] = "Nakit",
            ["Admin.Customers.DealerCollections.Method.Check"] = "Cek",
            ["Admin.Customers.DealerCollections.Method.CreditCard"] = "Kredi karti",
            ["Admin.Customers.DealerCollections.Method.Other"] = "Diger",
            ["Admin.Customers.DealerCollections.Method.PromissoryNote"] = "Senet",
            ["Admin.Customers.DealerCollections.NoData"] = "Tahsilat bulunamadi.",
            ["Admin.Customers.DealerCollections.Status.Cancelled"] = "Iptal edildi",
            ["Admin.Customers.DealerCollections.Status.Posted"] = "Kaydedildi",
            ["Admin.Customers.DealerCollections.Validation.AmountRange"] = "Tutar 0,0001 ile 99999999999999,9999 arasinda olmalidir.",
            ["Admin.Customers.DealerCollections.Validation.AmountScale"] = "Tutar en fazla 4 ondalik basamak icerebilir.",
            ["Admin.Customers.DealerCollections.Validation.CustomerInvalid"] = "Secilen musteri bu bayi ile eslesmiyor.",
            ["Admin.Customers.DealerCollections.Validation.DealerRequired"] = "Gecerli bir bayi secilmelidir.",
            ["Admin.Customers.DealerCollections.Validation.MethodRequired"] = "Gecerli bir tahsilat yontemi secilmelidir.",
            ["Admin.Customers.Dealers.Transactions.Source.Collection"] = "Tahsilat #{0}",
            ["Admin.Customers.Dealers.Transactions.Source.ManualAdjustment"] = "Manuel duzeltme",
            ["Admin.Customers.Dealers.Transactions.Source.Order"] = "Siparis #{0}",
            ["Admin.Customers.Dealers.Transactions.Source.OrderByReference"] = "Siparis #{0}"
        });
    }
}
