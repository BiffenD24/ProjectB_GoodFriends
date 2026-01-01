using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Models.DTO;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class EditAddressModel : PageModel
    {
        readonly IAddressesService _addressService = null;
        readonly IFriendsService _friendsService = null;
        readonly ILogger<EditAddressModel> _logger = null;

        [BindProperty]
        public Guid? AddressId { get; set; }

        [BindProperty]
        public string StreetAddress { get; set; }

        [BindProperty]
        public int ZipCode { get; set; }

        [BindProperty]
        public string City { get; set; }

        [BindProperty]
        public string Country { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? FriendId { get; set; }

        public IAddress Address { get; set; }
        public bool IsNewAddress { get; set; }
        public string ErrorMessage { get; set; }
        public bool ShowSuccessMessage { get; set; }
        public Dictionary<string, string> ValidationErrors { get; set; } = new();

        public async Task<IActionResult> OnGet(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    // New address
                    IsNewAddress = true;
                    Address = null;
                    return Page();
                }

                if (!Guid.TryParse(id, out var addressId))
                {
                    _logger.LogWarning("Invalid address ID provided");
                    return RedirectToPage("/Friends/Overview");
                }

                // Load existing address
                var result = await _addressService.ReadAddressAsync(addressId, false);
                if (result?.Item == null)
                {
                    _logger.LogWarning($"Address with ID {addressId} not found");
                    return RedirectToPage("/Friends/Overview");
                }

                Address = result.Item;
                AddressId = Address.AddressId;
                StreetAddress = Address.StreetAddress;
                ZipCode = Address.ZipCode;
                City = Address.City;
                Country = Address.Country;
                IsNewAddress = false;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading address for edit: {ex.Message}");
                ErrorMessage = "An error occurred while loading the address details.";
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
                    IsNewAddress = !AddressId.HasValue || AddressId == Guid.Empty;
                    if (!IsNewAddress)
                    {
                        var result = await _addressService.ReadAddressAsync(AddressId.Value, false);
                        if (result?.Item != null)
                        {
                            Address = result.Item;
                        }
                    }
                    return Page();
                }

                // Create or update address
                var addressDto = new AddressCuDto
                {
                    AddressId = AddressId == Guid.Empty ? null : AddressId,
                    StreetAddress = StreetAddress?.Trim(),
                    ZipCode = ZipCode,
                    City = City?.Trim(),
                    Country = Country?.Trim()
                };

                var response = AddressId.HasValue && AddressId != Guid.Empty
                    ? await _addressService.UpdateAddressAsync(addressDto)
                    : await _addressService.CreateAddressAsync(addressDto);

                if (response?.Item != null)
                {
                    _logger.LogInformation($"Address {(IsNewAddress ? "created" : "updated")}: {response.Item.AddressId}");
                    
                    // If this is a new address and associated with a friend, update the friend
                    if (IsNewAddress && FriendId.HasValue)
                    {
                        var friendResult = await _friendsService.ReadFriendAsync(FriendId.Value, false);
                        if (friendResult?.Item != null)
                        {
                            var friendDto = new FriendCuDto(friendResult.Item)
                            {
                                AddressId = response.Item.AddressId
                            };
                            await _friendsService.UpdateFriendAsync(friendDto);
                        }
                    }

                    ShowSuccessMessage = true;
                    Address = response.Item;
                    AddressId = Address.AddressId;
                    StreetAddress = Address.StreetAddress;
                    ZipCode = Address.ZipCode;
                    City = Address.City;
                    Country = Address.Country;
                    IsNewAddress = false;
                    
                    // Redirect after success
                    if (FriendId.HasValue)
                    {
                        return Redirect($"/Friends/Details/{FriendId}");
                    }
                    else
                    {
                        return Redirect($"/Friends/Overview");
                    }
                }

                ErrorMessage = "Failed to save address. Please try again.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving address: {ex.Message}");
                ErrorMessage = $"An error occurred: {ex.Message}";
                IsNewAddress = !AddressId.HasValue || AddressId == Guid.Empty;
                return Page();
            }
        }

        private void ValidateInput()
        {
            ValidationErrors.Clear();

            // Validate StreetAddress
            if (string.IsNullOrWhiteSpace(StreetAddress))
            {
                ValidationErrors["StreetAddress"] = "Street address is required.";
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(StreetAddress, @"[^a-zA-Z0-9\s]"))
            {
                ValidationErrors["StreetAddress"] = "Street address can only contain letters, numbers, and spaces.";
            }
            else if (StreetAddress.Length > 255)
            {
                ValidationErrors["StreetAddress"] = "Street address cannot exceed 255 characters.";
            }

            // Validate City
            if (string.IsNullOrWhiteSpace(City))
            {
                ValidationErrors["City"] = "City is required.";
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(City, @"[^a-zA-Z0-9\s]"))
            {
                ValidationErrors["City"] = "City can only contain letters, numbers, and spaces.";
            }
            else if (City.Length > 100)
            {
                ValidationErrors["City"] = "City cannot exceed 100 characters.";
            }

            // Validate ZipCode
            if (ZipCode < 0 || ZipCode > 999999)
            {
                ValidationErrors["ZipCode"] = "Zip code must be between 0 and 999999.";
            }

            // Validate Country
            if (string.IsNullOrWhiteSpace(Country))
            {
                ValidationErrors["Country"] = "Country is required.";
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(Country, @"[^a-zA-Z0-9\s]"))
            {
                ValidationErrors["Country"] = "Country can only contain letters, numbers, and spaces.";
            }
            else if (Country.Length > 100)
            {
                ValidationErrors["Country"] = "Country cannot exceed 100 characters.";
            }
        }

        public EditAddressModel(IAddressesService addressService, IFriendsService friendsService, ILogger<EditAddressModel> logger)
        {
            _addressService = addressService;
            _friendsService = friendsService;
            _logger = logger;
        }
    }
}
