   M S B A M L   `   `   `  ���� C  ?Analyzer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:AnalyzerAnalyzer      Analyzer.EditDevice   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     (localclr-namespace:Analyzer     H DMahApps.Metro, Version=1.6.5.1, Culture=neutral, PublicKeyToken=null6mah,http://metro.mahapps.com/winfx/xaml/controls  -   $��EditDeviceWindow��  � Title$  Edit Device��$	��400��$	��800�� � 
ResizeMode$ NoResize=� � WindowStartupLocation$ CenterScreen=�|�.  .���  � ColumnDefinitions �� $��1* ��� $��1* �.+��� -    ��Name$ 	grpParams��$��0��$��
Parameters��$��18,22,24,22q�.���  �� $��5* ��� $��8* �.+��� -    ��Name$ label��$��0��$��Device Name:��$
��Left=�$��	10,10,0,0q�$	��Top=�$��14(��� -   $ label2��$��0��$	��IP:��$
��Left=�$��	10,44,0,0q�$	��Top=�$��14(��� -   $ label3��$��0��$��Number of Lines:��$
��Left=�$��
10,134,0,0q�$	��Top=�$��14(��� -   $ label4��$��0��$��
Smoothing:��$
��Left=�$��
10,190,0,0q�$	��Top=�$��14(��� -    ��Name$ txtName��$��1��*  $MahApps.Metro.Controls.TextBoxHelper   ClearTextButton . $
��Left=�$��29��$��	15,10,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��180��$��14(�+  %MahApps.Metro.Controls.ControlsHelper	   FocusBorderBrush	 Z� 
   MouseOverBorderBrush
 Z� �� -   $ txtIp��$��1�� .    	Watermark$ 192.168.0.xxx��$
��Left=�$��29��$��	15,44,0,0q�$
 Wrap=�$	��Top=�$	��180��$��14(�	 Z� 
 Z� *  $MahApps.Metro.Controls.NumericUpDown  -	   $��nudLines��$��1��   Minimum$ 1Q�   Maximum$
 1023Q�   Interval$ 1Q�$��17,123,25,129q�  -
   $��nudPort��$��1��$ 1Q�$ 99999Q�$ 1Q�$��17,78,25,174q��� -   $ label2_Copy��$��0��$��Port:��$
��Left=�$��	10,89,0,0q�$	��Top=�$��14(��� -    ��Name$ sldSmoothing�� � Minimum$ 1Q� � Maximum$ 50Q�$��1��$
��Left=�$��40��$��
15,179,0,0q�$	��Top=�$	��150��
������� �� IsSnapToTickEnabled .  � SmallChange$ 1Q� � LargeChange$ 2Q�)  #MahApps.Metro.Controls.SliderHelper   EnableMouseWheel$ ControlFocused=�   ThumbFillBrush �	#FFCFCFCF   ThumbFillHoverBrush �	#FFCFCFCF   ThumbFillPressedBrush �	#FFCFCFCF   ThumbFillDisabledBrush
 �   ���Z� ��Z� �� -   $ lblSmoothing��$��1��$��14(�$��170,190,11,62q�����  �� ElementName$ sldSmoothing�� �� Path$ Value�� $��2��$��	0,250,0,0q� �� $��1* ��� $��1* ��� $��1* �.+��� -    ��Name$ 	btnDelete��$��0��$��Delete Device��$��5,0,5,0q�$	��Top=��� -   $ 
btnRestore��$��1��$��Restore Settings��$��5,0,5,0q�$	��Top=��� -   $ btnSave��$��2��$��Save Device��$��5,0,5,0q�$	��Top=��� -   $ grpTest��$��1��$��Test Device��$��24,22,18,22q�.���  �� $��1* ��� $��1* � � RowDefinitions �� $	��10* ��� $	��40* �.+��� -   $ btnTestConnection��$��0��$��0��$��Test Connection��$
��Left=�$��	10,10,0,0q�$	��Top=�$	��152��$��29���� -   $ 
btnRequest��$��0��$��1��$��Request Parameters��$
��Left=�$��	10,10,0,0q�$	��Top=�$	��152��$��29���� -   $ 	txtOutput��$&  Output of parameter response ...��$��1��$��10,10,10,10q�$
 Wrap=�$�� ��$��2��