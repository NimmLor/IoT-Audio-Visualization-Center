   M S B A M L   `   `   `  ����C  ?Analyzer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:AnalyzerAnalyzer  5   	       Analyzer.Networkscanner   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   H DMahApps.Metro, Version=1.6.5.1, Culture=neutral, PublicKeyToken=null6mah,http://metro.mahapps.com/winfx/xaml/controls  5   	   "localclr-namespace:Analyzer  5   	     � Title$  Networkscanner��5	   	   $	��450��6    $	��650��6-   .��5
      � .+�5   
   �� $��
IP-Range: ��6   $��10,13,565,379q�6%   �� 5   
   -   5         ��Name$ IPstart��5      $
��Left=�6#   $��23��6>   $��	82,13,0,0q�6J    �� TextWrapping$
 Wrap=�6]   $��192.168.0.1��6q   $	��Top=�6�   $	��100��6�   �� 5   
   -   5        $ IPend��5      $
��Left=�6!   $��23��6<   $��
197,13,0,0q�6H   $
 Wrap=�6\   $��192.168.0.250��6p   $	��Top=�6�   $	��100��6�   �� 5   
   -   5         ��Name$ btnScan��5      $	��140��6"   $��Start Networkscan��6.   $	��Top=�6J   $��325,13,177,0q�6x   �� 5   
   -   5        $ 	btnCancel��5      $	��140��6$   {�.  $��Cancel��6B   $
��Left=�6S   $	��Top=�6o   $��
482,13,0,0q�6�   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ 	dgDevices��5         AutoGenerateColumns .  $
��Left=�6�   $��
10,45,0,10q�6�   $	��622��6�   ���	#FF727272   
IsReadOnly . ��.   K� ZIndex$ 2��6  ��6&   ��  �� Path$ MyUtils.NetworkDevices�	 �� Mode$	 OneWay=�
   Columns
 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$
 Ping��6E   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *DisplayPing+6E     5      $ 
IP-Address��6<    6%   �� *IP+6<     5      $ Hostname��6B    6%   �� *
Hostname+6B   4  .System.Windows.Controls.DataGridTemplateColumn  5      $ Action��6)      CellTemplate 5      �� 5      .R�5      �  � ColumnDefinitions 5   "   �� 5   &   $��1* �67   6D   �� 5   &   $��1* �67   6D   .+�5   "   �� $��
btnOpenWeb��6Z   -   5        $��0��5   )   ��69   �� *IP+{�6l   �� *isLedDevice+.��6�   Open Website6�   �� 5   "   $��btnAddDevice��6Y   -   5        $��1��5   )   ��69   �� *IP+{�6m   �� *isLedDevice+.��6�   
Add Device6�   5      5       5"      5%      5'      5(      