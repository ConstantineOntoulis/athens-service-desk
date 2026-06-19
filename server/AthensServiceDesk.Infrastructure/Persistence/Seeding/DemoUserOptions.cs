namespace AthensServiceDesk.Infrastructure.Persistence.Seeding;

public sealed class DemoUserOptions
{
    public const string SectionName = "DemoUsers";
    public bool Enabled { get; set; }
    public string DefaultPassword { get; set; } = string.Empty;
}
