using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Models.DTO;
using Services.Interfaces;

namespace AppRazor.Pages.Friends
{
    public class EditFriendModel : PageModel
    {
        readonly IFriendsService _service = null;
        readonly ILogger<EditFriendModel> _logger = null;

        [BindProperty]
        public Guid? FriendId { get; set; }

        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public DateTime? Birthday { get; set; }

        [BindProperty]
        public Guid? AddressId { get; set; }

        public IFriend Friend { get; set; }
        public bool IsNewFriend { get; set; }
        public string ErrorMessage { get; set; }
        public bool ShowSuccessMessage { get; set; }
        public Dictionary<string, string> ValidationErrors { get; set; } = new();

        public async Task<IActionResult> OnGet(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    // New friend
                    IsNewFriend = true;
                    Friend = null;
                    return Page();
                }

                if (!Guid.TryParse(id, out var friendId))
                {
                    _logger.LogWarning("Invalid friend ID provided");
                    return RedirectToPage("/Friends/Overview");
                }

                // Load existing friend
                var result = await _service.ReadFriendAsync(friendId, false);
                if (result?.Item == null)
                {
                    _logger.LogWarning($"Friend with ID {friendId} not found");
                    return RedirectToPage("/Friends/Overview");
                }

                Friend = result.Item;
                FriendId = Friend.FriendId;
                FirstName = Friend.FirstName;
                LastName = Friend.LastName;
                Email = Friend.Email;
                Birthday = Friend.Birthday;
                AddressId = Friend.Address?.AddressId;
                IsNewFriend = false;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading friend for edit: {ex.Message}");
                ErrorMessage = "An error occurred while loading the friend details.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                // Server-side validation
                ValidateInput();
                
                if (ValidationErrors.Count > 0)
                {
                    ErrorMessage = "Please correct the highlighted fields.";
                    IsNewFriend = !FriendId.HasValue || FriendId == Guid.Empty;
                    if (!IsNewFriend)
                    {
                        var result = await _service.ReadFriendAsync(FriendId.Value, false);
                        if (result?.Item != null)
                        {
                            Friend = result.Item;
                            AddressId = Friend.Address?.AddressId;
                        }
                    }
                    return Page();
                }

                // Create or update friend
                var friendDto = new FriendCuDto
                {
                    FriendId = FriendId == Guid.Empty ? null : FriendId,
                    FirstName = FirstName?.Trim(),
                    LastName = LastName?.Trim(),
                    Email = Email?.Trim(),
                    Birthday = Birthday,
                    AddressId = AddressId
                };

                var response = FriendId.HasValue && FriendId != Guid.Empty
                    ? await _service.UpdateFriendAsync(friendDto)
                    : await _service.CreateFriendAsync(friendDto);

                if (response?.Item != null)
                {
                    _logger.LogInformation($"Friend {(IsNewFriend ? "created" : "updated")}: {response.Item.FriendId}");
                    ShowSuccessMessage = true;
                    Friend = response.Item;
                    FriendId = Friend.FriendId;
                    FirstName = Friend.FirstName;
                    LastName = Friend.LastName;
                    Email = Friend.Email;
                    Birthday = Friend.Birthday;
                    AddressId = Friend.Address?.AddressId;
                    IsNewFriend = false;
                    
                    // Redirect after 2 seconds
                    return Redirect($"/Friends/Details/{Friend.FriendId}");
                }

                ErrorMessage = "Failed to save friend. Please try again.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving friend: {ex.Message}");
                ErrorMessage = $"An error occurred: {ex.Message}";
                IsNewFriend = !FriendId.HasValue || FriendId == Guid.Empty;
                return Page();
            }
        }

        private void ValidateInput()
        {
            ValidationErrors.Clear();

            // Validate FirstName
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                ValidationErrors["FirstName"] = "First name is required.";
            }
            else if (FirstName.Length > 100)
            {
                ValidationErrors["FirstName"] = "First name cannot exceed 100 characters.";
            }

            // Validate LastName
            if (string.IsNullOrWhiteSpace(LastName))
            {
                ValidationErrors["LastName"] = "Last name is required.";
            }
            else if (LastName.Length > 100)
            {
                ValidationErrors["LastName"] = "Last name cannot exceed 100 characters.";
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ValidationErrors["Email"] = "Email is required.";
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                ValidationErrors["Email"] = "Email must be a valid email address.";
            }
            else if (Email.Length > 255)
            {
                ValidationErrors["Email"] = "Email cannot exceed 255 characters.";
            }

            // Validate Birthday
            if (Birthday.HasValue)
            {
                if (Birthday.Value > DateTime.Now)
                {
                    ValidationErrors["Birthday"] = "Birthday must be in the past.";
                }
                else if (Birthday.Value.Year < 1900)
                {
                    ValidationErrors["Birthday"] = "Birthday must be after 1900.";
                }
            }
        }

        public EditFriendModel(IFriendsService service, ILogger<EditFriendModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
