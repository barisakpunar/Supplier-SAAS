using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents a dealer transaction list item model
/// </summary>
public partial record DealerTransactionListItemModel : BaseNopEntityModel
{
    #region Properties

    public int DealerId { get; set; }

    public string DealerName { get; set; }

    public int StoreId { get; set; }

    public string StoreName { get; set; }

    public int? OrderId { get; set; }

    public int? CustomerId { get; set; }

    public string TransactionType { get; set; }

    public string Direction { get; set; }

    public decimal Amount { get; set; }

    public decimal DebitAmount { get; set; }

    public decimal CreditAmount { get; set; }

    public decimal? RunningBalance { get; set; }

    public string Note { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    #endregion
}
