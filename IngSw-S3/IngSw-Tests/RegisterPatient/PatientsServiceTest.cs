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
    [Fact]
    public async Task AddPatient_WhenTheHealthcareSystemExists_ShouldCreateThePatient()
    {
        // Arrange
        var iPatientRepository = Substitute.For<IPatientRepository>();
        var patientsService = new PatientsService(iPatientRepository);

        var patientDto = new PatientDto.Request(
            cuilPatient: "20-45750673-8",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA"
        );

        /*iPatientRepository.GetByCuil(patientDto.cuilPatient)
        .Returns(Task.FromResult<List<Patient>?>(new List<Patient>()));*/

        // Act
        var result = await patientsService.AddPatient(patientDto);

        // Assert
        await iPatientRepository.Received(1).AddPatient(Arg.Any<Patient>());
        Assert.NotNull(result);
        Assert.Equal(patientDto.cuilPatient, result.cuilPatient);
        Assert.Equal(patientDto.namePatient, result.namePatient);
        Assert.Equal(patientDto.lastNamePatient, result.lastNamePatient);
    }

    [Fact]
    public async Task AddPatient_WhenCuilIsNotValid_ThenShouldThrowExceptionAndNotCreateThePatient()
    {
        // Arrange
        var iPatientRepository = Substitute.For<IPatientRepository>();
        var patientsService = new PatientsService(iPatientRepository);

        var patientDto = new PatientDto.Request(
            cuilPatient: "45750673",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => patientsService.AddPatient(patientDto)
        );

        Assert.Equal("CUIL con formato inválido.", exception.Message);

        await iPatientRepository.DidNotReceive().AddPatient(Arg.Any<Patient>());
    }

    [Fact]
    public async Task AddPatient_WhenCuilIsNull_ThenShouldThrowExceptionAndNotCreateThePatient()
    {
        // Arrange
        var iPatientRepository = Substitute.For<IPatientRepository>();
        var patientsService = new PatientsService(iPatientRepository);

        var patientDto = new PatientDto.Request(
            cuilPatient: null!,
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => patientsService.AddPatient(patientDto)
        );

        Assert.Equal("CUIL no puede ser vacío.", exception.Message);

        await iPatientRepository.DidNotReceive().AddPatient(Arg.Any<Patient>());
    }

    [Fact]
    public async Task AddPatient_WhenCuilIsWhiteSpace_ThenShouldThrowExceptionAndNotCreateThePatient()
    {
        // Arrange
        var iPatientRepository = Substitute.For<IPatientRepository>();
        var patientsService = new PatientsService(iPatientRepository);

        var patientDto = new PatientDto.Request(
            cuilPatient: "   ",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => patientsService.AddPatient(patientDto)
        );

        Assert.Equal("CUIL no puede ser vacío.", exception.Message);

        await iPatientRepository.DidNotReceive().AddPatient(Arg.Any<Patient>());
    }

    [Fact]
    public async Task AddPatient_WhenPatientAlreadyExists_ThenShouldThrowExceptionAndNotCreateThePatient()
    {
        // Arrange
        var iPatientRepository = Substitute.For<IPatientRepository>();
        var patientsService = new PatientsService(iPatientRepository);

        var patientDto = new PatientDto.Request(
            cuilPatient: "20-45750673-8",
            namePatient: "Lautaro",
            lastNamePatient: "Lopez",
            email: "lautalopez@gmail.com",
            streetDomicilie: "Avenue Nine Of July",
            numberDomicilie: 356,
            localityDomicilie: "CABA"
        );

        var existingPatient = new Patient
        {
            Cuil = Cuil.Create("20-45750673-8"),
            Name = "Lautaro",
            LastName = "Lopez",
            Email = "lautalopez@gmail.com"
        };

        iPatientRepository.GetByCuil(patientDto.cuilPatient)!.Returns(Task.FromResult(new List<Patient> { existingPatient }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessConflictException>(
            () => patientsService.AddPatient(patientDto)
        );

        Assert.Equal($"El paciente de cuil {patientDto.cuilPatient} ya se encuentra registrado", exception.Message);

        await iPatientRepository.DidNotReceive().AddPatient(Arg.Any<Patient>());
    }
}
