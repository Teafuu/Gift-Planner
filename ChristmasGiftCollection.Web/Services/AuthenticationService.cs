using ChristmasGiftCollection.Core.Models;
using ChristmasGiftCollection.Core.Services;
using Microsoft.JSInterop;

namespace ChristmasGiftCollection.Web.Services;

/// <summary>
/// Session-based authentication service for family members (works with Blazor Server)
/// Scoped per circuit - each user connection gets their own instance with isolated authentication state
/// Persists to browser localStorage for 30-day "remember me" functionality
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IJSRuntime _jsRuntime;
    private Guid? _currentMemberId;

    public AuthenticationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> LoginAsync(Guid memberId, string pin, IMemberService memberService)
    {
        var member = await memberService.GetMemberByIdAsync(memberId);
        if (member == null) return false;

        // Verify PIN (plain text)
        if (member.PinCode != pin)
            return false;

        // Store member ID in memory for this circuit/session
        _currentMemberId = memberId;

        // Save to localStorage for 30-day persistence
        await SaveToStorageAsync();

        return true;
    }

    public async Task<bool> SetPinAsync(Guid memberId, string pin, IMemberService memberService)
    {
        // Validate PIN format (4 digits)
        if (pin.Length != 4 || !pin.All(char.IsDigit))
            return false;

        // Store PIN as plain text
        await memberService.SetPinCodeAsync(memberId, pin);

        // Automatically log in after setting PIN
        _currentMemberId = memberId;

        // Save to localStorage for 30-day persistence
        await SaveToStorageAsync();

        return true;
    }

    public async Task LogoutAsync()
    {
        _currentMemberId = null;
        await ClearStorageAsync();
    }

    public async Task<Member?> GetCurrentMemberAsync(IMemberService memberService)
    {
        if (!_currentMemberId.HasValue) return null;
        return await memberService.GetMemberByIdAsync(_currentMemberId.Value);
    }

    public Guid? GetCurrentMemberId()
    {
        return _currentMemberId;
    }

    public bool IsAuthenticated() => _currentMemberId.HasValue;

    public async Task InitializeFromStorageAsync(IMemberService memberService)
    {
        try
        {
            var storedMemberIdString = await _jsRuntime.InvokeAsync<string>("authStorage.getRememberedMember");

            if (string.IsNullOrEmpty(storedMemberIdString))
                return;

            if (Guid.TryParse(storedMemberIdString, out var storedMemberId))
            {
                // Verify member still exists
                var member = await memberService.GetMemberByIdAsync(storedMemberId);
                if (member != null)
                {
                    _currentMemberId = storedMemberId;
                }
                else
                {
                    // Member doesn't exist anymore, clear storage
                    await ClearStorageAsync();
                }
            }
        }
        catch (JSException)
        {
            // JavaScript not ready or localStorage not available, ignore
        }
        catch (InvalidOperationException)
        {
            // JSRuntime not initialized yet, ignore
        }
    }

    public async Task SaveToStorageAsync()
    {
        if (!_currentMemberId.HasValue)
            return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("authStorage.saveRememberedMember", _currentMemberId.Value.ToString());
        }
        catch (JSException)
        {
            // JavaScript not ready or localStorage not available, ignore
        }
        catch (InvalidOperationException)
        {
            // JSRuntime not initialized yet, ignore
        }
    }

    public async Task ClearStorageAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("authStorage.clearRememberedMember");
        }
        catch (JSException)
        {
            // JavaScript not ready or localStorage not available, ignore
        }
        catch (InvalidOperationException)
        {
            // JSRuntime not initialized yet, ignore
        }
    }
}
