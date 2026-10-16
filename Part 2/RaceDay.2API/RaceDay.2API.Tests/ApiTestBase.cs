using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RaceDay._2API.Dtos;

namespace RaceDay._2API.Tests;

public abstract class ApiTestBase : IDisposable
{
    protected const string TestPassword = "Password123!";

    protected readonly RaceDayFactory Factory = new();
    private int _userCounter;

    public void Dispose() => Factory.Dispose();

    // A new client has its own cookie jar, so it behaves like one browser session
    protected HttpClient NewClient() =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected static async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>())!;

    // Registers a new user with the given role, logs in, and returns the logged-in client
    protected async Task<HttpClient> LoggedInClientAsync(string role)
    {
        var client = NewClient();
        var email = $"{role.ToLowerInvariant()}{++_userCounter}@test.com";

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "0821234567",
            Email = email,
            Password = TestPassword,
            Role = role,
            OrganiserCode = role == "Organiser" ? RaceDayFactory.OrganiserCode : null
        });
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = TestPassword });
        login.EnsureSuccessStatusCode();

        return client;
    }

    protected async Task<int> GetUserIdAsync(HttpClient client)
    {
        var me = await ReadAsync<ProfileResponseDto>(await client.GetAsync("/api/users/me"));
        return me.UserID;
    }

    protected static EventRequestDto SampleEvent(DateOnly? date = null) => new()
    {
        EventName = "Test Marathon",
        EventType = "Run",
        EventDate = date ?? DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
        StartTime = new TimeOnly(6, 0),
        EndTime = new TimeOnly(11, 0),
        Location = "Johannesburg CBD",
        DistanceKm = 42.2m,
        Description = "Test event"
    };

    protected async Task<EventResponseDto> CreateEventAsync(HttpClient organiser, DateOnly? date = null)
    {
        var response = await organiser.PostAsJsonAsync("/api/events", SampleEvent(date));
        response.EnsureSuccessStatusCode();
        return await ReadAsync<EventResponseDto>(response);
    }

    protected async Task<CategoryResponseDto> CreateCategoryAsync(
        HttpClient organiser, int eventId, string name = "10km")
    {
        var response = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories",
            new CategoryRequestDto { CategoryName = name });
        response.EnsureSuccessStatusCode();
        return await ReadAsync<CategoryResponseDto>(response);
    }

    protected Task<HttpResponseMessage> EnrolRawAsync(HttpClient participant, int eventId, int categoryId) =>
        participant.PostAsJsonAsync($"/api/events/{eventId}/enrolments",
            new EnrolmentRequestDto { CategoryID = categoryId });

    protected async Task<EnrolmentResponseDto> EnrolAsync(HttpClient participant, int eventId, int categoryId)
    {
        var response = await EnrolRawAsync(participant, eventId, categoryId);
        response.EnsureSuccessStatusCode();
        return await ReadAsync<EnrolmentResponseDto>(response);
    }
}