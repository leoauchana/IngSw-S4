using IngSw_Application.DTOs;
using IngSw_Application.Exceptions;
using IngSw_Application.Services;
using IngSw_Domain.Entities;
using IngSw_Domain.Interfaces;
using IngSw_Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace IngSw_Tests.RegisterPatient;

public class AuthServiceTest
{
	private readonly IAuthRepository _authRepository;
	private readonly AuthService _authService;
	public AuthServiceTest(/*IConfiguration configuration*/)
	{
		_authRepository = Substitute.For<IAuthRepository>();
		_authService = new AuthService(_authRepository/*, configuration*/);
	}

    [Fact]
    public async Task Login_WhenYouEnterTheCorrectEmailAndPassword_ThenYouLogIn_ShouldCreateTheEmployee()
	{
		// Arrange

		var userDto = new UserDto.Request("ramirobrito@gmail.com", "bocateamo");
		var userFound = new User
		{ 
			Email = "ramirobrito@gmail.com",
			Password = "bocateamo",
			Employee = new Employee
			{
				Name = "Ramiro",
				LastName = "Brito",
				Cuil = Cuil.Create("20-42365986-7"),
				PhoneNumber = "381754963",
				Email = "ramirobrito@gmail.com",
				Registration = "LO78Q"
			}
        };
		_authRepository.GetByEmail("ramirobrito@gmail.com")!
			.Returns(Task.FromResult(userFound));

		// Act

		var result = await _authService.Login(userDto);

        // Assert

        await _authRepository.Received(1).GetByEmail(Arg.Any<string>());
        Assert.NotNull(result);
        Assert.Equal(userFound.Email, result.email);
        Assert.Equal(userFound.Employee.Name, result.name);
        Assert.Equal(userFound.Employee.LastName, result.lastName);

    }

    [Fact]
    public async Task Login_WhenYouEnterNotValidPassword_ThenShouldEntityNotFoundException()
	{
		// Arrange

		var userDto = new UserDto.Request("ramirobrito@gmail.com", "riverteamo");
		var userFound = new User
		{ 
			Email = "ramirobrito@gmail.com",
			Password = "bocateamo",
			Employee = new Employee
			{
				Name = "Ramiro",
				LastName = "Brito",
				Cuil = Cuil.Create("20-42365986-7"),
				PhoneNumber = "381754963",
				Email = "ramirobrito@gmail.com",
				Registration = "LO78Q"
			}
        };
		_authRepository.GetByEmail("ramirobrito@gmail.com")!
			.Returns(Task.FromResult(userFound));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _authService.Login(userDto)
        );

        Assert.Equal("Usuario o contraseña incorrecto.", exception.Message);

        await _authRepository.Received(1).GetByEmail("ramirobrito@gmail.com");

    }

    [Fact]
    public async Task Login_WhenYouEnterEmptyEmail_ThenShouldBusinessConflictException()
	{
		// Arrange

		var userDto = new UserDto.Request("", "riverteamo");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessConflictException>(
            () => _authService.Login(userDto)
        );

        Assert.Equal("Debe ingresar correctamente los datos", exception.Message);

    }

    [Fact]
    public async Task Login_WhenYouEnterEmptyPassword_ThenShouldBusinessConflictException()
	{
		// Arrange

		var userDto = new UserDto.Request("ramirobrito@gmail.com", "");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessConflictException>(
            () => _authService.Login(userDto)
        );

        Assert.Equal("Debe ingresar correctamente los datos", exception.Message);

    }


}
