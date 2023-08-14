using System;
using System.Diagnostics;

namespace HanoiTowers
{
    class Program
    {
        static void Main(string[] args)
        {
            bool programFinished = false;

            // Do-while loop that runs the program until the user enters "x".
            do
            {
                Console.Clear();

                // Reads the Hanoi Tower type from which the user chooses.
                HanoiType towerType = Functions.SelectHanoiTower();

                int numOfDiscs = Functions.ReadInput("discs");

                Console.Clear();

                // Display the type of Hanoi Tower and the number of discs being used.
                Console.WriteLine($"Running case: {towerType} with {numOfDiscs} discs:");

                // We are working with only four rods.
                int numRods = 4;

                // Create a variable of type IHanoiTower to store our instance later.
                IHanoiTower hanoiTower = null;

                // Start the stopwatch.
                Stopwatch sw = Stopwatch.StartNew();

                // Use the Factory design pattern and store the instance of the selected tower.
                hanoiTower = HanoiTowerFactory.CreateHanoiTower(towerType, numOfDiscs, numRods);

                // Execute the method to calculate the shortest path.
                // Currently, we are not returning the actual path, but only the length and maximum RAM usage.
                short length = hanoiTower.ShortestPath(out long maxMemory);

                // Display the final result.
                Console.WriteLine();
                Console.WriteLine($"\n\n RESULT FOR {towerType} WITH {numOfDiscs} DISCS");
                Console.WriteLine($"Dimension: {numOfDiscs}\nSteps: {length}\nTime: {sw.Elapsed.TotalSeconds}\nMaximum memory: {maxMemory / 1000000} MB");

                // Ask the user if they want to continue.
                // If the user enters anything other than "exit," the program will continue execution.
                Console.Write("\n\nIf you wish to exit the program, type 'exit'; otherwise, the program will start again: ");
                if (Console.ReadLine() == "exit") { programFinished = true; }
            }
            while (!programFinished);
        }
    }
}
