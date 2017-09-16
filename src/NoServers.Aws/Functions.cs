using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NoServers.Aws.Security;
using NoServers.DataAccess;
using NoServers.DataAccess.Aws;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace NoServers.Aws
{
    public class Functions
    {
        private readonly IDataAccess _DataAccess;
        private readonly ITokenVerifier _TokenVerifier;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            // configure data access
            var region = RegionEndpoint.GetBySystemName(
                GetEnvironmentVariableWithDefault("AWS_DEFAULT_REGION", "us-east-1"));

            var ddb = new AmazonDynamoDBClient(region);
            var ctx = new DynamoDBContext(ddb, new DynamoDBContextConfig
            {
                ConsistentRead = true
            });
            _DataAccess = new AwsDataAccess(ctx);

            // configure auth0
            var assembly = typeof(Functions).GetTypeInfo().Assembly;
            var auth0 = JsonConvert.DeserializeObject<Auth0Options>(
                ResourceHelper.GetResourceAsString(
                    assembly,
                    "NoServers.Aws.auth0.json"));
            var tokenVerifierOptions = new TokenVerifierOptions($"https://{auth0.Domain}/", auth0.ClientId,
                CertificateHelper.GetCertificateFromResource(
                    assembly,
                    "NoServers.Aws.private.key"
                ));
            _TokenVerifier = new TokenVerifier(tokenVerifierOptions);

            // use camelCasing when returning JSON
            JsonConvert.DefaultSettings =
                () => new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
        }

        #region "Respond to OPTIONS requests"

        // Responds to the OPTIONS verb for resources that support only GET
        public Task<APIGatewayProxyResponse> GetGetOptionsAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return GetSingleMethodOptionsResult("GET");
        }

        // Responds to the OPTIONS verb for resources that support only POST
        public Task<APIGatewayProxyResponse> GetPostOptionsAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return GetSingleMethodOptionsResult("POST");
        }

        // Responds to the OPTIONS verb for resources that support only DELETE
        public Task<APIGatewayProxyResponse> GetDeleteOptionsAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return GetSingleMethodOptionsResult("DELETE");
        }

        // Responds to the OPTIONS verb for resources that support only PUT
        public Task<APIGatewayProxyResponse> GetPutOptionsAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return GetSingleMethodOptionsResult("PUT");
        }

        // Responds to the OPTIONS verb for resources that support GET and POST
        public Task<APIGatewayProxyResponse> GetGetAndPostOptionsAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return Task.FromResult(GetOptionsResponse(new[] { "GET", "POST" }));
        }

        // Responds to the OPTIONS verb for resources that support POST and PUT
        public Task<APIGatewayProxyResponse> GetPostAndPutOptionsAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return Task.FromResult(GetOptionsResponse(new[] { "POST", "PUT" }));
        }

        // Responds to the OPTIONS verb for resources that support GET and DELETE
        public Task<APIGatewayProxyResponse> GetGetAndDeleteOptionsAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return Task.FromResult(GetOptionsResponse(new[] { "GET", "DELETE" }));
        }

        private static Task<APIGatewayProxyResponse> GetSingleMethodOptionsResult(string methodName)
        {
            return Task.FromResult(GetOptionsResponse(new[] { methodName }));
        }

        private static APIGatewayProxyResponse GetOptionsResponse(IEnumerable<string> allowedMethods)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = new Dictionary<string, string>().AddCorsForOptionsVerb(allowedMethods)
            };
        }

        #endregion

        #region "Authentication"

        private Task<APIGatewayProxyResponse> VerifyAuthentication(
            APIGatewayProxyRequest request,
            ILambdaContext context,
            Func<APIGatewayProxyRequest, ILambdaContext, string, Task<APIGatewayProxyResponse>> action
        )
        {
            const string bearerPrefix = "Bearer ";

            var auth = request.Headers["Authorization"];
            if (!auth.StartsWith(bearerPrefix))
            {
                var msg = $"Malformed or missing Authorization header: {auth}";
                context.Logger.Log(msg);
                return Task.FromResult(GetForbiddenResponse("Malformed or missing Authorization header"));
            }

            string user;
            try
            {
                var token = auth.Substring(bearerPrefix.Length).Trim();
                user = _TokenVerifier.GetVerifiedUserFromToken(token);
            }
            catch (Exception ex)
            {
                context.Logger.Log($"An exception of type {ex.GetType()} occurred while parsing token: {ex.Message}");
                return Task.FromResult(GetForbiddenResponse("Error validating token"));
            }
            return action(request, context, user);
        }

        private Task<APIGatewayProxyResponse> VerifyAuthentication(
            APIGatewayProxyRequest request,
            ILambdaContext context,
            Func<Task<APIGatewayProxyResponse>> action
        )
        {
            return VerifyAuthentication(request, context, (r, c, u) => action());
        }

        private Task<APIGatewayProxyResponse> VerifyAdmin(
            APIGatewayProxyRequest request,
            ILambdaContext context,
            Func<Task<APIGatewayProxyResponse>> action
        )
        {
            return VerifyAuthentication(request, context, async (r, c, u) =>
            {
                var user = await _DataAccess.GetUserByIdentifierAsync(u).ConfigureAwait(false);
                if (user == null)
                {
                    context.Logger.Log($"Could not find user with identifier='{u}'");
                    return GetForbiddenResponse("Invalid user");
                }
                if (user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    return await action().ConfigureAwait(false);
                }
                context.Logger.Log($"User {u} is not an administator");
                return GetForbiddenResponse("Access Denied");
            });
        }

        #endregion

        // operation: GetTitles
        public Task<APIGatewayProxyResponse> GetTitlesAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAuthentication(request, context, async () => 
                GetJsonResponse(await _DataAccess.GetTitlesAsync().ConfigureAwait(false)));
        }

        // operation: CreateTitle
        public Task<APIGatewayProxyResponse> CreateTitleAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAdmin(request, context, async () =>
            {
                var title = JsonConvert.DeserializeObject<Title>(request?.Body);
                context.Logger.Log($"Saving title with Id={title.Id} Text={title.Text}");
                await _DataAccess.CreateTitleAsync(title).ConfigureAwait(false);
                return GetJsonResponseForCreated(title);
            });
        }

        // operation: GetUserByIdentifier
        public Task<APIGatewayProxyResponse> GetUserByIdentifierAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAuthentication(request, context, async () =>
            {
                var identifier = request.PathParameters["identifier"];
                var user = await _DataAccess.GetUserByIdentifierAsync(identifier).ConfigureAwait(false);
                return GetJsonResponse(user);
            });
        }

        // operation: CreateUser
        public Task<APIGatewayProxyResponse> CreateUserAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAdmin(request, context, async () =>
            {
                var user = JsonConvert.DeserializeObject<User>(request?.Body);
                context.Logger.Log($"Saving user with identifier={user.Identifier}");
                await _DataAccess.CreateUserAsync(user).ConfigureAwait(false);
                return GetJsonResponseForCreated(user);
            });
        }

        // operation: GetEventsAfter
        public Task<APIGatewayProxyResponse> GetEventsAfter(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAdmin(request, context, async () =>
            {
                var cutoff = DateTime.Parse(request.PathParameters["cutoff"]);
                var evts = await _DataAccess.GetEventsAfterAsync(cutoff).ConfigureAwait(false);
                return GetJsonResponse(evts);
            });
        }

        // operation: GetEventById
        public Task<APIGatewayProxyResponse> GetEventByIdAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAdmin(request, context, async () =>
            {
                var id = Guid.Parse(request.PathParameters["id"]);
                var evt = await _DataAccess.GetEventByIdAsync(id).ConfigureAwait(false);
                return GetJsonResponse(evt);
            });
        }

        // operation: GetActiveEvent
        public Task<APIGatewayProxyResponse> GetActiveEventAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAuthentication(request, context, async () =>
            {
                var evt = await _DataAccess.GetActiveEvent().ConfigureAwait(false);
                return GetJsonResponse(evt);
            });
        }

        // operation: DeleteEvent
        public Task<APIGatewayProxyResponse> DeleteEventAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAdmin(request, context, async () =>
            {
                var id = Guid.Parse(request.PathParameters["id"]);
                await _DataAccess.DeleteEventAsync(id).ConfigureAwait(false);
                return GetSimpleResponse();
            });
        }

        // operation: CreateEvent
        public Task<APIGatewayProxyResponse> CreateEventAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAdmin(request, context, async () =>
            {
                var evt = JsonConvert.DeserializeObject<Event>(request?.Body);
                evt.Id = Guid.NewGuid();
                await _DataAccess.CreateEventAsync(evt).ConfigureAwait(false);
                return GetJsonResponseForCreated(evt);
            });
        }

        // operation: UpdateEvent
        public Task<APIGatewayProxyResponse> UpdateEventAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAdmin(request, context, async () =>
            {
                var evt = JsonConvert.DeserializeObject<Event>(request?.Body);
                await _DataAccess.UpdateEventAsync(evt).ConfigureAwait(false);
                return GetJsonResponse(evt);
            });
        }

        // operation: GetRegistrations
        public Task<APIGatewayProxyResponse> GetRegistrationsAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAdmin(request, context, async () =>
            {
                var id = Guid.Parse(request.PathParameters["id"]);
                var registrations = await _DataAccess.GetRegistrationsAsync(id).ConfigureAwait(false);
                return GetJsonResponse(registrations);
            });
        }

        // operation: CreateRegistration
        public Task<APIGatewayProxyResponse> CreateRegistrationAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return VerifyAuthentication(request, context, async () =>
            {
                var registration = JsonConvert.DeserializeObject<Registration>(request?.Body);
                registration.Id = Guid.NewGuid();
                await _DataAccess.CreateRegistrationAsync(registration).ConfigureAwait(false);
                return GetJsonResponseForCreated(registration);
            });
        }

        private static string GetEnvironmentVariableWithDefault(string name, string defaultValue)
        {
            return Environment.GetEnvironmentVariable(name) ?? defaultValue;
        }

        private static APIGatewayProxyResponse GetJsonResponse<T>(T data)
        {
            return GetJsonResponseWithHttpStatus(data, HttpStatusCode.OK);
        }

        private static APIGatewayProxyResponse GetJsonResponseForCreated<T>(T data)
        {
            return GetJsonResponseWithHttpStatus(data, HttpStatusCode.Created);
        }

        private static APIGatewayProxyResponse GetJsonResponseWithHttpStatus<T>(T data, HttpStatusCode httpStatusCode)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)httpStatusCode,
                Body = JsonConvert.SerializeObject(data),
                Headers = GetJsonHeaders()
            };
        }

        private static Dictionary<string, string> GetJsonHeaders()
        {
            return new Dictionary<string, string>()
                .AddContentType("application/json")
                .AddCorsOrigin();
        }

        private static APIGatewayProxyResponse GetSimpleResponse(HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)httpStatusCode,
                Headers = GetResponseHeaders()
            };
        }

        private static APIGatewayProxyResponse GetForbiddenResponse(string reason = null)
        {
            var response = GetSimpleResponse(HttpStatusCode.Forbidden);
            #if DEBUG
            if (reason != null) response.Body = reason;
            #endif
            return response;
        }

        private static Dictionary<string, string> GetResponseHeaders()
        {
            return new Dictionary<string, string>().AddCorsOrigin();
        }
    }
}
