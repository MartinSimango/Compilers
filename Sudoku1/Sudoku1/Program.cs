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

            
            string [,] assingedBoard = new string[9,9]; //read in the assigned board
            string [,] solutionBoard = new string [9,9];


            //read the already assinged board
            readBoard(assingedBoard, data, false);
            Console.WriteLine("Already Assigned\n");
            printAssignedBoard(assingedBoard);
            readBoard(solutionBoard, data, true);
            Console.WriteLine("Solution Board\n");
            printAssignedBoard(solutionBoard);


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
        static void printSuggestionBoard(string[,] board)
        {
            //print the header
            string num; //num to print row
            Console.WriteLine("        0   1   2     3   4   5     6   7   8 \n"+
                              "     |=============+=============+=============|");
            for (int i = 1; i <= 27; i++)
            {
                num = "  ";
                if (i%3 == 2)
                {
                    num = " "+ i/3;
                   
                }
                string s = String.Format(" {0}  | {1}.{2}.{3} | {4}.{5}.{6} | {7}.{8}.{9} |",
                                         num, "123", "123", "123", "123", "123", "123", "123", "123", "123");
                Console.WriteLine(s);
                if (i % 3 == 0 && i!=27 )
                { 
                    Console.WriteLine("     |----+---+----+----+---+----+----+---+----| ");
                }
            }
            //print footer
            Console.WriteLine ("     |=============+=============+=============|\n\n");

            

      
                
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
