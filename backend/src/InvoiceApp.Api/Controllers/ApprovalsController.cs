using InvoiceApp.Api.Auth;
using InvoiceApp.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireRole("Approver", "Admin", "Manager", "Director")]
public class ApprovalsController : ApiControllerBase
{
    private readonly ApprovalService _approvals;

    public ApprovalsController(ApprovalService approvals)
    {
        _approvals = approvals;
    }

    public record RejectRequest(string Reason);
    public record ApproveRequest(string? SecondApprover, string? SecondRole);

    [HttpPost("{invoiceId}/approve")]
    public ActionResult<Invoice> Approve(string invoiceId, [FromBody] ApproveRequest? request) =>
        Handle(() => _approvals.Approve(
            invoiceId,
            HttpContext.GetSession().Username,
            HttpContext.GetSession().Role,
            request?.SecondApprover,
            request?.SecondRole));

    [HttpPost("{invoiceId}/reject")]
    public ActionResult<Invoice> Reject(string invoiceId, [FromBody] RejectRequest request) =>
        Handle(() => _approvals.Reject(invoiceId, HttpContext.GetSession().Username, request.Reason));
}
