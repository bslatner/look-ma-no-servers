using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace NoServers.DataAccess.Aws.Test
{
    class Program
    {
        static void Main()
        {
            // read config file
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var config = builder.Build();

            // build Dynamo DB client from config
            var awsOptions = config.GetAWSOptions();
            var client = awsOptions.CreateServiceClient<IAmazonDynamoDB>();
            var ctx = new DynamoDBContext(client, new DynamoDBContextConfig
            {
                ConsistentRead = true
            });
            var dataAccess = new AwsDataAccess(ctx);

            RunTest(dataAccess).Wait();
        }

        private static async Task RunTest(IDataAccess dataAccess)
        {
            await VerifyTitles(dataAccess).ConfigureAwait(false);
            await VerifyUsers(dataAccess).ConfigureAwait(false);
            await VerifyEvents(dataAccess).ConfigureAwait(false);
            await VerifyRegistrations(dataAccess).ConfigureAwait(false);
        }

        private static async Task VerifyTitles(IDataAccess dataAccess)
        {
            Console.WriteLine("Verifying titles...");
            var titles = await dataAccess.GetTitlesAsync().ConfigureAwait(false);
            if (titles.Count == 0)
            {
                Console.WriteLine("Adding titles...");
                await dataAccess.CreateTitleAsync("cio", "CIO").ConfigureAwait(false);
                await dataAccess.CreateTitleAsync("cto", "CTO").ConfigureAwait(false);
                await dataAccess.CreateTitleAsync("it-director", "Director of IT").ConfigureAwait(false);
                await dataAccess.CreateTitleAsync("net-admin", "Network Admin").ConfigureAwait(false);
                await dataAccess.CreateTitleAsync("support-specialist", "Support Specialist").ConfigureAwait(false);
                await dataAccess.CreateTitleAsync("other", "Other").ConfigureAwait(false);
                titles = await dataAccess.GetTitlesAsync().ConfigureAwait(false);
            }
            titles.ShouldContain(t => t.Id == "cio" && t.Text == "CIO");
            titles.ShouldContain(t => t.Id == "cto" && t.Text == "CTO");
            titles.ShouldContain(t => t.Id == "it-director" && t.Text == "Director of IT");
            titles.ShouldContain(t => t.Id == "net-admin" && t.Text == "Network Admin");
            titles.ShouldContain(t => t.Id == "support-specialist" && t.Text == "Support Specialist");
            titles.ShouldContain(t => t.Id == "other" && t.Text == "Other");
        }

        private static async Task VerifyUsers(IDataAccess dataAccess)
        {
            Console.WriteLine("Verifying users...");
            var user = await dataAccess.GetUserByIdentifierAsync("test").ConfigureAwait(false);
            if (user == null)
            {
                Console.WriteLine("Creating user...");
                await dataAccess.CreateUserAsync("test", "admin").ConfigureAwait(false);
                user = await dataAccess.GetUserByIdentifierAsync("test").ConfigureAwait(false);
            }
            user.Identifier.ShouldBe("test");
            user.Role.ShouldBe("admin");
        }

        private static async Task VerifyEvents(IDataAccess dataAccess)
        {
            Console.WriteLine("Verifying events...");
            var events = await dataAccess.GetEventsAfterAsync(new DateTime(2017, 1, 1)).ConfigureAwait(false);
            if (events.Count == 0)
            {
                Console.WriteLine("Creating test event...");
                await CreateTestEvent(dataAccess).ConfigureAwait(false);
                events = await dataAccess.GetEventsAfterAsync(new DateTime(2017, 1, 1)).ConfigureAwait(false);
            }
            var testEvent = events.SingleOrDefault(e => e.Name == "Test Event")
                            ?? await CreateTestEvent(dataAccess).ConfigureAwait(false);
            testEvent.Name.ShouldBe("Test Event");
            testEvent.IsActive.ShouldBeFalse();
            testEvent.DateTime.ShouldBe(new DateTime(2017, 12, 3));

            Console.WriteLine("Updating test event...");
            testEvent.Name = "Test Event (updated)";
            testEvent.IsActive = true;
            testEvent.DateTime = new DateTime(2017, 12, 10);
            await dataAccess.UpdateEventAsync(testEvent).ConfigureAwait(false);

            var updated = await dataAccess.GetEventByIdAsync(testEvent.Id).ConfigureAwait(false);
            updated.Id.ShouldBe(testEvent.Id);
            updated.Name.ShouldBe("Test Event (updated)");
            updated.IsActive.ShouldBeTrue();
            updated.DateTime.ShouldBe(new DateTime(2017, 12, 10));

            Console.WriteLine("Fetching active event...");
            var active = await dataAccess.GetActiveEvent().ConfigureAwait(false);
            active.Id.ShouldBe(testEvent.Id);
            active.Name.ShouldBe("Test Event (updated)");
            active.IsActive.ShouldBeTrue();
            active.DateTime.ShouldBe(new DateTime(2017, 12, 10));

            Console.WriteLine("Deleting test event...");
            await dataAccess.DeleteEventAsync(updated.Id).ConfigureAwait(false);
            events = await dataAccess.GetEventsAfterAsync(new DateTime(2017, 1, 1)).ConfigureAwait(false);
            events.Exists(e => e.Id == updated.Id).ShouldBeFalse();
        }

        private static async Task<Event> CreateTestEvent(IDataAccess dataAccess)
        {
            var evt = new Event
            {
                Id = Guid.NewGuid(),
                DateTime = new DateTime(2017, 12, 3),
                Name = "Test Event",
                IsActive = false
            };
            await dataAccess.CreateEventAsync(evt).ConfigureAwait(false);
            return evt;
        }

        private static async Task VerifyRegistrations(IDataAccess dataAccess)
        {
            Console.WriteLine("Verifying registrations...");

            Console.WriteLine("Creating registration...");
            var dt = new DateTime(2017, 8, 31, 15, 26, 0);
            var reg = new Registration
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "Person",
                Company = "BigCo, LLC",
                Title = "Bigwig",
                EmailAddress = "test@test.com",
                Timestamp = dt
            };
            await dataAccess.CreateRegistrationAsync(reg).ConfigureAwait(false);

            Console.WriteLine("Fetching all registrations...");
            var allRegs = await dataAccess.GetRegistrationsAsync(reg.EventId).ConfigureAwait(false);
            allRegs.Count.ShouldBe(1);
            allRegs[0].Id.ShouldBe(reg.Id);
            allRegs[0].EventId.ShouldBe(reg.EventId);
            allRegs[0].FirstName.ShouldBe("Test");
            allRegs[0].LastName.ShouldBe("Person");
            allRegs[0].Company.ShouldBe("BigCo, LLC");
            allRegs[0].Title.ShouldBe("Bigwig");
            allRegs[0].EmailAddress.ShouldBe("test@test.com");
            allRegs[0].Timestamp.ShouldBe(dt);

            Console.WriteLine("Deleting registration...");
            await dataAccess.DeleteRegistrationAsync(allRegs[0]).ConfigureAwait(false);
            allRegs = await dataAccess.GetRegistrationsAsync(reg.EventId).ConfigureAwait(false);
            allRegs.Count.ShouldBe(0);
        }
    }

}
