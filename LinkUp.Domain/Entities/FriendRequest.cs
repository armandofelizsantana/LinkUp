using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LinkUp.Domain.Enums;

namespace LinkUp.Domain.Entities;

public class FriendRequest
{
    public int Id { get; private set; }

    public string SenderId { get; private set; } = string.Empty;
    public string ReceiverId { get; private set; } = string.Empty;

    public FriendRequestStatus Status { get; private set; }

    public DateTime SentAtUtc { get; private set; }
    public DateTime? RespondedAtUtc { get; private set; }

    public bool IsVisibleForSender { get; private set; } = true;

    private FriendRequest() { } 

    public FriendRequest(string senderId, string receiverId)
    {
        if (string.IsNullOrWhiteSpace(senderId) ||
            string.IsNullOrWhiteSpace(receiverId))
            throw new ArgumentException("Los usuarios son obligatorios.");

        if (senderId == receiverId)
            throw new InvalidOperationException(
                "No puede enviarse una solicitud a sí mismo.");

        SenderId = senderId;
        ReceiverId = receiverId;
        Status = FriendRequestStatus.Pending;
        SentAtUtc = DateTime.UtcNow;
    }

    public void Accept(string receiverId)
    {
        EnsurePending();
        EnsureReceiver(receiverId);

        Status = FriendRequestStatus.Accepted;
        RespondedAtUtc = DateTime.UtcNow;
    }

    public void Reject(string receiverId)
    {
        EnsurePending();
        EnsureReceiver(receiverId);

        Status = FriendRequestStatus.Rejected;
        RespondedAtUtc = DateTime.UtcNow;
    }

    public void Cancel(string senderId)
    {
        EnsurePending();

        if (SenderId != senderId)
            throw new UnauthorizedAccessException(
                "Solo el emisor puede cancelar la solicitud.");

        Status = FriendRequestStatus.Cancelled;
        RespondedAtUtc = DateTime.UtcNow;
    }

    public void HideFromSenderHistory(string senderId)
    {
        if (SenderId != senderId)
            throw new UnauthorizedAccessException(
                "Solo el emisor puede ocultar la solicitud.");

        if (Status != FriendRequestStatus.Accepted &&
            Status != FriendRequestStatus.Rejected)
            throw new InvalidOperationException(
                "Solo pueden ocultarse solicitudes aceptadas o rechazadas.");

        if (!IsVisibleForSender)
            throw new InvalidOperationException(
                "La solicitud ya está oculta.");

        IsVisibleForSender = false;
    }

    private void EnsurePending()
    {
        if (Status != FriendRequestStatus.Pending)
            throw new InvalidOperationException(
                "La solicitud ya fue respondida o cancelada.");
    }

    private void EnsureReceiver(string receiverId)
    {
        if (ReceiverId != receiverId)
            throw new UnauthorizedAccessException(
                "Solo el receptor puede responder esta solicitud.");
    }
}
