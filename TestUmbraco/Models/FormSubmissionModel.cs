namespace TestUmbraco.Models
{
    /// <summary>
    /// Модель для привязки данных формы при отправке
    /// </summary>
    public class FormSubmissionModel
    {
        /// <summary>
        /// ID формы (formBuilderBlock)
        /// </summary>
        public int FormId { get; set; }

        /// <summary>
        /// Название формы
        /// </summary>
        public string FormTitle { get; set; } = string.Empty;

        /// <summary>
        /// Email получателя (из настроек формы)
        /// </summary>
        public string EmailRecipient { get; set; } = string.Empty;

        /// <summary>
        /// Тема письма (из настроек формы)
        /// </summary>
        public string EmailSubject { get; set; } = string.Empty;

        /// <summary>
        /// Сообщение об успехе (из настроек формы)
        /// </summary>
        public string SuccessMessage { get; set; } = string.Empty;

        /// <summary>
        /// URL логотипа для письма
        /// </summary>
        public string LogoMailUrl { get; set; } = string.Empty;

        /// <summary>
        /// Данные полей формы (ключ - название поля, значение - введенное значение)
        /// </summary>
        public Dictionary<string, string> Fields { get; set; }

        /// <summary>
        /// Токен reCAPTCHA для проверки
        /// </summary>
        public string? ReCaptchaToken { get; set; }

        /// <summary>
        /// Конструктор, инициализирующий Fields пустым словарем
        /// </summary>
        public FormSubmissionModel()
        {
            Fields = new Dictionary<string, string>();
        }
    }
}
