using Nito.AsyncEx;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Services;
using TwitchLib.Models.Client;

namespace PromBot
{
    public partial class Client : IDisposable
    {
        public string Nickname;
        public string ApiToken;
        public string Channel;
        public string ClientId;
        private readonly AsyncLock _connectionLock;
        private readonly ManualResetEvent _disconnectedEvent;
        private readonly ManualResetEventSlim _connectedEvent;
        private readonly TaskManager _taskManager;
        private readonly ServiceCollection _services;
        private Stopwatch _connectionStopwatch;
        public MessageQueue TwitchClientMessageQueue { get; }
        public MessageQueue TwitchWhisperMessageQueue { get; }
        public CancellationToken CancelToken { get; private set; }
        public TwitchClient TwitchClient { get; private set; }
        public DateTime LastMessageReceived { get; set; }
        private readonly ILogger _logger;

        public Client(string nick, string pass, string channel, string clientid)
        {
            _logger = Bootstrapper.Logger;
            _connectionStopwatch = new Stopwatch();

            //Async
            _taskManager = new TaskManager(Disconnect);
            _connectionLock = new AsyncLock();
            _disconnectedEvent = new ManualResetEvent(true);
            _connectedEvent = new ManualResetEventSlim(false);
            CancelToken = new CancellationToken(true);
            
            Nickname = nick;
            ApiToken = pass;
            ClientId = clientid;
            Channel = channel;

            //Client
            var credentials = new ConnectionCredentials(Nickname, ApiToken);
            TwitchClient = new TwitchClient(credentials, channel, '!', '!', false);
            TwitchClient.OnConnected += Client_OnConnected;
            TwitchClientMessageQueue = new MessageQueue(TwitchClient, MessageQueueType.Chat);
            TwitchWhisperMessageQueue = new MessageQueue(TwitchClient, MessageQueueType.Whisper);
            TwitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;
            TwitchClient.WhisperThrottler = new MessageThrottler(60, TimeSpan.FromSeconds(60));

            //Extensibility
            _services = new ServiceCollection(this);
        }
      
        private void TwitchClient_OnMessageReceived(object sender, TwitchLib.Events.Client.OnMessageReceivedArgs e)
        {
            LastMessageReceived = DateTime.Now;
        }

        private void TwitchWhispers_OnConnected(object sender, TwitchLib.Events.Client.OnConnectedArgs e)
        {
            _logger.Information("Twitch Whisper Client Connected");
        }
        
        public async Task SendMessage(string message, string sender = Bot.BotNickname)
        {
            await Task.Run(() => {
                TwitchClientMessageQueue.QueueSend(message, sender);
            });
        }

        public async Task SendWhisper(string message, string target, string sender = Bot.BotNickname)
        {
            await Task.Run(() => {
                TwitchWhisperMessageQueue.QueueWhisper(message, target, sender);
            });
        }

        private async void Client_OnConnected(object sender, TwitchLib.Events.Client.OnConnectedArgs e)
        {
            _logger.Information("Connected to Twitch Channel {Channel}", e.AutoJoinChannel);
            await SendMessage($"{Bot.BotNickname} Online...").ConfigureAwait(false);
        }

        private Task ConnectClient()
        {
            return Task.Run(() => {
                TwitchClient.Connect();
            });
        }
        
        public async Task Connect()
        {
            try
            {
                using (await _connectionLock.LockAsync().ConfigureAwait(false))
                {
                    await Disconnect().ConfigureAwait(false);
                    _taskManager.ClearException();
                    _disconnectedEvent.Reset();

                    var cancelSource = new CancellationTokenSource();
                    CancelToken = cancelSource.Token;
                    var tasks = new Task[] { CancelToken.Wait(), TwitchClientMessageQueue.RunSendQueue(CancelToken), TwitchWhisperMessageQueue.RunSendQueue(CancelToken) };
                    await _taskManager.Start(tasks, cancelSource).ConfigureAwait(false);
                    await ConnectClient().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await _taskManager.SignalError(ex).ConfigureAwait(false);
                throw;
            }
        }
       
        public Task Disconnect()
        {
            if (TwitchClient.IsConnected)
            {
                TwitchClient.Disconnect();
            }
            TwitchClientMessageQueue.Clear();
            TwitchWhisperMessageQueue.Clear();

            _connectedEvent.Reset();
            _disconnectedEvent.Set();
            return _taskManager.Stop(true);
        }

        #region Services
        public T AddService<T>(T instance)
            where T : class, IService
            => _services.Add(instance);
        public T AddService<T>()
            where T : class, IService, new()
            => _services.Add(new T());
        public T GetService<T>(bool isRequired = true)
            where T : class, IService
            => _services.Get<T>(isRequired);
        #endregion

        #region Async Wrapper
        public void ExecuteAndWait(Func<Task> asyncAction)
        {
            asyncAction().GetAwaiter().GetResult();
            _disconnectedEvent.WaitOne();
        }
        #endregion

        #region IDisposable
        private bool _isDisposed = false;

        protected virtual void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    _disconnectedEvent.Dispose();
                    _connectedEvent.Dispose();
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            if (TwitchClient.IsConnected)
                Disconnect();
            Dispose(true);
        }
        #endregion
    }
}