using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShacabWf.Web.Data;
using ShacabWf.Web.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ShacabWf.Web.Controllers
{
    public class SupportDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadsFolder;
        private readonly ILogger<SupportDocumentsController> _logger;

        public SupportDocumentsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<SupportDocumentsController> logger)
        {
            _context = context;
            _logger = logger;
            
            try
            {
                // Create uploads folder in wwwroot if it doesn't exist
                _uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads");
                _logger.LogInformation($"Uploads folder path: {_uploadsFolder}");
                
                if (!Directory.Exists(_uploadsFolder))
                {
                    _logger.LogInformation("Creating uploads directory as it doesn't exist");
                    Directory.CreateDirectory(_uploadsFolder);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting up uploads folder: {ex.Message}");
                // Fallback to temp folder if we can't create the uploads folder
                _uploadsFolder = Path.GetTempPath();
                _logger.LogInformation($"Using fallback temp directory: {_uploadsFolder}");
            }
        }

        // GET: SupportDocuments
        public async Task<IActionResult> Index()
        {
            // Set current user for the view if authenticated
            if (User.Identity.IsAuthenticated)
            {
                // Find user by username or other identifier
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                
                ViewBag.CurrentUser = currentUser;
            }
            
            var documents = await _context.SupportDocuments
                .Include(d => d.UploadedBy)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
            
            return View(documents);
        }

        // GET: SupportDocuments/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Find user by username
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            
            // Set current user for the view
            ViewBag.CurrentUser = currentUser;
            
            // Check if user has Admin role
            if (currentUser == null || !currentUser.HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        // POST: SupportDocuments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(IFormFile file, string description)
        {
            _logger.LogInformation("Create POST action called");
            
            try
            {
                // Find user by username
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                
                // Set current user for the view
                ViewBag.CurrentUser = currentUser;
                
                // Check if user has Admin role
                if (currentUser == null || !currentUser.HasRole("Admin"))
                {
                    _logger.LogWarning($"Unauthorized access attempt by user {User.Identity.Name}");
                    return RedirectToAction("Index", "Home");
                }

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file was uploaded");
                    ModelState.AddModelError("File", "Please select a file to upload");
                    return View();
                }

                _logger.LogInformation($"File received: {file.FileName}, Size: {file.Length} bytes");

                // Check file type (only allow PDF and Word documents)
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension != ".pdf" && extension != ".doc" && extension != ".docx")
                {
                    _logger.LogWarning($"Invalid file type: {extension}");
                    ModelState.AddModelError("File", "Only PDF and Word documents are allowed");
                    return View();
                }

                // Ensure uploads directory exists
                if (!Directory.Exists(_uploadsFolder))
                {
                    _logger.LogInformation($"Creating uploads directory: {_uploadsFolder}");
                    Directory.CreateDirectory(_uploadsFolder);
                }

                // Generate unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(_uploadsFolder, uniqueFileName);
                _logger.LogInformation($"Saving file to: {filePath}");

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create database record
                var supportDocument = new SupportDocument
                {
                    FileName = file.FileName,
                    FilePath = uniqueFileName,
                    Description = description ?? string.Empty, // Ensure description is not null
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadedAt = DateTime.Now,
                    UploadedById = currentUser.Id
                };

                _context.Add(supportDocument);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"File successfully uploaded: {file.FileName}");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                return View();
            }
        }

        // GET: SupportDocuments/Download/5
        public async Task<IActionResult> Download(int id)
        {
            _logger.LogInformation($"Download requested for document ID: {id}");
            
            try
            {
                var document = await _context.SupportDocuments.FindAsync(id);
                if (document == null)
                {
                    _logger.LogWarning($"Document with ID {id} not found");
                    return NotFound();
                }

                _logger.LogInformation($"Found document: {document.FileName}, Path: {document.FilePath}");
                
                var filePath = Path.Combine(_uploadsFolder, document.FilePath);
                _logger.LogInformation($"Full file path: {filePath}");
                
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"File not found at path: {filePath}");
                    
                    // Check if the file exists in the temp directory as a fallback
                    var tempFilePath = Path.Combine(Path.GetTempPath(), document.FilePath);
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        _logger.LogInformation($"File found in temp directory: {tempFilePath}");
                        filePath = tempFilePath;
                    }
                    else
                    {
                        return NotFound("The requested file could not be found on the server.");
                    }
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                _logger.LogInformation($"Returning file: {document.FileName}, Type: {document.ContentType}, Size: {memory.Length} bytes");
                return File(memory, document.ContentType, document.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading file with ID {id}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred while downloading the file: {ex.Message}");
            }
        }

        // GET: SupportDocuments/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            // Find user by username
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            
            // Set current user for the view
            ViewBag.CurrentUser = currentUser;
            
            // Check if user has Admin role
            if (currentUser == null || !currentUser.HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var document = await _context.SupportDocuments
                .Include(d => d.UploadedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: SupportDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find user by username
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            
            // Check if user has Admin role
            if (currentUser == null || !currentUser.HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var document = await _context.SupportDocuments.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Delete file from disk
            var filePath = Path.Combine(_uploadsFolder, document.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Delete database record
            _context.SupportDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
} 