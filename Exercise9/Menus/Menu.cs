using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exercise9.Menus
{
    public class Menu<T>(
        Func<string> menuName,
        Func<MenuItem<T>[]> menuItems,
        bool isMainMenu = false,
        Func<string>? extra = null
    )
        where T : class
    {
        private readonly Func<string> _menuName = menuName;

        private readonly Func<MenuItem<T>[]> _items = menuItems;

        private readonly bool _isMainMenu = isMainMenu;

        private string currentInput = "";
        private string? _message;
        private readonly Func<string>? _extra = extra;

        public static Menu<T> CreateSelectionMenu(
            Func<string> menuName,
            IEnumerable<T> values,
            Func<T, string> getDisplayText,
            bool isMainMenu = false,
            Func<string>? extra = null,
            string backText = "Back"
        )
        {
            ArgumentNullException.ThrowIfNull(values);
            ArgumentNullException.ThrowIfNull(getDisplayText);

            MenuItem<T>[] items = values
                .Select(value => new MenuItem<T>(getDisplayText(value), value: value))
                .Append(new MenuItem<T>(backText))
                .ToArray();

            return new Menu<T>(menuName, () => items, isMainMenu, extra);
        }

        public async Task<T?> ShowMenu()
        {
            int selectedIndex = 0;

            while (true)
            {
                MenuItem<T>[] items = _items();
                if (items.Length == 0)
                    return null;

                ShowMainMenu(items, selectedIndex, _message, _extra?.Invoke());
                _message = null;

                if (HandleInput(Console.ReadKey(true), items, selectedIndex, out selectedIndex))
                {
                    if (selectedIndex == -1)
                        return null;

                    MenuItem<T> selected = items[selectedIndex];
                    Menu<T>? submenu;

                    if (selected.SubmenuFactory != null)
                    {
                        submenu = selected.SubmenuFactory();

                        if (submenu == null)
                        {
                            selectedIndex = 0;
                            continue;
                        }
                    }
                    else
                    {
                        submenu = selected.Submenu;
                    }

                    if (submenu != null)
                    {
                        T? result = await submenu.ShowMenu();
                        if (result != null)
                            return result;

                        selectedIndex = 0;
                        continue;
                    }

                    if (selected.Action != null)
                    {
                        _message = selected.Action.Invoke();
                        if (!selected.BackOutWhenDone)
                            continue;
                    }

                    if (selected.TaskAction != null)
                    {
                        _message = await selected.TaskAction();

                        if (!selected.BackOutWhenDone)
                            continue;
                    }

                    if (selected.ValueFactory != null)
                    {
                        T? value = selected.ValueFactory();
                        if (value != null)
                            return value;

                        _message = "Creation cancelled.";
                        selectedIndex = 0;
                        continue;
                    }

                    if (selected.Value != null)
                        return selected.Value;

                    if (selected.Submenu == null)
                        return null;
                }
            }
        }

        public void ShowMainMenu(
            MenuItem<T>[] items,
            int selectedIndex,
            string? message,
            string? extra
        )
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;

            string title = _menuName();
            string border = new('*', title.Length + 8);

            Console.WriteLine($"{border}\n*   {title}   *\n{border}");

            if (extra != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\n{extra}");
            }
            Console.ForegroundColor = ConsoleColor.Blue;

            if (message != null)
                Console.WriteLine($"{message}");
            else
                Console.WriteLine();

            Console.ResetColor();
            if (items != null)
                for (int i = 0; i < items.Length; i++)
                {
                    bool selected = i == selectedIndex;
                    int menuNumber = i + 1;
                    PrintMenuItem(menuNumber, items[i].Text, selected);
                }

            Console.Write(currentInput);
        }

        static void PrintMenuItem(int number, string text, bool selected)
        {
            Console.Write($"{number}.[");

            Console.ForegroundColor = selected ? ConsoleColor.Green : ConsoleColor.DarkGray;

            Console.Write(selected ? "#" : " ");

            Console.ResetColor();
            Console.WriteLine($"] {text}");
        }

        private bool HandleInput(
            ConsoleKeyInfo input,
            MenuItem<T>[] items,
            int currentIndex,
            out int selectedIndex
        )
        {
            selectedIndex = currentIndex;

            switch (input.Key)
            {
                case ConsoleKey.DownArrow:
                    selectedIndex++;
                    selectedIndex = (currentIndex + 1) % items.Length;
                    currentInput = "";
                    return false;
                case ConsoleKey.UpArrow:
                    selectedIndex--;
                    selectedIndex = (currentIndex - 1 + items.Length) % items.Length;
                    currentInput = "";

                    return false;
                case ConsoleKey.Backspace:
                    if (!string.IsNullOrEmpty(currentInput))
                        currentInput = currentInput[..^1];
                    return false;

                case ConsoleKey.Enter:
                    if (currentInput == "0" && !_isMainMenu)
                    {
                        selectedIndex = -1;
                        return true;
                    }

                    if (
                        currentInput != ""
                        && int.TryParse(currentInput, out int currentNumber)
                        && currentNumber >= 1
                        && currentNumber <= items.Length
                    )
                    {
                        selectedIndex = currentNumber - 1;
                        currentInput = "";
                    }

                    return true;
                case ConsoleKey.D0:
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                case ConsoleKey.D6:
                case ConsoleKey.D7:
                case ConsoleKey.D8:
                case ConsoleKey.D9:
                    currentInput += input.KeyChar;
                    if (int.TryParse(currentInput, out int current))
                    {
                        if (current == 0)
                        {
                            if (_isMainMenu)
                                selectedIndex = items.Length - 1;
                        }
                        else if (current > 0 && current <= items.Length)
                            selectedIndex = current - 1;
                        else
                        {
                            selectedIndex = 0;
                            currentInput = "";
                        }
                    }
                    return false;
                default:
                    Console.WriteLine("Incorrect input, please try again\n");
                    return false;
            }
        }
    }
}
