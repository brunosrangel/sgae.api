namespace Sgae.Application.Upload.DTOs;

/// <summary>
/// Modelo de requisição para upload de imagem no formato Base64.
/// </summary>
public class UploadImagemBase64RequestDto
{
    /// <summary>
    /// Conteúdo da imagem em Base64 (suporta com ou sem prefixo 'data:image/...;base64,').
    /// </summary>
    public string Base64Data { get; set; } = null!;

    /// <summary>
    /// Nome original do arquivo (ex: 'foto_buzios.jpg').
    /// </summary>
    public string NomeArquivo { get; set; } = null!;

    /// <summary>
    /// Categoria litúrgica ou de sistema (ex: 'FotoBuzios', 'FotoPerfil', 'Comprovante', 'Atendimento').
    /// </summary>
    public string? Categoria { get; set; } = "Geral";

    /// <summary>
    /// Descrição ou legenda opcional da imagem.
    /// </summary>
    public string? Descricao { get; set; }

    /// <summary>
    /// Identificador opcional da entidade relacionada (Atendimento, Lead, Usuário, etc.).
    /// </summary>
    public Guid? EntidadeRelacionadaId { get; set; }
}
