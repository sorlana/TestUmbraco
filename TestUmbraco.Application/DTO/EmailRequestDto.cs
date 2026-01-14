namespace TestUmbraco.Application.DTO
{
    public class EmailRequestDto
    {
        public required string Name { get; set; }
        public required string Phone { get; set; }
        public required string Email { get; set; }
        public required string Comment { get; set; }
    }
}