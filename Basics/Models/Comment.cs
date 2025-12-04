using Microsoft.Extensions.Hosting;
using System;

namespace Basics.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Text { get; set; } = string.Empty; // Yorum içeriği
        public DateTime PublishedOn { get; set; } = DateTime.Now;

        public string UserName { get; set; } = string.Empty;

        public int PostId { get; set; }
        public BlogPost Post { get; set; } // gezinme özelliği
    }
}
