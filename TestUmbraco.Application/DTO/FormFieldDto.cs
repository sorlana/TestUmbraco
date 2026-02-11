namespace TestUmbraco.Application.DTO
{
    public class FormFieldDto
    {
        public string Label { get; set; }
        public string Type { get; set; }
        public string Placeholder { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationPattern { get; set; }
        public string ErrorMessage { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
