using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exercise9.UI.Menus
{
    public class Menu<T>(
        Func<string> menuName,
        Func<MenuItem<T>[]> menuItems,
        bool isMainMenu = false,
        Func<string>? extra = null,
        int trailingItemsToPreserve = 0
    )
        where T : class
    {
        private readonly Func<string> _menuName = menuName;

        private readonly Func<MenuItem<T>[]> _items = menuItems;

        private readonly bool _isMainMenu = isMainMenu;
        private readonly int _trailingItemsToPreserve = trailingItemsToPreserve;

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
            int? previousItemCount = null;

            while (true)
            {
                MenuItem<T>[] items = _items();
                if (items.Length == 0)
                    return null;

                if (_trailingItemsToPreserve > 0 && previousItemCount.HasValue)
                    selectedIndex = CalculateSelectedIndex(
                        selectedIndex,
                        previousItemCount.Value,
                        items,
                        _trailingItemsToPreserve
                    );
                previousItemCount = items.Length;

                if (
                    selectedIndex < 0
                    || selectedIndex >= items.Length
                    || !items[selectedIndex].IsEnabled
                )
                    selectedIndex = FindEnabledIndex(
                        items,
                        Math.Clamp(selectedIndex, 0, items.Length - 1),
                        1
                    );
                if (selectedIndex < 0)
                    return null;

                ShowMainMenu(items, selectedIndex, _message, _extra?.Invoke());
                _message = null;

                if (HandleInput(Console.ReadKey(true), items, selectedIndex, out selectedIndex))
                {
                    if (selectedIndex == -1)
                        return null;

                    MenuItem<T> selected = items[selectedIndex];
                    if (!selected.IsEnabled)
                    {
                        _message = "This menu option is currently unavailable.";
                        continue;
                    }

                    Menu<T>? submenu = selected.Submenu;

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
                        continue;
                    }

                    if (selected.TaskAction != null)
                    {
                        _message = await selected.TaskAction();
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
                    PrintMenuItem(menuNumber, items[i].Text, selected, items[i].IsEnabled);
                }

            Console.Write(currentInput);
        }

        static void PrintMenuItem(int number, string text, bool selected, bool isEnabled)
        {
            if (!isEnabled)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            else if (selected)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
                Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write($"{number}.[{(selected ? "#" : " ")}] ");
            if (selected)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static int CalculateSelectedIndex(
            int selectedIndex,
            int previousItemCount,
            MenuItem<T>[] items,
            int trailingItemsToPreserve
        )
        {
            if (
                previousItemCount < trailingItemsToPreserve
                || items.Length < trailingItemsToPreserve
            )
                return Math.Clamp(selectedIndex, 0, items.Length - 1);

            int previousOptionCount = previousItemCount - trailingItemsToPreserve;
            int currentOptionCount = items.Length - trailingItemsToPreserve;
            if (selectedIndex < previousOptionCount)
                return currentOptionCount > 0
                    ? Math.Min(selectedIndex, currentOptionCount - 1)
                    : 0;

            int positionFromEnd = previousItemCount - selectedIndex;
            int newIndex = items.Length - positionFromEnd;
            if (
                trailingItemsToPreserve >= 3
                && positionFromEnd == 2
                && !items[newIndex].IsEnabled
                && items[newIndex - 1].IsEnabled
            )
                return newIndex - 1;

            return newIndex;
        }

        private static int FindEnabledIndex(MenuItem<T>[] items, int startIndex, int direction)
        {
            for (int step = 0; step < items.Length; step++)
            {
                int index =
                    ((startIndex + direction * step) % items.Length + items.Length) % items.Length;
                if (items[index].IsEnabled)
                    return index;
            }

            return -1;
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
                    selectedIndex = FindEnabledIndex(items, currentIndex + 1, 1);
                    currentInput = "";
                    return false;
                case ConsoleKey.UpArrow:
                    selectedIndex = FindEnabledIndex(items, currentIndex - 1, -1);
                    currentInput = "";
                    return false;
                case ConsoleKey.Backspace:
                    if (!string.IsNullOrEmpty(currentInput))
                        currentInput = currentInput[..^1];
                    return false;

                case ConsoleKey.Enter:
                    if (currentInput == "0")
                    {
                        if (!_isMainMenu)
                        {
                            selectedIndex = -1;
                            return true;
                        }

                        int lastIndex = items.Length - 1;
                        if (items[lastIndex].IsEnabled)
                        {
                            selectedIndex = lastIndex;
                            currentInput = "";
                            return true;
                        }

                        currentInput = "";
                        return false;
                    }

                    if (currentInput != "")
                    {
                        if (
                            int.TryParse(currentInput, out int currentNumber)
                            && currentNumber >= 1
                            && currentNumber <= items.Length
                            && items[currentNumber - 1].IsEnabled
                        )
                        {
                            selectedIndex = currentNumber - 1;
                            currentInput = "";
                            return true;
                        }

                        currentInput = "";
                        return false;
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
                            int lastIndex = items.Length - 1;
                            if (_isMainMenu && items[lastIndex].IsEnabled)
                                selectedIndex = lastIndex;
                        }
                        else if (current > 0 && current <= items.Length)
                        {
                            if (items[current - 1].IsEnabled)
                                selectedIndex = current - 1;
                        }
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
