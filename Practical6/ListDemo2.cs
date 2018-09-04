   using Library;
   using System.Collections;
   using System.Text;

   class ListDemo2 {
   // Demonstrate simple application of the non-generic ArrayList class (C# version 1.1)

     class Entry {
       public string name;
       public int age;                                     // other fields could be added

       public Entry(string name, int age) {                // constructor
         this.name = name;
         this.age = age;
       }
     } // Entry

     static ArrayList list = new ArrayList();              // global for simplicity here!

     public static int position(string name) {
     // Finds position of entry with search key name in list, or -1 if not found
       int i = list.Count - 1;                             // index of last entry
       while (i >= 0 &&                                    // short-circuit protection
              !name.Equals(((Entry)list[i]).name))         // must cast before extracting field
         i--;                                              // will reach -1 if no match
       return i;
     }

     public static void Main (string[] args) {
     // Build a list of people's names and ages
       IO.WriteLine("Supply a list of people's names and ages.  CTRL-Z to terminate");
       do {
         string name = IO.ReadWord();
         if (IO.EOF()) break;
         int age = IO.ReadInt();
         IO.ReadLn();
         list.Add(new Entry(name, age));                   // add to end of list
       } while (!IO.EOF());

       IO.WriteLine(list.Count + " items stored");         // report size of list

       Entry patEntry = new Entry("Pat", 61);              // that fellow again!
       list[0] = patEntry;                                 // insert him on position 0

       StringBuilder sb = new StringBuilder();             // demonstrate StringBuilder use
       for (int i = 0; i < list.Count; i++) {              // display each entry
         Entry e = (Entry) list[i];                        // retrieve (via a cast) an item at position i
         IO.Write(e.name, -16); IO.WriteLine(e.age);       // -16 means "left justify"
         sb.Append(e.name + " ");                          // add the names to a StringBuffer object
       }
       IO.WriteLine();

       int where = position("Peter");                      // find the silly fellow!
       if (where < 0) IO.WriteLine("Peter not found");
       else {
         Entry peter = (Entry) list[where];                // retrieve (via a cast) an item at position where
         IO.WriteLine("Peter found at position " + where + ". He is " + peter.age + " years old");
       }

       if (sb.Length > 0) {
         IO.WriteLine();
         IO.WriteLine("Summary of names:");
         IO.WriteLine();
         IO.WriteLine(sb.ToString());
       }
     }

   } // ListDemo2
