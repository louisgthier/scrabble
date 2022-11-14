using NUnit.Framework;
using System;
using Scrabble;

namespace ScrabbleTests
{
    [TestFixture()]
    public class JoueurTests
    {

        [Test()]
        public void Add_MotTest()
        {
            Joueur Louis = new Joueur("Louis");

            Louis.Add_Mot("anticonstitutionnelement");

            Assert.IsTrue(Louis.FoundWordsCount == 1);
        }

        [Test()]
        public void ToStringNoWordsTest()
        {
            Joueur Louis = new Joueur("Louis");

            Assert.AreEqual(Louis.ToString(), "Louis,0,,");
        }

        [Test()]
        public void ToStringWithWordsTest()
        {
            Joueur Louis = new Joueur("Louis");

            Louis.Add_Mot("bonjour");
            Louis.Add_Mot("maison");

            Assert.AreEqual(Louis.ToString(), "Louis,0,bonjour maison,");
        }

    }
}
