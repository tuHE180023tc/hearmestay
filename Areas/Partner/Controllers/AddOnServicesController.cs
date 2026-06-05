using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models;
using HearMeStay.Services.Interfaces;

namespace HearMeStay.Areas.Partner.Controllers
{
    [Area("Partner")][Authorize(Roles = "HotelPartner")]
    public class AddOnServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public AddOnServicesController(ApplicationDbContext ctx, UserManager<ApplicationUser> um) { _context = ctx; _userManager = um; }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var services = await _context.AddOnServices.Where(s => s.Accommodation.OwnerId == userId).Include(s => s.Accommodation).ToListAsync();
            return View(services);
        }

        [HttpGet] public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var acc = await _context.Accommodations.FirstOrDefaultAsync(a => a.OwnerId == userId);
            if (acc == null) return RedirectToAction("MyAccommodation", "Accommodations");
            ViewBag.AccommodationId = acc.Id;
            return View(new AddOnService { AccommodationId = acc.Id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddOnService model)
        {
            var userId = _userManager.GetUserId(User);
            if (!await _context.Accommodations.AnyAsync(a => a.Id == model.AccommodationId && a.OwnerId == userId)) return NotFound();
            _context.AddOnServices.Add(model); await _context.SaveChangesAsync();
            TempData["Success"] = "Đã thêm dịch vụ."; return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var svc = await _context.AddOnServices.Include(s => s.Accommodation).FirstOrDefaultAsync(s => s.Id == id && s.Accommodation.OwnerId == userId);
            if (svc == null) return NotFound();
            return View(svc);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddOnService model)
        {
            if (id != model.Id) return NotFound();
            var userId = _userManager.GetUserId(User);
            if (!await _context.Accommodations.AnyAsync(a => a.Id == model.AccommodationId && a.OwnerId == userId)) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật dịch vụ.";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var svc = await _context.AddOnServices.Include(s => s.Accommodation).FirstOrDefaultAsync(s => s.Id == id && s.Accommodation.OwnerId == userId);
            if (svc == null) return NotFound();
            _context.AddOnServices.Remove(svc); await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa dịch vụ."; return RedirectToAction("Index");
        }
    }
}
