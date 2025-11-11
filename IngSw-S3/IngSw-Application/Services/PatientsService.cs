using IngSw_Application.DTOs;
using IngSw_Application.Exceptions;
using IngSw_Domain.Entities;
using IngSw_Domain.Interfaces;
using IngSw_Domain.ValueObjects;

namespace IngSw_Application.Services;

public class PatientsService
{
    private readonly IPatientRepository _patientRepository;
    private readonly ISocialWorkServiceApi _socialWorkServiceApi;
    public PatientsService(IPatientRepository patientRepository, ISocialWorkServiceApi socialWorkServiceApi)
    {
        _patientRepository = patientRepository;
        _socialWorkServiceApi = socialWorkServiceApi;
    }

    public async Task<PatientDto.Response?> AddPatient(PatientDto.Request patientData)
    {
        var patientFound = await _patientRepository.GetByCuil(patientData.cuilPatient);
        if (patientFound != null)
            throw new BusinessConflictException($"El paciente de cuil {patientData.cuilPatient} ya se encuentra registrado");
        Affiliate? affiliation = null;
        bool oneCompleted = string.IsNullOrEmpty(patientData.nameSocialWork) != string.IsNullOrEmpty(patientData.affiliateNumber);
        if (oneCompleted)
        {
            throw new ArgumentException("Si se ingresa la obra social, también debe ingresarse el número de afiliado (y viceversa).");
        }
        if (!string.IsNullOrEmpty(patientData.nameSocialWork) && !string.IsNullOrEmpty(patientData.affiliateNumber))
        {
            if (!await _socialWorkServiceApi.ExistingSocialWork(patientData.nameSocialWork))
                throw new BusinessConflictException("La obra social no existe, por lo tanto no se puede registrar al paciente.");
            if (!await _socialWorkServiceApi.IsAffiliated(patientData.affiliateNumber))
                throw new BusinessConflictException("El paciente no es afiliado de la obra social, por lo tanto no se puede registrar al paciente.");
            var socialWork = new SocialWork { Id = Guid.NewGuid(), Name = patientData.nameSocialWork };
            affiliation = new Affiliate { Id = Guid.NewGuid(), SocialWork = socialWork, AffiliateNumber = patientData.affiliateNumber };
        }
        var newPatient = new Patient
        {
            Cuil = Cuil.Create(patientData.cuilPatient),
            Name = patientData.namePatient,
            LastName = patientData.lastNamePatient,
            Email = patientData.email,
            Domicilie = new Domicilie
            {
                Number = patientData.numberDomicilie,
                Street = patientData.streetDomicilie,
                Locality = patientData.localityDomicilie
            },
            Affiliate = affiliation
        };
        await _patientRepository.AddPatient(newPatient);
        return new PatientDto.Response(newPatient.Cuil.Value!, newPatient.Name!, newPatient.LastName!, newPatient.Email!,
                    newPatient.Domicilie!.Street!, newPatient.Domicilie.Number, newPatient.Domicilie.Locality!);
    }

    public async Task<List<PatientDto.Response>?> GetByCuil(string cuilPatient)
    {
        //var cuilValid = Cuil.Create(cuilPatient);
        var patientsFounds = await _patientRepository.GetByCuil(cuilPatient);
        if (patientsFounds == null || !(patientsFounds.Count > 0))
            throw new NullException($"No hay pacientes que coincidan con el cuil {cuilPatient} registrados.");
        return patientsFounds.Select(pr => new PatientDto.Response(pr.Cuil!.Value!, pr.Name!, pr.LastName!,
                    pr.Email!, pr.Domicilie!.Street!, pr.Domicilie.Number, pr.Domicilie.Locality!))
                    .ToList();
    }
}
