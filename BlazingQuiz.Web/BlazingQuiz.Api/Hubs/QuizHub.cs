using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BlazingQuiz.Api.Hubs
{
    public class QuizHub : Hub
    {
        // Store connections by room ID
        private static readonly Dictionary<string, HashSet<string>> _roomConnections = new Dictionary<string, HashSet<string>>();
        
        // Store user ID to connection ID mapping for direct user messaging
        private static readonly Dictionary<string, string> _userConnections = new Dictionary<string, string>();

        public async Task JoinRoom(string roomCode)
        {
            // Add the connection to the room
            if (!_roomConnections.ContainsKey(roomCode))
            {
                _roomConnections[roomCode] = new HashSet<string>();
            }

            _roomConnections[roomCode].Add(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            // Store user ID to connection mapping if available in the context
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                
                // Add user to their personal group to enable direct messaging
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User-{userId}");
            }

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
            
            // Remove user from their personal group as well
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User-{userId}");
            }
            
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

        public async Task ParticipantRemoved(string roomCode, int userId)
        {
            // Notify the specific user that they have been removed from the room
            string userGroup = $"User-{userId}";
            await Clients.Group(userGroup).SendAsync("RemovedFromRoom", roomCode, "You have been removed by the host.");
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

            // Remove user connection mapping if it exists
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && _userConnections.ContainsKey(userId))
            {
                _userConnections.Remove(userId);
                
                // Also remove from personal user group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User-{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}