using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library;
namespace Sudoku1
{
    class Program
    {
        static void Main(string[] args)
        {
            if( args.Length !=1)
            {
                Console.WriteLine("missing input file");
                System.Environment.Exit(1);
            }
            InFile data = new InFile(args[0]);
            if (data.OpenError())
            {
                Console.WriteLine("cannot open " + args[0]);
                System.Environment.Exit(1);
            }

            IntSet numberSet = new IntSet(1, 2, 3, 4, 5, 6, 7, 8, 9);
            string [,] assingedBoard = new string[9,9]; //read in the assigned board
            string [,] solutionBoard = new string[9,9];

            int [,][,] suggestBoard = new int[9,9][,];
           // List<IntSet[,]> suggestBoard = new List<IntSet[,]>();
           // IntSet [,] generatedBoard = new IntSet[9,9]; 

           
            //read the already assinged board
            readBoard(assingedBoard, data, false);
            Console.WriteLine("Already Assigned\n");
            printAssignedBoard(assingedBoard);
            readBoard(solutionBoard, data, true);
            Console.WriteLine("Solution Board\n");
            printAssignedBoard(solutionBoard);

            initializeSuggestedBoard(suggestBoard);
            updateSuggestedBoard(assingedBoard,suggestBoard);
            
            printSuggestionBoard(suggestBoard);


            Console.ReadKey();
        }
        static void readBoard(string[,] board,InFile data,bool solution)
        {
            int row=0, col = 0;
            while (row != 9 && !data.NoMoreData())
            {
                int num = data.ReadInt();
                if (!solution)
                {
                    
                }
                if (num == 0)
                {
                    board[row, col] = "..";
                }
                else
                {
                    board[row, col] = " " + num;
                }
                col++;
                if (col == 9)
                {
                    row++;
                    col = 0;
                }

            }
            /*if the board just filled was not the solution then
              skip next three lines to get to the next board (which is the solution board)*/
            if (!solution)
            {
                Console.WriteLine(data.ReadLine());
                Console.WriteLine(data.ReadLine());
                Console.WriteLine(data.ReadLine());
            }
        }
        static string getSingleBlock(int [,] block,int index) //converts a co-ordinate (row and col) into a string of suggested numbers
        {
            string retVal = "";
            for(int i = 0; i < 3; i++)
            {
                if (block[index, i] == 0)
                {
                    retVal += " ";
                }
                else
                {
                    retVal += block[index, i] + "";
                }
                
            }
            return retVal;
        }
        static void printSuggestionBoard(int[,][,] board)
        {
            //print the header
            string num; //num to print row
            Console.WriteLine("        0   1   2     3   4   5     6   7   8 \n"+
                              "     |=============+=============+=============|");
            int row = 0;
            int col = 0;
            for (int i = 1; i <= 27; i++)
            {
                num = "  ";
                if (i%3 == 2)
                {
                    num = " "+ i/3;
                   
                }
                
                string s = String.Format(" {0}  | {1}.{2}.{3} | {4}.{5}.{6} | {7}.{8}.{9} |", num,
                                         getSingleBlock(board[row,0],(i-1)%3), getSingleBlock(board[row, 1], (i - 1) % 3), getSingleBlock(board[row, 2], (i - 1) % 3),
                                         getSingleBlock(board[row, 3], (i - 1) % 3), getSingleBlock(board[row, 4], (i - 1) % 3), getSingleBlock(board[row, 5], (i - 1) % 3),
                                         getSingleBlock(board[row, 6], (i - 1) % 3), getSingleBlock(board[row, 7], (i - 1) % 3) , getSingleBlock(board[row, 8], (i - 1) % 3));
                Console.WriteLine(s);

                if (i % 3 == 0 && i!=27 )
                {
                    row++;
                    Console.WriteLine("     |----+---+----+----+---+----+----+---+----| ");
                }
            }
            //print footer
            Console.WriteLine ("     |=============+=============+=============|\n\n");

            

                
        }
        static void initializeSuggestedBoard(int[,][,] board)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    board[row, col] = new int[3,3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
                   
                }
            }     
        }

        static void updateSuggestedBoard(string[,] assignedBoard, int[,][,] board)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (assignedBoard[row, col] != "..")
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                board[row, col][i, j] = 0;
                            }
                        }
             

                    }
                    
                }
            }
        }
        static void printAssignedBoard(string [,] board)
        {
          
            //print header

            Console.WriteLine("       0   1   2   3   4   5   6   7   8 \n");
            for (int i = 0; i < 9; i++) {
                string s = String.Format("  {0}:  {1}  {2}  {3}  {4}  {5}  {6}  {7}  {8}  {9} ",
                                            i+"" ,board[i, 0], board[i, 1], board[i, 2], board[i, 3], board[i, 4], board[i, 5], 
                                            board[i, 6], board[i, 7], board[i, 8]);

                Console.WriteLine(s);
            }

            //print footer
            Console.WriteLine();
            Console.WriteLine("       0   1   2   3   4   5   6   7   8 \n");


        

        }
    }
}
