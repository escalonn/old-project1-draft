using System;

namespace PizzaStore.Library.Models
{
    public abstract class TimeProvider
    {
        private static TimeProvider s_current = DefaultTimeProvider.Instance;

        public static TimeProvider Current
        {
            get => s_current;
            set => s_current = value ?? throw new ArgumentNullException(nameof(value));
        }

        public abstract DateTime UtcNow { get; }

        public static void ResetToDefault() => s_current = DefaultTimeProvider.Instance;
    }
}
