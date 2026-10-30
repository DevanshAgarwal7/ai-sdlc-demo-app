using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ApprovalServiceTests
{
    private static Invoice InvoiceWith(string id, InvoiceStatus status, decimal amount = 100m) =>
        new() { Id = id, CustomerName = "Test School", Amount = amount, Status = status };

    private static ApprovalService ServiceFor(params Invoice[] invoices) =>
        new(new FakeInvoiceRepository(invoices));

    [Fact]
    public void Submit_MovesDraftToPendingApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.Draft);

        var result = ServiceFor(invoice).Submit("INV-1");

        Assert.Equal(InvoiceStatus.PendingApproval, result.Status);
    }

    [Fact]
    public void Submit_RejectsInvoiceThatIsNotADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Approved));

        Assert.Throws<InvalidOperationException>(() => service.Submit("INV-1"));
    }

    [Fact]
    public void Approve_SetsStatusApproverAndTimestamp()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Approve("INV-1", "demo.approver", "Approver");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("demo.approver", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Approve_RejectsInvoiceThatIsStillADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Draft));

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.approver", "Approver"));
    }

    [Fact]
    public void Approve_ThrowsWhenInvoiceDoesNotExist()
    {
        var service = ServiceFor();

        Assert.Throws<KeyNotFoundException>(() => service.Approve("INV-404", "demo.approver", "Approver"));
    }

    [Fact]
    public void Approve_AllowsManagerUpTo50000()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 50000m);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "manager.one", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("manager.one", result.DecidedBy);
    }

    [Fact]
    public void Approve_RejectsManagerAbove50000()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 50001m);
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "manager.one", "Manager"));

        Assert.Contains("50,000", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
    }

    [Fact]
    public void Approve_RejectsManagerAbove50000_WithoutChangingInvoiceState()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 50001m);
        invoice.DecidedBy = "prior.approver";
        invoice.DecidedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        invoice.DecisionNote = "Original note";
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "manager.one", "Manager"));

        Assert.Contains("50,000", ex.Message);
        Assert.Equal(50001m, invoice.Amount);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Equal("prior.approver", invoice.DecidedBy);
        Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), invoice.DecidedAt);
        Assert.Equal("Original note", invoice.DecisionNote);
    }

    [Fact]
    public void Approve_AllowsDirectorUpTo500000()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500000m);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "director.one", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("director.one", result.DecidedBy);
    }

    [Fact]
    public void Approve_RejectsDirectorAbove500000()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500001m);
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "director.one", "Director"));

        Assert.Contains("500,000", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
    }

    [Fact]
    public void Approve_RejectsDirectorAbove500000_WithoutChangingInvoiceState()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500001m);
        invoice.DecidedBy = "prior.director";
        invoice.DecidedAt = new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc);
        invoice.DecisionNote = "Original note";
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "director.one", "Director"));

        Assert.Contains("500,000", ex.Message);
        Assert.Equal(500001m, invoice.Amount);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Equal("prior.director", invoice.DecidedBy);
        Assert.Equal(new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc), invoice.DecidedAt);
        Assert.Equal("Original note", invoice.DecisionNote);
    }

    [Fact]
    public void Approve_RequiresTwoDistinctApproversAbove500000()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500001m);
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.Approve("INV-1", "manager.one", "Manager", "manager.one", "Director"));

        Assert.Contains("same user", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
    }

    [Fact]
    public void Approve_RequiresSecondApproverToBeDirectorAbove500000()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500001m);
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.Approve("INV-1", "manager.one", "Manager", "director.two", "Manager"));

        Assert.Contains("second approver", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
    }

    [Fact]
    public void Approve_AllowsTwoDistinctDirectorApproversAbove500000()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500001m);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "manager.one", "Manager", "director.two", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("manager.one", result.DecidedBy);
    }

    [Fact]
    public void Reject_RecordsReasonAndApprover()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Reject("INV-1", "demo.approver", "Duplicate invoice");

        Assert.Equal(InvoiceStatus.Rejected, result.Status);
        Assert.Equal("Duplicate invoice", result.DecisionNote);
    }

    [Fact]
    public void Reject_RequiresAReason()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        Assert.Throws<ArgumentException>(() => service.Reject("INV-1", "demo.approver", "  "));
    }

    [Fact]
    public void Approve_AllowsManagerBelowThresholdBoundary()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 49999m);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "manager.one", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("manager.one", result.DecidedBy);
    }

    [Fact]
    public void Approve_AllowsDirectorBelowThresholdBoundary()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 499999m);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "director.one", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("director.one", result.DecidedBy);
    }

    [Fact]
    public void Approve_RejectsDuplicateApproverAbove500000_WithoutChangingInvoiceState()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500001m);
        invoice.DecidedBy = "prior.approver";
        invoice.DecidedAt = new DateTime(2024, 3, 3, 0, 0, 0, DateTimeKind.Utc);
        invoice.DecisionNote = "Original note";
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.Approve("INV-1", "manager.one", "Manager", "manager.one", "Director"));

        Assert.Contains("same user", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(500001m, invoice.Amount);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Equal("prior.approver", invoice.DecidedBy);
        Assert.Equal(new DateTime(2024, 3, 3, 0, 0, 0, DateTimeKind.Utc), invoice.DecidedAt);
        Assert.Equal("Original note", invoice.DecisionNote);
    }

    [Fact]
    public void Approve_RejectsNonDirectorSecondApproverAbove500000_WithoutChangingInvoiceState()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500001m);
        invoice.DecidedBy = "prior.director";
        invoice.DecidedAt = new DateTime(2024, 4, 4, 0, 0, 0, DateTimeKind.Utc);
        invoice.DecisionNote = "Original note";
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.Approve("INV-1", "manager.one", "Manager", "director.two", "Manager"));

        Assert.Contains("second approver", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(500001m, invoice.Amount);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Equal("prior.director", invoice.DecidedBy);
        Assert.Equal(new DateTime(2024, 4, 4, 0, 0, 0, DateTimeKind.Utc), invoice.DecidedAt);
        Assert.Equal("Original note", invoice.DecisionNote);
    }
}
