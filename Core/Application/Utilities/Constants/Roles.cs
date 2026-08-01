namespace Application.Utilities.Constants
{
    public static class Roles
    {
        public const string JobSeeker = "jobseeker";
        public const string Employer = "employer";
        public const string Admin = "admin";

        public const string EmployerOrAdmin = $"{Employer},{Admin}";
        public const string JobSeekerOrAdmin = $"{JobSeeker},{Admin}";
    }
}
