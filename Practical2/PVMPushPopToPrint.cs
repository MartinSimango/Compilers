// Definition of simple stack machine and simple emulator for Assem level 1
// Uses auxiliary methods Push, Pop and Next
// P.D. Terry, Rhodes University, 2017
// This version for Practical 2

// Louise Poole, Martin Simango, Matt Doherty

using Library;
using System;
using System.IO;
using System.Diagnostics;

namespace Assem {


  class Processor {
    public int sp;            // Stack pointer
    public int hp;            // Heap pointer
    public int gp;            // Global frame pointer
    public int fp;            // Local frame pointer
    public int mp;            // Mark stack pointer
    public int ir;            // Instruction register
    public int pc;            // Program counter
  } // end Processor

  class PVM {

  // Machine opcodes

    public const int
      nop    =    1,
      dsp    =    2,
      ldc    =    3,
      lda    =    4,
      ldv    =    5,
      sto    =    6,
      ldxa   =    7,
      inpi   =    8,
      prni   =    9,
      inpb   =   10,
      prnb   =   11,
      prns   =   12,
      prnl   =   13,
      neg    =   14,
      add    =   15,
      sub    =   16,
      mul    =   17,
      div    =   18,
      rem    =   19,
      not    =   20,
      and    =   21,
      or     =   22,
      ceq    =   23,
      cne    =   24,
      clt    =   25,
      cle    =   26,
      cgt    =   27,
      cge    =   28,
      brn    =   29,
      bze    =   30,
      anew   =   31,
      halt   =   32,
      stk    =   33,
      heap   =   34,
      ldl    =   35,
      stl    =   36,
      lda_0  =   37,
      lda_1  =   38,
      lda_2  =   39,
      lda_3  =   40,
      ldl_0  =   41,
      ldl_1  =   42,
      ldl_2  =   43,
      ldl_3  =   44,
      stl_0  =   45,
      stl_1  =   46,
      stl_2  =   47,
      stl_3  =   48,
      ldc_0  =   49,
      ldc_1  =   50,
      ldc_2  =   51,
      ldc_3  =   52,
      inpc   =   53,