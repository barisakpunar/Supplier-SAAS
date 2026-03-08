using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Customer;

public partial record DealerFinanceModel : BaseNopModel
{
    public DealerFinanceModel()
    {
        Transactions = new List<DealerFinanceTransactionModel>();
        FinancialInstruments = new List<DealerFinanceInstrumentModel>();
    }

    public int DealerId { get; set; }

    public string DealerName { get; set; }

    public bool OpenAccountEnabled { get; set; }

    public decimal CreditLimit { get; set; }

    public decimal CurrentDebt { get; set; }

    public decimal AvailableCredit { get; set; }

    public IList<DealerFinanceTransactionModel> Transactions { get; set; }

    public IList<DealerFinanceInstrumentModel> FinancialInstruments { get; set; }
}

public partial record DealerFinanceTransactionModel : BaseNopModel
{
    public DateTime CreatedOn { get; set; }

    public string TransactionType { get; set; }

    public string Source { get; set; }

    public string ReferenceNo { get; set; }

    public string DocumentNo { get; set; }

    public DateTime? DueDate { get; set; }

    public decimal DebitAmount { get; set; }

    public decimal CreditAmount { get; set; }

    public decimal RunningBalance { get; set; }

    public string Note { get; set; }
}

public partial record DealerFinanceInstrumentModel : BaseNopModel
{
    public string InstrumentType { get; set; }

    public string InstrumentStatus { get; set; }

    public decimal Amount { get; set; }

    public string InstrumentNo { get; set; }

    public DateTime? IssueDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string Note { get; set; }
}
