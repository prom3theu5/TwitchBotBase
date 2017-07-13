using PromBot.Models;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib;
using Serilog;

namespace PromBot
{
    public enum MessageQueueType
    {
        Chat,
        Whisper
    }

    public class MessageQueue
    {
        private const int WarningStart = 30;
        private TwitchClient _client;
        private MessageQueueType _type;

        private readonly Random _nonceRand;
        private readonly ConcurrentQueue<Message> _pendingSends;
        private readonly ConcurrentDictionary<int, string> _pendingSendsByNonce;
        private int _count, _nextWarning;
        private readonly ILogger _logger;


        public int Count => _count;
        public int SendCount => _pendingSends.Count;

        internal MessageQueue(TwitchClient Client, MessageQueueType type)
        {
            _logger = Bootstrapper.Container.GetInstance<ILogger>();
            _nextWarning = WarningStart;
            _client = Client;
            _type = type;

            _nonceRand = new Random();
            _pendingSends = new ConcurrentQueue<Message>();
            _pendingSendsByNonce = new ConcurrentDictionary<int, string>();
        }
        
        internal Message QueueSend(string message, string sender = Bot.BotNickname)
        {
            Message msg = new Message(message, sender)
            {
                Nonce = GenerateNonce()
            };

            if (_pendingSendsByNonce.TryAdd(msg.Nonce, msg.TwitchMessage))
            {
                msg.State = MessageState.Queued;
                IncrementCount();
                _pendingSends.Enqueue(msg);
            }
            else
                msg.State = MessageState.Failed;
            return msg;
        }

        internal Message QueueWhisper(string message, string target, string sender = Bot.BotNickname)
        {
            Message msg = new Message(message, sender, null, null, target)
            {
                Nonce = GenerateNonce()
            };

            if (_pendingSendsByNonce.TryAdd(msg.Nonce, msg.TwitchMessage))
            {
                msg.State = MessageState.Queued;
                IncrementCount();
                _pendingSends.Enqueue(msg);
            }
            else
                msg.State = MessageState.Failed;
            return msg;
        }

        public Task RunSendQueue(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (!cancelToken.IsCancellationRequested)
                    {
                        while (_pendingSends.TryDequeue(out Message msg))
                        {
                            DecrementCount();
                            if (_pendingSendsByNonce.TryRemove(msg.Nonce, out string text))
                            {
                                try
                                {

                                    if (_client != null)
                                    {
                                        if (_type == MessageQueueType.Whisper)
                                        {
                                            if (!_client.WhisperThrottler.MessagePermitted(text))
                                            {
                                                _logger.Error("Message throttled in Send Queue (Rate of 60 per 60 seconds Reached!");
                                                await Task.Delay(250).ConfigureAwait(false);
                                                return;
                                            }
                                        }
                                    }

                                    switch (_type)
                                    {
                                        case MessageQueueType.Chat:
                                            _client.SendMessage(Bot.Channel, msg.TwitchMessage);
                                            msg.State = MessageState.Normal;
                                            break;
                                        case MessageQueueType.Whisper:
                                            _client.SendWhisper(msg.Target, msg.TwitchMessage);
                                            msg.State = MessageState.Normal;
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    msg.State = MessageState.Failed;
                                    _logger.Error("Failed to send message to {Channel}, Error: {Error}", msg.Channel, ex.Message);
                                }
                            }

                        }
                        await Task.Delay(250).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) { }
            });
        }

        private void IncrementCount()
        {
            int count = Interlocked.Increment(ref _count);
            if (count >= _nextWarning)
            {
                _nextWarning <<= 1;
                count = _pendingSends.Count;
                _logger.Warning($"Queue is backed up, currently at ({count} sends.");
            }
            else if (count < WarningStart) //Reset once the problem is solved
                _nextWarning = WarningStart;
        }
        private void DecrementCount()
        {
            int count = Interlocked.Decrement(ref _count);
            if (count < (WarningStart / 2)) //Reset once the problem is solved
                _nextWarning = WarningStart;
        }

        /// <summary> Clears all queued message sends/edits/deletes. </summary>
        public void Clear()
        {

            while (_pendingSends.TryDequeue(out Message msg))
                DecrementCount();
            _pendingSendsByNonce.Clear();
        }

        private int GenerateNonce()
        {
            lock (_nonceRand)
                return _nonceRand.Next(1, int.MaxValue);
        }
    }
}