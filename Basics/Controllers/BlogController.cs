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
}

