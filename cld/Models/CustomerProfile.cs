using Microsoft.Azure.Cosmos.Table;

namespace cld.Models
{
    public class CustomerProfile : TableEntity
    {
        public CustomerProfile() { }

        public CustomerProfile(string email)
        {
            PartitionKey = email;
            RowKey = email;
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }

        // Implement the Timestamp property
        public DateTimeOffset Timestamp { get; set; }
    }
}
