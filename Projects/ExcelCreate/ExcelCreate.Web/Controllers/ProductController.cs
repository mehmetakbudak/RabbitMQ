using ExcelCreate.Web.Models;
using ExcelCreate.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace ExcelCreate.Web.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _userManager = userManager;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid()}";

            UserFile userFile = new()
            {
                FileName = fileName,
                FilePath = "",
                FileStatus = FileStatus.Creating,
                UserId = user.Id
            };

            await _context.UserFiles.AddAsync(userFile);

            await _context.SaveChangesAsync();

            _rabbitMQPublisher.Publish(new CreateExcelMessage
            {
                FileId = userFile.Id
            });

            TempData["StartCreatingExcel"] = true;

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var userFiles = await _context.UserFiles
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.Id).ToListAsync();

            return View(userFiles);
        }
    }
}
