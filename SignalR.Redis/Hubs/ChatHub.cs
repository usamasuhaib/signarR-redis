using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using SignalR.Redis.Models;
using System.Text.Json;

namespace SignalR.Redis.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ChatHub> _logger;
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _expiration = TimeSpan.FromMinutes(1); // Expiration time of 2 minutes


        public ChatHub(UserManager<ApplicationUser> userManager, ILogger<ChatHub> logger,IDistributedCache cache)
        {
            _userManager = userManager;
            _logger = logger;
            _cache = cache;
        }
        public async Task SendMessage( string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            var userName = user.Email;


            // Broadcast the message to all clients
            await Clients.All.SendAsync("ReceiveMessage", userName, message);


            // Store message in Redis cache
            await StoreMessageAsync(userName, message);
            _logger.LogInformation("Message Sent to all users");
        }

        public async Task StoreMessageAsync(string userName, string message)
        {
            try
            {
                // Fetch existing messages from the cache
                var existingMessagesJson = await _cache.GetStringAsync("chat_messages");

                // Deserialize existing messages or initialize an empty list
                var messages = existingMessagesJson != null ?
                    JsonSerializer.Deserialize<List<ChatMessages>>(existingMessagesJson) :
                    new List<ChatMessages>();

                // Add the new message to the list
                messages.Add(new ChatMessages
                {
                    UserName = userName,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                });

                // Serialize updated messages and store back to cache with expiration time
                var updatedMessagesJson = JsonSerializer.Serialize(messages);
                await _cache.SetStringAsync("chat_messages", updatedMessagesJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _expiration
                });
            }
            catch (Exception ex)
            {
                // Handle exceptions, if any
                _logger.LogError(ex, "Error occurred while storing message in cache");
            }
        }

        public async Task GetCachedMessages()
        {
            try
            {
                // Fetch existing messages from the cache
                var existingMessagesJson = await _cache.GetStringAsync("chat_messages");

                // Deserialize existing messages or initialize an empty list
                var messages = existingMessagesJson != null ?
                    JsonSerializer.Deserialize<List<ChatMessages>>(existingMessagesJson) :
                    new List<ChatMessages>();

                // Send the cached messages to the calling client
                await Clients.Caller.SendAsync("ReceiveCachedMessages", messages);
            }
            catch (Exception ex)
            {
                // Handle exceptions, if any
                _logger.LogError(ex, "Error occurred while retrieving cached messages");
            }
        }


    }
}