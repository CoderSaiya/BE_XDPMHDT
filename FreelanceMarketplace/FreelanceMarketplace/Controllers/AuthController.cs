﻿using FreelanceMarketplace.Data;
using FreelanceMarketplace.Models.DTOs.Req;
using FreelanceMarketplace.Models.DTOs.Res;
using FreelanceMarketplace.Models;
using FreelanceMarketplace.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Users> _passwordHasher;

    public AuthController(IAuthService authService, IUserService userService, AppDbContext context)
    {
        _authService = authService;
        _userService = userService;
        _context = context;
        _passwordHasher = new PasswordHasher<Users>();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterReq registerReq)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new Response<string>
            {
                Success = false,
                Message = "Invalid data.",
                Data = null
            });
        }

        var result = await _userService.RegisterUserAsync(registerReq);
        if (!result)
        {
            return BadRequest(new Response<string>
            {
                Success = false,
                Message = "Username is already taken.",
                Data = null
            });
        }

        return Ok(new Response<string>
        {
            Success = true,
            Message = "User registered successfully.",
            Data = null
        });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(int userId, string token)
    {
        var result = await _userService.ConfirmEmailAsync(userId, token);
        if (!result)
        {
            return BadRequest(new Response<string>
            {
                Success = false,
                Message = "Invalid confirmation link or token.",
                Data = null
            });
        }

        return Ok(new Response<string>
        {
            Success = true,
            Message = "Email confirmed successfully.",
            Data = null
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginReq request)
    {
        string accessToken = _authService.Login(request.Username, request.Password);
        if (accessToken == null)
        {
            return Unauthorized(new Response<string>
            {
                Success = false,
                Message = "Invalid credentials.",
                Data = null
            });
        }

        Users user = _userService.GetUserByUsername(request.Username);
        string refreshToken = _authService.GenerateRefreshToken();
        _userService.SaveRefreshToken(user.Id, refreshToken);

        return Ok(new Response<object>
        {
            Success = true,
            Message = "Login successful",
            Data = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }
        });
    }

    [HttpPost("refresh-token")]
    public IActionResult RefreshToken([FromBody] TokenDto request)
    {
        var newAccessToken = _authService.RefreshToken(request.RefreshToken);
        if (newAccessToken == null)
        {
            return Unauthorized(new Response<string>
            {
                Success = false,
                Message = "Invalid refresh token.",
                Data = null
            });
        }

        return Ok(new Response<string>
        {
            Success = true,
            Message = "Token refreshed",
            Data = newAccessToken
        });
    }

    [HttpGet("signin-google")]
    public IActionResult SignInWithGoogle()
    {
        var redirectUrl = Url.Action("GoogleResponse", "GoogleLogin");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, "Google");
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync("Google");
        if (result.Succeeded)
        {
            // success
            var claims = result.Principal.Identities.First().Claims;
            _authService.GenerateAccessToken(claims);
        }

        return Redirect("/");
    }

    [HttpGet("signin-facebook")]
    public IActionResult SignInWithFacebook()
    {
        var redirectUrl = Url.Action("FacebookResponse", "FacebookLogin");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, "Facebook");
    }

    [HttpGet("facebook-response")]
    public async Task<IActionResult> FacebookResponse()
    {
        var result = await HttpContext.AuthenticateAsync("Facebook");
        if (result.Succeeded)
        {
            // success
            var claims = result.Principal.Identities.First().Claims;
            _authService.GenerateAccessToken(claims);
        }

        return Redirect("/");
    }
}
