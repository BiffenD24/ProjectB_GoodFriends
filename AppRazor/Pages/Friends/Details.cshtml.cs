using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages.Friends
{
    public class DetailsModel : PageModel
    {
        readonly IFriendsService _service = null;
        readonly ILogger<DetailsModel> _logger = null;

        public IFriend Friend { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var friendId))
                {
                    _logger.LogWarning("Invalid friend ID provided");
                    return RedirectToPage("/Friends/Overview");
                }

                var result = await _service.ReadFriendAsync(friendId, false);
                if (result?.Item == null)
                {
                    _logger.LogWarning($"Friend with ID {friendId} not found");
                    return RedirectToPage("/Friends/Overview");
                }

                Friend = result.Item;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading friend details: {ex.Message}");
                return RedirectToPage("/Friends/Overview");
            }
        }

        public DetailsModel(IFriendsService service, ILogger<DetailsModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
