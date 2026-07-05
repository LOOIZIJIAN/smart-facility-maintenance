namespace SmartFacilityMaintenance.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Maintenance = "Maintenance";
    public const string Student = "Student";

    public static readonly string[] All = { Admin, Staff, Maintenance, Student };
}
