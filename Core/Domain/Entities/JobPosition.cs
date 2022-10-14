using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class JobPosition : BaseEntity
    {
        public string PositionName { get; set; }
    }
}