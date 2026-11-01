namespace InvoiceApp.Domain;

/// <summary>
/// Moves an invoice through its lifecycle: Draft -> PendingApproval -> Approved or Rejected.
/// One invoice at a time, decided as a whole by a single approver.
/// </summary>
public class ApprovalService
{
    private readonly IInvoiceRepository _repository;

    public ApprovalService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public Invoice Submit(string invoiceId)
    {
        var invoice = Find(invoiceId);

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Invoice {invoiceId} is {invoice.Status}; only drafts can be submitted.");
        }

        invoice.Status = InvoiceStatus.PendingApproval;
        invoice.DecidedBy = null;
        invoice.DecidedAt = null;
        invoice.DecisionNote = null;
        invoice.PendingSecondApprover = null;
        invoice.PendingSecondApproverRole = null;
        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Approve(string invoiceId, string approver)
        => Approve(invoiceId, approver, string.Empty, null, null);

    public Invoice Approve(string invoiceId, string approver, string role)
        => Approve(invoiceId, approver, role, null, null);

    public Invoice Approve(string invoiceId, string approver, string role, string? secondApprover, string? secondRole)
    {
        var invoice = Find(invoiceId);
        EnsurePending(invoice);

        if (invoice.Amount > 500000m)
        {
            ProcessHighValueApproval(invoice, approver, role, secondApprover, secondRole);
            _repository.Update(invoice);
            return invoice;
        }

        ValidateSingleApprover(invoice, approver, role);

        invoice.Status = InvoiceStatus.Approved;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        invoice.DecisionNote = null;
        invoice.PendingSecondApprover = null;
        invoice.PendingSecondApproverRole = null;
        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Reject(string invoiceId, string approver, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required to reject an invoice.", nameof(reason));
        }

        var invoice = Find(invoiceId);
        EnsurePending(invoice);

        invoice.Status = InvoiceStatus.Rejected;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        invoice.DecisionNote = reason;
        invoice.PendingSecondApprover = null;
        invoice.PendingSecondApproverRole = null;
        _repository.Update(invoice);
        return invoice;
    }

    private Invoice Find(string invoiceId) =>
        _repository.GetById(invoiceId)
        ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");

    private static void EnsurePending(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                $"Invoice {invoice.Id} is {invoice.Status}; only pending invoices can be decided.");
        }
    }

    private static void ValidateSingleApprover(Invoice invoice, string approver, string role)
    {
        if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase)
            && invoice.Amount > 50000m)
        {
            throw new InvalidOperationException(
                $"Managers cannot approve invoices above 50,000. Invoice {invoice.Id} is {invoice.Amount:C}.");
        }

        if (string.Equals(role, "Director", StringComparison.OrdinalIgnoreCase)
            && invoice.Amount > 500000m)
        {
            throw new InvalidOperationException(
                "Directors can approve up to 500,000. Invoices above 500,000 require two distinct approvers, and the second approver must be a Director.");
        }
    }

    private static void ProcessHighValueApproval(
        Invoice invoice,
        string approver,
        string role,
        string? secondApprover,
        string? secondRole)
    {
        if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase)
            && invoice.Amount > 50000m)
        {
            throw new InvalidOperationException(
                $"Managers cannot approve invoices above 50,000. Invoice {invoice.Id} is {invoice.Amount:C}.");
        }

        if (!string.IsNullOrWhiteSpace(invoice.PendingSecondApprover))
        {
            if (string.Equals(invoice.DecidedBy, approver, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The same user may not count as both approvers for a single invoice.");
            }

            if (!string.Equals(invoice.PendingSecondApprover, approver, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The second approver must be the pending Director approver for this invoice.");
            }

            if (!string.Equals(role, "Director", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The second approver for invoices above 500,000 must be a Director.");
            }

            invoice.Status = InvoiceStatus.Approved;
            invoice.DecidedBy = approver;
            invoice.DecidedAt = DateTime.UtcNow;
            invoice.DecisionNote = null;
            invoice.PendingSecondApprover = null;
            invoice.PendingSecondApproverRole = null;
            return;
        }

        if (!string.Equals(role, "Director", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invoices above 500,000 require two distinct approvers, and the second approver must be a Director.");
        }

        if (string.IsNullOrWhiteSpace(secondApprover))
        {
            throw new InvalidOperationException(
                "Invoices above 500,000 require two distinct approvers, and the second approver must be a Director.");
        }

        if (string.Equals(approver, secondApprover, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The same user may not count as both approvers for a single invoice.");
        }

        if (!string.Equals(secondRole, "Director", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The second approver for invoices above 500,000 must be a Director.");
        }

        invoice.Status = InvoiceStatus.PendingApproval;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        invoice.DecisionNote = "Awaiting second Director approval.";
        invoice.PendingSecondApprover = secondApprover;
        invoice.PendingSecondApproverRole = secondRole;
    }
}
