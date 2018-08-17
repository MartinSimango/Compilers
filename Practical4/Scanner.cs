
using System;
using System.IO;
using System.Collections;
using System.Text;

namespace Home {

public class Token {
	public int kind;    // token kind
	public int pos;     // token position in the source text (starting at 0)
	public int col;     // token column (starting at 0)
	public int line;    // token line (starting at 1)
	public string val;  // token value
	public Token next;  // AW 2003-03-07 Tokens are kept in linked list
}

public class Buffer {
	public const char EOF = (char)256;
	static byte[] buf;
	static int bufLen;
	static int pos;

	public static void Fill (Stream s) {
		bufLen = (int) s.Length;
		buf = new byte[bufLen];
		s.Read(buf, 0, bufLen);
		pos = 0;
	}

	public static int Read () {
		if (pos < bufLen) return buf[pos++];
		else return EOF;                          /* pdt */
	}

	public static int Peek () {
		if (pos < bufLen) return buf[pos];
		else return EOF;                          /* pdt */
	}

	/* AW 2003-03-10 moved this from ParserGen.cs */
	public static string GetString (int beg, int end) {
		StringBuilder s = new StringBuilder(64);
		int oldPos = Buffer.Pos;
		Buffer.Pos = beg;
		while (beg < end) { s.Append((char)Buffer.Read()); beg++; }
		Buffer.Pos = oldPos;
		return s.ToString();
	}

	public static int Pos {
		get { return pos; }
		set {
			if (value < 0) pos = 0;
			else if (value >= bufLen) pos = bufLen;
			else pos = value;
		}
	}

} // end Buffer

public class Scanner {
	const char EOL = '\n';
	const int  eofSym = 0;
	const int charSetSize = 256;
	const int maxT = 18;
	const int noSym = 18;
	static short[] start = {
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0, 85,  0, 62, 36,  0,  0,  0, 41,  0, 67,  0,  0, 84, 86,  0,
	  0,  0, 57,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0, 19,  9,  6,  0, 26,  0,  1,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0, 12,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  -1};


	static Token t;          // current token
	static char ch;          // current input character
	static int pos;          // column number of current character
	static int line;         // line number of current character
	static int lineStart;    // start position of current line
	static int oldEols;      // EOLs that appeared in a comment;
	static BitArray ignore;  // set of characters to be ignored by the scanner

	static Token tokens;     // the complete input token stream
	static Token pt;         // current peek token

	public static void Init (string fileName) {
		FileStream s = null;
		try {
			s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			Init(s);
		} catch (IOException) {
			Console.WriteLine("--- Cannot open file {0}", fileName);
			System.Environment.Exit(1);
		} finally {
			if (s != null) s.Close();
		}
	}

	public static void Init (Stream s) {
		Buffer.Fill(s);
		pos = -1; line = 1; lineStart = 0;
		oldEols = 0;
		NextCh();
		ignore = new BitArray(charSetSize+1);
		ignore[' '] = true;  // blanks are always white space
		ignore[0] = true; ignore[1] = true; ignore[2] = true; ignore[3] = true; 
		ignore[4] = true; ignore[5] = true; ignore[6] = true; ignore[7] = true; 
		ignore[8] = true; ignore[9] = true; ignore[10] = true; ignore[11] = true; 
		ignore[12] = true; ignore[13] = true; ignore[14] = true; ignore[15] = true; 
		ignore[16] = true; ignore[17] = true; ignore[18] = true; ignore[19] = true; 
		ignore[20] = true; ignore[21] = true; ignore[22] = true; ignore[23] = true; 
		ignore[24] = true; ignore[25] = true; ignore[26] = true; ignore[27] = true; 
		ignore[28] = true; ignore[29] = true; ignore[30] = true; ignore[31] = true; 
		
		//--- AW: fill token list
		tokens = new Token();  // first token is a dummy
		Token node = tokens;
		do {
			node.next = NextToken();
			node = node.next;
		} while (node.kind != eofSym);
		node.next = node;
		node.val = "EOF";
		t = pt = tokens;
	}

	static void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; }
		else {
			ch = (char)Buffer.Read(); pos++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && Buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; lineStart = pos + 1; }
		}

	}



	static void CheckLiteral() {
		switch (t.val) {
			default: break;
		}
	}

	/* AW Scan() renamed to NextToken() */
	static Token NextToken() {
		while (ignore[ch]) NextCh();

		t = new Token();
		t.pos = pos; t.col = pos - lineStart + 1; t.line = line;
		int state = start[ch];
		StringBuilder buf = new StringBuilder(16);
		buf.Append(ch); NextCh();
		switch (state) {
			case -1: { t.kind = eofSym; goto done; } // NextCh already done /* pdt */
			case 0: { t.kind = noSym; goto done; }   // NextCh already done
			case 1:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 2; }
				else { t.kind = noSym; goto done; }
			case 2:
				if (ch == 'u') { buf.Append(ch); NextCh(); goto case 3; }
				else { t.kind = noSym; goto done; }
			case 3:
				if (ch == 's') { buf.Append(ch); NextCh(); goto case 4; }
				else { t.kind = noSym; goto done; }
			case 4:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 5; }
				else { t.kind = noSym; goto done; }
			case 5:
				{ t.kind = 1; goto done; }
			case 6:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 7; }
				else { t.kind = noSym; goto done; }
			case 7:
				if (ch == 'g') { buf.Append(ch); NextCh(); goto case 8; }
				else { t.kind = noSym; goto done; }
			case 8:
				{ t.kind = 2; goto done; }
			case 9:
				if (ch == 'a') { buf.Append(ch); NextCh(); goto case 10; }
				else { t.kind = noSym; goto done; }
			case 10:
				if (ch == 't') { buf.Append(ch); NextCh(); goto case 11; }
				else { t.kind = noSym; goto done; }
			case 11:
				{ t.kind = 3; goto done; }
			case 12:
				if (ch == 'c') { buf.Append(ch); NextCh(); goto case 13; }
				else { t.kind = noSym; goto done; }
			case 13:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 14; }
				else { t.kind = noSym; goto done; }
			case 14:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 15; }
				else { t.kind = noSym; goto done; }
			case 15:
				if (ch == 't') { buf.Append(ch); NextCh(); goto case 16; }
				else { t.kind = noSym; goto done; }
			case 16:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 17; }
				else { t.kind = noSym; goto done; }
			case 17:
				if (ch == 'r') { buf.Append(ch); NextCh(); goto case 18; }
				else { t.kind = noSym; goto done; }
			case 18:
				{ t.kind = 4; goto done; }
			case 19:
				if (ch == 'i') { buf.Append(ch); NextCh(); goto case 20; }
				else { t.kind = noSym; goto done; }
			case 20:
				if (ch == 'c') { buf.Append(ch); NextCh(); goto case 21; }
				else { t.kind = noSym; goto done; }
			case 21:
				if (ch == 'y') { buf.Append(ch); NextCh(); goto case 22; }
				else { t.kind = noSym; goto done; }
			case 22:
				if (ch == 'c') { buf.Append(ch); NextCh(); goto case 23; }
				else { t.kind = noSym; goto done; }
			case 23:
				if (ch == 'l') { buf.Append(ch); NextCh(); goto case 24; }
				else { t.kind = noSym; goto done; }
			case 24:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 25; }
				else { t.kind = noSym; goto done; }
			case 25:
				{ t.kind = 5; goto done; }
			case 26:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 27; }
				else { t.kind = noSym; goto done; }
			case 27:
				if (ch == 'u') { buf.Append(ch); NextCh(); goto case 28; }
				else { t.kind = noSym; goto done; }
			case 28:
				if (ch == 'r') { buf.Append(ch); NextCh(); goto case 29; }
				else { t.kind = noSym; goto done; }
			case 29:
				if (ch == 'b') { buf.Append(ch); NextCh(); goto case 30; }
				else { t.kind = noSym; goto done; }
			case 30:
				if (ch == 'y') { buf.Append(ch); NextCh(); goto case 31; }
				else { t.kind = noSym; goto done; }
			case 31:
				if (ch == 'f') { buf.Append(ch); NextCh(); goto case 32; }
				else { t.kind = noSym; goto done; }
			case 32:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 33; }
				else { t.kind = noSym; goto done; }
			case 33:
				if (ch == 'u') { buf.Append(ch); NextCh(); goto case 34; }
				else { t.kind = noSym; goto done; }
			case 34:
				if (ch == 'r') { buf.Append(ch); NextCh(); goto case 35; }
				else { t.kind = noSym; goto done; }
			case 35:
				{ t.kind = 6; goto done; }
			case 36:
				if (ch == 'a') { buf.Append(ch); NextCh(); goto case 37; }
				else { t.kind = noSym; goto done; }
			case 37:
				if (ch == 'd') { buf.Append(ch); NextCh(); goto case 38; }
				else { t.kind = noSym; goto done; }
			case 38:
				{ t.kind = 7; goto done; }
			case 39:
				if (ch == 'm') { buf.Append(ch); NextCh(); goto case 40; }
				else { t.kind = noSym; goto done; }
			case 40:
				{ t.kind = 8; goto done; }
			case 41:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 42; }
				else { t.kind = noSym; goto done; }
			case 42:
				if (ch == 'l') { buf.Append(ch); NextCh(); goto case 43; }
				else { t.kind = noSym; goto done; }
			case 43:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 44; }
				else { t.kind = noSym; goto done; }
			case 44:
				if (ch == 'n') { buf.Append(ch); NextCh(); goto case 45; }
				else { t.kind = noSym; goto done; }
			case 45:
				{ t.kind = 9; goto done; }
			case 46:
				if (ch == 'r') { buf.Append(ch); NextCh(); goto case 47; }
				else { t.kind = noSym; goto done; }
			case 47:
				if (ch == 'g') { buf.Append(ch); NextCh(); goto case 48; }
				else { t.kind = noSym; goto done; }
			case 48:
				if (ch == 'a') { buf.Append(ch); NextCh(); goto case 49; }
				else { t.kind = noSym; goto done; }
			case 49:
				if (ch == 'r') { buf.Append(ch); NextCh(); goto case 50; }
				else { t.kind = noSym; goto done; }
			case 50:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 51; }
				else { t.kind = noSym; goto done; }
			case 51:
				if (ch == 't') { buf.Append(ch); NextCh(); goto case 52; }
				else { t.kind = noSym; goto done; }
			case 52:
				{ t.kind = 10; goto done; }
			case 53:
				if (ch == 'i') { buf.Append(ch); NextCh(); goto case 54; }
				else { t.kind = noSym; goto done; }
			case 54:
				if (ch == 'c') { buf.Append(ch); NextCh(); goto case 55; }
				else { t.kind = noSym; goto done; }
			case 55:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 56; }
				else { t.kind = noSym; goto done; }
			case 56:
				{ t.kind = 11; goto done; }
			case 57:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 58; }
				else { t.kind = noSym; goto done; }
			case 58:
				if (ch == 'b') { buf.Append(ch); NextCh(); goto case 59; }
				else { t.kind = noSym; goto done; }
			case 59:
				if (ch == 'y') { buf.Append(ch); NextCh(); goto case 60; }
				else { t.kind = noSym; goto done; }
			case 60:
				if (ch == 'n') { buf.Append(ch); NextCh(); goto case 61; }
				else { t.kind = noSym; goto done; }
			case 61:
				{ t.kind = 12; goto done; }
			case 62:
				if (ch == 'a') { buf.Append(ch); NextCh(); goto case 63; }
				else { t.kind = noSym; goto done; }
			case 63:
				if (ch == 't') { buf.Append(ch); NextCh(); goto case 64; }
				else { t.kind = noSym; goto done; }
			case 64:
				if (ch == 'h') { buf.Append(ch); NextCh(); goto case 65; }
				else { t.kind = noSym; goto done; }
			case 65:
				if (ch == 'y') { buf.Append(ch); NextCh(); goto case 66; }
				else { t.kind = noSym; goto done; }
			case 66:
				{ t.kind = 13; goto done; }
			case 67:
				if (ch == 'a') { buf.Append(ch); NextCh(); goto case 68; }
				else { t.kind = noSym; goto done; }
			case 68:
				if (ch == 'n') { buf.Append(ch); NextCh(); goto case 69; }
				else { t.kind = noSym; goto done; }
			case 69:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 70; }
				else { t.kind = noSym; goto done; }
			case 70:
				if (ch == 't') { buf.Append(ch); NextCh(); goto case 71; }
				else { t.kind = noSym; goto done; }
			case 71:
				{ t.kind = 14; goto done; }
			case 72:
				if (ch == 'n') { buf.Append(ch); NextCh(); goto case 73; }
				else { t.kind = noSym; goto done; }
			case 73:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 74; }
				else { t.kind = noSym; goto done; }
			case 74:
				{ t.kind = 15; goto done; }
			case 75:
				if (ch == 'd') { buf.Append(ch); NextCh(); goto case 76; }
				else { t.kind = noSym; goto done; }
			case 76:
				if (ch == 'w') { buf.Append(ch); NextCh(); goto case 77; }
				else { t.kind = noSym; goto done; }
			case 77:
				if (ch == 'a') { buf.Append(ch); NextCh(); goto case 78; }
				else { t.kind = noSym; goto done; }
			case 78:
				{ t.kind = 16; goto done; }
			case 79:
				if (ch == 'n') { buf.Append(ch); NextCh(); goto case 80; }
				else { t.kind = noSym; goto done; }
			case 80:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 81; }
				else { t.kind = noSym; goto done; }
			case 81:
				if (ch == 'l') { buf.Append(ch); NextCh(); goto case 82; }
				else { t.kind = noSym; goto done; }
			case 82:
				if (ch == 'e') { buf.Append(ch); NextCh(); goto case 83; }
				else { t.kind = noSym; goto done; }
			case 83:
				{ t.kind = 17; goto done; }
			case 84:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 39; }
				else if (ch == 'a') { buf.Append(ch); NextCh(); goto case 46; }
				else { t.kind = noSym; goto done; }
			case 85:
				if (ch == 'l') { buf.Append(ch); NextCh(); goto case 53; }
				else if (ch == 'n') { buf.Append(ch); NextCh(); goto case 72; }
				else { t.kind = noSym; goto done; }
			case 86:
				if (ch == 't') { buf.Append(ch); NextCh(); goto case 87; }
				else { t.kind = noSym; goto done; }
			case 87:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 88; }
				else { t.kind = noSym; goto done; }
			case 88:
				if (ch == 'm') { buf.Append(ch); NextCh(); goto case 89; }
				else { t.kind = noSym; goto done; }
			case 89:
				if (ch == 'b') { buf.Append(ch); NextCh(); goto case 90; }
				else { t.kind = noSym; goto done; }
			case 90:
				if (ch == 'i') { buf.Append(ch); NextCh(); goto case 91; }
				else { t.kind = noSym; goto done; }
			case 91:
				if (ch == 'z') { buf.Append(ch); NextCh(); goto case 92; }
				else { t.kind = noSym; goto done; }
			case 92:
				if (ch == 'o') { buf.Append(ch); NextCh(); goto case 75; }
				else if (ch == 'a') { buf.Append(ch); NextCh(); goto case 79; }
				else { t.kind = noSym; goto done; }

		}
		done:
		t.val = buf.ToString();
		return t;
	}

	/* AW 2003-03-07 get the next token, move on and synch peek token with current */
	public static Token Scan () {
		t = pt = t.next;
		return t;
	}

	/* AW 2003-03-07 get the next token, ignore pragmas */
	public static Token Peek () {
		do {                      // skip pragmas while peeking
			pt = pt.next;
		} while (pt.kind > maxT);
		return pt;
	}

	/* AW 2003-03-11 to make sure peek start at current scan position */
	public static void ResetPeek () { pt = t; }

} // end Scanner

} // end namespace
