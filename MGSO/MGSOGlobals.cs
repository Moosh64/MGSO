using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

namespace MGSO
{
    public static class MGSOGlobals
    {
        //This should hopefully run parallel to the list in Grid_Add.
        //If it doesn't, all hope is lost.
        public static List<Script> allScripts = new List<Script>();

        //The contents of the current global script being added. Used by some of the events in the Add grid
        public static string[] loadedScript;
        public static List<string> potentialGlobalScripts = new List<string>();

        //Returns the number of instances a character in a string
        public static int CountChar(string str, char chk)
        {
            int count = 0;
            foreach (char c in str)
            {
                if (c == chk)
                    count++;
            }
            return count;
        }
        //Returns the index after the first instance of a string in another string
        //How it matches strings is a bit fuzzy, as it will ignore extra spaces and tabs
        public static int FindStartingSubstringIndex(string line, string startingStr, int startIndex=0)
        {
            int i;
            int match=0;
            int ret=0;

            char[] startingChars = startingStr.ToCharArray();
            int maxMatch = startingStr.Length-1;
            char c;

            int maxIndex = line.Length;
            for(i=startIndex; i<maxIndex; i++)
            {
                c = line.ElementAt(i);
                //Count all characters included in startingStr and increase the return value
                if (c == startingChars[match])
                {
                    match = Math.Min(match + 1, maxMatch);
                    ret = Math.Min(i + 1, maxIndex - 1);
                }
                //Increase the return value for tabs and spaces not included in startingStr
                else if (c == ' ' || c == '\t')
                {
                    ret = i+1;
                }
                else
                    break;
            }
            //If the full string was matched, return index of the first character past startingStr
            if (match >= maxMatch)
                return ret;
            return -1;
        }
        //Returns the first string after an index, ending at a space, tab, new line, or brackets
        static string FindSubstringAt(string line, int startIndex, int wordNum=1)
        {
            int i;

            char[] lineChars = line.ToCharArray();
            char c;
            List<char> ret = new List<char>();

            int wordsLeft = wordNum;
            bool inWord = true;
            int maxIndex = line.Length;
            for (i=startIndex; i<maxIndex; i++)
            {
                c = line.ElementAt(i);

                //The following characters can count as the end of a word, but are otherwise skipped
                if(c=='[' || c=='(' || c=='{' || c =='='||c==' ' || c == '\t' || c == '\n')
                {
                    if (inWord)
                        wordsLeft--;
                    inWord = false;
                    if(wordsLeft==0)
                        break;
                }
                else
                {
                    inWord = true;
                    if(wordsLeft==1)
                        ret.Add(c);
                }
            }
            return String.Join("", ret);
        }
        //Returns the first index of a string after an index, ending at a space, tab, new line, or brackets
        static int FindSubstringIndexAt(string line, int startIndex, int wordNum=1)
        {
            int i;

            char[] lineChars = line.ToCharArray();
            char c;

            int wordsLeft = wordNum;
            bool inWord = true;
            for (i = startIndex; i < line.Length; i++)
            {
                c = line.ElementAt(i);
                //The following characters can count as the end of a word, but are otherwise skipped
                if (c == '[' || c == '(' || c == '{' || c == '=' || c == ' ' || c == '\t' || c == '\n')
                {
                    if (inWord)
                        wordsLeft--;
                    inWord = false;
                    if (wordsLeft == 1)
                        break;
                }
                else
                {
                    inWord = true;
                }
            }
            return i;
        }

        //Stores a text file for later use and counts the number of global scripts in it
        public static void StoreScript(string body)
        {
            int i; int j;
            //Split input into an array and store it for later
            loadedScript = body.Split('\n');
            string line;
            int numGlobals = 0;

            potentialGlobalScripts.Clear();

            //Cycle through all lines of text to count the number of global scripts in it
            int count = loadedScript.Length;
            for(i=0; i<count; i++)
            {
                line = loadedScript[i];
                if(line.Contains("global"))
                {
                    j = FindStartingSubstringIndex(line, "global script");
                    if (j > -1)
                    {
                        //Replace carriage return to prevent a bug recognizing script names
                        potentialGlobalScripts.Add(FindSubstringAt(line.Replace("\r", ""), j));
                        numGlobals++;
                    }
                }
            }
        }
        //Stores the various sections of a script into their respective variables 
        public static int StoreScriptSections(string body, string name, string main)
        {
            int i;
            int section = (int)scriptSection.HEADER;
            int depthChange = 0;
            int depth = 0;
            int trueDepth = 0;
            bool globalFound = false;
            bool depthIssue = false;

            tempName = name;
            tempHeaders.Clear();
            tempFunctions.Clear();
            tempInit.Clear();
            tempUpdate1.Clear();
            tempUpdate2.Clear();

            //These are marker strings for each of the lists for different sections of the script
            tempHeaders.Add("//%MGSO% HEADER_START " + name + "\n");
            tempFunctions.Add("//%MGSO% FUNCTIONS_START " + name + "\n");
            tempInit.Add("//%MGSO% INIT_START " + name + "\n");
            tempUpdate1.Add("//%MGSO% UPDATE1_START " + name + "\n");
            tempUpdate2.Add("//%MGSO% UPDATE2_START " + name + "\n");

            /** 
             * 
             *   Here we're scanning through the whole script line by line to find
             *   idividual sections.
             *   
             *   HEADER: Everything above and below the global.
             *   FUNCTIONS: Functions above and below run() in the global.
             *   INIT: Everything between void run() and while(true)
             *   UPDATE1: Everything before Waitdraw()
             *   UPDATE2: Everything after Waitdraw() and before Waitframe()
             *  
             *   It can be assumed scripts will always be written in this order, 
             *   although Waitdraw() may be missing and some parts may be blank.
             *   
             *   >HEADER
             *       >FUNCTIONS
             *           >INIT
             *               >UPDATE1
             *               >UPDATE2
             *       >FUNCTIONS
             *   >HEADER
             */

            string[] lines = body.Split('\n');
            int maxIndex = lines.Length;
            for (i=0; i<maxIndex; i++)
            {
                //skipLine skips over recording a line, for fixing problems with closing braces
                bool skipLine = false;

                //Depth keeps track of the tab level, done in multiple steps
                //-depthChange gets the net total of braces per line
                //-depth applies the change
                //-trueDepth gets the result but without counting opening braces
                depthChange = CountChar(lines[i], '{') - CountChar(lines[i], '}');
                if(FindStartingSubstringIndex(lines[i], "//") != -1)
                {
                    depthChange = 0;
                }
                trueDepth = depth + Math.Min(depthChange, 0);
                depth += depthChange;
                if (Math.Abs(depthChange) > 1)
                    depthIssue = true;

                //Update section based on various things in the script.
                if (section == (int)scriptSection.UPDATE2)
                {
                    if (trueDepth == 1 && depthChange < 0) //Closing } out of run()
                    {
                        section = (int)scriptSection.FUNCTIONS;
                        skipLine = true;
                    }
                }
                else if (section == (int)scriptSection.UPDATE1) {
                    if (FindStartingSubstringIndex(lines[i], "Waitdraw(") != -1)
                    {
                        section = (int)scriptSection.UPDATE2;
                        skipLine = true;
                    }
                    else if (trueDepth == 1 && depthChange < 0) //Closing } out of run()
                    {
                        section = (int)scriptSection.FUNCTIONS;
                        skipLine = true;
                    }
                }
                else if (section == (int)scriptSection.INIT) {
                    if(FindStartingSubstringIndex(lines[i], "while(true)") != -1)
                    {
                        section = (int)scriptSection.UPDATE1;
                        //skipLine not needed because of tab level
                    }
                    else if (trueDepth == 1 && depthChange < 0) //Closing } out of run()
                    {
                        section = (int)scriptSection.FUNCTIONS;
                        skipLine = true;
                    }
                }
                else if (section == (int)scriptSection.FUNCTIONS)
                {
                    if(FindStartingSubstringIndex(lines[i], "void run") != -1)
                    {
                        section = (int)scriptSection.INIT;
                        //skipLine not needed because of tab level
                    }
                    else if (trueDepth == 0 && depthChange < 0)  //Closing } out of script
                    {
                        section = (int)scriptSection.HEADER;
                        skipLine = true;
                    }
                }
                else if (section == (int)scriptSection.HEADER && FindStartingSubstringIndex(lines[i], "global script " + main) != -1)
                {
                    globalFound = true;
                    section = (int)scriptSection.FUNCTIONS;
                    //skipLine not needed because of tab level
                }

                //If the current line isn't skipped, add lines to various lists 
                //determined by the current section
                if (!skipLine)
                {
                    switch (section)
                    {
                        case (int)scriptSection.HEADER:
                            //imports aren't included in the header section to prevent conflicts
                            if (FindStartingSubstringIndex(lines[i], "import") == -1)
                            {
                                tempHeaders.Add(lines[i]);
                            }
                            break;
                        case (int)scriptSection.FUNCTIONS:
                            if (depth >= 1 && !(depth == 1 && depthChange == 1))
                            {
                                tempFunctions.Add(lines[i]);
                            }
                            break;
                        case (int)scriptSection.INIT:
                            if (depth >= 2 && !(depth == 2 && depthChange == 1))
                            {
                                tempInit.Add(lines[i]);
                            }
                            break;
                        case (int)scriptSection.UPDATE1:
                            if (depth >= 3 && !(depth == 3 && depthChange == 1))
                            {
                                //Waitframes also aren't skipped when determining the section, so we do that here
                                if (FindStartingSubstringIndex(lines[i], "Waitframe(") == -1)
                                {
                                    tempUpdate1.Add(lines[i]);
                                }
                            }
                            break;
                        case (int)scriptSection.UPDATE2:
                            if (depth >= 3 && !(depth == 3 && depthChange == 1))
                            {
                                if (FindStartingSubstringIndex(lines[i], "Waitframe(") == -1)
                                {
                                    tempUpdate2.Add(lines[i]);
                                }
                            }
                            break;
                    }
                }
            }
            //Similar to the markers at the start, we put ones at the end.
            tempHeaders.Add("//%MGSO% HEADER_END " + name + "\n");
            tempFunctions.Add("//%MGSO% FUNCTIONS_END " + name + "\n");
            tempInit.Add("//%MGSO% INIT_END " + name + "\n");
            tempUpdate1.Add("//%MGSO% UPDATE1_END " + name + "\n");
            tempUpdate2.Add("//%MGSO% UPDATE2_END " + name + "\n");

            if (!globalFound)
            {
                return 2; //Global script not found
            }
            else if (section != (int)scriptSection.HEADER)
            {
                return 1; //Script error?
            }
            else if (depthIssue)
            {
                return 3; //Depth issue
            }
            return 0; //Everything fine
        }
        //Add a script to the list box and the script list, using values from the temp globals
        public static void AddScript(ListBox lb)
        {
            lb.Items.Add(tempName);
            Script scr = new Script();
            scr.scriptName = tempName;
            scr.scriptHeader = String.Join("\n", tempHeaders.ToArray());
            scr.scriptFunctions = String.Join("\n", tempFunctions.ToArray());
            scr.scriptInit = String.Join("\n", tempInit.ToArray());
            scr.scriptUpdate1 = String.Join("\n", tempUpdate1.ToArray());
            scr.scriptUpdate2 = String.Join("\n", tempUpdate2.ToArray());
            allScripts.Add(scr);
        }
        //Remove a script from the list box and the script list
        public static void RemScript(ListBox lb)
        {
            int index = lb.SelectedIndex;
            if (index > -1)
            {
                lb.Items.RemoveAt(index);
                allScripts.RemoveAt(index);
            }
        }
        //Load potential global script function names into the Add2 Grid listbox
        public static void LoadFunctions(string body, ListBox lb)
        {
            lb.Items.Clear();

            int i; int j;

            int depth = 0;
            int depthChange = 0;
            int trueDepth = 0;

            string[] lines = body.Split('\n');
            int maxIndex = lines.Length;
            for (i = 0; i < maxIndex; i++)
            {
                //Depth keeps track of the tab level, done in multiple steps
                //-depthChange gets the net total of braces per line
                //-depth applies the change
                //-trueDepth gets the result but without counting opening braces
                depthChange = CountChar(lines[i], '{') - CountChar(lines[i], '}');
                if (FindStartingSubstringIndex(lines[i], "//") != -1)
                {
                    depthChange = 0;
                }
                trueDepth = depth + Math.Min(depthChange, 0);
                depth += depthChange;

                //Only look for void functions at depth 0. These are the only candidates
                //for simple global script update functions, so the rest can be ignored.
                j = FindStartingSubstringIndex(lines[i], "void ");
                if (j > -1 && trueDepth == 0) 
                {
                    lb.Items.Add(FindSubstringAt(lines[i], j) + "();");
                }
            }
        }
        //Load constants in a header into the Const Grid listbox
        public static void LoadConstants(ListBox lb)
        {
            lb.Items.Clear();
            constInitValues.Clear();

            int i;
            foreach (string line in tempHeaders)
            {
                //First keyword - "const"
                i = FindStartingSubstringIndex(line, "const");
                if (i > -1)
                {
                    //Second keyword - constant's datatype. Unused.
                    //string dataType = FindSubstringAt(line, i);

                    //Third keyword - constant's name
                    string name = FindSubstringAt(line, i, 2);
                    //Fourth keyword - constant's value
                    string value = FindSubstringAt(line, i, 3);
                    value = value.Replace(";", "");
                    value = value.Replace(" ", "");

                    lb.Items.Add(name);
                    constInitValues.Add(value);
                }
            }
        }
        //Get the value of a constant
        public static string GetConstantValue(string constantName)
        {
            int i;
            foreach (string line in tempHeaders)
            {
                //First keyword - "const"
                i = FindStartingSubstringIndex(line, "const");
                if (i > -1)
                {
                    //Third keyword - constant's name
                    string name = FindSubstringAt(line, i, 2);
                    if (name == constantName)
                    {
                        //Fourth keyword - constant's value
                        string value = FindSubstringAt(line, i, 3);
                        value = value.Replace(";", "");
                        value = value.Replace(" ", "");
                        return value;
                    }
                }
            }

            return "";
        }
        //Get the description of a constant
        public static string GetConstantDescription(string constantName)
        {
            int i; int j;
            string result = "";

            string[] lines = tempHeaders.ToArray();
            int maxIndex = lines.Length;
            
            //First we find the index of the constant with constantName
            for(i=0; i<maxIndex; i++)
            {
                //First keyword - "const"
                j = FindStartingSubstringIndex(lines[i], "const");
                if (j > -1)
                {
                    //Third keyword - constant's name
                    string name = FindSubstringAt(lines[i], j, 2);
                    if (name == constantName)
                    {
                        break;
                    }
                }
            }
            
            int commentLineIndex = i;
            //If the for loop reached the end, there is no comment, return nothing
            if (i >= maxIndex)
                return "";
            //Find the start of the comment
            j = lines[i].IndexOf("//");
            if (j > -1)
            {
                //Add the first line.
                //Index increased by 2 to skip "//" because we used IndexOf instead of my own function
                j = Math.Min(j + 2, lines[i].Length - 1);
                lines[i] = lines[i].Replace("\r", "");
                lines[i] = lines[i].Replace("\n", "");
                result += GetComment(lines[i], j, false);
                if (i < maxIndex - 1)
                {
                    //Look for lone comments continuing on the next line
                    j = FindStartingSubstringIndex(lines[i + 1], "//");
                    if (j > -1 && !lines[i + 1].StartsWith("//")) //Don't count comments without any tabs in. These are most likely unrelated.
                    {
                        //Search through lines until final line is reached or break;
                        while (i < maxIndex - 1)
                        {
                            i++;
                            j = FindStartingSubstringIndex(lines[i], "//");
                            if (j > -1 && !lines[i].StartsWith("//"))
                            {
                                lines[i] = lines[i].Replace("\r", "");
                                lines[i] = lines[i].Replace("\n", "");
                                result += GetComment(lines[i], j);
                            }
                            else
                            {
                                break;
                            }
                        }
                        return result;
                    }
                }
            }
            //If there's no comment on the line of, check the lines above
            else
            {
                if (i > 0)
                {
                    j = FindStartingSubstringIndex(lines[i - 1], "//");
                    if (j > -1 && lines[i - 1].StartsWith("//")) //Only check for comments without tabs. Ones with them may be extended comments from a constant above.
                    {
                        int commentLines = 1;
                        //Search backwards through lines and record the number of comments until first line is reached or break;
                        while (i > 0)
                        {
                            i--;
                            commentLines++;
                            j = FindStartingSubstringIndex(lines[i], "//");
                            if (j == -1)
                            {
                                break;
                            }
                        }
                        //Print back the comments found above in the reverse order they were found
                        for(i=commentLineIndex-commentLines; i<commentLineIndex; i++)
                        {
                            j = FindStartingSubstringIndex(lines[i], "//");
                            //Don't copy lines without comments or MGSO tags
                            if (j > -1 && !lines[i].StartsWith("//%MGSO%")) 
                            {
                                lines[i] = lines[i].Replace("\r", "");
                                lines[i] = lines[i].Replace("\n", "");
                                result += GetComment(lines[i], j);
                            }
                        }
                        return result;
                    }
                }
            }
            return result;
        }
        //Gets one line of comments with returns if it looks like a list
        public static string GetComment(string line, int index, bool doReturn=true)
        {
            string ret = line.Substring(index) + " ";
            //Lines starting with *, -, and > characters are treated like a list and have line breaks
            if (doReturn && (ret.StartsWith("*") || ret.StartsWith("-") || ret.StartsWith(">")))
            {
                ret = "\n" + ret;
            }
            return ret;
        }
        //Sets the value of a constant
        public static void SetConstantValue(string constantName, string value)
        {
            int i; int j;
            string[] lines = tempHeaders.ToArray();

            int maxIndex = lines.Length;
            for (i = 0; i < maxIndex; i++)
            {
                //First keyword - "const"
                j = FindStartingSubstringIndex(lines[i], "const");
                if (j > -1)
                {
                    //Third keyword - constant's name
                    string name = FindSubstringAt(lines[i], j, 2);
                    if (name == constantName)
                    {
                        //Start and end strings for everything before and after the value keyword
                        string start = "";
                        string end = "";

                        //The value keyword starts at '='
                        if (lines[i].Contains('='))
                        {
                            j = lines[i].IndexOf('=');
                            if (j > 0)
                            {
                                start = lines[i].Substring(0, j);
                            }
                        }
                        //And ends at ';'
                        if (lines[i].Contains(';'))
                        {
                            j = lines[i].IndexOf(';');
                            end = lines[i].Substring(j);
                        }

                        //If all went well, update the line with the old value sandwiched between the start and end strings
                        if (start != "" && end != "")
                        {
                            lines[i] = start + "= " + value + end;
                        }
                    }
                }
            }
            tempHeaders = lines.ToList();
        }
        //Swaps the positions on two scripts on the list
        public static void SwapScript(ListBox lb, int index1, int index2)
        {
            object backupItem = lb.Items[index2];
            Script backupScript = allScripts[index2];

            lb.Items[index2] = lb.Items[index1];
            allScripts[index2] = allScripts[index1];

            lb.Items[index1] = backupItem;
            allScripts[index1] = backupScript;
        }
        //Assembles all the parts of the different globals and puts them in the final text box
        public static void FinalizeScript(ListBox lb, TextBox tb)
        {
            //New line character. This has been changed in the past, 
            //so I did this in case for whatever reason I have to change it again.
            string nl = "\r\n"; 

            tb.Text = "";
            //First add DECLARATION tags to the text box
            //This is basically a copy of Main_ListBox for MGSO to identify when loading scripts
            foreach (string s in lb.Items) {
                tb.Text += "//%MGSO% DECLARATION " + s + nl;
            }
            tb.Text += nl;

            //Add all the Header sections for scripts to the text box
            foreach(Script scr in allScripts)
            {
                tb.Text += scr.scriptHeader + nl;
            }
            tb.Text += "global script MGSO_Combined" + nl;
            tb.Text += "{" + nl;
            tb.Text += "    void run()" + nl;
            tb.Text += "    {" + nl;
            //Add all the Init sections for scripts to the text box
            foreach (Script scr in allScripts)
            {
                tb.Text += scr.scriptInit + nl;
            }
            tb.Text += "        while(true)" + nl;
            tb.Text += "        {" + nl;
            //Add all the Update1 sections for scripts to the text box
            foreach (Script scr in allScripts)
            {
                tb.Text += scr.scriptUpdate1 + nl;
            }
            tb.Text += "            Waitdraw();" + nl;
            //Add all the Update2 sections for scripts to the text box
            foreach (Script scr in allScripts)
            {
                tb.Text += scr.scriptUpdate2 + nl;
            }
            tb.Text += "            Waitframe();" + nl;
            tb.Text += "        }" + nl;
            //Add all the Functions sections for scripts to the text box
            foreach (Script scr in allScripts)
            {
                tb.Text += scr.scriptFunctions + nl;
            }
            tb.Text += "    }" + nl;
            tb.Text += "}" + nl;
        }
        //Auto intends the script
        public static void AutoIndent(TextBox tb)
        {
            int i; int j;

            int depth = 0;
            int depthChange = 0;
            int trueDepth = 0;

            bool ifStatement = false;

            string[] lines = tb.Text.Split('\n');
            int maxIndex = lines.Length;
            for(i = 0; i<maxIndex; i++)
            {
                lines[i] = lines[i].TrimStart().Replace("\r", "").Replace("\n", "");
                //Depth keeps track of the tab level, done in multiple steps
                //-depthChange gets the net total of braces per line
                //-depth applies the change
                //-trueDepth gets the result but without counting opening braces
                depthChange = CountChar(lines[i], '{') - CountChar(lines[i], '}');
                //Brackets inside comments shouldn't count towards depth
                if (FindStartingSubstringIndex(lines[i], "//") != -1)
                {
                    depthChange = 0;
                }
                trueDepth = depth + Math.Min(depthChange, 0);
                depth += depthChange;

                lines[i].Trim();
                string tabs = "";
                if (trueDepth > 0)
                {
                    //Add a number of tabs based on depth
                    for (j = 0; j < trueDepth; j++) tabs += "    ";
                }
                if (ifStatement)
                    tabs += "    ";
                lines[i] = tabs + lines[i];

                //Add a tab on the next line for if statements without brackets and when the instruction is not on the same line
                ifStatement = false;
                if (depthChange == 0 && FindStartingSubstringIndex(lines[i], "if(") != -1 && !lines[i].Contains(';')) 
                {
                    ifStatement = true;
                }
            }
            tb.Text = String.Join("\r\n", lines);
        }
        //Returns if the script has a unique name
        public static bool IsScriptNameUnique(ListBox lb, string name)
        {
            foreach(string s in lb.Items)
            {
                if (s == name)
                    return false;
            }
            return true;
        }
        //Load an existing script and use comment tags to split up the functions
        public static int FirstLoadScript(TextBox tb, ListBox lb)
        {
            lb.Items.Clear();
            allScripts.Clear();

            int i;
            string[] lines = IdiotProofScript(tb.Text).Split('\n');

            //Various variables related to what section of the script is being read
            string flsScriptName = ""; //The name of the script of the section being loaded, "" for none
            int flsSectionType = 0; //The type of the section being loaded (int)scriptSection
            int flsScriptIndex = -1; //The index in the script list for the script of the section being loaded

            string flsTempName = "";
            string flsScriptTempName = "";
            //Used to load one more line when a section reaches its end so the end tag can be loaded as well
            bool sectionEnd = false;
            //Used to catch scripts without any tags being added. 
            bool hasSectionTags = false;

            int maxIndex = lines.Length;
            for(i=0; i<maxIndex; i++)
            {
                sectionEnd = false;

                string line = lines[i].TrimStart();
                //Search for MGSO tags
                if (line.StartsWith("//%MGSO%"))
                {
                    flsTempName = FindSubstringAt(line, 0, 2);
                    //If sectionName is blank, the function is not currently inside an MGSO tag pair
                    if (flsScriptName == "")
                    {
                        switch (flsTempName)
                        {
                            //Search for DECLARATION tags first. These should all be at the top 
                            //and will add the script to the list for the rest of the tags to reference.
                            case "DECLARATION":
                                flsScriptTempName = line.Substring(FindSubstringIndexAt(line, 0, 3) + 1).Replace("\n", "").Replace("\r", "");
                                lb.Items.Add(flsScriptTempName);
                                allScripts.Add(new Script());
                                break;
                            //All other tags tell the function to start loading in lines
                            case "HEADER_START":
                                flsScriptTempName = line.Substring(FindSubstringIndexAt(line, 0, 3) + 1).Replace("\n", "").Replace("\r", "");
                                flsScriptName = flsScriptTempName;
                                flsSectionType = (int)scriptSection.HEADER;
                                flsScriptIndex = lb.Items.IndexOf(flsScriptName);
                                hasSectionTags = true;
                                break;
                            case "FUNCTIONS_START":
                                flsScriptTempName = line.Substring(FindSubstringIndexAt(line, 0, 3) + 1).Replace("\n", "").Replace("\r", "");
                                flsScriptName = flsScriptTempName;
                                flsSectionType = (int)scriptSection.FUNCTIONS;
                                flsScriptIndex = lb.Items.IndexOf(flsScriptName);
                                hasSectionTags = true;
                                break;
                            case "INIT_START":
                                flsScriptTempName = line.Substring(FindSubstringIndexAt(line, 0, 3) + 1).Replace("\n", "").Replace("\r", "");
                                flsScriptName = flsScriptTempName;
                                flsSectionType = (int)scriptSection.INIT;
                                flsScriptIndex = lb.Items.IndexOf(flsScriptName);
                                hasSectionTags = true;
                                break;
                            case "UPDATE1_START":
                                flsScriptTempName = line.Substring(FindSubstringIndexAt(line, 0, 3) + 1).Replace("\n", "").Replace("\r", "");
                                flsScriptName = flsScriptTempName;
                                flsSectionType = (int)scriptSection.UPDATE1;
                                flsScriptIndex = lb.Items.IndexOf(flsScriptName);
                                hasSectionTags = true;
                                break;
                            case "UPDATE2_START":
                                flsScriptTempName = line.Substring(FindSubstringIndexAt(line, 0, 3) + 1).Replace("\n", "").Replace("\r", "");
                                flsScriptName = flsScriptTempName;
                                flsSectionType = (int)scriptSection.UPDATE2;
                                flsScriptIndex = lb.Items.IndexOf(flsScriptName);
                                hasSectionTags = true;
                                break;
                        }
                    }
                    //If the section isn't blank, look for ending tags
                    else
                    {
                        switch (flsTempName)
                        {
                            case "HEADER_END":
                                flsScriptName = "";
                                sectionEnd = true;
                                break;
                            case "FUNCTIONS_END":
                                flsScriptName = "";
                                sectionEnd = true;
                                break;
                            case "INIT_END":
                                flsScriptName = "";
                                sectionEnd = true;
                                break;
                            case "UPDATE1_END":
                                flsScriptName = "";
                                sectionEnd = true;
                                break;
                            case "UPDATE2_END":
                                flsScriptName = "";
                                sectionEnd = true;
                                break;
                        }
                    }
                }

                line = lines[i].Replace("\n", "").Replace("\r", "");
                //If in a section or just ended, add a line to the appropriate 
                //section of the script at the scriptIndex
                if (flsScriptName != "" || sectionEnd)
                {
                    if (flsScriptIndex > -1)
                    {
                        switch (flsSectionType)
                        {
                            case (int)scriptSection.HEADER:
                                allScripts.ElementAt(flsScriptIndex).scriptHeader += line + "\n";
                                break;
                            case (int)scriptSection.FUNCTIONS:
                                allScripts.ElementAt(flsScriptIndex).scriptFunctions += line + "\n";
                                break;
                            case (int)scriptSection.INIT:
                                allScripts.ElementAt(flsScriptIndex).scriptInit += line + "\n";
                                break;
                            case (int)scriptSection.UPDATE1:
                                allScripts.ElementAt(flsScriptIndex).scriptUpdate1 += line + "\n";
                                break;
                            case (int)scriptSection.UPDATE2:
                                allScripts.ElementAt(flsScriptIndex).scriptUpdate2 += line + "\n";
                                break;
                        }
                    }
                }
            }

            //If the end of the textbox has been reached and the current section hasn't closed out, return an error
            if (flsScriptName != "")
            {
                string part = "";
                switch (flsSectionType)
                {
                    case (int)scriptSection.HEADER:
                        part = "HEADER_END";
                        break;
                    case (int)scriptSection.FUNCTIONS:
                        part = "FUNCTIONS_END";
                        break;
                    case (int)scriptSection.INIT:
                        part = "INIT_END";
                        break;
                    case (int)scriptSection.UPDATE1:
                        part = "UPDATE1_END";
                        break;
                    case (int)scriptSection.UPDATE2:
                        part = "UPDATE2_END";
                        break;
                }
                MessageBox.Show("MGSO script tag missing:\n//%MGSO% " + part + " " + flsScriptName, "Error loading script");
                return 1;
            }
            //Return an error if no tags were found.
            else if (!hasSectionTags)
            {
                MessageBox.Show("This script doesn't have any MGSO section tags. No part of it could be identified. Please select \"Start Fresh\" instead.", "Error loading script");
                return 1;
            }
            return 0;
        }
        //Returns a string with end comments removed
        public static string StripComments(string body)
        {
            int i = body.IndexOf("//");
            if (i > -1)
                body = body.Substring(0, i);
            return body;
        }
        //Try to fix stupid shit people might've done to a script that might interfere with it loading correctly
        public static string IdiotProofScript(string body)
        {
            int i;
            int depthChange;

            string[] lines = body.Split('\n');
            int maxIndex = lines.Length;
            for (i = 0; i < maxIndex; i++)
            {
                string line = StripComments(lines[i]);
                depthChange = CountChar(lines[i], '{') - CountChar(lines[i], '}');
                //depthChange should never increase or decrease by > 1.
                //Add linebreaks if this happens.
                if (Math.Abs(depthChange) > 1)
                    lines[i] = lines[i].Replace("{", "\n{").Replace("}", "\n}");
                //A line with more closing braces than opening should not have anything before the brace.
                //Add linebreaks if this happens.
                if (Math.Abs(depthChange) >= 1)
                {
                    if(line.Contains('}')&&!line.Trim().EndsWith("}"))
                        lines[i] = lines[i].Replace("}", "\n}");
                }
            }
            return String.Join("\n", lines);
        }

        //Temporary storage for script sections
        public static string tempName;
        public static Script tempScript;
        public static List<string> tempHeaders = new List<string>();
        public static List<string> tempFunctions = new List<string>();
        public static List<string> tempInit = new List<string>();
        public static List<string> tempUpdate1 = new List<string>();
        public static List<string> tempUpdate2 = new List<string>();
        
        public static List<string> constInitValues = new List<string>();

        public enum scriptSection : int { HEADER, FUNCTIONS, INIT, UPDATE1, UPDATE2 };
    }
}
