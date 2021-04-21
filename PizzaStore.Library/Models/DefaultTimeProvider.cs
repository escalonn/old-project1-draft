using System;

namespace PizzaStore.Library.Models
{
    public class DefaultTimeProvider : TimeProvider
    {
        private static TimeProvider s_instance;

        private DefaultTimeProvider() { }

        public static TimeProvider Instance => s_instance ?? (s_instance = new DefaultTimeProvider());

        public override DateTime UtcNow => DateTime.UtcNow;
    }
}