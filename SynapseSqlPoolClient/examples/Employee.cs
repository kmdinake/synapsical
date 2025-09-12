namespace EFCoreUsageExample
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }

        public Employee(string name, string position, Guid id)
        {
            Id = id;
            Name = name;
            Position = position;
        }

        public static Employee Ghost => new("Ghost", "N/A", Guid.Empty);
    }
}