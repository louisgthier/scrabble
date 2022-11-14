using System;
using NUnit.Framework;
using Scrabble;

namespace ScrabbleTests
{
    [TestFixture()]
    public class Sac_JetonsTests
    {

        [Test()]
        public void Retire_Jeton()
        {
            Sac_Jetons bag = new Sac_Jetons("Jetons.txt");

            Random rand = new Random();

            Jeton drawedToken = bag.Retire_Jeton(rand);

            Assert.IsTrue(drawedToken != null && drawedToken is Jeton);
        }
    }
}
