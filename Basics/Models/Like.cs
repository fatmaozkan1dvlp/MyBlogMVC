using Microsoft.Extensions.Hosting;
using System;

namespace Basics.Models 
{
    public class Like
    {
        public int LikeId { get; set; }
        public DateTime LikedOn { get; set; } = DateTime.Now;

        public string? UserName { get; set; }

        // Foreign Key: Hangi yazıya ait?
        public int PostId { get; set; }
        public BlogPost Post { get; set; } 
    }
}
