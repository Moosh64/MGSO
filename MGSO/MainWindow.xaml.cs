using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MGSO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Setting default show/hide values on the grids themselves makes them tedious to edit.
            //So I'm setting it here instead.
            Grid_Main.IsEnabled = false;
            Grid_Main.Visibility = Visibility.Hidden;

            Grid_Add.IsEnabled = false;
            Grid_Add.Visibility = Visibility.Hidden;

            Grid_Add2.IsEnabled = false;
            Grid_Add2.Visibility = Visibility.Hidden;

            Grid_Const.IsEnabled = false;
            Grid_Const.Visibility = Visibility.Hidden;

            Grid_Final.IsEnabled = false;
            Grid_Final.Visibility = Visibility.Hidden;
        }

        //Intro Grid - Gives the option to paste an existing script with MGSO tags for updates
        private void Intro_YesButton_Click(object sender, RoutedEventArgs e)
        {
            //Load the contents of the text box into slots. If an error occurs, will produce a popup, else returns 0
            int result = MGSOGlobals.FirstLoadScript(Intro_TextBox, Main_ListBox);

            if (result == 0)
            {
                Grid_Intro.IsEnabled = false;
                Grid_Intro.Visibility = Visibility.Hidden;

                Grid_Main.IsEnabled = true;
                Grid_Main.Visibility = Visibility.Visible;
            }
        }
        private void Intro_NoButton_Click(object sender, RoutedEventArgs e)
        {
            Grid_Intro.IsEnabled = false;
            Grid_Intro.Visibility = Visibility.Hidden;
            
            Grid_Main.IsEnabled = true;
            Grid_Main.Visibility = Visibility.Visible;
        }

        //Main Grid - A list of global scripts with buttons to add/remove/view constants/swap
        private void Main_AddButton_Click(object sender, RoutedEventArgs e)
        {
            Add_ScriptName.Text = "";
            Add_ScriptBody.Text = "";
            Add_ScriptMain2.Text = "";

            Grid_Main.IsEnabled = false;
            Grid_Main.Visibility = Visibility.Hidden;

            Grid_Add.IsEnabled = true;
            Grid_Add.Visibility = Visibility.Visible;
        }
        private void Main_RemButton_Click(object sender, RoutedEventArgs e)
        {
            MGSOGlobals.RemScript(Main_ListBox);
        }
        private void Main_ConstButton_Click(object sender, RoutedEventArgs e)
        {
            if (Main_ListBox.SelectedIndex > -1)
            {
                Grid_Main.IsEnabled = false;
                Grid_Main.Visibility = Visibility.Hidden;

                Grid_Const.IsEnabled = true;
                Grid_Const.Visibility = Visibility.Visible;

                MGSOGlobals.tempScript = MGSOGlobals.allScripts.ElementAt(Main_ListBox.SelectedIndex);
                
                MGSOGlobals.tempHeaders = MGSOGlobals.tempScript.scriptHeader.Split('\n').ToList();
                MGSOGlobals.LoadConstants(Const_ListBox);
            }
        }
        private void Main_SwapButton_Click(object sender, RoutedEventArgs e)
        {
            //Check if selected item is valid
            if (Main_ListBox.SelectedIndex > -1)
            {
                int swapIndex = Main_ListBox.Items.IndexOf(Main_SwapTextBox.Text);
                //Check if swap item is valid
                if (swapIndex > -1)
                {
                    MGSOGlobals.SwapScript(Main_ListBox, Main_ListBox.SelectedIndex, swapIndex);
                }
                else
                {
                    //If it isn't valid, it could be empty or just entered wrong
                    if(Main_SwapTextBox.Text == "")
                        MessageBox.Show("Please enter the name of the script to swap in the text box below and select the other script in the dropdown.", "An error occurred");
                    else
                        MessageBox.Show("Script by the name of " + Main_SwapTextBox.Text + " not found.", "An error occurred");
                }
            }
        }
        private void Main_FinishButton_Click(object sender, RoutedEventArgs e)
        {
            MGSOGlobals.FinalizeScript(Main_ListBox, Final_ResultTextBox);
            if (Main_AutoTabCheckbox.IsChecked == true)
                MGSOGlobals.AutoIndent(Final_ResultTextBox);

            Grid_Main.IsEnabled = false;
            Grid_Main.Visibility = Visibility.Hidden;

            Grid_Final.IsEnabled = true;
            Grid_Final.Visibility = Visibility.Visible;

            Final_CopyToClipboardButton.IsEnabled = true;
            Final_CopyToClipboardButton.Visibility = Visibility.Visible;
        }

        //Add Grid - First step of the script adding process. Paste in a script and give it a name
        private void Add_AddScriptButton_Click(object sender, RoutedEventArgs e)
        {
            Add_ScriptName.Text = Add_ScriptName.Text.Trim();
            //Ensure the script has a name
            if (Add_ScriptName.Text == "")
            {
                MessageBox.Show("You need to give this script a name.", "An error occurred");
                return;
            }
            //Ensure the name is unique
            else if (!MGSOGlobals.IsScriptNameUnique(Main_ListBox, Add_ScriptName.Text))
            {
                MessageBox.Show("Two scripts cannot have the same name.", "Duplicate script");
                return;
            }

            //Put the text box in a global array
            MGSOGlobals.StoreScript(Add_ScriptBody.Text);
            string mainGlobalScript = "";
            //If a main global script has been specified, use that one
            if (MGSOGlobals.potentialGlobalScripts.Contains(Add_ScriptMain2.Text))
            {
                mainGlobalScript = Add_ScriptMain2.Text;
            }
            //If a main global isn't specified and there's too  many scripts, return an error
            if (MGSOGlobals.potentialGlobalScripts.Count > 1 && mainGlobalScript=="")
            {
                int count = 0;
                string test = ""; //String that will contain all possible main global scripts listed
                foreach (string s in MGSOGlobals.potentialGlobalScripts)
                {
                    count++;
                    if(count==1)
                        test += s;
                    else  if(count== MGSOGlobals.potentialGlobalScripts.Count) //Last script needs an and
                    {
                        if(count==2) //The second script in a list of two doesn't need a comma seperating it
                            test += " and " + s;
                        else
                            test += ", and " + s;
                    }
                    else //Scripts past the first need a comma
                        test += ", " + s;
                } //Curse you, grammar!
                MessageBox.Show("MGSO has found the following global scripts: " + test + ". Please select the active (slot 2) global script from these options.", "Multiple global scripts found");
            }
            //Otherwise resume loading the script
            else
            {
                //Store the sections of a script to temp globals and return the result
                int result = MGSOGlobals.StoreScriptSections(MGSOGlobals.IdiotProofScript(Add_ScriptBody.Text), Add_ScriptName.Text, mainGlobalScript);
                if (result == 1) //Failure reading script
                {
                    MessageBox.Show("Not all parts of the global script could be read correctly.", "An error occurred");
                }
                else if (result == 2) //No example script, proceed to Add2 Grid
                {
                    MessageBoxResult mbr = MessageBox.Show("There was no example global script included. Try using functions?", "An error occurred", MessageBoxButton.YesNo);
                    if(mbr == MessageBoxResult.Yes)
                    {
                        MGSOGlobals.LoadFunctions(String.Join("\r\n", MGSOGlobals.tempHeaders.ToArray()), Add2_ListBox);

                        Grid_Add.IsEnabled = false;
                        Grid_Add.Visibility = Visibility.Hidden;

                        Grid_Add2.IsEnabled = true;
                        Grid_Add2.Visibility = Visibility.Visible;
                    }
                }
                else if (result == 3) //Depth issue. Hopefully resolved.
                {
                    MessageBox.Show("Something went wrong while loading the script. You probably shouldn't be seeing this message. If you are, you should maybe report it. This script you're trying to use was probably written by a madman.", "An error occurred");
                }
                else if(result == 0) //Success, return to Main Grid
                {
                    MGSOGlobals.AddScript(Main_ListBox);
                    Add_ScriptBody.Text = "";
                    Add_ScriptName.Text = "";
                    Add_ScriptMain2.Text = "";

                    Grid_Add.IsEnabled = false;
                    Grid_Add.Visibility = Visibility.Hidden;

                    Grid_Main.IsEnabled = true;
                    Grid_Main.Visibility = Visibility.Visible;
                }
            }
        }
        private void Add_CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Add_ScriptBody.Text = "";
            Add_ScriptName.Text = "";
            Add_ScriptMain2.Text = "";

            Grid_Add.IsEnabled = false;
            Grid_Add.Visibility = Visibility.Hidden;

            Grid_Main.IsEnabled = true;
            Grid_Main.Visibility = Visibility.Visible;
        }

        //Add2 Grid - Second optional step of the script adding process. If the script has no example global, try and create one
        private void Add2_SetInitButton_Click(object sender, RoutedEventArgs e)
        {
            //Sets the Init function to the current selected list item
            if (Add2_ListBox.SelectedIndex != -1)
                Add2_SetInitTextBox.Text = Add2_ListBox.SelectedItem.ToString();
        }
        private void Add2_SetUpdate1Button_Click(object sender, RoutedEventArgs e)
        {
            //Sets the Updat1 function to the current selected list item
            if (Add2_ListBox.SelectedIndex != -1)
                Add2_SetUpdate1TextBox.Text = Add2_ListBox.SelectedItem.ToString();
        }
        private void Add2_SetUpdate2Button_Click(object sender, RoutedEventArgs e)
        {
            //Sets the Updat2 function to the current selected list item
            if (Add2_ListBox.SelectedIndex != -1)
                Add2_SetUpdate2TextBox.Text = Add2_ListBox.SelectedItem.ToString();
        }
        private void Add2_CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Add2_SetInitTextBox.Text = "";
            Add2_SetUpdate1TextBox.Text = "";
            Add2_SetUpdate2TextBox.Text = "";

            Grid_Add2.IsEnabled = false;
            Grid_Add2.Visibility = Visibility.Hidden;

            Grid_Add.IsEnabled = true;
            Grid_Add.Visibility = Visibility.Visible;
        }
        private void Add2_ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            //Reset temp global lists and give them the data from the text boxes
            MGSOGlobals.tempInit.Clear();
            MGSOGlobals.tempInit.Add("//%MGSO% INIT_START " + MGSOGlobals.tempName + "\r\n");
            MGSOGlobals.tempInit.Add(Add2_SetInitTextBox.Text + "\r\n");
            MGSOGlobals.tempInit.Add("//%MGSO% INIT_END " + MGSOGlobals.tempName + "\r\n");

            MGSOGlobals.tempUpdate1.Clear();
            MGSOGlobals.tempUpdate1.Add("//%MGSO% UPDATE1_START " + MGSOGlobals.tempName + "\r\n");
            MGSOGlobals.tempUpdate1.Add(Add2_SetUpdate1TextBox.Text + "\r\n");
            MGSOGlobals.tempUpdate1.Add("//%MGSO% UPDATE1_END " + MGSOGlobals.tempName + "\r\n");

            MGSOGlobals.tempUpdate2.Clear();
            MGSOGlobals.tempUpdate2.Add("//%MGSO% UPDATE2_START " + MGSOGlobals.tempName + "\r\n");
            MGSOGlobals.tempUpdate2.Add(Add2_SetUpdate2TextBox.Text + "\r\n");
            MGSOGlobals.tempUpdate2.Add("//%MGSO% UPDATE2_END " + MGSOGlobals.tempName + "\r\n");
        
            //Add the script with the new Init, Update1, and Update2
            MGSOGlobals.AddScript(Main_ListBox);

            Add2_SetInitTextBox.Text = "";
            Add2_SetUpdate1TextBox.Text = "";
            Add2_SetUpdate2TextBox.Text = "";

            Grid_Add2.IsEnabled = false;
            Grid_Add2.Visibility = Visibility.Hidden;

            Grid_Main.IsEnabled = true;
            Grid_Main.Visibility = Visibility.Visible;
        }

        //Const Grid - A list of constants with a description and a box to change the value
        private void Const_ListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if(Const_ListBox.SelectedIndex != -1)
            { 
                string name = Const_ListBox.SelectedItem.ToString();
                Const_Name.Text = name;
                Const_Value.Text = MGSOGlobals.GetConstantValue(name);
                Const_Description.Text = MGSOGlobals.GetConstantDescription(name);
            }
        }
        private void Const_Done_Click(object sender, RoutedEventArgs e)
        {
            MGSOGlobals.tempScript.scriptHeader = String.Join("\n", MGSOGlobals.tempHeaders);

            Const_Name.Text = "";
            Const_Value.Text = "";
            Const_Description.Text = "";

            Grid_Const.IsEnabled = false;
            Grid_Const.Visibility = Visibility.Hidden;

            Grid_Main.IsEnabled = true;
            Grid_Main.Visibility = Visibility.Visible;
        }
        private void Const_Revert_Click(object sender, RoutedEventArgs e)
        {
            //Reverts a constant back to what it was when the Const Grid was first opened
            if (Const_ListBox.SelectedIndex > -1)
            {
                Const_Value.Text = MGSOGlobals.constInitValues.ElementAt(Const_ListBox.SelectedIndex);
            }
        }
        private void Const_Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            //If the selected constant is valid, update the values in the script
            if (Const_ListBox.SelectedIndex > -1)
            {
                //Exclude changes that would clear or add confusing characters to it
                //I also wanted to have a sanity check for the type of constant (Decimal, Hex, Binary) 
                //to only allow valid numbers, but this caused stack overflows I couldn't figure the cause of.
                if (Const_Value.Text.Trim() != "" && !Const_Value.Text.Contains(';') && !Const_Value.Text.Contains('='))
                {
                    MGSOGlobals.SetConstantValue(Const_ListBox.SelectedItem.ToString(), Const_Value.Text);
                }
            }
        }

        //Final Grid - A box with the combined global script that can be copied to the clipboard
        private void Final_GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            Grid_Final.IsEnabled = false;
            Grid_Final.Visibility = Visibility.Hidden;

            Grid_Main.IsEnabled = true;
            Grid_Main.Visibility = Visibility.Visible;
        }
        private void Final_CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            //Make sure the text being copied to the clipboard is ASCII only.
            //This fixes a bug where scripts were crashing or hanging ZQuest
            //due to spooky special characters.
            string text = Regex.Replace(Final_ResultTextBox.Text, @"[^\u0000-\u007F]+", string.Empty);
            System.Windows.Clipboard.SetText(text, TextDataFormat.Text);

            Final_CopyToClipboardButton.IsEnabled = false;
            Final_CopyToClipboardButton.Visibility = Visibility.Hidden;

        }
    }
}
