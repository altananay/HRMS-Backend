using Application.Features.Contacts.Commands;
using Application.Features.Cvs.Commands;
using Application.Features.EmployerAuth.Commands;
using Application.Features.Employers.Commands;
using Application.Features.JobAdvertisements.Commands;
using Application.Features.JobSeekers.Commands;
using Application.Features.SystemStaffs.Commands;
using Application.Utilities.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Objects;

namespace Application.Mapping
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Contact, CreateContactCommand>().ReverseMap();
            CreateMap<Contact, UpdateContactCommand>().ReverseMap();
            CreateMap<Employer, UpdateEmployerCommand>().ReverseMap();
            CreateMap<Employer, EmployerRegisterCommand>().ReverseMap();
            CreateMap<JobAdvertisement, CreateJobAdvertisementCommand>().ReverseMap();
            CreateMap<JobAdvertisement, UpdateJobAdvertisementCommand>().ReverseMap();
            CreateMap<SystemStaff, CreateSystemStaffCommand>().ReverseMap();
            CreateMap<SystemStaff, UpdateSystemStaffCommand>().ReverseMap();
            CreateMap<JobSeeker, CreateJobSeekerCommand>().ReverseMap();
            CreateMap<Project, CreateProjectDto>().ReverseMap();
            CreateMap<JobExperience, CreateJobExperienceDto>().ReverseMap();
            CreateMap<Cv, CreateCvCommand>().ReverseMap();
            CreateMap<Cv, UpdateCvCommand>().ReverseMap();
        }
    }
}