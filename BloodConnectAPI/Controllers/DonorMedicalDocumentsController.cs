using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodConnectAPI.Controllers;

[ApiController]
[Route("api/medical-documents")]
[Authorize]
[Produces("application/json")]
public class DonorMedicalDocumentsController : ControllerBase
{
    private readonly IDonorMedicalDocumentService _documentService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DonorMedicalDocumentsController> _logger;

    public DonorMedicalDocumentsController(
        IDonorMedicalDocumentService documentService,
        IWebHostEnvironment env,
        ILogger<DonorMedicalDocumentsController> logger)
    {
        _documentService = documentService;
        _env = env;
        _logger = logger;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ServiceResponse<DonorMedicalDocumentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument([FromForm] int donorID, [FromForm] string documentType, IFormFile file)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var dto = new SubmitMedicalDocumentDto
        {
            DonorID = donorID,
            DocumentType = documentType,
            File = file
        };

        var uploadedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var response = await _documentService.UploadDocumentAsync(dto, rootPath, uploadedBy);

        if (!response.IsSuccess) return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.DocumentID }, response);
    }

    [HttpGet("donor/{donorId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDonorId(int donorId)
    {
        var response = await _documentService.GetByDonorIdAsync(donorId);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<DonorMedicalDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _documentService.GetByIdAsync(id);
        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// تحميل وعرض الملف الفعلي للوثيقة الطبية
    /// يُستخدم بدلاً من الروابط المباشرة لحل مشاكل CORS وترميز الأسماء العربية
    /// </summary>
    [HttpGet("{id}/file")]
    [Authorize(Roles = "Admin")]
    [Produces("application/octet-stream", "application/pdf", "image/jpeg", "image/png")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(int id)
    {
        var response = await _documentService.GetByIdAsync(id);
        if (!response.IsSuccess || response.Data == null)
            return NotFound(new { message = "الوثيقة غير موجودة" });

        var filePath = response.Data.FilePath; // e.g. /uploads/medical-documents/1_فحص دم_xxx.pdf

        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        // بناء المسار الكامل على القرص
        var absolutePath = Path.Combine(rootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(absolutePath))
        {
            _logger.LogWarning("الملف غير موجود على القرص: {Path}", absolutePath);
            return NotFound(new { message = "الملف غير موجود على القرص" });
        }

        // تحديد نوع المحتوى بناءً على الامتداد
        var extension = Path.GetExtension(absolutePath).ToLower();
        var contentType = extension switch
        {
            ".pdf"  => "application/pdf",
            ".jpg"  => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png"  => "image/png",
            _       => "application/octet-stream"
        };

        // اسم الملف للـ Content-Disposition (يدعم UTF-8)
        var fileName = Path.GetFileName(absolutePath);
        var encodedFileName = Uri.EscapeDataString(fileName);

        Response.Headers.Append("Content-Disposition", $"inline; filename*=UTF-8''{encodedFileName}");

        var stream = System.IO.File.OpenRead(absolutePath);
        return File(stream, contentType);
    }

    [HttpPatch("{id}/verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<DonorMedicalDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyDocument(int id, [FromBody] VerifyMedicalDocumentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var verifiedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _documentService.VerifyDocumentAsync(id, dto, verifiedBy);

        if (!response.IsSuccess) return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var response = await _documentService.DeleteAsync(id, rootPath);

        if (!response.IsSuccess) return BadRequest(response);

        return Ok(response);
    }
}
