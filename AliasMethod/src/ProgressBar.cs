using System;
using System.Text;
using System.Threading;

namespace AliasMethod
{
    class ProgressBar : IDisposable
    {
        readonly StringBuilder Bar = new StringBuilder();
        int Percent = 0;
        readonly long TotalCount;

        public ProgressBar(long totalCount)
        {
            TotalCount = totalCount;
            Console.BackgroundColor = ConsoleColor.Blue;
        }

        public void Update(long count)
        {
            int updatePercent = (int)(count * 100 / TotalCount);
            if (updatePercent > Percent)
            {
                Percent = updatePercent;

                Bar.Clear();
                Bar.Append('[');
                Bar.Append('=', Math.Max(Percent / 2 - 1, 0));
                Bar.Append('|');
                Bar.Append(' ', 50 - Math.Max(Percent / 2, 1));
                Bar.Append($"]     {Percent} %     ");
                Console.Write($"\r{Bar.ToString()}");
            }
        }

        public void Dispose()
        {
            Bar.Clear();
            Thread.Sleep(250);
            Console.ResetColor();
            Console.WriteLine("\n...\n Done! \n...\n");
        }
    }
}
