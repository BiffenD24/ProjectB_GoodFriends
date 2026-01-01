using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages
{
	public class ViewGroupModel : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<ViewGroupModel> _logger = null;

        public IFriend Friend { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Guid _friendId = Guid.Parse(Request.Query["id"]);
            Friend = (await _service.ReadFriendAsync(_friendId, false)).Item;

            return Page();
        }

        //Inject services just like in WebApi
        public ViewGroupModel(IFriendsService service, ILogger<ViewGroupModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
