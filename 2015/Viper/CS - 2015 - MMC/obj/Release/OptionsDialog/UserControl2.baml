   M S B A M L   `   `   `  ���� 	  UIAPI-    'Revit.SDK.Samples.UIAPI.CS.UserControl2   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     .��� 
�������.+��� -   $��stackPanel1��$	��512��$	��Top=�$
��Left=�$	��432��.+��� -   $��	groupBox1��$��Text Visibility��$��88��$	��418��.��� .+�� -   $��grid1��$��65��$
��Left=�$	��Top=�$	��406��  � RowDefinitions  �� �� ��  � ColumnDefinitions �� $	��27* ��� $
��383* �.+��� -   $��	checkBox1��$��16��$
��Left=�$��6,6,0,0q�$	��Top=� d� 	IsChecked$
 TrueY��� -   $��	checkBox2��$��1��$��1��$��1��$��16��$
��Left=�$��6,6,0,0q�$	��Top=�$
 TrueY��� -   $��	checkBox3��$��1��$��2��$��16��$
��Left=�$��6,6,0,0q�$	��Top=�$
 TrueY��� -   $��
textBlock1��$��1��$��1��$��23��$
��Left=�$��0,4,0,0q�$5��/Show tool messages (always ON for basic wheels)��$	��Top=��� -   $��
textBlock2��$��1��$��1��$��1��$��23��$
��Left=�$��0,4,0,0q�$0��*Show tooltips (always ON for basic wheels)��$	��Top=��� -	   $��
textBlock3��$��1��$��2��$��23��$
��Left=�$��0,4,0,0q�$8��2Show tool cursor text (always ON for basic wheels)��$	��Top=��� -
   $��	groupBox2��$#��Big Steering Wheel Appearance��$��64��$	��422��.��� .+�� -   $��grid2��$��41��$
��Left=�$	��Top=�$	��410�� �� $
��102* ��� $
��134* ��� $
��113* ��� $	��60* �.+��� -   $��
textBlock4��$��22��$
��Left=�$��	69,10,0,0q�$��Size:��$	��Top=�$��33���� -   $��	comboBox1��$��1��$��23��$
��Left=�$��1,10,0,0q�$	��Top=�$	��133��.@��� $��Small���� $��Normal�� �� 
IsSelected . �� $��Large���� -   $��
textBlock5��$��2��$��25��$
��Left=�$��	65,10,0,0q�$��Opacity:��$	��Top=�$��48���� -   $��	comboBox2��$��3��$��23��$
��Left=�$��0,8,0,0q�$	��Top=�$��54��.@��� $	��25%���� $	��50%�� . �� $	��75%���� $	��90%���� -   $��	groupBox3��$��Mini Wheel Appearance��$��64��$	��422��.��� .+�� -   $��grid3��$��41��$
��Left=�$	��Top=�$	��410�� �� $
��102* ��� $
��134* ��� $
��113* ��� $	��60* �.+��� -   $��
textBlock6��$��22��$
��Left=�$��	69,10,0,0q�$��Size:��$	��Top=�$��33���� -   $��	comboBox3��$��1��$��23��$
��Left=�$��1,10,0,0q�$	��Top=�$	��133��.@��� $��Small���� $��Normal�� . �� $��Large���� -   $��
textBlock7��$��2��$��25��$
��Left=�$��	65,10,0,0q�$��Opacity:��$	��Top=�$��48���� -   $��	comboBox4��$��3��$��23��$
��Left=�$��0,8,0,0q�$	��Top=�$��54��.@��� $	��25%���� $	��50%�� . �� $	��75%���� $	��90%���� -   $��	groupBox4��$��Look Tool Behavior��$��64��$	��422��.��� .+�� -   $��grid4��$��41��$
��Left=�$	��Top=�$	��410�� �� $	��29* ��� $
��381* �.+��� -   $��	checkBox4��$��2��$��16��$
��Left=�$��8,14,0,0q�$	��Top=��� -   $��
textBlock8��$��1��$��30��$
��Left=�$��0,12,0,0q�$7��1Invert vertical axis (Pull mouse back to look up)��$	��Top=�$	��321���� -   $��	groupBox5��$��	Walk Tool��$	��102��$	��422��.��� .+�� -   $��grid5��$��79��$
��Left=�$	��Top=�$	��410��$y�Visible=�  �� �� ��  �� $	��55* ��� $	��26* ��� $
��236* ��� $��33 ��� $	��60* �.+��� -   $��	checkBox5��$��16��$
��Left=�$��6,6,0,0q�$	��Top=�$
 TrueY��� -   $��
textBlock9��$��1��$��3��$��26��$
��Left=�$��0,4,0,0q�$#��Move parallel to ground plane��$	��Top=�$	��321���� -   $��textBlock10��$��1��$��3��$��1��$��27��$
��Left=�$��Speed Factor:��$	��Top=�$	��321���� -   $��slider1��$��2��$��2��$��26��$
��Left=�$��1,0,0,0q�$	��Top=�$	��229���� -    $��textBlock11��$��1��$��2��$��26��$
��Left=�$��1,6,0,0q�$	��0.1��$��Center=�$��47���� -!   $��textBlock12��$��3��$��2��$��23��$
��Left=�$
��10.0��$��Center=�$��0,3,0,0q��� -"   $��textBox1��$��4��$��2��$��20��$
��Left=�$��3,0,0,0q�$	��Top=�$��41��$��3���� -#   $��	groupBox6��$��	Zoom Tool��$��64��$	��422��.��� .+�� -$   $��grid6��$��41��$
��Left=�$	��Top=�$	��410�� �� $	��29* ��� $
��381* �.+��� -%   $��	checkBox6��$��2��$��16��$
��Left=�$��8,14,0,0q�$	��Top=��� -&   $��textBlock13��$��1��$��23��$
��Left=�$��0,12,0,0q�$2��,Zoom in one increment with each mouse click ��$	��Top=�$	��381���� -'   $��	groupBox7��$��
Orbit Tool��$��64��$	��422��.��� .+�� -(   $��grid7��$��41��$
��Left=�$	��Top=�$	��410�� �� $	��29* ��� $
��381* �.+��� -)   $��	checkBox7��$��2��$��16��$
��Left=�$��8,14,0,0q�$	��Top=�$
 TrueY��� -*   $��textBlock14��$��1��$��30��$
��Left=�$��0,12,0,0q�$��Keep scene upright��$	��Top=�$	��321��