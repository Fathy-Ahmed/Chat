using Chat.Application.Interfaces;
using Chat.API.Contracts.Auth;
using Chat.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chat.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user is null)
            return Unauthorized("Invalid username or password.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Unauthorized("Invalid username or password.");

        var token = _tokenService.CreateToken(user);
        return Ok(new LoginResponseDto(token, user.Id, user.UserName ?? string.Empty, user.Email));
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
            return Conflict("Username is already taken.");

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return BadRequest(string.Join(" ", createResult.Errors.Select(e => e.Description)));

        var token = _tokenService.CreateToken(user);
        return Ok(new LoginResponseDto(token, user.Id, user.UserName ?? string.Empty, user.Email));
    }
}
