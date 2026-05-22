using AutoMapper;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs.Reports.Common;
using BloodConnectAPI.Models.DTOs.Reports.Dashboard;
using BloodConnectAPI.Models.DTOs.Reports.Donations;
using BloodConnectAPI.Models.DTOs.Reports.Donors;
using BloodConnectAPI.Models.DTOs.Reports.Inventory;
using BloodConnectAPI.Models.DTOs.Reports.Patients;
using BloodConnectAPI.Models.DTOs.Reports.Requests;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace BloodConnectAPI.Service.Implementations;

public class ReportService : IReportService
{
    private static readonly string[] BloodTypeOrder = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region Donor Reports

    public async Task<ServiceResponse<DonorStatisticsDto>> GetDonorStatisticsByBloodTypeAsync()
    {
        try
        {
            _logger.LogInformation("Starting donor statistics by blood type report generation");

            var donors = (await _unitOfWork.Donors.GetAllAsync()).ToList();
            var bloodTypes = await GetOrderedBloodTypesAsync();
            var donorCounts = donors
                .GroupBy(d => d.BloodTypeID)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new DonorStatisticsDto
            {
                Statistics = bloodTypes
                    .Select(bloodType => new BloodTypeStatistic
                    {
                        BloodType = bloodType.TypeName,
                        Count = donorCounts.TryGetValue(bloodType.BloodTypeID, out var count) ? count : 0
                    })
                    .ToList()
            };

            return ServiceResponse<DonorStatisticsDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donor statistics by blood type report");
            return ServiceResponse<DonorStatisticsDto>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<ActiveVsInactiveDonorsDto>> GetActiveVsInactiveDonorsAsync()
    {
        try
        {
            var donors = (await _unitOfWork.Donors.GetAllAsync()).ToList();
            var activeCount = donors.Count(d => d.IsActive);
            var inactiveCount = donors.Count(d => !d.IsActive);
            var totalCount = donors.Count;

            var result = new ActiveVsInactiveDonorsDto
            {
                ActiveCount = activeCount,
                InactiveCount = inactiveCount,
                TotalCount = totalCount,
                ActivePercentage = totalCount > 0 ? (double)activeCount / totalCount * 100 : 0,
                InactivePercentage = totalCount > 0 ? (double)inactiveCount / totalCount * 100 : 0
            };

            return ServiceResponse<ActiveVsInactiveDonorsDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating active vs inactive donors report");
            return ServiceResponse<ActiveVsInactiveDonorsDto>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<PagedResult<DonorsByCityDto>>> GetDonorsByCityAsync(BloodConnectAPI.Models.DTOs.Reports.Common.PaginationParams pagination)
    {
        try
        {
            var donors = (await _unitOfWork.Donors.GetAllAsync()).ToList();
            var cityGroups = donors
                .GroupBy(d => d.City)
                .Select(g => new DonorsByCityDto
                {
                    City = g.Key,
                    DonorCount = g.Count()
                })
                .OrderByDescending(c => c.DonorCount)
                .ThenBy(c => c.City)
                .ToList();

            var totalCount = cityGroups.Count;
            var skip = (pagination.PageNumber - 1) * pagination.PageSize;

            var result = new PagedResult<DonorsByCityDto>
            {
                Items = cityGroups.Skip(skip).Take(pagination.PageSize).ToList(),
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return ServiceResponse<PagedResult<DonorsByCityDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donors by city report");
            return ServiceResponse<PagedResult<DonorsByCityDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<EligibleDonorsByBloodTypeDto>>> GetEligibleDonorsByBloodTypeAsync()
    {
        try
        {
            var eligibilityDate = DateTime.UtcNow.AddDays(-56);
            var donors = (await _unitOfWork.Donors.GetAllAsync()).ToList();
            var bloodTypes = await GetOrderedBloodTypesAsync();
            var eligibleCounts = donors
                .Where(d => d.IsActive && (!d.LastDonationDate.HasValue || d.LastDonationDate.Value <= eligibilityDate))
                .GroupBy(d => d.BloodTypeID)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = bloodTypes
                .Select(bloodType => new EligibleDonorsByBloodTypeDto
                {
                    BloodType = bloodType.TypeName,
                    EligibleCount = eligibleCounts.TryGetValue(bloodType.BloodTypeID, out var count) ? count : 0
                })
                .ToList();

            return ServiceResponse<List<EligibleDonorsByBloodTypeDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating eligible donors by blood type report");
            return ServiceResponse<List<EligibleDonorsByBloodTypeDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    #endregion

    #region Donation Reports

    public async Task<ServiceResponse<List<DonationsByPeriodDto>>> GetDonationsByTimePeriodAsync(DateFilterDto? filter, string groupBy)
    {
        try
        {
            var validGroupByValues = new[] { "day", "week", "month", "year" };
            if (!validGroupByValues.Contains(groupBy.ToLowerInvariant()))
            {
                return ServiceResponse<List<DonationsByPeriodDto>>.FailureResponse(
                    "ظ‚ظٹظ…ط© groupBy ط؛ظٹط± طµط­ظٹط­ط©. ط§ظ„ظ‚ظٹظ… ط§ظ„ظ…ط³ظ…ظˆط­ط©: day, week, month, year");
            }

            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<DonationsByPeriodDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter, defaultToCurrentMonth: true);
            var donations = (await _unitOfWork.Donations.GetByDateRangeAsync(startDate, endDate)).ToList();

            var groupedDonations = groupBy.ToLowerInvariant() switch
            {
                "day" => GroupByDay(donations),
                "week" => GroupByWeek(donations),
                "month" => GroupByMonth(donations),
                "year" => GroupByYear(donations),
                _ => new List<DonationsByPeriodDto>()
            };

            return ServiceResponse<List<DonationsByPeriodDto>>.SuccessResponse(groupedDonations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donations by time period report");
            return ServiceResponse<List<DonationsByPeriodDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    private List<DonationsByPeriodDto> GroupByDay(List<Donation> donations)
    {
        return donations
            .GroupBy(d => d.DonationDate.Date)
            .Select(g => new DonationsByPeriodDto
            {
                Period = g.Key.ToString("yyyy-MM-dd"),
                DonationCount = g.Count(),
                PeriodStart = g.Key,
                PeriodEnd = g.Key.AddDays(1).AddTicks(-1)
            })
            .OrderBy(x => x.PeriodStart)
            .ToList();
    }

    private List<DonationsByPeriodDto> GroupByWeek(List<Donation> donations)
    {
        return donations
            .GroupBy(d => new
            {
                Year = d.DonationDate.Year,
                Week = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    d.DonationDate,
                    System.Globalization.CalendarWeekRule.FirstDay,
                    DayOfWeek.Sunday)
            })
            .Select(g =>
            {
                var firstDayOfWeek = GetFirstDayOfWeek(g.Key.Year, g.Key.Week);
                return new DonationsByPeriodDto
                {
                    Period = $"{g.Key.Year}-W{g.Key.Week:D2}",
                    DonationCount = g.Count(),
                    PeriodStart = firstDayOfWeek,
                    PeriodEnd = firstDayOfWeek.AddDays(7).AddTicks(-1)
                };
            })
            .OrderBy(x => x.PeriodStart)
            .ToList();
    }

    private List<DonationsByPeriodDto> GroupByMonth(List<Donation> donations)
    {
        return donations
            .GroupBy(d => new { d.DonationDate.Year, d.DonationDate.Month })
            .Select(g => new DonationsByPeriodDto
            {
                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                DonationCount = g.Count(),
                PeriodStart = new DateTime(g.Key.Year, g.Key.Month, 1),
                PeriodEnd = new DateTime(g.Key.Year, g.Key.Month, 1).AddMonths(1).AddTicks(-1)
            })
            .OrderBy(x => x.PeriodStart)
            .ToList();
    }

    private List<DonationsByPeriodDto> GroupByYear(List<Donation> donations)
    {
        return donations
            .GroupBy(d => d.DonationDate.Year)
            .Select(g => new DonationsByPeriodDto
            {
                Period = g.Key.ToString(),
                DonationCount = g.Count(),
                PeriodStart = new DateTime(g.Key, 1, 1),
                PeriodEnd = new DateTime(g.Key, 12, 31, 23, 59, 59, 999)
            })
            .OrderBy(x => x.PeriodStart)
            .ToList();
    }

    private DateTime GetFirstDayOfWeek(int year, int weekNumber)
    {
        var jan1 = new DateTime(year, 1, 1);
        var daysOffset = DayOfWeek.Sunday - jan1.DayOfWeek;
        var firstSunday = jan1.AddDays(daysOffset);
        var firstWeek = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
            jan1,
            System.Globalization.CalendarWeekRule.FirstDay,
            DayOfWeek.Sunday);

        var weekNum = weekNumber;
        if (firstWeek == 1)
        {
            weekNum -= 1;
        }

        return firstSunday.AddDays(weekNum * 7);
    }

    public async Task<ServiceResponse<List<BloodQuantityDto>>> GetBloodQuantityDonatedAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<BloodQuantityDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var donations = (await _unitOfWork.Donations.GetByDateRangeAsync(startDate, endDate)).ToList();
            var bloodTypes = await GetOrderedBloodTypesAsync();
            var quantitiesByBloodTypeId = donations
                .GroupBy(d => d.BloodTypeID)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Quantity));

            var result = bloodTypes
                .Select(bloodType => new BloodQuantityDto
                {
                    BloodType = bloodType.TypeName,
                    TotalQuantityML = quantitiesByBloodTypeId.TryGetValue(bloodType.BloodTypeID, out var totalQuantity)
                        ? totalQuantity
                        : 0
                })
                .OrderByDescending(q => q.TotalQuantityML)
                .ThenBy(q => GetBloodTypeOrder(q.BloodType))
                .ToList();

            result.Add(new BloodQuantityDto
            {
                BloodType = "Total",
                TotalQuantityML = result.Sum(q => q.TotalQuantityML)
            });

            return ServiceResponse<List<BloodQuantityDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating blood quantity donated report");
            return ServiceResponse<List<BloodQuantityDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<DonationTestResultsDto>> GetDonationTestResultsAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<DonationTestResultsDto>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var donations = (await _unitOfWork.Donations.GetByDateRangeAsync(startDate, endDate)).ToList();
            var totalCount = donations.Count;

            var result = new DonationTestResultsDto
            {
                PendingCount = donations.Count(d => d.TestResult == TestResult.Pending),
                AcceptedCount = donations.Count(d => d.TestResult == TestResult.Accepted),
                RejectedCount = donations.Count(d => d.TestResult == TestResult.Rejected),
                TotalCount = totalCount
            };

            result.AcceptanceRate = totalCount > 0 ? (double)result.AcceptedCount / totalCount * 100 : 0;
            result.RejectionRate = totalCount > 0 ? (double)result.RejectedCount / totalCount * 100 : 0;

            return ServiceResponse<DonationTestResultsDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donation test results report");
            return ServiceResponse<DonationTestResultsDto>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<DonationsByBloodTypeDto>>> GetDonationsByBloodTypeAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<DonationsByBloodTypeDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var donations = (await _unitOfWork.Donations.GetByDateRangeAsync(startDate, endDate)).ToList();
            var bloodTypes = await GetOrderedBloodTypesAsync();
            var totalCount = donations.Count;
            var donationCounts = donations
                .GroupBy(d => d.BloodTypeID)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = bloodTypes
                .Select(bloodType =>
                {
                    donationCounts.TryGetValue(bloodType.BloodTypeID, out var donationCount);
                    return new DonationsByBloodTypeDto
                    {
                        BloodType = bloodType.TypeName,
                        DonationCount = donationCount,
                        Percentage = totalCount > 0 ? (double)donationCount / totalCount * 100 : 0
                    };
                })
                .OrderByDescending(d => d.DonationCount)
                .ThenBy(d => GetBloodTypeOrder(d.BloodType))
                .ToList();

            return ServiceResponse<List<DonationsByBloodTypeDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donations by blood type report");
            return ServiceResponse<List<DonationsByBloodTypeDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<MostActiveDonorDto>>> GetMostActiveDonorsAsync(DateFilterDto? filter, int limit)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<MostActiveDonorDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            limit = limit <= 0 ? 10 : limit;

            var (startDate, endDate) = ResolveDateRange(filter);
            var donations = (await _unitOfWork.Donations.GetByDateRangeAsync(startDate, endDate)).ToList();
            var donorsById = (await _unitOfWork.Donors.GetAllAsync()).ToDictionary(d => d.DonorID);
            var bloodTypeNames = await GetBloodTypeNameMapAsync();

            var result = donations
                .GroupBy(d => d.DonorID)
                .Select(g => new
                {
                    DonorID = g.Key,
                    DonationCount = g.Count()
                })
                .Where(x => donorsById.ContainsKey(x.DonorID))
                .OrderByDescending(x => x.DonationCount)
                .ThenBy(x => donorsById[x.DonorID].FullName)
                .Take(limit)
                .Select(x =>
                {
                    var donor = donorsById[x.DonorID];
                    return new MostActiveDonorDto
                    {
                        DonorID = donor.DonorID,
                        DonorName = donor.FullName,
                        BloodType = bloodTypeNames.TryGetValue(donor.BloodTypeID, out var bloodTypeName)
                            ? bloodTypeName
                            : string.Empty,
                        DonationCount = x.DonationCount
                    };
                })
                .ToList();

            return ServiceResponse<List<MostActiveDonorDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating most active donors report");
            return ServiceResponse<List<MostActiveDonorDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    #endregion

    #region Inventory Reports

    public async Task<ServiceResponse<List<InventoryAvailabilityDto>>> GetInventoryAvailabilityAsync()
    {
        try
        {
            var inventories = (await _unitOfWork.BloodInventories.GetAllWithBloodTypesAsync()).ToList();
            var inventoryByBloodTypeId = inventories.ToDictionary(i => i.BloodTypeID);
            var bloodTypes = await GetOrderedBloodTypesAsync();

            var result = bloodTypes
                .Select(bloodType =>
                {
                    inventoryByBloodTypeId.TryGetValue(bloodType.BloodTypeID, out var inventory);
                    var quantityAvailable = inventory?.QuantityAvailable ?? 0;
                    var quantityReserved = inventory?.QuantityReserved ?? 0;

                    return new InventoryAvailabilityDto
                    {
                        BloodType = bloodType.TypeName,
                        QuantityAvailable = quantityAvailable,
                        QuantityReserved = quantityReserved,
                        TotalAvailable = quantityAvailable - quantityReserved
                    };
                })
                .ToList();

            return ServiceResponse<List<InventoryAvailabilityDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory availability report");
            return ServiceResponse<List<InventoryAvailabilityDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<ExpiringBloodUnitsDto>>> GetExpiringBloodUnitsAsync()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var expiringItems = (await _unitOfWork.BloodInventoryItems.GetExpiringItemsAsync()).ToList();

            var result = expiringItems
                .GroupBy(item => new
                {
                    BloodType = item.Inventory.BloodType.TypeName,
                    ExpiryDate = item.ExpiryDate.Date
                })
                .Select(group => new ExpiringBloodUnitsDto
                {
                    BloodType = group.Key.BloodType,
                    UnitCount = group.Count(),
                    DaysUntilExpiry = Math.Max(0, (group.Key.ExpiryDate - today).Days),
                    ExpiryDate = group.Key.ExpiryDate
                })
                .OrderBy(item => item.ExpiryDate)
                .ThenBy(item => GetBloodTypeOrder(item.BloodType))
                .ToList();

            return ServiceResponse<List<ExpiringBloodUnitsDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expiring blood units report");
            return ServiceResponse<List<ExpiringBloodUnitsDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<ExpiredBloodUnitsDto>>> GetExpiredBloodUnitsAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<ExpiredBloodUnitsDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var expiredItems = (await _unitOfWork.BloodInventoryItems.GetExpiredItemsAsync())
                .Where(item => item.ExpiryDate >= startDate && item.ExpiryDate <= endDate)
                .ToList();

            var result = expiredItems
                .GroupBy(item => item.Inventory.BloodType.TypeName)
                .Select(group => new ExpiredBloodUnitsDto
                {
                    BloodType = group.Key,
                    ExpiredUnitCount = group.Count(),
                    TotalQuantityWasted = group.Sum(item => item.Quantity)
                })
                .OrderByDescending(item => item.ExpiredUnitCount)
                .ThenBy(item => GetBloodTypeOrder(item.BloodType))
                .ToList();

            if (result.Count > 0)
            {
                result.Add(new ExpiredBloodUnitsDto
                {
                    BloodType = "Total",
                    ExpiredUnitCount = result.Sum(item => item.ExpiredUnitCount),
                    TotalQuantityWasted = result.Sum(item => item.TotalQuantityWasted)
                });
            }

            return ServiceResponse<List<ExpiredBloodUnitsDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expired blood units report");
            return ServiceResponse<List<ExpiredBloodUnitsDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<ConsumptionRateDto>>> GetInventoryConsumptionRateAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<ConsumptionRateDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var totalDays = Math.Max(1, (endDate.Date - startDate.Date).Days + 1);
            var bloodTypes = await GetOrderedBloodTypesAsync();
            var requestsById = (await _unitOfWork.BloodRequests.GetAllAsync()).ToDictionary(r => r.RequestID);
            var inventoriesByBloodTypeId = (await _unitOfWork.BloodInventories.GetAllWithBloodTypesAsync()).ToDictionary(i => i.BloodTypeID);
            var disbursements = (await _unitOfWork.BloodDisbursements.GetAllAsync())
                .Where(d => d.DisbursementDate >= startDate && d.DisbursementDate <= endDate)
                .ToList();

            var totalConsumedByBloodTypeId = disbursements
                .Where(d => requestsById.ContainsKey(d.RequestID))
                .GroupBy(d => requestsById[d.RequestID].BloodTypeID)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.QuantityUsed));

            var result = bloodTypes
                .Select(bloodType =>
                {
                    totalConsumedByBloodTypeId.TryGetValue(bloodType.BloodTypeID, out var totalConsumed);
                    inventoriesByBloodTypeId.TryGetValue(bloodType.BloodTypeID, out var inventory);
                    var currentInventory = (inventory?.QuantityAvailable ?? 0) - (inventory?.QuantityReserved ?? 0);
                    var averageDailyConsumption = totalConsumed / (double)totalDays;

                    return new ConsumptionRateDto
                    {
                        BloodType = bloodType.TypeName,
                        TotalConsumed = totalConsumed,
                        AverageDailyConsumption = averageDailyConsumption,
                        CurrentInventory = currentInventory,
                        ProjectedDaysUntilStockout = averageDailyConsumption > 0
                            ? Math.Max(0, (int)Math.Ceiling(currentInventory / averageDailyConsumption))
                            : 0
                    };
                })
                .ToList();

            return ServiceResponse<List<ConsumptionRateDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory consumption rate report");
            return ServiceResponse<List<ConsumptionRateDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<LowInventoryAlertDto>>> GetLowInventoryAlertsAsync(int threshold)
    {
        try
        {
            threshold = threshold <= 0 ? 10 : threshold;
            var criticalThreshold = Math.Max(1, threshold / 2);
            var lowStockInventories = (await _unitOfWork.BloodInventories.GetLowStockAsync(threshold)).ToList();

            var result = lowStockInventories
                .Select(inventory => new LowInventoryAlertDto
                {
                    BloodType = inventory.BloodType.TypeName,
                    CurrentQuantity = inventory.QuantityAvailable,
                    Threshold = threshold,
                    Severity = inventory.QuantityAvailable <= criticalThreshold ? "Critical" : "Warning"
                })
                .OrderBy(alert => alert.CurrentQuantity)
                .ThenBy(alert => GetBloodTypeOrder(alert.BloodType))
                .ToList();

            return ServiceResponse<List<LowInventoryAlertDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating low inventory alerts report");
            return ServiceResponse<List<LowInventoryAlertDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    #endregion

    #region Blood Request Reports

    public async Task<ServiceResponse<RequestsByStatusDto>> GetRequestsByStatusAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<RequestsByStatusDto>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var requests = (await _unitOfWork.BloodRequests.GetAllAsync())
                .Where(r => r.RequestDate >= startDate && r.RequestDate <= endDate)
                .ToList();

            var pendingCount = requests.Count(r => r.Status == RequestStatus.Pending);
            var fulfilledCount = requests.Count(r => IsFulfilledLike(r.Status));
            var cancelledCount = requests.Count(r => r.Status == RequestStatus.Cancelled);
            var totalCount = requests.Count;

            var result = new RequestsByStatusDto
            {
                PendingCount = pendingCount,
                FulfilledCount = fulfilledCount,
                CancelledCount = cancelledCount,
                TotalCount = totalCount,
                FulfillmentRate = totalCount > 0 ? (double)fulfilledCount / totalCount * 100 : 0,
                CancellationRate = totalCount > 0 ? (double)cancelledCount / totalCount * 100 : 0
            };

            return ServiceResponse<RequestsByStatusDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating requests by status report");
            return ServiceResponse<RequestsByStatusDto>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<RequestsByUrgencyDto>>> GetRequestsByUrgencyAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<RequestsByUrgencyDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var pendingRequests = (await _unitOfWork.BloodRequests.GetAllAsync())
                .Where(r => r.Status == RequestStatus.Pending && r.RequestDate >= startDate && r.RequestDate <= endDate)
                .ToList();
            var requestCountsByUrgency = pendingRequests
                .GroupBy(r => r.UrgencyLevel)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = Enum.GetValues<UrgencyLevel>()
                .OrderBy(GetUrgencySortOrder)
                .Select(urgency => new RequestsByUrgencyDto
                {
                    UrgencyLevel = urgency.ToString(),
                    RequestCount = requestCountsByUrgency.TryGetValue(urgency, out var count) ? count : 0
                })
                .ToList();

            return ServiceResponse<List<RequestsByUrgencyDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating requests by urgency report");
            return ServiceResponse<List<RequestsByUrgencyDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<FulfillmentRateDto>>> GetRequestFulfillmentRateAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<FulfillmentRateDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var requests = (await _unitOfWork.BloodRequests.GetAllAsync())
                .Where(r => r.RequestDate >= startDate && r.RequestDate <= endDate)
                .ToList();
            var bloodTypes = await GetOrderedBloodTypesAsync();
            var requestMetricsByBloodTypeId = requests
                .GroupBy(r => r.BloodTypeID)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        TotalRequests = g.Count(),
                        FulfilledRequests = g.Count(r => IsFulfilledLike(r.Status))
                    });

            var result = bloodTypes
                .Select(bloodType =>
                {
                    requestMetricsByBloodTypeId.TryGetValue(bloodType.BloodTypeID, out var metrics);
                    var totalRequests = metrics?.TotalRequests ?? 0;
                    var fulfilledRequests = metrics?.FulfilledRequests ?? 0;

                    return new FulfillmentRateDto
                    {
                        BloodType = bloodType.TypeName,
                        TotalRequests = totalRequests,
                        FulfilledRequests = fulfilledRequests,
                        FulfillmentRate = totalRequests > 0 ? (double)fulfilledRequests / totalRequests * 100 : 0
                    };
                })
                .ToList();

            return ServiceResponse<List<FulfillmentRateDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating request fulfillment rate report");
            return ServiceResponse<List<FulfillmentRateDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<RequestsByBloodTypeDto>>> GetRequestsByBloodTypeAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<RequestsByBloodTypeDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var requests = (await _unitOfWork.BloodRequests.GetAllAsync())
                .Where(r => r.RequestDate >= startDate && r.RequestDate <= endDate)
                .ToList();
            var totalCount = requests.Count;
            var bloodTypes = await GetOrderedBloodTypesAsync();
            var requestCountsByBloodTypeId = requests
                .GroupBy(r => r.BloodTypeID)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = bloodTypes
                .Select(bloodType =>
                {
                    requestCountsByBloodTypeId.TryGetValue(bloodType.BloodTypeID, out var requestCount);
                    return new RequestsByBloodTypeDto
                    {
                        BloodType = bloodType.TypeName,
                        RequestCount = requestCount,
                        Percentage = totalCount > 0 ? (double)requestCount / totalCount * 100 : 0
                    };
                })
                .OrderByDescending(item => item.RequestCount)
                .ThenBy(item => GetBloodTypeOrder(item.BloodType))
                .ToList();

            return ServiceResponse<List<RequestsByBloodTypeDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating requests by blood type report");
            return ServiceResponse<List<RequestsByBloodTypeDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<AvgFulfillmentTimeDto>>> GetAverageFulfillmentTimeAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<List<AvgFulfillmentTimeDto>>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var (startDate, endDate) = ResolveDateRange(filter);
            var requests = (await _unitOfWork.BloodRequests.GetAllAsync())
                .Where(r => r.Status != RequestStatus.Cancelled && IsFulfilledLike(r.Status) && r.RequestDate >= startDate && r.RequestDate <= endDate)
                .ToList();
            var latestDisbursementDateByRequestId = (await _unitOfWork.BloodDisbursements.GetAllAsync())
                .GroupBy(d => d.RequestID)
                .ToDictionary(g => g.Key, g => g.Max(d => d.DisbursementDate));

            var result = Enum.GetValues<UrgencyLevel>()
                .OrderBy(GetUrgencySortOrder)
                .Select(urgency =>
                {
                    var urgencyRequests = requests.Where(r => r.UrgencyLevel == urgency).ToList();
                    return new AvgFulfillmentTimeDto
                    {
                        UrgencyLevel = urgency.ToString(),
                        AverageHours = urgencyRequests.Count > 0
                            ? urgencyRequests.Average(request =>
                            {
                                var completionDate = GetRequestCompletionDate(request, latestDisbursementDateByRequestId);
                                return completionDate >= request.RequestDate
                                    ? (completionDate - request.RequestDate).TotalHours
                                    : 0;
                            })
                            : 0,
                        RequestCount = urgencyRequests.Count
                    };
                })
                .ToList();

            return ServiceResponse<List<AvgFulfillmentTimeDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating average fulfillment time report");
            return ServiceResponse<List<AvgFulfillmentTimeDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    #endregion

    #region Patient Reports

    public async Task<ServiceResponse<PatientCountDto>> GetRegisteredPatientsCountAsync(DateFilterDto? filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return ServiceResponse<PatientCountDto>.FailureResponse(
                    "طھط§ط±ظٹط® ط§ظ„ط¨ط¯ط§ظٹط© ظٹط¬ط¨ ط£ظ† ظٹظƒظˆظ† ظ‚ط¨ظ„ طھط§ط±ظٹط® ط§ظ„ظ†ظ‡ط§ظٹط©");
            }

            var allPatients = (await _unitOfWork.Patients.GetAllAsync()).ToList();
            var filteredPatients = allPatients;
            var hasDateFilter = filter != null && (filter.StartDate.HasValue || filter.EndDate.HasValue);

            if (hasDateFilter)
            {
                var (startDate, endDate) = ResolveDateRange(filter);
                filteredPatients = allPatients
                    .Where(patient => patient.CreatedAt >= startDate && patient.CreatedAt <= endDate)
                    .ToList();
            }

            var result = new PatientCountDto
            {
                TotalCount = filteredPatients.Count
            };

            if (hasDateFilter)
            {
                var (startDate, endDate) = ResolveDateRange(filter);
                if (SpansMultipleMonths(startDate, endDate))
                {
                    result.Trends = filteredPatients
                        .GroupBy(patient => new { patient.CreatedAt.Year, patient.CreatedAt.Month })
                        .OrderBy(group => group.Key.Year)
                        .ThenBy(group => group.Key.Month)
                        .Select(group => new MonthlyTrend
                        {
                            Month = $"{group.Key.Year}-{group.Key.Month:D2}",
                            Count = group.Count()
                        })
                        .ToList();
                }
            }

            return ServiceResponse<PatientCountDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating registered patients count report");
            return ServiceResponse<PatientCountDto>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<PatientsByBloodTypeDto>>> GetPatientsByBloodTypeAsync()
    {
        try
        {
            var patients = (await _unitOfWork.Patients.GetAllAsync()).ToList();
            var bloodTypes = await GetOrderedBloodTypesAsync();
            var totalCount = patients.Count;
            var patientCountsByBloodTypeId = patients
                .GroupBy(patient => patient.BloodTypeID)
                .ToDictionary(group => group.Key, group => group.Count());

            var result = bloodTypes
                .Select(bloodType =>
                {
                    patientCountsByBloodTypeId.TryGetValue(bloodType.BloodTypeID, out var patientCount);
                    return new PatientsByBloodTypeDto
                    {
                        BloodType = bloodType.TypeName,
                        PatientCount = patientCount,
                        Percentage = totalCount > 0 ? (double)patientCount / totalCount * 100 : 0
                    };
                })
                .OrderByDescending(item => item.PatientCount)
                .ThenBy(item => GetBloodTypeOrder(item.BloodType))
                .ToList();

            return ServiceResponse<List<PatientsByBloodTypeDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating patients by blood type report");
            return ServiceResponse<List<PatientsByBloodTypeDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    public async Task<ServiceResponse<List<PatientsWithActiveRequestsDto>>> GetPatientsWithActiveRequestsAsync()
    {
        try
        {
            var patientsById = (await _unitOfWork.Patients.GetAllAsync()).ToDictionary(patient => patient.PatientID);
            var bloodTypeNames = await GetBloodTypeNameMapAsync();
            var pendingRequests = (await _unitOfWork.BloodRequests.GetPendingRequestsAsync()).ToList();

            var result = pendingRequests
                .GroupBy(request => request.PatientID)
                .Where(group => patientsById.ContainsKey(group.Key))
                .Select(group =>
                {
                    var patient = patientsById[group.Key];
                    var highestUrgency = group.Max(request => request.UrgencyLevel);

                    return new
                    {
                        Patient = patient,
                        HighestUrgency = highestUrgency,
                        ActiveRequestCount = group.Count()
                    };
                })
                .OrderBy(item => GetUrgencySortOrder(item.HighestUrgency))
                .ThenByDescending(item => item.ActiveRequestCount)
                .ThenBy(item => item.Patient.FullName)
                .Select(item => new PatientsWithActiveRequestsDto
                {
                    PatientID = item.Patient.PatientID,
                    PatientName = item.Patient.FullName,
                    BloodType = bloodTypeNames.TryGetValue(item.Patient.BloodTypeID, out var bloodTypeName)
                        ? bloodTypeName
                        : string.Empty,
                    ActiveRequestCount = item.ActiveRequestCount,
                    HighestUrgencyLevel = item.HighestUrgency.ToString()
                })
                .ToList();

            return ServiceResponse<List<PatientsWithActiveRequestsDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating patients with active requests report");
            return ServiceResponse<List<PatientsWithActiveRequestsDto>>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    #endregion

    #region Dashboard

    public async Task<ServiceResponse<DashboardSummaryDto>> GetDashboardSummaryAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayOfYear = new DateTime(now.Year, 1, 1);

            var donors = (await _unitOfWork.Donors.GetAllAsync()).ToList();
            var donations = (await _unitOfWork.Donations.GetAllAsync()).ToList();
            var requests = (await _unitOfWork.BloodRequests.GetAllAsync()).ToList();
            var bloodTypes = await GetOrderedBloodTypesAsync();
            var inventoriesByBloodTypeId = (await _unitOfWork.BloodInventories.GetAllWithBloodTypesAsync())
                .ToDictionary(inventory => inventory.BloodTypeID);
            var expiringItems = (await _unitOfWork.BloodInventoryItems.GetExpiringItemsAsync()).ToList();
            var latestDisbursementDateByRequestId = (await _unitOfWork.BloodDisbursements.GetAllAsync())
                .GroupBy(disbursement => disbursement.RequestID)
                .ToDictionary(group => group.Key, group => group.Max(disbursement => disbursement.DisbursementDate));

            var result = new DashboardSummaryDto
            {
                ActiveDonorsCount = donors.Count(donor => donor.IsActive),
                InactiveDonorsCount = donors.Count(donor => !donor.IsActive),
                DonationsThisMonth = donations.Count(donation => donation.DonationDate >= firstDayOfMonth && donation.DonationDate <= now),
                DonationsThisYear = donations.Count(donation => donation.DonationDate >= firstDayOfYear && donation.DonationDate <= now),
                PendingRequestsCount = requests.Count(request => request.Status == RequestStatus.Pending),
                FulfilledRequestsThisMonth = requests.Count(request =>
                {
                    if (!IsFulfilledLike(request.Status))
                    {
                        return false;
                    }

                    var completionDate = GetRequestCompletionDate(request, latestDisbursementDateByRequestId);
                    return completionDate >= firstDayOfMonth && completionDate <= now;
                }),
                EmergencyRequestsCount = requests.Count(request =>
                    request.Status == RequestStatus.Pending &&
                    request.UrgencyLevel == UrgencyLevel.Emergency),
                InventoryByBloodType = bloodTypes
                    .Select(bloodType =>
                    {
                        inventoriesByBloodTypeId.TryGetValue(bloodType.BloodTypeID, out var inventory);
                        return new InventoryStatus
                        {
                            BloodType = bloodType.TypeName,
                            QuantityAvailable = inventory?.QuantityAvailable ?? 0
                        };
                    })
                    .ToList(),
                ExpiringUnitsCount = expiringItems.Count
            };

            return ServiceResponse<DashboardSummaryDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard summary report");
            return ServiceResponse<DashboardSummaryDto>.FailureResponse("ط­ط¯ط« ط®ط·ط£ ط£ط«ظ†ط§ط، طھظˆظ„ظٹط¯ ط§ظ„طھظ‚ط±ظٹط±");
        }
    }

    #endregion

    private static (DateTime StartDate, DateTime EndDate) ResolveDateRange(DateFilterDto? filter, bool defaultToCurrentMonth = false)
    {
        var now = DateTime.UtcNow;

        if (filter == null || (!filter.StartDate.HasValue && !filter.EndDate.HasValue))
        {
            return defaultToCurrentMonth
                ? (new DateTime(now.Year, now.Month, 1), now)
                : (DateTime.MinValue, now);
        }

        return (filter.StartDate ?? DateTime.MinValue, filter.EndDate ?? now);
    }

    private async Task<List<BloodType>> GetOrderedBloodTypesAsync()
    {
        var bloodTypes = (await _unitOfWork.BloodTypes.GetAllAsync()).ToList();
        return bloodTypes.OrderBy(bloodType => GetBloodTypeOrder(bloodType.TypeName)).ToList();
    }

    private async Task<Dictionary<int, string>> GetBloodTypeNameMapAsync()
    {
        return (await GetOrderedBloodTypesAsync()).ToDictionary(bloodType => bloodType.BloodTypeID, bloodType => bloodType.TypeName);
    }

    private static int GetBloodTypeOrder(string bloodType)
    {
        var index = Array.IndexOf(BloodTypeOrder, bloodType);
        return index >= 0 ? index : int.MaxValue;
    }

    private static bool IsFulfilledLike(RequestStatus status)
    {
        return status == RequestStatus.Fulfilled || status == RequestStatus.PartiallyFulfilled;
    }

    private static int GetUrgencySortOrder(UrgencyLevel urgencyLevel)
    {
        return urgencyLevel switch
        {
            UrgencyLevel.Emergency => 0,
            UrgencyLevel.Urgent => 1,
            UrgencyLevel.Normal => 2,
            _ => int.MaxValue
        };
    }

    private static DateTime GetRequestCompletionDate(BloodRequest request, IReadOnlyDictionary<int, DateTime> latestDisbursementDateByRequestId)
    {
        if (latestDisbursementDateByRequestId.TryGetValue(request.RequestID, out var latestDisbursementDate))
        {
            return latestDisbursementDate;
        }

        return request.UpdatedAt ?? request.RequestDate;
    }

    private static bool SpansMultipleMonths(DateTime startDate, DateTime endDate)
    {
        return startDate.Year != endDate.Year || startDate.Month != endDate.Month;
    }
}
