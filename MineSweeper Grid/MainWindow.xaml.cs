using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MineSweeper_Grid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MineSweeperArray sweeperArray;
        private static bool isGridActive;//true: Grid_OnLoaded <- Reset_Button_OnClick ; false: GridLeftClickTrueEvent

        private int time;
        private DispatcherTimer Timer;

        private int nonMineCellsRevealed;
        private SortedList<int, string> highScoresList = new SortedList<int, string>();
        public MainWindow()
        {
            InitializeComponent();
            InitializeHighScores();
            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        //CSV read from scores.txt Only first 3 scores are taken
        private void InitializeHighScores()
        {
            string line;
            int counter = 0;
            int currentKey = 0;

            using (StreamReader highscorestxt = new StreamReader("scores.txt", false))
            {
                while ((line = highscorestxt.ReadLine()) != null)
                {
                    counter++;
                    String[] values = line.Split(new char[] { ',' });
                    if (values != null)
                    {
                        if (int.TryParse(values[0], out currentKey) && currentKey < 6000 && currentKey > 0)
                        {
                            highScoresList.Add(currentKey, null);
                            if (values.Length > 1)
                            {
                                string fullName = null;
                                for (int i = 1; i < values.Length; i++)
                                {
                                    fullName += values[i];
                                    if (i + 1 < values.Length)
                                    {
                                        fullName += ",";
                                    }
                                }
                                highScoresList[currentKey] = fullName;
                            }
                            else
                            {
                                highScoresList[currentKey] = "Unknown";
                            }
                        }
                        else
                        { continue; }
                    }
                    if (highScoresList.Count > 2)
                    {
                        break;
                    }
                }
            }
        }

        //Timer stops at 60:59
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (time < 5998)
            {
                time++;
                GameTimer.Content = string.Format("{0:00}:{1:00}", time / 60, time % 60);
            }
            else
            {
                GameTimer.Foreground = Brushes.Red;
                Timer.Stop();
            }
        }

        private const string ImgNameMine = @"mine.jpg";
        private const string ImgNameFlag = @"flag.png";
        private static Image CreateImage(string imgPath)
        {
            Image Mine = new Image();
            Mine.Width = 15;
            Mine.Height = 15;
            ImageSource MineImg = new BitmapImage(new Uri(imgPath, UriKind.Relative));
            Mine.Source = MineImg;
            return Mine;
        }

        //Initialize SweeperGrid on startup and Reset;
        private void Grid_OnLoaded(object sender, RoutedEventArgs e)
        {
            GameTimer.Foreground = Brushes.Black;
            time = 0;
            Timer.Start();
            isGridActive = true;
            nonMineCellsRevealed = 0;
            sweeperArray = new MineSweeperArray();
            //var ourGrid = sender as Grid;
            Grid ourGrid = SweeperGrid;
            ourGrid.Children.Clear();
            ourGrid.RowDefinitions.Clear();
            ourGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < sweeperArray.NumberOfCellsX; i++)
            {
                var newColumn = new ColumnDefinition();
                ourGrid.ColumnDefinitions.Add(newColumn);
            }
            for (int i = 0; i < sweeperArray.NumberOfCellsY; i++)
            {
                var newRow = new RowDefinition();
                ourGrid.RowDefinitions.Add(newRow);
            }

            for (var r = 0; r < sweeperArray.NumberOfCellsY; r++)
            {
                for (var c = 0; c < sweeperArray.NumberOfCellsX; c++)
                {
                    var newRectangle = new Rectangle { Stroke = Brushes.Black, Fill = Brushes.LightSlateGray };
                    Grid.SetColumn(newRectangle, c);
                    Grid.SetRow(newRectangle, r);
                    ourGrid.Children.Add(newRectangle);
                }
            }
        }

        private void Reset_Button_OnClick(object sender, RoutedEventArgs e)
        {
            ResetButton.BorderBrush = Brushes.Orange;
            Grid_OnLoaded(SweeperGrid, e);
        }
        //For "better" looks
        private void ResetButton_ColorChange(object sender, RoutedEventArgs e)
        {
            ResetButton.BorderBrush = Brushes.Black;
        }


        //Handle Left, Right and both Mouse buttons at the same time
        public void GridClick(object sender, MouseButtonEventArgs e)
        {
            if (isGridActive)//true: Grid_OnLoaded <- Reset_Button_OnClick ; false: GridLeftClickTrueEvent
            {
                var leftPressed = false;
                var rightPressed = false;
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                    leftPressed = true;
                if (Mouse.RightButton == MouseButtonState.Pressed)
                    rightPressed = true;

                if (leftPressed && rightPressed)
                {
                    LeftNRightClickTrueEvent(sender, e);
                    return;
                }
                if (leftPressed)
                {
                    GridLeftClickTrueEvent(sender, e);
                    return;
                }
                GridRightClickTrueEvent(sender, e);
            }
        }

        //Left+Right Click, if clicked cell is revealed or empty -> return
        //Chain reveal empty cells and their neighboring cells
        public void LeftNRightClickTrueEvent(object sender, MouseButtonEventArgs e)
        {
            var ourGrid = sender as Grid;
            int col = sweeperArray.GetCellColumn(ourGrid, e);
            int row = sweeperArray.GetCellRow(ourGrid, e);
            if (sweeperArray.GetTag(row, col) != MineTag.Revealed || sweeperArray.GetValue(row, col) < 1)
            {
                return;
            }
            int valueOfCell = sweeperArray.GetValue(row, col);
            int numberOfFlaggedCells = 0;
            for (int r = row - 1; r < row + 2; r++)
            {
                for (int c = col - 1; c < col + 2; c++)
                {
                    if (c < 0 || c > sweeperArray.NumberOfCellsX - 1 || r < 0 || r > sweeperArray.NumberOfCellsY - 1)
                        continue;
                    if (sweeperArray.GetTag(r, c) == MineTag.Flagged)
                    {
                        numberOfFlaggedCells++;
                    }
                }
            }
            if (numberOfFlaggedCells == valueOfCell)
            {
                for (int r = row - 1; r < row + 2; r++)
                {
                    for (int c = col - 1; c < col + 2; c++)
                    {
                        if (c < 0 || c > sweeperArray.NumberOfCellsX - 1 || r < 0 || r > sweeperArray.NumberOfCellsY - 1)
                            continue;
                        RevealCell(ourGrid, r, c);
                        if (sweeperArray.GetValue(r, c) == 0)
                        {
                            PopEmptyCellsRecursive(ourGrid, r, c);
                        }
                    }
                }
            }
        }
        //Remove flag or place flag
        public void GridRightClickTrueEvent(object sender, MouseButtonEventArgs e)
        {
            //Grid size 400x400 Cells 16x16
            var ourGrid = sender as Grid;
            int col = sweeperArray.GetCellColumn(ourGrid, e);
            int row = sweeperArray.GetCellRow(ourGrid, e);

            if (sweeperArray.GetTag(row, col) == MineTag.Flagged)
            {
                Rectangle newRectangle = new Rectangle { Stroke = Brushes.Black, Fill = Brushes.LightSlateGray };
                Grid.SetColumn(newRectangle, col);
                Grid.SetRow(newRectangle, row);
                ourGrid.Children.Add(newRectangle);
                sweeperArray.SetTag(row, col, MineTag.NotRevealed);
                return;
            }
            if (sweeperArray.GetTag(row, col) == MineTag.NotRevealed)
            {
                Image imgFlag = CreateImage(ImgNameFlag);
                Grid.SetColumn(imgFlag, col);
                Grid.SetRow(imgFlag, row);
                ourGrid.Children.Add(imgFlag);
                sweeperArray.SetTag(row, col, MineTag.Flagged);
            }
        }
        //Receal non-flagged cells
        public void GridLeftClickTrueEvent(object sender, MouseButtonEventArgs e)
        {
            //Grid size 400x400 Cells 16x16
            var ourGrid = sender as Grid;
            int col = sweeperArray.GetCellColumn(ourGrid, e);
            int row = sweeperArray.GetCellRow(ourGrid, e);

            if (sweeperArray.GetTag(row, col) == MineTag.Flagged)
            {
                return;
            }

            RevealCell(ourGrid, row, col);

            if (sweeperArray.GetValue(row, col) == 0)
            {
                PopEmptyCellsRecursive(ourGrid, row, col);
            }
        }

        public void GameOver()
        {
            const EndGameState state = EndGameState.GameOver;
            EndGameWindow winDWin = new EndGameWindow(time, state);
            winDWin.Show();

            isGridActive = false;

            Timer.Stop();
        }

        //Draw revealed Cells when not flagged
        public void RevealCell(Grid ourGrid, int row, int col)
        {
            if (sweeperArray.GetTag(row, col) == MineTag.Flagged)
            {
                return;
            }
            var grid = new Grid();
            grid.Children.Add(new Rectangle() { Stroke = Brushes.Black, Fill = Brushes.SeaShell });
            if (sweeperArray.GetValue(row, col) >= 9)
            {
                var img = CreateImage(ImgNameMine);
                grid.Children.Add(img);
            }
            else
            {
                grid.Children.Add(new TextBlock()
                {
                    Text = (sweeperArray.GetValue(row, col) <= 0 ? " " : sweeperArray.GetValue(row, col).ToString()),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    FontSize = 15
                });
                if (sweeperArray.GetTag(row, col) == MineTag.NotRevealed)
                {
                    sweeperArray.SetTag(row, col, MineTag.Revealed);
                    Grid.SetColumn(grid, col);
                    Grid.SetRow(grid, row);
                    ourGrid.Children.Add(grid);
                    CheckWinCondition(row, col);
                    return;
                }
            }
            Grid.SetColumn(grid, col);
            Grid.SetRow(grid, row);
            ourGrid.Children.Add(grid);
            if (sweeperArray.GetValue(row, col) >= 9)
            {
                GameOver();
            }
        }

        //Chain reveal cells when not adjacent to mines, aka "empty" cells
        public void PopEmptyCellsRecursive(Grid ourGrid, int row, int col)
        {
            if (sweeperArray.GetTag(row, col) == MineTag.Flagged)
                return;
            sweeperArray.MarkCellAsChecked(row, col);
            for (int r = row - 1; r < row + 2; r++)
            {
                for (int c = col - 1; c < col + 2; c++)
                {
                    if (c < 0 || c > sweeperArray.NumberOfCellsX - 1 || r < 0 || r > sweeperArray.NumberOfCellsY - 1)
                        continue;
                    RevealCell(ourGrid, r, c);
                    if (!isGridActive)
                    { return; }
                    if (sweeperArray.GetValue(r, c) == 0)
                    {
                        PopEmptyCellsRecursive(ourGrid, r, c);
                    }
                }
            }
        }


        public void CheckWinCondition(int row, int column)
        {
            if (sweeperArray.GetTag(row, column) != MineTag.NotRevealed)
                nonMineCellsRevealed++;

            if (nonMineCellsRevealed == (sweeperArray.NumberOfCellsX * sweeperArray.NumberOfCellsY) - sweeperArray.MineDensity)
            {
                Timer.Stop();
                int yourTime = time;
                bool isHighScore = (highScoresList.Count < 3 || (highScoresList.Last().Key > yourTime));
                if (isHighScore)
                {
                    highScoresList.Add(yourTime, "Unknown");
                    EndGameState state = EndGameState.HighScore;
                    EndGameWindow winDWin = new EndGameWindow(time, state, ref highScoresList);
                    winDWin.Show();
                }
                else
                {
                    EndGameState state = EndGameState.Win;
                    EndGameWindow winDWin = new EndGameWindow(time, state);
                    winDWin.Show();
                }
                isGridActive = false;
            }
        }


        private void HighScores_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            Label highScores = sender as Label;
            StringBuilder highScoresString = new StringBuilder();
            ToolTip newToolTip = new ToolTip
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold
            };

            if (highScoresList.Count > 0)
            {
                foreach (var score in highScoresList)
                {
                    highScoresString.Append("Time: " + score.Key + " seconds Name: " + score.Value + "\n");
                }
                highScoresString.Length--;
                newToolTip.Content = highScoresString;
            }
            else
            {
                newToolTip.Content = "No High Scores recorded yet.";
            }
            highScores.ToolTip = newToolTip;
        }
    }
}
