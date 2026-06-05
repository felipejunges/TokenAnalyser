using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TokenAnalyser.Data;

namespace TokenAnalyser.Services;

public class UserManagementService(
    IDbContextFactory<AppDbContext> dbFactory,
    UserManager<IdentityUser> userManager)
{
    public async Task<List<(IdentityUser User, IList<string> Roles)>> GetAllUsersWithRolesAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var users = await db.Users.OrderBy(u => u.Email).ToListAsync();
        var result = new List<(IdentityUser, IList<string>)>();
        foreach (var u in users)
            result.Add((u, await userManager.GetRolesAsync(u)));
        return result;
    }

    public async Task<IdentityResult> CreateAsync(string email, string password, bool isAdmin)
    {
        var user = new IdentityUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded && isAdmin)
            await userManager.AddToRoleAsync(user, "Admin");
        return result;
    }

    public async Task<IdentityResult> UpdateAsync(string userId, string email, bool isAdmin)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        user.Email = email;
        user.UserName = email;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) return result;

        var inRole = await userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin && !inRole) await userManager.AddToRoleAsync(user, "Admin");
        if (!isAdmin && inRole) await userManager.RemoveFromRoleAsync(user, "Admin");

        return result;
    }

    public async Task<IdentityResult> ResetPasswordAsync(string userId, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<IdentityResult> DeleteAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        return await userManager.DeleteAsync(user);
    }
}

