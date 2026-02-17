namespace TestUmbraco.Application.DTO
{
    public class FormSubmissionDto
    {
        public int FormId { get; set; }
        public required string FormTitle { get; set; }
        public DateTime SubmittedAt { get; set; }
        public required string IpAddress { get; set; }
        public Dictionary<string, string> FieldValues { get; set; }
        public string LogoUrl { get; set; }

        public FormSubmissionDto()
        {
            FieldValues = new Dictionary<string, string>();
            SubmittedAt = DateTime.UtcNow;
            LogoUrl = string.Empty;
            FormTitle = string.Empty;
            IpAddress = string.Empty;
        }
    }
}
