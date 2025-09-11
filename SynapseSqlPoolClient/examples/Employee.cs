namespace EFCoreUsageExample
{
    public class Employee(string name, string position, Guid id)
    {
        public Guid Id { get; set; } = id;
        public string Name { get; set; } = name;
        public string Position { get; set; } = position;

        public static Employee Ghost => new("Ghost", "N/A", Guid.Empty);
    }
}