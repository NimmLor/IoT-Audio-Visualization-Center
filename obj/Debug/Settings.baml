   M S B A M L   `   `   `  ����C  ?Analyzer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:AnalyzerAnalyzer  5   	       Analyzer.Settings   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   H DMahApps.Metro, Version=1.6.5.1, Culture=neutral, PublicKeyToken=null6mah,http://metro.mahapps.com/winfx/xaml/controls  5   	   "localclr-namespace:Analyzer  5   	     � Title$  Settings��5
   	   $	��400��6   $	��600��6'    � WindowStartupLocation$ CenterScreen=�63    � Icon$ Resources/AppIcon.ico��6X   .��5      � $��10q�6    � ColumnDefinitions 5   
   �� 5      $��2* �6   �� 5      $��2* �6    � RowDefinitions 5   
   �� 5      $��2* �6   �� 5      $��2* �6   �� 5      $��2* �6   �� 5      $��2* �6   �� 5      $��2* �6   �� 5      $��2* �6   .+�5   
   �� $��Start with Windows��6   $��0��6-   $��0��6:   $��14(�6J   $
��Left=�6X   $��Center=�6s   �� 5   
   $��Launch in system-tray��6   $��1��60   $��0��6=   $��14(�6M   $
��Left=�6[   $��Center=�6v   �� 5   
   $��Main spectrum smoothing��6   $��2��62   $��0��6?   $��14(�6O   $
��Left=�6]   $��Center=�6x   �� 5   
   $��Hide device spectrums��6   $��3��60   $��0��6=   $��14(�6M   $
��Left=�6[   $��Center=�6v   �� 5   
   $�� ��6   $��4��6   $��0��6(   $��14(�68   $
��Left=�6F   $��Center=�6a   �� 5   
   -   5        $��	ckWindows��5   0   $��1��6   $��0��6#   $��Enable��6A   �� 5   
   -   5        $��ckSystemTray��5   0   $��1��6   $��1��6#   $��Enable��6D   � 5    
   $��2��6   $��1��6    5!      �� 5"      $��8* �6#   �� 5#      $��2* �6#   .+�5%      �� -   5        $��sldSmoothing��5%   %   $��0��6   $��Center=�69   $��0,0,0,0q�6V   $��Center=�6g   $	��210��6�   
������� �� IsSnapToTickEnabled .  � Value$ 2Q�6�    � SmallChange$ 1Q�6�    � LargeChange$ 2Q�6�   )  #MahApps.Metro.Controls.SliderHelper	   EnableMouseWheel$	 ControlFocused=�6  
   ThumbFillBrush
 �	#FFCFCFCF   ThumbFillHoverBrush �	#FFCFCFCF   ThumbFillPressedBrush �	#FFCFCFCF   ThumbFillDisabledBrush
 �   �$��16(�6  $��29��6   � Maximum$ 30Q�6I  ��6�   Z� ��6�   Z� 6I  �� 5&      -   5        $��lblSmoothing��5&   $   $��1��6   $��2��68   $��Center=�6D   $��0,0,0,0q�6a   $��Center=�6r   $��14(�6�   5'      �� 5(   
   -   5        $��ckDeviceSpectrums��5(   0   $��1��6   $��3��6#   $��Enable��6I   � 5+   
   $��2��6   $��5��6#   .+�5,      �  5-      �� 5.      $��2* �6'   �� 5/      $��2* �6'   �� 50      $��2* �6'   �� 51      $��2* �6'    54      �� 55      $��2* �6$   �� 56      $��2* �6$   .+�58      �� -   5        $��btnSave��58   6   $��0��6   $��0��6)   $��Save settings��6E   �� 59      -   5        $��
btnRestore��59   6   $��1��6   $��0��6)   $��Restore defauls��6H   �� 5:      -   5        $��	btnExport��5:   6   $��2��6   $��0��6)   $��Export settings��6G   �� 5;      -	   5        $��btnLoad��5;   6   $��3��6   $��0��6)   $��Export settings��6E   �� 5<      -
   5        $��btnReset��5<   6   $��0��6   $��1��6)   $��Delete all devices��6F   5>      5?      5@      5A      