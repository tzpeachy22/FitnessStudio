namespace api.Models;

public sealed class Business
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<CustomerPackage> Packages { get; set; } = [];
    public List<TimetableSchedule> TimetableSchedules { get; set; } = [];
}
