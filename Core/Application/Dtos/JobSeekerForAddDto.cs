using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class JobSeekerForAddDto : IDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Information { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}