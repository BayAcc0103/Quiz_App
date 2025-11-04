using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BlazingQuiz.Api.Hubs
{
    public class QuizHub : Hub
    {
        // Store connections by room ID
        private static readonly Dictionary<string, HashSet<string>> _roomConnections = new Dictionary<string, HashSet<string>>();
        
        public async Task JoinRoom(string roomCode)
        {
            // Add the connection to the room
            if (!_roomConnections.ContainsKey(roomCode))
            {
                _roomConnections[roomCode] = new HashSet<string>();
            }
            
            _roomConnections[roomCode].Add(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            
            await Clients.Caller.SendAsync("JoinedRoom", roomCode);
        }

        public async Task LeaveRoom(string roomCode)
        {
            // Remove the connection from the room
            if (_roomConnections.ContainsKey(roomCode))
            {
                _roomConnections[roomCode].Remove(Context.ConnectionId);
                if (_roomConnections[roomCode].Count == 0)
                {
                    _roomConnections.Remove(roomCode);
                }
            }
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Caller.SendAsync("LeftRoom", roomCode);
        }

        public async Task StartQuiz(string roomCode)
        {
            // Notify all participants in the room that the quiz is starting
            await Clients.Group(roomCode).SendAsync("QuizStarted", roomCode);
        }

        public async Task SendQuestion(string roomCode, object question)
        {
            // Send a question to all participants in the room
            await Clients.Group(roomCode).SendAsync("ReceiveQuestion", question);
        }

        public async Task SubmitAnswer(string roomCode, object answerData)
        {
            // Relay the answer to other participants (could be used for real-time answer tracking)
            await Clients.Group(roomCode).SendAsync("ReceiveAnswer", answerData);
        }

        public async Task EndQuiz(string roomCode)
        {
            // Notify all participants in the room that the quiz has ended
            await Clients.Group(roomCode).SendAsync("QuizEnded", roomCode);
        }

        public async Task ParticipantReady(string roomCode, string userName, bool isReady)
        {
            // Notify all participants that someone has changed their ready status
            await Clients.Group(roomCode).SendAsync("ParticipantReadyStatusChanged", userName, isReady);
        }

        public async Task UpdateParticipantsList(string roomCode, object participants)
        {
            // Update the list of participants for all in the room
            await Clients.Group(roomCode).SendAsync("ParticipantsListUpdated", participants);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Clean up connections when a client disconnects
            foreach (var room in _roomConnections.ToList())
            {
                if (room.Value.Contains(Context.ConnectionId))
                {
                    room.Value.Remove(Context.ConnectionId);
                    if (room.Value.Count == 0)
                    {
                        _roomConnections.Remove(room.Key);
                    }
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Key);
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}