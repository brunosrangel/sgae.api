namespace Sgae.Domain.Common;

/// <summary>
/// Helper para garantir que todos os objetos DateTime estejam no padrão UTC (DateTimeKind.Utc).
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Garante que o DateTime informado seja do tipo Utc.
    /// Se for Unspecified, força Utc. Se for Local, converte para Utc.
    /// </summary>
    public static DateTime EnsureUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
        {
            return dateTime;
        }

        if (dateTime.Kind == DateTimeKind.Local)
        {
            return dateTime.ToUniversalTime();
        }

        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    /// <summary>
    /// Garante que o DateTime nulo ou preenchido informado seja do tipo Utc.
    /// </summary>
    public static DateTime? EnsureUtc(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return null;
        }

        return EnsureUtc(dateTime.Value);
    }

    /// <summary>
    /// Método de extensão para garantir que o DateTime informado seja do tipo Utc.
    /// </summary>
    public static DateTime ToUtc(this DateTime dateTime)
    {
        return EnsureUtc(dateTime);
    }

    /// <summary>
    /// Método de extensão para garantir que o DateTime? informado seja do tipo Utc.
    /// </summary>
    public static DateTime? ToUtc(this DateTime? dateTime)
    {
        return EnsureUtc(dateTime);
    }

    /// <summary>
    /// Tenta converter e analisar strings de data (e opcionalmente horário) para um objeto DateTime UTC.
    /// Suporta formatos como "yyyy-MM-dd" e "HH:mm" ou ISO 8601 completos.
    /// </summary>
    public static DateTime ParseUtc(string dateStr, string? timeStr = null)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
        {
            throw new ArgumentException("A string de data não pode ser vazia.");
        }

        // Se a string de data já for um formato ISO 8601 completo (ex: contém 'T' ou 'Z' ou espaço e hora)
        if (DateTime.TryParse(dateStr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedFull))
        {
            // Se um horário separado também foi enviado, vamos combiná-lo se a data não possuir hora
            if (parsedFull.TimeOfDay == TimeSpan.Zero && !string.IsNullOrWhiteSpace(timeStr))
            {
                if (TimeSpan.TryParse(timeStr, out var ts))
                {
                    parsedFull = parsedFull.Date.Add(ts);
                }
            }
            return EnsureUtc(parsedFull);
        }

        // Se as duas partes foram passadas separadamente
        if (!string.IsNullOrWhiteSpace(timeStr))
        {
            var combinedStr = $"{dateStr} {timeStr}";
            if (DateTime.TryParse(combinedStr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedCombined))
            {
                return EnsureUtc(parsedCombined);
            }
        }

        // Fallback para TryParse com a data apenas
        if (DateTime.TryParse(dateStr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDateOnly))
        {
            return EnsureUtc(parsedDateOnly);
        }

        throw new FormatException($"Não foi possível converter as strings de data '{dateStr}' e horário '{timeStr}' em um DateTime válido.");
    }

    /// <summary>
    /// Verifica se uma string de data (ou data e horário combinados) está em um formato ISO 8601 válido (ex: yyyy-MM-dd ou completo).
    /// </summary>
    public static bool IsValidIso8601(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
        {
            return false;
        }

        // Validar formato padrão yyyy-MM-dd usando DateTime.TryParseExact
        string[] formats = { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss" };
        return DateTime.TryParseExact(dateStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _);
    }
}
