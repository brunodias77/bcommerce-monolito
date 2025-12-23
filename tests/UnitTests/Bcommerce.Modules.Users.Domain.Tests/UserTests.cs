using Bcommerce.Modules.Users.Domain.Entities;
using Bcommerce.Modules.Users.Domain.Events;
using Bcommerce.Modules.Users.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bcommerce.Modules.Users.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_ShouldCreateUser_WhenValidDataProvided()
    {
        // Arrange
        var firstName = "Bruno";
        var lastName = "Dias";
        var email = "bruno.dias@example.com";
        var password = "Password123!";

        // Act
        var user = User.Create(firstName, lastName, new Email(email), new Password(password));

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Value.Should().Be(email);
        user.DomainEvents.Should().ContainSingle(e => e is UserRegisteredEvent);
    }

    [Fact]
    public void Create_ShouldInitializeWithDefaultStatus()
    {
        // Act
        var user = User.Create("First", "Last", new Email("test@test.com"), new Password("Pass123!"));

        // Assert
        user.IsActive.Should().BeTrue();
    }
}
