using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoServers.DataAccess
{
    public interface IDataAccess
    {
        Task<List<Title>> GetTitlesAsync();
        Task<Title> CreateTitleAsync(string id, string text);
        Task<Title> CreateTitleAsync(Title title);

        Task<User> GetUserByIdentifierAsync(string identifier);
        Task<User> CreateUserAsync(string identifier, string role);
        Task<User> CreateUserAsync(User user);

        Task<List<Event>> GetEventsAfterAsync(DateTime cutoff);
        Task<Event> GetEventByIdAsync(Guid id);
        Task<Event> GetActiveEvent();
        Task CreateEventAsync(Event evt);
        Task UpdateEventAsync(Event evt);
        Task DeleteEventAsync(Guid id);

        Task CreateRegistrationAsync(Registration registration);
        Task<List<Registration>> GetRegistrationsAsync(Guid eventId);
        Task DeleteRegistrationAsync(Registration registration);
    }
}