using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Scrabble
{
    /// <summary>
    /// Contient tous les mots autorisés et permet de les retrouver rapidement
    /// </summary>
    public class Dictionnaire
    {
        string language;

        Dictionary<int, string[]> dictionary;

        /// <summary>
        /// Crée un dictionnaire à partir d'un fichier
        /// </summary>
        /// <param name="filepath">Le nom du fichier</param>
        /// <param name="language"></param>
        public Dictionnaire(string filepath, string language)
        {
            this.language = language;
            dictionary = new Dictionary<int, string[]>();

            try
            {
                using (StreamReader sr = new StreamReader(filepath))
                {
                    string line1;
                    string line2;
                    while ((line1 = sr.ReadLine()) != null && (line2 = sr.ReadLine()) != null)
                    {

                        int numberOfLetters = int.Parse(line1);
                        string[] words = line2.Split(' ');

                        dictionary.Add(numberOfLetters, words);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override string ToString ()
        {
            string result = language+"\n";

            foreach (KeyValuePair<int, string[]> element in dictionary)
            {
                result += "Mots à "+element.Key + " lettres: "+ element.Value.Length + "\n";    
            }
            return result;
        }

        public List<string> ToList()
        {
            List<string> result = new List<string>();

            foreach (KeyValuePair<int, string[]> keyValuePair in dictionary)
            {
                result.AddRange(keyValuePair.Value.ToList());
            }

            return result;
        }

        /// <summary>
        /// Recherche dichotomique pour vérifier si un mot est dans le dictionnaire
        /// </summary>
        /// <param name="mot">Le mot à chercher</param>
        /// <param name="firstIndex"></param>
        /// <param name="lastIndex"></param>
        /// <returns>Est-ce que le mot est présent</returns>
        public bool RechDichoRecursif(string mot, int firstIndex=0, int lastIndex=-10)
        {
            // Si le dictionnaire ne contient pas de mots de la longueur de celui recherché, on retourne false
            if (!dictionary.ContainsKey(mot.Length))
                return false;

            mot = mot.ToUpper();

            // On définit la borne maximum de la recherche dichotomique lors du premier appel de la fonction (lastIndex est à -10 par défaut)
            if (lastIndex == -10)
                lastIndex = dictionary[mot.Length].Length-1;

            // On trouve le mot au centre de l'espace de recherche
            int middle = (lastIndex + firstIndex) / 2;
            string middleWord = dictionary[mot.Length][middle];

            if (middleWord == mot) // On tombe directement sur le mot cherché
                return true;
            else if (lastIndex - firstIndex <= 1) // Il ne reste plus que 2 mots possibles ou moins
            {
                if (dictionary[mot.Length][lastIndex] == mot || dictionary[mot.Length][firstIndex] == mot)
                    return true;
                else
                    return false;
            }
            else if (middleWord.CompareTo(mot) < 0) // Le mot cherché est après le mot du milieu
            {
                firstIndex = middle+1;
                return RechDichoRecursif(mot, firstIndex, lastIndex);
            }
            else // Le mot cherché est avant le mot du milieu
            {
                lastIndex = middle - 1;
                return RechDichoRecursif(mot, firstIndex, lastIndex);
            }
        }
    }

}
