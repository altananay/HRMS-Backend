using Application.Context;
using Application.Dtos;
using Application.Repositories;
using Domain.Entities;
using MongoDB.Driver;

namespace Persistence.Repositories
{
    public class EmployerReadRepository : ReadRepository<Employer>, IEmployerReadRepository
    {
        private readonly IMongoContext _mongoContext;
        private readonly string _collection;


        public EmployerReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
            _mongoContext = mongoContext;
            _collection = typeof(Employer).Name.ToLowerInvariant() + "s";
        }

        public IMongoCollection<Employer> collection => _mongoContext.database.GetCollection<Employer>(_collection);

        public GetEmployerDto GetByEmployerIdWithFields(string id)
        {
            var result = (from employer in collection.AsQueryable()
            where employer.Id == id
            select new GetEmployerDto { Id=employer.Id, NumberOfEmployees = employer.NumberOfEmployees, CompanyName = employer.CompanyName, CompanyPhone = employer.CompanyPhone, Departments = employer.Departments, Description = employer.Description, Sector = employer.Sector, WebSite = employer.WebSite, Email = employer.Email }).FirstOrDefault();
            return result;
        }

        public IQueryable<GetAllEmployerDto> GetAllEmployer()
        {
            var result = from employer in collection.AsQueryable()
                         select new GetAllEmployerDto { Id = employer.Id, NumberOfEmployees = employer.NumberOfEmployees, CompanyName = employer.CompanyName, CompanyPhone = employer.CompanyPhone, Departments = employer.Departments, Description = employer.Description, Sectors = employer.Sector, Email = employer.Email, WebSite = employer.WebSite };
            return result;
        }
    }
}