using System;
using NUnit.Framework;
using Scrabble;

namespace ScrabbleTests
{
    [TestFixture()]
    public class DictionnaireTests
    {

        [Test()]
        public void RechDichoRecursifTest()
        {
            Dictionnaire dico = new Dictionnaire("Francais.txt", "Français");
            Assert.IsTrue (dico.RechDichoRecursif("Aimer") && dico.RechDichoRecursif("AiMer") && !dico.RechDichoRecursif("J")
                && dico.RechDichoRecursif("AA") && dico.RechDichoRecursif("XI") && !dico.RechDichoRecursif("ANTICONSTITUTIONNELEMENT")
                && dico.RechDichoRecursif("CONSTITUTION")) ;
            
        }

    }
}
