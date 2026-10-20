using LinkUp.Shared.Options;
using LinkUp.Shared.Services;
using Xunit;

namespace LinkUp.Tests.Shared;

public sealed class LocalImageStorageServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(), "linkup-image-tests", Guid.NewGuid().ToString("N"));

    private LocalImageStorageService CreateService(long maxBytes = 5L * 1024 * 1024) =>
        new(new ImageStorageOptions
        {
            StorageDirectory = _directory,
            PublicBasePath = "/uploads/images",
            MaxFileSizeBytes = maxBytes
        });

    private static readonly byte[] Png =
        [137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 0, 0];

    [Fact]
    public async Task SaveAsync_StoresValidPngWithServerGeneratedName()
    {
        var service = CreateService();
        using var stream = new MemoryStream(Png);

        var saved = await service.SaveAsync(stream, "../../picture.png", "image/png");

        Assert.EndsWith(".png", saved.FileName);
        Assert.True(Guid.TryParseExact(Path.GetFileNameWithoutExtension(saved.FileName), "N", out _));
        Assert.Equal("/uploads/images/" + saved.FileName, saved.PublicUrl);
        Assert.Equal(Png, await File.ReadAllBytesAsync(Path.Combine(_directory, saved.FileName)));
    }

    [Fact]
    public async Task DeleteAsync_RemovesPreviouslyStoredImage()
    {
        var service = CreateService();
        using var stream = new MemoryStream(Png);
        var saved = await service.SaveAsync(stream, "photo.png", "image/png");

        await service.DeleteAsync(saved.FileName);

        Assert.False(File.Exists(Path.Combine(_directory, saved.FileName)));
    }

    [Theory]
    [InlineData("picture.exe", "image/png")]
    [InlineData("picture.png", "application/octet-stream")]
    [InlineData("picture.jpg", "image/png")]
    public async Task SaveAsync_RejectsDisallowedExtensionOrMime(string name, string mime)
    {
        using var stream = new MemoryStream(Png);
        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateService().SaveAsync(stream, name, mime));
    }

    [Fact]
    public async Task SaveAsync_RejectsMismatchedFileSignature()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8]);
        await Assert.ThrowsAsync<InvalidDataException>(
            () => CreateService().SaveAsync(stream, "photo.png", "image/png"));
        Assert.False(Directory.Exists(_directory));
    }

    [Fact]
    public async Task SaveAsync_RejectsEmptyImage()
    {
        using var stream = new MemoryStream();
        await Assert.ThrowsAsync<InvalidDataException>(
            () => CreateService().SaveAsync(stream, "photo.png", "image/png"));
    }

    [Fact]
    public async Task SaveAsync_RejectsImageOverConfiguredLimit()
    {
        using var stream = new MemoryStream(Png);
        await Assert.ThrowsAsync<InvalidDataException>(
            () => CreateService(maxBytes: 8).SaveAsync(stream, "photo.png", "image/png"));
        Assert.False(Directory.Exists(_directory));
    }

    [Theory]
    [InlineData("../other.png")]
    [InlineData("other.png")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.exe")]
    public async Task DeleteAsync_RejectsUnknownOrUnsafeName(string name)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateService().DeleteAsync(name));
    }

    [Fact]
    public void Constructor_RejectsRelativeStoragePath()
    {
        Assert.Throws<ArgumentException>(() => new LocalImageStorageService(
            new ImageStorageOptions { StorageDirectory = "uploads" }));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
