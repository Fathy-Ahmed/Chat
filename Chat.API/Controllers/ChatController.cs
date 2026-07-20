using Chat.Application.Interfaces;
using Chat.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chat.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }


    [HttpGet("history/{otherUserId}")]
    public async Task<IActionResult> GetHistory(string otherUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var history = await _chatService.GetHistoryAsync(userId, otherUserId);
        return Ok(history);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var users = await _chatService.GetAvailableUsersAsync(currentUserId);
        return Ok(users);
    }

}
