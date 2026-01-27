using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.NewsArticles
{
    public class CreateModel : PageModel
    {
        private readonly INewsArticleService _newsService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;
        private readonly INewsTagService _newsTagService;

        public CreateModel(
            INewsArticleService newsService,
            ICategoryService categoryService,
            ITagService tagService,
            INewsTagService newsTagService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
            _tagService = tagService;
            _newsTagService = newsTagService;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; } = new();

        [BindProperty]
        public List<int> SelectedTagIds { get; set; } = new();

        public List<Category> Categories { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return Page();
            }

            try
            {
                // Set default values
                NewsArticle.CreatedDate = DateTime.Now;
                NewsArticle.ModifiedDate = DateTime.Now;
                NewsArticle.NewsStatus = true;

                // Get current user from session/cookie
                var userId = HttpContext.Session.GetInt32("UserId") ?? 1;
                NewsArticle.CreatedById = (short)userId;
                NewsArticle.UpdatedById = (short)userId;

                // Create article
                await _newsService.CreateAsync(NewsArticle);

                // Add tags
                if (SelectedTagIds.Any())
                {
                    await _newsTagService.UpdateArticleTagsAsync(NewsArticle.NewsArticleId, SelectedTagIds);
                }

                TempData["SuccessMessage"] = "Article created successfully!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating article: {ex.Message}");
                await LoadDropdownsAsync();
                return Page();
            }
        }

        private async Task LoadDropdownsAsync()
        {
            Categories = await _categoryService.SearchAsync(null, true);
            Tags = await _tagService.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(Categories, "CategoryId", "CategoryName");
        }
    }
}