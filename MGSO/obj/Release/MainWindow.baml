   M S B A M L   `   `   `  ���� ?  ;MGSO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:MGSOMGSO      MGSO.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     $localclr-namespace:MGSO       � Title$#  Moosh Global Script Organizer��$	��512��$	��512�� � Icon$ MGSO.ico�� � 
ResizeMode$ NoResize=�.��� ���	#FF202020.+�� -    �Name$ 	Grid_Main��$
��Left=�$	��462��$��	10,10,0,0q�$	��Top=�$	��486��.+��� -    ��Name$ Main_ListBox��$
��Left=�$	��404��$��	28,32,0,0q�$	��Top=�$	��245��
���   ����	#FF5B71B4
��������� -    ��Name$ Main_AddButton��$	��Add��$
��Left=�$��
343,32,0,0q�$	��Top=�$��75���� -   $ Main_RemButton��$��Remove��$
��Left=�$��
343,84,0,0q�$	��Top=�$��75���� -   $ Main_ConstButton��$��	Constants��$
��Left=�$��343,134,0,0q�$	��Top=�$��75���� -   $ Main_SwapButton��$
��Swap��$
��Left=�$��343,184,0,0q�$	��Top=�$��75���� -    ��Name$ Main_SwapTextBox��$
��Left=�$��23��$��322,208,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��120��
���   �
������� � ToolTip$m gPut the name of the script you want to swap here. Press Swap to swap places with the current selection.���� -   	 ��Name$	 Main_AutoTabCheckbox��$��Auto Indent��$
��Left=�$��331,364,0,0q�$	��Top=�
�������$	��102��$S MIf this is checked the finished global script will automatically redo tabbing���� -	   $ Main_FinishButton��$��Finish��$
��Left=�$��322,306,0,0q�$	��Top=�$	��120��$��44��� -
   $ 
Grid_Intro��$
��Left=�$	��462��$��	10,10,0,0q�$	��Top=�$	��486��.+��� -   
 ��Name$
 Intro_Label��$&�� Use existing MGSO global script?��$
��Left=�$��	10,10,0,0q�$	��Top=�
�������$	��466��$��29��$��Center=�$
��Bold"��� -   $ Intro_YesButton��$	��Yes��$
��Left=�$��
133,54,0,0q�$	��Top=�$��75���� -   $ Intro_NoButton��$��Start Fresh��$
��Left=�$��
291,54,0,0q�$	��Top=�$��75���� -   $ Intro_TextBox��$
��Left=�$	��363��$��	10,89,0,0q�$	��Top=�$	��466��
���   �
�������$� �If you've used MGSO to generate a combined global script before, you can paste it in here to edit it. Global scripts without MGSO tags won't be recognized.��" �� VerticalScrollBarVisibility$
 Auto=� �� AcceptsReturn .  �� 
AcceptsTab . $��Courier New)� �� TextDecorations Z� � -   $ Grid_Add��$
��Left=�$	��462��$��	10,10,0,0q�$	��Top=�$	��486��$y�Visible=�.+��� -   $
 
Add_Label1��$��Script Name��$
��Left=�$��	10,10,0,0q�$	��Top=�
�������$	��106��$
��Bold"��� -   $ Add_AddScriptButton��$��
Add Script��$
��Left=�$��
401,35,0,0q�$	��Top=�$��75���� -   $
 
Add_Label2��$��Script Body��$
��Left=�$��	10,59,0,0q�$	��Top=�
�������$	��106��$
��Bold"��� -   $ Add_ScriptBody��$
��Left=�$	��339��$��	10,89,0,0q�$	��Top=�$	��466��
���   �
������� .  . $? 9Paste your global script from the database into this box.��$
 Auto=�$��Courier New)��� -   $ Add_ScriptName��$
��Left=�$��19��$��	10,35,0,0q�$	��Top=�$	��386��
���   �
�������$d ^Give a unique name to your script here. This is used to label and identify it by this program.���� -   $
 
Add_Label3��$��Main2��$
��Left=�$��
272,59,0,0q�$	��Top=�$
��Bold"�
��������� -   $ Add_ScriptMain2��$
��Left=�$��19��$��
322,63,0,0q�$	��Top=�$	��154��
���   �
�������$� �If there's multiple global scripts in your paste, put the name of the one that goes in global slot 2 in here. Otherwise leave it blank.���� -   $ Add_CancelButton��$��Cancel��$
��Left=�$��206,433,0,0q�$	��Top=�$��75��� -   $ 	Grid_Add2��$
��Left=�$	��462��$��	10,10,0,0q�$	��Top=�$	��486��.+��� -   $ Add2_ListBox��$
��Left=�$	��310��$��	28,88,0,0q�$	��Top=�$	��140��
���   �
��������� -   $ Add2_SetInitButton��$��=>��$
��Left=�$��187,146,0,0q�$	��Top=�$��29���� -   $ Add2_SetInitTextBox��$
��Left=�$��66��$��236,122,0,0q�$
 Wrap=�$	��Top=�$	��212��
���   �
������� .  . $
 Auto=�$��Courier New)��� -   $
 Add2_InitLabel��$��Init:��$
��Left=�$��
236,88,0,0q�$	��Top=�
�������$
��Bold"�$	��212���� -   $ Add2_SetUpdate1Button��$��=>��$
��Left=�$��187,251,0,0q�$	��Top=�$��29���� -   $ Add2_SetUpdate1TextBox��$
��Left=�$��66��$��236,227,0,0q�$
 Wrap=�$	��Top=�$	��212��
���   �
������� .  . $
 Auto=�$��Courier New)��� -   $
 Add2_SetUpdate1Label��$��	Update 1:��$
��Left=�$��236,193,0,0q�$	��Top=�
�������$
��Bold"�$	��212���� -    $ Add2_SetUpdate2Button��$��=>��$
��Left=�$��187,356,0,0q�$	��Top=�$��29���� -!   $ Add2_SetUpdate2TextBox��$
��Left=�$��66��$��236,332,0,0q�$
 Wrap=�$	��Top=�$	��212��
���   �
������� .  . $
 Auto=�$��Courier New)��� -"   $
 Add2_SetUpdate2Label��$��	Update 2:��$
��Left=�$��236,298,0,0q�$	��Top=�
�������$
��Bold"�$	��212���� -#   $ Add2_CancelButton��$��Cancel��$
��Left=�$��
93,420,0,0q�$	��Top=�$��75���� -$   $ Add2_ConfirmButton��$��Confirm��$
��Left=�$��310,420,0,0q�$	��Top=�$��75���� -%   $
 Add2_Instructions��$����Set the following fields to the appropriate parts of the global script. 
Hopefully the script has functions for this. Else, just paste whatever it
says in the script setup page.��$
��Left=�$��61��$��	28,20,0,0q�$	��Top=�$	��420��
�������$��Disabled=�$
��Bold"�� -&   $ 
Grid_Const��$
��Left=�$	��462��$��	10,10,0,0q�$	��Top=�$	��486��.+��� -'   $ Const_ListBox��$
��Left=�$	��411��$��	22,24,0,0q�$	��Top=�$	��189��
���   �
��������� -(   $ Const_Value��$
��Left=�$��23��$��
216,54,0,0q�$	��Top=�$	��171��
���   �
������� �� MaxLines$ 1��$ Disabled=��� -)   $ Const_Description��$
��Left=�$	��289��$��
216,82,0,0q�$
 Wrap=�$	��Top=�$	��251��
����������	#00000000���	#00ABADB3$� �NOTE: This description is an approximation read from comments in the code. Information may be missing or incorrect. If that happens, consult the script's setup instructions.�� �� SelectionBrush �	#000078D7 �� 
IsReadOnly .  �� IsUndoEnabled .  |�.  $
 Auto=�$��0q��� -*   $ 
Const_Done��$
��Done��$
��Left=�$��312,396,0,0q�$	��Top=�$��75���� -+    ��Name$ 
Const_Name��$
��Left=�$��
216,32,0,0q�$
��Wrap=�$	��Top=�$	��251��
�������$
��Bold"��� -,   $ Const_Revert��$��Revert��$
��Left=�$��
392,54,0,0q�$	��Top=�$��75��� --   $ 
Grid_Final��$
��Left=�$	��462��$��	10,10,0,0q�$	��Top=�$	��486��.+��� -.   $ Final_ResultTextBox��$
��Left=�$	��373��$��	10,10,0,0q�$	��Top=�$	��466��
���   �
�������$��Courier New)�$��Expanded&� .  . $
 Auto=��� -/   $ Final_GoBackButton��$��Go Back��$
��Left=�$��
10,414,0,0q�$	��Top=�$	��150���� -0   $! Final_CopyToClipboardButton��$��Copy to Clipboard��$
��Left=�$��326,415,0,0q�$	��Top=�$	��150��