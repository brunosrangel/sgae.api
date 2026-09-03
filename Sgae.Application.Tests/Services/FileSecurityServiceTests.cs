using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Sgae.Infrastructure.Services;
using Xunit;

namespace Sgae.Application.Tests.Services;

public class FileSecurityServiceTests
{
    private readonly FileSecurityService _service;

    public FileSecurityServiceTests()
    {
        var mockLogger = new Mock<ILogger<FileSecurityService>>();
        _service = new FileSecurityService(mockLogger.Object);
    }

    [Fact]
    public void ValidateRawBytes_WithValidJpegMagicNumbers_ShouldSucceed()
    {
        // Arrange (JPEG header: FF D8 FF E0)
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        var fileName = "buzios_leitura.jpg";

        // Act
        var result = _service.ValidateRawBytes(jpegBytes, fileName, "image/jpeg");

        // Assert
        result.IsValid.Should().BeTrue();
        result.DetectedMimeType.Should().Be("image/jpeg");
        result.DetectedExtension.Should().Be(".jpg");
        result.FileSizeBytes.Should().Be(jpegBytes.Length);
    }

    [Fact]
    public void ValidateRawBytes_WithValidPngMagicNumbers_ShouldSucceed()
    {
        // Arrange (PNG header: 89 50 4E 47 0D 0A 1A 0A)
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };
        var fileName = "caida_buzios.png";

        // Act
        var result = _service.ValidateRawBytes(pngBytes, fileName, "image/png");

        // Assert
        result.IsValid.Should().BeTrue();
        result.DetectedMimeType.Should().Be("image/png");
        result.DetectedExtension.Should().Be(".png");
    }

    [Fact]
    public void ValidateRawBytes_WithValidPdfMagicNumbers_ShouldSucceed()
    {
        // Arrange (PDF header: 25 50 44 46 (%PDF))
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x37, 0x0A };
        var fileName = "relatorio_liturgico.pdf";

        // Act
        var result = _service.ValidateRawBytes(pdfBytes, fileName, "application/pdf");

        // Assert
        result.IsValid.Should().BeTrue();
        result.DetectedMimeType.Should().Be("application/pdf");
        result.DetectedExtension.Should().Be(".pdf");
    }

    [Fact]
    public void ValidateRawBytes_WithBlockedExtension_ShouldFail()
    {
        // Arrange
        var bytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 }; // MZ executable header
        var fileName = "script_malicioso.exe";

        // Act
        var result = _service.ValidateRawBytes(bytes, fileName);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("não é permitida por motivos de segurança");
    }

    [Fact]
    public void ValidateRawBytes_WithSpoofedMimeType_ShouldFail()
    {
        // Arrange (Declared as image/jpeg, but byte payload is plain text or invalid header)
        var invalidBytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
        var fileName = "fake_image.jpg";

        // Act
        var result = _service.ValidateRawBytes(invalidBytes, fileName, "image/jpeg");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("não é suportado ou possui cabeçalho binário inválido");
    }

    [Fact]
    public void ValidateBase64File_WithValidBase64Jpeg_ShouldSucceed()
    {
        // Arrange (Valid JPEG bytes encoded in base64)
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        var base64Data = "data:image/jpeg;base64," + Convert.ToBase64String(jpegBytes);
        var fileName = "buzios_foto.jpg";

        // Act
        var result = _service.ValidateBase64File(base64Data, fileName);

        // Assert
        result.IsValid.Should().BeTrue();
        result.DetectedMimeType.Should().Be("image/jpeg");
    }

    [Fact]
    public void ValidateBase64File_WithInvalidBase64_ShouldFail()
    {
        // Arrange
        var invalidBase64 = "não-é-um-base64-válido!!!!";

        // Act
        var result = _service.ValidateBase64File(invalidBase64, "teste.jpg");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Formato Base64 inválido");
    }

    [Theory]
    [InlineData("../../../etc/passwd", "passwd")]
    [InlineData("..\\..\\windows\\system32\\cmd.exe", "cmd.exe")]
    [InlineData("foto buzios   teste.jpg", "foto_buzios_teste.jpg")]
    public void SanitizeFileName_ShouldPreventPathTraversalAndNormalizeSpaces(string input, string expectedSubstring)
    {
        // Act
        var sanitized = _service.SanitizeFileName(input);

        // Assert
        sanitized.Should().NotContain("../");
        sanitized.Should().NotContain("..\\");
        sanitized.Should().Contain(expectedSubstring);
    }
}
