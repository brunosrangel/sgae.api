using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Usuarios.DTOs;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Sgae.Domain.Exceptions;

namespace Sgae.Application.Usuarios.Commands.RegisterUsuario;

public record RegisterUsuarioCommand(
    string Nome,
    string Email,
    string Senha,
    PerfilUsuario Perfil,
    Guid? SacerdoteId = null,
    Guid? PastoralRoleId = null,
    bool PrimeiroAcesso = false
) : IRequest<UsuarioDto>;

public class RegisterUsuarioCommandValidator : AbstractValidator<RegisterUsuarioCommand>
{
    public RegisterUsuarioCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MaximumLength(150).WithMessage("O nome não pode exceder 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.")
            .MaximumLength(150).WithMessage("O e-mail não pode exceder 150 caracteres.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve possuir pelo menos 6 caracteres.");

        RuleFor(x => x.Perfil)
            .IsInEnum().WithMessage("O perfil de acesso informado é inválido.");
    }
}

public class RegisterUsuarioCommandHandler : IRequestHandler<RegisterUsuarioCommand, UsuarioDto>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUsuarioCommandHandler(
        IAppDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioDto> Handle(RegisterUsuarioCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Usuarios
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (existingUser)
        {
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");
        }

        if (request.SacerdoteId.HasValue)
        {
            var sacerdoteExists = await _context.Sacerdotes
                .AnyAsync(s => s.Id == request.SacerdoteId.Value, cancellationToken);

            if (!sacerdoteExists)
                throw new NotFoundException("Sacerdote", request.SacerdoteId.Value);
        }

        if (request.PastoralRoleId.HasValue)
        {
            var roleExists = await _context.PastoralRoles
                .AnyAsync(r => r.Id == request.PastoralRoleId.Value, cancellationToken);

            if (!roleExists)
                throw new NotFoundException("PastoralRole", request.PastoralRoleId.Value);
        }

        var passwordHash = _passwordHasher.HashPassword(request.Senha);

        var usuario = new Usuario(
            request.Nome,
            normalizedEmail,
            passwordHash,
            request.Perfil,
            request.SacerdoteId,
            request.PastoralRoleId,
            request.PrimeiroAcesso
        );

        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        string? sacerdoteNome = null;
        if (usuario.SacerdoteId.HasValue)
        {
            sacerdoteNome = (await _context.Sacerdotes.FindAsync(new object[] { usuario.SacerdoteId.Value }, cancellationToken))?.Nome;
        }

        string? pastoralRoleNome = null;
        if (usuario.PastoralRoleId.HasValue)
        {
            pastoralRoleNome = (await _context.PastoralRoles.FindAsync(new object[] { usuario.PastoralRoleId.Value }, cancellationToken))?.Nome;
        }

        return new UsuarioDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil,
            PerfilDescricao = usuario.Perfil.ToString(),
            StatusAtivo = usuario.StatusAtivo,
            PrimeiroAcesso = usuario.PrimeiroAcesso,
            UltimoAcesso = usuario.UltimoAcesso,
            SacerdoteId = usuario.SacerdoteId,
            SacerdoteNome = sacerdoteNome,
            PastoralRoleId = usuario.PastoralRoleId,
            PastoralRoleNome = pastoralRoleNome,
            CreatedAt = usuario.CreatedAt
        };
    }
}
