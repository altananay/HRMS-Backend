using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IDeleteRepository<T> : IRepository<T> where T : BaseEntity
    {
        Task<bool> Delete(T entity);
    }
}