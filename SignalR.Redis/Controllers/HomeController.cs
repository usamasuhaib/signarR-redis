using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using SignalR.Redis.Models;
using System.Diagnostics;
using System.Text.Json;

namespace SignalR.Redis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDistributedCache _cache;

        public HomeController(ILogger<HomeController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
            _logger.LogDebug(1, "NLog injected into HomeController");

        }

        public IActionResult Index()
        {
            _logger.LogInformation("Hello, this is the index!");
            return View();
        }

        [Authorize]
        public IActionResult Chats()
        {


            return View();
        }
public async Task<ActionResult<List<ChatMessages>>> GetChatMessages()
        {
            try
            {
                // Retrieve chat messages from Redis cache
                string? existingMessagesJson = await _cache.GetStringAsync("chat_messages");
                if (existingMessagesJson != null)
                {
                    var existingMessages = JsonSerializer.Deserialize<List<ChatMessages>>(existingMessagesJson);
                    return Ok(existingMessages); // Return the deserialized list of ChatMessage objects
                }
                else
                {
                    return Ok(new List<ChatMessages>()); // Return an empty list if no messages found
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, ex.Message);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
