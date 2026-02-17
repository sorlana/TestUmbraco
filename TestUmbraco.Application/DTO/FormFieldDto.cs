namespace TestUmbraco.Application.DTO
{
    public class FormFieldDto
    {
        public required string Label { get; set; }
        public required string Type { get; set; }
        public required string Placeholder { get; set; }
        public bool IsRequired { get; set; }
        public required string ValidationPattern { get; set; }
        public required string ErrorMessage { get; set; }
        public required string Name { get; set; }
        public required string Value { get; set; }
    }
}
