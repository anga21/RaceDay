using System.Net;
using System.Net.Http.Json;
using RaceDay._2API.Dtos;

namespace RaceDay._2API.Tests;

public class EventTests : ApiTestBase
{
    [Fact]
    public async Task Organiser_CanCreateEvent()
    {
        var organiser = await LoggedInClientAsync("Organiser");

        var response = await organiser.PostAsJsonAsync("/api/events", SampleEvent());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await ReadAsync<EventResponseDto>(response);
        Assert.Equal("Run", body.EventType);
        Assert.Equal(42.2m, body.DistanceKm);
    }

    [Fact]
    public async Task Organiser_CanUpdateOwnEvent()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var created = await CreateEventAsync(organiser);
        var update = SampleEvent();
        update.EventName = "Renamed Marathon";

        var response = await organiser.PutAsJsonAsync($"/api/events/{created.EventID}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<EventResponseDto>(response);
        Assert.Equal("Renamed Marathon", body.EventName);
    }

    [Fact]
    public async Task Organiser_CanDeleteOwnEvent()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var created = await CreateEventAsync(organiser);

        var delete = await organiser.DeleteAsync($"/api/events/{created.EventID}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var get = await NewClient().GetAsync($"/api/events/{created.EventID}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidEventType_ReturnsBadRequest()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var bad = SampleEvent();
        bad.EventType = "Swim";

        var response = await organiser.PostAsJsonAsync("/api/events", bad);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_EndTimeBeforeStartTime_ReturnsBadRequest()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var bad = SampleEvent();
        bad.EndTime = new TimeOnly(5, 0);

        var response = await organiser.PostAsJsonAsync("/api/events", bad);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Anyone_CanViewEvents_WithoutLoggingIn()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var created = await CreateEventAsync(organiser);

        var response = await NewClient().GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await ReadAsync<List<EventResponseDto>>(response);
        Assert.Contains(events, e => e.EventID == created.EventID);
    }

    [Fact]
    public async Task GetEvent_UnknownId_ReturnsNotFound()
    {
        var response = await NewClient().GetAsync("/api/events/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotUpdateAnotherOrganisersEvent()
    {
        var owner = await LoggedInClientAsync("Organiser");
        var other = await LoggedInClientAsync("Organiser");
        var created = await CreateEventAsync(owner);

        var response = await other.PutAsJsonAsync($"/api/events/{created.EventID}", SampleEvent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotDeleteAnotherOrganisersEvent()
    {
        var owner = await LoggedInClientAsync("Organiser");
        var other = await LoggedInClientAsync("Organiser");
        var created = await CreateEventAsync(owner);

        var response = await other.DeleteAsync($"/api/events/{created.EventID}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_EventWithEnrolments_ReturnsConflict()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);
        var category = await CreateCategoryAsync(organiser, ev.EventID);
        await EnrolAsync(participant, ev.EventID, category.CategoryID);

        var response = await organiser.DeleteAsync($"/api/events/{ev.EventID}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}