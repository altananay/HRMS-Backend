namespace Domain.Entities
{
    public class Education
    {
        public string School { get; set; }
        public string Major { get; set; }
        public string? Grade { get; set; }
        public string[] Years { get; set; }
        public string? Graduate { get; set; }
        public string? Status { get; set; }
    }
}