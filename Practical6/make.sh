#!/bin/bash
# Parse the file 
wine cmd /c coco $1.atg

if [ "$?" -ne "0" ]
then
    exit 0
fi

# Compile the file 
mcs $1.cs Parser.cs Scanner.cs assem/CodeGen.cs assem/PVM.cs assem/Table.cs Library.cs

if [ $? -eq 0 ]
then 
    echo "Compiled succesfully"
else
    echo "Something went wrong"
fi
