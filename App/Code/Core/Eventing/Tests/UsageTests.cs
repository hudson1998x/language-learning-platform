using LLE.Eventing;

namespace Tests;

public sealed class EventingUsageTests
{
    private sealed class User
    {
        public string Username { get; set; } = "";
    }

    private sealed class UserEvents : EventTable
    {
        public EventCollection<User> Created { get; } = new();
    }

    [Fact]
    public void EventTables_Should_Be_Singletons()
    {
        var a = Eventing.Of<UserEvents>();
        var b = Eventing.Of<UserEvents>();

        Assert.Same(a, b);
    }

    [Fact]
    public async Task Pipeline_Should_Execute_In_Order()
    {
        var order = new List<int>();

        var evt = new EventCollection<User>();

        evt.Pipeline(user =>
        {
            order.Add(1);
            return user;
        });

        evt.Pipeline(user =>
        {
            order.Add(2);
            return user;
        });

        await evt.DispatchAsync(new User());

        Assert.Equal(new[] { 1, 2 }, order);
    }

    [Fact]
    public async Task Pipeline_Should_Transform_Payload()
    {
        var evt = new EventCollection<User>();

        evt.Pipeline(user =>
        {
            user.Username = user.Username.Trim();
            return user;
        });

        evt.Pipeline(user =>
        {
            user.Username = user.Username.ToUpperInvariant();
            return user;
        });

        var result = await evt.DispatchAsync(new User
        {
            Username = " john "
        });

        Assert.Equal("JOHN", result.Username);
    }

    [Fact]
    public async Task Dispatch_Should_Return_Final_Pipeline_Result()
    {
        var evt = new EventCollection<User>();

        evt.Pipeline(user =>
        {
            user.Username = "Dave";
            return user;
        });

        var result = await evt.DispatchAsync(new User());

        Assert.Equal("Dave", result.Username);
    }

    [Fact]
    public async Task Concurrent_Should_Run_All_Handlers()
    {
        var count = 0;

        var evt = new EventCollection<User>();

        evt.Concurrent(_ => Interlocked.Increment(ref count));
        evt.Concurrent(_ => Interlocked.Increment(ref count));
        evt.Concurrent(_ => Interlocked.Increment(ref count));

        await evt.DispatchAsync(new User());

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task Concurrent_Should_Run_After_Pipeline()
    {
        var state = "";

        var evt = new EventCollection<User>();

        evt.Pipeline(user =>
        {
            state += "A";
            return user;
        });

        evt.Pipeline(user =>
        {
            state += "B";
            return user;
        });

        evt.Concurrent(_ =>
        {
            Assert.Equal("AB", state);
        });

        await evt.DispatchAsync(new User());
    }

    [Fact]
    public async Task Dispatch_Should_Aggregate_Concurrent_Exceptions()
    {
        var evt = new EventCollection<User>();

        evt.Concurrent(_ => throw new InvalidOperationException("A"));
        evt.Concurrent(_ => throw new InvalidOperationException("B"));

        var ex = await Assert.ThrowsAsync<AggregateException>(() =>
            evt.DispatchAsync(new User()));

        Assert.Equal(2, ex.InnerExceptions.Count);
    }

    [Fact]
    public async Task Dispatch_Should_Aggregate_Pipeline_Exceptions()
    {
        var evt = new EventCollection<User>();

        evt.Pipeline((Action<User>)(_ =>
        {
            throw new InvalidOperationException("A");
        }));

        evt.Pipeline((Action<User>)(_ =>
        {
            throw new InvalidOperationException("B");
        }));

        var ex = await Assert.ThrowsAsync<AggregateException>(() =>
            evt.DispatchAsync(new User()));

        Assert.Equal(2, ex.InnerExceptions.Count);
    }

    [Fact]
    public async Task Pipeline_Should_Continue_After_Failure()
    {
        var ran = false;

        var evt = new EventCollection<User>();

        evt.Pipeline((Action<User>)(_ =>
        {
            throw new InvalidOperationException();
        }));

        evt.Pipeline(user =>
        {
            ran = true;
            return user;
        });

        await Assert.ThrowsAsync<AggregateException>(() =>
            evt.DispatchAsync(new User()));

        Assert.True(ran);
    }
}