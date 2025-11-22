using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.Components.Services.SignalR
{
    public class QuizHubService
    {
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;
        private bool _isConnectionStarted = false;

        public QuizHubService(IConfiguration configuration)
        {
            // Fallback to the hardcoded URL if ApiSettings is not available
            var baseUrl = configuration.GetSection("ApiSettings")["BaseUrl"] ?? "https://b861mvjb-7048.asse.devtunnels.ms/";
            // Ensure the URL doesn't have trailing slash and add the hub endpoint
            _hubUrl = baseUrl.TrimEnd('/') + "/quizhub";
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public async Task StartConnectionAsync()
        {
            if (_isConnectionStarted && _hubConnection?.State == HubConnectionState.Connected)
            {
                return; // Already connected
            }

            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }

            Console.WriteLine($"Attempting to connect to SignalR hub: {_hubUrl}");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                })
                .WithAutomaticReconnect(new RetryPolicy())
                .Build();

            // Define client-side methods that can be called from the server
            _hubConnection.On<string>("JoinedRoom", (roomCode) =>
            {
                OnJoinedRoom?.Invoke(roomCode);
            });

            _hubConnection.On<string>("LeftRoom", (roomCode) =>
            {
                OnLeftRoom?.Invoke(roomCode);
            });

            _hubConnection.On<string, object>("ReceiveQuestion", (roomCode, question) =>
            {
                OnReceiveQuestion?.Invoke(roomCode, question);
            });

            _hubConnection.On<string, object>("ReceiveAnswer", (roomCode, answerData) =>
            {
                OnReceiveAnswer?.Invoke(roomCode, answerData);
            });

            _hubConnection.On<string>("QuizStarted", (roomCode) =>
            {
                OnQuizStarted?.Invoke(roomCode);
            });

            _hubConnection.On<string>("QuizEnded", (roomCode) =>
            {
                OnQuizEnded?.Invoke(roomCode);
            });

            _hubConnection.On<string, bool>("ParticipantReadyStatusChanged", (userName, isReady) =>
            {
                OnParticipantReadyStatusChanged?.Invoke(userName, isReady);
            });

            _hubConnection.On<object>("ParticipantsListUpdated", (participants) =>
            {
                OnParticipantsListUpdated?.Invoke(participants);
            });

            _hubConnection.On<string, string, string>("UserRemovedFromRoom", (roomCode, roomName, hostName) =>
            {
                OnUserRemovedFromRoom?.Invoke(roomCode, roomName, hostName);
            });

            _hubConnection.On<string, string>("RemovedFromRoom", (roomCode, message) =>
            {
                OnRemovedFromRoom?.Invoke(roomCode, message);
            });

            try
            {
                await _hubConnection.StartAsync();
                _isConnectionStarted = true;
                Console.WriteLine($"Successfully connected to SignalR hub: {_hubUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting SignalR connection to {_hubUrl}: {ex.Message}");
                _isConnectionStarted = false;
                throw;
            }
        }

        public async Task StopConnectionAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    await _hubConnection.StopAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping SignalR connection: {ex.Message}");
                }
                finally
                {
                    await _hubConnection.DisposeAsync();
                    _isConnectionStarted = false;
                }
            }
        }

        private class RetryPolicy : IRetryPolicy
        {
            public TimeSpan? NextRetryDelay(RetryContext retryContext)
            {
                // Retry with exponential backoff up to 30 seconds, max 10 attempts
                if (retryContext.PreviousRetryCount < 10)
                {
                    var nextRetryDelay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryContext.PreviousRetryCount), 30));
                    Console.WriteLine($"Retrying connection in {nextRetryDelay.TotalSeconds} seconds...");
                    return nextRetryDelay;
                }

                Console.WriteLine("Max retry attempts reached, stopping reconnection attempts.");
                return null; // Stop retrying after 10 attempts
            }
        }

        // Event handlers that can be subscribed to by components
        public event Action<string>? OnJoinedRoom;
        public event Action<string>? OnLeftRoom;
        public event Action<string, object>? OnReceiveQuestion;
        public event Action<string, object>? OnReceiveAnswer;
        public event Action<string>? OnQuizStarted;
        public event Action<string>? OnQuizEnded;
        public event Action<string, bool>? OnParticipantReadyStatusChanged;
        public event Action<object>? OnParticipantsListUpdated;
        public event Action<string, string, string>? OnUserRemovedFromRoom; // Event for when user is removed from room
        public event Action<string, string>? OnRemovedFromRoom; // Event for when user is removed from room with custom message

        // Methods to call server-side hub methods
        public async Task JoinRoomAsync(string roomCode)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("JoinRoom", roomCode);
            }
            else
            {
                // Try to ensure connection is established
                await StartConnectionAsync();
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("JoinRoom", roomCode);
                }
            }
        }

        public async Task LeaveRoomAsync(string roomCode)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveRoom", roomCode);
            }
        }

        public async Task StartQuizAsync(string roomCode)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("StartQuiz", roomCode);
            }
        }

        public async Task SendQuestionAsync(string roomCode, object question)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendQuestion", roomCode, question);
            }
        }

        public async Task SubmitAnswerAsync(string roomCode, object answerData)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SubmitAnswer", roomCode, answerData);
            }
        }

        public async Task EndQuizAsync(string roomCode)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("EndQuiz", roomCode);
            }
        }

        public async Task NotifyParticipantReadyAsync(string roomCode, string userName, bool isReady)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("ParticipantReady", roomCode, userName, isReady);
            }
        }

        public async Task UpdateParticipantsListAsync(string roomCode, object participants)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("UpdateParticipantsList", roomCode, participants);
            }
        }

        public async Task NotifyParticipantRemovedAsync(string roomCode, int userId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("ParticipantRemoved", roomCode, userId);
            }
        }
    }
}