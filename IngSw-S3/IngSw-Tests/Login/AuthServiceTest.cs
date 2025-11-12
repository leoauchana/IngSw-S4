using IngSw_Application.DTOs;
using IngSw_Application.Exceptions;
using IngSw_Application.Interfaces;
using IngSw_Application.Services;
using IngSw_Domain.Entities;
using IngSw_Domain.Interfaces;
using IngSw_Domain.ValueObjects;
using NSubstitute;
using Xunit;

namespace IngSw_Tests.Login;

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
    public async Task Login_WhenYouEnterTheCorrectEmailAndPassword_ThenYouLogIn_ShouldGetTheEmployee()
	{
		// Arrange

		var userDto = new UserDto.Request("ramirobrito@gmail.com", "bocateamo");
		var userFound = new User
		{ 
			Email = "ramirobrito@gmail.com",
			Password = BCrypt.Net.BCrypt.HashPassword("bocateamo"),
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
    public async Task Login_WhenEnteredInvalidEmail_ShouldThrowEntityNotFoundException()
    {
        // Arrange

        var userDto = new UserDto.Request("matiasbrito@gmail.com", "bocateamo");
        var userFound = new User
        {
            Email = "ramirobrito@gmail.com",
            Password = BCrypt.Net.BCrypt.HashPassword("bocateamo"),
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
        await _authRepository.Received(1).GetByEmail(Arg.Any<string>());

    }
    [Fact]
    public async Task Login_WhenEnteredInvalidPassword_ShouldThrowEntityNotFoundException()
    {
		// Arrange

		var userDto = new UserDto.Request("ramirobrito@gmail.com", "riverteamo");
		var userFound = new User
		{ 
			Email = "ramirobrito@gmail.com",
			Password = BCrypt.Net.BCrypt.HashPassword("bocateamo"),
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
        await _authRepository.Received(1).GetByEmail(Arg.Any<string>());
    }
    [Fact]
    public async Task Login_WhenEnteredEmptyEmail_ShouldThrowArgumentException()
    {
		// Arrange

		var userDto = new UserDto.Request("", "riverteamo");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Login(userDto)
        );
        Assert.Equal("Debe ingresar correctamente los datos", exception.Message);
        await _authRepository.Received(0).GetByEmail(Arg.Any<string>());
    }
    [Fact]
    public async Task Login_WhenEnteredEmptyPassword_ShouldThrowArgumentException()
	{
		// Arrange

		var userDto = new UserDto.Request("ramirobrito@gmail.com", "");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Login(userDto)
        );

        Assert.Equal("Debe ingresar correctamente los datos", exception.Message);
        await _authRepository.Received(0).GetByEmail(Arg.Any<string>());
    }
    [Fact]
    public async Task Register_WhenTheFormIsCompleteCorrectly_ThenShouldCreateUserAndEmployee()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        _authRepository.Register(Arg.Any<User>())
            .Returns(callInfo => callInfo.Arg<User>());

        //Act
        var result = await _authService.Register(userDto);

        //Assert
        await _authRepository.Received(1).Register(Arg.Any<User>());
        Assert.NotNull(result);
        Assert.Equal(userDto.email, result.email);
        Assert.Equal(userDto.cuil, result.cuil);
        Assert.Equal(userDto.licence, result.licence);
        Assert.Equal(userDto.typeEmployee, result.typeEmployee);
    }
    [Fact]
    public async Task Register_WhenUserIsRegistering_ThenThePasswordShouldBeHashed()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        User? userCaptured = null;

        _authRepository.Register(Arg.Do<User>(u => userCaptured = u))
            .Returns(callInfo => callInfo.Arg<User>());

        //Act
        var result = await _authService.Register(userDto);

        //Assert
        await _authRepository.Received(1).Register(Arg.Any<User>());
        Assert.NotNull(userCaptured);
        Assert.NotEqual(userDto.password, userCaptured.Password);
        Assert.True(BCrypt.Net.BCrypt.Verify(userDto.password, userCaptured.Password));
    }
    [Fact]
    public async Task Register_WhenPasswordsDontMatching_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo1",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("Las contraseñas no coinciden.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenEmailIsOmitted_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Email' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenEmailIsWhiteSpace_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "   ",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Email' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenEmailIsNull_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: null!,
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Email' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenPasswordIsOmitted_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Contraseña' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenPasswordIsWhiteSpace_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "   ",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Contraseña' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenPasswordIsNull_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: null!,
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Contraseña' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenConfirmPasswordIsOmitted_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Confirmacion' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenConfirmPasswordIsWhiteSpace_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "   ",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Confirmacion' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenConfirmPasswordIsNull_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: null!,
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Confirmacion' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenNameIsOmitted_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Nombre' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenNameIsWhiteSpace_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "   ",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Nombre' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenNameIsNull_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: null!,
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Nombre' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenLastNameIsOmitted_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Apellido' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenLastNameIsWhiteSpace_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "   ",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Apellido' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenLastNameIsNull_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: null!,
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Apellido' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenCuilIsNotValid_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "2048556782",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("CUIL con formato inválido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenCuilIsOmitted_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Cuil' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenCuilIsWhiteSpace_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "   ",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Cuil' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenCuilIsNull_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: null!,
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Cuil' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenLicenceIsOmitted_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Licencia' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenLicenceIsWhiteSpace_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "   ",
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Licencia' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenLicenceIsNull_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: null!,
            phoneNumber: "381754963",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Licencia' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenPhoneIsOmitted_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Numero de Telefono' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenPhoneIsWhiteSpace_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "  ",
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Numero de Telefono' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenPhoneIsNull_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: null!,
            typeEmployee: "nurse"
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Numero de Telefono' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenEmployeeIsOmitted_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: ""
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Tipo de Empleado' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenEmployeeIsWhiteSpace_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: "   "
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Tipo de Empleado' no puede ser omitido.", exception.Message);
    }
    [Fact]
    public async Task Register_WhenEmployeeIsNull_ThenShouldArgumentException()
    {
        // Arrange
        var userDto = new UserDto.RequestRegister(
            email: "ramirobrito@gmail.com",
            password: "bocateamo",
            confirmPassword: "bocateamo",
            name: "Ramiro",
            lastName: "Brito",
            cuil: "20-45750673-8",
            licence: "LO78Q",
            phoneNumber: "381754963",
            typeEmployee: null!
            );

        //Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.Register(userDto)
        );

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("El campo 'Tipo de Empleado' no puede ser omitido.", exception.Message);
    }
}