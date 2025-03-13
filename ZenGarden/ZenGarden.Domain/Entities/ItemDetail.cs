﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZenGarden.Domain.Entities;

public partial class ItemDetail
{

    public int ItemDetailId { get; set; }
   
    public int ItemId { get; set; } // Liên kết với bảng Item

 
    public string Description { get; set; } // Mô tả item


    public string Type { get; set; } // Loại item (background, music, xp_boost, xp_protect)

    public string MediaUrl { get; set; } // Đường dẫn file ảnh hoặc nhạc

    public string Effect { get; set; } // Chứa JSON hiệu ứng

    public int? Duration { get; set; } // Thời gian hiệu lực (giây), NULL nếu vĩnh viễn

    public int Sold { get; set; } = 0; // Số lần bán 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Item Item { get; set; }



}