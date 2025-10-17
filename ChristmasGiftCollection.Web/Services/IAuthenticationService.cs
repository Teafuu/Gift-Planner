using ChristmasGiftCollection.Core.Models;
using ChristmasGiftCollection.Core.Services;

namespace ChristmasGiftCollection.Web.Services;

/// <summary>
/// Service for handling family member authentication with PIN codes
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticate a member with their PIN code
    /// </summary>
    Task<bool> LoginAsync(Guid memberId, string pin, IMemberService memberService);

    /// <summary>
    /// Set or update a member's PIN code
    /// </summary>
    Task<bool> SetPinAsync(Guid memberId, string pin, IMemberService memberService);

    /// <summary>
    /// Log out the current member
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Get the currently authenticated member
    /// </summary>
    Task<Member?> GetCurrentMemberAsync(IMemberService memberService);

    /// <summary>
    /// Get the ID of the currently authenticated member
    /// </summary>
    Guid? GetCurrentMemberId();

    /// <summary>
    /// Check if a member is currently authenticated
    /// </summary>
    bool IsAuthenticated();

    /// <summary>
    /// Initialize authentication from browser storage (called on app start)
    /// </summary>
    Task InitializeFromStorageAsync(IMemberService memberService);

    /// <summary>
    /// Save current member to browser storage for 30 days
    /// </summary>
    Task SaveToStorageAsync();

    /// <summary>
    /// Clear browser storage
    /// </summary>
    Task ClearStorageAsync();
}
