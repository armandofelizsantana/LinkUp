using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LinkUp.Domain.Enums;

namespace LinkUp.Domain.Entities;

public class Friendship
{
    public int Id { get; private set; }

    public string User1Id { get; private set; } = string.Empty;
    public string User2Id { get; private set; } = string.Empty;

    public FriendshipStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private Friendship() { } 
    public Friendship(string firstUserId, string secondUserId)
    {
        if (string.IsNullOrWhiteSpace(firstUserId) ||
            string.IsNullOrWhiteSpace(secondUserId))
            throw new ArgumentException("Los usuarios son obligatorios.");

        if (firstUserId == secondUserId)
            throw new InvalidOperationException(
                "Un usuario no puede ser amigo de sí mismo.");

      
        var users = new[] { firstUserId, secondUserId };
        Array.Sort(users, StringComparer.Ordinal);

        User1Id = users[0];
        User2Id = users[1];

        Status = FriendshipStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public bool IncludesUser(string userId) =>
        User1Id == userId || User2Id == userId;

    public void Delete()
    {
        if (Status != FriendshipStatus.Active)
            throw new InvalidOperationException(
                "La amistad ya se encuentra eliminada.");

        Status = FriendshipStatus.Deleted;
        DeletedAtUtc = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Status != FriendshipStatus.Deleted)
            throw new InvalidOperationException(
                "La amistad ya está activa.");

        Status = FriendshipStatus.Active;
        DeletedAtUtc = null;
    }
}
