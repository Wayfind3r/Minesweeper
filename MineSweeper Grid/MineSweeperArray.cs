using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MineSweeper_Grid
{
    public enum MineTag
    {
        NotRevealed,
        Revealed,
        Flagged
    };

    public class MineSweeperArray
    {
        private static int[,] Minefield;
        private static MineTag[,] MinefieldTags;
        //400x400, 16x16 grid, 40 mines, default sizes for this build, do not alter
        public int NumberOfCellsX { get; } = 16;
        public int NumberOfCellsY { get; } = 16;
        public int GridDimensionX { get; } = 400;
        public int GridDimensionY { get; } = 400;
        public int MineDensity { get; } = 40;

        //ctor initializes a new random Array
        //A cell can have up to 8 adjacent mines 
        //Values 0 to 8 = number of adjecent mines
        //Value 9 = Mine
        //Value -1 = cell checked for the sake of chain reveal -> MarkCellAsChecked
        public MineSweeperArray()
        {
            Minefield = new int[NumberOfCellsY, NumberOfCellsX];
            MinefieldTags = new MineTag[NumberOfCellsY,NumberOfCellsX];
            var rng = new Random();
            int mineCounter = 0;
            while (mineCounter < MineDensity)
            {
                int row = rng.Next(0, NumberOfCellsY-1);
                int column = rng.Next(0,NumberOfCellsX-1);
                if (Minefield[row, column] < 9)
                {
                    Minefield[row, column] = 9;
                    mineCounter++;
                    for (int r = row - 1; r < row + 2; r++)
                    {
                        for (int c = column - 1; c < column + 2; c++)
                        {
                            if (r >= 0 && r < NumberOfCellsY && c >= 0 && c < NumberOfCellsX)
                            {
                                Minefield[r, c]++;
                            }
                        }
                    }
                }
            }
            for (int r = 0; r < NumberOfCellsY; r++)
            {
                for (int c = 0; c < NumberOfCellsX; c++)
                {
                    MinefieldTags[r, c] = MineTag.NotRevealed;
                }
            }
        }
        
        public int GetValue(int row, int column)
        {
            return Minefield[row, column];
        }
        //Get cell State
        public MineTag GetTag(int row, int column)
        {
            return MinefieldTags[row, column];
        }
        //Set cell State
        public void SetTag(int row, int column, MineTag newTag)
        {
            MinefieldTags[row, column] = newTag;
        }
        //Cells with value of -1 will not be checked again for chain reveal (Left+Right click)
        public void MarkCellAsChecked(int row, int column)
        {
            Minefield[row, column] = -1;
        }

        public int GetCellColumn(Grid ourGrid, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(ourGrid);
            int col = Convert.ToInt32(clickPoint.X);
            int cellDimension = GridDimensionX / NumberOfCellsX;
            col /= cellDimension;
            return col;
        }

        public int GetCellRow(Grid ourGrid, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(ourGrid);
            int row = Convert.ToInt32(clickPoint.Y);
            int cellDimension = GridDimensionY / NumberOfCellsY;
            row /= cellDimension;
            return row;
        }
    }
}