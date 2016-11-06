
// Formats an error string for output
function __errorString(err)
{
    var errString = "";
  
    if((err != null) && (err.name != null))
        errString += ("    " + err.name + ": ");
    else
        errString += "    ERROR: ";

    if((err != null) && (err.message != null))
        errString += err.message;

    if((err != null) && (err.number != null))
        errString += (" (" + (err.number & 0xFFFF) + ")");

    return errString;
}

// Formats an operation string for output
function __opString(op)
{
    var opString = "Executing - ";
    // We don't want to print out the parameters because then Schemer will record every test case because 
    // the output we print out will be unique for each test case
    if(op != null)
    {
        if(op.indexOf("(") != -1)   // If it is a method, get the name
        {
            opString += (op.substring(0, op.indexOf("(")));
        }
        else if((op.substring(0, 3) == "obj") && (op.indexOf(" =") != -1)) // If it is a property set, get the property name
        {
            opString += (op.substring(0, op.indexOf(" =")));
        }
        else    // Just print
        {
            opString += (op);
        }
    }
    return opString;
}

// Output a message
function __output(mes)
{
    WScript.StdOut.write(mes);
}

// Output a message with a line break
function __outputLn(mes)
{
    WScript.StdOut.WriteLine(mes);
}

try
{

var obj = new ActiveXObject("%PROGID%")
%SCRIPT%

}
catch(e)
{
    __outputLn("An error occurred: ");
    __outputLn(e.message);
}