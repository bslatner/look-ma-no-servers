using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using NoServers.DataAccess.Aws.Model;

namespace NoServers.DataAccess.Aws
{
    public class AwsDataAccess : IDataAccess
    {
        private readonly IDynamoDBContext _Context;

        public AwsDataAccess(IDynamoDBContext context)
        {
            _Context = context;
        }

        private static async Task<List<T>> GetAllAsync<T>(AsyncSearch<T> searchResult)
        {
            List<T> result;
            do
            {
                result = await searchResult.GetRemainingAsync().ConfigureAwait(false);
            } while (!searchResult.IsDone);
            return result;
        }

        public async Task<List<Title>> GetTitlesAsync()
        {
            var result = await GetAllAsync(_Context.ScanAsync<TitleDto>(null)).ConfigureAwait(false);
            return result
                .Select(GetTitleFromDto)
                .ToList();
        }

        public async Task<Title> CreateTitleAsync(string id, string text)
        {
            var dto = GetDtoFromTitle(id, text);
            await _Context.SaveAsync(dto).ConfigureAwait(false);
            return GetTitleFromDto(dto);
        }

        public async Task<Title> CreateTitleAsync(Title title)
        {
            var dto = GetDtoFromTitle(title);
            await _Context.SaveAsync(dto).ConfigureAwait(false);
            return title;
        }

        private static TitleDto GetDtoFromTitle(string id, string text)
        {
            return new TitleDto
            {
                Id = id,
                Text = text
            };
        }

        private static TitleDto GetDtoFromTitle(Title title)
        {
            return new TitleDto
            {
                Id = title.Id,
                Text = title.Text
            };
        }

        private static Title GetTitleFromDto(TitleDto dto)
        {
            return new Title
            {
                Id = dto.Id,
                Text = dto.Text
            };
        }

        public async Task<User> GetUserByIdentifierAsync(string identifier)
        {
            return GetUserFromDto(
                await _Context.LoadAsync<UserDto>(identifier).ConfigureAwait(false));
        }

        public async Task<User> CreateUserAsync(string identifier, string role)
        {
            var dto = GetDtoFromUser(identifier, role);
            await _Context.SaveAsync(dto).ConfigureAwait(false);
            return GetUserFromDto(dto);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var dto = GetDtoFromUser(user);
            await _Context.SaveAsync(dto).ConfigureAwait(false);
            return user;
        }

        private static UserDto GetDtoFromUser(string identifier, string role)
        {
            return new UserDto
            {
                Identifier = identifier,
                Role = role
            };
        }

        private static UserDto GetDtoFromUser(User user)
        {
            return new UserDto
            {
                Identifier = user.Identifier,
                Role = user.Role
            };
        }

        private static User GetUserFromDto(UserDto dto)
        {
            if (dto == null) return null;
            return new User
            {
                Identifier = dto.Identifier,
                Role = dto.Role
            };
        }

        public async Task<List<Event>> GetEventsAfterAsync(DateTime cutoff)
        {
            //TODO: Create secondary index and query it.
            var events = await GetAllAsync(_Context.ScanAsync<EventDto>(null)).ConfigureAwait(false);
            return events
                .Where(e => e.DateTime >= cutoff)
                .Select(GetEventFromDto)
                .ToList();
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            var dto = await _Context.LoadAsync<EventDto>(id).ConfigureAwait(false);
            return GetEventFromDto(dto);
        }

        public async Task<Event> GetActiveEvent()
        {
            var conditions = new[] { new ScanCondition("IsActive", ScanOperator.Equal, true) };
            var search = _Context.ScanAsync<EventDto>(conditions);
            var active = await search.GetRemainingAsync().ConfigureAwait(false);
            if (active.Count == 0)
            {
                return null;
            }
            if (active.Count == 1)
            {
                return GetEventFromDto(active[0]);
            }
            throw new InvalidOperationException("More than one event is marked active.");
        }

        public Task CreateEventAsync(Event evt)
        {
            return _Context.SaveAsync(GetDtoFromEvent(evt));
        }

        public Task UpdateEventAsync(Event evt)
        {
            return _Context.SaveAsync(GetDtoFromEvent(evt));
        }

        public Task DeleteEventAsync(Guid id)
        {
            return _Context.DeleteAsync<EventDto>(id);
        }

        private static EventDto GetDtoFromEvent(Event evt)
        {
            return new EventDto
            {
                Id = evt.Id,
                Name = evt.Name,
                DateTime = evt.DateTime,
                IsActive = evt.IsActive
            };
        }

        private static Event GetEventFromDto(EventDto dto)
        {
            if (dto == null) return null;
            return new Event
            {
                Id = dto.Id,
                Name = dto.Name,
                DateTime = dto.DateTime,
                IsActive = dto.IsActive
            };
        }

        public Task CreateRegistrationAsync(Registration registration)
        {
            return _Context.SaveAsync(GetDtoFromRegistration(registration));
        }

        public async Task<List<Registration>> GetRegistrationsAsync(Guid eventId)
        {
            var regs = await GetAllAsync(
                _Context.QueryAsync<RegistrationDto>(eventId)).ConfigureAwait(false);
            return regs.Select(GetRegistrationFromDto).ToList();
        }

        public Task DeleteRegistrationAsync(Registration registration)
        {
            return _Context.DeleteAsync<RegistrationDto>(registration.EventId, registration.Id);
        }

        private static RegistrationDto GetDtoFromRegistration(Registration registration)
        {
            return new RegistrationDto
            {
                Id = registration.Id,
                EventId = registration.EventId,
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                EmailAddress = registration.EmailAddress,
                Company = registration.Company,
                Title = registration.Title,
                Timestamp = registration.Timestamp
            };
        }

        private static Registration GetRegistrationFromDto(RegistrationDto dto)
        {
            return new Registration
            {
                Id = dto.Id,
                EventId = dto.EventId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailAddress = dto.EmailAddress,
                Company = dto.Company,
                Title = dto.Title,
                Timestamp = dto.Timestamp
            };
        }
    }
}
