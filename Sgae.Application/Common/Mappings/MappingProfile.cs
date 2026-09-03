using AutoMapper;
using Sgae.Application.Agendamentos.DTOs;
using Sgae.Application.Atendimentos.DTOs;
using Sgae.Application.Leads.DTOs;
using Sgae.Application.Perfis.DTOs;
using Sgae.Application.Sacerdotes.DTOs;
using Sgae.Domain.Entities;

namespace Sgae.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeamento de Sacerdote para SacerdoteDto
        CreateMap<Sacerdote, SacerdoteDto>();

        // Mapeamento de AnexoAtendimento para AnexoAtendimentoResponseDto
        CreateMap<AnexoAtendimento, AnexoAtendimentoResponseDto>()
            .ForMember(dest => dest.TemConteudoBinario, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Base64Data)))
            .ForMember(dest => dest.UrlDownload, opt => opt.MapFrom(src => $"/api/Atendimentos/{src.AtendimentoId}/anexos/{src.Id}/download"));

        // Mapeamento de Atendimento para AtendimentoResponseDto
        CreateMap<Atendimento, AtendimentoResponseDto>()
            .ForMember(dest => dest.SacerdoteNome, opt => opt.MapFrom(src => src.Sacerdote != null ? src.Sacerdote.Nome : string.Empty))
            .ForMember(dest => dest.ConsulenteNome, opt => opt.MapFrom(src => src.Consulente != null ? src.Consulente.Nome : string.Empty))
            .ForMember(dest => dest.ConsulenteTelefone, opt => opt.MapFrom(src => src.Consulente != null ? src.Consulente.Telefone : null))
            .ForMember(dest => dest.ConsulenteEmail, opt => opt.MapFrom(src => src.Consulente != null ? src.Consulente.Email : null))
            .ForMember(dest => dest.QuantidadeAnexos, opt => opt.MapFrom(src => src.Anexos != null ? src.Anexos.Count : 0))
            .ForMember(dest => dest.Anexos, opt => opt.MapFrom(src => src.Anexos));

        // Mapeamento de PerfilConsulente para PerfilConsulenteDto
        CreateMap<PerfilConsulente, PerfilConsulenteDto>();

        // Mapeamento de AtendimentoEspiritual para AtendimentoEspiritualDto (Atendimentos)
        CreateMap<AtendimentoEspiritual, Sgae.Application.Atendimentos.DTOs.AtendimentoEspiritualDto>()
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo))
            .ForMember(dest => dest.TipoDescricao, opt => opt.MapFrom(src => src.Tipo.ToString()));

        // Mapeamento de AtendimentoEspiritual para AgendamentoAtendimentoEspiritualDto (Agendamentos)
        CreateMap<AtendimentoEspiritual, Sgae.Application.Agendamentos.DTOs.AgendamentoAtendimentoEspiritualDto>()
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()));

        // Mapeamento de Acompanhamento para AcompanhamentoDto
        CreateMap<Acompanhamento, AcompanhamentoDto>();

        // Mapeamento de AuditLog para AuditLogDto
        CreateMap<AuditLog, AuditLogDto>();

        // Mapeamento de LeadHistorico para LeadHistoricoDto
        CreateMap<LeadHistorico, LeadHistoricoDto>();

        // Mapeamento bidirecional ou unidirecional de Lead para LeadDto
        CreateMap<Lead, LeadDto>()
            .ForMember(dest => dest.NomeCompleto, opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Uf, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.DataCadastro, opt => opt.MapFrom(src => src.DataContato))
            .ForMember(dest => dest.CanalCaptacaoNome, opt => opt.MapFrom(src => src.CanalCaptacao != null ? src.CanalCaptacao.Nome : null))
            .ForMember(dest => dest.Origem, opt => opt.MapFrom(src => src.Origem.ToString()))
            .ForMember(dest => dest.Historico, opt => opt.MapFrom(src => src.Historico))
            .ForMember(dest => dest.Perfil, opt => opt.MapFrom(src => src.Perfil));

        // Mapeamento enriquecido de Agendamento buscando campos da entidade navegacional 'Lead', 'Sacerdote' e 'ServicoConsulta'
        CreateMap<Agendamento, AgendamentoDto>()
            .ForMember(dest => dest.LeadNome, opt => opt.MapFrom(src => src.Lead != null ? src.Lead.Nome : string.Empty))
            .ForMember(dest => dest.LeadTelefone, opt => opt.MapFrom(src => src.Lead != null ? src.Lead.Telefone : string.Empty))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.DataHora.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.Horario, opt => opt.MapFrom(src => src.DataHora.ToString("HH:mm")))
            .ForMember(dest => dest.Modalidade, opt => opt.MapFrom(src => src.Modalidade.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SacerdoteId, opt => opt.MapFrom(src => src.SacerdoteId))
            .ForMember(dest => dest.Sacerdote, opt => opt.MapFrom(src => src.Sacerdote != null ? src.Sacerdote.Nome : null))
            .ForMember(dest => dest.SacerdoteNome, opt => opt.MapFrom(src => src.Sacerdote != null ? src.Sacerdote.Nome : null))
            .ForMember(dest => dest.ServicoConsultaId, opt => opt.MapFrom(src => src.ServicoConsultaId))
            .ForMember(dest => dest.TipoConsulta, opt => opt.MapFrom(src => src.ServicoConsulta != null ? src.ServicoConsulta.Nome : null))
            .ForMember(dest => dest.ServicoConsultaNome, opt => opt.MapFrom(src => src.ServicoConsulta != null ? src.ServicoConsulta.Nome : null))
            .ForMember(dest => dest.FormaPagamento, opt => opt.MapFrom(src => src.FormaPagamento))
            .ForMember(dest => dest.Pago, opt => opt.MapFrom(src => src.Pago))
            .ForMember(dest => dest.Observacoes, opt => opt.MapFrom(src => src.Observacoes))
            .ForMember(dest => dest.WhatsappConfirmacaoDisparada, opt => opt.MapFrom(src => src.WhatsappConfirmacaoDisparada))
            .ForMember(dest => dest.ConfigLembrete, opt => opt.MapFrom(src => src.ConfigLembrete))
            .ForMember(dest => dest.FrequenciaLembrete, opt => opt.MapFrom(src => src.FrequenciaLembrete))
            .ForMember(dest => dest.Atendimento, opt => opt.MapFrom(src => src.Atendimento));
    }
}
