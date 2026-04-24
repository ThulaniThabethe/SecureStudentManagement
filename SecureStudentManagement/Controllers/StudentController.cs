using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecureStudentManagement.Models;
using SecureStudentManagement.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SecureStudentManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly StudentCosmosDbService _cosmosService;
        private readonly StudentCloudStorageService _storageService;

        public StudentController(
            StudentCosmosDbService cosmosService,
            StudentCloudStorageService storageService)
        {
            _cosmosService = cosmosService;
            _storageService = storageService;
        }

        public async Task<IActionResult> List(string search, int page = 1)
        {
            int pageSize = 5;

            var learners = await _cosmosService.GetPagedLearnersAsync(page, pageSize);

            if (!string.IsNullOrWhiteSpace(search))
            {
                learners = learners.Where(l =>
                    (!string.IsNullOrEmpty(l.FirstName) && l.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.LastName) && l.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.Email) && l.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.id) && l.id.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var totalCount = await _cosmosService.GetTotalCountAsync();

            var vm = new PagedLearnerViewModel
            {
                Learners = learners,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Search = search
            };

            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Learner());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Learner learner, IFormFile profileImage)
        {
            learner.id = Guid.NewGuid().ToString();

            if (profileImage != null && profileImage.Length > 0)
            {
                learner.ProfileImageUrl = await _storageService.SaveProfileImageAsync(profileImage);
            }
            else
            {
                learner.ProfileImageUrl = "";
            }

            learner.IsDeleted = false;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please complete all required fields correctly.";
                return View(learner);
            }

            try
            {
                await _cosmosService.AddLearnerAsync(learner);

                TempData["Success"] = "Learner added successfully.";
                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(learner);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(List));

            var learner = await _cosmosService.GetLearnerByIdAsync(id);

            if (learner == null)
                return RedirectToAction(nameof(List));

            return View(learner);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(List));

            var learner = await _cosmosService.GetLearnerByIdAsync(id);

            if (learner == null)
                return RedirectToAction(nameof(List));

            return View(learner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Learner learner, IFormFile profileImage)
        {
            if (string.IsNullOrWhiteSpace(learner.id))
            {
                TempData["Error"] = "Invalid learner record.";
                return RedirectToAction(nameof(List));
            }

            var existingLearner = await _cosmosService.GetLearnerByIdAsync(learner.id);

            if (existingLearner == null)
            {
                TempData["Error"] = "Learner not found.";
                return RedirectToAction(nameof(List));
            }

            if (profileImage != null && profileImage.Length > 0)
            {
                learner.ProfileImageUrl = await _storageService.SaveProfileImageAsync(profileImage);
            }
            else
            {
                learner.ProfileImageUrl = existingLearner.ProfileImageUrl;
            }

            learner.IsDeleted = existingLearner.IsDeleted;

            ModelState.Remove("profileImage");
            ModelState.Remove(nameof(Learner.ProfileImageUrl));

            if (!ModelState.IsValid)
            {
                return View(learner);
            }

            await _cosmosService.UpdateLearnerAsync(learner);

            TempData["Success"] = "Learner updated successfully.";

            return RedirectToAction(nameof(Edit), new { id = learner.id });
        }


        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(List));

            var learner = await _cosmosService.GetLearnerByIdAsync(id);

            if (learner == null)
                return RedirectToAction(nameof(List));

            return View(learner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            try
            {
                await _cosmosService.SoftDeleteLearnerAsync(id);
                TempData["Success"] = "Learner marked as inactive";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> SoftDelete(string id)
        {
            try
            {
                await _cosmosService.SoftDeleteLearnerAsync(id);
                TempData["Success"] = "Student marked as inactive";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(List));
        }
    }
}