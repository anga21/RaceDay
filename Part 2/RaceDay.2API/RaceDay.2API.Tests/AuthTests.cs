using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay._2API.Data;
using RaceDay._2API.Dtos;

namespace RaceDay._2API.Tests;

public class AuthTests : ApiTestBase
{
    private static RegisterDto NewRegistration(string role, string email, string? code = null) => new()
    {
        FirstName = "Test",
        LastName = "User",
        PhoneNumber = "0821234567",
        Email = email,
        Password = TestPassword,
        Role = role,
        OrganiserCode = code
    };

    [Fact]
    public async Task Register_ValidParticipant_ReturnsCreated()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            NewRegistration("Participant", "lindiwe@test.com"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await ReadAsync<UserResponseDto>(response);
        Assert.Equal("Participant", body.Role);
        Assert.Equal("lindiwe@test.com", body.Email);
    }

    [Fact]
    public async Task Register_Password_IsStoredHashed()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/auth/register",
            NewRegistration("Participant", "hash@test.com"));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
        var user = db.Users.Single(u => u.Email == "hash@test.com");

        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
        Assert.NotEqual(TestPassword, user.PasswordHash);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var client = NewClient();
        var dto = NewRegistration("Participant", "dup@test.com");
        await client.PostAsJsonAsync("/api/auth/register", dto);

        var second = await client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_Organiser_WithoutCode_ReturnsForbidden()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            NewRegistration("Organiser", "org@test.com", code: null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_Organiser_WithCorrectCode_ReturnsCreated()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            NewRegistration("Organiser", "org@test.com", RaceDayFactory.OrganiserCode));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await ReadAsync<UserResponseDto>(response);
        Assert.Equal("Organiser", body.Role);
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            NewRegistration("Admin", "admin@test.com"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_ReturnsBadRequest()
    {
        var client = NewClient();
        var dto = NewRegistration("Participant", "short@test.com");
        dto.Password = "abc";

        var response = await client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_CorrectCredentials_ReturnsOk()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/auth/register",
            NewRegistration("Participant", "login@test.com"));

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "login@test.com", Password = TestPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/auth/register",
            NewRegistration("Participant", "wrongpw@test.com"));

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "wrongpw@test.com", Password = "WrongPassword1!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "nobody@test.com", Password = TestPassword });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_EndsTheSession()
    {
        var client = await LoggedInClientAsync("Participant");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/users/me")).StatusCode);

        var logout = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/users/me")).StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var client = NewClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}