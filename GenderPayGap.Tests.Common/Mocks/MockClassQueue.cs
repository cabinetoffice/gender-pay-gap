using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;

public class MockClassQueue : IQueue
{

    private readonly Queue<object> _queue = new Queue<object>();

    public MockClassQueue(string queueName)
    {
        Name = queueName;
    }

    public string Name { get; }

    public Task AddMessageAsync<T>(T instance)
    {
        _queue.Enqueue(instance);
        return Task.CompletedTask;
    }

    public Task AddMessageAsync(string message)
    {
        throw new NotImplementedException();
    }

}
