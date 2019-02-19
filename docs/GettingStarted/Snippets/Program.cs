using System;
using System.Linq;
using System.CommandLine.DragonFruit;

namespace Snippets
{
    partial class Program
    {{
        static void Main(
            string region,
            string project,
            string session)
        {
            switch (region)
            {
                case "run1":
                    Run1();
                    break;
                case "run2":
                    Run2();
                    break;
            }
        }

        public static void Run1()
        {
            #region run1
            Console.WriteLine(DateTime.Now);
            #endregion
        }

        public static void Run2()
        {
            #region run2
            Console.WriteLine(Guid.NewGuid());
            #endregion
        }

        public static void Run3()
        {
            #region run3
            // something something
            #endregion
        }
    }
}



