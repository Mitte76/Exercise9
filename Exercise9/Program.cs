using System.Collections.Generic;
using System.Threading.Tasks;
using Exercise6.Facilities;
using Exercise6.Menus;

namespace Exercise6
{
    internal class Program
    {
        private static readonly List<Garage> facilities = [];

        static async Task Main()
        {
            Menu menu = new MenuFactory(facilities).CreateMainMenu();
            await menu.ShowMenu();
        }
    }
}
