using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exercise6.Menus
{
    public class Menu(
        Func<string> menuName,
        Func<MenuItem[]> menuItems,
        bool isMainMenu = false,
        Func<string>? extra = null
    )
    {
        private readonly Func<string> _menuName = menuName;

        private readonly Func<MenuItem[]> _items = menuItems;

        private readonly bool _isMainMenu = isMainMenu;

        private string currentInput = "";
        private string? _message;
        private readonly Func<string>? _extra = extra;

        public async Task<bool> ShowMenu()
        {
            int selectedIndex = 0;

            while (true)
            {
                MenuItem[] items = _items();
                ShowMainMenu(items, selectedIndex, _message, _extra?.Invoke());
                _message = null;

                if (HandleInput(Console.ReadKey(true), items, selectedIndex, out selectedIndex))
                {
                    if (selectedIndex == -1 || items == null)
                        return false;

                    MenuItem selected = items[selectedIndex];
                    Menu? submenu;

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
                        await submenu.ShowMenu();
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

                    if (selected.Submenu == null)
                        return false;
                }
            }
        }

        public void ShowMainMenu(
            MenuItem[] items,
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
                    int menuNumber = (i + 1) % items.Length;
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
            MenuItem[] items,
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
                        if (current < items.Length && current >= 0)
                            selectedIndex = (current + items.Length - 1) % items.Length;
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
