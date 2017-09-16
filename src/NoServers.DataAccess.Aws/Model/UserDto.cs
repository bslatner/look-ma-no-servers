using Amazon.DynamoDBv2.DataModel;

namespace NoServers.DataAccess.Aws.Model
{
    [DynamoDBTable("event-registration-user")]
    public class UserDto
    {
        [DynamoDBHashKey]
        public string Identifier { get; set; }

        public string Role { get; set; }
    }
}