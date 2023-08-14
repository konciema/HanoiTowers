using System;
using System.Collections.Generic;
using System.Text;

namespace HanoiTowers
{
    // Interface with the HanoiTower definition
    public interface IHanoiTower
    {
        /// <summary>
        /// Method that calculates the shortest path.
        /// maxMemory - maximum RAM usage.
        /// </summary>
        short ShortestPath(out long maxMemory);

        /// <summary>
        /// Method for recording a new state.
        /// </summary>
        /// <param name="state">Current state.</param>
        /// <param name="disc">Selected disc.</param>
        /// <param name="toRod">Selected rod.</param>
        void AddState(byte[] state, int disc, byte toRod);

        /// <summary>
        /// Method for the next move.
        /// </summary>
        void MakeMove(byte[] state);

        /// <summary>
        /// Method to calculate the final state.
        /// </summary>
        /// <returns>Final state.</returns>
        long FinalState();

        /// <summary>
        /// Method to calculate the state when all disks are of equal size.
        /// </summary>
        /// <param name="pegNumber">Number of pegs.</param>
        /// <returns>State when all disks are of equal size.</returns>
        long StateAllEqual(int pegNumber);

        /// <summary>
        /// Method to obtain the array when all disks are of equal size.
        /// </summary>
        /// <param name="pegNumber">Number of pegs.</param>
        /// <returns>Array when all disks are of equal size.</returns>
        byte[] ArrayAllEqual(byte pegNumber);

        /// <summary>
        /// Method to reset an array.
        /// </summary>
        void ResetArray(bool[] array);
    }
}
