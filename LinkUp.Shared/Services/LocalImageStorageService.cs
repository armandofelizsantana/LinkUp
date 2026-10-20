using LinkUp.Application.Interfaces;
using LinkUp.Application.Models;
using LinkUp.Shared.Options;

namespace LinkUp.Shared.Services;

/// <summary>
/// Guarda imágenes validadas bajo nombres generados por el servidor.
/// No confía en la extensión, MIME ni nombre aportados por el cliente.
/// </summary>
public sealed class LocalImageStorageService : IImageStorageService
{
    private const int BufferSize = 81920;
    private readonly string _storageDirectory;
    private readonly string _publicBasePath;
    private readonly long _maxBytes;

    public LocalImageStorageService(ImageStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.StorageDirectory))
            throw new ArgumentException("Configure un directorio de almacenamiento.", nameof(options));
        if (!Path.IsPathFullyQualified(options.StorageDirectory))
            throw new ArgumentException("El directorio de almacenamiento debe ser absoluto.", nameof(options));
        if (options.MaxFileSizeBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "El tamaño máximo debe ser positivo.");
        if (string.IsNullOrWhiteSpace(options.PublicBasePath) ||
            !options.PublicBasePath.StartsWith('/') ||
            options.PublicBasePath.StartsWith("//", StringComparison.Ordinal) ||
            options.PublicBasePath.Contains('?') ||
            options.PublicBasePath.Contains('#') ||
            options.PublicBasePath.Contains("..", StringComparison.Ordinal))
            throw new ArgumentException("La ruta pública debe ser una ruta local absoluta.", nameof(options));

        _storageDirectory = Path.GetFullPath(options.StorageDirectory);
        _publicBasePath = options.PublicBasePath.TrimEnd('/');
        _maxBytes = options.MaxFileSizeBytes;
    }

    public async Task<StoredImage> SaveAsync(
        Stream imageStream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        if (!imageStream.CanRead)
            throw new ArgumentException("El archivo debe poder leerse.", nameof(imageStream));

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var canonicalExtension = (extension, contentType.ToLowerInvariant()) switch
        {
            (".jpg" or ".jpeg", "image/jpeg") => ".jpg",
            (".png", "image/png") => ".png",
            (".gif", "image/gif") => ".gif",
            (".webp", "image/webp") => ".webp",
            _ => throw new ArgumentException("Formato de imagen no permitido (JPG, PNG, GIF o WEBP).")
        };

        // Se limita el tamaño mientras se lee, sin confiar en Content-Length.
        using var buffer = new MemoryStream();
        var chunk = new byte[BufferSize];
        while (true)
        {
            var read = await imageStream.ReadAsync(chunk, cancellationToken);
            if (read == 0) break;
            if (buffer.Length + read > _maxBytes)
                throw new InvalidDataException("La imagen supera el límite de 5 MB.");
            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }

        if (buffer.Length == 0)
            throw new InvalidDataException("La imagen está vacía.");

        if (!HasExpectedSignature(buffer.GetBuffer().AsSpan(0, (int)buffer.Length), canonicalExtension))
            throw new InvalidDataException("El contenido no corresponde al formato declarado.");

        Directory.CreateDirectory(_storageDirectory);
        var fileName = $"{Guid.NewGuid():N}{canonicalExtension}";
        var fullPath = Path.Combine(_storageDirectory, fileName);
        buffer.Position = 0;

        try
        {
            await using var file = new FileStream(
                fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                BufferSize, FileOptions.Asynchronous);
            await buffer.CopyToAsync(file, cancellationToken);
        }
        catch
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            throw;
        }

        return new StoredImage(fileName, $"{_publicBasePath}/{fileName}");
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedFileName);
        cancellationToken.ThrowIfCancellationRequested();

        // Solo se admiten identificadores generados por este servicio.
        var extension = Path.GetExtension(storedFileName).ToLowerInvariant();
        var name = Path.GetFileNameWithoutExtension(storedFileName);
        if (storedFileName != Path.GetFileName(storedFileName) ||
            name.Length != 32 ||
            !Guid.TryParseExact(name, "N", out _) ||
            extension is not (".jpg" or ".png" or ".gif" or ".webp"))
            throw new ArgumentException("Identificador de imagen no válido.", nameof(storedFileName));

        var fullPath = Path.Combine(_storageDirectory, storedFileName);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    private static bool HasExpectedSignature(ReadOnlySpan<byte> bytes, string extension)
    {
        return extension switch
        {
            ".jpg" => bytes.Length >= 3 &&
                bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
            ".png" => bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".gif" => bytes.StartsWith("GIF87a"u8) || bytes.StartsWith("GIF89a"u8),
            ".webp" => bytes.Length >= 12 &&
                bytes[..4].SequenceEqual("RIFF"u8) &&
                bytes.Slice(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
