namespace SyncFiler.Helppers
{
    public static class HelloSquare
    {
        public static void SayHello(string title)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            int squareWidth = 20;
            int squareHeight = 4;

            string message = title;
            int messageLength = message.Length;

            // Get the console window width
            int consoleWidth = Console.WindowWidth;

            // Calculate horizontal padding for centering
            int horizontalPadding = (squareWidth - 2 - messageLength) / 2;
            int totalHorizontalPadding = (consoleWidth - squareWidth) / 2;

            // Print the box
            Console.WriteLine(new string(' ', totalHorizontalPadding) + new string('*', squareWidth));

            for (int i = 0; i < (squareHeight - 2) / 2; i++)
            {
                Console.WriteLine(new string(' ', totalHorizontalPadding) + '*' + new string(' ', squareWidth - 2) + '*');
            }

            Console.WriteLine(new string(' ', totalHorizontalPadding) + '*' + new string(' ', horizontalPadding) + message + new string(' ', horizontalPadding) + '*');

            for (int i = 0; i < squareHeight - 2 - (squareHeight - 2) / 2; i++)
            {
                Console.WriteLine(new string(' ', totalHorizontalPadding) + '*' + new string(' ', squareWidth - 2) + '*');
            }

            Console.WriteLine(new string(' ', totalHorizontalPadding) + new string('*', squareWidth));

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
