namespace CoreApp.Services
{
    /// <summary>
    /// Тестовый класс для примера передачи зависимоти через конструктор
    /// </summary>
    public class ServiceUsingExample
    {
        private readonly IMessageSender _sender;

        //Зависимость IMesageService передается через конструктор класса
        public ServiceUsingExample(IMessageSender sender)
        {
            _sender = sender;
        }

        public string Send() => _sender.Send();
    }
}
