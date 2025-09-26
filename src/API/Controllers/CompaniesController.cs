using API.Contracts.Companies;
using Application.Companies;
using MediatR;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization; // uncomment when auth is in place

namespace API.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CompaniesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<Guid>> Register([FromBody] RegisterCompanyRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(new RegisterCompanyCommand(request.LegalName, request.TaxId, request.Email), ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyResponse>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetCompanyByIdQuery(id), ct);
        return Ok(new CompanyResponse(dto.Id, dto.LegalName, dto.TaxId, dto.Email));
    }

    [HttpPost("{id:guid}/state-registrations")]
    public async Task<IActionResult> AddOrUpdateStateRegistration([FromRoute] Guid id, [FromBody] AddOrUpdateStateRegistrationRequest req, CancellationToken ct)
    {
        await _mediator.Send(new AddOrUpdateStateRegistrationCommand(id, req.Uf, req.Ie, req.Status, req.RegimeTributario, req.LastCheckedAt), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/state-registrations/refresh")]
    // [Authorize(Policy = "SefazQuery")] // enable after you wire JWT
    public async Task<IActionResult> RefreshSefaz([FromRoute] Guid id, [FromBody] RefreshSefazRequest req, CancellationToken ct)
    {
        await _mediator.Send(new RefreshStateRegistrationFromSefazCommand(id, req.Uf), ct);
        return NoContent();
    }
}
