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
        bool backUpWhenDone = false,
        Func<Menu<T>?>? submenuFactory = null,
        Func<T?>? valueFactory = null
    )
        where T : class
    {
        public string Text { get; } = text;
        public T? Value { get; } = value;
        public Menu<T>? Submenu { get; } = submenu;
        public Func<Menu<T>?>? SubmenuFactory { get; } = submenuFactory;
        public Func<T?>? ValueFactory { get; } = valueFactory;
        public Func<string?>? Action { get; } = action;
        public Func<Task<string?>>? TaskAction { get; } = taskAction;
        public bool BackOutWhenDone { get; } = backUpWhenDone;
    }
}
