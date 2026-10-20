namespace LinkUp.Shared.Options;

public sealed class ImageStorageOptions
{
    public string StorageDirectory { get; set; } = string.Empty;
    public string PublicBasePath { get; set; } = "/uploads/images";
    public long MaxFileSizeBytes { get; set; } = 5L * 1024 * 1024;
}
