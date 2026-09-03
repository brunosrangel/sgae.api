using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Representa um anexo multimídia vinculado a um Atendimento Espiritual / Jogo de Búzios.
/// Armazena fotos de caídas de búzios, anotações manuscritas, digitalizações e documentos.
/// </summary>
public class AnexoAtendimento : BaseEntity
{
    private AnexoAtendimento() { }

    public AnexoAtendimento(
        Guid atendimentoId,
        string nomeArquivo,
        string tipoArquivo,
        long tamanhoBytes,
        string? base64Data = null,
        string? legenda = null,
        int rotacaoGraus = 0,
        string? categoria = null)
    {
        if (atendimentoId == Guid.Empty)
            throw new ArgumentException("O anexo deve estar vinculado a um Atendimento válido.");

        if (string.IsNullOrWhiteSpace(nomeArquivo))
            throw new ArgumentException("O nome do arquivo é obrigatório.");

        AtendimentoId = atendimentoId;
        NomeArquivo = nomeArquivo.Trim();
        TipoArquivo = !string.IsNullOrWhiteSpace(tipoArquivo) ? tipoArquivo.Trim() : "application/octet-stream";
        TamanhoBytes = tamanhoBytes;
        Base64Data = base64Data;
        Legenda = legenda?.Trim();
        RotacaoGraus = rotacaoGraus;
        Categoria = !string.IsNullOrWhiteSpace(categoria) ? categoria.Trim() : "FotoBuzios";
    }

    public Guid AtendimentoId { get; private set; }
    public virtual Atendimento Atendimento { get; private set; } = null!;

    public string NomeArquivo { get; private set; } = null!;
    public string TipoArquivo { get; private set; } = null!;
    public long TamanhoBytes { get; private set; }
    public string? Base64Data { get; private set; }
    public string? Legenda { get; private set; }
    public int RotacaoGraus { get; private set; }
    public string? Categoria { get; private set; }

    public void UpdateLegenda(string? legenda)
    {
        Legenda = legenda?.Trim();
        RegisterUpdate();
    }

    public void UpdateRotacao(int graus)
    {
        RotacaoGraus = graus % 360;
        RegisterUpdate();
    }

    public void UpdateConteudo(string base64Data, long tamanhoBytes)
    {
        Base64Data = base64Data;
        TamanhoBytes = tamanhoBytes;
        RegisterUpdate();
    }
}
