using System;
using System.Collections.Generic;
using System.Text;

namespace HanoiTowers
{
    class HanoiTowerFactory
    {
        public static IHanoiTower CreateHanoiTower(HanoiType type, int numDiscs, int numRods)
        {
            IHanoiTower tower = null;

            switch (type)
            {
                case HanoiType.K13_01:
                    tower = new K13_01(numDiscs, numRods, type);
                    break;

                case HanoiType.K13_12:
                    tower = new K13_12(numDiscs, numRods, type);
                    break;

                case HanoiType.K13e_01:
                    tower = new K13e_01(numDiscs, numRods, type);
                    break;

                case HanoiType.K13e_12:
                    tower = new K13e_12(numDiscs, numRods, type);
                    break;

                case HanoiType.K13e_23:
                    tower = new K13e_23(numDiscs, numRods, type);
                    break;

                case HanoiType.K13e_30:
                    tower = new K13e_30(numDiscs, numRods, type);
                    break;

                case HanoiType.P4_01:
                    tower = new P4_01(numDiscs, numRods, type);
                    break;

                case HanoiType.P4_12:
                    tower = new P4_12(numDiscs, numRods, type);
                    break;

                case HanoiType.P4_23:
                    tower = new P4_23(numDiscs, numRods, type);
                    break;

                case HanoiType.P4_31:
                    tower = new P4_31(numDiscs, numRods, type);
                    break;

                case HanoiType.C4_01:
                    tower = new C4_01(numDiscs, numRods, type);
                    break;

                case HanoiType.C4_12:
                    tower = new C4_12(numDiscs, numRods, type);
                    break;

                case HanoiType.K4e_01:
                    tower = new K4e_01(numDiscs, numRods, type);
                    break;

                case HanoiType.K4e_12:
                    tower = new K4e_12(numDiscs, numRods, type);
                    break;

                case HanoiType.K4e_23:
                    tower = new K4e_23(numDiscs, numRods, type);
                    break;
            }

            return tower;
        }
    }
}
