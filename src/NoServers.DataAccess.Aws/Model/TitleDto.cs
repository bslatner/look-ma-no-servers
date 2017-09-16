using Amazon.DynamoDBv2.DataModel;

namespace NoServers.DataAccess.Aws.Model
{
    [DynamoDBTable("event-registration-title")]
    public class TitleDto
    {
        [DynamoDBHashKey]
        public string Id { get; set; }

        public string Text { get; set; }
    }
}
