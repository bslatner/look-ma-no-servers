using System;
using Amazon.DynamoDBv2.DataModel;

namespace NoServers.DataAccess.Aws.Model
{
    [DynamoDBTable("event-registration-event")]
    public class EventDto
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsActive { get; set; }
    }
}