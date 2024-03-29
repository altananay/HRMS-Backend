﻿using Domain.Common;

namespace Domain.Entities
{
    public class JobApplication : BaseEntity
    {
        public string? JobAdvertisementId { get; set; }   
        public string? EmployerId { get; set; }
        public string? JobSeekerId { get; set; }
        public string JobSeekerDescription { get; set; }
        public string EmployerDescription { get; set; }
        public string Result { get; set; }
    }
}