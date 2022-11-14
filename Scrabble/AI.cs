using System;
using System.Collections.Generic;
namespace Scrabble
{
    public class AI : Joueur
    {

        /// <summary>
        /// Créer une IA. Appelle le constructeur de la classe Joueur.
        /// </summary>
        /// <param name="playerData"></param>
        public AI(string name) : base(name) { }

        /// <summary>
        /// Récupérer les données d'une IA sauvegardée. Appelle le constructeur de la classe Joueur.
        /// </summary>
        /// <param name="playerData"></param>
        public AI(Dictionary<string, string> playerData) : base(playerData) { }

        /// <summary>
        /// Fonction appelée par le jeu pour savoir l'action que souhaite jouer une IA lorsque c'est son tour.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="dictionary"></param>
        /// <param name="bag"></param>
        /// <returns>L'action jouée sous forme de Tuple</returns>
        public Tuple<string, int, int, char> GetAction(Plateau board, Dictionnaire dictionary, Sac_Jetons bag)
        {
            string bestWord = null; // Le meilleur mot trouvé
            int bestScore = 0;
            int bestLine = -1;
            int bestColumn = -1;
            char bestDirection = '>';

            List<string> wordList = dictionary.ToList(); // Tous les mots du dictionnaire

            // Si c'est le premier tour, on trouve le meilleur mot possible avec les lettres données
            if (board.Board[7, 7] == ' ')
            {
                // On itère parmi tous les mots du dictionnaire
                foreach (string word in wordList)
                {
                    List<char> tempHand = new List<char>(this.GetHand().ToCharArray());
                    // On vérifie si le mot peut être placé avec la main actuelle du joueur
                    if (HandContainsLetters(word.ToCharArray(), tempHand))
                    {
                        for (int j = 7 - (word.Length - 1); j <= 7; j++)
                        {
                            int wordScore = 0;
                            board.Test_Plateau(word, 7, j, '>', dictionary, this, ref wordScore, bag, false);
                            if (wordScore > bestScore)
                            {
                                bestWord = word;
                                bestScore = wordScore;
                                bestLine = 7;
                                bestColumn = j;
                            }
                        }
                    }
                }
                bestDirection = '>';

            }
            else //Ce n'est pas le premier tour
            {

                int IA = 2; // La version de l'IA à utiliser (1 ou 2)
                if (IA == 2)
                {

                    // IA V2

                    // On récupère un pattern pour chaque ligne et chaque colonne et on trouve tous les mots qu'on peut placer selon ce pattern
                    for (int i = 0; i < board.Board.GetLength(0); i++)
                    {
                        List<Tuple<string, int, int, char>> words = new List<Tuple<string, int, int, char>>();

                        // Pattern à la ligne i
                        string linePattern = "";
                        for (int column = 0; column < board.Board.GetLength(1); column++)
                        {
                            linePattern += board.Board[i, column];
                        }

                        List<Tuple<string, int>> wordsOnLine = WordsThatMatchPattern(linePattern, this.GetHand().ToCharArray(), dictionary);

                        foreach (Tuple<string, int> wordOnLine in wordsOnLine)
                        {
                            words.Add(new Tuple<string, int, int, char>(wordOnLine.Item1, i, wordOnLine.Item2, '>'));
                        }

                        // Pattern à la colonne i
                        string columnPattern = "";
                        for (int line = 0; line < board.Board.GetLength(1); line++)
                        {
                            columnPattern += board.Board[line, i];
                        }

                        List<Tuple<string, int>> wordsOnColumn = WordsThatMatchPattern(columnPattern, this.GetHand().ToCharArray(), dictionary);


                        foreach (Tuple<string, int> wordOnColumn in wordsOnColumn)
                        {
                            words.Add(new Tuple<string, int, int, char>(wordOnColumn.Item1, wordOnColumn.Item2, i, 'v'));
                        }


                        foreach (Tuple<string, int, int, char> word in words)
                        {

                            int wordScore = 0;
                            if (board.Test_Plateau(word.Item1, word.Item2, word.Item3, word.Item4, dictionary, this, ref wordScore, bag, false))
                            {
                                if (wordScore > bestScore)
                                {
                                    bestWord = word.Item1;
                                    bestScore = wordScore;
                                    bestLine = word.Item2;
                                    bestColumn = word.Item3;
                                    bestDirection = word.Item4;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // IA V1 (l'IA est plus rapide mais moins performante au jeu)
                    #region

                    //On récupère toutes les lettres du plateau avec leur position
                    for (int i = 0; i < board.Board.GetLength(0); i++)
                    {
                        for (int j = 0; j < board.Board.GetLength(1); j++)
                        {
                            Tuple<char, int, int> boardLetter = new Tuple<char, int, int>(board.Board[i, j], i, j);

                            if (boardLetter.Item1 == ' ')
                                continue;

                            // Trouver tous les mots que l'on peut former avec cette lettre et les lettres de notre main
                            foreach (string word in wordList)
                            {
                                List<char> tempHand = new List<char>(this.GetHand().ToCharArray());
                                tempHand.Add(boardLetter.Item1);
                                if (HandContainsLetters(word.ToCharArray(), tempHand))
                                {


                                    // Utiliser TestPlateau pour essayer de placer le mot sur le plateau aux emplacement où la lettre du plateau se trouve au bon endroit du mot
                                    for (int k = 0; k < word.Length; k++)
                                    {
                                        if (word[k] == boardLetter.Item1)
                                        {
                                            int wordScore = 0;
                                            if (board.Test_Plateau(word, boardLetter.Item2, boardLetter.Item3 - k, '>', dictionary, this, ref wordScore, bag, false))
                                            {
                                                if (wordScore > bestScore)
                                                {
                                                    bestWord = word;
                                                    bestScore = wordScore;
                                                    bestLine = boardLetter.Item2;
                                                    bestColumn = boardLetter.Item3 - k;
                                                    bestDirection = '>';
                                                }
                                            }
                                            wordScore = 0;
                                            if (board.Test_Plateau(word, boardLetter.Item2 - k, boardLetter.Item3, 'v', dictionary, this, ref wordScore, bag, false))
                                            {
                                                if (wordScore > bestScore)
                                                {
                                                    bestWord = word;
                                                    bestScore = wordScore;
                                                    bestLine = boardLetter.Item2 - k;
                                                    bestColumn = boardLetter.Item3;
                                                    bestDirection = 'v';
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                }
            }

            Console.WriteLine("Mot trouvé par l'IA :");
            Console.WriteLine(bestWord);

            System.Threading.Thread.Sleep(2000);

            return new Tuple<string, int, int, char>(bestWord, bestLine, bestColumn, bestDirection);
        }

        /// <summary>
        /// Trouve les mots qui sont compatibles avec le pattern passé en paramètre
        /// </summary>
        /// <param name="pattern">Une ligne ou colonne du plateau</param>
        /// <param name="lettersOfHand">Les lettres de la main courante du joueur</param>
        /// <param name="dictionary">Le dictionnaire de mots</param>
        /// <returns></returns>
        List<Tuple<string, int>> WordsThatMatchPattern(string pattern, char[] lettersOfHand, Dictionnaire dictionary)
        {
            List<Tuple<string, int>> result = new List<Tuple<string, int>>();
            string followingChars = ""; //Les caractères à la suite dans le pattern
            for (int i = 0; i < pattern.Length;i++)
            {
                if (pattern[i] != ' ')
                {
                    followingChars += pattern[i];
                }
                if ((pattern [i] == ' ' || i == pattern.Length - 1) && followingChars.Length >= 1 )
                {
                    //On itère parmi les mots du dictionnaire pour trouver les mots compatibles avec le pattern et les caractères qui se suivent
                    foreach (string word in dictionary.ToList())
                    {
                        List<char> boardLetters = new List<char>(pattern.Replace(" ", "")); // Lest lettres du plateau se trouvant sur cette ligne/colonne
                        List<char> handLetters = new List<char>(lettersOfHand); // Les lettres de la main du joueur
                        if (word.Contains(followingChars) && HandAndBoardContainsLetters(word.ToCharArray(), handLetters, boardLetters))
                        {
                            // On teste toutes les positions du mot dans le pattern

                            for (int k = 0; k < pattern.Length; k++) //Position du mot dans le pattern
                            {
                                for (int l = 0; l < word.Length; l++) //Index de la lettre dans le mot
                                {
                                    if (k + l < pattern.Length && (pattern[k + l] == word[l] || pattern[k + l] == ' '))
                                    {
                                        if (l == word.Length - 1)
                                        {
                                            // Le mot match le pattern
                                            result.Add(new Tuple<string, int>(word, k));
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // Le mot ne match pas le pattern à la position k
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    followingChars = "";
                }
            }

            return result;
        }

        /// <summary>
        /// Vérifier si les lettres de la main et du plateau permettent d'écrire un certain mot
        /// </summary>
        /// <param name="letters">Le mot</param>
        /// <param name="tempHand">La main courante</param>
        /// <param name="tempBoardLetters">Les lettres du plateau</param>
        /// <returns></returns>
        bool HandAndBoardContainsLetters(char[] letters, List<char> tempHand, List<char> tempBoardLetters)
        {
            for (int i = 0; i < letters.Length; i++)
            {
                char letter = letters[i];
                if (tempBoardLetters.Contains(letter))
                {
                    tempBoardLetters.Remove(letter);
                }
                else if (tempHand.Contains(letter))
                {
                    tempHand.Remove(letter);
                }
                else if (tempHand.Contains('*'))
                {
                    tempHand.Remove('*');
                }
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Vérifier si la main courante permet d'écrire le mot spécifié sous forme de tableau de lettres
        /// </summary>
        /// <param name="letters">Le mot</param>
        /// <param name="tempHand">La main courante</param>
        /// <returns></returns>
        bool HandContainsLetters(char[] letters, List<char> tempHand)
        {
            char[] lettersWithAsterisks = (char[])letters.Clone();
            for (int i = 0; i < letters.Length; i++)
            {
                char letter = letters[i];
                if (tempHand.Contains(letter))
                {
                    tempHand.Remove(letter);
                }
                else if (tempHand.Contains('*'))
                {
                    tempHand.Remove('*');
                    lettersWithAsterisks[i] = '*';
                }
                else
                    return false;
            }


            return true;
        }
    }
}
