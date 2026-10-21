// <copyright file="Logout.cshtml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using EnterpriseIam.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnterpriseIam.Identity.Api.Pages.Account;

/// <summary>Ends the local identity-provider session using an antiforgery-protected POST.</summary>
public sealed class LogoutModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    /// <summary>Ends the local sign-in session.</summary>
    /// <returns>A safe local redirect.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync();
        return this.LocalRedirect("/");
    }
}
