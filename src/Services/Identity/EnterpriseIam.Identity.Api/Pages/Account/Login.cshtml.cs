// <copyright file="Login.cshtml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using EnterpriseIam.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnterpriseIam.Identity.Api.Pages.Account;

/// <summary>Processes secure global-user interactive sign-in requests.</summary>
public sealed class LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> users) : PageModel
{
    /// <summary>Gets or sets the account username.</summary>
    [BindProperty]
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the supplied password.</summary>
    [BindProperty]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the locally validated return URL.</summary>
    [BindProperty]
    public string ReturnUrl { get; set; } = "/";

    /// <summary>Gets a value indicating whether account authentication failed.</summary>
    public bool HasError
    {
        get; private set;
    }

    /// <summary>Displays the login form.</summary>
    /// <param name="returnUrl">The identity-provider authorization return URL.</param>
    public void OnGet(string? returnUrl)
    {
        this.ReturnUrl = this.SafeReturnUrl(returnUrl);
    }

    /// <summary>Validates the password and establishes the Identity application cookie.</summary>
    /// <returns>The redirect or the original login form.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        this.ReturnUrl = this.SafeReturnUrl(this.ReturnUrl);
        if (!this.ModelState.IsValid || string.IsNullOrWhiteSpace(this.Username) || string.IsNullOrWhiteSpace(this.Password))
        {
            this.HasError = true;
            return this.Page();
        }

        var user = await users.FindByNameAsync(this.Username);
        if (user is null || user.IsDisabled || !await signInManager.CanSignInAsync(user))
        {
            this.HasError = true;
            return this.Page();
        }

        var result = await signInManager.PasswordSignInAsync(user, this.Password, false, true);
        if (!result.Succeeded)
        {
            this.HasError = true;
            return this.Page();
        }

        return this.LocalRedirect(this.ReturnUrl);
    }

    private string SafeReturnUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value) && this.Url.IsLocalUrl(value) ? value : "/";
}
