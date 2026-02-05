using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Services;

namespace MinCoreBank.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersViewController : Controller
    {
        private readonly IUserService _userService;
        private const int PageSize = 10; // 10 rows per page

        public UsersViewController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index(int page = 1, string statusFilter = "all", string roleFilter = "all", string branchFilter = "all", string searchTerm = "")
        {
            // Get all users
            var result = await _userService.GetAllUsers();

            if (!result.Success)
            {
                // Handle error - maybe return an empty list with error message
                ViewBag.Error = result.Message;
                return View(new List<Users>());
            }

            var usersList = result.Data.ToList();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                usersList = usersList.Where(u => u.Status == statusFilter).ToList();
            }

            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "all")
            {
                usersList = usersList.Where(u => u.Role == roleFilter).ToList();
            }

            if (!string.IsNullOrEmpty(branchFilter) && branchFilter != "all")
            {
                usersList = usersList.Where(u => u.BranchId == branchFilter).ToList();
            }

            // Apply search if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                usersList = usersList.Where(u =>
                    u.Id.ToLower().Contains(searchTerm) ||
                    u.Name_ar.ToLower().Contains(searchTerm) ||
                    u.Name_en.ToLower().Contains(searchTerm)).ToList();
            }

            // Calculate pagination
            var totalCount = usersList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            // Ensure page is within valid range
            page = page < 1 ? 1 : page > totalPages ? totalPages : page;

            // Get users for current page
            var pagedUsers = usersList
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Pass data to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = PageSize;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.BranchFilter = branchFilter;
            ViewBag.SearchTerm = searchTerm;

            return View(pagedUsers);
        }
    }
}