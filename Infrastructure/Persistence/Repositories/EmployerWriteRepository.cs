using Application.Context;
using Application.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class EmployerWriteRepository : WriteRepository<Employer>, IEmployerWriteRepository
    {
        public EmployerWriteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}