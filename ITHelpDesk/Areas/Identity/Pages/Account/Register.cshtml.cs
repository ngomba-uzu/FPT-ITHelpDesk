﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITHelpDesk.Areas.Identity.Pages.Account
{
  
        public class RegisterModel : PageModel
        {
            private readonly SignInManager<IdentityUser> _signInManager;
            private readonly UserManager<IdentityUser> _userManager;
            private readonly IUserStore<IdentityUser> _userStore;
            private readonly IUserEmailStore<IdentityUser> _emailStore;
            private readonly ILogger<RegisterModel> _logger;
            private readonly IEmailSender _emailSender;
            private readonly ApplicationDbContext _context;

            public RegisterModel(
                UserManager<IdentityUser> userManager,
                IUserStore<IdentityUser> userStore,
                SignInManager<IdentityUser> signInManager,
                ILogger<RegisterModel> logger,
                IEmailSender emailSender,
                ApplicationDbContext context)
            {
                _userManager = userManager;
                _userStore = userStore;
                _emailStore = GetEmailStore();
                _signInManager = signInManager;
                _logger = logger;
                _emailSender = emailSender;
                _context = context;
            }

            [BindProperty]
            public InputModel Input { get; set; }

            public string ReturnUrl { get; set; }

            public IList<AuthenticationScheme> ExternalLogins { get; set; }

            public List<SelectListItem> Departments { get; set; }
            public List<SelectListItem> Ports { get; set; }

            public class InputModel
            {
                [Required]
                [Display(Name = "First Name")]
                public string FirstName { get; set; }

                [Required]
                [Display(Name = "Last Name")]
                public string LastName { get; set; }

                [Required]
                [EmailAddress]
                [Display(Name = "Email")]
                public string Email { get; set; }

                [Required]
                [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
                [DataType(DataType.Password)]
                [Display(Name = "Password")]
                public string Password { get; set; }

                [DataType(DataType.Password)]
                [Display(Name = "Confirm password")]
                [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
                public string ConfirmPassword { get; set; }

                [Required]
                [Display(Name = "Department")]
                public int DepartmentId { get; set; }

                [Required]
                [Display(Name = "Port")]
                public int PortId { get; set; }
            }

            public async Task OnGetAsync(string returnUrl = null)
            {
                ReturnUrl = returnUrl;
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                Departments = await _context.Departments
                    .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DepartmentName })
                    .ToListAsync();

                Ports = await _context.Ports
                    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.PortName })
                    .ToListAsync();
            }

            public async Task<IActionResult> OnPostAsync(string returnUrl = null)
            {
                returnUrl ??= Url.Content("~/");
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                if (ModelState.IsValid)
                {
                    // Generate username from firstname_lastname
                    var username = $"{Input.FirstName}_{Input.LastName}".ToLower();

                // Check for existing user with same first/last name combination
                var users = _userManager.Users.OfType<ApplicationUser>();
                var existingUser = await users
                               .FirstOrDefaultAsync(u =>
                            u.FirstName == Input.FirstName &&
                            u.LastName == Input.LastName);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError(string.Empty, "A user with this first and last name already exists.");
                        await RepopulateDropdowns();
                        return Page();
                    }

                    var user = new ApplicationUser
                    {
                        UserName = username,
                        Email = Input.Email,
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        DepartmentId = Input.DepartmentId,
                        PortId = Input.PortId,
                        CreatedOnDateTime = DateTime.Now,
                        FullName = $"{Input.FirstName} {Input.LastName}"
                    };

                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");
                        await _userManager.AddToRoleAsync(user, "User");

                        // Email confirmation logic
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        try
                        {
                            await _emailSender.SendEmailAsync(
                                Input.Email,
                                "Confirm your email",
                                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Email sending failed");
                            // Continue registration even if email fails
                        }

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("/Account/Login");
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                await RepopulateDropdowns();
                return Page();
            }

            private async Task RepopulateDropdowns()
            {
                Departments = await _context.Departments
                    .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DepartmentName })
                    .ToListAsync();

                Ports = await _context.Ports
                    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.PortName })
                    .ToListAsync();
            }

            private IdentityUser CreateUser()
            {
                try
                {
                    return Activator.CreateInstance<IdentityUser>();
                }
                catch
                {
                    throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                        $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor.");
                }
            }

            private IUserEmailStore<IdentityUser> GetEmailStore()
            {
                if (!_userManager.SupportsUserEmail)
                {
                    throw new NotSupportedException("The default UI requires a user store with email support.");
                }
                return (IUserEmailStore<IdentityUser>)_userStore;
            }
        }
    }