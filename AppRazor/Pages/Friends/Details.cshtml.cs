using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages.Friends
{
    public class DetailsModel : PageModel
    {
        readonly IFriendsService _friendsService = null;
        readonly IPetsService _petsService = null;
        readonly IQuotesService _quotesService = null;
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

                var result = await _friendsService.ReadFriendAsync(friendId, false);
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

        public async Task<IActionResult> OnPostDeletePetAsync(string petId, string friendId)
        {
            try
            {
                if (string.IsNullOrEmpty(petId) || !Guid.TryParse(petId, out var petGuid))
                {
                    _logger.LogWarning("Invalid pet ID provided");
                    return RedirectToPage(new { id = friendId });
                }

                var result = await _petsService.DeletePetAsync(petGuid);
                if (result?.Item == null)
                {
                    _logger.LogWarning($"Pet with ID {petId} not found");
                }
                else
                {
                    _logger.LogInformation($"Pet {petId} deleted successfully");
                }

                return RedirectToPage(new { id = friendId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting pet: {ex.Message}");
                return RedirectToPage(new { id = friendId });
            }
        }

        public async Task<IActionResult> OnPostDeleteQuoteAsync(string quoteId, string friendId)
        {
            try
            {
                if (string.IsNullOrEmpty(quoteId) || !Guid.TryParse(quoteId, out var quoteGuid))
                {
                    _logger.LogWarning("Invalid quote ID provided");
                    return RedirectToPage(new { id = friendId });
                }

                var result = await _quotesService.DeleteQuoteAsync(quoteGuid);
                if (result?.Item == null)
                {
                    _logger.LogWarning($"Quote with ID {quoteId} not found");
                }
                else
                {
                    _logger.LogInformation($"Quote {quoteId} deleted successfully");
                }

                return RedirectToPage(new { id = friendId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting quote: {ex.Message}");
                return RedirectToPage(new { id = friendId });
            }
        }

        public DetailsModel(IFriendsService friendsService, IPetsService petsService, IQuotesService quotesService, ILogger<DetailsModel> logger)
        {
            _friendsService = friendsService;
            _petsService = petsService;
            _quotesService = quotesService;
            _logger = logger;
        }
    }
}
