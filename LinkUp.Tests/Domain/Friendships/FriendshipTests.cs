using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LinkUp.Domain.Entities;
using LinkUp.Domain.Enums;
using Xunit;

namespace LinkUp.Tests.Domain.Friendships;

public class FriendshipTests
{
    [Fact]
    public void Create_ShouldStartAsActive()
    {
        var friendship = new Friendship("user1", "user2");

        Assert.Equal(FriendshipStatus.Active, friendship.Status);
        Assert.Null(friendship.DeletedAtUtc);
    }

    [Fact]
    public void Create_ShouldNormalizeUserOrder()
    {
        var first = new Friendship("userB", "userA");
        var second = new Friendship("userA", "userB");

        Assert.Equal(first.User1Id, second.User1Id);
        Assert.Equal(first.User2Id, second.User2Id);
    }

    [Fact]
    public void Create_ShouldRejectSameUser()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new Friendship("user1", "user1"));
    }

    [Fact]
    public void Delete_ShouldChangeStatus()
    {
        var friendship = new Friendship("user1", "user2");

        friendship.Delete();

        Assert.Equal(FriendshipStatus.Deleted, friendship.Status);
        Assert.NotNull(friendship.DeletedAtUtc);
    }

    [Fact]
    public void Reactivate_ShouldRestoreFriendship()
    {
        var friendship = new Friendship("user1", "user2");

        friendship.Delete();
        friendship.Reactivate();

        Assert.Equal(FriendshipStatus.Active, friendship.Status);
        Assert.Null(friendship.DeletedAtUtc);
    }

    [Fact]
    public void Delete_ShouldRejectAlreadyDeletedFriendship()
    {
        var friendship = new Friendship("user1", "user2");
        friendship.Delete();

        Assert.Throws<InvalidOperationException>(() =>
            friendship.Delete());
    }

    [Fact]
    public void IncludesUser_ShouldIdentifyParticipants()
    {
        var friendship = new Friendship("user1", "user2");

        Assert.True(friendship.IncludesUser("user1"));
        Assert.True(friendship.IncludesUser("user2"));
        Assert.False(friendship.IncludesUser("user3"));
    }
}
