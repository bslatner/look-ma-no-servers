using System;

namespace NoServers.DataAccess
{
    public class Registration
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public DateTime Timestamp { get; set; }
    }
}