using System;
using System.Collections.Generic;
using System.Text;

namespace HanoiTowers
{
    /// <summary>
    /// List of all tower types.
    /// </summary>
    public enum HanoiType
    {
        K13_01,    // Tower with 3 rods and 1 disk initially
        K13_12,    // Tower with 3 rods and 2 disks initially
        K13e_01,   // Tower with 3 rods and 1 disk initially, expanded version
        K13e_12,   // Tower with 3 rods and 2 disks initially, expanded version
        K13e_23,   // Tower with 3 rods and 3 disks initially, expanded version
        K13e_30,   // Tower with 3 rods and 0 disks initially, expanded version
        P4_01,     // Tower with 4 rods and 1 disk initially
        P4_12,     // Tower with 4 rods and 2 disks initially
        P4_23,     // Tower with 4 rods and 3 disks initially
        P4_31,     // Tower with 4 rods and 1 disk initially
        C4_01,     // Tower with 4 rods and 1 disk initially
        C4_12,     // Tower with 4 rods and 2 disks initially
        K4e_01,    // Tower with 4 rods and 1 disk initially, expanded version
        K4e_12,    // Tower with 4 rods and 2 disks initially, expanded version
        K4e_23     // Tower with 4 rods and 3 disks initially, expanded version
    }
}
