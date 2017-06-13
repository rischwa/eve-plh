using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLHLib
{
    public static class Types
    {

        public static bool IsSmartBomb(this int weaponTypeId)
        {
            return SMARTBOMB_IDS.BinarySearchContains(weaponTypeId);
        }
        public static readonly int[] SMARTBOMB_IDS = {
                                                         1547,
                                                         1549,
                                                         1551,
                                                         1553,
                                                         1557,
                                                         1559,
                                                         1563,
                                                         1565,
                                                         3897,
                                                         3899,
                                                         3901,
                                                         3903,
                                                         3907,
                                                         3909,
                                                         3913,
                                                         3915,
                                                         3937,
                                                         3939,
                                                         3941,
                                                         3943,
                                                         3947,
                                                         3949,
                                                         3953,
                                                         3955,
                                                         3977,
                                                         3979,
                                                         3981,
                                                         3983,
                                                         3987,
                                                         3989,
                                                         3993,
                                                         3995,
                                                         9668,
                                                         9670,
                                                         9678,
                                                         9680,
                                                         9702,
                                                         9706,
                                                         9728,
                                                         9734,
                                                         9744,
                                                         9750,
                                                         9762,
                                                         9772,
                                                         9784,
                                                         9790,
                                                         9800,
                                                         9808,
                                                         14188,
                                                         14190,
                                                         14192,
                                                         14194,
                                                         14196,
                                                         14198,
                                                         14200,
                                                         14202,
                                                         14204,
                                                         14206,
                                                         14208,
                                                         14210,
                                                         14212,
                                                         14214,
                                                         14218,
                                                         14220,
                                                         14222,
                                                         14224,
                                                         14226,
                                                         14228,
                                                         14544,
                                                         14546,
                                                         14548,
                                                         14550,
                                                         14692,
                                                         14694,
                                                         14696,
                                                         14698,
                                                         14784,
                                                         14786,
                                                         14788,
                                                         14790,
                                                         14792,
                                                         14794,
                                                         14796,
                                                         14798,
                                                         15152,
                                                         15154,
                                                         15156,
                                                         15158,
                                                         15405,
                                                         15925,
                                                         15927,
                                                         15929,
                                                         15931,
                                                         15933,
                                                         15935,
                                                         15937,
                                                         15939,
                                                         15941,
                                                         15943,
                                                         15945,
                                                         15947,
                                                         15949,
                                                         15951,
                                                         15953,
                                                         15955,
                                                         15957,
                                                         15959,
                                                         15961,
                                                         15963,
                                                         21532,
                                                         21534,
                                                         21536,
                                                         21538,
                                                         23864,
                                                         23866,
                                                         23868,
                                                         28545,
                                                         28550,
                                                         28557
                                                     };

        public static bool IsCommandDestroyer(int shipTypeId)
        {
            switch (shipTypeId)
            {
                case 37480:
                case 37481:
                case 37482:
                case 37483:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsGateCampShip(int shipTypeID, int weaponTypeID)
        {
            if (!GATECAMP_SHIP_LIST.Contains(shipTypeID))
            {
                return false;
            }
            //sometimes the weapon does not appear on the killmail, but the shiptype instead
            if (shipTypeID == weaponTypeID)
            {
                return true;
            }

            var isThrasherOrSvipul = shipTypeID == 16242 || shipTypeID == 34562;
            if (isThrasherOrSvipul)
            {
                return ARTILLERY_WEAPON_TYPE_IDS.BinarySearchContains(weaponTypeID)
                       || WARP_SCRAMBLER_TYPE_IDS.BinarySearchContains(weaponTypeID);
            }

            var isLoki = shipTypeID == 29990;
            if (isLoki)
            {
                return ARTILLERY_WEAPON_TYPE_IDS.BinarySearchContains(weaponTypeID) || WEBIFIER_TYPE_IDS.BinarySearchContains(weaponTypeID);
            }

            return true;
            //TODO (Arty) Svipul, Interceptor, Daredevil, (Arty) Thrasher, Gnosis, (Arty) Loki, Sabre, Hic?, Dic?
        }

        private static bool BinarySearchContains(this int[] array, int weaponTypeID)
        {
            return Array.BinarySearch(array, weaponTypeID) >= 0;
        }

        public static readonly int[] GATECAMP_SHIP_LIST = {//interdictors
                                                              22456, //sabre
                                                              22460, 224644, 224452,

                                                              //hics
                                                              12017, 35781, 11995, 12013, 12021,

                                                              //interceptors
                                                              11200, 11302, 35779, 11176, 33673, 11178, 11184, 11186, 11196, 11198,

                                                              //dd
                                                              17928,
                                                              //gnosis
                                                              3756,
                                                              //thrasher
                                                              16242,
                                                              //loki
                                                              29990,
                                                              //svipul
                                                              34562
        };


        public static readonly int[] ARTILLERY_WEAPON_TYPE_IDS =
        {
            487, 488, 492, 493, 497, 498, 2865, 2905, 2921, 2961, 2969, 2977, 8903,
            9207, 9367, 9373, 9411, 9419, 9451, 9491, 12201, 12202, 12203, 13774,
            13775, 13779, 13781, 13783, 13784, 14461, 14463, 14465, 14467, 15443,
            15445, 16047, 16048, 16052, 16053, 16055, 16056, 16148, 16149, 20454,
            21547, 21549, 21553, 21555, 21559, 21561, 21744, 23428, 23462, 23998,
            24210
        };

        public static readonly int[] WEBIFIER_TYPE_IDS =
        {
            526, 527, 4025, 4027, 4029, 4031, 14262, 14264, 14266, 14268, 14270, 14648, 14650,
            14652, 14654, 15419, 17500, 17559, 28514, 30328
        };

        public static readonly int[] WARP_SCRAMBLER_TYPE_IDS = {447,448,3242,3244,5399,5401,5403,5405,5439,5441,5443,5445,14242,14244,14246,14248,14250,14252,14254,14256,14258,14260,14656,14658,14660,14662,14664,14666,14668,14670,15431,15433,15887,15889,15891,15893,16140,21510,21512,22476,28516,28518,32459

};
    }
}
