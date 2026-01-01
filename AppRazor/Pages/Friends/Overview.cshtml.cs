using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages
{
	public class OverviewModel : PageModel
    {
        readonly IFriendsService _service = null;
        readonly ILogger<OverviewModel> _logger = null;

        public Dictionary<string, List<IFriend>> FriendsByCountry { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool UseSeeds { get; set; } = true;

        public async Task<IActionResult> OnGet()
        {
            FriendsByCountry = await _service.ReadFriendsByCountryAsync(UseSeeds, false);
            return Page();
        }

        //Inject services just like in WebApi
        public OverviewModel(IFriendsService service, ILogger<OverviewModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
