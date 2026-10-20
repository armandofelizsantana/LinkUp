namespace LinkUp.Application.Models;

/// <summary>
/// Identificador persistible y URL pública de una imagen almacenada.
/// </summary>
public sealed record StoredImage(string FileName, string PublicUrl);
