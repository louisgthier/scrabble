using System;

namespace Scrabble
{
    /// <summary>
    /// Représente un jeton
    /// </summary>
    public class Jeton
    {
        char letter;
        int score;

        /// <summary>
        /// La lettre du jeton
        /// </summary>
        public char Letter { get { return letter; } }
        /// <summary>
        /// Le nombre de points qu'apporte le jeton
        /// </summary>
        public int Score { get { return score; } }

        public Jeton(char letter, int score)
        {

            this.letter = letter.ToString().ToUpper()[0];
            this.score = score;

        }

        public override string ToString()
        {
            return letter + ";" + score;
        }
    }
}
