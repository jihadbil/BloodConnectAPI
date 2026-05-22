using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BloodConnectAPI.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class DonationLabReportsController : ControllerBase
{
    private readonly IDonationLabReportService _reportService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DonationLabReportsController> _logger;

    public DonationLabReportsController(
        IDonationLabReportService reportService,
        IWebHostEnvironment env,
        ILogger<DonationLabReportsController> logger)
    {
        _reportService = reportService;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// رفع ملف تحليل لتبرع معين
    /// </summary>
    [HttpPost("api/donations/{donationId}/lab-reports")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ServiceResponse<DonationLabReportDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(int donationId, [FromForm] string reportType, [FromForm] string? notes, IFormFile file)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var dto = new UploadDonationLabReportDto
        {
            DonationID = donationId,
            ReportType = reportType,
            File = file,
            Notes = notes
        };

        var uploadedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        _logger.LogInformation("محاولة رفع ملف تحليل للتبرع: {DonationId} من المستخدم: {UserId}", donationId, uploadedBy);

        var response = await _reportService.UploadAsync(dto, rootPath, uploadedBy);

        if (!response.IsSuccess) return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { reportId = response.Data?.LabReportID }, response);
    }

    /// <summary>
    /// الحصول على جميع التقارير لتبرع معين
    /// </summary>
    [HttpGet("api/donations/{donationId}/lab-reports")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<DonationLabReportDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDonationId(int donationId)
    {
        var response = await _reportService.GetByDonationIdAsync(donationId);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على تقرير تحليل بـ ID
    /// </summary>
    [HttpGet("api/lab-reports/{reportId}")]
    [ProducesResponseType(typeof(ServiceResponse<DonationLabReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int reportId)
    {
        var response = await _reportService.GetByIdAsync(reportId);
        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// تحميل وعرض ملف التقرير الفعلي
    /// </summary>
    [HttpGet("api/lab-reports/{reportId}/file")]
    [Produces("application/octet-stream", "application/pdf", "image/jpeg", "image/png")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(int reportId)
    {
        var response = await _reportService.GetByIdAsync(reportId);
        if (!response.IsSuccess || response.Data == null)
            return NotFound(new { message = "تقرير التحليل غير موجود" });

        var filePath = response.Data.FilePath; // e.g. /uploads/lab-reports/1_CBC_xxx.pdf
        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        // بناء المسار الكامل على القرص
        var absolutePath = Path.Combine(rootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(absolutePath))
        {
            _logger.LogWarning("ملف التحليل غير موجود على القرص: {Path}", absolutePath);
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

    /// <summary>
    /// حذف تقرير تحليل مخبري
    /// </summary>
    [HttpDelete("api/lab-reports/{reportId}")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int reportId)
    {
        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var response = await _reportService.DeleteAsync(reportId, rootPath);

        if (!response.IsSuccess) return BadRequest(response);

        return Ok(response);
    }
}
