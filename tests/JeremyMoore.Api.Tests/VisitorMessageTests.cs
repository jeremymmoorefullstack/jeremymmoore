using JeremyMoore.Api.Agent;

namespace JeremyMoore.Api.Tests;

public class VisitorMessageTests
{
    [Fact]
    public void A_complete_message_has_no_problems() =>
        Assert.Empty(new VisitorMessage("Ada", "ada@example.com", "Need a .NET contractor for Q1.", "Acme").Problems());

    [Theory]
    [InlineData(null, "ada@example.com", "Hi", "a name")]
    [InlineData("Ada", "not-an-email", "Hi", "a valid email address")]
    [InlineData("Ada", "ada@example.com", " ", "a message")]
    public void Missing_or_bad_fields_are_named(string? name, string? email, string? message, string expected) =>
        Assert.Contains(new VisitorMessage(name, email, message).Problems(), problem => problem.StartsWith(expected));

    [Fact]
    public void Overlong_messages_are_rejected() =>
        Assert.NotEmpty(new VisitorMessage("Ada", "ada@example.com", new string('x', 2001)).Problems());
}
