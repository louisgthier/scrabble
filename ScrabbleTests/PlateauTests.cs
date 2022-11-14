using NUnit.Framework;
using System;
using Scrabble;
namespace ScrabbleTests
{
    [TestFixture()]
    public class PlateauTests
    {
        [Test()]
        public void Board_Tests ()
        {
            Plateau plateau = new Plateau();
            plateau.PlaceWord("BONJOUR", 7, 7, '>');

            Assert.AreEqual(plateau.Board[7, 9], 'N');
        }
    }
}
