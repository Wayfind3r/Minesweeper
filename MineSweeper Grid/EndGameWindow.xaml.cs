using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MineSweeper_Grid
{
    /// <summary>
    /// Interaction logic for You_Win.xaml
    /// </summary>
    public partial class EndGameWindow : Window
    {
        private int time;
        private EndGameState gameState;
        private SortedList<int, string> currentScores = new SortedList<int, string>(); 
        public EndGameWindow(int yourTime, EndGameState state)
        {
            time = yourTime;
            gameState = state;
            InitializeComponent();
        }

        //ctor for High Scores
        public EndGameWindow(int yourTime, EndGameState state, ref SortedList<int, string> scores) :this (yourTime,state)
        {
            if (scores != null)
            {
                currentScores = scores;
            }

        }
        
        //Handle all 3 end-game scenarios
        public void YouWin_OnLoaded(object sender, RoutedEventArgs e)
        {
            switch (gameState)
            {
                    case EndGameState.Win:
                {
                    CropWindow();
                    Height = 120;
                    Title = "YOU WIN!";
                    WinTextBlock.Text = String.Format("You win! Your time was {0:00}:{1:00}", time/60, time%60);
                    break;
                }
                case EndGameState.HighScore:
                {
                    Title = "NEW HIGH SCORE!";
                    WinTextBlock.Text = String.Format(
                        "You win! Your time was {0:00}:{1:00}",
                        time/60, time%60);
                    break;
                }
                    case EndGameState.GameOver:
                {
                    CropWindow();
                    Width = 150;
                    Height = 120;
                    Title = "GAME OVER";
                    WinTextBlock.Text = "Game Over.";
                    break;
                }
            }
            
        }

        public void CropWindow()
        {
            ContentGrid.Children.Remove(NameBox);
            ContentGrid.Children.Remove(EnterYourName);
            ContentGrid.Children.Remove(HighScoreBlock);
            ContentGrid.RowDefinitions.Remove(NameBoxRow);
            ContentGrid.RowDefinitions.Remove(EnterYourNameRow);
            ContentGrid.RowDefinitions.Remove(HighScoreRow);
        }

        //Close window, Record name and write scores files
        private void OkCloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (gameState == EndGameState.HighScore)
            {
                if (NameBox.Text != null)
                {
                    currentScores[time] = NameBox.Text;
                }
                while (currentScores.Count > 3)
                {
                    currentScores.Remove(currentScores.Last().Key);
                }
                using (StreamWriter file = new StreamWriter("scores.txt", false))
                {
                    foreach (var score in currentScores)
                    {
                        file.WriteLine(score.Key + "," + score.Value);
                    }
                }
            }
            this.Close();
        }
    }
}
