using System;

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
}
