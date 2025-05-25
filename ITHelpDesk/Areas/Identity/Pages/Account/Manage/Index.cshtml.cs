// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITHelpDesk.Data;
using ITHelpDesk.Models;

namespace ITHelpDesk.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _environment = environment;
        }

        public string Username { get; set; }
        public string CurrentRole { get; set; }
        public DateTime CreatedOnDateTime { get; set; }
        public int TechnicianGroupId { get; set; }
        public string FullName { get; set; }
        public string ProfilePictureUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<SelectListItem> Departments { get; set; }
        public List<SelectListItem> Ports { get; set; }

        public class InputModel
        {
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Department")]
            public int DepartmentId { get; set; }

            [Required]
            [Display(Name = "Port")]
            public int PortId { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile ProfilePictureFile { get; set; }
        }

        private async Task LoadAsync(IdentityUser identityUser)
        {
            var username = await _userManager.GetUserNameAsync(identityUser);
            var email = await _userManager.GetEmailAsync(identityUser);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(identityUser);

            // Get user role
            var roles = await _userManager.GetRolesAsync(identityUser);
            CurrentRole = roles.FirstOrDefault() ?? "No Role Assigned";

            // Load departments and ports for dropdowns
            Departments = await _context.Departments
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DepartmentName })
                .ToListAsync();

            Ports = await _context.Ports
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.PortName })
                .ToListAsync();

            // Try to get ApplicationUser if it exists
            var applicationUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == identityUser.Id) as ApplicationUser;

            Input = new InputModel
            {
                Email = email,
                PhoneNumber = phoneNumber,
                FirstName = applicationUser?.FirstName ?? "",
                LastName = applicationUser?.LastName ?? "",
                DepartmentId = applicationUser?.DepartmentId ?? 0,
                PortId = applicationUser?.PortId ?? 0
            };

            FullName = $"{Input.FirstName} {Input.LastName}";
            Username = username;

            // Assign CreatedOnDateTime from ApplicationUser (custom property)
            CreatedOnDateTime = applicationUser?.CreatedOnDateTime ?? DateTime.MinValue;


            if (applicationUser?.ProfilePicture != null)
            {
                ProfilePictureUrl = $"data:{applicationUser.ProfilePictureContentType};base64,{Convert.ToBase64String(applicationUser.ProfilePicture)}";
            }
            else
            {
                // Default profile picture if none is set
                ProfilePictureUrl = "/images/default-profile.png";
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(identityUser);
                return Page();
            }

            // Update email if changed
            var email = await _userManager.GetEmailAsync(identityUser);
            if (Input.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(identityUser, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set email.";
                    return RedirectToPage();
                }
            }

            // Update phone number if changed
            var phoneNumber = await _userManager.GetPhoneNumberAsync(identityUser);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(identityUser, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // Update ApplicationUser specific properties
            var applicationUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == identityUser.Id) as ApplicationUser;
            if (applicationUser != null)
            {
                applicationUser.FirstName = Input.FirstName;
                applicationUser.LastName = Input.LastName;
                applicationUser.DepartmentId = Input.DepartmentId;
                applicationUser.PortId = Input.PortId;

                // Handle profile picture upload
                if (Input.ProfilePictureFile != null && Input.ProfilePictureFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await Input.ProfilePictureFile.CopyToAsync(memoryStream);

                        // Validate file size (e.g., 2MB max)
                        if (memoryStream.Length < 2097152) // 2MB in bytes
                        {
                            applicationUser.ProfilePicture = memoryStream.ToArray();
                            applicationUser.ProfilePictureContentType = Input.ProfilePictureFile.ContentType;
                        }
                        else
                        {
                            ModelState.AddModelError("Input.ProfilePictureFile", "The file is too large (max 2MB).");
                            await LoadAsync(identityUser);
                            return Page();
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }

            await _signInManager.RefreshSignInAsync(identityUser);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePictureAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var applicationUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == identityUser.Id) as ApplicationUser;
            if (applicationUser != null)
            {
                // Clear the profile picture data
                applicationUser.ProfilePicture = null;
                applicationUser.ProfilePictureContentType = null;
                await _context.SaveChangesAsync();
            }

            await _signInManager.RefreshSignInAsync(identityUser);
            StatusMessage = "Your profile picture has been removed";
            return RedirectToPage();
        }
    }
}