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
      // and others like this

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
      while (ch > EOF && ch <= ' ') GetChar();
      StringBuilder symLex = new StringBuilder();
      int symKind = noSym;
      if (Char.IsLetter(ch)){
          do {
            symLex.Append(ch); 
            GetChar();
          } while (Char.IsLetterOrDigit(ch));
          switch (symLex.ToString()){ // Check this works~!
            case "int":
              symKind = intSym;
              break;
            case "bool":
              symKind = boolSym;
              break;
            case "char":
              symKind = charSym;
              break;
            case "void":
              symKind = voidSym;    
              break;
            default:
              symKind = identSym;
              break;
          }
      }
      else if (Char.IsDigit(ch)){
        do {
          symLex.Append(ch);
          GetChar();
        } while(Char.IsDigit(ch));
        symKind = numSym;
      }
      else{
        switch(ch){
          case EOF:
            symKind = EOFSym;
            break;
          case '(':
            symKind = lParenSym;
            break;
          case ')':
            symKind = rParenSym;
            break;
          case '*':
            symKind = pointerSym;
            break;
          case '[':
            symKind = lbrackSym;
            break;
          case ']':
            symKind = rbrackSym;
            break;
          case ',':
            symKind = commaSym;
            break;
          case ';':
            symKind = semiColonSym;
            break;
          case '/':
            skipComment();
            break;
          default:
            symKind = noSym;
            break;
        }
        symLex.Append(ch);
        GetChar();
      }
      // over to you!

      sym = new Token(symKind, symLex.ToString());
    } // GetSym


    static void skipComment(){
      GetChar();
      // Check if comment closes
      if (ch == '*'){
        do{
          GetChar();
          if (ch == '*'){ 
            GetChar();
            if (ch == '/'){ 
              GetChar();
              return;
            }
          }
        }while (ch != '*');
      }
      Console.WriteLine("Comment was never closed! Exiting");
      System.Environment.Exit(1);
      // exit for fail 
    }
    

  /*  ++++ Commented out for the moment

    // +++++++++++++++++++++++++++++++ Parser +++++++++++++++++++++++++++++++++++
  */
    // IntSets
    static IntSet TypeSym = new IntSet(intSym, voidSym, boolSym, charSym);

    static void Accept(int wantedSym, string errorMessage) {
    // Checks that lookahead token is wantedSym
      if (sym.kind == wantedSym) GetSym(); else Abort(errorMessage);
    } // Accept

    static void Accept(IntSet allowedSet, string errorMessage) {
    // Checks that lookahead token is in allowedSet
      if (allowedSet.Contains(sym.kind)) GetSym(); else Abort(errorMessage);
    } // Accept

    static void CDecls() {
      while (sym.kind != EOFSym) DecList(); // GetChar(); 
    } // CDecls

    static void DecList() {
      if (TypeSym.Contains(sym.kind)) 
        Accept(TypeSym, "Type expected");
      else Abort("Type expected");

      if (sym.kind == pointerSym)
        OneDecl();
      else Abort("* expected");
      while (sym.kind == commaSym){
        Accept(semiColonSym, "; expected");
        if (sym.kind == pointerSym)
          OneDecl();
        else Abort("* expected");
      }
      Accept(semiColonSym, "; expected");
    }

    static void OneDecl(){
      if (sym.kind == pointerSym){
        Accept(pointerSym, "* expected");
        OneDecl();
      }
      else if (firstDirect.Contains(sym.kind)) Direct(); 
      else Abort("OneDecl alternative");
    }


    static IntSet firstDirect = new IntSet(identSym, lParenSym); 

    static void Direct(){
    //Direct = ( ident | "(" OneDecl ")" ) [ Suffix ] .
      if (sym.kind == lbrackSym){
        Accept(lParenSym, "( expected");
        OneDecl();
        Accept(rParenSym, ") expected");
      }
      else Accept(identSym, "identifier expected");
      // first(Suffix) -> first(Array) -> "[" 
      if (sym.kind == lbrackSym){
        Accept(lbrackSym, "[ expect");
        Suffix();
      }
    }

    static void Suffix(){ 
      //Suffix = Array { Array } | Params .
      if (sym.kind == rbrackSym){
        Array(); 
        while (sym.kind == rbrackSym) Array();
      }
      else Params();
    }

    static void Params(){
      //Params = "(" [ OneParam { "," OneParam } ] ")" .
      Accept(lParenSym, "( expected");
      // first(OneParam) -> identSym
      if (sym.kind == identSym){
        OneParam();
        while (sym.kind == commaSym) OneParam();
      }
      Accept(rParenSym, ") expected");
    }

    static IntSet firstOneDecl = new IntSet(pointerSym, identSym, lbrackSym);

    static void OneParam(){
      //OneDecl = "*" OneDecl | Direct .
      Accept(TypeSym, "Type expected");
      if(firstOneDecl.Contains(sym.kind)) OneDecl();
    }

    static void Array(){
      //Array = "[" [ number ] "]" .
      Accept(rbrackSym, "[ expected");
      if (sym.kind == numSym) Accept(numSym, "number expected");
      Accept(rbrackSym, "] expected");
    }

/*
   PRODUCTIONS
   Cdecls = { DecList } EOF .
   DecList = Type OneDecl { "," OneDecl } ";" .
   Type = "int" | "void" | "bool" | "char" .
   OneDecl = "*" OneDecl | Direct .
   Direct = ( ident | "(" OneDecl ")" ) [ Suffix ] .
   Suffix = Array { Array } | Params .
   Params = "(" [ OneParam { "," OneParam } ] ")" .
   OneParam = Type [ OneDecl ] .
   Array = "[" [ number ] "]" .
   END Cdecls.
*/

/*  ++++++ */

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

      do {
        GetSym();                                 // Lookahead symbol
        OutFile.StdOut.Write(sym.kind, 3);
        OutFile.StdOut.WriteLine(" " + sym.val);  // See what we got
      } while (sym.kind != EOFSym);

  /*  After the scanner is debugged we shall substitute this code: */

      GetSym();                                   // Lookahead symbol
      CDecls();                                   // Start to parse from the goal symbol
      // if we get back here everything must have been satisfactory
      Console.WriteLine("Parsed correctly");

      output.Close();
    } // Main

  } // Declarations

