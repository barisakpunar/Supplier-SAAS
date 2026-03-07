using FluentAssertions;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Data.DataProviders;
using Nop.Services.Customers;
using NUnit.Framework;

namespace Nop.Tests.Nop.Services.Tests.Customers;

[TestFixture]
[NonParallelizable]
public class DealerServiceTests : ServiceTest
{
    private IDealerService _dealerService;
    private IRepository<DealerInfo> _dealerRepository;
    private IRepository<DealerFinancialProfile> _dealerFinancialProfileRepository;
    private IRepository<DealerCustomerMapping> _dealerCustomerMappingRepository;
    private IRepository<DealerPaymentMethodMapping> _dealerPaymentMethodMappingRepository;
    private IRepository<DealerTransaction> _dealerTransactionRepository;
    private IRepository<DealerCollection> _dealerCollectionRepository;
    private IRepository<DealerFinancialInstrument> _dealerFinancialInstrumentRepository;
    private IRepository<DealerFinanceAuditLog> _dealerFinanceAuditLogRepository;
    private IRepository<DealerTransactionAllocation> _dealerTransactionAllocationRepository;
    private IDataProviderManager _dataProviderManager;

    private readonly List<int> _createdDealerIds = [];

    [OneTimeSetUp]
    public void SetUp()
    {
        _dealerService = GetService<IDealerService>();
        _dealerRepository = GetService<IRepository<DealerInfo>>();
        _dealerFinancialProfileRepository = GetService<IRepository<DealerFinancialProfile>>();
        _dealerCustomerMappingRepository = GetService<IRepository<DealerCustomerMapping>>();
        _dealerPaymentMethodMappingRepository = GetService<IRepository<DealerPaymentMethodMapping>>();
        _dealerTransactionRepository = GetService<IRepository<DealerTransaction>>();
        _dealerCollectionRepository = GetService<IRepository<DealerCollection>>();
        _dealerFinancialInstrumentRepository = GetService<IRepository<DealerFinancialInstrument>>();
        _dealerFinanceAuditLogRepository = GetService<IRepository<DealerFinanceAuditLog>>();
        _dealerTransactionAllocationRepository = GetService<IRepository<DealerTransactionAllocation>>();
        _dataProviderManager = GetService<IDataProviderManager>();
    }

    [SetUp]
    public async Task PerTestSetUp()
    {
        await EnsureFinanceTablesAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        foreach (var dealerId in _createdDealerIds.Distinct().ToList())
        {
            await _dealerTransactionAllocationRepository.DeleteAsync(item => item.DealerId == dealerId);
            await _dealerFinanceAuditLogRepository.DeleteAsync(item => item.DealerId == dealerId);
            await _dealerFinancialInstrumentRepository.DeleteAsync(item => item.DealerId == dealerId);
            await _dealerCollectionRepository.DeleteAsync(item => item.DealerId == dealerId);
            await _dealerTransactionRepository.DeleteAsync(item => item.DealerId == dealerId);
            await _dealerPaymentMethodMappingRepository.DeleteAsync(item => item.DealerId == dealerId);
            await _dealerCustomerMappingRepository.DeleteAsync(item => item.DealerId == dealerId);
            await _dealerFinancialProfileRepository.DeleteAsync(item => item.DealerId == dealerId);
            await _dealerRepository.DeleteAsync(item => item.Id == dealerId);
        }

        _createdDealerIds.Clear();
    }

    [Test]
    public async Task CanCreateAutomaticAllocationsUsingFifoAndExistingCoverage()
    {
        var dealer = await CreateDealerAsync();
        var createdOnUtc = DateTime.UtcNow;

        var debitTransaction1 = await InsertTransactionAsync(dealer.Id, DealerTransactionType.OpenAccountOrder,
            DealerTransactionDirection.Debit, 100m, createdOnUtc.AddMinutes(-30));
        var debitTransaction2 = await InsertTransactionAsync(dealer.Id, DealerTransactionType.ManualDebitAdjustment,
            DealerTransactionDirection.Debit, 50m, createdOnUtc.AddMinutes(-20));
        _ = await InsertTransactionAsync(dealer.Id, DealerTransactionType.OpenAccountOrder,
            DealerTransactionDirection.Debit, 30m, createdOnUtc.AddMinutes(-10));

        var previousCreditTransaction = await InsertTransactionAsync(dealer.Id, DealerTransactionType.OpenAccountCollection,
            DealerTransactionDirection.Credit, 15m, createdOnUtc.AddMinutes(-5));

        await _dealerService.InsertDealerTransactionAllocationAsync(new DealerTransactionAllocation
        {
            DealerId = dealer.Id,
            CreditDealerTransactionId = previousCreditTransaction.Id,
            DebitDealerTransactionId = debitTransaction2.Id,
            Amount = 15m,
            CreatedByCustomerId = 1,
            CreatedOnUtc = createdOnUtc.AddMinutes(-5)
        });

        var collection = await InsertCollectionAsync(dealer.Id, 120m, createdOnUtc);
        var creditTransaction = await InsertTransactionAsync(dealer.Id, DealerTransactionType.OpenAccountCollection,
            DealerTransactionDirection.Credit, 120m, createdOnUtc);

        var allocations = await _dealerService.CreateAutomaticAllocationsAsync(dealer.Id, creditTransaction.Id, 120m,
            collection.Id, 1, createdOnUtc);

        allocations.Should().HaveCount(2);

        allocations[0].DebitDealerTransactionId.Should().Be(debitTransaction1.Id);
        allocations[0].Amount.Should().Be(100m);
        allocations[0].DealerCollectionId.Should().Be(collection.Id);

        allocations[1].DebitDealerTransactionId.Should().Be(debitTransaction2.Id);
        allocations[1].Amount.Should().Be(20m);
        allocations[1].DealerCollectionId.Should().Be(collection.Id);

        var persistedAllocations = await _dealerService.SearchDealerTransactionAllocationsAsync(dealerId: dealer.Id,
            creditDealerTransactionId: creditTransaction.Id, pageSize: int.MaxValue);

        persistedAllocations.Should().HaveCount(2);
        persistedAllocations.Sum(item => item.Amount).Should().Be(120m);
    }

    [Test]
    public async Task CanCancelAllocationsByCollection()
    {
        var dealer = await CreateDealerAsync();
        var createdOnUtc = DateTime.UtcNow;

        _ = await InsertTransactionAsync(dealer.Id, DealerTransactionType.OpenAccountOrder,
            DealerTransactionDirection.Debit, 80m, createdOnUtc.AddMinutes(-15));

        var collection = await InsertCollectionAsync(dealer.Id, 50m, createdOnUtc);
        var creditTransaction = await InsertTransactionAsync(dealer.Id, DealerTransactionType.OpenAccountCollection,
            DealerTransactionDirection.Credit, 50m, createdOnUtc);

        var allocations = await _dealerService.CreateAutomaticAllocationsAsync(dealer.Id, creditTransaction.Id, 50m,
            collection.Id, 1, createdOnUtc);

        allocations.Should().HaveCount(1);

        var cancelledOnUtc = createdOnUtc.AddMinutes(1);
        await _dealerService.CancelDealerTransactionAllocationsByCollectionAsync(collection.Id, 1, cancelledOnUtc);

        var activeAllocations = await _dealerService.SearchDealerTransactionAllocationsAsync(dealerCollectionId: collection.Id,
            activeOnly: true, pageSize: int.MaxValue);
        activeAllocations.Should().BeEmpty();

        var allAllocations = await _dealerService.SearchDealerTransactionAllocationsAsync(dealerCollectionId: collection.Id,
            pageSize: int.MaxValue);
        allAllocations.Should().HaveCount(1);
        allAllocations[0].CancelledByCustomerId.Should().Be(1);
        allAllocations[0].CancelledOnUtc.Should().Be(cancelledOnUtc);
    }

    private async Task<DealerInfo> CreateDealerAsync()
    {
        var dealer = new DealerInfo
        {
            Name = $"Allocation Test Dealer {Guid.NewGuid():N}",
            StoreId = 1,
            Active = true,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _dealerService.InsertDealerAsync(dealer);
        _createdDealerIds.Add(dealer.Id);

        return dealer;
    }

    private async Task EnsureFinanceTablesAsync()
    {
        var dataProvider = _dataProviderManager.DataProvider;

        await dataProvider.ExecuteNonQueryAsync("""
            CREATE TABLE IF NOT EXISTS "DealerFinancialInstrument" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_DealerFinancialInstrument" PRIMARY KEY AUTOINCREMENT,
                "DealerId" INTEGER NOT NULL,
                "DealerCollectionId" INTEGER NULL,
                "CustomerId" INTEGER NULL,
                "InstrumentTypeId" INTEGER NOT NULL,
                "InstrumentStatusId" INTEGER NOT NULL,
                "Amount" DECIMAL(18,4) NOT NULL DEFAULT 0,
                "InstrumentNo" TEXT NULL,
                "IssueDateUtc" TEXT NULL,
                "DueDateUtc" TEXT NULL,
                "BankName" TEXT NULL,
                "BranchName" TEXT NULL,
                "AccountNo" TEXT NULL,
                "DrawerName" TEXT NULL,
                "Note" TEXT NULL,
                "CreatedByCustomerId" INTEGER NOT NULL,
                "CreatedOnUtc" TEXT NOT NULL,
                "UpdatedOnUtc" TEXT NULL
            );
            """);

        await dataProvider.ExecuteNonQueryAsync("""
            CREATE TABLE IF NOT EXISTS "DealerFinanceAuditLog" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_DealerFinanceAuditLog" PRIMARY KEY AUTOINCREMENT,
                "DealerId" INTEGER NOT NULL,
                "EntityTypeId" INTEGER NOT NULL,
                "DealerCollectionId" INTEGER NULL,
                "DealerFinancialInstrumentId" INTEGER NULL,
                "ActionTypeId" INTEGER NOT NULL,
                "StatusBeforeId" INTEGER NULL,
                "StatusAfterId" INTEGER NULL,
                "Note" TEXT NULL,
                "PerformedByCustomerId" INTEGER NULL,
                "PerformedOnUtc" TEXT NOT NULL
            );
            """);

        await dataProvider.ExecuteNonQueryAsync("""
            CREATE TABLE IF NOT EXISTS "DealerTransactionAllocation" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_DealerTransactionAllocation" PRIMARY KEY AUTOINCREMENT,
                "DealerId" INTEGER NOT NULL,
                "DealerCollectionId" INTEGER NULL,
                "CreditDealerTransactionId" INTEGER NOT NULL,
                "DebitDealerTransactionId" INTEGER NOT NULL,
                "Amount" DECIMAL(18,4) NOT NULL DEFAULT 0,
                "CreatedByCustomerId" INTEGER NOT NULL,
                "CreatedOnUtc" TEXT NOT NULL,
                "CancelledByCustomerId" INTEGER NULL,
                "CancelledOnUtc" TEXT NULL
            );
            """);
    }

    private async Task<DealerTransaction> InsertTransactionAsync(int dealerId, DealerTransactionType transactionType,
        DealerTransactionDirection direction, decimal amount, DateTime createdOnUtc)
    {
        var transaction = new DealerTransaction
        {
            DealerId = dealerId,
            TransactionTypeId = (int)transactionType,
            DirectionId = (int)direction,
            SourceTypeId = (int)DealerTransactionSourceType.ManualAdjustment,
            Amount = amount,
            CreatedOnUtc = createdOnUtc,
            Note = "test"
        };

        await _dealerService.InsertDealerTransactionAsync(transaction);
        return transaction;
    }

    private async Task<DealerCollection> InsertCollectionAsync(int dealerId, decimal amount, DateTime createdOnUtc)
    {
        var collection = new DealerCollection
        {
            DealerId = dealerId,
            CollectionMethodId = (int)DealerCollectionMethod.Cash,
            CollectionStatusId = (int)DealerCollectionStatus.Posted,
            Amount = amount,
            CollectionDateUtc = createdOnUtc,
            CreatedByCustomerId = 1,
            CreatedOnUtc = createdOnUtc,
            ReferenceNo = Guid.NewGuid().ToString("N")
        };

        await _dealerService.InsertDealerCollectionAsync(collection);
        return collection;
    }
}
