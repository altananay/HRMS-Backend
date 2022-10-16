using Application.Abstractions.Common;
using Application.Constants;
using Application.Dtos;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Security.Hashing;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Concretes.Common
{
    public class EmployerManager : IEmployerService
    {
        private readonly IEmployerReadRepository _employerReadRepository;
        private readonly IEmployerDeleteRepository _employerDeleteRepository;
        private readonly IEmployerWriteRepository _employerWriteRepository;

        public EmployerManager(IEmployerReadRepository employerReadRepository, IEmployerDeleteRepository employerDeleteRepository, IEmployerWriteRepository employerWriteRepository)
        {
            _employerReadRepository = employerReadRepository;
            _employerDeleteRepository = employerDeleteRepository;
            _employerWriteRepository = employerWriteRepository;
        }

        public IResult Add(EmployerForRegisterDto employer)
        {
            string[] claims = { "employer" };
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(employer.Password, out passwordHash, out passwordSalt);
            var employerEntity = new Employer
            {
                CompanyName = employer.CompanyName,
                CompanyPhone = employer.CompanyPhone,
                Email = employer.Email,
                WebSite = employer.WebSite,
                Status = true,
                Claims = claims,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            _employerWriteRepository.Add(employerEntity);
            return new SuccessResult(Messages.EmployerAdded);
        }

        public IResult Delete(string id)
        {
            try
            {
                var result = _employerDeleteRepository.Delete(id);
                return new SuccessResult(Messages.EmployerDeleted);
            }
            catch
            {
                return new ErrorResult(Messages.UnknownError);
            }
        }

        public IDataResult<IQueryable<Employer>> GetAll()
        {
            return new SuccessDataResult<IQueryable<Employer>>(_employerReadRepository.GetAll());
        }

        public IDataResult<Employer> GetByEmail(string email)
        {
            return new SuccessDataResult<Employer>(_employerReadRepository.Get(e => e.Email == email)); 
        }

        public IDataResult<Employer> GetById(string id)
        {
            return new SuccessDataResult<Employer>(_employerReadRepository.Get(e => e.Id == id));
        }

        public IResult Update(EmployerForUpdateDto employer)
        {
            var result = _employerReadRepository.Get(e => e.Email == employer.Email);
            var employerEntity = new Employer
            {
                Email = employer.Email,
                Claims = result.Claims,
                CompanyName = employer.CompanyName,
                CompanyPhone = employer.CompanyPhone,
                CreatedAt = result.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                WebSite = employer.WebSite,
                PasswordHash = result.PasswordHash,
                PasswordSalt = result.PasswordHash,
                Status = result.Status,
                Id = result.Id
            };
            var result2 = _employerWriteRepository.Update(employerEntity);
            return new SuccessResult(Messages.EmployerUpdated);
        }
    }
}