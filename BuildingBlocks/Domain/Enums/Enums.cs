using System.ComponentModel;

namespace BuildingBlocks.Domain.Enums;

public enum RegistrationStatus
{
    [Description("Active (Ativo): The company is legally registered, fully compliant, and authorized to operate and conduct business activities. It has an active registration status and is current on tax filings and licenses.")]
    Active,

    [Description("Pending Transfer / Pending Archival (Transferência Pendente / Arquivamento Pendente): Transitional statuses reflecting that the registration is in the process of being transferred between managing units or entities.")]
    Pending,

    [Description("Annulled (Anulado): A registration that was issued in error or revoked due to inaccuracies or non-compliance.")]
    Annulled,

    [Description("Retired or Dissolved (Baixado / Encerrado): The company has ceased operations either voluntarily or due to administrative or legal proceedings such as bankruptcy, dissolution, or formal closure procedures.")]
    Retired,

    [Description("Inactive (Inativo): The company is still legally registered but is not currently authorized to operate. This status can arise from failure to comply with requirements such as tax payments, filing annual reports, or maintaining a registered agent.")]
    Inactive,

    [Description("Lapsed (Vencido): The company’s registration or license has expired or is overdue for renewal, often triggering this status until compliance requirements are met again.")]
    Lapsed,

    [Description("Merged (Incorporado / Fundido): The company no longer exists as an independent entity due to merger or acquisition, with its operations absorbed by another entity.")]
    Merged,

    [Description("Duplicate (Duplicado): Occurs when multiple registrations exist for the same legal entity, often an error corrected by marking redundant records as duplicates.")]
    Duplicate,

    [Description("The status is unknown or not yet verified.")]
    Unknown
}

/*
 While each state may have some specific codes, typical SEFAZ status enums include:
100 – Autorizado o uso da NF-e (Invoice Authorized)
101 – Cancelamento homologado (Cancellation Approved)
102 – Inutilização homologada (Number Range Voided)
110 – Evento registrado e vinculado a NF-e (Event Registered)
135 – Ciência da operação (Operation Knowledge Confirmed)
210 – Lote em processamento (Batch Processing)
301 – Denied or rejected statuses (e.g., Rejeição)
999 – Erro ou status desconhecido (Error or unknown status)
 */
