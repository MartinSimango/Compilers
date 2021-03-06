  // Do learn to insert your names and a brief description of
  // what the program is supposed to do!

  // This is a skeleton program for developing a parser for C declarations
  // P.D. Terry, Rhodes University, 2015

  using Library;
  using System;
  using System.Text;

  class Token {
    public int kind;
    public string val;

    public Token(int kind, string val) {
      this.kind = kind;
      this.val = val;
    } // constructor

  } // Token

  class Declarations {

    // +++++++++++++++++++++++++ File Handling and Error handlers ++++++++++++++++++++

    static InFile input;
    static OutFile output;

    static string NewFileName(string oldFileName, string ext) {
    // Creates new file name by changing extension of oldFileName to ext
      int i = oldFileName.LastIndexOf('.');
      if (i < 0) return oldFileName + ext; else return oldFileName.Substring(0, i) + ext;
    } // NewFileName

    static void ReportError(string errorMessage) {
    // Displays errorMessage on standard output and on reflected output
      Console.WriteLine(errorMessage);
      output.WriteLine(errorMessage);
    } // ReportError

    static void Abort(string errorMessage) {
    // Abandons parsing after issuing error message
      ReportError(errorMessage);
      output.Close();
      System.Environment.Exit(1);
    } // Abort

    // +++++++++++++++++++++++  token kinds enumeration +++++++++++++++++++++++++

    const int
      noSym        =  0,
      EOFSym       =  1,
      lParenSym    =  2,
      rParenSym    =  3,
      pointerSym   =  4,
      lbrackSym    =  5,
      rbrackSym    =  6,
      intSym       =  7,
      charSym      =  8,
      boolSym      =  9,
      voidSym      =  10,
      commaSym     =  11,
      semiColonSym =  12,
      identSym     =  13,
      numSym       =  14;

    // +++++++++++++++++++++++++++++ Character Handler ++++++++++++++++++++++++++

    const char EOF = '\0';
    static bool atEndOfFile = false;

    // Declaring ch as a global variable is done for expediency - global variables
    // are not always a good thing

    static char ch;    // look ahead character for scanner

    static void GetChar() {
    // Obtains next character ch from input, or CHR(0) if EOF reached
    // Reflect ch to output
      if (atEndOfFile) ch = EOF;
      else {
        ch = input.ReadChar();
        atEndOfFile = ch == EOF;
        if (!atEndOfFile) output.Write(ch);
      }
    } // GetChar

    // +++++++++++++++++++++++++++++++ Scanner ++++++++++++++++++++++++++++++++++

    // Declaring sym as a global variable is done for expediency - global variables
    // are not always a good thing

    static Token sym;

    static void GetSym() {
    // Scans for next sym from input
      while (ch > EOF && ch <= ' ') GetChar();//skip whitespace
      StringBuilder symLex = new StringBuilder();
      int symKind = noSym;
        if (Char.IsLetter(ch))
        {
            do
            {
                symLex.Append(ch);
                GetChar();
            } while (Char.IsLetterOrDigit(ch) || ch=='_');

            if (symLex.ToString().Equals("int")) symKind = intSym;
            else if (symLex.ToString().Equals("char")) symKind = charSym;
            else if (symLex.ToString().Equals("bool")) symKind = boolSym;
            else if (symLex.ToString().Equals("void")) symKind = voidSym;
            else symKind = identSym;
        }
        else if (Char.IsDigit(ch))
        {
            do
            {
                symLex.Append(ch);
                GetChar();
            } while (Char.IsDigit(ch));
            symKind = numSym;
        }
        else
        {
            symLex.Append(ch);
            switch (ch)
            {
                case EOF:
                    symLex = new StringBuilder("EOF");
                    symKind = EOFSym;
                    break;
                case '(':
                    symKind = lParenSym; GetChar();
                    break;
                case ')':
                    symKind = rParenSym; GetChar();
                    break;
                case '*':
                    symKind = pointerSym; GetChar();
                    break;
                case '[':
                    symKind = lbrackSym; GetChar();
                    break;
                case ']':
                    symKind = rbrackSym; GetChar();
                    break;
                case ',':
                    symKind = commaSym; GetChar();
                    break;
                case ';':
                    symKind = semiColonSym; GetChar();
                    break;
				case '/':
					GetChar();
					if(ch == '*'){
						do{
							GetChar();
							if(ch=='*'){
								GetChar();
								if(ch == '/'){
									GetChar();
									break;
								}
							}
						} while(ch!=EOF);
						GetSym(); 
					    return;
 				}
					else if(ch == '/'){
						do{
							GetChar();
						}while(ch!='\n' && ch!= EOF);
						GetChar();
						GetSym();
						return;
					}
					else
						symKind = noSym;
					break;
                default:
                    symKind = noSym; GetChar();
                    break;
            }
        }
      sym = new Token(symKind, symLex.ToString());
    } // GetSym


    // +++++++++++++++++++++++++++++++ Parser +++++++++++++++++++++++++++++++++++
    static IntSet 
      firstDecList = new IntSet(intSym, voidSym, boolSym, charSym),
      firstType = new IntSet(intSym, voidSym, boolSym, charSym),
      firstOneDecl = new IntSet(pointerSym, identSym, lParenSym),
      firstDirect = new IntSet(identSym, lParenSym),
      firstSuffix = new IntSet(lbrackSym, lParenSym),
      firstParams = new IntSet(lParenSym),
      firstOneParam = new IntSet(intSym, voidSym, boolSym, charSym),
      firstArray = new IntSet(lbrackSym);
    static void Accept(int wantedSym, string errorMessage) {
    // Checks that lookahead token is wantedSym
      if (sym.kind == wantedSym) 
		  GetSym(); 
	  else 
		  Abort(errorMessage);
    } // Accept

    static void Accept(IntSet allowedSet, string errorMessage) {
    // Checks that lookahead token is in allowedSet
      if (allowedSet.Contains(sym.kind)) 
		  GetSym(); 
	  else 
		  Abort(errorMessage);
    } // Accept

    static void CDecls() {
		 while (firstDecList.Contains(sym.kind)) DecList();
		 Accept(EOFSym, "EOF expected");
	}
	static void DecList(){
		Type();
		OneDecl();
		while (sym.kind == commaSym){
			Accept(commaSym, ", expected");
		    OneDecl();
		}
		Accept(semiColonSym, "; expected");
	}
	static void Type(){
		Accept(firstType, "type expected");		
	}
	
	static void OneDecl(){
		if(sym.kind == pointerSym){
			Accept(pointerSym,"* expected");
			OneDecl();
		}
		else {
			Direct();
		}
	
	}
		
    static void Direct(){
		if(sym.kind == lParenSym) {
			Accept(lParenSym,"( expected");
			OneDecl();
			Accept(rParenSym,") expected " );
		}
		else{
			Accept(identSym, "identifier expected");
		}
		if(firstSuffix.Contains(sym.kind)){
			Suffix();
		}
			
	}
	static void Suffix(){
		if(firstArray.Contains(sym.kind)){
			Array();
			while(firstArray.Contains(sym.kind)) Array();
		}
		else {
			 Params();
		}
		
	}
	static void Params(){
		Accept(lParenSym,"( expected");
		if(firstOneParam.Contains(sym.kind)){
			OneParam();
			while(sym.kind == commaSym){
				Accept(commaSym,", expected");
				OneParam();
			}
		}
		Accept(rParenSym,") expected");
	}
	static void OneParam(){
		Type();
		if(firstOneDecl.Contains(sym.kind)){
			OneDecl();
		}
	}
	static void Array(){
		Accept(lbrackSym,"[ expected");
		if(sym.kind == numSym){
			Accept(numSym,"numSym expected");
		}
		Accept(rbrackSym,"] expected");
	}

    // +++++++++++++++++++++ Main driver function +++++++++++++++++++++++++++++++

    public static void Main(string[] args) {
      // Open input and output files from command line arguments
      if (args.Length == 0) {
        Console.WriteLine("Usage: Declarations FileName");
        System.Environment.Exit(1);
      }
      input = new InFile(args[0]);
      output = new OutFile(NewFileName(args[0], ".out"));

      GetChar();                                  // Lookahead character

  //  To test the scanner we can use a loop like the following:

    /*  do {
        GetSym();                                 // Lookahead symbol
        OutFile.StdOut.Write(sym.kind, 3);
        OutFile.StdOut.WriteLine(" " + sym.val);  // See what we got
      } while (sym.kind != EOFSym);
*/
   // After the scanner is debugged we shall substitute this code:

      GetSym();                                   // Lookahead symbol
      CDecls();                                   // Start to parse from the goal symbol
      // if we get back here everything must have been satisfactory
      Console.WriteLine("Parsed correctly");

  
      output.Close();
    } // Main

  } // Declarations

