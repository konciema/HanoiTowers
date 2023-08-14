using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HanoiTowers
{
    /// <summary>
    /// Abstract class for the Hanoi tower object.
    /// </summary>
    public abstract class HanoiTower : IHanoiTower
    {
        // Define variables that all towers will have regardless of the type.
        protected readonly int numDiscs;
        protected readonly int numOfRods;
        protected readonly HanoiType type;

        protected HashSet<long> setPrev;

        protected HashSet<long> setCurrent;
        protected Queue<long> setNew;
        protected byte[] stateArray;

        protected short currentDistance;

        protected long finalState;

        // Constructor for HanoiTower.
        public HanoiTower(int numDiscs, int numOfRods, HanoiType type)
        {
            this.numDiscs = numDiscs;
            this.numOfRods = numOfRods;
            this.type = type;
        }

        // Abstract method for moving discs.
        public abstract void MakeMove(byte[] state);

        // Method for calculating the shortest path/number of steps needed.
        public short ShortestPath(out long maxMemory)
        {
            // Set necessary variables.
            setPrev = new HashSet<long>();
            setCurrent = new HashSet<long>();
            setNew = new Queue<long>();

            currentDistance = 0;

            // Convert the initial state to a long variable.
            long initialState = Functions.StateToLong(stateArray, this.numOfRods);

            // Add the current state to the setCurrent HashSet.
            setCurrent.Add(initialState);

            // Set variables for tracking memory usage and the number of all steps performed.
            long maxCardinality = 0;
            maxMemory = 0;

            // Unconditional while loop. We break the loop when we return the calculated currentDistance value.
            while (true)
            {
                // Condition to record the maximum number of necessary steps.
                if (maxCardinality < setCurrent.Count)
                    maxCardinality = setCurrent.Count;

                // Variable of type bool to track if we are in the last step of the operation.
                bool isFinalOperation = false;

                // Linq parallel method. For each state in the setCurrent HashSet, perform the following steps.
                setCurrent.AsParallel().ForAll(num =>
                {
                    // If the current state is equal to the final state, then we know this is the last step of the calculation.
                    if (num == finalState) { isFinalOperation = true; }

                    // Perform the next move. Pass the current state as a "State" type, obtained by converting the long to a State.
                    MakeMove(Functions.LongToState(num, this.numDiscs, this.numOfRods));
                });

                // Variable to store the current memory consumption.
                long mem = GC.GetTotalMemory(false);

                // If the current memory consumption is greater, store it.
                if (maxMemory < mem)
                {
                    maxMemory = mem;
                }

                // If we know that we are in the last step of the moves, then return the current number of necessary steps.
                if (isFinalOperation)
                {
                    return currentDistance;
                }

                // Store the current steps in the setPrev HashSet as the previous steps list.
                setPrev = setCurrent;

                // Clear the current steps.
                setCurrent = new HashSet<long>();

                // Calculate the number of new steps.
                int elts = setNew.Count;

                // For loop iterating through all the steps.
                for (int i = 0; i < elts; i++)
                {
                    // Add the first step in the new steps list to the current steps list.
                    setCurrent.Add(setNew.Dequeue());
                }

                // Clear the new steps list.
                setNew = new Queue<long>();

                // Increment the number of performed steps by one.
                currentDistance++;

                // Print the current operation.
                Console.WriteLine("Current distance: " + currentDistance + "     Maximum cardinality: " + maxCardinality);
                Console.WriteLine("Memory allocation: " + mem / 1000000 + "MB  \t\t Maximum memory: " + maxMemory / 1000000 + "MB");
                Console.CursorTop -= 2;
            }
        }

        // Calculate a new state by moving the selected disk to the given rod.
        public void AddState(byte[] state, int disc, byte toRod)
        {
            byte[] newState = new byte[state.Length];
            for (int x = 0; x < state.Length; x++)
                newState[x] = state[x];
            newState[disc] = toRod;
            long currentState = Functions.StateToLong(newState, this.numOfRods);
            if (!setPrev.Contains(currentState))
            {
                // Since we perform steps in parallel, we need to lock the new steps list until we store the current state in it.
                lock (setNew)
                {
                    // Add the current state to the end of the new steps list.
                    setNew.Enqueue(currentState);
                }
            }
        }



        // Method called upon object construction to create an array with all values equal to the specified rod number.
        public byte[] ArrayAllEqual(byte rodNumber)
        {
            byte[] arr = new byte[this.numDiscs];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = rodNumber;
            return arr;
        }

        // Method called upon object construction to calculate the final state.
        public long FinalState()
        {
            long num = 0;
            long factor = 1;
            for (int i = numDiscs - 1; i >= 0; i--)
            {
                num += factor;
                factor *= this.numOfRods;
            }
            return num;
        }

        // Method called upon object creation to calculate the state with all discs on the specified rod.
        public long StateAllEqual(int rodNumber)
        {
            long num = 0;
            long factor = 1;
            for (int i = numDiscs - 1; i >= 0; i--)
            {
                num += rodNumber * factor;
                factor *= this.numOfRods;
            }
            return num;
        }

        // Method for resetting an array of possible locations where a disc can be moved.
        public void ResetArray(bool[] array)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = true;
        }
    }


    public class K13_01 : HanoiTower
    {
        //"Overridden" method for calculating the next step for the tower.
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel command execution, we need to utilize a local variable called "canMoveArray"; otherwise, we might read it with each call, which is not correct.
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values are "true".
            ResetArray(canMoveArray);

            if (numDiscs > 1)
            {
                for (int i = 0; i < numDiscs - 2; i++)
                {
                    if (canMoveArray[state[i]])
                    {
                        if (state[i] == 0)
                        {
                            for (byte j = 1; j < numOfRods; j++)
                            {
                                if (canMoveArray[j])
                                {
                                    AddState(state, i, j);
                                }
                            }
                        }
                        else // From other vertices we can only move to center
                        {
                            if (canMoveArray[0])
                            {
                                AddState(state, i, 0);
                            }
                        }
                    }
                    canMoveArray[state[i]] = false;
                }
                // The second biggest:
                if (state[numDiscs - 2] == 0 && state[numDiscs - 1] == 0)
                {
                    if (canMoveArray[0] && canMoveArray[2])
                    {
                        AddState(state, numDiscs - 2, 2);
                    }
                    if (canMoveArray[0] && canMoveArray[3])
                    {
                        AddState(state, numDiscs - 2, 3);
                    }
                    canMoveArray[0] = false;
                }
                else if (state[numDiscs - 2] == 0 && state[numDiscs - 1] == 1)
                {
                    if (canMoveArray[0] && canMoveArray[1])
                    {
                        AddState(state, numDiscs - 2, 1);
                    }
                    canMoveArray[0] = false;
                }
                else if (state[numDiscs - 2] > 1 && state[numDiscs - 1] == 1)
                {
                    if (canMoveArray[state[numDiscs - 2]] && canMoveArray[0])
                    {
                        AddState(state, numDiscs - 2, 0);
                    }
                    canMoveArray[state[numDiscs - 2]] = false;
                }
                // Biggest disk is moved only once
                if (state[numDiscs - 1] == 0)
                {
                    if (canMoveArray[0] && canMoveArray[1])
                    {
                        AddState(state, numDiscs - 1, 1);
                    }
                }
            }
            else
            {
                for (int i = 0; i < numDiscs; i++)
                {
                    if (canMoveArray[state[i]])
                    {
                        if (state[i] == 0)
                        {
                            for (byte j = 1; j < numOfRods; j++)
                            {
                                if (canMoveArray[j])
                                {
                                    AddState(state, i, j);
                                }
                            }
                        }
                        else // From other vertices we can only move to center
                        {
                            if (canMoveArray[0])
                            {
                                AddState(state, i, 0);
                            }
                        }
                    }
                    canMoveArray[state[i]] = false;
                }
            }
        }



        public K13_01(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(0);
            this.finalState = FinalState();
        }
    }

    public class K13_12 : HanoiTower
    {
        // Overridden method for the next move while building the tower
        public override void MakeMove(byte[] state)
        {
            // We use parallel programming - new variable canMove
            bool[] canMove = new bool[this.numOfRods];

            // Reset the array so that all values are "true".
            ResetArray(canMove);

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMove[state[i]])
                {
                    if (state[i] == 0)
                    {
                        for (byte j = 1; j < numOfRods; j++)
                        {
                            if (canMove[j])
                            {
                                AddState(state, i, j);
                            }
                        }
                    }
                    else // From other vertices we can only move to the center
                    {
                        if (canMove[0])
                        {
                            AddState(state, i, 0);
                        }
                    }
                }
                canMove[state[i]] = false;
            }
        }

        // Constructor for tower K13_12. Upon creating the object, we set its state array and calculate the final state.
        public K13_12(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(2);
            this.finalState = FinalState();
        }
    }

    public class K13e_01 : HanoiTower
    {
        //"Overridden" method for calculating the next step for the tower
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel command execution, we need to utilize a local variable called "canMoveArray"; otherwise, we might read it with each call, which is not correct.
            bool[] canMoveArray = new bool[this.numOfRods];

            // Reset the array so that all values are "true".
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        for (byte j = 1; j < numOfRods; j++)
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        if (canMoveArray[0])
                        {
                            newState = new byte[state.Length];
                            for (int x = 0; x < state.Length; x++)
                                newState[x] = state[x];
                            newState[i] = 0;
                            long currentState = Functions.StateToLong(newState, this.numOfRods);
                            if (!setPrev.Contains(currentState))
                            {
                                // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                lock (setNew)
                                {
                                    setNew.Enqueue(currentState);
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 0, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }



        public K13e_01(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(0);
            this.finalState = StateAllEqual(1);
        }
    }

    public class K13e_12 : HanoiTower
    {
        //"Overridden" method for calculating the next step for the tower
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel command execution, we need to utilize a local variable called "canMoveArray"; otherwise, we might read it with each call, which is not correct.
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values are "true".
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        for (byte j = 1; j < numOfRods; j++)
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        if (canMoveArray[0])
                        {
                            newState = new byte[state.Length];
                            for (int x = 0; x < state.Length; x++)
                                newState[x] = state[x];
                            newState[i] = 0;
                            long currentState = Functions.StateToLong(newState, this.numOfRods);
                            if (!setPrev.Contains(currentState))
                            {
                                // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                lock (setNew)
                                {
                                    setNew.Enqueue(currentState);
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 0, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for tower K13e_12. Upon creating the object, we set its state array and calculate the final state.
        public K13e_12(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(1);
            this.finalState = StateAllEqual(2);
        }
    }

    public class K13e_23 : HanoiTower
    {
        //"Overridden" method for calculating the next step for the tower
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel command execution, we need to utilize a local variable called "canMoveArray"; otherwise, we might read it with each call, which is not correct.
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values are "true".
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        for (byte j = 1; j < numOfRods; j++)
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        if (canMoveArray[0])
                        {
                            newState = new byte[state.Length];
                            for (int x = 0; x < state.Length; x++)
                                newState[x] = state[x];
                            newState[i] = 0;
                            long currentState = Functions.StateToLong(newState, this.numOfRods);
                            if (!setPrev.Contains(currentState))
                            {
                                // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                lock (setNew)
                                {
                                    setNew.Enqueue(currentState);
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 0, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the new steps list until we store the current state in it.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }



        // Constructor for tower K13e_23. Upon creating the object, we set its state array and calculate the final state.
        public K13e_23(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(2);
            this.finalState = StateAllEqual(3);
        }
    }

    public class K13e_30 : HanoiTower
    {
        // Overridden method for calculating the next move for tower K13e_30
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel execution of commands, we need to use a local variable canMoveArray, otherwise, it could be read with every call, which is not correct
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values in the array are "true"
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        for (byte j = 1; j < numOfRods; j++)
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        if (canMoveArray[0])
                        {
                            newState = new byte[state.Length];
                            for (int x = 0; x < state.Length; x++)
                                newState[x] = state[x];
                            newState[i] = 0;
                            long currentState = Functions.StateToLong(newState, this.numOfRods);
                            if (!setPrev.Contains(currentState))
                            {
                                // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                lock (setNew)
                                {
                                    setNew.Enqueue(currentState);
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 0, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        //Constructor for towerK13e_30. Upon creating the object, we set its state array and calculate the final state.
        public K13e_30(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(3);
            this.finalState = StateAllEqual(0);
        }
    }

    public class P4_01 : HanoiTower
    {
        // Overridden method for calculating the next move for tower P4_01
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel execution of commands, we need to use a local variable canMoveArray, otherwise, it could be read with every call, which is not correct
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values in the array are "true"
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        foreach (byte j in new byte[] { 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        foreach (byte j in new byte[] { 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 1, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end

                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for tower P4_01. Upon creating the object, we set its state array and calculate the final state.
        public P4_01(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(0);
            this.finalState = StateAllEqual(1);
        }
    }

    public class P4_12 : HanoiTower
    {
        // Overridden method for calculating the next move for tower P4_12
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel execution of commands, we need to use a local variable canMoveArray, otherwise, it could be read with every call, which is not correct
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values in the array are "true"
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        foreach (byte j in new byte[] { 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        foreach (byte j in new byte[] { 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 1, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for tower P4_12. Upon creating the object, we set its state array and calculate the final state.
        public P4_12(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(1);
            this.finalState = StateAllEqual(2);
        }
    }

    public class P4_23 : HanoiTower
    {
        // Overridden method for calculating the next move for tower P4_23
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel execution of commands, we need to use a local variable canMoveArray, otherwise, it could be read with every call, which is not correct
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values in the array are "true"
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        foreach (byte j in new byte[] { 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        foreach (byte j in new byte[] { 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 1, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for tower P4_23. Upon creating the object, we set its state array and calculate the final state.
        public P4_23(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(2);
            this.finalState = StateAllEqual(3);

        }
    }

    public class P4_31 : HanoiTower
    {
        // Overridden method for calculating the next move for tower P4_31
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel execution of commands, we need to use a local variable canMoveArray, otherwise, it could be read with every call, which is not correct
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values in the array are "true"
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        foreach (byte j in new byte[] { 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        foreach (byte j in new byte[] { 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 1, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 2 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for tower P4_31. Upon creating the object, we set its state array and calculate the final state.
        public P4_31(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(3);
            this.finalState = StateAllEqual(1);
        }
    }

    public class C4_01 : HanoiTower
    {
        // Overridden method for calculating the next move for tower C4_01
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel execution of commands, we need to use a local variable canMoveArray, otherwise, it could be read with every call, which is not correct
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values in the array are "true"
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        foreach (byte j in new byte[] { 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        foreach (byte j in new byte[] { 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for tower C4_01. Upon creating the object, we set its state array and calculate the final state.
        public C4_01(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(0);
            this.finalState = StateAllEqual(1);
        }
    }

    public class C4_12 : HanoiTower
    {
        // Overridden method for calculating the next move for tower C4_12
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel execution of commands, we need to use a local variable canMoveArray, otherwise, it could be read with every call, which is not correct
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values in the array are "true"
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        foreach (byte j in new byte[] { 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        foreach (byte j in new byte[] { 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for tower C4_12. Upon creating the object, we set its state array and calculate the final state.
        public C4_12(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(1);
            this.finalState = StateAllEqual(2);
        }
    }

    public class K4e_01 : HanoiTower
    {
        // Overridden method for calculating the next move for tower K4e_01
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel execution of commands, we need to use a local variable canMoveArray; otherwise, it could be read with every call, which is not correct
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values in the array are "true"
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        foreach (byte j in new byte[] { 1, 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        foreach (byte j in new byte[] { 0, 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel command execution, we need to lock the table of new steps until we write the current step to the end.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    //When using parallel command execution, we need to lock the table of new steps until we append the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {

                                    // As we are using parallel command execution, we need to lock the table of new steps until we write the current step to the end.
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for the tower K4e_01. Upon creating the object, we set its state table and calculate the final state.
        public K4e_01(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(0);
            this.finalState = StateAllEqual(1);
        }
    }

    public class K4e_12 : HanoiTower
    {
        //Method that calculates steps
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel command execution, we need to utilize a local variable called "canMoveArray"; otherwise, we might read it with each call, which is not correct.
            bool[] canMoveArray = new bool[numOfRods];

            // We reset the table so that all values in the array are "true."
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        foreach (byte j in new byte[] { 1, 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        foreach (byte j in new byte[] { 0, 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for tower K4e_12. Upon creating the object, we set its state array and calculate the final state.
        public K4e_12(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(1);
            this.finalState = StateAllEqual(2);
        }
    }

    public class K4e_23 : HanoiTower
    {
        // Overridden method for calculating the next move for tower K4e_23
        public override void MakeMove(byte[] state)
        {
            // Since we are using parallel execution of commands, we need to use a local variable canMoveArray; otherwise, it could be read with every call, which is not correct
            bool[] canMoveArray = new bool[numOfRods];

            // Reset the array so that all values in the array are "true"
            ResetArray(canMoveArray);

            byte[] newState;

            for (int i = 0; i < numDiscs; i++)
            {
                if (canMoveArray[state[i]])
                {
                    if (state[i] == 0)
                    {
                        foreach (byte j in new byte[] { 1, 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 1)
                    {
                        foreach (byte j in new byte[] { 0, 2, 3 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 2)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                    else if (state[i] == 3)
                    {
                        foreach (byte j in new byte[] { 0, 1 })
                        {
                            if (canMoveArray[j])
                            {
                                newState = new byte[state.Length];
                                for (int x = 0; x < state.Length; x++)
                                    newState[x] = state[x];
                                newState[i] = j;
                                long currentState = Functions.StateToLong(newState, this.numOfRods);
                                if (!setPrev.Contains(currentState))
                                {
                                    // Since we are using parallel execution of commands, we need to lock the array of new steps until we write the current step to the end
                                    lock (setNew)
                                    {
                                        setNew.Enqueue(currentState);
                                    }
                                }
                            }
                        }
                    }
                }
                canMoveArray[state[i]] = false;
            }
        }

        // Constructor for tower K4e_23. Upon creating the object, we set its state array and calculate the final state.
        public K4e_23(int numDiscs, int numOfRods, HanoiType type) : base(numDiscs, numOfRods, type)
        {
            this.stateArray = ArrayAllEqual(2);
            this.finalState = StateAllEqual(3);
        }
    }
}
