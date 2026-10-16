using System.Net;
using System.Net.Http.Json;
using RaceDay._2API.Dtos;

namespace RaceDay._2API.Tests;

public class RoleAccessTests : ApiTestBase
{
    [Fact]
    public async Task Unauthenticated_CannotCreateEvent_Returns401()
    {
        var response = await NewClient().PostAsJsonAsync("/api/events", SampleEvent());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent_Returns403()
    {
        var participant = await LoggedInClientAsync("Participant");

        var response = await participant.PostAsJsonAsync("/api/events", SampleEvent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotUpdateEvent_Returns403()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);

        var response = await participant.PutAsJsonAsync($"/api/events/{ev.EventID}", SampleEvent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotDeleteEvent_Returns403()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);

        var response = await participant.DeleteAsync($"/api/events/{ev.EventID}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotAddCategory_Returns403()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);

        var response = await participant.PostAsJsonAsync($"/api/events/{ev.EventID}/categories",
            new CategoryRequestDto { CategoryName = "Senior" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotViewEventEnrolments_Returns403()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);

        var response = await participant.GetAsync($"/api/events/{ev.EventID}/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCaptureResult_Returns403()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);
        var category = await CreateCategoryAsync(organiser, ev.EventID);
        var enrolment = await EnrolAsync(participant, ev.EventID, category.CategoryID);

        var response = await participant.PutAsJsonAsync(
            $"/api/enrolments/{enrolment.EnrolmentID}/result",
            new ResultRequestDto { Position = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotEnrolInEvent_Returns403()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var ev = await CreateEventAsync(organiser);
        var category = await CreateCategoryAsync(organiser, ev.EventID);

        var response = await EnrolRawAsync(organiser, ev.EventID, category.CategoryID);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotViewParticipantEnrolments_Returns403()
    {
        var organiser = await LoggedInClientAsync("Organiser");

        var response = await organiser.GetAsync("/api/users/me/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_CannotEnrol_Returns401()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var ev = await CreateEventAsync(organiser);
        var category = await CreateCategoryAsync(organiser, ev.EventID);

        var response = await EnrolRawAsync(NewClient(), ev.EventID, category.CategoryID);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}