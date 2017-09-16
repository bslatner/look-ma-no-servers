using System;
using Amazon.DynamoDBv2.DataModel;

namespace NoServers.DataAccess.Aws.Model
{
    [DynamoDBTable("event-registration-registration")]
    public class RegistrationDto
    {
        [DynamoDBRangeKey]
        public Guid Id { get; set; }

        [DynamoDBHashKey]
        public Guid EventId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public DateTime Timestamp { get; set; }
    }
}