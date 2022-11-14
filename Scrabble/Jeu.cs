using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Timers;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;

namespace Scrabble
{
    /// <summary>
    /// Classe gérant le déroulé d'une partie de Scrabble
    /// </summary>
    static class Jeu
    {

        // Noms des fichiers/dossiers utilisés pour sauvegarder une partie
        const string SavedGameFolder = "SavedGame/";
        const string SavedPlayersFile = "players.csv";
        const string SavedBagFile = "bag.txt";
        const string SavedBoardFile = "board.txt";
        const string SavedSeedFile = "seed.txt";
        const string SavedTimePerTurnFile = "time.txt";
        const string SavedTurnFile = "turn.txt";

        static int timePerTurn = 60; // Le temps par tour en secondes (la valeur par défaut est changée en début de jeu)

        static Dictionnaire dictionary; // Le dictionnaire avec tous les mots autorisés
        static Plateau board; // Le plateau de jeu
        static Sac_Jetons bag; // Le sac de jetons

        static List<Joueur> joueurs; // La liste des joueurs en jeu

        static int randomSeed; // La graine du générateur de nombres aléatoires

        static Random rand; // Le générateur de nombres aléatoires

        static int timeLeft; // Le temps restant avant le tour suivant

        static bool isShowingError = false; // Un message d'erreur est il en train d'être affiché ?


        static int skippedTurns = 0; // Le nombre de joueurs ayant passé leur tour d'affilé

        static int turn; // Le numéro du joueur dont c'est le tour
        static int Turn
        {
            get { return turn; }
            set
            {
                turn = value;
                while (turn > joueurs.Count - 1)
                    turn -= joueurs.Count;
            }
        }

        static int turnIndex = 0;

        public static void Main(string[] args)
        {

            // Si une ancienne partie a été sauvegardée, demander au joueur s'il souhaite la charger
            bool loadGame = false;
            if (Directory.Exists(SavedGameFolder))
            {
                Console.WriteLine("Souhaitez-vous charger la dernière partie enregistrée ? (o/n)");
                loadGame = Console.ReadKey().KeyChar.ToString().ToLower() == "o";
            }

            // Créer le dictionnaire en français
            dictionary = new Dictionnaire("Francais.txt", "Francais");

            if (loadGame)
            {
                // Charger l'ancienne partie
                board = new Plateau(SavedGameFolder + SavedBoardFile);
                bag = new Sac_Jetons("Jetons.txt", SavedGameFolder + SavedBagFile);

                joueurs = GetPlayers(SavedGameFolder + SavedPlayersFile);
                randomSeed = int.Parse(File.ReadAllLines(SavedGameFolder + SavedSeedFile)[0]);
                timePerTurn = int.Parse(File.ReadAllLines(SavedGameFolder + SavedTimePerTurnFile)[0]);
                rand = new Random(randomSeed); // Seeds with * : 2, 4
                turn = int.Parse(File.ReadAllLines(SavedGameFolder + SavedTurnFile)[0]);
            }
            else
            {
                // Créer une nouvelle partie
                board = new Plateau();
                bag = new Sac_Jetons("Jetons.txt");

                joueurs = GetPlayers();

                Random seedRand = new Random();
                randomSeed = seedRand.Next();
                //randomSeed = 2; // Définir une seed spécifique (commenter cette ligne pour avoir des parties aléatoires)
                bool timeIsSet;
                do
                {
                    Console.WriteLine("Combien de temps durent les tours de jeu (en minutes) :");
                    timeIsSet = int.TryParse(Console.ReadLine(), out timePerTurn) && timePerTurn > 0;
                } while (!timeIsSet);
                timePerTurn *= 60;
                rand = new Random(randomSeed); // Seeds with * : 2, 4
                Turn = rand.Next(0, joueurs.Count);
            }

            // Commencer le timer qui actualise le temps restant à chaque seconde

            Timer t = new Timer();
            t.Interval = 1000;
            t.Elapsed += OnTickOfTimer;
            t.AutoReset = true;
            t.Enabled = true;


            // Lancer le premier tour de jeu. Cette fonction est récursive et s'appelera elle même ensuite.
            PlayTurn(joueurs[turn]);
            


            // Fin du jeu
            t.Enabled = false;
            Console.Clear();
            ShowBoard();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPlus personne ne peut jouer, le jeu est terminé !\n");
            Console.ResetColor();
            for (int i = 0; i < joueurs.Count;i++)
            {
                Console.WriteLine("Joueur " + (i + 1) + " (" + joueurs[i].Name + ")");
                Console.Write("Score : ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(joueurs[i].Score);
                Console.ResetColor();
                Console.Write("Nombre de mots trouvés : ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(joueurs[i].FoundWordsCount + "\n");
                Console.ResetColor();
            }
            Console.ReadKey();
        }

        /// <summary>
        /// Passer au tour suivant
        /// </summary>
        static void NextTurn()
        {
            // Si chaque joueur a passé son tour 3 fois d'affilé le jeu s'arrête
            if (skippedTurns < joueurs.Count*3)
            {
                Turn++;
                turnIndex++;
                PlayTurn(joueurs[Turn]);
            }
        }

        /// <summary>
        /// Jouer un tour de jeu
        /// </summary>
        /// <param name="joueur">Le joueur qui doit jouer</param>
        static void PlayTurn(Joueur joueur)
        {
            int thisTurnIndex = turnIndex;

            // Piocher des jetons si le joueur n'en a pas 7
            joueur.Draw(bag, rand);

            // Réinitialiser le chrono
            timeLeft = timePerTurn;

            string mot;
            int line;
            int column;
            char direction;
            int score = 0;

            // Si le joueur n'est pas une IA, lui demander l'action qu'il souhaite faire
            if (!(joueur is AI))
            {
                // Demander ce que le joueur souhaite faire tant que son action n'est pas valide
                do
                {
                    // Demander le mot à placer
                    do
                    {
                        ShowInterface(joueur);
                        Console.WriteLine("Quel mot souhaitez-vous entrer ?\n(Entrée pour passer votre tour, $ pour sauvegarder)");
                        mot = Console.ReadLine().ToUpper();
                        if (timeLeft <= 0)
                        {
                            ShowError("Le délai de temps a été dépassé, c'est au tour du joueur suivant");
                            skippedTurns++;
                            NextTurn();
                            return;
                        }
                        if (mot.Trim() == "$")
                        {
                            SaveGame();
                            ShowError("Le jeu a bien été sauvegardé");
                        }
                    } while (mot.Length < 2 && mot.Trim() != "");

                    mot = mot.Trim();

                    // Le joueur passe son tour si il n'a rien entré
                    if (mot == "")
                    {
                        skippedTurns++;
                        NextTurn();
                        return;
                    }

                    // Demander la colonne et la ligne où placer le mot jusqu'à ce que cette position soit valide
                    do
                    {
                        ShowInterface(joueur);
                        Console.WriteLine("Entrez la colonne et la ligne (exemple : C12) :");
                        string coordinates = Console.ReadLine().ToUpper();
                        if (timeLeft <= 0)
                        {
                            ShowError("Le délai de temps a été dépassé, c'est au tour du joueur suivant");
                            skippedTurns++;
                            NextTurn();
                            return;
                        }
                        coordinates = coordinates.Trim();
                        string lineString = "";
                        string columnString = "";
                        foreach (char element in coordinates)
                        {
                            if (char.IsDigit(element))
                                lineString += element;
                            if (char.IsLetter(element))
                                columnString += element;
                        }
                        if (columnString.Length == 0 || lineString.Length == 0 || columnString.Length > 1 || lineString.Length > 2 || coordinates.Length != lineString.Length + columnString.Length)
                        {
                            ShowError("Vos coordonnées ne sont pas dans le bon format");
                            continue;
                        }

                        line = int.Parse(lineString) - 1;
                        column = (int)columnString[0] - (int)'A';
                        break;
                    } while (true);

                    // Demander la direction dans laquelle entrer le mot
                    do
                    {
                        ShowInterface(joueur);
                        Console.WriteLine("Entrez la direction (>/v) :");
                        ConsoleKeyInfo key = Console.ReadKey();
                        if (timeLeft <= 0)
                        {
                            ShowError("Le délai de temps a été dépassé, c'est au tour du joueur suivant");
                            skippedTurns++;
                            NextTurn();
                            return;
                        }
                        if (key.Key == ConsoleKey.RightArrow || key.KeyChar == '>')
                            direction = '>';
                        else if (key.Key == ConsoleKey.DownArrow || key.KeyChar == 'v')
                            direction = 'v';
                        else
                        {
                            ShowError("La direction n'est pas valide (>/v)");
                            continue;
                        }
                        break;
                    }
                    while (true);
                } while (!board.Test_Plateau(mot, line, column, direction, dictionary, joueur, ref score, bag)); // Tester si le mot peut être placé et si oui enregistrer son score
            }
            else // Le joueur est une IA
            {
                ShowInterface(joueur);
                Console.WriteLine("L'IA est en train de réfléchir...");
                Tuple<string, int, int, char> action = (joueur as AI).GetAction(board, dictionary, bag); // Demander à l'IA l'actio qu'elle estime être la meilleure
                mot = action.Item1;
                line = action.Item2;
                column = action.Item3;
                direction = action.Item4;

                // Si l'IA n'a rien trouvé, passer son tour
                if (mot == null || !board.Test_Plateau(mot, line, column, direction, dictionary, joueur, ref score, bag))
                {
                    skippedTurns++;
                    NextTurn();
                    return;
                }
                
            }

            skippedTurns = 0; // Si le joueur a pu jouer, réinitialiser le compteur de tours passés d'affilé

            joueur.Add_Score(score); // Ajouter le score de ce tour au score du joueur
            joueur.Add_Mot(mot); // Ajouter le mot trouvé aux mots trouvés du joueur

            // Placer le mot sur le plateau et retirer les jetons utilisés
            board.PlaceWord(mot, line, column, direction);
            foreach (char letter in mot)
            {
                Jeton tokenToRemove = joueur.Hand.Find(x => x.Letter == letter);
                if (tokenToRemove == null)
                    tokenToRemove = joueur.Hand.Find(x => x.Letter == '*');
                joueur.Remove_Main_Courante(tokenToRemove);
            }

            // Passer au tour suivant
            NextTurn();
        }

        /// <summary>
        /// Afficher un message d'erreur ou une information au joueur au cours de la partie
        /// </summary>
        /// <param name="error">Le message à afficher</param>
        public static void ShowError(string error)
        {
            isShowingError = true;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
            Console.ReadKey();
            isShowingError = false;
        }

        /// <summary>
        /// Fonction appelée toutes les secondes par le timer
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e">Les données de l'évènement déclenché par le timer</param>
        private static void OnTickOfTimer(Object source, ElapsedEventArgs e)
        {
            if (timeLeft > 0)
                timeLeft--; // Réduire le temps restant
            if (!isShowingError)
            {
                if (Console.WindowWidth >= 55 && Console.WindowHeight >= 35)
                {
                    int cursorLeft = Console.CursorLeft;
                    int cursorTop = Console.CursorTop;
                    Console.SetCursorPosition(0, Console.WindowTop);
                    Console.Write(new string(' ', Console.WindowWidth - 5));
                    Console.SetCursorPosition(0, Console.WindowTop);
                    Console.Write("Il vous reste " + timeLeft + "s");
                    Console.SetCursorPosition(cursorLeft, cursorTop);
                }
            }
        }

        /// <summary>
        /// Afficher l'interface (temps restant, plateau, main courante, score) pour le joueur qui doit jouer
        /// </summary>
        /// <param name="joueur">Le joueur qui doit jouer</param>
        static void ShowInterface(Joueur joueur)
        {
            Console.Clear();
            Console.WriteLine("Il vous reste " + timeLeft + "s");
            ShowBoard();

            Console.WriteLine("\nC'est au tour du joueur " + (turn + 1) + " (" + joueur.Name + ") :");

            Console.WriteLine("\nVotre score : " + joueur.Score);

            Console.WriteLine("\nVotre main :");

            // Afficher la main courante
            foreach (Jeton jeton in joueur.Hand)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(" "+jeton.Letter+" ");
                Console.ResetColor();
                Console.Write("  ");
            }
            Console.WriteLine();
            foreach (Jeton jeton in joueur.Hand)
            {
                Console.Write("(" + jeton.Score.ToString() + ")");
                if (jeton.Score < 10)
                    Console.Write("  ");
                else
                    Console.Write(" ");
            }
            
            Console.WriteLine("\n");

            
        }

        /// <summary>
        /// Afficher le plateau
        /// </summary>
        static void ShowBoard()
        {

            Console.WriteLine();
            Console.WriteLine("   A B C D E F G H I J K L M N O");
            Console.WriteLine();

            string[] boardArray = board.ToString().Split('\n');
            for (int i = 0; i < board.EmptyBoard.GetLength(0); i++)
            {
                Console.ResetColor();
                if (i + 1 <= 9)
                    Console.Write((i + 1) + "  ");
                else
                    Console.Write((i + 1) + " ");
                for (int j = 0; j < board.EmptyBoard.GetLength(1); j++)
                {
                    // Changer la couleur de la case
                    Console.ForegroundColor = ConsoleColor.Black;
                    if (boardArray[i][j] == ' ')
                    {
                        switch (board.EmptyBoard[i, j])
                        {
                            default:
                            case '1':
                                Console.BackgroundColor = ConsoleColor.Green;
                                break;

                            case '2':
                                Console.BackgroundColor = ConsoleColor.Blue;
                                break;

                            case '3':
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                break;

                            case '4':
                                Console.BackgroundColor = ConsoleColor.Magenta;
                                break;

                            case '5':
                                Console.BackgroundColor = ConsoleColor.Red;
                                break;
                        }
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                    }

                    // Afficher la lettre posée sur la case
                    Console.Write(boardArray[i][j] + " ");
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Obtenir la liste des joueurs auprès de l'utilisateur
        /// </summary>
        /// <returns>La liste des joueurs présents pour la partie</returns>
        static List<Joueur> GetPlayers()
        {
            List<Joueur> joueurs = new List<Joueur>();
            Console.Clear();
            Console.WriteLine("Pour ajouter une IA au jeu, donnez le nom \"ordi\" au joueur");
            for (int i = 1; i <= 4; i++)
            {
                if (i <= 2)
                    Console.WriteLine("Entrez le nom du joueur " + i + " : ");
                else
                    Console.WriteLine("Entrez le nom du joueur " + i + " (tapez la touche Entrée si tous les joueurs ont déjà été ajoutés) : ");

                // Récupérer le nom et enlever les caractères interdits
                string name = Console.ReadLine().Replace(",", "").Replace(";", "").Replace(" ", "");
                if (name == "")
                {
                    if (i > 2)
                        break;
                    else
                    {
                        Console.WriteLine("Il doit y avoir au moins deux joueurs");
                        i--;
                        continue;
                    }
                }

                if (name.ToLower() == "ordi")
                    joueurs.Add(new AI(name));
                else
                    joueurs.Add(new Joueur(name));
            }

            Console.Clear();
            Console.WriteLine(joueurs.Count + " joueurs sont en jeu :");
            for (int i = 0; i < joueurs.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + joueurs[i].Name);
            }
            Console.WriteLine();

            return joueurs;
        }

        /// <summary>
        /// Obtenir la liste des joueurs d'une partie sauvegardée
        /// </summary>
        /// <param name="filepath">Le nom du fichier où sont sauvegardés les joueurs</param>
        /// <returns></returns>
        static List<Joueur> GetPlayers(string filepath)
        {
            List<Joueur> joueurs = new List<Joueur>();

            string[] playersStrings = File.ReadAllLines(filepath);

            for (int i = 1; i < playersStrings.Length; i++)
            {
                // Enregistrer les données de chaque joueur dans un Dictionary et créer le joueur
                Dictionary<string, string> playerData = new Dictionary<string, string>();
                string[] labels = playersStrings[0].Split(',');
                string[] playerStrings = playersStrings[i].Split(',');
                for (int j = 0; j < labels.Length; j++)
                {
                    playerData.Add(labels[j], playerStrings[j]);
                }

                if (playerData["name"].ToLower() == "ordi")
                    joueurs.Add(new AI(playerData));
                else
                    joueurs.Add(new Joueur(playerData));
            }

            return joueurs;
        }

        /// <summary>
        /// Sauvegarde la partie dans un dossier
        /// </summary>
        static void SaveGame()
        {
            // Créer le dossier s'il n'existe pas déjà
            System.IO.Directory.CreateDirectory(SavedGameFolder);

            //Sauvegarder les joueurs
            StreamWriter playersWriter = File.CreateText(SavedGameFolder + SavedPlayersFile);
            playersWriter.WriteLine("name,score,foundWords,hand");
            foreach (Joueur joueur in joueurs)
            {
                playersWriter.WriteLine(joueur);
            }
            playersWriter.Close();

            //Sauvegarder le plateau
            StreamWriter boardWriter = File.CreateText(SavedGameFolder + SavedBoardFile);
            boardWriter.Write(board.ToString());
            boardWriter.Close();

            //Sauvegarder le sac de jetons
            StreamWriter bagWriter = File.CreateText(SavedGameFolder + SavedBagFile);
            bagWriter.Write(bag.ToString());
            bagWriter.Close();

            //Sauvegarder la seed
            File.WriteAllText(SavedGameFolder + SavedSeedFile, randomSeed.ToString());

            //Sauvegarder le temps entre chaque tour
            File.WriteAllText(SavedGameFolder + SavedTimePerTurnFile, timePerTurn.ToString()) ;

            //Sauvegarder le tour actuel
            File.WriteAllText(SavedGameFolder + SavedTurnFile, Turn.ToString());
        }
    }
}