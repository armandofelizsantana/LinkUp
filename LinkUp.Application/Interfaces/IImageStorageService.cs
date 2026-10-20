using LinkUp.Application.Models;

namespace LinkUp.Application.Interfaces;

public interface IImageStorageService
{
    Task<StoredImage> SaveAsync(
        Stream imageStream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storedFileName,
        CancellationToken cancellationToken = default);
}
