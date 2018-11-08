using System;

namespace MLS.Project.Generators
{
    public static  class TextGenerator
    {
        private static readonly Random RandomGenerator = new Random();

        public static char GetLowerCaseLetter()
        {

            var num = RandomGenerator.Next(0, 26); // Zero to 25
            var let = (char)('a' + num);
            return @let;
        }
    }
}