using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models.Enums;

namespace HearMeStay.Areas.Admin.Controllers
{
    [Area("Admin")][Authorize(Roles = "Admin")]
    public class AccommodationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccommodationsController(ApplicationDbContext ctx) { _context = ctx; }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Accommodations.Include(a => a.Owner).OrderByDescending(a => a.CreatedAt).ToListAsync();
            return View(list);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id) { var a = await _context.Accommodations.FindAsync(id); if (a != null) { a.Status = AccommodationStatus.Approved; await _context.SaveChangesAsync(); } TempData["Success"] = "Đã duyệt."; return RedirectToAction("Index"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id) { var a = await _context.Accommodations.FindAsync(id); if (a != null) { a.Status = AccommodationStatus.Rejected; await _context.SaveChangesAsync(); } TempData["Success"] = "Đã từ chối."; return RedirectToAction("Index"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id) { var a = await _context.Accommodations.FindAsync(id); if (a != null) { a.Status = AccommodationStatus.Hidden; await _context.SaveChangesAsync(); } TempData["Success"] = "Đã ẩn."; return RedirectToAction("Index"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Unhide(int id) { var a = await _context.Accommodations.FindAsync(id); if (a != null) { a.Status = AccommodationStatus.Approved; await _context.SaveChangesAsync(); } TempData["Success"] = "Đã mở lại."; return RedirectToAction("Index"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) { 
            var a = await _context.Accommodations.FindAsync(id); 
            if (a != null) { 
                _context.Accommodations.Remove(a); 
                await _context.SaveChangesAsync(); 
            } 
            TempData["Success"] = "Đã xóa vĩnh viễn."; 
            return RedirectToAction("Index"); 
        }
    }
}
