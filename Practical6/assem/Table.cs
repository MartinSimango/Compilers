// Handle label table for simple PVM assembler
// P.D. Terry, Rhodes University, 2015

using Library;
using System.Collections.Generic;

namespace Assem {

  class LabelEntry {

    public string name;
    public Label label;
    public List<int> refs;

    public LabelEntry(string name, Label label, int lineNumber) {
      this.name  = name;
      this.label = label; 
      this.refs = new List<int>();
      this.refs.Add(lineNumber);
    }

    public void AddReference(int lineNumber) {
      this.refs.Add(lineNumber);
    }

  } // end LabelEntry

// -------------------------------------------------------------------------------------

  class LabelTable {

    public static List<LabelEntry> list = new List<LabelEntry>();

    public static void Insert(LabelEntry entry) {
    // Inserts entry into label table
      list.Add(entry);
    } // insert

    public static LabelEntry Find(string name) {
    // Searches table for label entry matching name.  If found then returns entry.
    // If not found, returns null
      int i = 0;
      while (i < list.Count && !name.Equals(list[i].name)) i++;
      if (i >= list.Count){ 
        return null; 
      } 
      else{
        LabelEntry entryFound = list[i];
        int labAdr = entryFound.label.Address();
        entryFound.AddReference(labAdr);
        list[i] = entryFound;
        return list[i];
      }
    } // find

    public static void CheckLabels() {
    // Checks that all labels have been defined (no forward references outstanding)
      for (int i = 0; i < list.Count; i++) {
        if (!list[i].label.IsDefined())
          Parser.SemError("undefined label - " + list[i].name);
        else {
          Parser.SemError("This object");
          //list[i].AddReference(list[i].label.Address());
        }
      }
    } // CheckLabels

    public static void ListReferences(OutFile output) {
    // Cross reference list of all labels used on output file
      IO.WriteLine("Labels:");
      for (int i = 0; i < list.Count; i++){
        string reflist = list[i].name;
        if (list[i].label.IsDefined())
          reflist += "  (DEFINED) ";
        foreach (int r in list[i].refs)
            reflist += " " + r;
        IO.WriteLine(reflist);
      }
    } // ListReferences

  } // end LabelTable

// -------------------------------------------------------------------------------------

  class VariableEntry {

    public string name;
    public int offset;

    public VariableEntry(string name, int offset) {
      this.name   = name;
      this.offset = offset;
    }

  } // end VariableEntry

// -------------------------------------------------------------------------------------

  class VarTable {

    private static List<VariableEntry> list = new List<VariableEntry>();
    private static int varOffset = 0;

    public static int FindOffset(string name) {
    // Searches table for variable entry matching name.  If found then returns the known offset.
    // If not found, makes an entry and updates the master offset
      int i = 0;
      while (i < list.Count && !name.Equals(list[i].name)) i++;      
      if (i >= list.Count) { 
        list.Add(new VariableEntry(name, i)); 
        varOffset = i; 
        return i;
      }
      else return i;
    } // FindOffset

    public static void ListReferences(OutFile output) {
    // Cross reference list of all variables on output file

    } // ListReferences

  } // end VarTable

} // end namespace
