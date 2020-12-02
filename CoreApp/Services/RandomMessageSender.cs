using System;

namespace CoreApp.Services
{
    public class RandomMessageSender : IMessageSender
    {
        private readonly int _val;

        public RandomMessageSender()
        {
            _val = new Random().Next(0, 10000);
        }

        public string Send()
        {
            return $"Sent {_val}";
        }
    }
}
