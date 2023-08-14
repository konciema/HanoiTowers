using System;
using System.Collections.Generic;
using System.Text;

namespace HanoiTowers
{
    public static class Functions
    {
        /// <summary>
        /// Method for converting the state to a long representation.
        /// </summary>
        public static long StateToLong(byte[] state, int numOfRods)
        {
            long num = 0;
            long factor = 1;
            for (int i = state.Length - 1; i >= 0; i--)
            {
                num += state[i] * factor;
                factor *= numOfRods;
            }
            return num;
        }

        /// <summary>
        /// Method for converting a long representation to a state.
        /// </summary>
        public static byte[] LongToState(long num, int numDiscs, int numOfRods)
        {
            byte[] tmpState = new byte[numDiscs];
            for (int i = numDiscs - 1; i >= 0; i--)
            {
                tmpState[i] = (byte)(num % numOfRods);
                num /= numOfRods;
            }
            return tmpState;
        }

        /// <summary>
        /// Method for reading user input.
        /// <para><paramref name="inputType" /> - type of input expected from the user:
        /// <br>"tower" - for tower type selection/input</br>
        /// <br>"discs" - for the number of discs input</br>
        /// </para>
        /// </summary>
        /// <returns>Returns the user input as an integer.</returns>
        public static int ReadInput(string inputType)
        {
            // Variable to store the user input as an integer
            int selection = int.MinValue;

            // Boolean variable to check if the input is correct
            bool isRightInput = false;

            int maxDiscs = 30;

            // If we want to read tower type input
            if (inputType == "tower")
            {
                // Read the number of defined tower types
                int hanoiCount = Enum.GetNames(typeof(HanoiType)).Length;

                // Do-while loop that requires user input until it meets the specified parameters
                do
                {
                    Console.Write($"\nSelect the tower type: ");
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out selection))
                    {
                        if (selection < 0 || selection > hanoiCount - 1)
                        {
                            Console.WriteLine($"Tower not found. Input a number between 0 and {hanoiCount - 1}");
                        }
                        else
                        {
                            isRightInput = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("You didn't input a number. Please try again!");
                    }
                }
                while (!isRightInput);
            }

            // If we want to read the number of discs input
            else if (inputType == "discs")
            {
                // Do-while loop that requires user input until it meets the specified parameters
                do
                {
                    Console.Write($"\nSelect the number of discs: ");
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out selection))
                    {
                        if (selection <= 0 || selection > maxDiscs)
                        {
                            Console.WriteLine($"Discs number is not right. Input a number between 1 and 30");
                        }
                        else
                        {
                            isRightInput = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("You didn't input a number. Please try again!");
                    }
                }
                while (!isRightInput);
            }

            // Return the user's choice
            return selection;
        }

        /// Method that gives the user all Hanoi Types
        public static void WriteHanoiTypes()
        {
            Console.WriteLine("Different coloring types of Hanoi Towers: ");
            foreach (string tower in Enum.GetNames(typeof(HanoiType)))
            {
                Console.WriteLine($"\t{(int)Enum.Parse(typeof(HanoiType), tower)} - {tower}");
            }
        }

        /// <summary>
        /// Method for selecting a tower type.
        /// </summary>
        /// <returns>Returns the selected tower type object.</returns>
        public static HanoiType SelectHanoiTower()
        {
            // Display all known tower types
            WriteHanoiTypes();

            // Return the selected tower type object
            return (HanoiType)Enum.Parse(typeof(HanoiType), ReadInput("tower").ToString());
        }
    }
}
