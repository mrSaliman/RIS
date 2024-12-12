﻿namespace Chat.Models;

public class User
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!; // "admin" или "user"
}