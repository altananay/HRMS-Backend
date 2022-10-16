using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class EmployerForUpdateDto : IDto
    {
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string WebSite { get; set; }
        public string Email { get; set; }
    }
}