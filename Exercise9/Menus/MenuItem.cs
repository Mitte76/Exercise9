using System;
using System.Threading.Tasks;

namespace Exercise9.Menus
{
    public class MenuItem<T>(
        string text,
        T? value = null,
        Menu<T>? submenu = null,
        Func<string?>? action = null,
        Func<Task<string?>>? taskAction = null,
        Func<T?>? valueFactory = null,
        bool isEnabled = true
    )
        where T : class
    {
        public string Text { get; } = text;
        public T? Value { get; } = value;
        public Menu<T>? Submenu { get; } = submenu;
        public Func<T?>? ValueFactory { get; } = valueFactory;
        public Func<string?>? Action { get; } = action;
        public Func<Task<string?>>? TaskAction { get; } = taskAction;
        public bool IsEnabled { get; } = isEnabled;
    }
}
