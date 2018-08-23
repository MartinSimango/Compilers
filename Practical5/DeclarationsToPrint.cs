    // Louise Poole, Martin Simango, Matt Doherty
	
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

    // +++++++++++++++++++++++++++++++ Scanner ++++++++++++++++++++++++++++++++++

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
            } while (Char.IsLetterOrDigit(ch));

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
                    symKind = EOFSym; break;
                case '(':
                    symKind = lParenSym; GetChar(); break;
                case ')':
                    symKind = rParenSym; GetChar(); break;
                case '*':
                    symKind = pointerSym; GetChar(); break;
                case '[':
                    symKind = lbrackSym; GetChar(); break;
                case ']':
                    symKind = rbrackSym; GetChar(); break;
                case ',':
                    symKind = commaSym; GetChar(); break;
                case ';':
                    symKind = semiColonSym; GetChar(); break;
                default:
                    symKind = noSym; GetChar(); break;
            }
        }
      sym = new Token(symKind, symLex.ToString());
    } // GetSym