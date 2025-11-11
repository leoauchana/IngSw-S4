using IngSw_Application.DTOs;
using IngSw_Application.Exceptions;
using IngSw_Application.Services;
using IngSw_Domain.Entities;
using IngSw_Domain.Interfaces;
using IngSw_Domain.ValueObjects;
using NSubstitute;
using System.Data.Common;

namespace IngSw_Tests.RegisterPatient;

public class PatientsServiceTest
{
    private readonly IPatientRepository _patientsRepository;
    private readonly PatientsService _patientsService;
    private readonly ISocialWorkServiceApi _socialWorkServiceApi;
    public PatientsServiceTest()
    {
        _patientsRepository = Substitute.For<IPatientRepository>();
        _socialWorkServiceApi = Substitute.For<ISocialWorkServiceApi>();
        _patientsService = new PatientsService(_patientsRepository, _socialWorkServiceApi);
    }
    [Fact]
    public async Task AddPatient_WhenTheHealthcareSystemExistsWithSocialWorkExisting_ShouldCreateThePatient()
    {
        // Arrange
        var patientDto = new PatientDto.Request(
            cuilPatient: "20-45750673-8",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: "OSPE",
            affiliateNumber: "4798540152"
        );
        _socialWorkServiceApi.ExistingSocialWork("OSPE")
            .Returns(Task.FromResult(true));
        _socialWorkServiceApi.IsAffiliated("4798540152")
        .Returns(Task.FromResult(true));
        // Act
        var result = await _patientsService.AddPatient(patientDto);

        // Assert
        await _patientsRepository.Received(1).AddPatient(Arg.Any<Patient>());
        await _socialWorkServiceApi.Received(1).ExistingSocialWork(Arg.Any<string>());
        await _socialWorkServiceApi.Received(1).IsAffiliated(Arg.Any<string>());
        Assert.NotNull(result);
        Assert.Equal(patientDto.cuilPatient, result.cuilPatient);
        Assert.Equal(patientDto.namePatient, result.namePatient);
        Assert.Equal(patientDto.lastNamePatient, result.lastNamePatient);
    }
    [Fact]
    public async Task AddPatient_WhenTheHealthcareSystemExistsWithoutSocialWork_ShouldCreateThePatient()
    {
        // Arrange
        var patientDto = new PatientDto.Request(
            cuilPatient: "20-45750673-8",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: null,
            affiliateNumber: null
        );

        // Act
        var result = await _patientsService.AddPatient(patientDto);

        // Assert
        await _patientsRepository.Received(1).AddPatient(Arg.Any<Patient>());
        Assert.NotNull(result);
        Assert.Equal(patientDto.cuilPatient, result.cuilPatient);
        Assert.Equal(patientDto.namePatient, result.namePatient);
        Assert.Equal(patientDto.lastNamePatient, result.lastNamePatient);
    }
    [Fact]
    public async Task AddPatient_WhenTheHealthcareSystemExistsWithSocialWorkInexisting_ShouldNotCreateThePatient()
    {
        // Arrange
        var patientDto = new PatientDto.Request(
            cuilPatient: "20-45750673-8",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: "Subsidio",
            affiliateNumber: "4798540152"
        );
        _socialWorkServiceApi.ExistingSocialWork("Subsidio")
            .Returns(Task.FromResult(false));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessConflictException>(
            () => _patientsService.AddPatient(patientDto)
        );
        Assert.Equal("La obra social no existe, por lo tanto no se puede registrar al paciente.", exception.Message);
        await _socialWorkServiceApi.Received(1).ExistingSocialWork(Arg.Any<string>());
        await _socialWorkServiceApi.Received(0).IsAffiliated(Arg.Any<string>());
        await _patientsRepository.Received(0).AddPatient(Arg.Any<Patient>());
    }
    [Fact]
    public async Task AddPatient_WhenTheHealthcareSystemExistsWithSocialWorkExistingButWitouthAffiliation_ShouldNotCreateThePatient()
    {
        // Arrange
        var patientDto = new PatientDto.Request(
            cuilPatient: "20-45750673-8",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: "Subsidio",
            affiliateNumber: "4798540152"
        );
        _socialWorkServiceApi.ExistingSocialWork("Subsidio")
            .Returns(Task.FromResult(true));
        _socialWorkServiceApi.IsAffiliated("4798540152")
            .Returns(Task.FromResult(false));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessConflictException>(
            () => _patientsService.AddPatient(patientDto)
        );
        Assert.Equal("El paciente no es afiliado de la obra social, por lo tanto no se puede registrar al paciente.", exception.Message);
        await _socialWorkServiceApi.Received(1).ExistingSocialWork(Arg.Any<string>());
        await _socialWorkServiceApi.Received(1).IsAffiliated(Arg.Any<string>());
        await _patientsRepository.Received(0).AddPatient(Arg.Any<Patient>());
    }
    [Fact]
    public async Task AddPatient_WhenSocialWorkOrAffiliateNumberIsMissing_ShouldThrowArgumentException()
    {
        // Arrange

        // Caso 1: Falta el número de afiliado, pero se indica la obra social
        var patientDtoMissingAffiliate = new PatientDto.Request(
            cuilPatient: "20-45750673-8",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: "Subsidio",
            affiliateNumber: null
        );
        // Caso 2: Falta la obra social, pero se indica el número de afiliado
        var patientDtoMissingSocialWork = new PatientDto.Request(
            cuilPatient: "20-45750673-8",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: null,
            affiliateNumber: "4798540152"
        );

        // Act & Assert

        var exception1 = await Assert.ThrowsAsync<ArgumentException>(
            () => _patientsService.AddPatient(patientDtoMissingAffiliate)
        );
        Assert.Equal("Si se ingresa la obra social, también debe ingresarse el número de afiliado (y viceversa).", exception1.Message);

        var exception2 = await Assert.ThrowsAsync<ArgumentException>(
            () => _patientsService.AddPatient(patientDtoMissingSocialWork)
        );
        Assert.Equal("Si se ingresa la obra social, también debe ingresarse el número de afiliado (y viceversa).", exception2.Message);

        await _socialWorkServiceApi.Received(0).ExistingSocialWork(Arg.Any<string>());
        await _socialWorkServiceApi.Received(0).IsAffiliated(Arg.Any<string>());
        await _patientsRepository.Received(0).AddPatient(Arg.Any<Patient>());
    }
    [Fact]
    public async Task AddPatient_WhenCuilIsNotValid_ThenShouldThrowExceptionAndNotCreateThePatient()
    {
        // Arrange
        var patientDto = new PatientDto.Request(
            cuilPatient: "45750673",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: "OSPE",
            affiliateNumber: "4798540152"
        );
        _socialWorkServiceApi.ExistingSocialWork("OSPE")
        .Returns(Task.FromResult(true));
        _socialWorkServiceApi.IsAffiliated("4798540152")
        .Returns(Task.FromResult(true));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _patientsService.AddPatient(patientDto)
        );

        Assert.Equal("CUIL con formato inválido.", exception.Message);

        await _patientsRepository.DidNotReceive().AddPatient(Arg.Any<Patient>());
    }
    [Fact]
    public async Task AddPatient_WhenCuilIsNull_ThenShouldThrowExceptionAndNotCreateThePatient()
    {
        // Arrange
        var patientDto = new PatientDto.Request(
            cuilPatient: null!,
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: "OSPE",
            affiliateNumber: "4798540152"
        );
        _socialWorkServiceApi.ExistingSocialWork("OSPE")
        .Returns(Task.FromResult(true));
        _socialWorkServiceApi.IsAffiliated("4798540152")
        .Returns(Task.FromResult(true));
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _patientsService.AddPatient(patientDto)
        );

        Assert.Equal("CUIL no puede ser vacío.", exception.Message);

        await _patientsRepository.DidNotReceive().AddPatient(Arg.Any<Patient>());
    }
    [Fact]
    public async Task AddPatient_WhenCuilIsWhiteSpace_ThenShouldThrowExceptionAndNotCreateThePatient()
    {
        // Arrange
        var patientDto = new PatientDto.Request(
            cuilPatient: "   ",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: "OSPE",
            affiliateNumber: "4798540152"
        );
        _socialWorkServiceApi.ExistingSocialWork("OSPE")
        .Returns(Task.FromResult(true));
        _socialWorkServiceApi.IsAffiliated("4798540152")
        .Returns(Task.FromResult(true));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _patientsService.AddPatient(patientDto)
        );
        Assert.Equal("CUIL no puede ser vacío.", exception.Message);
        await _patientsRepository.DidNotReceive().AddPatient(Arg.Any<Patient>());
    }
    [Fact]
    public async Task AddPatient_WhenPatientAlreadyExists_ThenShouldThrowExceptionAndNotCreateThePatient()
    {
        // Arrange
        var patientDto = new PatientDto.Request(
            cuilPatient: "20-45750673-8",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA",
            nameSocialWork: "OSPE",
            affiliateNumber: "4798540152"
        );

        var existingPatient = new Patient
        {
            Cuil = Cuil.Create("20-45750673-8"),
            Name = "Lautaro",
            LastName = "Lopez",
            Email = "lautalopez@gmail.com"
        };

        _patientsRepository.GetByCuil(patientDto.cuilPatient)!.Returns(Task.FromResult(new List<Patient> { existingPatient }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessConflictException>(
            () => _patientsService.AddPatient(patientDto)
        );
        Assert.Equal($"El paciente de cuil {patientDto.cuilPatient} ya se encuentra registrado", exception.Message);
        await _patientsRepository.DidNotReceive().AddPatient(Arg.Any<Patient>());
    }
    [Fact]
    public async Task GetByCuil_WhenPatientsExistWithMatchingCuil_ShouldReturnAllMatchingPatients()
    {
        // Arrange
        string cuilReceived = "20-45758";

        var patientsFromRepository = new List<Patient>
        {
            new Patient
            {
                Cuil = Cuil.Create("20-45758331-8"),
                Name = "Lautaro",
                LastName = "Lopez",
                Email = "lautalopez@gmail.com",
                Domicilie = new Domicilie
                {
                    Number = 324,
                    Street = "Jujuy",
                    Locality = "San Miguel"
                }
            },
            new Patient
            {
                Cuil = Cuil.Create("20-43758621-4"),
                Name = "Lucia",
                LastName = "Perez",
                Email = "luciaperez@gmail.com",
                Domicilie = new Domicilie
                {
                    Number = 356,
                    Street = "Avenue Nine Of July",
                    Locality = "CABA"
                },
            }
        };

        _patientsRepository.GetByCuil(cuilReceived)
               .Returns(Task.FromResult<List<Patient>?>(patientsFromRepository));

        // Act
        var patientsFound = await _patientsService.GetByCuil(cuilReceived);

        // Assert
        await _patientsRepository.Received(1).GetByCuil(cuilReceived);

        Assert.NotNull(patientsFound);
        Assert.Equal(2, patientsFound.Count);
        // Comprobamos propiedades de los DTOs resultantes
        Assert.Equal(patientsFromRepository[0].Cuil.Value, patientsFound[0].cuilPatient);
        Assert.Equal(patientsFromRepository[1].Cuil.Value, patientsFound[1].cuilPatient);
        Assert.Equal(patientsFromRepository[0].Name, patientsFound[0].namePatient);
        Assert.Equal(patientsFromRepository[1].Name, patientsFound[1].namePatient);
    }
    [Fact]
    public async Task GetByCuil_WhenNoPatientsFound_ShouldThrowNullException()
    {
        // Arrange
        string cuilReceived = "20-45758";
        //Act & Arrange
        var exception = await Assert.ThrowsAsync<NullException>(
            () => _patientsService.GetByCuil(cuilReceived)
            );
        Assert.Equal($"No hay pacientes que coincidan con el cuil {cuilReceived} registrados.", exception.Message);
    }
}
