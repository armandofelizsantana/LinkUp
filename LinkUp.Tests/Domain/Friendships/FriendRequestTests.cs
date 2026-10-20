
using LinkUp.Domain.Entities;
using LinkUp.Domain.Enums;
using Xunit;

namespace LinkUp.Tests.Domain.Friendships;

public class FriendRequestTests
{
    [Fact]
    public void Create_ShouldStartAsPending()
    {
        var request = new FriendRequest("sender", "receiver");

        Assert.Equal(FriendRequestStatus.Pending, request.Status);
        Assert.True(request.IsVisibleForSender);
        Assert.Null(request.RespondedAtUtc);
    }

    [Fact]
    public void Create_ShouldRejectSelfRequest()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new FriendRequest("user1", "user1"));
    }

    [Fact]
    public void Accept_ShouldChangeStatus()
    {
        var request = new FriendRequest("sender", "receiver");

        request.Accept("receiver");

        Assert.Equal(FriendRequestStatus.Accepted, request.Status);
        Assert.NotNull(request.RespondedAtUtc);
    }

    [Fact]
    public void Accept_ShouldRejectUnauthorizedUser()
    {
        var request = new FriendRequest("sender", "receiver");

        Assert.Throws<UnauthorizedAccessException>(() =>
            request.Accept("anotherUser"));

        Assert.Equal(FriendRequestStatus.Pending, request.Status);
    }

    [Fact]
    public void Reject_ShouldChangeStatus()
    {
        var request = new FriendRequest("sender", "receiver");

        request.Reject("receiver");

        Assert.Equal(FriendRequestStatus.Rejected, request.Status);
    }

    [Fact]
    public void Cancel_ShouldAllowOnlySender()
    {
        var request = new FriendRequest("sender", "receiver");

        Assert.Throws<UnauthorizedAccessException>(() =>
            request.Cancel("receiver"));

        request.Cancel("sender");

        Assert.Equal(FriendRequestStatus.Cancelled, request.Status);
    }

    [Fact]
    public void Respond_ShouldRejectAlreadyProcessedRequest()
    {
        var request = new FriendRequest("sender", "receiver");
        request.Accept("receiver");

        Assert.Throws<InvalidOperationException>(() =>
            request.Reject("receiver"));
    }

    [Fact]
    public void HideFromHistory_ShouldPreserveAcceptedStatus()
    {
        var request = new FriendRequest("sender", "receiver");
        request.Accept("receiver");

        request.HideFromSenderHistory("sender");

        Assert.False(request.IsVisibleForSender);
        Assert.Equal(FriendRequestStatus.Accepted, request.Status);
    }

    [Fact]
    public void HideFromHistory_ShouldRejectPendingRequest()
    {
        var request = new FriendRequest("sender", "receiver");

        Assert.Throws<InvalidOperationException>(() =>
            request.HideFromSenderHistory("sender"));
    }
}
