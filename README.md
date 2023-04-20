# OwnershipStealer

This C# code is a console application that attempts to take ownership of a specified file.

## Code Logic
The program first checks if a file name was specified as a command line argument, and if not, prompts the user to enter one. It then proceeds to try and take ownership of the file by performing the following steps:

* Get the current process token.
* Lookup the LUID (Locally Unique Identifier) for the SeTakeOwnershipPrivilege.
* Enable the SeTakeOwnershipPrivilege in the token.
* Attempt to take ownership of the file by creating a new FileSecurity object for the file, setting the owner to the current user, and then setting the new access control on the file.
* If the program encounters an UnauthorizedAccessException, it will retry until the ownership of the file has been successfully taken or an exception is thrown that prevents the program from continuing.

Once ownership has been taken, the program ends and displays a message indicating that it has finished.

## Usage
* Clone or download the repository.
* Open the project in Visual Studio.
* Build the project.
* Open the command prompt or terminal and navigate to the location of the executable file.
* Execute the command with the file path as an argument:
* OwnershipStealer.exe [file path]
* If no file path is provided, the program will prompt for one.

## Video Proof-of-Concept
https://youtu.be/sOcJp58sblM
