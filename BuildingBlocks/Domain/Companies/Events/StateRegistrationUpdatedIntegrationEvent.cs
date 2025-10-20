using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Domain.Companies.Events;

/// <summary>
/// Represents an integration event that occurs when a company's state registration is updated.
/// </summary>
/// <remarks>This event is typically used to notify other systems or services about changes to a company's state
/// registration.</remarks>
public sealed class StateRegistrationUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid CompanyId { get; }
    public string StateRegistration { get; }
    public StateRegistrationUpdatedIntegrationEvent(Guid? tenantId, Guid companyId, string stateRegistration)
        : base(tenantId)
    {
        CompanyId = companyId;
        StateRegistration = stateRegistration;
    }
}
