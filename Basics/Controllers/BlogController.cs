using Basics.Data;
using Basics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Basics.Controllers;

public class BlogController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var posts = await _context.BlogPosts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(posts);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var post = await _context.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == id);

        return post is null ? NotFound() : View(post);
    }

    [Authorize]
    public IActionResult Create() => View(new BlogPost());

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPost post)
    {
        if (!ModelState.IsValid)
        {
            return View(post);
        }

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        post.AuthorId = userId;
        post.CreatedAt = DateTime.UtcNow;

        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post is null)
        {
            return NotFound();
        }

        if (!IsOwner(post))
        {
            return Forbid();
        }

        return View(post);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogPost model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var post = await _context.BlogPosts.FindAsync(id);
        if (post is null)
        {
            return NotFound();
        }

        if (!IsOwner(post))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        post.Title = model.Title;
        post.Content = model.Content;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.BlogPosts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null)
        {
            return NotFound();
        }

        if (!IsOwner(post))
        {
            return Forbid();
        }

        return View(post);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post is null)
        {
            return NotFound();
        }

        if (!IsOwner(post))
        {
            return Forbid();
        }

        _context.BlogPosts.Remove(post);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> MyPosts()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var posts = await _context.BlogPosts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(posts);
    }

    private bool IsOwner(BlogPost post)
    {
        var userId = _userManager.GetUserId(User);
        return userId is not null && post.AuthorId == userId;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddComment(int PostId, string Text) // UserName parametresi kaldýrýldý
    {
        var currentUserName = User.Identity.Name;

        if (string.IsNullOrWhiteSpace(currentUserName) || string.IsNullOrWhiteSpace(Text))
        {
            return RedirectToAction("Details", new { id = PostId });
        }

        var entity = new Comment
        {
            PostId = PostId,
            UserName = currentUserName, // Giriþ yapan kullanýcý adý kullanýldý
            Text = Text,
            PublishedOn = DateTime.Now
        };

        _context.Comments.Add(entity);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = PostId });
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleLike(int PostId, string UserName)
    {
        var currentUserName = User.Identity.Name;

        if (string.IsNullOrWhiteSpace(currentUserName))
        {
            return RedirectToPage("/Account/Login");
        }

        // 1. Kullanýcýnýn bu yazýyý daha önce beðenip beðenmediðini kontrol et.
        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == PostId && l.UserName == UserName);

        if (existingLike == null)
        {
            // (Like!)
            var newLike = new Like
            {
                PostId = PostId,
                UserName = UserName,
                LikedOn = DateTime.Now
            };
            _context.Likes.Add(newLike);
        }
        else
        {
            // (Unlike!)
            _context.Likes.Remove(existingLike);
        }

        // Deðiþiklikleri veritabanýna kaydet.
        await _context.SaveChangesAsync();

        // Ýþlem bitince kullanýcýyý tekrar yazýnýn olduðu sayfaya gönder.
        return RedirectToAction("Details", new { id = PostId });
    }
}

