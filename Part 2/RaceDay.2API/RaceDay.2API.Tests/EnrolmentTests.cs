using System.Net;
using RaceDay._2API.Dtos;

namespace RaceDay._2API.Tests;

public class EnrolmentTests : ApiTestBase
{
    [Fact]
    public async Task Participant_CanEnrol_AndEnrolmentIsRecorded()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var participantId = await GetUserIdAsync(participant);
        var ev = await CreateEventAsync(organiser);
        var category = await CreateCategoryAsync(organiser, ev.EventID, "10km");

        var response = await EnrolRawAsync(participant, ev.EventID, category.CategoryID);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var enrolment = await ReadAsync<EnrolmentResponseDto>(response);

        // The record links the participant, the event and the chosen category
        Assert.Equal(participantId, enrolment.ParticipantID);
        Assert.Equal(ev.EventID, enrolment.EventID);
        Assert.Equal(category.CategoryID, enrolment.CategoryID);
        Assert.Equal("Active", enrolment.Status);

        // And it appears in the participant's own list
        var mine = await ReadAsync<List<EnrolmentResponseDto>>(
            await participant.GetAsync("/api/users/me/enrolments"));
        Assert.Contains(mine, e => e.EnrolmentID == enrolment.EnrolmentID);
    }

    [Fact]
    public async Task Enrol_Twice_ReturnsConflict()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);
        var category = await CreateCategoryAsync(organiser, ev.EventID);
        await EnrolAsync(participant, ev.EventID, category.CategoryID);

        var second = await EnrolRawAsync(participant, ev.EventID, category.CategoryID);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Enrol_WithCategoryFromAnotherEvent_ReturnsBadRequest()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var eventA = await CreateEventAsync(organiser);
        var eventB = await CreateEventAsync(organiser);
        var categoryOfB = await CreateCategoryAsync(organiser, eventB.EventID);

        var response = await EnrolRawAsync(participant, eventA.EventID, categoryOfB.CategoryID);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_UnknownEvent_ReturnsNotFound()
    {
        var participant = await LoggedInClientAsync("Participant");

        var response = await EnrolRawAsync(participant, 999, 1);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_InPastEvent_ReturnsBadRequest()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var ev = await CreateEventAsync(organiser, yesterday);
        var category = await CreateCategoryAsync(organiser, ev.EventID);

        var response = await EnrolRawAsync(participant, ev.EventID, category.CategoryID);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CanCancelOwnEnrolment()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);
        var category = await CreateCategoryAsync(organiser, ev.EventID);
        var enrolment = await EnrolAsync(participant, ev.EventID, category.CategoryID);

        var delete = await participant.DeleteAsync($"/api/enrolments/{enrolment.EnrolmentID}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var mine = await ReadAsync<List<EnrolmentResponseDto>>(
            await participant.GetAsync("/api/users/me/enrolments"));
        Assert.Equal("Cancelled", Assert.Single(mine).Status);
    }

    [Fact]
    public async Task Participant_CannotCancelAnotherParticipantsEnrolment()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var owner = await LoggedInClientAsync("Participant");
        var other = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);
        var category = await CreateCategoryAsync(organiser, ev.EventID);
        var enrolment = await EnrolAsync(owner, ev.EventID, category.CategoryID);

        var response = await other.DeleteAsync($"/api/enrolments/{enrolment.EnrolmentID}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanViewEnrolments_ForOwnEvent()
    {
        var organiser = await LoggedInClientAsync("Organiser");
        var participant = await LoggedInClientAsync("Participant");
        var ev = await CreateEventAsync(organiser);
        var category = await CreateCategoryAsync(organiser, ev.EventID);
        var enrolment = await EnrolAsync(participant, ev.EventID, category.CategoryID);

        var response = await organiser.GetAsync($"/api/events/{ev.EventID}/enrolments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await ReadAsync<List<EnrolmentResponseDto>>(response);
        Assert.Equal(enrolment.EnrolmentID, Assert.Single(list).EnrolmentID);
    }

    [Fact]
    public async Task Organiser_CannotViewEnrolments_ForAnotherOrganisersEvent()
    {
        var owner = await LoggedInClientAsync("Organiser");
        var other = await LoggedInClientAsync("Organiser");
        var ev = await CreateEventAsync(owner);

        var response = await other.GetAsync($"/api/events/{ev.EventID}/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}