using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class ListOfFriendModel : PageModel
    {
        readonly IFriendsService _service = null;
        readonly ILogger<ListOfFriendModel> _logger = null;

        [BindProperty]
        public bool UseSeeds { get; set; } = true;
        
        public List<IFriend> Friend { get; set; }

        public int NrOfGroups { get; set; }

        //Pagination
        public int NrOfPages { get; set; }
        public int PageSize { get; } = 10;

        public int ThisPageNr { get; set; } = 0;
        public int PrevPageNr { get; set; } = 0;
        public int NextPageNr { get; set; } = 0;
        public int NrVisiblePages { get; set; } = 0;

        //ModelBinding for the form
        [BindProperty]
        public string SearchFilter { get; set; } = null;

        //will execute on a Get request
        public async Task<IActionResult> OnGet()
        {   
            //Read a QueryParameters
            if (int.TryParse(Request.Query["pagenr"], out int pagenr))
            {
                ThisPageNr = pagenr;
            }

            SearchFilter = Request.Query["search"];

            //Use the Service
            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Friend = resp.PageItems;
            NrOfGroups = resp.DbItemsCount;

            //Pagination
            UpdatePagination(resp.DbItemsCount);

            return Page();
        }

        private void UpdatePagination(int nrOfItems)
        {
            //Pagination
            NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
            PrevPageNr = Math.Max(0, ThisPageNr - 1);
            NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
            NrVisiblePages = Math.Min(10, NrOfPages);
        }

        public async Task<IActionResult> OnPostSearch()
        {
            //Use the Service
            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Friend = resp.PageItems;
            NrOfGroups = resp.DbItemsCount;

            //Pagination
            UpdatePagination(resp.DbItemsCount);

            //Page is rendered as the postback is part of the form tag
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteGroup(Guid groupId)
        {
            await _service.DeleteFriendAsync(groupId);

            //Use the Service
            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Friend = resp.PageItems;
            NrOfGroups = resp.DbItemsCount;

            //Pagination
            UpdatePagination(resp.DbItemsCount);

            return Page();
        }

        //Inject services just like in WebApi
        public ListOfFriendModel(IFriendsService service, ILogger<ListOfFriendModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
