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
        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Approve(string invoiceId, string approver)
        => Approve(invoiceId, approver, string.Empty);

    public Invoice Approve(string invoiceId, string approver, string role)
    {
        var invoice = Find(invoiceId);
        EnsurePending(invoice);
        EnsureRoleMayApprove(invoice, role);

        invoice.Status = InvoiceStatus.Approved;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
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

    private static void EnsureRoleMayApprove(Invoice invoice, string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return;
        }

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
                $"Directors cannot approve invoices above 500,000. Invoice {invoice.Id} is {invoice.Amount:C}.");
        }
    }
}
