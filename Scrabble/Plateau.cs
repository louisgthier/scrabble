using System;
using System.IO;
using System.Collections.Generic;

namespace Scrabble
{
    public class Plateau
    {
        const string BoardTemplateFilepath = "BoardTemplate.txt";

        char[,] board; // Matrice contenant toutes les lettres du plateau
        public char[,] Board { get { return board; } }
        char[,] emptyBoard; // Matrice contenant le poids de chaque case
        public char[,] EmptyBoard { get { return emptyBoard; } }

        /// <summary>
        /// Créer un plateau vide
        /// </summary>
        public Plateau()
        {
            board = new char[15, 15];
            emptyBoard = new char[15, 15];

            string[] boardTemplate = File.ReadAllLines(BoardTemplateFilepath);

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = ' ';
                    emptyBoard[i, j] = boardTemplate[i][j];
                }
            }
        }

        /// <summary>
        /// Récupérer un plateau à partir d'un fichier
        /// </summary>
        /// <param name="filepath"></param>
        public Plateau(string filepath)
        {
            board = new char[15, 15];
            emptyBoard = new char[15, 15];

            string[] boardTemplate = File.ReadAllLines(BoardTemplateFilepath);
            string[] savedBoard = File.ReadAllLines(filepath);

            for (int i = 0; i < board.GetLength(0); i++)
            {
                savedBoard[i] = savedBoard[i].Replace(";", "").Replace("_", " ");
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = savedBoard[i][j];
                    emptyBoard[i, j] = boardTemplate[i][j];
                }
            }
        }



        /// <summary>
        /// Vérifie si un mot peut bien être placé et si oui retourne true et change la valeur de la variable score
        /// </summary>
        /// <param name="mot">Le mot à placer</param>
        /// <param name="ligne">La ligne où placer la première lettre du mot</param>
        /// <param name="colonne">La colonne où placer la première lettre du mot</param>
        /// <param name="direction">La direction du mot</param>
        /// <param name="dictionnaire">Le dictionnaire où chercher le mot</param>
        /// <param name="joueur">Le joueur qui place le mot</param>
        /// <param name="score">Une référence vers la variable score où sera sauvegardé le score de ce mot</param>
        /// <param name="bag">Le sac de jetons d'où proviennent les jetons</param>
        /// <param name="debug">Est-ce que les erreur doivent être affichées</param>
        /// <returns></returns>
        public bool Test_Plateau(string mot, int ligne, int colonne, char direction, Dictionnaire dictionnaire, Joueur joueur, ref int score, Sac_Jetons bag, bool debug = true)
        {
            mot = mot.ToUpper();

            // Vérifier que la direction est correcte
            if (direction != '>' && direction != 'v')
            {
                if (debug)
                    Jeu.ShowError("La direction n'est pas valide");
                return false;
            }


            // Vérifier que le mot appartient bien au dictionnaire
            if (!dictionnaire.RechDichoRecursif(mot))
            {
                if (debug)
                    Jeu.ShowError("Le mot n'appartient pas au dictionnaire");
                return false;
            }

            // Vérifier que la première lettre du mot ne sort pas du plateau
            if (ligne < 0 || ligne >= board.GetLength(0) || colonne < 0 || colonne >= board.GetLength(1))
            {
                if (debug)
                    Jeu.ShowError("Le mot sort du plateau");
                return false;
            }

            // Vérifier que la dernière lettre du mot ne sort pas du plateau
            int ligneFin;
            int colonneFin;
            if (direction == '>')
            {
                ligneFin = ligne;
                colonneFin = colonne + mot.Length - 1;
            }
            else
            {
                ligneFin = ligne + mot.Length - 1;
                colonneFin = colonne;
            }
            if (ligneFin < 0 || ligneFin >= board.GetLength(0) || colonneFin < 0 || colonneFin >= board.GetLength(1))
            {
                if (debug)
                    Jeu.ShowError("Le mot sort du plateau");
                return false;
            }

            // Vérifier qu'il n'y a aucune lettre avant et après le mot
            if (direction == '>')
            {
                if ((colonne - 1 >= 0 && board[ligne, colonne - 1] != ' ') || (colonneFin + 1 < board.GetLength(1) && board[ligne, colonneFin + 1] != ' '))
                {
                    if (debug)
                        Jeu.ShowError("Des lettres se trouvent déjà avant ou après les mot, veuillez entrer le mot en entier");
                    return false;
                }
            }
            else
            {
                if ((ligne - 1 >= 0 && board[ligne - 1, colonne] != ' ') || (ligneFin + 1 < board.GetLength(0) && board[ligneFin + 1, colonne] != ' '))
                {
                    if (debug)
                        Jeu.ShowError("Des lettres se trouvent déjà avant ou après les mot, veuillez entrer le mot en entier");
                    return false;
                }
            }

            // Vérifier que le mot peut être placé
            bool canBePlaced = true;
            List<Tuple<int, int, char>> newLetters = new List<Tuple<int, int, char>>(); // Letters that were not on the board before

            switch (direction)
            {
                case 'v':
                    int k = 0;
                    for (int i = ligne; i <= ligne + mot.Length - 1; i++)
                    {

                        if (board[i, colonne] == ' ')
                        {
                            newLetters.Add(new Tuple<int, int, char>(i, colonne, mot[k]));
                        }
                        else if (mot[k] != board[i, colonne])
                        {
                            canBePlaced = false;
                            break;
                        }
                        k++;
                    }
                    break;

                default:
                case '>':
                    k = 0;
                    for (int j = colonne; j <= colonne + mot.Length - 1; j++)
                    {

                        if (board[ligne, j] == ' ')
                        {
                            newLetters.Add(new Tuple<int, int, char>(ligne, j, mot[k]));
                        }
                        else if (mot[k] != board[ligne, j])
                        {
                            canBePlaced = false;
                            break;
                        }
                        k++;
                    }
                    break;
            }

            // Le mot n'ajoute aucune lettre au plateau
            if (newLetters.Count == 0)
            {
                if (debug)
                    Jeu.ShowError("Aucune lettre n'a été ajoutée au plateau");
                return false;
            }

            if (!canBePlaced)
            {
                if (debug)
                    Jeu.ShowError("Le mot ne peut pas être placé sur le plateau à cet endroit");
                return false;
            }

            // Vérifier que le joueur a les jetons nécessaires pour placer le mot
            List<char> hand = new List<char>();
            char[] wordWithAsterisks = mot.ToCharArray();
            foreach (Jeton jeton in joueur.Hand)
            {
                hand.Add(jeton.Letter);
            }
            foreach (Tuple<int, int, char> newLetter in newLetters)
            {
                char letter = newLetter.Item3;
                if (hand.Contains(letter))
                {
                    hand.Remove(letter);
                    
                }
                else if (hand.Contains('*') && char.IsLetter(letter))
                {
                    hand.Remove('*');

                    int indexOfLetter = Math.Max(newLetter.Item1-ligne,newLetter.Item2-colonne);

                    wordWithAsterisks[indexOfLetter] = '*';

                }
                else
                {
                    if (debug)
                        Jeu.ShowError("Vous ne possédez pas tous les jetons nécessaires pour placer ce mot");
                    return false;
                }
            }

            // Vérifier que les mots entourant le nouveau mot existent (on place temporairement le mot sur un nouveau board)
            char[,] tempBoard = (char[,])board.Clone();

            List<Tuple<int, string>> newWords = new List<Tuple<int, string>>();

            foreach (Tuple<int, int, char> newLetter in newLetters)
            {
                tempBoard[newLetter.Item1, newLetter.Item2] = newLetter.Item3;
            }

            foreach (Tuple<int, int, char> newLetter in newLetters)
            {
                newWords.AddRange(GetWordAtPosition(newLetter.Item1, newLetter.Item2, tempBoard, direction));
            }

            foreach (Tuple<int, string> newWord in newWords)
            {
                string word = newWord.Item2;
                if (!dictionnaire.RechDichoRecursif(word))
                {
                    if (debug)
                        Jeu.ShowError("Tous les nouveaux mots créés n'appartiennent pas au dictionnaire");
                    return false;
                }
            }

            // Vérifier que au moins une lettre du mot est située à côté d'une lettre déjà posée
            int tempLine = ligne;
            int tempColumn = colonne;
            bool isNextToAnotherLetter = false;
            for (int i = 0; i < mot.Length; i++)
            {
                // Check the cases next to the letter
                if (GetCase(tempLine+1, tempColumn) != ' ' || GetCase(tempLine, tempColumn+1) != ' ' || GetCase(tempLine - 1, tempColumn) != ' ' || GetCase(tempLine, tempColumn - 1) != ' ')
                {
                    isNextToAnotherLetter = true;
                    break;
                }

                // Check if the word is on the center of the board
                if (tempColumn == (board.GetLength(0)-1) / 2 && tempLine == (board.GetLength(0) - 1) / 2)
                {
                    isNextToAnotherLetter = true;
                    break;
                }

                if (direction == '>')
                {
                    tempColumn++;
                }
                else
                {
                    tempLine++;
                }
            }

            if (!isNextToAnotherLetter)
            {
                if (debug)
                    Jeu.ShowError("Le mot n'est pas à côté d'un mot existant ou ne touche pas la case du milieu");
                return false;
            }

            score = CalculateScore(mot, ligne, colonne, direction, newWords, newLetters, bag, wordWithAsterisks);

            //ToDo Retourner le bon nombre de mots ajoutés

            return true;
        }


        /// <summary>
        /// Returns the char placed on the board at the specified line and column. Returns a whitespace if coordinates are outside of the board.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <returns>The char on the board if it exists, a whitespace otherwise</returns>
        public char GetCase(int line, int column)
        {
            if (line >= 0 && column >= 0 && line < board.GetLength(0) && column < board.GetLength(1))
                return board[line, column];
            else
                return ' ';
        }

        /// <summary>
        /// Calculer le score d'un mot
        /// </summary>
        /// <param name="word">Le mot</param>
        /// <param name="line">La ligne du mot</param>
        /// <param name="column">La colonne du mot</param>
        /// <param name="direction">La direction du mot</param>
        /// <param name="newWords">Les nouveaux mots formés</param>
        /// <param name="newLetters">Les lettres ajoutées au plateau</param>
        /// <param name="bag">Le sac de jetons</param>
        /// <param name="wordWithAsterisks">Le mot avec les jokers aux bons emplacements</param>
        /// <returns></returns>
        int CalculateScore(string word, int line, int column, char direction, List<Tuple<int, string>> newWords, List<Tuple<int, int, char>> newLetters, Sac_Jetons bag, char[] wordWithAsterisks)
        {
            int score = 0;

            //ToDo Donner la possiblité de choisir où placer les Jokers dans le mot

            // Calculer le score du mot ajouté
            int tempLine = line;
            int tempColumn = column;
            int wordScore = 0;
            int wordMultiplicator = 1;

            for (int i = 0; i < word.Length; i++)
            {
                int letterMultiplicator = 1;
                if (emptyBoard[tempLine, tempColumn] == '2' && board[tempLine, tempColumn] == ' ')
                    letterMultiplicator = 2;
                if (emptyBoard[tempLine, tempColumn] == '3' && board[tempLine, tempColumn] == ' ')
                    letterMultiplicator = 3;
                if (emptyBoard[tempLine, tempColumn] == '4' && board[tempLine, tempColumn] == ' ')
                    wordMultiplicator *= 2;
                if (emptyBoard[tempLine, tempColumn] == '5' && board[tempLine, tempColumn] == ' ')
                    wordMultiplicator *= 3;

                wordScore += bag.GetScore(wordWithAsterisks[i]) * letterMultiplicator;

                if (direction == '>')
                    tempColumn++;
                else
                    tempLine++;
            }
            score += wordScore * wordMultiplicator;

            // Calculer le score de tous les mots complétés par les nouvelles lettres
            foreach (Tuple<int, string> newWord in newWords)
            {
                wordScore = 0;
                wordMultiplicator = 1;
                int letterMultiplicator = 1;


                char addedLetter = ' '; // La lettre qui a été ajoutée pour compléter le mot
                bool isJoker = false;

                if (direction == '>')
                {
                    tempColumn = newWord.Item1;
                    tempLine = line;

                    foreach (Tuple<int, int, char> newLetter in newLetters)
                    {
                        if (newLetter.Item2 == tempColumn)
                        {
                            int indexOfLetter = Math.Max(newLetter.Item1 - line, newLetter.Item2 - column);
                            addedLetter = newLetter.Item3;
                            if (wordWithAsterisks[indexOfLetter] == '*')
                            {
                                isJoker = true;
                            }

                            break;
                        }

                    }
                }
                if (direction == 'v')
                {
                    tempLine = newWord.Item1;
                    tempColumn = column;

                    foreach (Tuple<int, int, char> newLetter in newLetters)
                    {
                        if (newLetter.Item1 == tempLine)
                        {
                            int indexOfLetter = Math.Max(newLetter.Item1 - line, newLetter.Item2 - column);
                            addedLetter = newLetter.Item3;
                            if (wordWithAsterisks[indexOfLetter] == '*')
                            {
                                isJoker = true;
                            }
                            break;
                        }
                    }
                }

                if (emptyBoard[tempLine, tempColumn] == '2' && board[tempLine, tempColumn] == ' ')
                    letterMultiplicator = 2;
                if (emptyBoard[tempLine, tempColumn] == '3' && board[tempLine, tempColumn] == ' ')
                    letterMultiplicator = 3;
                if (emptyBoard[tempLine, tempColumn] == '4' && board[tempLine, tempColumn] == ' ')
                    wordMultiplicator *= 2;
                if (emptyBoard[tempLine, tempColumn] == '5' && board[tempLine, tempColumn] == ' ')
                    wordMultiplicator *= 3;

                foreach (char element in newWord.Item2)
                {
                    if (element == addedLetter)
                    {
                        if (!isJoker)
                        {
                            wordScore += bag.GetScore(element) * letterMultiplicator;
                        }
                        addedLetter = ' ';
                    }
                    else
                    {
                        wordScore += bag.GetScore(element);
                    }
                }
                score += wordScore * wordMultiplicator;
            }

            // Ajouter 50 points si le joueur fait un SCRABBLE
            if(newLetters.Count == 7)
            {
                score += 50;
            }

            return score;
        }

        /// <summary>
        /// Placer un mot sur le plateau
        /// </summary>
        /// <param name="mot"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <param name="direction"></param>
        public void PlaceWord(string mot, int line, int column, char direction)
        {
            for (int i = 0; i < mot.Length; i++)
            {
                board[line, column] = mot[i];
                if (direction == '>')
                {
                    column++;
                }
                else
                {
                    line++;
                }
            }
        }


        /// <summary>
        /// Récupérer un mot sur un plateau temporaire à un emplacement et selon un axe
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <param name="board"></param>
        /// <param name="placedWordDirection"></param>
        /// <returns></returns>
        public static List<Tuple<int, string>> GetWordAtPosition(int line, int column, char[,] board, char placedWordDirection)
        {
            List<Tuple<int, string>> words = new List<Tuple<int, string>>();

            if (board[line, column] == ' ')
                return words;

            if (placedWordDirection == '>')
            {
                //Mot vertical
                int startLine = line;
                while (startLine - 1 >= 0 && board[startLine - 1, column] != ' ')
                {
                    startLine--;
                }
                int endLine = line;
                while (endLine + 1 < board.GetLength(0) && board[endLine + 1, column] != ' ')
                {
                    endLine++;
                }

                string verticalWord = "";
                for (int i = startLine; i <= endLine; i++)
                {
                    verticalWord += board[i, column];
                }

                if (verticalWord.Length > 1)
                    words.Add(new Tuple<int, string>(column, verticalWord));
            }

            if (placedWordDirection == 'v')
            {
                //Mot horizontal
                int startColumn = column;
                while (startColumn - 1 >= 0 && board[line, startColumn - 1] != ' ')
                {
                    startColumn--;
                }
                int endColumn = column;
                while (endColumn + 1 < board.GetLength(1) && board[line, endColumn + 1] != ' ')
                {
                    endColumn++;
                }

                string horizontalWord = "";
                for (int j = startColumn; j <= endColumn; j++)
                {
                    horizontalWord += board[line, j];
                }

                if (horizontalWord.Length > 1)
                    words.Add(new Tuple<int, string>(line, horizontalWord));
            }

            return words;

        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    result += board[i, j];
                }
                if (i < board.GetLength(1) - 1)
                    result += "\n";
            }

            return result;
        }
    }
}
