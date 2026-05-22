using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Service.Common;

/// <summary>
/// قواعد التبرع الطبية والتجارية
/// </summary>
public static class DonationBusinessRules
{
    /// <summary>
    /// الحد الأدنى لعمر المتبرع (بالسنوات)
    /// </summary>
    public const int MinimumAge = 18;

    /// <summary>
    /// الحد الأقصى لعمر المتبرع (بالسنوات)
    /// </summary>
    public const int MaximumAge = 65;

    /// <summary>
    /// الفترة بين التبرعات للرجال (بالأيام) - 3 أشهر
    /// </summary>
    public const int MaleDonationIntervalDays = 90;

    /// <summary>
    /// الفترة بين التبرعات للنساء (بالأيام) - 4 أشهر
    /// </summary>
    public const int FemaleDonationIntervalDays = 120;

    /// <summary>
    /// مدة صلاحية الدم (بالأيام) - 42 يوم
    /// </summary>
    public const int BloodExpiryDays = 42;

    /// <summary>
    /// التحقق من أن عمر المتبرع مناسب (18-65 سنة)
    /// </summary>
    /// <param name="dateOfBirth">تاريخ ميلاد المتبرع</param>
    /// <returns>نتيجة التحقق ورسالة الخطأ إن وجدت</returns>
    public static (bool IsValid, string ErrorMessage) ValidateDonorAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Year;

        // تعديل العمر إذا لم يحل عيد الميلاد هذا العام بعد
        if (today < dateOfBirth.AddYears(age))
            age--;

        if (age < MinimumAge)
            return (false, $"العمر غير مناسب للتبرع. الحد الأدنى {MinimumAge} سنة (العمر الحالي: {age} سنة)");

        if (age > MaximumAge)
            return (false, $"العمر غير مناسب للتبرع. الحد الأقصى {MaximumAge} سنة (العمر الحالي: {age} سنة)");

        return (true, string.Empty);
    }

    /// <summary>
    /// التحقق من الفترة الزمنية بين التبرعات (مع مراعاة الجنس)
    /// </summary>
    /// <param name="lastDonationDate">تاريخ آخر تبرع</param>
    /// <param name="gender">جنس المتبرع</param>
    /// <returns>نتيجة التحقق ورسالة الخطأ إن وجدت</returns>
    public static (bool IsValid, string ErrorMessage) ValidateDonationInterval(
        DateTime? lastDonationDate,
        Gender gender)
    {
        // إذا لم يتبرع من قبل، فهو مؤهل
        if (lastDonationDate == null)
            return (true, string.Empty);

        // تحديد الفترة المطلوبة حسب الجنس
        var requiredIntervalDays = gender == Gender.Male
            ? MaleDonationIntervalDays
            : FemaleDonationIntervalDays;

        var requiredIntervalMonths = requiredIntervalDays / 30;

        // حساب عدد الأيام منذ آخر تبرع
        var daysSinceLastDonation = (DateTime.UtcNow - lastDonationDate.Value).Days;

        if (daysSinceLastDonation < requiredIntervalDays)
        {
            var remainingDays = requiredIntervalDays - daysSinceLastDonation;
            var genderText = gender == Gender.Male ? "للرجال" : "للنساء";

            return (false,
                $"يجب الانتظار {requiredIntervalDays} يوم (حوالي {requiredIntervalMonths} أشهر) {genderText} بين التبرعات. " +
                $"آخر تبرع كان قبل {daysSinceLastDonation} يوم. " +
                $"المدة المتبقية: {remainingDays} يوم");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// حساب تاريخ انتهاء صلاحية الدم
    /// </summary>
    /// <param name="donationDate">تاريخ التبرع</param>
    /// <returns>تاريخ انتهاء الصلاحية</returns>
    public static DateTime CalculateBloodExpiryDate(DateTime donationDate)
    {
        return donationDate.AddDays(BloodExpiryDays);
    }

    /// <summary>
    /// التحقق من أن الكمية المتبرع بها صحيحة
    /// </summary>
    /// <param name="quantity">الكمية بالوحدات</param>
    /// <returns>نتيجة التحقق ورسالة الخطأ إن وجدت</returns>
    public static (bool IsValid, string ErrorMessage) ValidateDonationQuantity(int quantity)
    {
        if (quantity <= 0)
            return (false, "الكمية يجب أن تكون أكبر من صفر");

        if (quantity > 5)
            return (false, "الكمية المتبرع بها كبيرة جداً. الحد الأقصى 5 وحدات");

        return (true, string.Empty);
    }
}
