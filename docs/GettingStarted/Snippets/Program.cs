using System;

namespace Snippets
{
    public class Program
    {
        static void Main(
            string region = null,
            string session = null,
            string package = null,
            string project = null,
            string[] args = null)
        {
            switch (region)
            {
                case "run":
                    Run();
                    break;
                case "run1":
                    Run1();
                    break;
                case "run2":
                    Run2();
                    break;
                case "run3":
                    Run3();
                    break;
            }
        }

        public static void Run()
        {
            #region run
            Console.WriteLine("Hello World!");
            #endregion
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
            #endregion
        }
    }
}