using System;
using System.IO;
using System.Threading.Tasks;

namespace EveLocalChatAnalyser.Utilities
{
    public static class SoundPlayer
    {
        private static readonly string TADA_SOUND_FILE = Environment.GetEnvironmentVariable("WINDIR") +
                                                         "\\Media\\tada.wav";

        private static readonly string NEXT_SCAN_FILE = Path.GetDirectoryName(typeof (SoundPlayer).Assembly.CodeBase) +
                                                        "\\Resources\\next_scan.wav";

        private static readonly string SCAN_FAILURE_FILE =
            Path.GetDirectoryName(typeof (SoundPlayer).Assembly.CodeBase) +
            "\\Resources\\scan_failure.wav";

        private static readonly string SCAN_SUCCESS_LOW_FILE =
            Path.GetDirectoryName(typeof (SoundPlayer).Assembly.CodeBase) +
            "\\Resources\\scan_success_low.wav";

        private static readonly string SCAN_SUCCESS_FILE =
            Path.GetDirectoryName(typeof (SoundPlayer).Assembly.CodeBase) +
            "\\Resources\\scan_success.wav";

        private static readonly string SCAN_ITEMS_ADDED_FILE =
            Path.GetDirectoryName(typeof (SoundPlayer).Assembly.CodeBase) +
            "\\Resources\\scan_anoms_added.wav";


        private static readonly System.Media.SoundPlayer TADA_PLAYER = new System.Media.SoundPlayer(TADA_SOUND_FILE);
        private static readonly System.Media.SoundPlayer NEXT_SCAN_PLAYER = new System.Media.SoundPlayer(NEXT_SCAN_FILE);

        private static readonly System.Media.SoundPlayer SCAN_SUCCESS_PLAYER =
            new System.Media.SoundPlayer(SCAN_SUCCESS_FILE);

        private static readonly System.Media.SoundPlayer SCAN_FAILURE_PLAYER =
            new System.Media.SoundPlayer(SCAN_FAILURE_FILE);

        private static readonly System.Media.SoundPlayer SCAN_ITEMS_ADDED_PLAYER =
            new System.Media.SoundPlayer(SCAN_ITEMS_ADDED_FILE);

        private static readonly System.Media.SoundPlayer SCAN_SUCCESS_LOW_PLAYER =
            new System.Media.SoundPlayer(SCAN_SUCCESS_LOW_FILE);

        public static void PlayNextScan()
        {
            Play(NEXT_SCAN_PLAYER);
        }

        public static void PlayScanPotentialSuccess()
        {
            Play(SCAN_SUCCESS_LOW_PLAYER);
        }

        public static void PlayScanFailure()
        {
            Play(SCAN_FAILURE_PLAYER);
        }

        public static void PlayScanSuccess()
        {
            Play(SCAN_SUCCESS_PLAYER);
        }

        public static void PlayScanItemsAdded()
        {
            Play(SCAN_ITEMS_ADDED_PLAYER);
        }

        private static void Play(System.Media.SoundPlayer player)
        {
            Task.Factory.StartNew(() =>
                {
                    lock (typeof (SoundPlayer))
                    {
                        player.Play();
                    }
                });
        }
    }
}