using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVE_Killboard_Analyser.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EVE_Killboard_Analyser_Test
{
    [TestClass]
    public class ZKillboardStompServiceTest
    {

        [TestMethod]
        public void Test()
        {
            ZKillboardStompFeedConsumer.Start();
        }
    }
}
