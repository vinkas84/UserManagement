namespace Domain.Models
{
    public class Client
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ApiKey { get; set; }
    }
}