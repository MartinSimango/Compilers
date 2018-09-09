#!/bin/bash
# Parse the file 
wine cmd /c coco $1.atg #-options m $2 $3 $4 $5 $6

if [ "$?" -ne "0" ]
then
    rm listing.txt
    exit 0
fi

# Compile the file 
if [ -f $1/CodeGen.cs  ]
then
	mcs $1.cs Parser.cs Scanner.cs Library.cs $1/*.cs > errors
    echo "---Using Parva directory---"
else
	mcs $1.cs Parser.cs Scanner.cs Library.cs > errors
    echo "NOT USING PARVA DIRECTORY"
fi

if [ $? -eq 0 ]
then 
    echo "Compiled succesfully"
else
    echo "Something went wrong"
fi

chmod +x Parva.exe
