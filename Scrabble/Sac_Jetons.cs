using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace Scrabble
{
    /// <summary>
    /// Représente un ensemble de jetons où le joueur pourra piocher
    /// </summary>
    public class Sac_Jetons
    {
        /// <summary>
        /// Le sac de jetons
        /// </summary>
        List<Jeton> bag;

        /// <summary>
        /// Le nombre de jetons du sac
        /// </summary>
        public int Count { get { return bag.Count; } }

        /// <summary>
        /// Les scores associés à chaque lettre
        /// </summary>
        Dictionary<char, int> Scores;


        /// <summary>
        /// Instancie un nouveau sac à partir d'un fichier représentant le sac plein
        /// </summary>
        /// <param name="filledBagFilepath"></param>
        public Sac_Jetons(string filledBagFilepath)
        {
            bag = new List<Jeton>();
            Scores = new Dictionary<char, int>();

            string[] lines = File.ReadAllLines(filledBagFilepath);
            

            foreach (string line in lines)
            {
                char letter = line.Split(';')[0][0];
                int score = int.Parse(line.Split(';')[1]);
                int count = int.Parse(line.Split(';')[2]);
                for (int i = 0; i < count;i++)
                {
                    bag.Add(new Jeton(letter, score));
                }
                Scores.Add(letter, score);
            }
        }

        /// <summary>
        /// Instancie un sac de jetons à partir d'un fichier où a été sauvegardé un sac de jetons
        /// </summary>
        /// <param name="filledBagFilepath">Le fichier avec les données du sac lorsqu'il est plein</param>
        /// <param name="savedBagFilepath">Le fichier avec les données d'un sac sauvegardé dont les jetons seront transférés dans le nouveau sac</param>
        public Sac_Jetons (string filledBagFilepath, string savedBagFilepath)
        {
            bag = new List<Jeton>();
            Scores = new Dictionary<char, int>();

            string[] savedBagLines = File.ReadAllLines(savedBagFilepath);


            foreach (string line in savedBagLines)
            {
                char letter = line.Split(';')[0][0];
                int score = int.Parse(line.Split(';')[1]);
                int count = int.Parse(line.Split(';')[2]);
                for (int i = 0; i < count; i++)
                {
                    bag.Add(new Jeton(letter, score));
                }
                
            }

            string[] filledBagLines = File.ReadAllLines(filledBagFilepath);

            foreach (string line in filledBagLines)
            {
                char letter = line.Split(';')[0][0];
                int score = int.Parse(line.Split(';')[1]);
                Scores.Add(letter, score);
            }
            
        }

        /// <summary>
        /// Pioche un jeton dans le sac
        /// </summary>
        /// <param name="r"></param>
        /// <returns>Le jeton pioché</returns>
        public Jeton Retire_Jeton (Random r)
        {
            int randInt = r.Next(0, bag.Count);

            Jeton token = bag[randInt];

            bag.Remove(token);

            return token;
        }

        /// <summary>
        /// Connaître le score d'une lettre
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public int GetScore(char letter)
        {
            return Scores[letter];
        }

        /// <summary>
        /// Décrire le sac de jetons
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";
            List<Jeton> tempBag = bag.ToList();
            while (tempBag.Count != 0)
            { 
                char countedLetter = tempBag[0].Letter;
                int count = 0;
                int score = tempBag[0].Score;
                for (int j = tempBag.Count-1; j >= 0; j--)
                {
                    if (tempBag[j].Letter == countedLetter)
                    {
                        tempBag.RemoveAt(j);
                        count++;
                    }
                }

                result += countedLetter + ";" + score + ";" + count + "\n";
            }
            return result;
        }


    }
}
