namespace backendShoppingcart.Common;

public static class ThaiTimeHelper
{
    private static readonly TimeZoneInfo ThaiTimeZone = 
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok"); // Windows
        // หรือถ้าใช้ Linux / Docker container ให้ใช้ "Asia/Bangkok" แทน

    public static DateTime GetThaiTime()
    {
        // ดึงเวลา UTC ปัจจุบัน แล้วแปลงเป็นเวลาไทย
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ThaiTimeZone);
    }

       // เพิ่ม Method นี้เพื่อให้ได้ string รูปแบบ 24 ชม. เป๊ะๆ

    public static string GetThaiTimeString()
    {
        return GetThaiTime().ToString("yyyy-MM-dd HH:mm:ss");
    }
}