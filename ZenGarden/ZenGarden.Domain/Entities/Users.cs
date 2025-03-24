﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

#nullable enable
public partial class Users
{
    public int UserId { get; set; }
    public int? RoleId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? OtpCodeHash { get; set; }
    public DateTime? OtpExpiry { get; set; }
 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual Bag? Bag { get; set; }
    public virtual ICollection<PurchaseHistory> PurchaseHistory { get; set; } = new List<PurchaseHistory>();
    public virtual Roles? Role { get; set; }
    public virtual ICollection<TradeHistory> TradeHistoryUserA { get; set; } = new List<TradeHistory>();
    public virtual ICollection<TradeHistory> TradeHistoryUserB { get; set; } = new List<TradeHistory>();
    public virtual ICollection<Transactions> Transactions { get; set; } = new List<Transactions>();
    public virtual UserExperience? UserExperience { get; set; }
    public virtual ICollection<UserTree> UserTree { get; set; } = new List<UserTree>();
    public virtual ICollection<UserXpLog> UserXpLog { get; set; } = new List<UserXpLog>();
    public virtual ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual UserConfig? UserConfig { get; set; }
    public virtual Wallet? Wallet { get; set; }



}