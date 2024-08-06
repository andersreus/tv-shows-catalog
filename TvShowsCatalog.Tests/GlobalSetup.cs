using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvShowsCatalog.Tests
{
    [SetUpFixture]
    public class GlobalSetup
    {
        private static GlobalSetupTeardown _setupTearDown;

        [OneTimeSetUp]
        public void SetUp()
        {
            _setupTearDown = new GlobalSetupTeardown();
            _setupTearDown.SetUp();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _setupTearDown.TearDown();
        }
    }
}
