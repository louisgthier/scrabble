using System;
using System.Collections.Generic;
namespace Scrabble
{
    /// <summary>
    /// Représente un joueur
    /// </summary>
    public class Joueur
    {
        // Le nom du joueur
        protected string name;
        public string Name { get { return name; } }

        // Le score du joueur
        protected int score;
        public int Score { get { return score; } }

        // Les mots trouvés par le joueur
        protected List<string> foundWords;
        public int FoundWordsCount { get { return this.foundWords.Count; } }

        // La main courante du joueur
        protected List<Jeton> hand;
        public List<Jeton> Hand { get { return hand; } }

        /// <summary>
        /// Créer un nouveau joueur
        /// </summary>
        /// <param name="name">Le nom du joueur</param>
        public Joueur(string name)
        {
            this.name = name;
            this.score = 0;
            this.foundWords = new List<string>();
            this.hand = new List<Jeton>();
        }

        /// <summary>
        /// Récupérer les données d'un joueur à partir d'un Dictionary
        /// </summary>
        /// <param name="playerData">Le Dictionary contenant les données du joueur</param>
        public Joueur (Dictionary<string, string> playerData)
        {
            this.name = playerData["name"];
            this.score = int.Parse(playerData["score"]);
            this.foundWords = new List<string>(playerData["foundWords"].Split(' '));
            this.hand = new List<Jeton>();
            if (playerData["hand"].Length > 0)
            {
                foreach (string element in playerData["hand"].Split(' '))
                {
                    this.hand.Add(new Jeton(element[0], int.Parse(element.Substring(1))));
                }
            }
        }

        /// <summary>
        /// Ajoute un mot à la liste de mots trouvés du joueur
        /// </summary>
        /// <param name="word">Le mot à ajouter</param>
        public void Add_Mot (string word)
        {
            if (this.foundWords == null)
                this.foundWords = new List<string> { word };
            else
                this.foundWords.Add(word);
        }


        /// <summary>
        /// Retourne une chaîne de caractères décrivant le joueur
        /// </summary>
        /// <returns>La chaîne de caractères décrivant le joueur</returns>
        public override string ToString()
        {
            string foundWords = "";
            for (int i = 0; i < this.foundWords.Count; i++)
            {
                if (i != 0)
                    foundWords += " ";
                foundWords += this.foundWords[i];
            }

            string hand = "";
            for (int i = 0; i < this.hand.Count; i++)
            {
                if (i != 0)
                   hand += " ";
                hand += this.hand[i].Letter+this.hand[i].Score.ToString();
            }

            return this.Name + "," + this.Score + "," + foundWords + "," + hand;
        }

        /// <summary>
        /// Récupérer la main courante du joueur
        /// </summary>
        /// <returns>Les lettres de la main courante en une seule string</returns>
        public string GetHand ()
        {
            string result = "";
            foreach (Jeton token in hand)
            {
                result += token.Letter;
            }
            return result;
        }

        /// <summary>
        /// Ajouter un certain score au joueur
        /// </summary>
        /// <param name="value">Le score à ajouter</param>
        public void Add_Score (int value)
        {
            this.score += value;
        }

        /// <summary>
        /// Ajouter un jeton à la main courante du joueur
        /// </summary>
        /// <param name="token">Le jeton à ajouter</param>
        public void Add_Main_Courante (Jeton token)
        {
            this.hand.Add(token);
        }

        /// <summary>
        /// Piocher des jetons tant que le joueur n'a pas 7 jetons et que le sac n'est pas vide
        /// </summary>
        /// <param name="bag">Le sac où piocher les jetons</param>
        /// <param name="rand">Le générateur de nombres aléatoire utilisé pour le tirage</param>
        public void Draw (Sac_Jetons bag, Random rand)
        {
            while (hand.Count < 7 && bag.Count > 0)
            {
                Add_Main_Courante(bag.Retire_Jeton(rand));
            }
        }

        /// <summary>
        /// Retire un jeton de la main courante du joueur.
        /// </summary>
        /// <param name="token">Le jeton à retirer</param>
        /// <returns>Retourne true si le jeton a pu être enlevé, false sinon</returns>
        public bool Remove_Main_Courante (Jeton token)
        {
            if (this.hand.Contains(token))
            {
                this.hand.Remove(token);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
