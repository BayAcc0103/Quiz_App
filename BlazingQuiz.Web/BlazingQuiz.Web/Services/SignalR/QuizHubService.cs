using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace BlazingQuiz.Web.Services.SignalR
{
    public class QuizHubService
    {
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;
        private bool _isConnectionStarted = false;

        public QuizHubService(IConfiguration configuration)
        {
            // Fallback to the hardcoded URL if ApiSettings is not available
            var baseUrl = configuration.GetSection("ApiSettings")["BaseUrl"] ?? "https://localhost:7048";
            _hubUrl = baseUrl.TrimEnd('/') + "/quizhub";
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public async Task StartConnectionAsync()
        {
            if (_isConnectionStarted && _hubConnection?.State == HubConnectionState.Connected)
            {
                return; // Already connected
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
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

            // Add reconnection logic
            _hubConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000); // Random delay before reconnect
                await StartConnectionAsync();
            };

            try
            {
                await _hubConnection.StartAsync();
                _isConnectionStarted = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting SignalR connection: {ex.Message}");
                throw;
            }
        }

        public async Task StopConnectionAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                _isConnectionStarted = false;
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
    }
}