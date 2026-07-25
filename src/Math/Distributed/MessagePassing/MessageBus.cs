namespace MathVerse.Math.Distributed.MessagePassing;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Asynchronous publish/subscribe message bus that routes messages through topic-based channels.
/// Each topic is backed by a <see cref="Channel"/> instance.
/// </summary>
public sealed class MessageBus : IDisposable
{
    private readonly ConcurrentDictionary<string, Channel> _channels = new();
    private readonly ConcurrentDictionary<string, List<Func<Message, ValueTask>>> _subscriptions = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _consumerTokens = new();
    private bool _disposed;

    /// <summary>
    /// Asynchronously publishes a message to the topic matching the receiver ID.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    public async ValueTask Publish(Message message)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(MessageBus));

        var topic = message.ReceiverId;
        var channel = _channels.GetOrAdd(topic, static t => new Channel(t));
        await channel.SendMessage(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Subscribes a handler to messages on the specified topic.
    /// A background consumer loop is started for the topic if one does not already exist.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="handler">The async handler invoked for each message on the topic.</param>
    public void Subscribe(string topic, Func<Message, ValueTask> handler)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(MessageBus));
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        var channel = _channels.GetOrAdd(topic, static t => new Channel(t));
        var handlers = _subscriptions.GetOrAdd(topic, static _ => new List<Func<Message, ValueTask>>());
        lock (handlers)
        {
            handlers.Add(handler);
        }

        if (!_consumerTokens.ContainsKey(topic))
        {
            var cts = new CancellationTokenSource();
            if (_consumerTokens.TryAdd(topic, cts))
            {
                StartConsumer(topic, channel, cts.Token);
            }
            else
            {
                cts.Dispose();
            }
        }
    }

    /// <summary>
    /// Unsubscribes all handlers from the specified topic and stops the consumer loop.
    /// </summary>
    /// <param name="topic">The topic to unsubscribe from.</param>
    public void Unsubscribe(string topic)
    {
        if (_consumerTokens.TryRemove(topic, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        _subscriptions.TryRemove(topic, out _);

        if (_channels.TryRemove(topic, out var channel))
        {
            channel.Complete();
        }
    }

    /// <summary>
    /// Starts a background consumer loop that reads messages from the channel and invokes all registered handlers.
    /// </summary>
    private void StartConsumer(string topic, Channel channel, CancellationToken ct)
    {
        _ = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var msg = await channel.ReceiveMessage(ct).ConfigureAwait(false);

                    if (_subscriptions.TryGetValue(topic, out var handlers))
                    {
                        Func<Message, ValueTask>[] snapshot;
                        lock (handlers)
                        {
                            snapshot = handlers.ToArray();
                        }

                        for (int i = 0; i < snapshot.Length; i++)
                        {
                            await snapshot[i](msg).ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }, ct);
    }

    /// <summary>
    /// Disposes the message bus, cancelling all consumer loops and completing all channels.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var kvp in _consumerTokens)
        {
            kvp.Value.Cancel();
            kvp.Value.Dispose();
        }

        foreach (var kvp in _channels)
        {
            kvp.Value.Complete();
        }

        _consumerTokens.Clear();
        _channels.Clear();
        _subscriptions.Clear();
    }
}
