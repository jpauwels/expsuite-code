Option Strict On
Option Explicit On
#Region "ExpSuite License"
'ExpSuite - software framework for applications to perform experiments (related but not limited to psychoacoustics).
'Copyright (C) 2003-2021 Acoustics Research Institute - Austrian Academy of Sciences; Piotr Majdak and Michael Mihocic
'Licensed under the EUPL, Version 1.2 or � as soon they will be approved by the European Commission - subsequent versions of the EUPL (the "Licence")
'You may not use this work except in compliance with the Licence. 
'You may obtain a copy of the Licence at: http://joinup.ec.europa.eu/software/page/eupl
'Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
'See the Licence for the specific language governing  permissions and limitations under the Licence. 
#End Region
Imports System.Text
Imports System.IO
'Imports System
Imports System.Reflection
'Imports System.Windows.Forms
Imports VB = Microsoft.VisualBasic
Imports System.Threading
Imports System.Globalization
' main form, appears when running the software, contains item list, several buttons, menus (menu bar, results menu)
''' <summary>
''' FrameWork Module. Main Window.
''' </summary>
''' <remarks></remarks>
Friend Class frmMain
    Inherits System.Windows.Forms.Form

    ''
    ' FrameWork Module. Main Window.

    Private ReadOnly IsInitializing As Boolean
    Private ReadOnly mlSelRow As Integer
    Private ReadOnly mlSelCol As Integer
    Private ReadOnly mlStimPartCnt As Integer ' counter of the stimulus part, used in tmrExp
    Private mblnUndo As Boolean
    Private mlFirstItemOfExp As Integer
    Private mlLastItemOfExp As Integer
    Private mlItemCountOfExp As Integer
    Private mlFirstItemAfterBreak As Integer

    'Const MF_BYPOSITION = &H400&
    Private Declare Function GetMenu Lib "user32" (ByVal hWnd As Integer) As Integer
    Private Declare Function GetSubMenu Lib "user32" (ByVal hMenu As Integer, ByVal nPos As Integer) As Integer
    Private Declare Function SetMenuItemBitmaps Lib "user32" (ByVal hMenu As Integer, ByVal nPosition As Integer, ByVal wFlags As Integer, ByVal hBitmapUnchecked As Integer, ByVal hBitmapChecked As Integer) As Integer
    Private mszQuickSaveFN As String
    Private mSortIndex As Integer = 1
    Private szBackupItemListFileName As String

    Dim dgvUndo As DataGridView

    Public ReadOnly Property MlSelRow1 As Integer
        Get
            Return mlSelRow
        End Get
    End Property

    '----------------------------------------------------------------------------------
    '----------------------------------------------------------------------------------
    '----------------------------------------------------------------------------------
    '   Begin of FRAMEWORK section
    '----------------------------------------------------------------------------------
    '----------------------------------------------------------------------------------
    '----------------------------------------------------------------------------------

    Public Delegate Sub ServeDataDelegate(ByVal szEvent As FWintern.ServeDataEnum, ByVal index As Integer, ByVal col As Integer, ByVal value As String, ByVal cnum As Integer)

    ''' <summary>
    ''' Selects sending mode, gets the data and calls a sendData function.
    ''' </summary>
    ''' <param name="szEvent">Used to select sending mode.</param>
    ''' <param name="index">Row index</param>
    ''' <param name="col">Column index</param>
    ''' <param name="value">Value</param>
    ''' <param name="cnum">Client Number</param>
    ''' <remarks></remarks>
    Public Sub ServeData(ByVal szEvent As FWintern.ServeDataEnum, Optional ByVal index As Integer = 0, Optional ByVal col As Integer = 0, Optional ByVal value As String = "", Optional ByVal cnum As Integer = 0)
        If Not gblnRemoteServerConnected Then Exit Sub
        If gblnRemoteClientConnected Then Exit Sub

        Proc.NetworkStatus(Me, Me._sbStatusBar_Panel5, 1)

        RemoteMonitorServerSend.SetBufferSize()
        Select Case szEvent
            Case ServeDataEnum.SendSettings1 '--------------------------------------------------------  Send Settings to one Client
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 1, 0, "", cnum)
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SettingsFilename, 0, 0, gszSettingFileName, cnum)
                Dim szSettings() As String = SettingsPackage()
                Dim list(bufferLarge - 8) As Byte
                Dim counter As Integer = 0
                For i As Integer = 0 To szSettings.Length - 1
                    Dim x() As Byte
                    If szSettings(i) Is Nothing Then
                        x = Encoding.ASCII.GetBytes("")
                    Else
                        x = Encoding.ASCII.GetBytes(szSettings(i))
                    End If
                    For h As Integer = 0 To x.Length - 1
                        list(counter) = x(h)
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                            counter = 0
                        End If
                    Next
                    list(counter) = 0
                    counter += 1
                    If counter = 2040 Then
                        RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                        counter = 0
                    End If
                Next
                If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 1, 0, "", cnum)
                Dim szKey As String = "experiment type"
                INISettings.FindParameter(gszSettingFileName, szKey)
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeSettings, szSettings.Length, 0, szKey, cnum)
                gblnLoadSettings = False
            Case ServeDataEnum.SendSettings '---------------------------------------------------------      Send Settings
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 1, 0, "")
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SettingsFilename, 0, 0, gszSettingFileName)
                Dim szSettings() As String = SettingsPackage()
                Dim list(bufferLarge - 8) As Byte
                Dim counter As Integer = 0
                For i As Integer = 0 To szSettings.Length - 1
                    Dim x() As Byte
                    If szSettings(i) Is Nothing Then
                        x = Encoding.ASCII.GetBytes("")
                    Else
                        x = Encoding.ASCII.GetBytes(szSettings(i))
                    End If
                    For h As Integer = 0 To x.Length - 1
                        list(counter) = x(h)
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                            counter = 0
                        End If
                    Next
                    list(counter) = 0
                    counter += 1
                    If counter = 2040 Then
                        RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                        counter = 0
                    End If
                Next
                If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 1, 0, "")
                Dim szKey As String = "experiment type"
                INISettings.FindParameter(gszSettingFileName, szKey)
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeSettings, szSettings.Length, 0, szKey)
                RemoteMonitorServerSend.SetBufferSize()
                gblnLoadSettings = False
            Case ServeDataEnum.ChangeSettings1 '------------------------------------------------------      Change Settings to one
                If gblnClientsSetting(cnum) Then RemoteMonitorServerSend.SendData(FWintern.ModeEnum.LoadSettings, 0, 0, "", cnum)
            Case ServeDataEnum.ChangeSettings '-------------------------------------------------------      Change Settings
                For i As Integer = 1 To glClientCount
                    If gblnClientsSetting(i) Then RemoteMonitorServerSend.SendData(FWintern.ModeEnum.LoadSettings, 0, 0, "", i)
                Next
            Case ServeDataEnum.ItemlistColCountListStatus1 '--------------------------------------------------------------      Send ItemCount and Itemlist to one
                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "", cnum)
                'Dim list(bufferLarge - 8) As Byte
                'Dim counter As Integer = 0
                'For j As Integer = 0 To ItemList.ColCount - 1
                '    Dim x() As Byte
                '    x = Encoding.ASCII.GetBytes(ItemList.ColCaption(j) & "," & ItemList.ColFlag(j) & "," & ItemList.ColUnit(j) & "," & ItemList.ColMin(j) & "," & ItemList.ColMax(j))
                '    For h As Integer = 0 To x.Length - 1
                '        list(counter) = x(h)
                '        counter += 1
                '        If counter = 2040 Then
                '            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                '            counter = 0
                '        End If
                '    Next
                '    list(counter) = 0
                '    counter += 1
                '    If counter = 2040 Then
                '        RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                '        counter = 0
                '    End If
                'Next
                'If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                'RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "", cnum)
                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeColumnHeaders, 0, ItemList.ColCount, "", cnum)

                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.CreateRows, ItemList.ItemCount, 0, "", cnum)
                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "", cnum)
                'ReDim list(bufferLarge - 8)
                'counter = 0
                'For i As Integer = 0 To ItemList.ItemCount - 1
                '    For j As Integer = 0 To ItemList.ColCount - 1
                '        Dim x() As Byte
                '        If ItemList.Item(i, j) Is Nothing Then
                '            x = Encoding.ASCII.GetBytes("")
                '        Else
                '            x = Encoding.ASCII.GetBytes(ItemList.Item(i, j))
                '        End If
                '        For h As Integer = 0 To x.Length - 1
                '            list(counter) = x(h)
                '            counter += 1
                '            If counter = 2040 Then
                '                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                '                counter = 0
                '            End If
                '        Next
                '        list(counter) = 0
                '        counter += 1
                '        If counter = 2040 Then
                '            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                '            counter = 0
                '        End If
                '    Next
                'Next
                'If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                'RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "", cnum)
                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeItemList, 0, 0, (ItemList.ItemCount * ItemList.ColCount).ToString, cnum)

                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "", cnum)
                Dim list(bufferLarge - 8) As Byte
                Dim counter As Integer = 0
                For j As Integer = 0 To ItemList.ColCount - 1
                    Me.SetProgressbar(j * 33 / (ItemList.ItemCount - 1))
                    Dim x() As Byte
                    x = Encoding.ASCII.GetBytes(ItemList.ColCaption(j) & "," & ItemList.ColFlag(j) & "," & ItemList.ColUnit(j) & "," & ItemList.ColMin(j) & "," & ItemList.ColMax(j))
                    For h As Integer = 0 To x.Length - 1
                        list(counter) = x(h)
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                            counter = 0
                        End If
                    Next
                    list(counter) = 0
                    counter += 1
                    If counter = 2040 Then
                        RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                        counter = 0
                    End If
                Next
                For i As Integer = 0 To ItemList.ItemCount - 1
                    Me.SetProgressbar(i * 33 / (ItemList.ItemCount - 1) + 33)
                    For j As Integer = 0 To ItemList.ColCount - 1
                        Dim x() As Byte
                        If ItemList.Item(i, j) Is Nothing Then
                            x = Encoding.ASCII.GetBytes("")
                        Else
                            x = Encoding.ASCII.GetBytes(ItemList.Item(i, j))
                        End If
                        For h As Integer = 0 To x.Length - 1
                            list(counter) = x(h)
                            counter += 1
                            If counter = 2040 Then
                                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                                counter = 0
                            End If
                        Next
                        list(counter) = 0
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                            counter = 0
                        End If
                    Next
                Next
                For i As Integer = 0 To ItemList.ItemCount - 1
                    Me.SetProgressbar(i * 34 / (ItemList.ItemCount - 1) + 66)
                    Dim int As Integer = ItemList.ItemStatus(i)
                    Dim x() As Byte = Encoding.ASCII.GetBytes(int.ToString)
                    For h As Integer = 0 To x.Length - 1
                        list(counter) = x(h)
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                            counter = 0
                        End If
                        list(counter) = 0
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                            counter = 0
                        End If
                    Next
                Next
                If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list, cnum)
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "", cnum)
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeListandHeaders, ItemList.ItemCount, ItemList.ColCount, (ItemList.ItemCount * ItemList.ColCount).ToString, cnum)
                Me.SetProgressbar(0)
            Case ServeDataEnum.Itemlist '-------------------------------------------------------------      Send Itemlist
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.CreateRows, ItemList.ItemCount, 0, "")
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                Dim list(bufferLarge - 8) As Byte
                Dim counter As Integer = 0
                For i As Integer = 0 To ItemList.ItemCount - 1
                    Me.SetProgressbar(i * 100 / (ItemList.ItemCount - 1))
                    For j As Integer = 0 To ItemList.ColCount - 1
                        Dim x() As Byte
                        If ItemList.Item(i, j) Is Nothing Then
                            x = Encoding.ASCII.GetBytes("")
                        Else
                            x = Encoding.ASCII.GetBytes(ItemList.Item(i, j))
                        End If
                        For h As Integer = 0 To x.Length - 1
                            list(counter) = x(h)
                            counter += 1
                            If counter = 2040 Then
                                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                                counter = 0
                            End If
                        Next
                        list(counter) = 0
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                            counter = 0
                        End If
                    Next
                Next
                If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeItemList, 0, 0, (ItemList.ItemCount * ItemList.ColCount).ToString)
                Me.SetProgressbar(0)
            Case ServeDataEnum.ItemlistColCountListStatus  '------------------------------------------------      Send ItemCount and Itemlist
                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                'Dim list(bufferLarge - 8) As Byte
                'Dim counter As Integer = 0
                'For j As Integer = 0 To ItemList.ColCount - 1
                '    Dim x() As Byte
                '    x = Encoding.ASCII.GetBytes(ItemList.ColCaption(j) & "," & ItemList.ColFlag(j) & "," & ItemList.ColUnit(j) & "," & ItemList.ColMin(j) & "," & ItemList.ColMax(j))
                '    For h As Integer = 0 To x.Length - 1
                '        list(counter) = x(h)
                '        counter += 1
                '        If counter = 2040 Then
                '            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                '            counter = 0
                '        End If
                '    Next
                '    list(counter) = 0
                '    counter += 1
                '    If counter = 2040 Then
                '        RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                '        counter = 0
                '    End If
                'Next
                'If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                'RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeColumnHeaders, 0, ItemList.ColCount, "")

                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.CreateRows, ItemList.ItemCount, 0, "")
                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                'ReDim list(bufferLarge - 8)
                'counter = 0
                'For i As Integer = 0 To ItemList.ItemCount - 1
                '    For j As Integer = 0 To ItemList.ColCount - 1
                '        Dim x() As Byte
                '        If ItemList.Item(i, j) Is Nothing Then
                '            x = Encoding.ASCII.GetBytes("")
                '        Else
                '            x = Encoding.ASCII.GetBytes(ItemList.Item(i, j))
                '        End If
                '        For h As Integer = 0 To x.Length - 1
                '            list(counter) = x(h)
                '            counter += 1
                '            If counter = 2040 Then
                '                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                '                counter = 0
                '            End If
                '        Next
                '        list(counter) = 0
                '        counter += 1
                '        If counter = 2040 Then
                '            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                '            counter = 0
                '        End If
                '    Next
                'Next
                'If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                'RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeItemList, 0, 0, (ItemList.ItemCount * ItemList.ColCount).ToString)

                'RemoteMonitorServerSend.SendData(FWintern.ModeEnum.CreateRows, ItemList.ItemCount, 0, "")
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                Dim list(bufferLarge - 8) As Byte
                Dim counter As Integer = 0
                For j As Integer = 0 To ItemList.ColCount - 1
                    Me.SetProgressbar(j * 33 / (ItemList.ItemCount - 1))
                    Dim x() As Byte
                    x = Encoding.ASCII.GetBytes(ItemList.ColCaption(j) & "," & ItemList.ColFlag(j) & "," & ItemList.ColUnit(j) & "," & ItemList.ColMin(j) & "," & ItemList.ColMax(j))
                    For h As Integer = 0 To x.Length - 1
                        list(counter) = x(h)
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                            counter = 0
                        End If
                    Next
                    list(counter) = 0
                    counter += 1
                    If counter = 2040 Then
                        RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                        counter = 0
                    End If
                Next
                For i As Integer = 0 To ItemList.ItemCount - 1
                    Me.SetProgressbar(i * 33 / (ItemList.ItemCount - 1) + 33)
                    For j As Integer = 0 To ItemList.ColCount - 1
                        Dim x() As Byte
                        If ItemList.Item(i, j) Is Nothing Then
                            x = Encoding.ASCII.GetBytes("")
                        Else
                            x = Encoding.ASCII.GetBytes(ItemList.Item(i, j))
                        End If
                        For h As Integer = 0 To x.Length - 1
                            list(counter) = x(h)
                            counter += 1
                            If counter = 2040 Then
                                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                                counter = 0
                            End If
                        Next
                        list(counter) = 0
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                            counter = 0
                        End If
                    Next
                Next
                For i As Integer = 0 To ItemList.ItemCount - 1
                    Me.SetProgressbar(i * 34 / (ItemList.ItemCount - 1) + 66)
                    Dim int As Integer = ItemList.ItemStatus(i)
                    Dim x() As Byte = Encoding.ASCII.GetBytes(int.ToString)
                    For h As Integer = 0 To x.Length - 1
                        list(counter) = x(h)
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                            counter = 0
                        End If
                        list(counter) = 0
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                            counter = 0
                        End If
                    Next
                Next
                If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeListandHeaders, ItemList.ItemCount, ItemList.ColCount, (ItemList.ItemCount * ItemList.ColCount).ToString)
                Me.SetProgressbar(0)
                'Case ServeDataEnum.Itemlist '-------------------------------------------------------------      Send Itemlist
                '    RemoteMonitorServerSend.SendData(FWintern.ModeEnum.CreateRows, ItemList.ItemCount, 0, "")
                '    RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                '    Dim list(bufferLarge - 8) As Byte
                '    Dim counter As Integer = 0
                '    For i As Integer = 0 To ItemList.ItemCount - 1
                '        Me.SetProgressbar(i * 100 / (ItemList.ItemCount - 1))
                '        For j As Integer = 0 To ItemList.ColCount - 1
                '            Dim x() As Byte
                '            If ItemList.Item(i, j) Is Nothing Then
                '                x = Encoding.ASCII.GetBytes("")
                '            Else
                '                x = Encoding.ASCII.GetBytes(ItemList.Item(i, j))
                '            End If
                '            For h As Integer = 0 To x.Length - 1
                '                list(counter) = x(h)
                '                counter += 1
                '                If counter = 2040 Then
                '                    RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                '                    counter = 0
                '                End If
                '            Next
                '            list(counter) = 0
                '            counter += 1
                '            If counter = 2040 Then
                '                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                '                counter = 0
                '            End If
                '        Next
                '    Next
                '    If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                '    RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                '    RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeItemList, 0, 0, (ItemList.ItemCount * ItemList.ColCount).ToString)
            Case ServeDataEnum.ListStatus '-----------------------------------------------------------      Send Status of all Items to one
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "", cnum)
                Dim list As String = ""
                Dim counter As Integer = 0
                For i As Integer = 0 To ItemList.ItemCount - 1
                    Dim int As Integer = ItemList.ItemStatus(i)
                    list += int.ToString
                    counter += 1
                    If counter = 2040 Then
                        RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendStrings, 0, 0, Mid(list, 1, 2040), cnum)
                        list = Mid(list, 2041, list.Length - 2040)
                        counter = 0
                    End If
                Next
                If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendStrings, 0, 0, list, cnum)
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "", cnum)
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeListStatus, 0, 0, "", cnum)
            Case ServeDataEnum.ChangeListStatus Or ServeDataEnum.ChangeItemStatus '-----------------------------------------------------      Send Status of all Items
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                Dim list As String = ""
                Dim counter As Integer = 0
                For i As Integer = 0 To ItemList.ItemCount - 1
                    Dim int As Integer = ItemList.ItemStatus(i)
                    list += int.ToString
                    counter += 1
                    If counter = 2040 Then
                        RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendStrings, 0, 0, Mid(list, 1, 2040))
                        list = Mid(list, 2041, list.Length - 2040)
                        counter = 0
                    End If
                Next
                If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendStrings, 0, 0, list)
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeListStatus, 0, 0, "")
                'Case ServeDataEnum.ChangeItemStatus '-----------------------------------------------------      Send Status of one Item
                '    RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeItemStatus, index, ItemList.ItemStatus(index), "")
            Case ServeDataEnum.ChangeItem '-----------------------------------------------------------      Send one Cell
                For Each cell As DataGridViewCell In dgvItemList.SelectedCells
                    RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeCell, cell.RowIndex, cell.ColumnIndex, cell.Value.ToString)
                Next
            Case ServeDataEnum.NextItem '-------------------------------------------------------------      Send one Item
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                Dim list(bufferLarge - 8) As Byte
                Dim counter As Integer = 0
                For j As Integer = 0 To ItemList.ColCount - 1
                    Dim x() As Byte
                    If ItemList.Item(ItemList.ItemIndex, j) Is Nothing Then
                        x = Encoding.ASCII.GetBytes("")
                    Else
                        x = Encoding.ASCII.GetBytes(ItemList.Item(ItemList.ItemIndex, j))
                    End If
                    For i As Integer = 0 To x.Length - 1
                        list(counter) = x(i)
                        counter += 1
                        If counter = 2040 Then
                            RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                            counter = 0
                        End If
                    Next
                    list(counter) = 0
                    counter += 1
                Next
                If counter <= 2040 Then RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.SendBytes, 0, 0, list)
                RemoteMonitorServerSend.SendDataSync(FWintern.ModeEnum.ChangeToBlockingMode, 0, 0, "")
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.DecodeNextItem, ItemList.ItemIndex, ItemList.ItemStatus(index), _sbStatusBar_Panel3.Text & "," & _sbStatusBar_Panel4.Text)
            Case ServeDataEnum.Renumber '------------------------------------------------------------       Renumber
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.Renumber, 0, 0, "")
            Case ServeDataEnum.Clear '---------------------------------------------------------------       Clear
            Case ServeDataEnum.Close '---------------------------------------------------------------       Close
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.Disconnect, 0, 0, "")
            Case ServeDataEnum.StartExp '------------------------------------------------------------       Experiment started
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.StartExperiment, 0, 0, "")
            Case ServeDataEnum.EveryItem '-----------------------------------------------------------       Beep EveryItem
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.BeepEveryItem, 0, 0, "")
            Case ServeDataEnum.ThirdLast '-----------------------------------------------------------       Beep ThirdLast
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.BeepThirdLast, 0, 0, "")
            Case ServeDataEnum.SecondLast '----------------------------------------------------------       Beep SecondLast
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.BeepSecondLast, 0, 0, "")
            Case ServeDataEnum.LastItem '------------------------------------------------------------       Beep LastItem
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.BeepLastItem, 0, 0, "")
            Case ServeDataEnum.EndOfExperiment '-----------------------------------------------------       Beep EndOfExperiment
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.BeepEndOfExperiment, 0, 0, "")
            Case ServeDataEnum.ErrorInExperiment '---------------------------------------------------       Beep ErrorInExperiment
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.BeepError, 0, 0, "")
            Case ServeDataEnum.Break '---------------------------------------------------------------       Beep Break
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.BeepBreak, 0, 0, "")
            Case ServeDataEnum.EndOfBlock '----------------------------------------------------------       Beep EndOfBlock
                RemoteMonitorServerSend.SendData(FWintern.ModeEnum.BeepBlockEnd, 0, 0, "")
        End Select

        Proc.NetworkStatus(gform, gtssl2, 0)
    End Sub

    ''' <summary>
    ''' Returns setting package for remote monitoring.
    ''' </summary>
    ''' <returns>Package as string.</returns>
    ''' <remarks></remarks>
    Public Function SettingsPackage() As String()
        Dim lX, lY As Integer
        Dim index As Integer = 0
        Dim szX As String
        Dim szSettings(200) As String

        ' frame work parameters
        szSettings(index) = "Framework Version=" & TStr(FW_MAJOR) & "." & TStr(FW_MINOR) & "." & TStr(FW_REVISION)
        index += 1
        szSettings(index) = "Computer Name=" & My.Computer.Name.ToString
        index += 1
        szSettings(index) = "Source Dir=" & gszSourceDir
        index += 1
        'szSettings(index) = "Destination Dir=" & gszDestinationDir
        'index += 1
        szSettings(index) = "Create New Work Dir=" & TStr(CInt(gblnNewWorkDir))
        index += 1
        szSettings(index) = "Silent Mode=" & TStr(CInt(gblnSilentMode))
        index += 1
        For lX = 0 To DataDirectory.Count - 1
            Dim csvX As New CSVParser With { _
                .Quota = """", _
                .Separator = "," _
            }
            szX = csvX.QuoteCell(DataDirectory.Title(lX)) & csvX.Separator & csvX.QuoteCell(DataDirectory.Path(lX))
            szSettings(index) = "Data Directory=" & szX
            index += 1
        Next
        szSettings(index) = "Do Not Connect To Output=" & TStr(CInt(gblnDoNotConnectToDevice))
        index += 1
        szSettings(index) = "Fitting File Left=" & gszFittFileLeft
        index += 1
        szSettings(index) = "Fitting File Right=" & gszFittFileRight
        index += 1
        szSettings(index) = "Destination Directory Active=" & TStr(CInt(gblnDestinationDir))
        index += 1
        szSettings(index) = "Experiment Description=" & gszDescription
        index += 1
        szSettings(index) = "Experiment ID=" & gszExpID
        index += 1
        szSettings(index) = "Application Title=" & My.Application.Info.AssemblyName
        index += 1
        szSettings(index) = "Application Version=" & My.Application.Info.Version.Major & "." & _
                                         My.Application.Info.Version.Minor & "." & _
                                         My.Application.Info.Version.Build
        index += 1
        szSettings(index) = "Item List Title=" & gszItemListTitle
        index += 1
        szSettings(index) = "Experiment Window Left=" & TStr(grectExp.Left)
        index += 1
        szSettings(index) = "Experiment Window Width=" & TStr(grectExp.Width)
        index += 1
        szSettings(index) = "Experiment Window Top=" & TStr(grectExp.Top)
        index += 1
        szSettings(index) = "Experiment Window Height=" & TStr(grectExp.Height)
        index += 1
        szSettings(index) = "Experiment Window On Top=" & TStr(CInt(gblnExpOnTop))
        index += 1
        szSettings(index) = "Override Exp. Mode=" & TStr(CInt(gblnOverrideExpMode))
        index += 1
        szSettings(index) = "Exp. Mode=" & TStr(gOExpMode)
        index += 1
        For lX = 0 To 1
            With gAudioSynth(lX)
                szX = TStr(lX) & ";" & TStr(.Signal) & ";" & TStr(.Par1) & ";" & TStr(.HighCut) & ";" & TStr(.LowCut) & ";" & TStr(.Vol)
            End With
            szSettings(index) = "Audio Synthesizer Unit=" & szX
            index += 1
        Next
        szX = TStr(glAudioDACAddStream(0))
        For lX = 1 To PLAYER_MAXCHANNELS - 1 : szX = szX & ";" & TStr(glAudioDACAddStream(lX)) : Next
        szSettings(index) = "Audio DAC AddStream=" & szX
        index += 1
        szSettings(index) = "Break Flags=" & TStr(glBreakFlags)
        index += 1
        szSettings(index) = "Break Interval=" & TStr(glBreakInterval)
        index += 1
        szSettings(index) = "Experiment Start Item=" & TStr(glExperimentStartItem)
        index += 1
        szSettings(index) = "Experiment End Item=" & TStr(glExperimentEndItem)
        index += 1

        ' electrode arrays
        If Not IsNothing(gfreqParL) Then
            For lX = 0 To gfreqParL.Length - 1
                With gfreqParL(lX)
                    szSettings(index) = "Frequency List Left=" & TStr(.sAmp) & ";" & TStr(.lRange) & ";" & TStr(.lPhDur) & ";" & TStr(.sSPLOffset) & ";" & TStr(.sCenterFreq) & ";" & TStr(.sBandwidth) & ";" & TStr(.sTHR) & ";" & TStr(.sMCL)
                    index += 1
                End With
            Next
        End If
        If Not IsNothing(gfreqParR) Then
            For lX = 0 To gfreqParR.Length - 1
                With gfreqParR(lX)
                    szSettings(index) = "Frequency List Right=" & TStr(.sAmp) & ";" & TStr(.lRange) & ";" & TStr(.lPhDur) & ";" & TStr(.sSPLOffset) & ";" & TStr(.sCenterFreq) & ";" & TStr(.sBandwidth) & ";" & TStr(.sTHR) & ";" & TStr(.sMCL)
                    index += 1
                End With
            Next
        End If
        ' variables
        If GetUboundVariables() <> -1 Then
            For lX = 0 To UBound(gvarExp)
                With gvarExp(lX)
                    If GetUbound(.varValue) > -1 Then
                        For lY = 0 To GetUbound(.varValue)
                            szSettings(index) = .szName & " List=" & .varValue(lY)
                            index += 1
                        Next
                    End If
                End With
            Next
        End If

        ' constants
        If GetUboundConstants() <> -1 Then
            For lX = 0 To UBound(gconstExp)
                With gconstExp(lX)
                    szSettings(index) = .szName & "=" & .varValue
                    index += 1
                End With
            Next
        End If

        ' procedure constants
        szSettings(index) = "Interstimulus Break=" & TStr(glInterStimBreak)
        index += 1
        szSettings(index) = "Experiment Type=" & TStr(glExpType)
        index += 1
        szSettings(index) = "Experiment HUI ID=" & TStr(glExpHUIID)
        index += 1
        szSettings(index) = "Experiment Screen Flags=" & TStr(glExpFlags)
        index += 1
        szSettings(index) = "Offset Left=" & TStr(glOffsetL)
        index += 1
        szSettings(index) = "Offset Right=" & TStr(glOffsetR)
        index += 1
        szSettings(index) = "Item Repetition=" & TStr(glRepetition)
        index += 1
        szSettings(index) = "Electrode Left=" & TStr(glElectrodeL)
        index += 1
        szSettings(index) = "Electrode Right=" & TStr(glElectrodeR)
        index += 1
        szSettings(index) = "Stimuli Creation=" & TStr(gStimOutput)        ' backwards comp. to vb6-apps
        index += 1
        szSettings(index) = "Stimuli Output=" & TStr(gStimOutput)
        index += 1
        szSettings(index) = "Sampling Rate=" & TStr(glSamplingRate)
        index += 1
        szSettings(index) = "Resolution=" & TStr(glResolution)
        index += 1
        szSettings(index) = "Fade In=" & TStr(gsFadeIn)
        index += 1
        szSettings(index) = "Fade Out=" & TStr(gsFadeOut)
        index += 1
        szSettings(index) = "Use Data Channel=" & TStr(CInt(gblnUseDataChannel))
        index += 1
        szSettings(index) = "Use Trigger Channel=" & TStr(CInt(gblnUseTriggerChannel))
        index += 1
        szSettings(index) = "Division Factor" & TStr(divFactor)
        index += 1
        szSettings(index) = "Prestimulus Break=" & TStr(glPreStimBreak)
        index += 1
        szSettings(index) = "Prestimulus Visual Offset=" & TStr(glPreStimVisu)
        index += 1
        szSettings(index) = "Poststimulus Visual Offset=" & TStr(glPostStimVisu)
        index += 1

        ' tracker
        szSettings(index) = "Tracker Use=" & TStr(CInt(gblnTrackerUse))
        index += 1
        szSettings(index) = "Tracker Repetition Rate=" & TStr(glTrackerRepRate)
        index += 1
        szSettings(index) = "Tracker Position Scaling=" & TStr(gsngTrackerPosScaling)
        index += 1
        szSettings(index) = "Tracker Range Enabled Sensor 0 Min=" & TStr(gtsTrackerMin(0).lStatus)
        index += 1
        szSettings(index) = "Tracker Range Enabled Sensor 1 Min=" & TStr(gtsTrackerMin(1).lStatus)
        index += 1
        szSettings(index) = "Tracker Range Enabled Sensor 0 Max=" & TStr(gtsTrackerMax(0).lStatus)
        index += 1
        szSettings(index) = "Tracker Range Enabled Sensor 1 Max=" & TStr(gtsTrackerMax(1).lStatus)
        index += 1
        For lX = 0 To 1
            szX = TStr(gtsTrackerMin(lX).sngX) & ";" & TStr(gtsTrackerMax(lX).sngX) & ";" & TStr(gtsTrackerMin(lX).sngY) & ";" & TStr(gtsTrackerMax(lX).sngY) & ";" & TStr(gtsTrackerMin(lX).sngZ) & ";" & TStr(gtsTrackerMax(lX).sngZ) & ";" & TStr(gtsTrackerMin(lX).sngA) & ";" & TStr(gtsTrackerMax(lX).sngA) & ";" & TStr(gtsTrackerMin(lX).sngE) & ";" & TStr(gtsTrackerMax(lX).sngE) & ";" & TStr(gtsTrackerMin(lX).sngR) & ";" & TStr(gtsTrackerMax(lX).sngR) & ";"
            szSettings(index) = "Tracker Range Sensor " & TStr(lX) & "=" & szX
            index += 1
        Next
        For lX = 0 To 1
            szX = TStr(gtsTrackerValues(lX).sngX) & ";" & TStr(gtsTrackerValues(lX).sngY) & ";" & TStr(gtsTrackerValues(lX).sngZ) & ";" & TStr(gtsTrackerValues(lX).sngA) & ";" & TStr(gtsTrackerValues(lX).sngE) & ";" & TStr(gtsTrackerValues(lX).sngR)
            szSettings(index) = "Tracker Sensor " & TStr(lX) & " Default Values=" & szX
            index += 1
        Next
        For lX = 0 To 1
            szX = TStr(gtsTrackerOffset(lX).sngX) & ";" & TStr(gtsTrackerOffset(lX).sngY) & ";" & TStr(gtsTrackerOffset(lX).sngZ) & ";" & TStr(gtsTrackerOffset(lX).sngA) & ";" & TStr(gtsTrackerOffset(lX).sngE) & ";" & TStr(gtsTrackerOffset(lX).sngR)
            szSettings(index) = "Tracker Sensor " & TStr(lX) & " Offset=" & szX
            index += 1
        Next

        ' turntable
        szSettings(index) = "Turntable Use=" & TStr(CInt(gblnTTUse))
        index += 1

        ' viwo
        szSettings(index) = "ViWo Average Window Head=" & TStr(gsngViWoAvgHead)
        index += 1
        szSettings(index) = "ViWo Average Window Pointer=" & TStr(gsngViWoAvgPointer)
        index += 1
        szSettings(index) = "ViWo Selected World=" & gszViWoWorld
        index += 1
        For lX = 0 To ViWo.GetParametersCount - 1
            Dim csvX As New CSVParser With { _
            .Quota = """",  _
            .Separator = "," _
            }
            szSettings(index) = "ViWo World Parameter=" & _
                    csvX.QuoteCell(gviwoparParameters(lX).Command) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Type) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).MIDI) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Default) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Par1) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Par2) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Name) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Value)
            index += 1
        Next

        ' Level Dancer
        szSettings(index) = "Level Dancer Stimulate Delayed=" & TStr(CInt(gblnLDDelayed))
        index += 1
        szSettings(index) = "Level Dancer Stimulate Left First=" & TStr(CInt(gblnLDLeftFirst))
        index += 1
        szSettings(index) = "Level Dancer Stimulus Length=" & TStr(glLDStimLength)
        index += 1
        szSettings(index) = "Level Dancer Pulse Period Left=" & TStr(gsLDPulsePeriodL)
        index += 1
        szSettings(index) = "Level Dancer Pulse Period Right=" & TStr(gsLDPulsePeriodR)
        index += 1
        szSettings(index) = "AppendPulseTrain=" & TStr(glAppendPulseTrainIndex)
        index += 1
        szSettings(index) = "AppendPulseTrain Parameters=" & frmLevelDancer.txtAppendPulseTrain.Text
        index += 1

        Dim szSettings1(index) As String

        For i As Integer = 0 To index
            szSettings1(i) = szSettings(i)
        Next
        ' Fitt4Fun parameters
        'WriteLine("Fitt4Fun Stimulus Length", TStr(glF4FStimLength))   ' LD used
        'WriteLine("Fitt4Fun Pulse Period", TStr(gsF4FPulsePeriod))     ' LD used

        Return szSettings1
    End Function

    Private Sub UndoDisable()
        mblnUndo = False
    End Sub

    Public Sub UndoSnapshot()
        If dgvUndo.InvokeRequired Then Err.Raise(vbObjectError, "undosnapshot", "invokerequired")
        If dgvItemList.Enabled Then
            CopyDataGridView(dgvItemList, dgvUndo)
            mblnUndo = True
        Else
            mblnUndo = False
        End If
    End Sub

    ''' <summary>
    ''' Set the Result List.
    ''' </summary>
    ''' <param name="szText">String with list items, separated by ;</param>
    ''' <remarks></remarks>
    Public Sub SetResultList(ByVal szText As String)
        If cmbResult.InvokeRequired Then Err.Raise(vbObjectError, "setresultlist", "invoke required")
        If cmdResultExecute.InvokeRequired Then Err.Raise(vbObjectError, "setresultlist", "invoke required")
        Dim lX, lIdx As Integer
        Dim szArr() As String

        If Len(szText) = 0 Then
            cmbResult.Visible = False
            cmdResultExecute.Visible = False
        Else
            lIdx = cmbResult.SelectedIndex
            cmbResult.Items.Clear()
            szArr = Split(szText, ";")
            For lX = 0 To GetUbound(szArr)
                cmbResult.Items.Add(szArr(lX))
            Next
            If cmbResult.Items.Count > lIdx Then
                cmbResult.SelectedIndex = lIdx
            Else
                cmbResult.SelectedIndex = 0
            End If
        End If

    End Sub

    '    Private Sub ShuffleItemBlock( lRowBeg As Integer,  lRowEnd As Integer)

    '        Dim sRnd() As Single
    '        Dim sMin As Single
    '        Dim lY, lX, lMin As Integer
    '        Dim lItemNr As Integer

    '        With dgvItemList
    '            If lRowBeg > lRowEnd Then
    '                lX = lRowBeg
    '                lRowEnd = lRowBeg
    '                lRowBeg = lX
    '            End If
    '            If lRowBeg < 1 Then GoTo SubEnd
    '            If lRowBeg >= .RowCount Then GoTo SubEnd
    '            If lRowEnd >= .RowCount Then GoTo SubEnd

    '            Dim lColBeg As Integer = 0
    '            Dim lColEnd As Integer = .ColumnCount - 1

    '            lItemNr = lRowEnd - lRowBeg + 1
    '            ' generate an array with random numbers
    '            ReDim sRnd(lItemNr - 1)
    '            For lX = 0 To lItemNr - 1
    '                sRnd(lX) = Rnd()
    '            Next
    '            pbStatus.Value = 0
    '            ' sort the item list (quick sort)
    '            For lX = 0 To lItemNr - 2 ' discard last item
    '                sMin = 1
    '                lMin = lX
    '                For lY = lX To lItemNr - 1 ' find index of minimum value
    '                    If sRnd(lY) < sMin Then sMin = sRnd(lY) : lMin = lY
    '                Next
    '                For lY = lColBeg To lColEnd ' swap 2 item rows
    '                    Dim objX As Object = .Item(lMin + lRowBeg, lY).Value
    '                    .Item(lMin + lRowBeg, lY).Value = .Item(lX + lRowBeg, lY).Value
    '                    .Item(lX + lRowBeg, lY).Value = objX
    '                Next
    '                sRnd(lMin) = sRnd(lX)
    '                sRnd(lX) = sMin
    '                pbStatus.Value = CInt(Math.Round((lX + 1) / (lItemNr - 1) * 100))
    '                Windows.Forms.Application.DoEvents()
    '                If gblnCancel Then Exit For
    '            Next
    '            gblnItemListShuffled = True
    '            FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
    'SubEnd:

    '        End With
    '        ServeData("Shuffle Block")

    '    End Sub

    ''' <summary>
    ''' NextItem: use Itemlist.NextItem instead.
    ''' </summary>
    ''' <param name="lNrInterl"></param>
    ''' <param name="sProgress"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function NextItem(ByVal lNrInterl As Integer, ByVal sProgress As Double) As Boolean

        ' log last item
        Dim szArr(ItemList.ColCount - 1) As String
        For lX As Integer = 0 To UBound(szArr)
            szArr(lX) = ItemList.Item(ItemList.ItemIndex, lX)
        Next
        STIM.Log(szArr)
        If gblnAutoBackupItemList Then STIM.BackupItemList(szBackupItemListFileName)

        ' if no interleaving then finish current item (if not finished by application)
        If lNrInterl < 1 Then
            If ItemList.ItemStatus(ItemList.ItemIndex) = clsItemList.Status.Processing Then _
                ItemList.ItemStatus(ItemList.ItemIndex) = clsItemList.Status.FinishedOK
        End If
        Dim lState As clsItemList.Status = ItemList.ItemStatus(ItemList.ItemIndex)

        ServeData(FWintern.ServeDataEnum.NextItem)

        ' number of items increased during running experiment
        If glExperimentEndItem = -1 Then mlLastItemOfExp = ItemList.ItemCount - 1

        ' error workaround if a sympathic user removed items from item list while experiment is running
        If mlLastItemOfExp > ItemList.ItemCount - 1 Then
            mlLastItemOfExp = ItemList.ItemCount - 1
            mlItemCountOfExp = ItemList.ItemCount
        End If

        ' count unprocessed items
        Dim lItemsLeft As Integer = 0
        For lX As Integer = mlFirstItemOfExp To mlLastItemOfExp ' find out how many unfinished items left
            If ItemList.ItemStatus(lX) = clsItemList.Status.Fresh Or _
               ItemList.ItemStatus(lX) = clsItemList.Status.Processing Then
                lItemsLeft += 1
            End If
        Next

        If lItemsLeft > 0 Then
            Dim lNextItem As Integer
            If lNrInterl > 0 Then
                ' interleaved procedure - get the next random item
                If lItemsLeft < lNrInterl Then lNrInterl = lItemsLeft
                Randomize()
                Dim lRandValue As Integer = CInt(Math.Round((lNrInterl - 1) * Rnd()))

                Dim szX As String = TStr(lRandValue) & ": "
                For lNextItem = mlFirstItemOfExp To mlLastItemOfExp 'loop: item where exp. is started to last item
                    If ItemList.ItemStatus(lNextItem) = clsItemList.Status.Fresh Or _
                       ItemList.ItemStatus(lNextItem) = clsItemList.Status.Processing Then
                        szX = szX & TStr(lNextItem) & " "
                        If lRandValue = 0 Then Exit For
                        lRandValue -= 1
                    End If
                Next
                Console.WriteLine(szX & ": exit with " & TStr(lNextItem))
            Else
                ' no interleaving 
                For lNextItem = ItemList.ItemIndex + 1 To mlLastItemOfExp
                    If ItemList.ItemStatus(lNextItem) = clsItemList.Status.Fresh Or _
                       ItemList.ItemStatus(lNextItem) = clsItemList.Status.Processing Then Exit For
                Next
            End If
            'Windows.Forms.Application.DoEvents()
            ItemList.ItemIndex = lNextItem
            'ItemList.SetOptimalColWidth()
            'Windows.Forms.Application.DoEvents()
        End If

        ServeData(FWintern.ServeDataEnum.NextItem)

        ' beep only if the previous item was finished
        If lState <> clsItemList.Status.Fresh And lState <> clsItemList.Status.Processing Then
            'If Not gblnRemoteRenumber Then ServeData("Next Item")
            Select Case lItemsLeft
                Case 1 ' last item
                    'If glFlagBeepExp > 0 Then BeepOnLast()
                    If gblnPlayWaveExp = True Then PlayWaveOnLastItem()
                    ServeData(FWintern.ServeDataEnum.LastItem)
                Case 2 ' second last
                    'If glFlagBeepExp > 0 Then BeepOnSecond()
                    If gblnPlayWaveExp = True Then PlayWaveOnSecondLastItem()
                    ServeData(FWintern.ServeDataEnum.SecondLast)
                Case 3 ' third last
                    'If glFlagBeepExp > 0 Then BeepOnThird()
                    'If gblnPlayWaveExp = True Then PlayWaveOnThirdLastItem()
                    ServeData(FWintern.ServeDataEnum.ThirdLast)
                Case Else   ' all other items
                    'If (glFlagBeepExp And 2) <> 0 Then BeepOnEveryItem()
                    ServeData(FWintern.ServeDataEnum.EveryItem)
            End Select
        End If

        lblSelItemNr.Text = "#" & TStr(ItemList.ItemIndex + 1)
        lblSelItemLabel.Text = "Current Item:"
        ' set progress bar
        If (glBreakFlags And 1) <> 0 And (glExpFlags And frmExp.EXPFLAGS.expflProgressSyncToBreak) <> 0 Then ' break flag true
            ' sync to break
            If (glBreakFlags And 2) = 2 Or (glBreakFlags And 4) = 4 Then
                ' sync only item counter to break interval
                Dim MaxItemNum As Integer = Math.Min(PercentToItems(glBreakInterval), ItemList.ItemCount) 'break interval should not be higher than total number of items :-)
                If mlFirstItemAfterBreak + MaxItemNum > ItemList.ItemCount Then
                    MaxItemNum = ItemList.ItemCount - mlFirstItemAfterBreak
                Else
                    mlFirstItemAfterBreak = 0
                End If
                Dim sX As Double = CDbl((ItemList.ItemIndex - mlFirstItemAfterBreak) Mod MaxItemNum) / CSng(MaxItemNum) * 100

                frmExp.SetProgress(sX)
                Me.SetProgressbar(CInt(sX))
            End If
        Else
            ' sync to item number
            If Not IsNothing(sProgress) AndAlso sProgress < 0 Then
                sProgress = (mlItemCountOfExp - lItemsLeft) / (mlItemCountOfExp) * 100
            Else
                If sProgress > 100 Then sProgress = 100
            End If
            frmExp.SetProgress(sProgress)
            Me.SetProgressbar(sProgress)
        End If

        ' initiate Break?
        If lState <> clsItemList.Status.Fresh And lState <> clsItemList.Status.Processing Then
            'If lItemsLeft > 0 And (ItemList.ItemIndex <> 0) AndAlso (ItemList.ItemIndex Mod PercentToItems(glBreakInterval)) = 0 Then
            If lItemsLeft > 0 And (ItemList.ItemIndex <> 0) AndAlso ((mlItemCountOfExp - lItemsLeft) Mod PercentToItems(glBreakInterval)) = 0 Then
                If (glBreakFlags And 3) = 3 Or (glBreakFlags And 5) = 5 Then 'break beep
                    ' break on item, percent
                    chkExpRun.CheckState = System.Windows.Forms.CheckState.Unchecked
                    'If glFlagBeepExp > 0 Then BeepOnBreak()
                    If gblnPlayWaveExp = True Then PlayWaveOnBreak()
                    ServeData(FWintern.ServeDataEnum.Break)
                End If
            End If
            ' wait for end of break?
            If chkExpRun.CheckState <> CheckState.Checked Then
                ' callback?
                If Not IsNothing(gOnBreakAddr) Then gOnBreakAddr()
                FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piResponse 'itemlist: response postfix when saving during break
                ' wait for the end of break
                Do
                    Sleep(1)
                    Windows.Forms.Application.DoEvents()
                Loop Until chkExpRun.CheckState = CheckState.Checked Or gblnCancel
                'ServeExpState 6 ' resume after break
                ' wait for go!
                mlFirstItemAfterBreak = ItemList.ItemIndex
                If (glExpFlags And frmExp.EXPFLAGS.expflWaitAfterBreak) <> 0 And ((glExpFlags And frmExp.EXPFLAGS.expflWaitForNext) = 0 Or glExpMode(glExpType) = 7) Then 'mode7 is special
                    ' Wait after break
                    Me.SetStatus("Waiting for subject...")
                    gblnWaitAfterBreak = True
                    frmExp.EnableResponse(False)
                    Dim lX As Integer = -1
                    Do
                        Sleep(1)
                        System.Windows.Forms.Application.DoEvents()
                        frmExp.GetResponse(lX)
                    Loop Until lX = rSTART Or lX = rNEXT Or lX = rCANCEL Or gblnCancel
                    frmExp.DisableResponse()
                    gblnWaitAfterBreak = False
                    ' End wait after break
                End If
            End If
        End If
        Return (lItemsLeft = 0)
    End Function

    Private Function PercentToItems(percent As Integer) As Integer

        If (glBreakFlags And 4) = 4 Then
            Dim lTemp As Integer = CInt(Math.Ceiling(ItemList.ItemCount * percent / 100))
            Console.WriteLine("Break after " & TStr(lTemp) & " items")
            Return lTemp
        Else
            Return percent
        End If

    End Function

    Private Function SnapShot(ByVal szTitle As String) As String
        Dim szArr() As String
        Dim lX, lY As Integer

        Debug.Print(System.DateTime.Now.ToString("HH:mm:ss"))
        STIM.Log("**********", "Snapshot", szTitle, System.DateTime.Now.ToString("HH:mm:ss"))
        ' snapshot procedure constants
        STIM.Log("Experiment Type", TStr(glExpType), gszExpTypeNames(glExpType))
        STIM.Log("Prestimulus Break", TStr(glPreStimBreak))
        STIM.Log("Prestimulus Visual Offset", TStr(glPreStimVisu))
        STIM.Log("Interstimulus Break", TStr(glInterStimBreak))
        STIM.Log("Poststimulus Visual Offset", TStr(glPostStimVisu))
        STIM.Log("Offset Left", TStr(glOffsetL))
        STIM.Log("Offset Right", TStr(glOffsetR))
        STIM.Log("Item Repetition", TStr(glRepetition))
        STIM.Log("Number of items", TStr((ItemList.ItemCount)))
        ' snapshot synthesizer (dithering) parameters
        lY = 0
        For lX = 0 To glPlayerChannels - 1
            lY += glAudioDACAddStream(lX) ' any unit used?
        Next
        If lY > 0 Then ' if used then log
            STIM.Log("Synthesizer #", "Signal", "Volume", "Low Cut", "High Cut", "Par 1")
            For lX = 0 To 1
                With gAudioSynth(lX)
                    STIM.Log("Synthesizer " & TStr(lX + 1), TStr(.Signal), TStr(.Vol), TStr(.LowCut), TStr(.HighCut), TStr(.Par1))
                End With
            Next
            STIM.Log("Channel", "Synthesizer Unit")
            For lX = 0 To glPlayerChannels - 1
                If glAudioDACAddStream(lX) > 0 Then STIM.Log(TStr(lX), TStr(glAudioDACAddStream(lX)))
            Next
        End If
        ' snapshot constants
        If Not IsNothing(gconstExp) Then
            For lX = 0 To UBound(gconstExp)
                STIM.Log(gconstExp(lX).szName, gconstExp(lX).varValue)
            Next
        End If
        ' snapshot variables
        If Not IsNothing(gvarExp) Then
            For lX = 0 To UBound(gvarExp)
                With gvarExp(lX)
                    If GetUbound(.varValue) > -1 Then
                        ReDim szArr(UBound(.varValue) + 1)
                        For lY = 0 To UBound(.varValue)
                            szArr(lY + 1) = .varValue(lY)
                        Next
                        szArr(0) = .szName
                        STIM.Log(szArr)
                    Else
                        STIM.Log(.szName, "Empty list")
                    End If
                End With
            Next
        End If

        ExpSuite.Events.OnSnapshot()
        STIM.Log("")
        STIM.Log("")
        SnapShot = ""
    End Function

    Private Sub Connect()
        Dim lX As Integer
        Dim stPar As New STIMULUSPARAMETER
        Dim szX As String
        Dim szErr As String
        'Dim szPre, szPost As String

        Dim start_time As DateTime = Now 'timer
        ' disable controls
        SetUIBusy()
        ' check parameters
        SetStatus("Check settings")
        If gStimOutput = GENMODE.genElectricalNIC And gblnUseMATLAB = False Then
            szErr = "NIC device requires MATLAB. Check 'Use MATLAB' in the options."
            GoTo SubError
        End If
        gszYamiVersion = Nothing

        Select Case gStimOutput
            Case GENMODE.genAcoustical, GENMODE.genAcousticalUnity 'acoustic
                'nothing
            Case Else 'electric
                'If gStimOutput <> (GENMODE.genAcoustical Or GENMODE.genAcousticalUnity) Then
                ' so far, both fitting files are needed -> both implants are used and monaural stimulation not possible
                ' use glImpLeft and glImpRight to reconstruct the query to allow monaural stimulation with RIB2 (note: not implemented yet)
                If gszFittFileLeft = "" Or gszFittFileRight = "" Then
                    szErr = "Set the fitting file names before connecting"
                    GoTo SubError
                End If
                ' read fitting file left
                F4FL.ClearParameters()
                szX = F4FL.OpenFile(gszSourceDir & "\" & gszFittFileLeft)
                If Len(szX) <> 0 Then
                    F4FL.ClearParameters()
                    szErr = "Error reading fitting file:" & vbCrLf & szX
                    GoTo SubError
                End If
                ' read fitting file right
                F4FR.ClearParameters()
                szX = F4FR.OpenFile(gszSourceDir & "\" & gszFittFileRight)
                If Len(szX) <> 0 Then
                    F4FR.ClearParameters()
                    szErr = "Error reading fitting file:" & vbCrLf & szX
                    GoTo SubError
                End If
                'End If
        End Select

        ' create event
        ExpSuite.Events.OnConnect()
        If Not (gblnConnectLeft Or gblnConnectRight) Then
            szErr = "Error on connect!"
            GoTo SubError
        End If
        ' init STIM1
        SetStatus("Init STIM - Please wait...")
        lstLog.Visible = True
        lstStatus.Visible = False
        mnuSTIMLogList.Checked = True
        STIM.SourceDir = gszSourceDir
        If gblnDestinationDir Then

            'absolute path?
            If Mid(gszDestinationDir, 1, 2) = "\\" Or InStr(1, gszDestinationDir, ":") > 0 Then
                STIM.DestinationDir = gszDestinationDir
            Else 'NOT absolute path (relative)
                'Fitting File In [application] \ [RIB/RIB2/NIC] folder?
                If Mid(gszDestinationDir, 1, 1) = "\" Then szX = "" Else szX = "\"
                STIM.DestinationDir = My.Application.Info.DirectoryPath & szX & gszDestinationDir ' relative path
            End If
            If (INISettings.gStimOutput = GENMODE.genAcoustical Or INISettings.gStimOutput = GENMODE.genVocoder Or INISettings.gStimOutput = GENMODE.genAcousticalUnity) And gblnDoNotConnectToDevice = False Then
                szErr = frmSettings.CheckDestinationDir(STIM.DestinationDir)
                If szErr <> "" Then MsgBox(szErr, MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation, "Special Characters in Output Directory") : szErr = ""
            End If
        Else
            STIM.DestinationDir = "%temp%"
        End If
        STIM.ID = gszExpID
        STIM.GenerationMode = INISettings.gStimOutput
        If gblnSilentMode Then STIM.LoggingMode = 0 Else STIM.LoggingMode = DirectCast(glLogMode, STIM.LOGMODE)
        STIM.Description = gszDescription
        'STIM.ShowStimulusFlags = TStr(glShowStimulusFlags)
        STIM.CreateWorkDir = gblnNewWorkDir
        STIM.UseMatlab = gblnUseMATLAB
        STIM.MATLABServer = gszMATLABServer
        STIM.MATLABPath = gszMATLABPath
        szErr = STIM.Init(glSamplingRate, glResolution) ' launch Matlab?
        If Len(szErr) <> 0 Then
            If InStr(szErr, "nonexistent or not a directory") > 0 Then
                If (INISettings.gStimOutput = GENMODE.genElectricalRIB2 And gblnRIB2Simulation) Or (INISettings.gStimOutput = GENMODE.genElectricalRIB And gblnRIBSimulation) Then
                    MsgBox("RIB/RIB2 path could not be found! Your are in stimulation mode, so I allow you to connect to output but be aware that RIB/RIB2 files are not available!", MsgBoxStyle.Exclamation, "RIB/RIB2 path") : szErr = ""
                Else
                    szErr = "Please check if the path for electrical stimulation files (Options -> RIB/RIB2/NIC) exists! " & _
                    "Otherwise create it, or set a different path." & vbCrLf & _
                    "You can also switch to acoustical stimulation in settings!" & vbCrLf & vbCrLf & szErr
                    GoTo SubError
                End If
            Else
                GoTo SubError
            End If

        End If


        ' log versions
        STIM.Log("Application Version", My.Application.Info.Version.Major & "." & _
                                        My.Application.Info.Version.Minor & "." & _
                                        My.Application.Info.Version.Build)
        STIM.Log("based on FrameWork Version", TStr(FW_MAJOR) & "." & TStr(FW_MINOR) & "." & TStr(FW_REVISION))
        ' register channels
        SetStatus("Register left channel...")
        stPar.szFittFile = gszFittFileLeft
        stPar.lChNr = 0
        stPar.lResolution = glResolution
        stPar.lSamplingRate = glSamplingRate
        szErr = STIM.RegisterChannel(stPar)
        If Len(szErr) <> 0 Then GoTo SubErrorSTIM
        stPar.lFadeIn = CInt(Math.Round(gsFadeIn * 1000 / stPar.sTimeBase))
        stPar.lFadeOut = CInt(Math.Round(gsFadeOut * 1000 / stPar.sTimeBase))
        gstLeft = stPar
        SetStatus("Register right channel...")
        stPar.szFittFile = gszFittFileRight
        stPar.lChNr = 1
        stPar.lResolution = glResolution
        stPar.lSamplingRate = glSamplingRate
        szErr = STIM.RegisterChannel(stPar)
        If Len(szErr) <> 0 Then GoTo SubErrorSTIM
        stPar.lFadeIn = CInt(Math.Round(gsFadeIn * 1000 / stPar.sTimeBase))
        stPar.lFadeOut = CInt(Math.Round(gsFadeOut * 1000 / stPar.sTimeBase))
        gstRight = stPar
        ' show status list again
        mnuSTIMLogList.Checked = False
        lstLog.Visible = False
        lstStatus.Visible = True
        ' configure output
        ' init output device(s)
        Me.SetStatus("Init Output Device...")
        szErr = Output.Connect(glTrackerCOM > 0 And gblnTrackerUse And glTrackerMode = 1) ' launch pd?    also for YAMI tracker
        If Len(szErr) <> 0 Then GoTo SubErrorSTIM
SubInitTracker:
        ' init tracker
        If glTrackerCOM > 0 And gblnTrackerUse And glTrackerMode = 1 Then 'YAMI tracker
            ' init
            Me.SetStatus("Init Tracker...")
            szErr = Tracker.Init(glTrackerCOM, glTrackerBaudrate, glTrackerSensorCount, gblnTrackerSimulation, glTrackerRepRate, gsngTrackerPosScaling)
            If Len(szErr) > 0 Then
                If MsgBox(szErr, MsgBoxStyle.Critical Or MsgBoxStyle.RetryCancel, "Connect to Tracker") = MsgBoxResult.Cancel Then
                    GoTo SubErrorSTIM
                Else
                    GoTo SubInitTracker
                End If
            End If
            ' set offset
            For lX = 0 To glTrackerSensorCount - 1
                Me.SetStatus("Set Offset Sensor " & TStr(lX))
                With gtsTrackerOffset(lX)
                    Tracker.SetOffset(CInt(lX), .sngX, .sngY, .sngZ, .sngA, .sngE, .sngR)
                End With
            Next
        ElseIf gblnTrackerUse And glTrackerMode = 2 Then 'ViWo Tracker
            ' init
            Me.SetStatus("Init Tracker...")
            szErr = Tracker.Init(-1, -1, 2, gblnTrackerSimulation, -1, -1)
            If Len(szErr) > 0 Then
                If MsgBox(szErr, MsgBoxStyle.Critical Or MsgBoxStyle.RetryCancel, "Connect to Tracker") = MsgBoxResult.Cancel Then
                    GoTo SubErrorSTIM
                Else
                    GoTo SubInitTracker
                End If
            End If
            ' set offset
            For lX = 0 To glTrackerSensorCount - 1
                Me.SetStatus("Set Offset Sensor " & TStr(lX))
                With gtsTrackerOffset(lX)
                    Tracker.SetOffset(CInt(lX), .sngX, .sngY, .sngZ, .sngA, .sngE, .sngR)
                End With
            Next
        ElseIf gblnTrackerUse And glTrackerMode = 3 Then 'Optitrack
            ' init
            Me.SetStatus("Init Tracker...")
            szErr = Tracker.Init(-1, -1, 1, gblnTrackerSimulation, -1, -1)

        End If
        ' init turntable
        If gblnTTUse And (glTTMode = 1 Or (glTTMode = 2 And glTTLPT > 0) Or glTTMode = 3) Then
            szErr = Turntable.Init
            If Len(szErr) > 0 Then GoTo SubErrorSTIM
            SetStatus("Turntable activated")
        End If
        ' ViWo: load world
        If ViWo.Connected Then
            SetStatus("ViWo: world loading")
            szErr = ViWo.WorldLoad(gszViWoWorld, Me.pbStatus)
            If Len(szErr) > 0 Then
                'SetUIBusy()
                ViWo.Disconnect()
                'If Len(gszViWoAddress) > 0 Then
                SetStatus("ViWo: try to reconnect")
                szErr = ViWo.Connect(pbStatus)
                If Len(szErr) > 0 Then GoTo SubErrorSTIM
                SetStatus("ViWo: connected to " & gszViWoAddress & ":" & TStr(glViWoPort))
                SetStatus("ViWo: world loading (2nd try)")
                szErr = ViWo.WorldLoad(gszViWoWorld, Me.pbStatus)
                'SetUIReady()
                If Len(szErr) > 0 Then GoTo SubErrorSTIM
            End If
            'SetUIReady()
            SetStatus("ViWo: connected to " & ViWo.Version)
            SetStatus("ViWo: loading preview parameters")
            szErr = ViWo.LoadPreviewParameters(gszViWoWorld, Me.pbStatus)
            If Len(szErr) > 0 Then GoTo SubErrorSTIM
            SetStatus("ViWo: setting parameters")
            szErr = ViWo.SendAllParameters
            If Len(szErr) > 0 Then GoTo SubErrorSTIM

            STIM.Log("ViWo version," & ViWo.Version)
            STIM.Log("ViWo address," & gszViWoAddress)
            STIM.Log("ViWo port," & TStr(glViWoPort))
            STIM.Log("")
        End If

        ' set global flags
        gblnFirstExperiment = True
        gblnStimulationDone = False
        'szBackupItemListFileName = STIM.WorkDir & "\~" & gszSettingTitle & "_backup_" & System.DateTime.Now.ToString("yyyyMMdd_HHmmss") & "." & gszItemListExtension 'backup file name
        'Dim elapsed_time As TimeSpan = Now.Subtract(start_time) 'timer

        ' update form
        SetStatus("Successfully connected (" & TStr(Math.Round(Now.Subtract(start_time).TotalSeconds)) & " s)")
        ToolTip1.SetToolTip(lblWorkDir, STIM.WorkDir)
        lblWorkDir.Text = CutDirName(STIM.WorkDir, (lblWorkDir.Width), "")
        lblWorkDir.Font = VB6.FontChangeItalic(lblWorkDir.Font, False)
        lstLog.Visible = False
        lstStatus.Visible = True
        ' lock work dir
        If Not (gblnSilentMode) Then FileOpen(98, STIM.WorkDir & "\stimlog.csv", OpenMode.Random, OpenAccess.Read, OpenShare.Shared)

        ' save settings and item list files
        Dim settingsFile As String = "settings.AMTatARI"
        Dim itemListFile As String = "itemlist.itl.csv"
        INISettings.WriteFile(STIM.WorkDir & "\" & settingsFile)
        ItemList.Save(STIM.WorkDir & "\" & itemListFile)

        ' copy script to generate sofa to the new folder
        Result.CopyScriptToGenerateSOFA()

        SetUIReady()
        Return

SubErrorSTIM:
        MsgBox(szErr, MsgBoxStyle.Critical, "Connect")
        SetStatus("Closing STIM")
        STIM.Finish()
        PlayerOff()
        gblnOutputStable = False
        lblWorkDir.Text = "not available yet..."
        ExpSuite.Events.OnDisconnect()
        SetUIReady()
        Return

SubError:
        MsgBox(szErr, MsgBoxStyle.Critical, "Connect")
        lblWorkDir.Text = "not available yet..."
        SetUIReady()
        Return

    End Sub


    Private Function Disconnect() As Integer
        Dim szErr As String

        ' disable controls
        SetUIBusy()
        ' backup log list?
        If gblnAutoBackupLogFile And gblnStimulationDone Then
            mnuBackupLogFileAs_Click(mnuBackupLogFileAs, New System.EventArgs())
        End If
        ' unlock work dir
        FileClose(98)
        ' Pull brake of turntable
        Turntable.PullBrake()
        ' ViWo: unload world
        If ViWo.Connected Then
            SetStatus("Closing ViWo")
            szErr = ViWo.WorldUnload
            If Len(szErr) > 0 Then GoTo SubError
        End If
        ' disconnect output device(s)
        SetStatus("Closing Output")
        szErr = Output.Disconnect
        gszYamiVersion = Nothing
        If Len(szErr) <> 0 Then GoTo SubError
        ' finish stim
        SetStatus("Closing STIM")
        szErr = STIM.Finish
        If Len(szErr) <> 0 Then GoTo SubError
        gblnOutputStable = False

        ' set labels
        SetStatus("Successfully disconnected")
        sbStatusBar.Items.Item(STB_LEFT).Text = ""
        sbStatusBar.Items.Item(STB_RIGHT).Text = ""
        mnuSTIMLogList.Checked = False
        lstLog.Visible = False
        lstStatus.Visible = True
        lblWorkDir.Font = VB6.FontChangeItalic(lblWorkDir.Font, True)
        ExpSuite.Events.OnDisconnect()
        GoTo SubEnd

SubError:
        MsgBox(szErr, MsgBoxStyle.Critical, "Disconnect")
        Disconnect = 1
        GoTo SubEnd

SubEnd:
        SetUIReady()
        Return Nothing
    End Function


    ' ------------------------------------------------------------------
    ' USER INTERFACE - GENERAL
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' Set the user interface to state: busy
    ''' </summary>
    ''' <remarks>This function can be used to disable the user interface while processing for longer period.
    ''' The most buttons but Cancel will be disabled, all menus too.
    ''' The cursor changes to hour glass and item list is not available.</remarks>
    Private Sub SetUIBusy()
        Dim ctrX As System.Windows.Forms.Control
        'Me.Cursor = Cursors.WaitCursor
  
        ' general controls
        If tbToolBar.InvokeRequired Then Err.Raise(vbObjectError, "Invokerequied")
        tbToolBar.Enabled = False
        cmdItemMoveTop.Enabled = False
        cmdItemMoveUp.Enabled = False
        cmdItemUndo.Enabled = False
        cmdItemInsert.Enabled = False
        cmdItemDel.Enabled = False
        cmdItemMoveDown.Enabled = False
        cmdItemMoveBottom.Enabled = False
        ctxtmnuItemClearCells.Enabled = False
        ctxtmnuItemCopy.Enabled = False
        ctxtmnuItemPaste.Enabled = False
        ctxtmnuItemDel.Enabled = False
        ctxtmnuItemDuplicateBlock.Enabled = False
        ctxtmnuItemInsert.Enabled = False
        ctxtmnuItemRenumber.Enabled = False
        ctxtmnuItemSetExperimentBlock.Enabled = False
        ctxtmnuItemShuffleBlock.Enabled = False
        ctxtmnuItemUndo.Enabled = False
        ctxtmnuOptColWidth.Enabled = False
        For Each ctrX In Me.PanelBottom.Controls
            If TypeOf ctrX Is System.Windows.Forms.Button Then ButtonState(CType(ctrX, Button), False)
            If TypeOf ctrX Is System.Windows.Forms.TextBox Then TextBoxState(DirectCast(ctrX, TextBox), False)
        Next ctrX
        For Each ctrX In Me.PanelItemList.Controls
            If TypeOf ctrX Is System.Windows.Forms.Button Then ButtonState(CType(ctrX, Button), False)
            If TypeOf ctrX Is System.Windows.Forms.TextBox Then TextBoxState(DirectCast(ctrX, TextBox), False)
        Next ctrX
        cmdCancel.Enabled = True
        ' menus
        mnuFile.Enabled = False
        mnuItemEdit.Enabled = False
        mnuView.Enabled = False
        mnuExp.Enabled = False
        mnuHelp.Enabled = False
        ' comboboxes
        cmbResult.Enabled = False
        ' itemlist
        'dgvItemList.Enabled = False
        ' flags
        gblnCancel = False
        ' cursor
        Me.Cursor = System.Windows.Forms.Cursors.WaitCursor ' hour glass

    End Sub

    ''' <summary>
    ''' Set the user interface to state: ready
    ''' </summary>
    ''' <remarks>SetUIReady sets the main window interface to a valid state after SetUIBusy.
    ''' The state of the buttons/menus depends on many parameters such as running experiment, connection to the output,
    ''' opened settings window, etc...</remarks>
    Public Sub SetUIReady()
        Dim szX, szY As String
        
        ' statusbar
        sbStatusBar.Items.Item(STB_LEFT).BackColor = Drawing.SystemColors.Control
        sbStatusBar.Items.Item(STB_RIGHT).BackColor = Drawing.SystemColors.Control
        ' menus
        mnuFile.Enabled = True
        mnuItemEdit.Enabled = dgvItemList.Enabled
        mnuItemUndo.Enabled = mblnUndo And Not gblnExperiment 'And Not gblnRemoteClientConnected
        mnuView.Enabled = True
        mnuExp.Enabled = True
        mnuHelp.Enabled = True
        mnuFileLoad.Enabled = Not gblnOutputStable And Not gblnExperiment And Not gblnSettingsForm 'And Not gblnRemoteClientConnected
        mnuFileNew.Enabled = Not gblnOutputStable And Not gblnExperiment And Not gblnSettingsForm 'And Not gblnRemoteClientConnected
        mnuFileSaveAs.Enabled = True
        mnuItemSaveListAs.Enabled = dgvItemList.Enabled
        mnuItemInsert.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        mnuItemRenumber.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        mnuItemDuplicateBlock.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        mnuItemDel.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        mnuItemShuffleBlock.Enabled = Not gblnExperiment 'Not gblnRemoteClientConnected
        ToolStripMenuItem1.Enabled = dgvItemList.Enabled And Not gblnRemoteClientConnected
        mnuFillAutomatically.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        mnuItemSetExperimentBlock.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        mnuItemClearList.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
        mnuItemAppend.Enabled = dgvItemList.Enabled 'And Not gblnRemoteClientConnected
        mnuBackupLogFileAs.Enabled = gblnOutputStable
        mnuQuickSave.Enabled = CBool(lblWorkDir.Text <> "not available yet...")
        mnuViewOptions.Enabled = Not gblnOutputStable And Not gblnExperiment And Not gblnSettingsForm
        mnuRemoteMonitor.Enabled = Not gblnExperiment
        mnuRemoteMonitorUpdateSettings.Enabled = gblnRemoteClientConnected
        mnuConnectOutput.Checked = gblnOutputStable
        mnuConnectOutput.Enabled = Not gblnExperiment
        mnuSnapshot.Enabled = gblnOutputStable
        mnuStartExp.Enabled = gblnOutputStable And dgvItemList.Enabled And Not gblnExperiment  'And Not gblnRemoteClientConnected
        mnuStartExpAtItem.Enabled = gblnOutputStable And dgvItemList.Enabled And Not gblnExperiment  'And Not gblnRemoteClientConnected
        mnuExpContinue.Enabled = gblnOutputStable And dgvItemList.Enabled And Not gblnExperiment  'And Not gblnRemoteClientConnected
        mnuItemStimulateSelected.Enabled = gblnOutputStable And dgvItemList.Enabled And Not gblnExperiment  'And Not gblnRemoteClientConnected
        mnuLevelDancer.Enabled = gblnOutputStable And Not gblnExperiment
        mnuRemoteMonitorGetSettings.Enabled = gblnRemoteClientConnected
        mnuRemoteMonitorGetItemlist.Enabled = gblnRemoteClientConnected
        mnuRemoteMonitorDisconnect.Enabled = gblnRemoteClientConnected
        mnuRemoteMonitorFollowCurrentItem.Enabled = gblnRemoteClientConnected
        mnuRemoteMonitorUpdateSettings.Enabled = gblnRemoteClientConnected
        If Not gblnRemoteClientConnected Then
            mnuRemoteMonitorUpdateSettings.Checked = True
            gblnRemoteMonitorUpdateSettings = True
        End If
        ' toolbar - buttons
        tbToolBar.Enabled = True
        tbButtonNew.Enabled = Not gblnOutputStable And Not gblnExperiment And Not gblnSettingsForm 'And Not gblnRemoteClientConnected
        tbButtonLoad.Enabled = Not gblnOutputStable And Not gblnExperiment And Not gblnSettingsForm 'And Not gblnRemoteClientConnected
        tbButtonSaveAs.Enabled = True
        tbButtonOptions.Enabled = Not gblnOutputStable And Not gblnExperiment And Not gblnSettingsForm
        tbButtonConnect.Checked = gblnOutputStable ' stimulation button
        tbButtonConnect.Enabled = Not gblnExperiment
        frmSettings.cmbExpType.Enabled = Not gblnExperiment And Not gblnSettingsForm 'And Not gblnRemoteClientConnected
        frmOptions.txtYAMIPort.Enabled = Not gblnExperiment And Not gblnSettingsForm 'And Not gblnRemoteClientConnected
        frmOptions.txtLocalYAMIPort.Enabled = Not gblnExperiment And Not gblnSettingsForm 'And Not gblnRemoteClientConnected
        If gblnOutputStable Then
            tbButtonConnect.Image = CType(My.Resources.connected, System.Drawing.Image)
        Else
            tbButtonConnect.Image = CType(My.Resources.disconnected, System.Drawing.Image)
        End If

        tbButtonSnapshot.Enabled = gblnOutputStable
        ' buttons
        cmdItemStimulateSelected.Enabled = Not gblnExperiment And gblnOutputStable And dgvItemList.Enabled  'And Not gblnRemoteClientConnected
        cmdContinueExp.Enabled = gblnOutputStable And dgvItemList.Enabled And Not gblnExperiment  'And Not gblnRemoteClientConnected
        cmdStartExp.Enabled = gblnOutputStable And dgvItemList.Enabled And Not gblnExperiment  'And Not gblnRemoteClientConnected
        cmdInitButton.Enabled = Not gblnExperiment And gblnOutputStable
        cmdGenerateSOFA.Enabled = Not gblnExperiment And gblnOutputStable
        cmdShowPlots.Enabled = Not gblnExperiment And gblnOutputStable
        cmdInitialCheck.Enabled = Not gblnExperiment And gblnOutputStable
        cmdSanityCheck.Enabled = Not gblnExperiment And gblnOutputStable

        szX = "Start Experiment"
        If glExperimentStartItem >= 0 And glExperimentEndItem >= 0 Then
            If glExperimentEndItem > glExperimentStartItem Then
                szX = szX & vbCrLf & "#" & TStr(glExperimentStartItem + 1) & " to #" & TStr(glExperimentEndItem + 1)
            Else
                szX = szX & vbCrLf & "#" & TStr(glExperimentStartItem + 1)
            End If
        End If
        cmdStartExp.Text = szX
        cmdItemCreateList.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
        If glRepetition > 1 Then
            cmdItemAddRepetition.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
            'cmdItemAddRepetition.Text = "Add Repetition (" & TStr(glRepetition) & ")"
            ToolTip1.SetToolTip(cmdItemAddRepetition, TStr(glRepetition) & " Repetitions")
        Else
            cmdItemAddRepetition.Enabled = False
            'cmdItemAddRepetition.Text = "Add Repetition"
            ToolTip1.SetToolTip(cmdItemAddRepetition, "")
        End If
        Me.cmdItemAddRepetition.Text = "Add Repetition"
        cmdItemShuffleList.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdItemDel.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdItemInsert.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdItemMoveUp.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdItemMoveDown.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdItemMoveTop.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdItemMoveBottom.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdItemUndo.Enabled = Not gblnExperiment And mblnUndo 'And Not gblnRemoteClientConnected
        cmdCreateAllStimuli.Enabled = gblnOutputStable And Not gblnExperiment And dgvItemList.Enabled 'And Not gblnRemoteClientConnected
        cmdItemSortList.Enabled = dgvItemList.Enabled And Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdItemSet.Enabled = dgvItemList.Enabled AndAlso (ItemList.SelectedColumnFirst = ItemList.SelectedColumnLast)
        If cmdItemSet.Enabled And ItemList.ItemCount > 0 Then
            cmdItemBrowse.Enabled = _
              ((ItemList.ColFlag(ItemList.SelectedColumnFirst) And 15) = clsItemList.ItemListFlags.ifDirectory) Or _
              ((ItemList.ColFlag(ItemList.SelectedColumnFirst) And 15) = clsItemList.ItemListFlags.ifFileName)
        Else
            cmdItemBrowse.Enabled = False
        End If
        cmdItemLogList.Enabled = dgvItemList.Enabled And Not gblnExperiment And gblnOutputStable 'And Not gblnRemoteClientConnected
        cmdItemStimulateAll.Enabled = dgvItemList.Enabled And Not gblnExperiment And gblnOutputStable 'And Not gblnRemoteClientConnected
        cmdCancel.Enabled = gblnExperiment 'And Not gblnRemoteClientConnected
        cmdExpShow.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdExpHide.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
        cmdResultExecute.Enabled = True

        cmdTTShow.Visible = glTTMode = 1 Or (glTTMode = 2 And glTTLPT > 0) Or glTTMode = 3
        lblTTShow.Visible = False ' cmdTTShow.Visible ' don't show the TT label in this version
        'cmdTTShow.Enabled = gblnOutputStable And gblnTTUse And (glTTLPT > 0)
        cmdTTShow.Enabled = gblnTTUse And cmdTTShow.Visible ' (glTTMode = 1 Or (glTTMode = 2 And glTTLPT > 0))
        ' cursor
        Me.Cursor = System.Windows.Forms.Cursors.Default ' default
        ' text boxes
        TextBoxState(txtSelItem, dgvItemList.Enabled And (ItemList.SelectedColumnFirst = ItemList.SelectedColumnLast))
        ' labels
        lblExpType.Text = "(" & TStr(glExpType) & ") " & gszExpTypeNames(glExpType)
        ToolTip1.SetToolTip(lblExpType, lblExpType.Text)
        Select Case INISettings.gStimOutput
            Case STIM.GENMODE.genAcoustical
                lblStimOutput.Text = "Acoustic (Pd)"
            Case STIM.GENMODE.genAcousticalUnity
                lblStimOutput.Text = "Acoustic (Unity)"
            Case GENMODE.genElectricalRIB
                lblStimOutput.Text = "Electric (RIB)"
            Case GENMODE.genElectricalNIC
                lblStimOutput.Text = "Electric (NIC)"
            Case GENMODE.genElectricalRIB2
                lblStimOutput.Text = "Electric (RIB2)"
            Case GENMODE.genVocoder
                lblStimOutput.Text = "Electrical/Vocoder (Pd)"
        End Select
        If Strings.Right(gszDestinationDir, 1) = "\" Then szY = "" Else szY = "\"
        If gblnDestinationDir Then
            If Len(gszDestinationDir) = 0 Then szX = My.Application.Info.DirectoryPath & szY Else szX = gszDestinationDir & szY
        Else
            szX = System.IO.Path.GetTempPath
        End If
        If ItemList.ItemCount < 1 Then lblItemNr.Text = "Empty" Else lblItemNr.Text = TStr(ItemList.ItemCount)
        If gblnNewWorkDir Then szY = gszExpID & "_*" Else szY = ""
        ToolTip1.SetToolTip(lblRootDir, szX & szY)

        lblRootDir.Text = CutDirName(szX, (lblRootDir.Width), szY)
        If Strings.Right(lblRootDir.Text, 1) = "\" Then lblRootDir.Text = Strings.Left(lblRootDir.Text, lblRootDir.Text.Length - 1)

        dgvItemList_SelChange(dgvItemList, New System.EventArgs())

        ' checkboxes
        chkExpRun.Enabled = gblnExperiment
        ' comboboxes
        cmbResult.Enabled = True
        ' timers
        If frmExp.gexpState <> frmExp.EXPSTATE.expInit Then tmrStatus.Enabled = gblnExperiment 'Then 'And Not gblnRemoteClientConnected
        connectionTimer.Enabled = gblnRemoteClientConnected
        ' form caption
        Dim szVersion As String = My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Build
        If My.Application.Info.Version.Revision <> 0 Then
            szVersion = szVersion & "." & My.Application.Info.Version.Revision
        End If
        szVersion = szVersion & " (FW " & FW_MAJOR & "." & FW_MINOR & "." & FW_REVISION & ")"
        If gblnSettingsChanged Then
            szX = " * - " & My.Application.Info.Title & " " & szVersion
        Else
        szX = " - " & My.Application.Info.Title & " " & szVersion
        End If
        Me.Text = CutDirName(gszSettingFileName, CInt(Me.Width / 1.2), szX, True)

        ' frames
        If gblnRemoteMonitorled Then
            'lblItemList.Text = "Remote from " & wskRemoteClient(1).RemoteHost
            lblItemList.ForeColor = Drawing.SystemColors.MenuHighlight
            lblItemList.Font = VB6.FontChangeItalic(lblItemList.Font, True)
        Else
            If Len(gszItemListTitle) = 0 Then lblItemList.Text = ""
            If Len(gszItemListTitle) <> 0 Then lblItemList.Text = gszItemListTitle
            lblItemList.ForeColor = Drawing.SystemColors.ControlText
            lblItemList.Font = VB6.FontChangeItalic(lblItemList.Font, False)
        End If

        'Connect button:
        Dim szOutput As String = ""
        If gblnUseMATLAB Then szOutput = vbCrLf & "- Matlab" 'Matlab?

        If gblnDoNotConnectToDevice = False Then 'connect to device?
            Select Case INISettings.gStimOutput 'device type?
                Case GENMODE.genAcoustical, GENMODE.genVocoder
                    szOutput = szOutput & vbCrLf & "- pd (Acoustical)"
                Case GENMODE.genAcousticalUnity
                    szOutput = szOutput & vbCrLf & "- Unity (Acoustical)"
                Case GENMODE.genElectricalNIC
                    szOutput = szOutput & vbCrLf & "- NIC (Electrical)"
                Case GENMODE.genElectricalRIB
                    szOutput = szOutput & vbCrLf & "- RIB (Electrical)"
                    If gblnRIBSimulation = True Then szOutput &= " [Simulation]"
                Case GENMODE.genElectricalRIB2
                    szOutput = szOutput & vbCrLf & "- RIB2 (Electrical)"
                    If gblnRIB2Simulation = True Then szOutput &= " [Simulation]"
            End Select
        End If

        If szOutput = "" Then szOutput = "No connections will be established"
        tbButtonConnect.ToolTipText = "Connect to output: " & szOutput

        Me.Cursor = Cursors.Default ' in case someone changed the cursor to hour glass symbol...

    End Sub
    ''' <summary>
    ''' Cut a part of a directory to get a string not longer than lWidth including an appendix szAppendix.
    ''' </summary>
    ''' <param name="szDir">String containing directory.</param>
    ''' <param name="lWidth">Width of the label.</param>
    ''' <param name="szAppendix">Additional appendix of the string. Will be considered calculating the total witdh of the caption.</param>
    ''' <param name="NoBackslash">Optional: If boolean is true, no backslash is appended to szDir.</param>
    ''' <returns>Cut string</returns>
    ''' <remarks>Parts of szDir between two "\" will be removed to match the width of lWidth+Width(szAppendix).
    '''If szDir ends with a backslash, it will be removed
    '''After cutting a backslash is appended to the end of the cutted string (if optional boolean NoBackslash is not true), then the szappendix is appended.
    ''' Works for frmMain only.</remarks>
    Private Function CutDirName(ByVal szDir As String, ByVal lWidth As Integer, ByVal szAppendix As String, Optional ByVal NoBackslash As Boolean = False) As String
        Dim lX, lY As Integer
        Dim szPost, szPre, szX As String
        Dim StringSize As New SizeF
        Dim g As Graphics = Me.CreateGraphics

        StringSize = g.MeasureString(szDir & szAppendix, Me.Font)
        If StringSize.Width > lWidth Then
            lX = InStr(1, szDir, "\")
            If Microsoft.VisualBasic.Right(szDir, 1) = "\" Then szDir = Mid(szDir, 1, Len(szDir) - 1)
            If lX > 0 Then
                szPre = Mid(szDir, 1, lX - 1)
                lY = InStrRev(szDir, "\")
                szPost = "\..." & Mid(szDir, lY)
                szX = Mid(szDir, lX, lY - lX)
                StringSize = g.MeasureString(szPre & szX & szPost & szAppendix, Me.Font)
                While (StringSize.Width > lWidth) And (lY > lX)
                    lY = InStrRev(szDir, "\", lY - 1)
                    szX = Mid(szDir, lX, lY - lX)
                End While
                szDir = szPre & szX & szPost
            End If
            If Not NoBackslash Then szDir &= "\"
        End If

        CutDirName = szDir & szAppendix

    End Function
    ''' <summary>
    ''' Set the status line.
    ''' </summary>
    ''' <param name="szStatus">String with the text. This text will be added to the log list too.</param>
    ''' <remarks></remarks>

    Delegate Sub SetStatusCallback(ByVal szStatus As String)
    Public Sub SetStatus(ByVal szStatus As String)
        'If Me.InvokeRequired Then Me.Invoke(New SetStatusDelegate(AddressOf SetStatus), szStatus)
        'If sbStatusBar.InvokeRequired Then Err.Raise(vbObjectError, "Invoke required")
        'sbStatusBar.Items.Item(STB_STATUS).Text = szStatus
        'If lstStatus.Items.Count > 1000 Then lstStatus.Items.RemoveAt((0))
        'lstStatus.Items.Add((szStatus))
        'lstStatus.SelectedIndex = lstStatus.Items.Count - 1
        Proc.SetStatus(Me.lstStatus, Me._sbStatusBar_Panel0, szStatus)
    End Sub

    Delegate Sub MnuEnableCallback(ByVal tsmi As ToolStripMenuItem, ByVal enable As Boolean)
    Public Sub MnuEnable(ByVal tsmi As ToolStripMenuItem, ByVal enable As Boolean)
        tsmi.Enabled = enable
    End Sub

    ''' <summary>
    ''' Set the progress bar to a value.
    ''' </summary>
    ''' <param name="sVal">Value of the progress bar in percent. Valid range: 0..100%</param>
    ''' <remarks></remarks>
    Public Sub SetProgressbar(ByVal sVal As Double)
        If sVal > 100 Then sVal = 100
        If sVal < 0 Then sVal = 0
        pbStatus.Value = CInt(sVal)
    End Sub

    ''' <summary>
    ''' Get the current progress bar value.
    ''' </summary>
    ''' <returns>Current value in range 0..100%</returns>
    ''' <remarks></remarks>
    Public Function GetProgressbar() As Double
        Return pbStatus.Value
    End Function

    ''' <summary>
    ''' Set the status of the output channels.
    ''' </summary>
    ''' <param name="lCh">Channel (0:left square, 1: right square)</param>
    ''' <param name="lState">State (see the list with available states)</param>
    ''' <remarks>This function can be used to indicate the status of two channels
    ''' in the bottom line of the main window, right to the status text.
    ''' Depending on the lState, the background color of the squares changes.
    ''' Four states are available:
    '''<li>0: the standard case, color: grey, standard </li>
    '''<li>1: creating stimulation files, color: yellow </li>
    '''<li>2: playing or stimulating now, color: red </li>
    '''<li>3: intermadiate (e.g. break between stimuli), color: green </li> </remarks>
    Public Sub SetOutputStatus(ByVal lCh As Integer, ByVal lState As Integer)

        If sbStatusBar.InvokeRequired Then Err.Raise(vbObjectError, "Invoke required")
        Select Case lCh
            Case 0
                lCh = STB_LEFT
            Case 1
                lCh = STB_RIGHT
        End Select
        Select Case lState
            Case 0
                sbStatusBar.Items.Item(lCh).BackColor = Drawing.SystemColors.Control
            Case 1
                sbStatusBar.Items.Item(lCh).BackColor = Color.Yellow
            Case 2
                sbStatusBar.Items.Item(lCh).BackColor = Color.Red
            Case 3
                sbStatusBar.Items.Item(lCh).BackColor = Color.DarkGreen
        End Select

    End Sub


    ''' <summary>
    ''' Clear all settings.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ClearParameters()

        INISettings.ClearParameters()

        F4FL.ClearParameters()
        F4FR.ClearParameters()

        gblnExperiment = False
        lblWorkDir.Text = "not available yet..."
        lblWorkDir.Font = VB6.FontChangeItalic(lblWorkDir.Font, False)
        ItemList.Clear()
        If mnuItemEdit.Enabled Then
            For Each ctrX As Control In Controls
                If (TypeOf ctrX Is Button) AndAlso ctrX.Enabled AndAlso ctrX.Visible Then ctrX.Focus() : Exit For
            Next ctrX
        End If
        gblnFirstExperiment = True
        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
    End Sub


    ' ------------------------------------------------------------------
    ' EVENTS - Buttons
    ' ------------------------------------------------------------------

    Private Sub chkExpRun_CheckStateChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles chkExpRun.CheckStateChanged
        If Me.IsInitializing = True Then
            Exit Sub
        Else
            If gblnExperiment Then
                frmExp.SetBreak((Not CBool(chkExpRun.CheckState)))
                If chkExpRun.CheckState = CheckState.Checked And (glBreakFlags And 1) = 1 Then
                    QueryPerformanceCounter(gcurHPTic) ' reset timer on resume
                End If
            End If
        End If
    End Sub

    Private Sub cmbResult_DoubleClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmbResult.DoubleClick
        cmdResultExecute_Click(cmdResultExecute, New System.EventArgs())
    End Sub

    Private Sub cmdCancel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdCancel.Click
        If gblnExperiment Then
            ' experiment pending - ask to be sure
            If MsgBox("You're going to cancel experiment." & vbCrLf & "Do you really want to cancel?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.Yes Then
                gblnCancel = True
                Turntable.EmergencyStop()
                Exit Sub
            End If
        Else
            ' cancel immediatly
            gblnCancel = True
            Turntable.EmergencyStop()
        End If
    End Sub

    Private Sub cmdItemBrowse_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemBrowse.Click
        Dim lX As Integer
        Dim szDir As String
        Dim szX As String

        If ItemList.ItemCount < 1 Then Return

        Select Case ItemList.ColFlag(ItemList.SelectedColumnFirst) And 15
            Case clsItemList.ItemListFlags.ifDirectory
                ' browse for directory
                Dim dlgBrowse As New FolderBrowserDialog With { _
                    .SelectedPath = txtSelItem.Text, _
                    .Description = "Pick a Directory", _
                    .ShowNewFolderButton = True _
                }
                If dlgBrowse.ShowDialog() = Windows.Forms.DialogResult.OK Then
                    txtSelItem.Text = dlgBrowse.SelectedPath
                Else
                    Return
                End If
            Case clsItemList.ItemListFlags.ifFileName
                ' browse for file name
                Dim dlgOpen As New OpenFileDialog
                If ((ItemList.ColFlag(ItemList.SelectedColumnFirst) And &H70S) = clsItemList.ItemListFlags.ifAbsolute) Then
                    dlgOpen.Title = "Browse for a file name..."
                    lX = InStrRev(txtSelItem.Text, "\")
                    If lX > 0 Then
                        szDir = Mid(txtSelItem.Text, 1, lX)
                    Else
                        szDir = gszCurrentDir
                    End If
                    ChangeDir(szDir)
                Else
                    lX = ((ItemList.ColFlag(ItemList.SelectedColumnFirst) \ &H10S) And 7) - 1
                    If lX >= DataDirectory.Count Then MsgBox("This column contains file relative to the data directory #" & TStr(lX + 1) & "." & vbCrLf & "This directory is not defined.") : Return
                    szDir = DataDirectory.Path(lX)
                    dlgOpen.Title = "Browse for a file name in this directory only..."
                End If
                dlgOpen.InitialDirectory = szDir
                dlgOpen.FileName = ""
                dlgOpen.Title = "Browse for a file..."
                dlgOpen.CheckFileExists = True
                dlgOpen.CheckPathExists = True
                szX = ItemList.ColUnit(ItemList.SelectedColumnFirst)
                If Len(szX) > 0 Then
                    dlgOpen.Filter = "Specific Files (" & szX & ")|" & szX & "|All Files (*.*)|*.*"
                    dlgOpen.DefaultExt = szX
                Else
                    dlgOpen.Filter = "All Files (*.*)|*.*"
                    dlgOpen.DefaultExt = "*.*"
                End If
                dlgOpen.FilterIndex = 1
                If dlgOpen.ShowDialog <> Windows.Forms.DialogResult.OK Then Return
                If ((ItemList.ColFlag(ItemList.SelectedColumnFirst) And &H70S) = clsItemList.ItemListFlags.ifAbsolute) Then
                    txtSelItem.Text = dlgOpen.FileName
                Else
                    If Len(szDir) > Len(dlgOpen.FileName) Then lX = 1 Else lX = 0
                    If LCase(Mid(dlgOpen.FileName, 1, Len(szDir))) <> LCase(szDir) Then lX = 1
                    If lX > 0 Then MsgBox("You are not supposed to go up in the directory tree here." & vbCrLf & "If you want to do that, change the data directory #" & TStr((ItemList.ColFlag((ItemList.SelectedColumnFirst)) \ &H10S) And 7)) : Return
                    txtSelItem.Text = Mid(dlgOpen.FileName, Len(szDir) + 2)
                End If
        End Select

    End Sub

    Private Sub cmdItemCreateList_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemCreateList.Click
        Dim szErr As String
        szErr = CreateItemList()
        If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Create Item List")

    End Sub

    Delegate Function CreateItemListDelegate(ByVal lFlags As Integer) As String
    ''' <summary>
    ''' Creates an item list.
    ''' </summary>
    ''' <param name="lFlags">Main Automatisations Flags (mafIgnoreOptionWarnings)</param>
    ''' <returns>Error message or empty if no error ocured.</returns>
    ''' <remarks>This public function can be used to create an item list without using the button or menu.</remarks>
    Public Function CreateItemList(Optional ByVal lFlags As Integer = 0) As String
        Dim szErr As String = ""

        SetUIBusy()
        If (lFlags And FWintern.AutomatisationFlags.IgnoreOptionWarnings) = 0 And dgvItemList.Enabled Then
            If MsgBox("The list contains items and will be cleared before creating new items." & vbCrLf & "Are you sure to continue?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then GoTo SubEnd
        End If

        Console.WriteLine("{0}", Thread.CurrentThread.GetHashCode())
        SetStatus("Create Item List...")
        Dim start_time As DateTime = Now

        UndoSnapshot()

        ItemList.Clear()
        Dim ctrX As System.Windows.Forms.Control
        If ExpSuite.Events.OnCreateItemList <> 0 Then
            For Each ctrX In Me.Controls
                If (TypeOf ctrX Is System.Windows.Forms.Button) Then If ctrX.Enabled And ctrX.Visible Then ctrX.Focus() : Exit For
            Next ctrX
            dgvItemList.Enabled = False
        Else
            If dgvItemList.InvokeRequired Then
                Console.WriteLine("CreateItemList Invoke Required")
            Else
                dgvItemList.Enabled = True
                gblnItemListShuffled = False
                gblnItemListRepeated = False
            End If
        End If
        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piFresh
        ' set optimal column width
        ItemList.SetOptimalColWidth()
        SetStatus("Item List created ("& Strings.Replace(Now.Subtract(start_time).TotalSeconds.ToString("0.0"),",",".") & " s)")
        'mnuOptColWidth_Click(mnuOptColWidth, New System.EventArgs())

SubEnd:
        gszItemListTitle = ""
        gblnSettingsChanged = True
        szBackupItemListFileName= Nothing 'new file name next time
        ServeData(FWintern.ServeDataEnum.ItemlistColCountListStatus)
        SetUIReady()
        Return szErr

    End Function

    Private Sub cmdExpHide_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdExpHide.Click
        SetUIBusy()
        frmExp.UnloadMe()
        SetUIReady()
    End Sub

    Private Sub cmdExpShow_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdExpShow.Click
        SetUIBusy()
        frmExp.Dispose() 'reset form
        ExpSuite.Events.OnExpShow(glExpType, grectExp, INISettings.glExpFlags)
        frmExp.SetHUIDevice(glExpHUIID)
        frmExp.ShowBlankScreen(gblnExpOnTop)
        SetUIReady()
    End Sub

    Private Sub cmdCreateAllStimuli_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdCreateAllStimuli.Click
        CreateAllStimuli()
    End Sub

    ''' <summary>
    ''' Creates all stimuli.
    ''' </summary>
    ''' <returns>Error message or empty if no error ocured.</returns>
    ''' <remarks>This public function can be used to create all stimuli without using the button or menu.</remarks>
    Public Function CreateAllStimuli() As String
        Dim szErr As String = ""
        If ItemList.ItemCount < 1 Then szErr = "No items available!" : GoTo SubEnd
        SetUIBusy()
        ExpSuite.Events.OnCreateAllStimuli()
SubEnd:
        SetUIReady()
        CreateAllStimuli = szErr
        SetProgressbar(0)
    End Function

    Private Sub cmdItemDel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemDel.Click

        'If dgvItemList.SelectedCells.Count < 1 Then Return

        'SetUIBusy()
        'UndoSnapshot()
        'Dim arr() As Integer = ItemList.SelectedItems

        'If arr.Length = ItemList.ItemCount Then
        '    ItemList.Clear()
        '    If mnuItemEdit.Enabled Then
        '        For Each ctrX As Control In Controls
        '            If (TypeOf ctrX Is Button) AndAlso ctrX.Enabled AndAlso ctrX.Visible Then ctrX.Focus() : Exit For
        '        Next ctrX
        '    End If
        'Else
        '    Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
        '    dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        '    Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
        '    dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        '    For lX As Integer = arr.Length - 1 To 0 Step -1
        '        dgvItemList.Rows.RemoveAt(arr(lX))
        '        pbStatus.Value = CInt(Math.Round((arr.Length - lX) / arr.Length * 100))
        '    Next
        '    dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
        '    dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)
        '    ItemList.ItemIndex = ItemList.ItemIndex
        'End If

        'FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
        'ServeData("Delete Item")
        'SetStatus("Items removed")
        'SetUIReady()

        glRemoteClickCounter += 1

        If Not gblnRemoteClicked Then
            gblnRemoteClicked = True
            While glRemoteClickCounter > 0
                Dim endfor As Integer = glRemoteClickCounter
                For i As Integer = 1 To endfor
                    If dgvItemList.SelectedCells.Count < 1 Then Return

                    SetUIBusy()
                    UndoSnapshot()
                    Dim arr() As Integer = ItemList.SelectedItems

                    If arr.Length = ItemList.ItemCount Then
                        ItemList.Clear()
                        If mnuItemEdit.Enabled Then
                            For Each ctrX As Control In Controls
                                If (TypeOf ctrX Is Button) AndAlso ctrX.Enabled AndAlso ctrX.Visible Then ctrX.Focus() : Exit For
                            Next ctrX
                        End If
                    Else
                        Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
                        dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
                        Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
                        dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
                        For lX As Integer = arr.Length - 1 To 0 Step -1
                            dgvItemList.Rows.RemoveAt(arr(lX))
                            pbStatus.Value = CInt(Math.Round((arr.Length - lX) / arr.Length * 100))
                        Next
                        dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
                        dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)
                        ItemList.ItemIndex = ItemList.ItemIndex
                    End If

                    FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
                    SetStatus("Items removed")
                    If i = endfor Then
                        glRemoteClickCounter -= endfor
                        ServeData(FWintern.ServeDataEnum.Itemlist)
                        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
                    End If
                Next
            End While
            gblnRemoteClicked = False
            SetUIReady()
        End If
    End Sub

    Private Sub cmdItemInsert_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemInsert.Click
        glRemoteClickCounter += 1

        If Not gblnRemoteClicked Then
            gblnRemoteClicked = True
            While glRemoteClickCounter > 0
                Dim endfor As Integer = glRemoteClickCounter
                For i As Integer = 1 To endfor
                    If ItemList.ItemCount < 1 Then Return
                    If ItemList.SelectedItems.Length < 1 Then Return
                    UndoSnapshot()
                    SetUIBusy()
                    dgvItemList.Rows.Insert(dgvItemList.CurrentCellAddress.Y + 1, 1)
                    FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
                    SetStatus("Item inserted")
                    If i = endfor Then
                        glRemoteClickCounter -= endfor
                        ServeData(FWintern.ServeDataEnum.Itemlist)
                        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
                    End If
                Next
            End While
            gblnRemoteClicked = False
            SetUIReady()
        End If
    End Sub

    Public Delegate Function RemoteItemCountDelegate(ByVal index As Integer) As Boolean

    Public Function RemoteItemCount(ByVal index As Integer) As Boolean
        ItemList.ItemCount = index
        SetUIReady()
        Return True
    End Function

    Private Sub cmdItemLogList_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemLogList.Click
        Dim lX, lY As Integer
        Dim szArr() As String
        Dim szX As String

        SetUIBusy()
        szX = InputBox("Input a title for item list" & vbCrLf & "(or leave it empty to cancel):", "Log List", "Log List")
        If Len(szX) = 0 Then
            MsgBox("Log List canceled")
            GoTo SubEnd
        End If
        ' log header
        STIM.Log("**********", "Item List", System.DateTime.Now.ToString("HH:mm:ss"), szX)
        ReDim szArr(ItemList.ColCount - 1)
        ' use all
        For lX = 0 To UBound(szArr)
            szArr(lX) = ItemList.ColCaption(lX)
        Next
        STIM.Log(szArr)
        ' log all items in the item list
        For lX = 0 To ItemList.ItemCount - 1
            For lY = 0 To ItemList.ColCount - 1
                szArr(lY) = ItemList.Item(lX, lY)
            Next
            STIM.Log(szArr)
            SetProgressbar(Math.Round(lX / ItemList.ItemCount * 100))
        Next

        STIM.Log("")
        STIM.Log("")
SubEnd:
        SetProgressbar(0)
        SetUIReady()

    End Sub

    Private Sub cmdItemAddRepetition_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs, Optional NoUI As Boolean = False) Handles cmdItemAddRepetition.Click
        Dim lType As Integer

        If dgvItemList.Enabled = False Or dgvItemList.RowCount < 1 Then Return

        If glRepetition = 1 Then Return ' repetition not necessary

        If NoUI = False Then
            lType = MsgBox("Do you want to repeat list after list?" & vbCrLf & "Click 'No' if you want to repeat item after item.", MsgBoxStyle.Question Or MsgBoxStyle.YesNoCancel Or MsgBoxStyle.DefaultButton1, "Add Repetition")
            If lType = MsgBoxResult.Cancel Then Return
        Else
            lType = MsgBoxResult.Yes
        End If

        UndoSnapshot()
        SetUIBusy()
        Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
        dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
        dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None


        If lType = MsgBoxResult.Yes Then
            ' add list after list
            Dim lRows As Integer = ItemList.ItemCount
            ItemList.ItemCount = glRepetition * lRows
            For lY As Integer = 1 To glRepetition - 1
                For lX As Integer = 0 To lRows - 1
                    CopyDataGridRow(dgvItemList.Rows(lX), dgvItemList.Rows(lX + lY * lRows))
                Next

            Next
            SetStatus("Item list repeated (list-by-list)")
        Else
            ' add item after item
            Dim dgvX As New DataGridView With { _
                .AllowUserToAddRows = False, _
                .ColumnCount = dgvItemList.ColumnCount _
            }
            For lX As Integer = 0 To dgvItemList.ColumnCount - 1
                dgvX.Columns(lX).HeaderText = dgvItemList.Columns(lX).HeaderText
            Next
            dgvX.RowCount = 0

            For lY As Integer = 0 To dgvItemList.RowCount - 1
                dgvX.Rows.Add(glRepetition)
                For lX As Integer = 0 To glRepetition - 1
                    CopyDataGridRow(dgvItemList.Rows(lY), dgvX.Rows(lX + lY * glRepetition))
                Next
            Next
            ItemList.ItemCount *= glRepetition
            dgvItemList.Enabled = False
            CopyDataGridView(dgvX, dgvItemList)
            dgvItemList.Enabled = True
            SetStatus("Item list repeated (item-by-item)")
        End If ' repeat list after list
        dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
        dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)

        dgvItemList.Enabled = True
        gblnItemListRepeated = True
        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        SetUIReady()
    End Sub

    Private Sub cmdItemMoveDown_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemMoveDown.Click
        SetUIBusy()
        'If ItemList.SelectedItemLast = ItemList.ItemCount - 1 Then SetUIReady() : Return
        Dim arr() As Integer = ItemList.SelectedItems
        If arr(arr.Length - 1) >= ItemList.ItemCount - 1 Then SetUIReady() : Return
        UndoSnapshot()
        Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
        dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
        dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        For lX As Integer = arr.Length - 1 To 0 Step -1
            dgvItemList.Rows.InsertCopy(arr(lX), arr(lX) + 2)
            CopyDataGridRow(dgvItemList.Rows(arr(lX)), dgvItemList.Rows(arr(lX) + 2))
            dgvItemList.Rows(arr(lX) + 2).Selected = True
            dgvItemList.Rows.RemoveAt(arr(lX))
        Next
        dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
        dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)
        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        SetUIReady()
    End Sub

    Private Sub cmdItemMoveUp_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemMoveUp.Click
        SetUIBusy()
        Dim arr() As Integer = ItemList.SelectedItems
        If arr(0) = 0 Then SetUIReady() : Return
        UndoSnapshot()
        Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
        dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
        dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        For lX As Integer = 0 To arr.Length - 1
            dgvItemList.Rows.InsertCopy(arr(lX) - 1, arr(lX) + 1)
            CopyDataGridRow(dgvItemList.Rows(arr(lX) - 1), dgvItemList.Rows(arr(lX) + 1))
            dgvItemList.Rows.RemoveAt(arr(lX) - 1)
        Next
        dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
        dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)
        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        SetUIReady()
    End Sub

    Private Sub cmdItemSet_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemSet.Click
        If dgvItemList.SelectedCells.Count < 1 Then Return

        Dim szY As String = Trim(txtSelItem.Text)
        Dim szErr As String = ""

        If Len(szY) = 0 Then Return
        For Each cell As DataGridViewCell In dgvItemList.SelectedCells
            Dim szX As String = ItemList.CheckItem(cell.ColumnIndex, szY)
            If Len(szX) > 0 Then szErr = szErr & szX & vbCrLf
        Next

        If Len(szErr) > 0 Then MsgBox(szErr, MsgBoxStyle.Critical) : Return

        UndoSnapshot()
        For Each cell As DataGridViewCell In dgvItemList.SelectedCells
            cell.Value = szY
        Next

        ServeData(FWintern.ServeDataEnum.ChangeItem)

        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
        SetUIReady()
        dgvItemList.Focus()

    End Sub

    Private Sub cmdItemShuffleList_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemShuffleList.Click

        If ItemList.ItemCount < 2 Then Return

        If Not gblnFirstExperiment And ((INIOptions.glWarningSwitches And FWintern.WarningSwitches.wsExpPerformedOnShuffle) <> 0) Then
            If MsgBox("An experiment was performed - the item list may contain results." & vbCrLf & "Are you sure you want to shuffle the item list?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then Return
        End If

        UndoSnapshot()
        SetUIBusy()
        Dim lIdx(ItemList.ItemCount - 1) As Integer
        For lX As Integer = 0 To ItemList.ItemCount - 1
            lIdx(lX) = lX
        Next
        ItemList.ShuffleItems(lIdx)
        gblnItemListShuffled = True

        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)

        ' renumber index
        For lX As Integer = 0 To ItemList.ItemCount - 1
            ItemList.Item(lX, ItemList.GetIndexCol) = TStr(lX + 1)
        Next
        ServeData(FWintern.ServeDataEnum.Renumber)

        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
        Me.SetStatus("Item list shuffled")
        SetUIReady()

    End Sub

    Private Sub cmdItemSortList_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemSortList.Click
        Dim szX As String
        Dim bStringSort As Boolean = False

        If ItemList.ItemCount < 1 Then Return

        With dgvItemList

            Dim szStringForSortedColums As String = " 1: " & dgvItemList.Columns(0).HeaderText.ToString
            Dim lX As Integer = 1
            While lX < .ColumnCount
                'If lX = 4 Then
                '    szStringForSortedColums = szStringForSortedColums & vbCrLf & " ..." & vbCrLf & " " & TStr(.ColumnCount) & ": " & .Columns(.ColumnCount - 1).HeaderText & vbCrLf & vbCrLf
                'Else
                szStringForSortedColums = szStringForSortedColums & vbCrLf & (" ") & TStr(lX + 1) & ": " & .Columns(lX).HeaderText
                'End If
                lX += 1
            End While
            'szStringForSortedColums = "1" & vbCrLf & "2" & vbCrLf & "1" & vbCrLf & "1" & vbCrLf & "5" & vbCrLf & "1" & vbCrLf & "7" & vbCrLf & "1" & vbCrLf & "9" & vbCrLf & "10" & vbCrLf & "10" & vbCrLf & "10" & vbCrLf & "10" & vbCrLf & "10" & vbCrLf & "9" & vbCrLf & "10" & vbCrLf & "10" & vbCrLf & "10" & vbCrLf & "10" & vbCrLf & "10" & vbCrLf & " " & vbCrLf & " "
            If mSortIndex > .ColumnCount Then mSortIndex = 1
            'szX = InputBox("Input the column index to sort, beginning from the index column as 1." & vbCrLf & vbCrLf & _
            '               szStringForSortedColums, "Sort Item List", TStr(mSortIndex))
            szX = InputBox("Sort by column index:" & vbCrLf & vbCrLf & _
               szStringForSortedColums & vbCrLf & " ", "Sort Item List", TStr(mSortIndex)) 
            If Not IsNumeric(szX) Then Return
            mSortIndex = CInt(szX)
            Dim lCol As Integer = CInt(Val(szX))
            If lCol < 1 Or lCol > dgvItemList.ColumnCount Then MsgBox("Invalid column") : Return

            UndoSnapshot()
            SetUIBusy()
            lCol -= 1

            Console.WriteLine(ItemList.ColFlag(lCol) & " -- " & clsItemList.ItemListFlags.ifFlagTypeMask)

            Select Case ItemList.ColFlag(lCol) And clsItemList.ItemListFlags.ifFlagTypeMask

                Case clsItemList.ItemListFlags.ifNumeric
                    If (ItemList.ColFlag(lCol) And clsItemList.ItemListFlags.ifVectorized) <> 0 Then
                        .Columns(lCol).ValueType = GetType(String)
                    Else
                        dgvItemList.Columns(lCol).ValueType = GetType(Double)
                    End If
                Case clsItemList.ItemListFlags.ifIndex
                    .Columns(lCol).ValueType = GetType(Integer)
                Case clsItemList.ItemListFlags.ifInteger And clsItemList.ItemListFlags.ifFlagTypeMask
                    .Columns(lCol).ValueType = GetType(Integer)
                Case Else
                    'Console.WriteLine(ItemList.ColFlag(lCol))
                    .Columns(lCol).ValueType = GetType(String)
                    bStringSort = True
            End Select

            If bStringSort Then
                dgvItemList.Sort(dgvItemList.Columns(lCol), System.ComponentModel.ListSortDirection.Ascending)
            Else
                For lX = 0 To .RowCount - 2
                    For lY As Integer = lX To .RowCount - 1
                        If Val(.Item(lCol, lY).Value) < Val(.Item(lCol, lX).Value) Then
                            'Replace
                            'dgvItemList.Rows.Insert(lY + 1, dgvItemList.Rows(lX).Clone)
                            dgvItemList.Rows.InsertCopy(lY, lX)
                            'Windows.Forms.Application.DoEvents()
                            CopyDataGridRow(dgvItemList.Rows(lY + 1), dgvItemList.Rows(lX))
                            'Windows.Forms.Application.DoEvents()
                            dgvItemList.Rows.RemoveAt(lY + 1)
                            'Windows.Forms.Application.DoEvents()
                        End If
                    Next
                Next
            End If

        End With

        Windows.Forms.Application.DoEvents()

        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        SetUIReady()

    End Sub

    Private Sub cmdItemStimulateAll_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemStimulateAll.Click
        StimulateAllItems()
    End Sub
    ''' <summary>
    ''' Stimulates all item(s).
    ''' </summary>
    ''' <returns>Error message or empty if no error ocured.</returns>
    ''' <remarks>This public function can be used to stimulate all items without using the button or menu.</remarks>
    Private Function StimulateAllItems() As String
        If ItemList.ItemCount < 1 Then Return "Stimulate all items not possible now: No items in the list"
        If Not gblnOutputStable Then Return "Stimulate all items not possible now: Not Connected"

        SetUIBusy()
        frmTurntable.StopTT4ATimer() 'avoid pulling brake during stimulation

        If glTriggerChannel > 0 And gblnUseTriggerChannel = True Then 'create trigger channel file?
            ' create trigger signal
            Dim szErr As String = Output.CreateTriggerSignal()
            If Len(szErr) <> 0 Then Return szErr
        End If

        ExpSuite.Events.OnStimulateAll()
        gblnStimulationDone=True
        If gblnAutoBackupItemList Then
            STIM.BackupItemList(STIM.WorkDir & "\~" & gszSettingTitle & "_backup_" & System.DateTime.Now.ToString("yyyyMMdd_HHmmss") & "." & gszItemListExtension)
            szBackupItemListFileName=Nothing 'new file name next time
        End If
        frmTurntable.StartTT4ATimer() 'enable brake timer

        SetUIReady()
        Return ""
    End Function

    Private Sub cmdItemUndo_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemUndo.Click
        mblnUndo = dgvItemList.Enabled
        Dim dgvTemp As New DataGridView
        CopyDataGridView(dgvItemList, dgvTemp)
        CopyDataGridView(dgvUndo, dgvItemList)
        CopyDataGridView(dgvTemp, dgvUndo)
        dgvItemList.Enabled = True
        ItemList.ItemCount = dgvItemList.RowCount
        ServeData(FWintern.ServeDataEnum.ItemlistColCountListStatus)
        SetStatus("Undo item list")
        SetUIReady()
    End Sub

    Private Sub cmdResultExecute_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdResultExecute.Click
        SetUIBusy()
        Result.Execute(cmbResult.SelectedIndex)
        SetUIReady()
    End Sub

    Public Sub mnuItemStimulateSelected_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemStimulateSelected.Click
        Dim szErr As String
        szErr = StimulateSelected()
        If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Stimulate Selected Item")
    End Sub

    Private Sub cmdItemStimulateSelected_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdItemStimulateSelected.Click
        Dim szErr As String
        szErr = StimulateSelected()
        If InStr(szErr, "RIB2", CompareMethod.Text) <> 0 Then szErr = "RIB2 Error!" & vbCrLf & "Please check if RIB2 files are available!" & vbCrLf & vbCrLf & szErr
        If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Stimulate Selected Item")
    End Sub

    ''' <summary>
    ''' Stimulates the selcted item(s).
    ''' </summary>
    ''' <param name="lFlags">Main Automatisations Flags (not used yet)</param>
    ''' <returns>Error message or empty if no error ocured.</returns>
    ''' <remarks>This public function can be used to stimulate items without using the button or menu.</remarks>
    Private Function StimulateSelected(Optional ByVal lFlags As Integer = 0) As String
        Dim lTO As Integer = 2000
        Dim szLeft As String = ""
        Dim szRight As String = ""
        Dim szErr As String
        Console.WriteLine("stimselected")
        If Not mnuItemStimulateSelected.Enabled Then Return "Stimulate item not possible now!" & vbCrLf & vbCrLf & "Possible Reasons:" & vbCrLf & vbCrLf & "- Not Connected" & vbCrLf & "- No item selected"
        If ItemList.ItemCount < 1 Then Return "Item list empty"
        If dgvItemList.SelectedCells.Count < 1 Then Return "No selection found"

        SetUIBusy()
        cmdItemSet.Enabled = False
        cmdItemBrowse.Enabled = False
        frmTurntable.StopTT4ATimer() 'avoid pulling brake during stimulation

        If szBackupItemListFileName Is Nothing Then szBackupItemListFileName = STIM.WorkDir & "\~" & gszSettingTitle & "_backup_" & System.DateTime.Now.ToString("yyyyMMdd_HHmmss") & "." & gszItemListExtension 'backup file name

        If glTriggerChannel > 0 And gblnUseTriggerChannel = True Then 'create trigger channel file?
            ' create trigger signal
            szErr = Output.CreateTriggerSignal()
            If Len(szErr) <> 0 Then Return szErr
        End If

        ' stimulate
        Dim Arr() As Integer = ItemList.SelectedItems
        For lRow As Integer = 0 To Arr.Length - 1
            'ItemList.ItemIndex = lRow
            'If gStimOutput = GENMODE.genVocoder Then Output.Send("/DAC/SetVol/*", 100) -> Uncomment this section, if you want to use ElectricVocoder
            szErr = ExpSuite.Events.OnStimulateSelected(Arr(lRow), lTO, szLeft, szRight)
            gblnStimulationDone = True
            If gblnCancel = True Then SetUIReady() : Return ""
            If Len(szErr) <> 0 Then SetUIReady() : Return szErr
            If gblnAutoBackupItemList Then STIM.BackupItemList(szBackupItemListFileName)
        Next

        ' ready!
        SetStatus("Stimulation finished successfully")
        frmTurntable.StartTT4ATimer() ' enable brake timer

        ' Play a jingle to finish off
        If ItemList.SelectedItemLast = ItemList.ItemCount Then
            Output.Send("/Play/OpenWAV/0", "open", "C:/Users/Admin/Documents/Code/expsuite-code/AMTatARI/Resources/Application/levelCleared.wav", 0, 44, 1, glResolution \ 8, "l")
            Output.Send("/DAC/SetStream/3", "set", "play0")
            Output.Send("/Play/SetDelay/0", 0.005)
            Output.Send("/DAC/SetVol/3", 80)
            Output.Send("/Play/StartAll/0")
            Sleep(5575)
            Output.Send("/DAC/SetVol*", 0)
            Output.Send("/Play/StartSynced/*", "stop")
            Output.Send("/Play/Stop/*")
        End If

        SetUIReady()
        Return ""

    End Function

    Private Function CalibrateOptitrack() As String

        Dim szX As String

        szX = Tracker.CalibrateOptitrack()

        If Len(szX) > 0 Then MsgBox(szX, MsgBoxStyle.Critical, "Show Turntable Interface")

        Return ""

    End Function

    Private Sub cmdTTShow_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdTTShow.Click

        Dim szX As String

        If glTTMode = 0 Then MsgBox("No turntable mode selected in options!", MsgBoxStyle.Critical, "Show Turntable Interface") : Exit Sub

        SetUIBusy()
        szX = Turntable.Show
        SetUIReady()

        If Len(szX) > 0 Then MsgBox(szX, MsgBoxStyle.Critical, "Show Turntable Interface")
    End Sub

    Private Sub dgvItemList_ColumnHeaderMouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles dgvItemList.ColumnHeaderMouseDoubleClick
        If ItemList.ItemCount < 1 Then Return
        For Each row As DataGridViewRow In dgvItemList.Rows
            row.Selected = False
            dgvItemList.Item(e.ColumnIndex, row.Index).Selected = True
        Next
    End Sub

    Private Sub dgvItemList_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles dgvItemList.Enter
        dgvItemList_SelChange(dgvItemList, New System.EventArgs())
    End Sub

    Private Sub dgvItemList_KeyDownEvent(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.KeyEventArgs) Handles dgvItemList.KeyDown
        
       
        Select Case eventArgs.KeyCode
            Case System.Windows.Forms.Keys.F2
                If txtSelItem.Enabled Then
                    txtSelItem.SelectionStart = 0
                    txtSelItem.SelectionLength = Len(txtSelItem.Text)
                    txtSelItem.Focus()
                End If
                'Case CType((System.Windows.Forms.Keys.ControlKey Or System.Windows.Forms.Keys.C), System.Windows.Forms.Keys) 'Ctrl+C
                '    'If eventArgs.Control Then
                '    ' CTRL+C: copy to clipboard
                '    mnuItemCopy_Click(mnuItemCopy, New System.EventArgs())
                '    'End If
            Case Keys.Delete
                mnuItemClearCells_Click(mnuItemClearCells, New System.EventArgs())
                        
            'case  Keys.v 
            '    if  eventArgs.Modifiers  = Keys.Control And Keys.Shift Then
            '        PasteFromClipboard
            '    End If

                'End If

'    If (e.KeyCode And Not Keys.Modifiers) = Keys.T AndAlso e.Modifiers = Keys.Ctrl Then
'    MessageBox.Show("Ctrl + T")
'End If
        End Select
    End Sub

    Private Sub dgvItemList_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles dgvItemList.Leave
        mnuItemEdit.Enabled = False
    End Sub

    Private Sub dgvItemList_MouseUpEvent(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.MouseEventArgs) Handles dgvItemList.MouseUp
        If eventArgs.Button = Windows.Forms.MouseButtons.Right Then ' right button?
            If dgvItemList.SelectedCells.Count > 0 Then
                popupMenuStrip.Show(dgvItemList, New System.Drawing.Point(eventArgs.X, eventArgs.Y))
            End If
        End If
    End Sub

    Private Sub ItemListSelectionChange
        If Not dgvItemList.Enabled Then
            lblSelItemNr.Text = ""
            lblSelItemLabel.Text = ""
            Return
        End If
        If Me.Cursor = Cursors.WaitCursor Then Return
        If dgvItemList.SelectedCells.Count < 1 Then Return

        Dim lRowBeg As Integer = dgvItemList.RowCount
        Dim lRowEnd As Integer = 0
        Dim lColBeg As Integer = dgvItemList.ColumnCount
        Dim lColEnd As Integer = 0
        For Each cell As DataGridViewCell In dgvItemList.SelectedCells
            If cell.RowIndex < lRowBeg Then lRowBeg = cell.RowIndex
            If cell.RowIndex > lRowEnd Then lRowEnd = cell.RowIndex
            If cell.ColumnIndex < lColBeg Then lColBeg = cell.ColumnIndex
            If cell.ColumnIndex > lColEnd Then lColEnd = cell.ColumnIndex
        Next

        If lRowBeg = lRowEnd Then
            ' one item selected
            lblSelItemNr.Text = "#" & TStr(lRowBeg + 1)
            lblSelItemLabel.Text = "Selected Item:"
            mnuItemDel.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
            mnuItemDel.Text = "Remove This Item"
            mnuItemInsert.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
            cmdItemInsert.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
            mnuItemDuplicateBlock.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
            mnuItemDuplicateBlock.Text = "Duplicate This Item"
            mnuItemShuffleBlock.Enabled = False
            If lColBeg = lColEnd Then
                lblSelColumn.Text = ItemList.ColCaption(lColBeg) & ":"
                ToolTip1.SetToolTip(dgvItemList, ItemList.ColCaption(lColBeg) & " (" & TStr(lRowBeg + 1) & "): " & ItemList.Item(lRowBeg, lColBeg))
                txtSelItem.Text = ItemList.Item(lRowBeg, lColBeg)
                cmdItemSet.Enabled = True
            Else
                lblSelColumn.Text = ""
                ToolTip1.SetToolTip(dgvItemList, "Item #" & TStr(lRowBeg + 1))
                txtSelItem.Text = ""
                cmdItemSet.Enabled = False
            End If
        Else
            ' many items selected
            Dim lRows(lRowEnd - lRowBeg) As Integer
            Dim lRowCount As Integer = 0        ' stores the correct number of items
            For Each cell As DataGridViewCell In dgvItemList.SelectedCells
                Dim Found As Boolean = False
                For Each lX As Integer In lRows
                    If cell.RowIndex + 1 = lX Then Found = True : Exit For
                Next
                If Not Found Then lRows(lRowCount) = cell.RowIndex + 1 : lRowCount += 1
            Next

            lblSelItemNr.Text = TStr(lRowCount) & " items, from #" & TStr(lRowBeg + 1) & " to #" & TStr(lRowEnd + 1)
            lblSelItemLabel.Text = "Selected Items:"
            mnuItemDel.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
            mnuItemDel.Text = "Remove Items"
            mnuItemInsert.Enabled = False 'And Not gblnRemoteClientConnected
            cmdItemInsert.Enabled = False 'And Not gblnRemoteClientConnected
            mnuItemDuplicateBlock.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
            mnuItemDuplicateBlock.Text = "Duplicate Items"
            mnuItemShuffleBlock.Enabled = Not gblnExperiment 'And Not gblnRemoteClientConnected
            If lColBeg = lColEnd Then
                lblSelColumn.Text = ItemList.ColCaption(lColBeg) & ":"
                ToolTip1.SetToolTip(dgvItemList, ItemList.ColCaption(lColBeg))
                txtSelItem.Text = ItemList.Item(lRowBeg, lColBeg)
                cmdItemSet.Enabled = True
            Else
                lblSelColumn.Text = ""
                ToolTip1.SetToolTip(dgvItemList, "")
                txtSelItem.Text = ""
                cmdItemSet.Enabled = False
            End If
        End If

        cmdItemBrowse.Enabled = cmdItemSet.Enabled And _
                ((ItemList.ColFlag(lColBeg) And 15) = clsItemList.ItemListFlags.ifDirectory Or _
                 (ItemList.ColFlag(lColBeg) And 15) = clsItemList.ItemListFlags.ifFileName)
        TextBoxState(txtSelItem, cmdItemSet.Enabled)
        mnuItemEdit.Enabled = True
        'SetStatus(dgvItemList.Item(lColBeg, lRowBeg).ValueType.ToString)
    End Sub
    Private Sub dgvItemList_SelChange(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles dgvItemList.SelectionChanged

        ItemListSelectionChange
    End Sub

    'Private Sub frmMain_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragEnter
    '    If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
    '        e.Effect = DragDropEffects.All      ' allow Drag and Drop effect
    '    Else
    '        e.Effect = DragDropEffects.None
    '    End If
    'End Sub

    Private Sub frmMain_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragDrop

        Dim szFile As String = Nothing
        Dim szFolder As String = Nothing

        On Error Resume Next

        Dim oDroppedData As DataObject = CType(e.Data, DataObject)
        'Dim sFileList As System.Text.StringBuilder = New System.Text.StringBuilder()

        For Each sFileName As String In oDroppedData.GetFileDropList()
            szFolder = System.IO.Path.GetDirectoryName(sFileName)
            szFile = System.IO.Path.GetFileName(sFileName)
        Next

        If szFolder <> Nothing And szFile <> Nothing Then

            SetUIBusy()
            If Strings.Right(szFile, 4) = ".csv" Then ' ITEM LIST
                Dim szTitle As String
                Dim szErr As String

                If ItemList.ItemCount > 0 Then
                    If MsgBox("The list contains not saved items." & vbCrLf & "Are you sure to load a new list?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then Return
                End If

                SetStatus("Load Item List...")
                Dim start_time As DateTime = Now
                szErr = ItemList.Load(szFolder & "\" & szFile, pbStatus)
                If Len(szErr) > 0 Then
                    MsgBox(szErr)
                    SetUIReady()
                Else
                    gszItemListTitle = szFile
                    gblnSettingsChanged = True
                    SetUIReady()
                    gblnItemListShuffled = False
                    gblnItemListRepeated = False
                    FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piNothing
                    ItemList.SetOptimalColWidth()
                    SetStatus("Item List loaded ("& Strings.Replace(Now.Subtract(start_time).TotalSeconds.ToString("0.0"),",",".") & " s)")
                    ServeData(FWintern.ServeDataEnum.ItemlistColCountListStatus)
                    ServeData(FWintern.ServeDataEnum.ChangeListStatus)
                    szBackupItemListFileName= Nothing 'new file name for item list backup next time
                End If
                lblSelItemNr.Text = ""
                lblSelItemLabel.Text = ""

            Else ' SETTING
                If gblnSettingsChanged Then
                    If MsgBox("Some settings were changed. If you load new settings, all changes will be discarded." & vbCrLf & "Continue anyway?", MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2 Or MsgBoxStyle.Exclamation, "Load settings") = MsgBoxResult.No Then Exit Sub
                End If
                If ItemList.ItemCount > 0 Then
                    If MsgBox("Loading new settings the item list will be cleared." & vbCrLf & "Proceed anyway?", MsgBoxStyle.Critical Or MsgBoxStyle.YesNo, "Load Settings") = MsgBoxResult.No Then Exit Sub
                End If
                LoadSettings(szFolder & "\" & szFile)
            End If

            gszCurrentDir = szFolder
        End If

        SetUIReady()

    End Sub

    Private Sub lblRootDir_DoubleClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles lblRootDir.DoubleClick
        Shell("explorer " & Chr(34) & ToolTip1.GetToolTip(lblRootDir), AppWinStyle.NormalFocus)
    End Sub

    Private Sub lblWorkDir_DoubleClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles lblWorkDir.DoubleClick
        If lblWorkDir.Text <> "not available yet..." Then
            Shell("explorer " & Chr(34) & STIM.WorkDir & Chr(34), AppWinStyle.NormalFocus)
        End If
    End Sub

    Private Sub lstStatus_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles lstStatus.SelectedIndexChanged
        If Me.IsInitializing = True Then
            Exit Sub
        Else
            If lstStatus.SelectedIndex > -1 Then ToolTip1.SetToolTip(lstStatus, VB6.GetItemString(lstStatus, lstStatus.SelectedIndex))
        End If
    End Sub

    Public Sub mnuBackupLogFileAs_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuBackupLogFileAs.Click
        Dim szErr, szFile As String
        Dim MyResult As System.Windows.Forms.DialogResult

        If gblnAutoBackupLogFileSilent = True Then 'silent backup of log file
            Dim bScanFn As Boolean = False
            Dim lX As Integer = 1

            Dim szLogFile As String = gszCurrentDir  & "\"

            If InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) > 0 Then
                szLogFile &=  Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) - 1)       
            Else
                szLogFile &= gszSettingTitle
            End If

            If My.Computer.FileSystem.FileExists(szLogFile & ".log.csv") Then
                Do While bScanFn = False
                lX += 1
                    
                If Not My.Computer.FileSystem.FileExists(szLogFile & " (" & TStr(lX) & ").log.csv") Then
                        szErr = STIM.BackupLogFile(szLogFile & " (" & TStr(lX) & ").log.csv")
                        bScanFn = True
                        End If
                Loop
            Else
                szErr = STIM.BackupLogFile(szLogFile & ".log.csv")
            End If


        Else
            Dim dlgSave As New SaveFileDialog With { _
                .InitialDirectory = gszCurrentDir, _
                .Filter = "Log files (*.log.csv)|*.log.csv|CSV-Files (*.csv)|*.csv|All Files (*.*)|*.*", _
                .DefaultExt = "log.csv", _
                .FilterIndex = 1, _
                .Title = "Backup Current Log File As...", _
                .SupportMultiDottedExtensions = True _
            }
            If gblnUseFileNaming Then
                    If InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) > 0 Then
                        szFile = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) - 1)
                    ElseIf InStrRev(gszSettingTitle, ".esf") > 0 Then
                        szFile = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, ".esf") - 1)
                    Else
                        szFile = gszSettingTitle
                    End If
                    GetNextFileVersion(szFile, ".log.csv")
                    dlgSave.FileName = szFile
                Else
                    dlgSave.FileName = ""
                End If

                If dlgSave.ShowDialog() = Windows.Forms.DialogResult.OK Then
                    On Error GoTo 0
                    If Len(dlgSave.FileName) <> 0 Then
                        szErr = STIM.BackupLogFile(dlgSave.FileName)
                        If Len(szErr) <> 0 Then
                            MsgBox("Error: " & szErr)
                        Else
                            gszCurrentDir = Mid(dlgSave.FileName, 1, Len(dlgSave.FileName) - Len((New System.IO.FileInfo(dlgSave.FileName)).Name) - 1)
                        End If
                    End If
                End If
        End If

      
    End Sub


    Public Sub mnuItemAppend_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemAppend.Click

        'SetUIBusy()
        ' get file name
        '.MaxFileSize = 8192
        Dim dlgOpen As New OpenFileDialog With { _
            .InitialDirectory = gszCurrentDir, _
            .Title = "Append Item List", _
            .FileName = "", _
            .CheckFileExists = True, _
            .CheckPathExists = True, _
            .Multiselect = True _
        }

        If gszItemListExtension <> "itl.csv" Then
            dlgOpen.Filter = "Item List (*." & gszItemListExtension & ")|*." & gszItemListExtension & "|ITL.CSV Item List (*.itl.csv)|*.itl.csv|CSV Item List (*.csv)|*.csv|All Files (*.*)|*.*"
            dlgOpen.DefaultExt = gszItemListExtension
        Else
            dlgOpen.Filter = "Item List (*.itl.csv)|*.itl.csv|CSV Item List (*.csv)|*.csv|All Files (*.*)|*.*"
            dlgOpen.DefaultExt = "*.itl.csv"
        End If

        dlgOpen.FilterIndex = 1
        dlgOpen.SupportMultiDottedExtensions = True
        If dlgOpen.ShowDialog() = Windows.Forms.DialogResult.Cancel Then Return
        gszCurrentDir = (New System.IO.FileInfo(dlgOpen.FileName)).DirectoryName
        If dlgOpen.FileNames.Length > 1 Then
            Dim szFN As String = ""
            For Each szFile As String In dlgOpen.FileNames
                szFN = szFN & vbCrLf & (New System.IO.FileInfo(szFile).Name)
            Next
            MsgBox(szFN, , "Following files will be appended (in this order)")
        End If

        UndoSnapshot()
        Dim szErr As String = ""
        Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
        dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
        dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        'Dim startTime As Date = System.DateTime.Now
        For Each szFile As String In dlgOpen.FileNames
            Me.Refresh()
            szErr = ItemList.Append(szFile, pbStatus)
            If Len(szErr) > 0 Then Exit For
            SetStatus("Item list appended: " & (New System.IO.FileInfo(szFile).Name))
            Me.Refresh()
        Next
        If Len(szErr) > 0 Then
            MsgBox(szErr, , "Error appending files")
        End If
        ItemList.SetOptimalColWidth()
        'Debug.Print(DateDiff(DateInterval.Second, startTime, System.DateTime.Now).ToString & " sec")

        gszItemListTitle = (New System.IO.FileInfo(dlgOpen.FileName)).Name
        dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
        dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)
        dgvItemList.CurrentCell = dgvItemList.Rows(dgvItemList.RowCount - 1).Cells(0)
        dgvItemList.Rows(dgvItemList.RowCount - 1).Selected = True

        dgvItemList.AutoResizeColumns()
        SetUIReady()
        gblnItemListShuffled = False
        gblnItemListRepeated = False
        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piNothing
        ServeData(FWintern.ServeDataEnum.ItemlistColCountListStatus)

    End Sub


    Public Sub mnuItemClearCells_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemClearCells.Click

        UndoSnapshot()

        For Each X As DataGridViewCell In dgvItemList.SelectedCells
            ItemList.Item(X.RowIndex, X.ColumnIndex) = ""
        Next

        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin

        ServeData(ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        SetUIReady()

    End Sub

    Public Sub mnuItemClearList_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemClearList.Click
        ' security check
        If MsgBox("All changes in the item list will be lost! (no undo)" & vbCrLf & "Do you want to continue?", MsgBoxStyle.Exclamation Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton1) = MsgBoxResult.No Then
            Exit Sub
        End If

        'SetUIBusy
        ItemList.Clear()
        If mnuItemEdit.Enabled Then
            For Each ctrX As Control In Controls
                If (TypeOf ctrX Is Button) AndAlso ctrX.Enabled AndAlso ctrX.Visible Then ctrX.Focus() : Exit For
            Next ctrX
        End If
        lblSelItemNr.Text = ""
        lblSelItemLabel.Text = ""
        gblnFirstExperiment = True
        gszItemListTitle = ""
        gblnSettingsChanged = True
        SetStatus("Item list cleared")
        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        SetUIReady()

    End Sub

    Public Delegate Sub RemoteItemClearListDelegate()

    Public Sub RemoteItemClearList()

        'SetUIBusy
        ItemList.Clear()
        If mnuItemEdit.Enabled Then
            For Each ctrX As Control In Controls
                If (TypeOf ctrX Is Button) AndAlso ctrX.Enabled AndAlso ctrX.Visible Then ctrX.Focus() : Exit For
            Next ctrX
        End If
        lblSelItemNr.Text = ""
        lblSelItemLabel.Text = ""
        gblnFirstExperiment = True
        gszItemListTitle = ""
        gblnSettingsChanged = True
        SetStatus("Item list cleared")
        SetUIReady()

    End Sub

    Public Sub mnuItemCopy_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemCopy.Click
        Console.WriteLine(dgvItemList.GetCellCount(DataGridViewElementStates.Selected))
        If txtSelItem.Focused = True Then
            Try ' text edit window focused
                Clipboard.SetDataObject(txtSelItem.SelectedText)
            Catch szX As System.Runtime.InteropServices.ExternalException
                MsgBox("Copy to clipboard error", MsgBoxStyle.Critical, szX)
            End Try
        ElseIf dgvItemList.GetCellCount(DataGridViewElementStates.Selected) = 1 Then
            Try ' one cell in item list selected
                Me.SetStatus("Copy from item list to clipboard...")
                If IsNothing(dgvItemList.CurrentCell.Value) OrElse dgvItemList.CurrentCell.Value.ToString = "" Then
                    Clipboard.SetDataObject(dgvItemList.GetClipboardContent) ' empty cell
                Else
                    Clipboard.SetDataObject(dgvItemList.CurrentCell.Value) ' content in cell
                End If
            Catch szX As System.Runtime.InteropServices.ExternalException
                MsgBox("Copy to clipboard error", MsgBoxStyle.Critical, szX)
            End Try
        ElseIf dgvItemList.GetCellCount(DataGridViewElementStates.Selected) > 1 Then

            With dgvItemList
                If gblnIncludeHeadersInClipboard Then
                    Me.SetStatus("Copy from item list (headers included) to clipboard...")
                    .ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText 'with header
                Else
                    Me.SetStatus("Copy from item list to clipboard...")
                    .ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText 'no header
                End If
            End With

            Try ' more cells in item list selcted
                Clipboard.SetDataObject(dgvItemList.GetClipboardContent)
            Catch szX As System.Runtime.InteropServices.ExternalException
                MsgBox("Copy to clipboard error", MsgBoxStyle.Critical, szX)
            End Try
        End If
    End Sub

    Private Sub PasteFromClipboard
        Dim arT() As String
        Dim i, ii As Integer

        'Console.Writeline(Me.ActiveControl.ToString)
        If Me.ActiveControl IsNot dgvItemList Then Exit Sub
        If dgvItemList.GetCellCount(DataGridViewElementStates.Selected) <> 1 Then
            Me.SetStatus("Paste from clipboard to item list only possible while one cell is selected!")
            Exit Sub
        End If

        Dim tArr() As String = Clipboard.GetText().Trim().Split(CType(Environment.NewLine, Char()))  '!!! Added Trim() ...because when pasting from vb App
        Dim tArr2() As String = Clipboard.GetText().Split(CType(Environment.NewLine, Char()))

        tArr = tArr2
        dim row As Integer = dgvItemList.CurrentCellAddress.Y()     'this is easier
        dim col As Integer = dgvItemList.CurrentCellAddress.X()

        ''add rows? nope
        'If (tArr.Length  > (dgvItemList.Rows.Count - row)) Then dgvItemList.Rows.Add(tArr.Length  - (dgvItemList.Rows.Count - row)) 'check length of the clipboard and the datagridview
        If dgvItemList.Rows.Count = 0 Then Exit Sub 'no item list available

        UndoSnapshot()
        Me.SetStatus("Paste from clipboard to item list...")

        For i = 0 To tArr.Length - 1 'treat row by row
            'tArr(i)=Trim(tArr(i))
            If tArr(i) <> "" Then
                arT = tArr(i).Split(CType(vbTab, Char())) 'split row
                dim curcol As Integer = col
                For ii = 0 To arT.Length - 1 'treat column by column (within one row)
                    If curcol > dgvItemList.ColumnCount - 1 Then Me.SetStatus("Column " & tstr(curcol) & " exceeding number of item list columns! Content not pasted entirely! Press 'Ctrl+Z' to undo (and rethink) changes...") : Exit For 'cell column (exceeding?)
                    If row > dgvItemList.Rows.Count - 1 Then Me.SetStatus("Row " & tstr(row + 1) & " exceeding number of item list rows! Content was not pasted entirely! Press 'Ctrl+Z' to undo (and rethink) changes...") : GoTo SubEnd 'cell row (exceeding?)
                    With dgvItemList.Item(curcol, row)
                        .Value = arT(ii).TrimStart
                    End With
                    curcol += 1
                Next
                row += 1
            End If
        Next
        ItemListSelectionChange
SubEnd:
        Me.SetStatus("Pasting from clipboard to item list done.")

    End Sub


    Public Sub mnuItemPaste_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemPaste.Click
        PasteFromClipboard
        '        Dim arT() As String
        '        Dim i, ii As Integer

        '        Console.Writeline(Me.ActiveControl.ToString)
        '        If Me.ActiveControl IsNot dgvItemList Then Exit Sub

        '        Dim tArr() As String = Clipboard.GetText().Trim().Split(CType(Environment.NewLine, Char()))  '!!! Added Trim() ...because when pasting from vb App
        '        Dim tArr2() As String = Clipboard.GetText().Split(CType(Environment.NewLine, Char()))

        '        tArr=tArr2
        '        dim row As Integer = dgvItemList.CurrentCellAddress.Y()     'this is easier
        '        dim col As Integer = dgvItemList.CurrentCellAddress.X()     

        '        ''add rows? nope
        '        'If (tArr.Length  > (dgvItemList.Rows.Count - row)) Then dgvItemList.Rows.Add(tArr.Length  - (dgvItemList.Rows.Count - row)) 'check length of the clipboard and the datagridview
        '        If dgvItemList.Rows.Count = 0 Then Exit Sub 'no item list available

        '        UndoSnapshot()
        '        Me.SetStatus("Paste from clipboard...")

        '        For i = 0 To tArr.Length - 1 'treat row by row
        '            'tArr(i)=Trim(tArr(i))
        '            If tArr(i) <> "" Then
        '                arT = tArr(i).Split(CType(vbTab, Char())) 'split row
        '                dim curcol As Integer= col
        '                For ii = 0 To arT.Length - 1 'treat column by column (within one row)
        '                    If curcol > dgvItemList.ColumnCount - 1 Then Me.SetStatus("Column " & tstr(curcol) & " exceeding number of item list columns! Content not pasted entirely! Press 'Ctrl+Z' to undo (and rethink) changes...") : Exit For 'cell column (exceeding?)
        '                    If row > dgvItemList.Rows.Count - 1 Then Me.SetStatus("Row " & tstr(row+1) & " exceeding number of item list rows! Content was not pasted entirely! Press 'Ctrl+Z' to undo (and rethink) changes...") :  GoTo SubEnd 'cell row (exceeding?)
        '                    With dgvItemList.Item(curcol, row)
        '                        .Value = arT(ii).TrimStart
        '                    End With
        '                    curcol += 1
        '                Next
        '                row += 1
        '            End If
        '        Next

        'SubEnd:
        '        Me.SetStatus("Pasting from clipboard to item list done.")

    End Sub



    Public Sub mnuItemDel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemDel.Click
        cmdItemDel_Click(cmdItemDel, New System.EventArgs())
    End Sub

    Public Sub mnuItemDuplicateBlock_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemDuplicateBlock.Click
        'glRemoteClickCounter += 1

        'If gblnRemoteDublicate Then
        '    gblnRemoteDublicate = False
        '    For i As Integer = 1 To glRemoteClickCounter
        '        SetUIBusy()
        '        If dgvItemList.SelectedCells.Count < 1 Then SetUIReady() : Return
        '        UndoSnapshot()
        '        Dim Arr() As Integer = ItemList.SelectedItems
        '        If Arr Is Nothing Then Return
        '        Dim lMax As Integer = Arr(Arr.Length - 1)
        '        Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
        '        dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        '        Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
        '        dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        '        With dgvItemList
        '            For lX As Integer = 0 To Arr.Length - 1
        '                .Rows.Insert(lMax + 1 + lX, .Rows(Arr(lX)).Clone)
        '                CopyDataGridRow(.Rows(Arr(lX)), .Rows(lMax + 1 + lX))
        '            Next
        '        End With
        '        dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
        '        dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)
        '        SetUIReady()
        '        If glRemoteClickCounter = 1 Then glRemoteClickCounter -= 1
        '        If i > 1 And i = glRemoteClickCounter Then glRemoteClickCounter = 0
        '        If i = glRemoteClickCounter Then ServeData("Dublicate")
        '    Next
        '    gblnRemoteDublicate = True
        'End If

        glRemoteClickCounter += 1

        If Not gblnRemoteClicked Then
            gblnRemoteClicked = True
            While glRemoteClickCounter > 0
                Dim endfor As Integer = glRemoteClickCounter
                For i As Integer = 1 To endfor
                    SetUIBusy()
                    If dgvItemList.SelectedCells.Count < 1 Then SetUIReady() : Return
                    UndoSnapshot()
                    Dim Arr() As Integer = ItemList.SelectedItems
                    If Arr Is Nothing Then Return
                    Dim lMax As Integer = Arr(Arr.Length - 1)
                    Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
                    dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
                    Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
                    dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
                    With dgvItemList
                        For lX As Integer = 0 To Arr.Length - 1
                            .Rows.Insert(lMax + 1 + lX, .Rows(Arr(lX)).Clone)
                            CopyDataGridRow(.Rows(Arr(lX)), .Rows(lMax + 1 + lX))
                        Next
                    End With
                    dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
                    dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)
                    If i = endfor Then
                        glRemoteClickCounter -= endfor
                        ServeData(FWintern.ServeDataEnum.Itemlist)
                        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
                    End If
                Next
            End While
            gblnRemoteClicked = False
            SetUIReady()
        End If
    End Sub

    Public Sub mnuItemInsert_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemInsert.Click
        cmdItemInsert_Click(cmdItemInsert, New System.EventArgs())
    End Sub

    Public Sub mnuItemLoadList_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemLoadList.Click
        Dim szFile, szTitle As String
        Dim szErr As String

        If ItemList.ItemCount > 0 Then
            If MsgBox("The list contains not saved items." & vbCrLf & "Are you sure to load a new list?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then Return
        End If
        ' get file name
        Dim dlgOpen As New OpenFileDialog With { _
            .InitialDirectory = gszCurrentDir, _
            .Title = "Load Item List", _
            .FileName = "", _
            .CheckFileExists = True, _
            .CheckPathExists = True _
        }

        If gszItemListExtension <> "itl.csv" Then
            dlgOpen.Filter = "Item List (*." & gszItemListExtension & ")|*." & gszItemListExtension & "|ITL.CSV Item List (*.itl.csv)|*.itl.csv|CSV Item List (*.csv)|*.csv|All Files (*.*)|*.*"
            dlgOpen.DefaultExt = gszItemListExtension
        Else
            dlgOpen.Filter = "Item List (*.itl.csv)|*.itl.csv|CSV Item List (*.csv)|*.csv|All Files (*.*)|*.*"
            dlgOpen.DefaultExt = "itl.csv"
        End If

        dlgOpen.FilterIndex = 1
        dlgOpen.SupportMultiDottedExtensions = True
        If dlgOpen.ShowDialog() <> Windows.Forms.DialogResult.OK Then Return
        szFile = dlgOpen.FileName
        szTitle = (New System.IO.FileInfo(dlgOpen.FileName)).Name
        gszCurrentDir = Mid(dlgOpen.FileName, 1, Len(dlgOpen.FileName) - Len(szTitle) - 1)

        SetUIBusy()
        SetStatus("Load Item List...")
        Dim start_time As DateTime = Now
        szErr = ItemList.Load(szFile, pbStatus)
        If Len(szErr) > 0 Then
            MsgBox(szErr)
            SetUIReady()
        Else
            gszItemListTitle = szTitle
            gblnSettingsChanged = True
            SetUIReady()
            gblnItemListShuffled = False
            gblnItemListRepeated = False
            FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piNothing
            ItemList.SetOptimalColWidth()
            'mnuOptColWidth_Click(mnuOptColWidth, New System.EventArgs())
            'ServeData("Load Itemlist")
            SetStatus("Item List loaded ("& Strings.Replace(Now.Subtract(start_time).TotalSeconds.ToString("0.0"),",",".") & " s)")
            ServeData(FWintern.ServeDataEnum.ItemlistColCountListStatus)
            ServeData(FWintern.ServeDataEnum.ChangeListStatus)
            szBackupItemListFileName= Nothing 'new file name for item list backup next time
        End If
        lblSelItemNr.Text = ""
        lblSelItemLabel.Text = ""

    End Sub

    Public Sub mnuItemSetExperimentBlock_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemSetExperimentBlock.Click

        If ItemList.SelectedItemFirst = 0 And ItemList.SelectedItemLast = ItemList.ItemCount - 1 Then
            glExperimentStartItem = -1
            glExperimentEndItem = -1
        Else
            glExperimentStartItem = ItemList.SelectedItemFirst
            glExperimentEndItem = ItemList.SelectedItemLast
        End If

        SetUIReady()

    End Sub

    Public Sub mnuQuickSave_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuQuickSave.Click
        Dim szSet, szList As String
        Dim szErr As String

        Dim inpX As New frmInputMultiline
        If gblnSettingsChanged Then
            inpX.Left = Me.Left
            inpX.Top = Me.Top
            inpX.Text_Renamed = gszDescription
            inpX.Title = "Some settings have been changed." & vbCrLf & vbCrLf & "Update the description of the settings before saving:"
            If Not inpX.ShowForm("Save Settings") Then Return
            gszDescription = inpX.Text_Renamed
        End If

        gszFileName = InputBox("Input the name of the settings file and the item list (without extensions): ", "Quick Save: Save Item List and Settings to the work directory", mszQuickSaveFN)
        If Len(gszFileName) = 0 Then Exit Sub

        mszQuickSaveFN = gszFileName

        szSet = STIM.WorkDir & "\" & gszFileName & "." & My.Application.Info.AssemblyName
        szList = STIM.WorkDir & "\" & gszFileName & "." & gszItemListExtension

        If Len(Dir(szSet)) > 0 Then
            If MsgBox("Settings file " & szSet & " exists." & vbCrLf & "Do you want to overwrite it?", MsgBoxStyle.Critical Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then Return
            Kill(szSet)
            If Len(Dir(szSet)) > 0 Then MsgBox("File could not be deleted.") : Return
        End If

        If Len(Dir(szList)) > 0 Then
            If MsgBox("Item list " & szList & " exists." & vbCrLf & "Do you want to overwrite it?", MsgBoxStyle.Critical Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then Return
            Kill(szList)
            If Len(Dir(szList)) > 0 Then MsgBox("File could not be deleted.") : Return
        End If

        ' save item list
        SetUIBusy()
        'Dim csvX As New CSVParser
        'szErr = csvX.WriteDGV(szList, dgvItemList, pbStatus)
        szErr = ItemList.Save(szList, pbStatus)
        If Len(szErr) <> 0 Then
            If szErr = "System Error: Das Argument Length muss gr��er als oder gleich 0 (null) sein." Then szErr = "Empty item lists cannot be saved!"
            MsgBox(szErr, MsgBoxStyle.Critical, "Save Item List")
        Else
            gszItemListTitle = gszFileName & "." & gszItemListExtension
            gblnSettingsChanged = True

            ' save settings
            INISettings.WriteFile(szSet)
            gszSettingTitle = gszFileName
            gszSettingFileName = szSet
            gblnSettingsChanged = False
        End If
        SetUIReady()

    End Sub

    Public Sub mnuRemoteMonitor_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuRemoteMonitor.Click

    End Sub


    Public Sub mnuItemRenumber_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemRenumber.Click
        If ItemList.ItemCount < 1 Then Return

        UndoSnapshot()

        ' linear renumbering
        For lX As Integer = 0 To ItemList.ItemCount - 1
            ItemList.Item(lX, ItemList.GetIndexCol) = TStr(lX + 1)
        Next
        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
        ServeData(FWintern.ServeDataEnum.Renumber)
    End Sub

    Public Sub RemoteItemRenumber()
        If ItemList.ItemCount < 1 Then Return

        ' linear renumbering
        For lX As Integer = 0 To ItemList.ItemCount - 1
            ItemList.Item(lX, ItemList.GetIndexCol) = TStr(lX + 1)
        Next
        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
    End Sub

    Public Sub mnuItemSaveListAs_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs, Optional NoUI As Boolean = False) Handles mnuItemSaveListAs.Click

        If ItemList.ItemCount = 0 Then
            MsgBox("Thou shalt not save an empty item list!", MsgBoxStyle.Exclamation, "Save Item List")
            Exit Sub
        End If
        Dim szFile, szTitle As String

        If NoUI = False Then
            ' get file name
            If gblnUseFileNaming Then
                frmItemListPostfix.ShowDialog()
                If gblnCancel Then gblnCancel = False : Return
            Else
                gszFileName = ""
            End If
            ' open "save as..." dialog
            Using dlgSave As New SaveFileDialog With { _
                .FileName = gszFileName, _
                .InitialDirectory = gszCurrentDir, _
                .Title = "Save Item List As...", _
                .SupportMultiDottedExtensions = True _
            }
                If gszItemListExtension <> "itl.csv" Then
                    dlgSave.Filter = "Item List (*." & gszItemListExtension & ")|*." & gszItemListExtension & "|ITL.CSV Item List (*.itl.csv)|*.itl.csv|CSV Item List (*.csv)|*.csv|All Files (*.*)|*.*"
                    dlgSave.DefaultExt = gszItemListExtension
                Else
                    dlgSave.Filter = "Item List (*.itl.csv)|*.itl.csv|CSV Item List (*.csv)|*.csv|All Files (*.*)|*.*"
                    dlgSave.DefaultExt = "itl.csv"
                End If
                dlgSave.FilterIndex = 1
                dlgSave.OverwritePrompt = True
                If dlgSave.ShowDialog() <> Windows.Forms.DialogResult.OK Then Return

                szFile = dlgSave.FileName
            End Using
            While InStr(szFile, ".itl.itl") > 0 'remove all the sekanten .itl.itl
                szFile = Strings.Replace(szFile, ".itl.itl", ".itl")
            End While
            szTitle = (New System.IO.FileInfo(szFile)).Name       ' get the file name without folder
        Else

            szTitle = frmItemListPostfix.CreateFileNameWrapper()
            szFile = szTitle & "." & gszItemListExtension & ""
            If My.Computer.FileSystem.FileExists(szFile) Then BackupFile(szFile)

        End If

        SetUIBusy()

        Dim szErr As String = ItemList.Save(szFile, pbStatus)
        If Len(szErr) <> 0 Then
            MsgBox(szErr, MsgBoxStyle.Critical, "Save Item List")
        Else
            gszItemListTitle = szTitle
            gblnSettingsChanged = True
        End If

        SetUIReady()
    End Sub

    Private Sub AutoSaveResponseList()
        Dim dlgSave As New SaveFileDialog With { _
            .FileName = gszFileName, _
            .InitialDirectory = gszCurrentDir, _
            .Title = "Save Item List As..." _
        }
        If gszItemListExtension <> "itl.csv" Then
            dlgSave.Filter = "Item List (*." & gszItemListExtension & ")|*." & gszItemListExtension & "|ITL.CSV Item List (*.itl.csv)|*.itl.csv|CSV Item List (*.csv)|*.csv|All Files (*.*)|*.*"
            dlgSave.DefaultExt = gszItemListExtension
        Else
            dlgSave.Filter = "Item List (*.itl.csv)|*.itl.csv|CSV Item List (*.csv)|*.csv|All Files (*.*)|*.*"
            dlgSave.DefaultExt = "itl.csv"
        End If

        dlgSave.FilterIndex = 1
        dlgSave.OverwritePrompt = True
        dlgSave.SupportMultiDottedExtensions = True
        If dlgSave.ShowDialog() <> Windows.Forms.DialogResult.OK Then Return

        Dim szFile As String = dlgSave.FileName
        Dim szTitle As String = (New System.IO.FileInfo(dlgSave.FileName)).Name       ' get the file name without extension

        SetUIBusy()

        Dim szErr As String = ItemList.Save(szFile, pbStatus)
        If Len(szErr) <> 0 Then
            MsgBox(szErr, MsgBoxStyle.Critical, "Save Item List")
        Else
            gszItemListTitle = szTitle
            gblnSettingsChanged = True
        End If

        SetUIReady()
    End Sub


    Public Sub mnuItemShuffleBlock_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemShuffleBlock.Click
        UndoSnapshot()
        SetUIBusy()
        ItemList.ShuffleItems(ItemList.SelectedItems)
        gblnItemListShuffled = True
        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piBegin
        SetUIReady()
    End Sub


    ' ------------------------------------------------------------------
    ' EVENTS - Menus
    ' ------------------------------------------------------------------


    Public Sub mnuHelpShortcuts_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuHelpShortcuts.Click
        Dim szLine As String
        szLine = "General:" & vbCrLf & "F4" & vbTab & "Snapshot: log used parameters" & vbCrLf & "F5" & vbTab & "Connect" & vbCrLf & "F6" & vbTab & "Stimulate selected item" & vbCrLf & "F7" & vbTab & "Start experiment" & vbCrLf & "F8" & vbTab & "Show the settings window" & vbCrLf & "F9" & vbTab & "Show/hide the stimulation log list" & vbCrLf & "CTRL+F7" & vbTab & "Show/don't show the stimulus before stimulation" & vbCrLf & "CTRL+N" & vbTab & "New settings" & vbCrLf & "CTRL+O" & vbTab & "Load settings" & vbCrLf & "CTRL+S" & vbTab & "Save settings (ask for the name before saving)" & vbCrLf & "CTRL+L" & vbTab & "Load item list" & vbCrLf & "CTRL+W" & vbTab & "Save item list" & vbCrLf & "CTRL+J" & vbTab & "Append an item list to the existing one" & vbCrLf & "CTRL+Q" & vbTab & "Quick Save: save settings and item list in the work directory" & vbCrLf & "CTRL+T" & vbTab & "Show subject's request text" & vbCrLf & vbCrLf & vbCrLf
        szLine = szLine & "Settings:" & vbCrLf & "CTRL+S" & vbTab & "Save settings and close window" & vbCrLf & vbCrLf & vbCrLf
        szLine = szLine & "Item list editing:" & vbCrLf & "CTRL+Z" & vbTab & "Undo changes in the item list" & vbCrLf & "CTRL+I" & vbTab & "Insert an item before selection" & vbCrLf & "CTRL+D" & vbTab & "Duplicate selected items" & vbCrLf & "CTRL+R" & vbTab & "Remove selected items" & vbCrLf & "Del   " & vbTab & "Clear the content of selected cells" & vbCrLf & "CTRL+C" & vbTab & "Copy selected item(s) to the clipboard"  & vbCrLf & "CTRL+Shift+V" & vbTab & "Paste to item list" & vbCrLf & "CTRL+A" & vbTab & "Select the item list content" & vbCrLf & "F2" & vbTab & "Edit a cell of the item list" & vbCrLf & "F3" & vbTab & "Set focus to the item list while editing" & vbCrLf & vbCrLf & vbCrLf
        szLine = szLine & "Experiment screen:" & vbCrLf & "ESC/F1" & vbTab & "Experiment running: Cancel experiment after confirmation" & vbCrLf & "ESC/F1" & vbTab & "Experiment not running: Hide the experiment screen" & vbCrLf & "B" & vbTab & "Experiment running: Initiate break during the experiment" & vbCrLf & "PAUSE" & vbTab & "Experiment break: Continue Experiment" & vbCrLf
        MsgBox(szLine, MsgBoxStyle.Information, My.Application.Info.AssemblyName & " Shortcuts")
    End Sub

    Public Sub mnuItemUndo_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuItemUndo.Click
        cmdItemUndo_Click(cmdItemUndo, New System.EventArgs())
    End Sub

    Public Sub mnuOptColWidth_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuOptColWidth.Click
        Dim blnDisableSetOptimalColWidthTemp As Boolean = gblnDisableSetOptimalColWidth ' Copy current status
        gblnDisableSetOptimalColWidth = False ' Enable automatic setting of column width
        ItemList.SetOptimalColWidth() ' Set column width
        gblnDisableSetOptimalColWidth = blnDisableSetOptimalColWidthTemp ' Set global variable back to old status
    End Sub

    Public Sub mnuSnapshot_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuSnapshot.Click
        Dim szX, szErr As String

        SetUIBusy()
        szX = InputBox("Input a title of snapshot" & vbCrLf & "(or leave it empty to cancel):", "Snapshot", "Snapshot")
        If Len(szX) = 0 Then
            MsgBox("Snapshot canceled")
            GoTo SubEnd
        End If
        szErr = SnapShot(szX)
        If Len(szErr) <> 0 Then
            MsgBox(szErr, MsgBoxStyle.Critical, "Snapshot")
        End If
SubEnd:
        SetUIReady()

    End Sub

    Public Sub mnuLevelDancer_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuLevelDancer.Click
        If gblnOutputStable = False Then MsgBox("Connect first...", MsgBoxStyle.Information) : Return
        If F4FL.ImpType = Implant.IMPLANTTYPE.imptInvalid And F4FR.ImpType = Implant.IMPLANTTYPE.imptInvalid Then
            MsgBox("Define implants for both ears...", MsgBoxStyle.Information)
            Return
        End If
        If IsNothing(gfreqParL) Then MsgBox("No signals for the left ear") : Return
        If IsNothing(gfreqParR) Then MsgBox("No signals for the right ear") : Return
        frmLevelDancer.ShowDialog()
        frmLevelDancer.Dispose()
    End Sub

    Public Sub mnuStartExp_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuStartExp.Click
        Dim szErr As String
        szErr = StartExperiment()
        If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Start Experiment")
    End Sub

    Private Sub cmdStartExp_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdStartExp.Click
        'ServeData("Start Exp")
        Dim szErr As String
        szErr = StartExperiment()
        If InStr(szErr, "RIB2", CompareMethod.Text) <> 0 Then szErr = "RIB2 Error!" & vbCrLf & "Please check if RIB2 files are available!" & vbCrLf & vbCrLf & szErr
        If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Start Experiment")
    End Sub

    ''' <summary>
    ''' Starts an experiment.
    ''' </summary>
    ''' <param name="lFlags"> Main Automatisations Flags (mafIgnoreOptionWarnings, mafIgnoreHUIWarnings)</param>
    ''' <returns>Error message or empty if no error ocured.</returns>
    ''' <remarks>This public function can be used to start an experiment without using the button or menu.</remarks>
    Public Function StartExperiment(Optional ByVal lFlags As AutomatisationFlags = 0) As String
        Dim szErr As String = ""
        Dim szArr(0) As String
        Dim lArr(0) As Integer

        ctxtmnuItemInsert.Enabled = False
        ctxtmnuItemDuplicateBlock.Enabled = False
        ctxtmnuItemDel.Enabled = False
        ctxtmnuFillAutomatically.Enabled = False
        ctxtmnuItemSetExperimentBlock.Enabled = False

        If mnuStartExp.Enabled = False Then
            szErr = "Start Experiment not possible now!" & vbCrLf & vbCrLf & _
                    "Possible Reasons:" & vbCrLf & vbCrLf & _
                    "- Not Connected" & vbCrLf & _
                    "- No Item List loaded/created"
            GoTo SubEnd
        End If

        If (lFlags And FWintern.AutomatisationFlags.IgnoreOptionWarnings) = 0 Then 'Option Warnings disabled
            ' first experiment?
            If Not gblnFirstExperiment And ((glWarningSwitches And WarningSwitches.wsResponseItemListOnExpRep) <> 0) Then
                If MsgBox("If you start another experiment without disconnecting," & vbCrLf & _
                    "all results will be written to the same log file." & vbCrLf & _
                    "Do you want to start the experiment?", _
                    MsgBoxStyle.Critical Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then
                    GoTo SubEnd
                End If
            End If

            'number of items in output directory
            Dim fileCount As Integer = IO.Directory.GetFiles(STIM.WorkDir, "*.*").Length
            If fileCount > 2500 Then
                If MsgBox("Your working directory contains already " & fileCount.ToString & "(!) files. This could slow down your system. It is recommended to disconnect and connect to a new directory!" & vbCrLf & vbCrLf & _
                        "Do you want to start the experiment anyway?", _
                        MsgBoxStyle.Critical Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then
                    GoTo SubEnd
                End If
            End If

            If Not gblnItemListShuffled And ((glWarningSwitches And WarningSwitches.wsNotShuffledOnExpStart) <> 0) Then
                If MsgBox("The item list has not been shuffled." & vbCrLf & _
                    "Do you want to continue anyway?", _
                    MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then
                    GoTo SubEnd
                End If
            End If
            If Not gblnItemListRepeated And ((glWarningSwitches And WarningSwitches.wsNotRepOnExpStart) <> 0) Then
                If MsgBox("The item repetition has not been applied to the item list." & vbCrLf & _
                    "Do you want to continue anyway?", _
                    MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then
                    GoTo SubEnd
                End If
            End If
            gblnFirstExperiment = False
        End If

        ' get the first and last item
        If glExperimentStartItem > -1 And glExperimentEndItem > -1 Then
            If (glExperimentStartItem >= ItemList.ItemCount) Or (glExperimentEndItem >= ItemList.ItemCount) Then
                szErr = "Can not start the experiment: The experiment range is not valid."
                GoTo SubEnd
            End If
            mlFirstItemOfExp = glExperimentStartItem
            mlLastItemOfExp = glExperimentEndItem
            If (lFlags And AutomatisationFlags.StartAtSelectedItem) <> 0 Then
                If ItemList.ItemIndex < glExperimentStartItem Or ItemList.ItemIndex > glExperimentEndItem Then
                    szErr = "Can not start the experiment: The selected item is outside of the experiment range." & vbCrLf & _
                            "Clear the experiment range or select an item within the range."
                    GoTo SubEnd
                End If
                mlFirstItemOfExp = ItemList.ItemIndex
            End If
        Else
            mlFirstItemOfExp = 0
            mlLastItemOfExp = ItemList.ItemCount - 1
            If (lFlags And AutomatisationFlags.StartAtSelectedItem) <> 0 Then mlFirstItemOfExp = ItemList.ItemIndex
        End If
        ' set the items to fresh if not-continue mode (ignored ignored)
        If (lFlags And AutomatisationFlags.ContinueExperiment) = 0 Then ' new experiment -> reset all items 
            For lX As Integer = mlFirstItemOfExp To mlLastItemOfExp
                If Not (ItemList.ItemStatus(lX) = clsItemList.Status.Ignored) Then ItemList.ItemStatus(lX) = clsItemList.Status.Fresh
            Next
        End If
        ' get the first fresh/processing item
        For mlFirstItemOfExp = mlFirstItemOfExp To mlLastItemOfExp
            If ItemList.ItemStatus(mlFirstItemOfExp) = clsItemList.Status.Fresh Or _
                   ItemList.ItemStatus(mlFirstItemOfExp) = clsItemList.Status.Processing Then Exit For
        Next
        If mlFirstItemOfExp > mlLastItemOfExp Then Return "No fresh item found."
        ' get the last fresh/processing item
        For mlLastItemOfExp = mlLastItemOfExp To mlFirstItemOfExp Step -1
            If ItemList.ItemStatus(mlLastItemOfExp) = clsItemList.Status.Fresh Or _
                   ItemList.ItemStatus(mlLastItemOfExp) = clsItemList.Status.Processing Then Exit For
        Next
        ItemList.ItemIndex = mlFirstItemOfExp
        mlFirstItemAfterBreak = mlFirstItemOfExp
        ' get the item count for this experiment
        mlItemCountOfExp = 0
        For lX As Integer = mlFirstItemOfExp To mlLastItemOfExp
            If ItemList.ItemStatus(lX) = clsItemList.Status.Fresh Or _
               ItemList.ItemStatus(lX) = clsItemList.Status.Processing Then mlItemCountOfExp += 1
        Next

        ' init general data
        UndoDisable()
        SetUIBusy()
        gblnCancel = False
        SetProgressbar(0)
        SetOnBreakCallback(Nothing)
        sbStatusBar.Items.Item(STB_REMAININGTIME).Text = "N/A"
        QueryPerformanceCounter(gcurHPTic) ' reset time/break mechanism
        chkExpRun.CheckState = CheckState.Checked ' unset break mode
        ' show experiment screen
        frmExp.Dispose() 'reset form
        ExpSuite.Events.OnExpShow(glExpType, grectExp, INISettings.glExpFlags)
        gblnExperiment = True
        ' set HUI
        If glExpHUIID > 0 Then
            szErr = HUI.GetDevicesList(szArr, lArr)
            If Len(szErr) <> 0 Then GoTo SubEnd
            If (lFlags And FWintern.AutomatisationFlags.IgnoreHUIWarnings) = 0 Then 'HUI Warnigs disabled
                If GetUbound(szArr) + 1 < glExpHUIID Then
                    MsgBox("The Human Interface (HUI) device you choosed is not available." & vbCrLf & "No HUI support in the experiment.", MsgBoxStyle.Information)
                Else
                    szErr = frmExp.SetHUIDevice(glExpHUIID)
                    If Len(szErr) <> 0 Then GoTo SubEnd
                End If
            End If
        Else
            szErr = frmExp.SetHUIDevice(0)
            If Len(szErr) <> 0 Then GoTo SubEnd
        End If

        frmTurntable.StopTT4ATimer() 'avoid pulling brake during stimulation

        Randomize()
        SetUIReady()

        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        ServeData(FWintern.ServeDataEnum.StartExp)

        ' create automatical snapshot
        szErr = SnapShot("Auto snapshot on experiment start")
        If Len(szErr) <> 0 Then GoTo SubEnd
        ' start logging the item list
        STIM.Log("**********", "Begin of Experiment", System.DateTime.Now.ToString("HH:mm:ss"))
        ReDim szArr(ItemList.ColCount - 1)
        For lX As Integer = 0 To ItemList.ColCount - 1
            szArr(lX) = ItemList.ColCaption(lX)
        Next
        STIM.Log(szArr)

        If glTriggerChannel > 0 And gblnUseTriggerChannel = True Then 'create trigger channel file?
            ' create trigger signal
            szErr = Output.CreateTriggerSignal()
            If Len(szErr) <> 0 Then GoTo SubEnd
        End If

        szBackupItemListFileName = STIM.WorkDir & "\~" & gszSettingTitle & "_backup_" & System.DateTime.Now.ToString("yyyyMMdd_HHmmss") & "." & gszItemListExtension 'backup file name
     
        szErr = ExpSuite.Events.OnStartExperiment(ItemList.ItemIndex)
        'ClipCursorToWindow(0)
        If Len(szErr) <> 0 Then
            ' error or cancel found
            frmExp.ShowBlankScreen(gblnExpOnTop)
            If CBool(INISettings.glExpFlags And frmExp.EXPFLAGS.expflClipMouseToWindow) Then frmExp.UnloadMe()
            STIM.Log(szErr)
            'If glFlagBeepExp > 0 Then BeepOnError()
            'If gblnPlayWaveExp = True Then PlayWaveOnError()
            ServeData(FWintern.ServeDataEnum.ErrorInExperiment)
        Else
            frmExp.ShowEndScreen()
            If gblnPlayWaveExp = True Then PlayWaveOnEnd()
            'If glFlagBeepExp > 0 Then BeepOnEnd()
            ServeData(FWintern.ServeDataEnum.EndOfExperiment)
            Me.SetProgressbar(100)
            frmExp.SetProgress(100)
            If gblnAutoBackupItemList Then
                STIM.BackupItemList(szBackupItemListFileName)
                szBackupItemListFileName= Nothing 'new file name next time
            End If
            
        End If
        gblnStimulationDone = True
        STIM.Log("**********", "End of Experiment", System.DateTime.Now.ToString("HH:mm:ss"))
        ' clean up...
        gblnExperiment = False
        FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piResponse
        frmExp.DisableResponse()

        '' Pull brake of turntable (check if TT was used happens in function)
        'Turntable.PullBrake()
        frmTurntable.StartTT4ATimer() 'enable brake timer

        STIM.Log("")
        STIM.Log("")

SubEnd:
        gblnExperiment = False
        SetUIReady()
        ClipCursorToWindow(0)
        Me.Focus()
        Return szErr

    End Function

    ''' <summary>
    ''' Wrapper to get the first item of experiment range.
    ''' </summary>
    ''' <returns>First item of experiment range.</returns>
    ''' <remarks></remarks>
    Public Function GetFirstItemOfExp() As Integer
        Return mlFirstItemOfExp
    End Function

    ''' <summary>
    ''' Wrapper to get the last item of experiment range.
    ''' </summary>
    ''' <returns>Last item of experiment range.</returns>
    ''' <remarks></remarks>
    Public Function GetLastItemOfExp() As Integer
        Return mlLastItemOfExp
    End Function

    Public Sub mnuStartExpAtItem_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuStartExpAtItem.Click

        ' show selected item
        ItemList.ItemIndex = ItemList.ItemIndex

        If MsgBox("Do you want to start the experiment at the selected item?", _
           MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2, _
           "Start Experiment") = MsgBoxResult.No Then Exit Sub

        Dim szErr As String
        szErr = StartExperiment(AutomatisationFlags.StartAtSelectedItem)
        If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Start Experiment at Selected Item")

    End Sub

    Public Sub mnuViewStimulus_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuViewStimulus.Click

        If Not gblnShowStimulus Then
            tbButtonShowStimulus.Checked = True
            If (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB) Or (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB2) Or (INISettings.gStimOutput = STIM.GENMODE.genVocoder) Then
                gblnShowStimulus = True
                glShowStimulusFlags = 0
            Else
                frmShowStimulus.ShowDialog()
            End If
        Else
            gblnShowStimulus = False
        End If
        If gblnShowStimulus Then
            If Len(gszShowStimulusParameter) > 0 Then
                STIM.ShowStimulusFlags = TStr(glShowStimulusFlags) & ",[" & gszShowStimulusAxes & "]," & gszShowStimulusParameter
            Else
                STIM.ShowStimulusFlags = TStr(glShowStimulusFlags) & ",[" & gszShowStimulusAxes & "]"
            End If
        End If
        If gblnShowStimulus Then mnuViewStimulus.CheckState = CheckState.Checked Else mnuViewStimulus.CheckState = CheckState.Unchecked
        If gblnShowStimulus Then tbButtonShowStimulus.CheckState = CheckState.Checked Else tbButtonShowStimulus.CheckState = CheckState.Unchecked
    End Sub

    Public Sub mnuFileExit_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuFileExit.Click
        Me.Close()
    End Sub

    Public Sub mnuFileNew_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuFileNew.Click
        If gblnSettingsChanged Then
            If MsgBox("Some settings were changed. If you proceed, all changes will be discarded." & vbCrLf & "Continue anyway?", MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2 Or MsgBoxStyle.Exclamation, "New setting") = MsgBoxResult.No Then Exit Sub
        End If

        ClearParameters()
        If Not IsNothing(glOnSettingsExpTypeChangeAddr) Then glOnSettingsExpTypeChangeAddr(-1, glExpType)
        If Not IsNothing(glOnSettingsSetAddr) Then glOnSettingsSetAddr()
        gblnSettingsChanged = True
        SetUIReady()
    End Sub

    Private Function CheckSignalConsistency(ByVal freqX() As clsFREQUENCY, ByVal szFile As String) As String
        Dim lX As Integer
        Dim szErr As String
        Dim F4FT As New Implant

        F4FT.ClearParameters()
        If Len(szFile) = 0 Then szErr = "No fitting file available" : GoTo SubErr
        szErr = F4FT.OpenFile(szFile)
        If Len(szErr) <> 0 Then szErr = "Error reading fitting file:" & szFile & vbCrLf & szErr : GoTo SubErr
        If F4FT.ImpType = Implant.IMPLANTTYPE.imptInvalid Then szErr = "Invalid implant type found" : GoTo SubErr

        If F4FT.ChannelsCount <> GetUbound(freqX) + 1 Then
            szErr = "Number of electrodes differ (F=" & TStr((F4FT.ChannelsCount)) & "; S=" & TStr(GetUbound(gfreqParL) + 1) & ")"
            GoTo SubErr
        End If

        For lX = 0 To F4FT.ChannelsCount - 1
            With F4FT.Channel(lX)
                If .lRange <> freqX(lX).lRange Then szErr = szErr & "Electrode #" & TStr(lX + 1) & ": Range not equal (F=" & TStr(.lRange) & "; S=" & TStr((freqX(lX).lRange)) & ")" & vbCrLf
                If .lMCL <> freqX(lX).sMCL Then szErr = szErr & "Electrode #" & TStr(lX + 1) & ": MCL not equal (F=" & TStr(.lMCL) & "; S=" & TStr((freqX(lX).sMCL)) & ")" & vbCrLf
                If .lTHR <> freqX(lX).sTHR Then szErr = szErr & "Electrode #" & TStr(lX + 1) & ": THR not equal (F=" & TStr(.lTHR) & "; S=" & TStr((freqX(lX).sTHR)) & ")" & vbCrLf
                If .lPhDur > freqX(lX).lPhDur Then szErr = szErr & "Electrode #" & TStr(lX + 1) & ": Phase duration in fitting file (=" & TStr(.lPhDur) & ") is higher than in the settings (=" & TStr((freqX(lX).lPhDur)) & ")" & vbCrLf
            End With
        Next

SubErr:
        If Len(szErr) > 0 Then
            szErr = "Lack of consistency between fitting file (F) and loaded settings (S):" & vbCrLf & vbCrLf & szErr & vbCrLf & vbCrLf & "Your options are: " & vbCrLf & "- Open Settings. The settings parameters will be automatically adapted to the fitting files. Click OK to confirm the adaptations." & vbCrLf & "- Do nothing. The parameters found in the setting file will be used in the experiment and cause problems, errors and much more..." & vbCrLf & "- Adapt the fitting files to be in consistency with settings and load this settings file again."
        End If
        Return szErr

    End Function


    Public Sub LoadSettings(ByVal szFile As String, Optional silentlist As Boolean = False)
        Dim szErr As String
        Dim szKey As String
        Dim lX, lY As Integer
        Dim szArr() As String = Nothing

        ' check the type of file
        szKey = "application title"
        szFile = (Trim(szFile).ToString)
        szErr = INISettings.FindParameter(szFile, szKey)
        If Len(szErr) > 0 Then GoTo Error_Load
        If Len(szKey) = 0 Then
            szKey = "experiment title" ' required for backw. compatibility to FW<0.8
            szErr = INISettings.FindParameter(szFile, szKey)
            If Len(szErr) > 0 Then GoTo Error_Load
        End If
        If Len(szKey) = 0 Then
            MsgBox("No application title found in this settings file." & vbCrLf & _
                   "Please check if file is a valid " & My.Application.Info.AssemblyName & " settings file!", MsgBoxStyle.Critical, "Loading error")
            Return
        End If
        If szKey <> My.Application.Info.AssemblyName Then
            If MsgBox("This file is a settings file for " & szKey & " and not for " & My.Application.Info.AssemblyName & "." & vbCrLf & "If you continue loading all settings from this file they will overwrite some of your current settings." & vbCrLf & "All other settings will remain." & vbCrLf & "Variables will be appended causing more variables entries than now." & vbCrLf & vbCrLf & "Do you want to load these settings file anyway?", MsgBoxStyle.Critical Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then GoTo Error_CancelClick
            ' read the experiment type
            szKey = "experiment type"
            szErr = INISettings.FindParameter(szFile, szKey)
            If szErr <> "" Then GoTo Error_Load
            If Not IsNothing(glOnSettingsExpTypeChangeAddr) Then glOnSettingsExpTypeChangeAddr(-1, CInt(Val(szKey)))
        Else
            ' check if settings were saved with a newer application version
            szKey = "application version"
            szErr = INISettings.FindParameter(szFile, szKey)
            szArr = Split(szKey, ".")
            If szArr.Length >= 2 AndAlso _
                (CInt(Val(szArr(0))) > My.Application.Info.Version.Major Or _
                CInt(Val(szArr(0))) = My.Application.Info.Version.Major And CInt(Val(szArr(1))) > My.Application.Info.Version.Minor Or _
                CInt(Val(szArr(0))) = My.Application.Info.Version.Major And CInt(Val(szArr(1))) = My.Application.Info.Version.Minor And CInt(Val(szArr(2))) > My.Application.Info.Version.Build) Then
                MsgBox("Settings file was created with a newer version of " & My.Application.Info.AssemblyName & "! Parameters are loaded but be aware that the functionality of your application may be limited! " & _
                       "Please consider updating you application!" & vbCrLf & vbCrLf & _
                       "Application version:" & vbTab & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Build & vbCrLf & _
                       "Settings version:" & vbTab & szArr(0) & "." & szArr(1) & "." & szArr(2), MsgBoxStyle.Exclamation, "Load Settings")
            End If
            'gszAPP_VERSION = My.Application.Info.Version.Major & "," & My.Application.Info.Version.Minor & "," & My.Application.Info.Version.Build & "," & My.Application.Info.Version.Revision

            ' read the experiment type
            szKey = "experiment type"
            szErr = INISettings.FindParameter(szFile, szKey)
            If szErr <> "" Then GoTo Error_Load
            If Not IsNothing(glOnSettingsExpTypeChangeAddr) Then glOnSettingsExpTypeChangeAddr(-1, CInt(Val(szKey)))

            szKey = "stimuli output"
            szErr = INISettings.FindParameter(szFile, szKey)
            If szErr <> "" Then GoTo Error_Load
            If Not IsNothing(glOnOutputDeviceChangeAddr) Then glOnOutputDeviceChangeAddr(gStimOutput, CType(CInt(Val(szKey)), GENMODE))

            ClearParameters() ' clear only on loading
            gblnSettingsChanged = False
        End If
        ' read new file
        gblnIsDotNETSetting = False ' assume a VB6-settings version
        ddTemp = New clsDataDirectory
        ddTemp.Reset()
        szErr = INISettings.ReadFile(szFile)
        If Len(szErr) > 0 Then GoTo Error_Load
        ' copy data directory data
        lY = ddTemp.Count
        If ddTemp.Count > DataDirectory.Count Then lY = DataDirectory.Count
        For lX = 0 To lY - 1
            If Len(ddTemp.Title(lX)) > 0 Then DataDirectory.Title(lX) = ddTemp.Title(lX)
            DataDirectory.Path(lX) = ddTemp.Path(lX)
        Next
        ' display new parameters
        gszSettingTitle = szFile
        lX = InStrRev(szFile, "\")
        If lX > 0 Then
            gszCurrentDir = Mid(szFile, 1, lX - 1)
        Else
            gszCurrentDir = ""
        End If
        ' get the new title and file name
        If InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) > 0 Then
            gszSettingTitle = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) - 1)
        ElseIf InStrRev(gszSettingTitle, ".esf") > 0 Then
            gszSettingTitle = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, ".esf") - 1)
        End If
        gszSettingFileName = szFile
        If InStrRev(gszSettingTitle, "\") > 0 Then gszSettingTitle = Mid(gszSettingTitle, InStrRev(gszSettingTitle, "\") + 1)
        If gblnIsDotNETSetting Then
            If szArr.Length >= 2 Then
                SetStatus(My.Application.Info.AssemblyName & " (v" & szArr(0) & "." & szArr(1) & "." & szArr(2) & ") Settings: " & gszSettingTitle)
            Else
                SetStatus(My.Application.Info.AssemblyName & " Settings: " & gszSettingTitle)
            End If
            If gszExpRequestText(glExpType) <> "" Then SetStatus("Subject's Request Text: " & gszExpRequestText(glExpType))
        Else
            SetStatus("VB6: " & My.Application.Info.AssemblyName & " Settings: " & gszSettingTitle)
        End If

        ' check if settings are consistent with fitting files in electrical mode
        If (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB) Or (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB2) Or (INISettings.gStimOutput = STIM.GENMODE.genVocoder) Then
            szKey = gszSourceDir
            If Len(gszFittFileLeft) <> 0 Then
                szErr = CheckSignalConsistency(gfreqParL, szKey & "\" & gszFittFileLeft)
                If Len(szErr) > 0 Then
                    MsgBox(szErr, MsgBoxStyle.Critical, "Loading settings for the left channel:")
                Else
                    szErr = F4FL.OpenFile(gszSourceDir & "\" & gszFittFileLeft)
                    If Len(szErr) <> 0 Then F4FL.ClearParameters()
                End If

            End If
            If Len(gszFittFileRight) <> 0 Then
                szErr = CheckSignalConsistency(gfreqParR, szKey & "\" & gszFittFileRight)
                If Len(szErr) > 0 Then
                    MsgBox(szErr, MsgBoxStyle.Critical, "Loading settings for the right channel:")
                Else
                    szErr = F4FR.OpenFile(gszSourceDir & "\" & gszFittFileRight)
                    If Len(szErr) <> 0 Then F4FR.ClearParameters()
                End If
            End If
        End If
        ' notice EventsSettings
        If Not IsNothing(glOnSettingsSetAddr) Then glOnSettingsSetAddr()

        ' item list found - load item list
        If Len(gszItemListTitle) > 0 AndAlso File.Exists(gszCurrentDir & "\" & gszItemListTitle) Then
            If silentlist = False AndAlso MsgBox("Item List " & vbCrLf & "     " & gszCurrentDir & "\" & gszItemListTitle & vbCrLf & " has been linked with these settings." & vbCrLf & vbCrLf & " Do you want this item list to be loaded now?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                gszItemListTitle = ""
                gblnSettingsChanged = True
            Else
                SetStatus("Load Item List...")
                Dim start_time As DateTime = Now
                szErr = ItemList.Load(gszCurrentDir & "\" & gszItemListTitle, pbStatus)
                If Len(szErr) > 0 Then
                    gszItemListTitle = ""
                    MsgBox(szErr)
                    gblnSettingsChanged = True
                Else
                    gszItemListTitle = gszItemListTitle
                    gblnSettingsChanged = False
                    gblnItemListShuffled = False
                    gblnItemListRepeated = False
                    FWintern.piItemListPostfixIndex = FWintern.ItemListPostfixIndex.piNothing
                    ItemList.SetOptimalColWidth()
                    SetStatus("Item List loaded ("& Strings.Replace(Now.Subtract(start_time).TotalSeconds.ToString("0.0"),",",".") & " s)")
                    szBackupItemListFileName= Nothing 'new file name for item list backup next time
                End If
            End If
        Else
            gszItemListTitle = ""
            gblnSettingsChanged = True
        End If

EndSub:
        SetUIReady()
        If gblnSettingsLoaded2 Then gblnSettingsLoaded1 = True
        If gblnSettingsLoaded1 Then
            ServeData(FWintern.ServeDataEnum.SendSettings, 0, 0, "")
            For i As Integer = 1 To glClientCount
                If clients(i) And gblnClientsSetting(i) Then
                    ServeData(FWintern.ServeDataEnum.ChangeSettings1, 0, 0, "", i)
                End If
            Next
            ServeData(FWintern.ServeDataEnum.ItemlistColCountListStatus, 0, 0, "")
        End If
        Return

Error_Load:
        MsgBox("Can't load settings:" & vbCrLf & szErr, MsgBoxStyle.Critical, "Load settings")
Error_CancelClick:

    End Sub

    Public Sub mnuFileLoad_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuFileLoad.Click
        Dim szFile As String

        If gblnSettingsChanged Then
            If MsgBox("Some settings were changed. If you load new settings, all changes will be discarded." & vbCrLf & "Continue anyway?", MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2 Or MsgBoxStyle.Exclamation, "Load settings") = MsgBoxResult.No Then Exit Sub
        End If

        If ItemList.ItemCount > 0 Then
            If MsgBox("Loading new settings the item list will be cleared." & vbCrLf & "Proceed anyway?", MsgBoxStyle.Critical Or MsgBoxStyle.YesNo, "Load Settings") = MsgBoxResult.No Then Exit Sub
        End If

        Dim dlgOpen As New OpenFileDialog With { _
            .Title = "Load Settings", _
            .InitialDirectory = gszCurrentDir, _
            .FileName = "", _
            .CheckFileExists = True, _
            .CheckPathExists = True, _
            .Filter = My.Application.Info.AssemblyName & " Setting Files (*." & My.Application.Info.AssemblyName & ")|*." & My.Application.Info.AssemblyName & "|" & "Any OLD ExpSuite Setting File (*.esf)|*.esf|All Files (*.*)|*.*", _
            .FilterIndex = 1, _
            .DefaultExt = "*." & My.Application.Info.AssemblyName, _
            .SupportMultiDottedExtensions = True _
        }
        If dlgOpen.ShowDialog() = Windows.Forms.DialogResult.OK Then
            szFile = dlgOpen.FileName
            gszCurrentDir = Mid(dlgOpen.FileName, 1, Len(dlgOpen.FileName) - Len((New System.IO.FileInfo(dlgOpen.FileName)).Name) - 1)
            LoadSettings(szFile)
            'save options (path)
            INIOptions.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\" & My.Application.Info.Title & "\" & My.Application.Info.Title & ".ini")
        End If

    End Sub

    Public Sub mnuFileSaveAs_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuFileSaveAs.Click
        Dim szFile As String
        Dim szErr As String

        Dim inpX As New frmInputMultiline
        If gblnSettingsChanged Then
            inpX.Left = Me.Left + CInt(Me.Width / 10)
            inpX.Top = Me.Top + CInt(Me.Height / 10)
            inpX.Text_Renamed = gszDescription
            inpX.Title = "Some settings have been changed." & vbCrLf & vbCrLf & "Update the description of the settings before saving:"
            If Not inpX.ShowForm("Save Settings") Then
                inpX = Nothing : Exit Sub   ' cancel
            End If
            gszDescription = inpX.Text_Renamed
            inpX = Nothing
        End If

        Dim dlgSave As New SaveFileDialog With { _
            .InitialDirectory = gszCurrentDir, _
            .FileName = gszSettingTitle, _
            .Title = "Save Settings", _
            .Filter = My.Application.Info.AssemblyName & " Setting File (*." & My.Application.Info.AssemblyName & ")|*." & My.Application.Info.AssemblyName & "|" & "Any ExpSuite Setting File (*.esf)|*.esf|All Files (*.*)|*.*", _
            .FilterIndex = 1, _
            .OverwritePrompt = True, _
            .SupportMultiDottedExtensions = True _
        }
        If dlgSave.ShowDialog() = Windows.Forms.DialogResult.OK Then
            szFile = dlgSave.FileName
            gszCurrentDir = Mid(dlgSave.FileName, 1, Len(dlgSave.FileName) - Len((New System.IO.FileInfo(dlgSave.FileName)).Name) - 1)

            ' save parameters
            szErr = INISettings.WriteFile(szFile)
            If Len(szErr) <> 0 Then
                MsgBox("Can't save file:" & vbCrLf & szErr, MsgBoxStyle.Critical, "Save file")
            Else
                'gszSettingTitle = (New System.IO.FileInfo(dlgSave.FileName)).Name
                gszSettingTitle = IO.Path.GetFileNameWithoutExtension(dlgSave.FileName)
                gszSettingFileName = dlgSave.FileName
                gblnSettingsChanged = False
            End If
            'save options (path)
            INIOptions.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\" & My.Application.Info.Title & "\" & My.Application.Info.Title & ".ini")

            SetUIReady()
        End If
    End Sub

    Public Sub mnuHelpAbout_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuHelpAbout.Click

        frmAbout.ShowDialog()
        frmAbout.Dispose()
    End Sub

    Public Sub mnuViewSettings_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuViewSettings.Click

        frmSettings.ShowDialog()
        frmSettings.Dispose()
        SetUIReady()
    End Sub

    Public Sub mnuSTIMLogList_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuSTIMLogList.Click
        mnuSTIMLogList.Checked = Not mnuSTIMLogList.Checked
        lstLog.Visible = mnuSTIMLogList.Checked
        lstStatus.Visible = Not mnuSTIMLogList.Checked
    End Sub

    Public Sub mnuConnectOutput_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuConnectOutput.Click
        Dim xxx As Boolean = False
        If gblnRemoteServerConnected Then gblnRemoteBlockIncommingMessages = True
        If gblnRemoteClientConnected Then
            xxx = True
            mnuRemoteMonitorDisconnect_Click(mnuRemoteMonitorDisconnect, New System.EventArgs())
        End If
        If mnuConnectOutput.Checked Then
            Disconnect()
        Else
            Connect()
        End If
        gblnRemoteConnectOutput = True
        If xxx Then
            mnuRemoteMonitorConnect_Click(mnuRemoteMonitorConnect, New System.EventArgs())
            mnuRemoteMonitorGetSettings.Enabled = True
            mnuRemoteMonitorGetItemlist.Enabled = True
            mnuRemoteMonitorDisconnect.Enabled = True
            mnuRemoteMonitorFollowCurrentItem.Enabled = True
        End If
        gblnRemoteBlockIncommingMessages = False
        gblnRemoteConnectOutput = False
    End Sub

    Public Sub mnuViewOptions_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuViewOptions.Click
        Dim szViwo As String = gszViWoAddress
        frmOptions.ShowDialog()
        frmOptions.Dispose()
        If Len(szViwo) > 0 And Len(gszViWoAddress) = 0 Then
            ViWo.Disconnect()   ' we were connected and now we do not viwo
            SetStatus("ViWo: disconnected")
        ElseIf Len(gszViWoAddress) <> 0 Then
            ViWo.Disconnect()   ' we need viwo - reconnect in any case
            SetStatus("ViWo: connecting to " & gszViWoAddress)
            Dim szX As String = ViWo.Connect(pbStatus)
            If Len(szX) > 0 Then
                MsgBox(szX, , "Error connecting to ViWo")
            Else
                SetStatus("ViWo: connected to " & ViWo.Version)
            End If
        End If
        SetUIReady()
    End Sub


    Private Sub tbToolBar_ButtonClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tbButtonNew.Click, tbButtonLoad.Click, tbButtonSaveAs.Click, tbButtonLoadItemList.Click, tbButtonSaveItemList.Click, tbButtonSettings.Click, tbButtonOptions.Click, tbButtonConnect.Click, tbButtonSnapshot.Click, tbButtonShowStimulus.Click
        Dim Button As System.Windows.Forms.ToolStripButton = CType(eventSender, System.Windows.Forms.ToolStripButton)
        On Error Resume Next
        Select Case Button.Name
            Case "tbButtonNew"
                mnuFileNew_Click(mnuFileNew, New System.EventArgs())
            Case "tbButtonLoad"
                mnuFileLoad_Click(mnuFileLoad, New System.EventArgs())
            Case "tbButtonSaveAs"
                mnuFileSaveAs_Click(mnuFileSaveAs, New System.EventArgs())
            Case "tbButtonSettings"
                mnuViewSettings_Click(mnuViewSettings, New System.EventArgs())
            Case "tbButtonOptions"
                mnuViewOptions_Click(mnuViewOptions, New System.EventArgs())
            Case "tbButtonConnect"
                mnuConnectOutput_Click(mnuConnectOutput, New System.EventArgs())
            Case "tbButtonShowStimulus"
                mnuViewStimulus_Click(mnuViewStimulus, New System.EventArgs())
            Case "tbButtonSnapshot"
                mnuSnapshot_Click(mnuSnapshot, New System.EventArgs())
            Case "tbButtonLoadItemList"
                mnuItemLoadList_Click(mnuItemLoadList, New System.EventArgs())
            Case "tbButtonSaveItemList"
                mnuItemSaveListAs_Click(mnuItemSaveListAs, New System.EventArgs())
        End Select
    End Sub

    ' ------------------------------------------------------------------
    ' EVENTS - Form
    ' ------------------------------------------------------------------

    Private Sub frmMain_KeyDown(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown
        Select Case eventArgs.KeyCode
            Case Keys.Escape
                If gblnExperiment Then
                    ' experiment pending - ask to be sure
                    If MsgBox("You're going to cancel the experiment." & vbCrLf & "Do you really want to cancel?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.Yes Then
                        gblnCancel = True
                        Exit Sub
                    End If
                Else
                    ' cancel immediatly
                    gblnCancel = True
                End If
            Case Keys.F3
                If dgvItemList.Enabled Then dgvItemList.Focus()
            Case Keys.T
                If My.Computer.Keyboard.CtrlKeyDown Then SetStatus("Subject's Request Text: " & gszExpRequestText(glExpType))
        End Select

    End Sub

    Private Sub frmMain_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        Dim szX As String
        Dim szCmd() As String
        Dim lX As Integer

        If InStr(1, My.Application.Info.AssemblyName, ".") > 0 Then Err.Raise(vbObjectError, "", "The experiment title is used for the extension of the setting files. Thus, it must not contain any dots (.)")
        'CreateAssociation(My.Application.Info.AssemblyName)
        Me.Icon = My.Resources.Icon
        QueryPerformanceFrequency(gcurHPFrequency)

        ' initialize F4F instances
        F4FL = New Implant
        F4FR = New Implant
        ' initialize item list
        DoubleBufferedDGV(dgvItemList, True) 'significant performance improvement (scrolling)
        ItemList = New clsItemList(dgvItemList)

        gszAPP_TITLE = My.Application.Info.Title
        gszAPP_VERSION = My.Application.Info.Version.Major & "," & My.Application.Info.Version.Minor & "," & My.Application.Info.Version.Build & "," & My.Application.Info.Version.Revision

        ''Check if Windows Media Player is installed and activated
        'If My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MediaPlayer\PlayerUpgrade", _
        '        "PlayerVersion", Nothing) Is Nothing Then
        '    MsgBox("Windows Media Player is not enabled or installed on this computer. It is required for ExpSuite FW v1.0 and newer!" & vbCrLf & vbCrLf & _
        '           "Please enable Windows Media Player in Windows Features:" & vbCrLf & vbCrLf & _
        '           "- Press ""Start"" button." & vbCrLf & _
        '           "- Open the control panel." & vbCrLf & _
        '           "- Launch the Programs and Features." & vbCrLf & _
        '           "- Click on the ""Turn Windows features on or off""." & vbCrLf & _
        '           "- Look for the ""Media Features"", and expand it." & vbCrLf & _
        '           "- Check the ""Windows Media Player""." & vbCrLf & _
        '           "- Press OK." & vbCrLf & vbCrLf & vbCrLf & _
        '           "Press OK to close " & gszAPP_TITLE & ".", _
        '        MsgBoxStyle.Critical, "Windows Media Player not found")
        '    End
        'Else
        '    Dim WMPversion As Object = My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MediaPlayer\PlayerUpgrade", "PlayerVersion", Nothing)
        '    Me.SetStatus("Windows Media Player version " & Trim(Replace(WMPversion.ToString, ",", ".")))
        'End If

        HistoryToolStripMenuItem.Text = "History of " & gszAPP_TITLE

        dgvUndo = New DataGridView
        DataDirectory = New clsDataDirectory
        ' initialize STIM
        STIM.Create()
        ' Initialize RIB
        RIB.Initialize()
        ' Initialize RIB2
        RIB2.Initialize()

        ' OPTIONS FILE ****
        ' read options
        INIOptions.ClearParameters()
        INIOptions.ReadFile(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\" & My.Application.Info.Title & "\" & My.Application.Info.Title & ".ini")
        frmOptions.UpdatePriority() 'set priority

        If ChangeDir(gszCurrentDir) Then
            gszCurrentDir = (Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\" & My.Application.Info.Title)
            ChangeDir(gszCurrentDir)
        End If

        ' RESOURCES DIRECTORY ****
        AppResourcesDirectory = My.Application.Info.DirectoryPath
        'remove possible backslash at ending
        If Strings.Right(AppResourcesDirectory, Len("\")) = "\" Then AppResourcesDirectory = Strings.Left(AppResourcesDirectory, Len(AppResourcesDirectory) - Len("\"))

        ' Set Application Resources folder (remove "\bin")
        If Strings.Right(AppResourcesDirectory, Len("\bin")) = "\bin" Then
            AppResourcesDirectory = Strings.Left(AppResourcesDirectory, Len(AppResourcesDirectory) - Len("\bin")) & "\Resources\Application"
        Else
            AppResourcesDirectory &= "\Resources\Application"
        End If

        ' If application resources directory not existing use application folder
        If Dir(AppResourcesDirectory, vbDirectory) = vbNullString Then
            AppResourcesDirectory = My.Application.Info.DirectoryPath
        End If
        'Console.WriteLine("Application Resources Directory: " & AppResourcesDirectory)
        Me.SetStatus("Resources Directory: " & AppResourcesDirectory)

        ' GLOBAL FW DIRECTORY ****
        If Directory.Exists(Mid(My.Application.Info.DirectoryPath, 1, InStrRev(My.Application.Info.DirectoryPath, "\")) & "_FW") Then
            FwGlobalDir = Mid(My.Application.Info.DirectoryPath, 1, InStrRev(My.Application.Info.DirectoryPath, "\")) & "_FW"
        End If

        ' set size
        'If grectMain.Width < Me.MinimumSize.Width Then grectMain.Width = Me.MinimumSize.Width
        grectMain.Width = Math.Max(grectMain.Width, Me.MinimumSize.Width)
        Me.Width = grectMain.Width
        'If grectMain.Height < Me.MinimumSize.Height Then grectMain.Height = Me.MinimumSize.Height
        grectMain.Height = Math.Max(grectMain.Height, Me.MinimumSize.Height)
        Me.Height = grectMain.Height

        ' set position
        Dim lFactor As Double = 0
        If grectMain.WindowState <> FormWindowState.Maximized Then lFactor = 0.25

        If grectMain.Left + lFactor * grectMain.Width > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width Then
            grectMain.Left = CInt(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - lFactor * grectMain.Width)
        End If
        If grectMain.Left + lFactor * 3 * grectMain.Width < 0 Then
            grectMain.Left = CInt(-lFactor * 3 * grectMain.Width)
        End If
        If grectMain.Top + lFactor * grectMain.Height > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height Then
            grectMain.Top = CInt(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - lFactor * grectMain.Height)
        End If
        'If grectMain.Top < 0 Then
        '    grectMain.Top = 0
        'End If
        grectMain.Top = Math.Max(grectMain.Top, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Top)

        Me.SetBounds(grectMain.Left, grectMain.Top, 0, 0, Windows.Forms.BoundsSpecified.X Or Windows.Forms.BoundsSpecified.Y)
        If grectMain.WindowState = FormWindowState.Maximized Then Me.WindowState = FormWindowState.Maximized 'do not load form minimized

        ' build controls
        lineToolbar.Width = Me.Width
        lstLog.Visible = False
        lstStatus.Visible = True

        ' result list: cmbresult
        cmbResult.Items.Add("Use frmMain.SetResultList")
        cmbResult.SelectedIndex = 0
        ' menus

        ' set volatile data
        gfreqDef(0) = New clsFREQUENCY        'create instance for both items in the array
        gfreqDef(1) = New clsFREQUENCY

        ' set user settings to default
        ClearParameters()

        ' setup user loading
        If Not gblnRemoteClientConnected Then
            ExpSuite.Events.OnLoad()
        End If
        
        'removed in .AddCol and placed here:
        ItemList.SetOptimalColWidth()

        For lX = 0 To GetUboundVariables()
            If (gvarExp(lX).Flags And FWintern.VariableFlags.vfFlagTypeMask) > FWintern.VariableFlags.vfElectrodeR Then
                Err.Raise(vbObjectError, "OnLoad", "Wrong type of the variable called " & gvarExp(lX).szName)
            End If
        Next
        If Not IsNothing(glOnSettingsExpTypeChangeAddr) Then glOnSettingsExpTypeChangeAddr(-1, 0)

        mnuRemoteMonitorUpdateSettings.Checked = True
        gblnRemoteMonitorUpdateSettings = True

        CheckForIllegalCrossThreadCalls = False
        ' Remote Monitor Server Enabled?
        If gblnRemoteMonitorServerEnabled Then RemoteMonitorServer.Init(Me, lstStatus, lblItemNr, _sbStatusBar_Panel0, _sbStatusBar_Panel5, _sbStatusBar_Panel3, _sbStatusBar_Panel4, dgvItemList, mnuRemoteMonitor, mnuRemoteMonitorDisconnectAllClients, mnuRemoteMonitorConnect, lblExpType)

        ' setup UI
        Me.AllowDrop = True             'Allows Dropping (for Drag and Drop)
        Me.Show()
        lblSelItemNr.Text = ""
        lblSelItemLabel.Text = ""
        SetUIReady()

        ' ViWo
        ViWo.Init()
        If Len(gszViWoAddress) > 0 Then
            SetUIBusy()
            SetStatus("ViWo: connecting to " & gszViWoAddress)
            szX = ViWo.Connect(pbStatus)
            If Len(szX) > 0 Then
                SetStatus("ViWo: " & szX)
            Else
                SetStatus("ViWo: connected to " & ViWo.Version)
            End If
            SetUIReady()
        End If

        gblnSettingsLoaded2 = True
        ' PopUpMenu for flgStatus
        ' share event handlers from menu items with context menu items
        AddHandler ctxtmnuItemUndo.Click, AddressOf mnuItemUndo_Click
        AddHandler ctxtmnuItemCopy.Click, AddressOf mnuItemCopy_Click
        'AddHandler ctxtmnuItemPaste.Click, AddressOf mnuItemPaste_Click
        AddHandler ctxtmnuItemClearCells.Click, AddressOf mnuItemClearCells_Click
        AddHandler ctxtmnuItemInsert.Click, AddressOf mnuItemInsert_Click
        AddHandler ctxtmnuItemDuplicateBlock.Click, AddressOf mnuItemDuplicateBlock_Click
        AddHandler ctxtmnuItemDel.Click, AddressOf mnuItemDel_Click
        AddHandler ctxtmnuItemShuffleBlock.Click, AddressOf mnuItemShuffleBlock_Click
        AddHandler ctxtmnuItemSetExperimentBlock.Click, AddressOf mnuItemSetExperimentBlock_Click
        AddHandler ctxtmnuFillAutomatically.Click, AddressOf mnuFillAutomatically_Click
        AddHandler ctxtmnuItemRenumber.Click, AddressOf mnuItemRenumber_Click
        AddHandler ctxtmnuOptColWidth.Click, AddressOf mnuOptColWidth_Click

        If gblnCheckForUpdates And gLastUpdateCheck <> "" Then
            Try
                Dim DaysSinceUpdateCheck As Long = DateDiff(DateInterval.Day, Date.ParseExact(gLastUpdateCheck, "yyyyMMdd", CultureInfo.InvariantCulture), _
                                        System.DateTime.Now, FirstDayOfWeek.Monday, FirstWeekOfYear.Jan1)
                If DaysSinceUpdateCheck >= CInt(gUpdateInterval) Then CheckForUpdates(True)
            Catch
                Console.WriteLine("Days since last update check could not be determined.")
            End Try
        End If

        ' get and evaluate the command line parameters
        If Not Proc.GetCommandLine Is Nothing Then 'flags?
            Dim szErr As String = ""
            Dim NoUI As Boolean = False
            szCmd = Proc.GetCommandLine(100)

            For Each Str As String In szCmd ' no UI??
                If LCase(Str).Equals("/startexperiment") Or LCase(Str).Equals("/s") Or LCase(Str).Equals("/c") Or _
                    LCase(Str).Equals("/connect") Or LCase(Str).Equals("/continue") Or LCase(Str).Equals("/continueexperiment") Then
                    'startexp = 1
                    NoUI = True
                    Exit For
                End If
            Next

            For lX = 0 To GetUbound(szCmd)

                If gblnCancel Then Exit For
                szX = szCmd(lX)
                Dim szXlc As String = LCase(szX)
                Select Case szXlc
                    Case "/c", "/connect"
                        ' connect
                        If Not (gblnOutputStable) Then Connect()
                    Case "/d", "/disconnect"
                        ' disconnect
                        If gblnOutputStable Then Disconnect()
                    Case "/startexperiment", "/s"
                        'start experiment
                        If Not (gblnOutputStable) Then Connect() 'connect
                        If Not (gblnCancel) Then StartExperiment(AutomatisationFlags.IgnoreOptionWarnings Or AutomatisationFlags.IgnoreHUIWarnings)
                    Case "/stimulateall"
                        If Not (gblnOutputStable) Then Connect() 'connect
                        cmdItemStimulateAll_Click(Nothing, Nothing)
                    Case "/continue", "/continueexperiment"
                        If Not (gblnOutputStable) Then Connect() 'connect
                        If Not (gblnCancel) Then StartExperiment(AutomatisationFlags.ContinueExperiment Or AutomatisationFlags.IgnoreOptionWarnings Or AutomatisationFlags.IgnoreHUIWarnings)
                    Case "/execute", "/e"
                        ' execute from results menu
                        If IsNumeric(szCmd(lX + 1)) AndAlso CInt(Val(szCmd(lX + 1))) > 0 AndAlso CInt(Val(szCmd(lX + 1))) <= Me.cmbResult.Items.Count Then
                            Me.SetStatus("Execute from results menu: " & Me.cmbResult.Items(CInt(Val(szCmd(lX + 1))) - 1).ToString)
                            If Not (gblnCancel) Then Result.Execute(CInt(Val(szCmd(lX + 1))) - 1)
                            lX += 1 'skip parameter in next loop step
                        Else
                            szErr = "'Execute' flag parameter not valid: " & szCmd(lX + 1)
                        End If
                    Case "/shuffle", "/shuffleitemlist"
                        If cmdItemShuffleList.Enabled Then Me.cmdItemShuffleList_Click(Nothing, Nothing)
                    Case "/saveitemlist"
                        ' save item list
                        Me.SetStatus("Save item list")
                        mnuItemSaveListAs_Click(mnuItemSaveListAs, New System.EventArgs(), NoUI)
                    Case "/createitemlist"
                        Me.SetStatus("Create item list")
                        cmdItemCreateList_Click(Nothing, Nothing)
                    Case "/addrepetition"
                        cmdItemAddRepetition_Click(Nothing, Nothing, NoUI)
                    Case "/createallstimuli"
                        cmdCreateAllStimuli_Click(Nothing, Nothing)
                    Case "^"
                        ' do nothing (CR handling)
                    Case Else
                        ' load settings
                        If Not (gblnCancel) Then
                            Me.SetStatus("Load settings: " & szX)
                            LoadSettings(szX, NoUI)
                        End If
                End Select
                If Len(szErr) <> 0 Then
                    MsgBox(szErr, MsgBoxStyle.Critical, "Invalid flag")
                    Exit For
                End If
            Next
        End If
        SetStatus("Welcome to " & gszAPP_TITLE & " " & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Build & _
                  " (FW " & FW_MAJOR & "." & FW_MINOR & "." & FW_REVISION & ")")
    End Sub

    Private Sub BackupFile(szFn As String)
        Dim bScanFn As Boolean = False
        Dim lX As Integer = 0

        Do While bScanFn = False
            lX += 1
            If Not My.Computer.FileSystem.FileExists("~" & TStr(lX) & szFn) Then
                My.Computer.FileSystem.RenameFile(szFn, "~" & TStr(lX) & szFn)
                bScanFn = True
            End If
        Loop
    End Sub

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        ' experiment pending - don't quit anyway
        If gblnExperiment Then
            MsgBox("Experiment is pending." & vbCrLf & "Cancel the experiment before exit.", MsgBoxStyle.Information)
            e.Cancel = True
            Exit Sub
        End If

        ' if connected cancel unload
        If gblnOutputStable Then
            If MsgBox(My.Application.Info.AssemblyName & " is connected to the output devices." & vbCrLf & "Do you want to disconnect and close this application?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then
                e.Cancel = True
                Exit Sub
            Else
                Disconnect()
            End If
        End If

        ' make sure to quit
        If gblnSettingsChanged Then
            If MsgBox("Some settings were changed. If you exit now, you will lose all changes." & vbCrLf & "Continue anyway?", MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2 Or MsgBoxStyle.Exclamation, "Exit Program") = MsgBoxResult.No Then
                e.Cancel = True : Exit Sub
            End If
        End If
    End Sub
    Private Sub frmMain_FormClosed(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ' disconnect all network connections
        SetStatus("Closing remote server...")
        ServeData(FWintern.ServeDataEnum.Close)
        If gblnRemoteServerConnected Then RemoteMonitorServer.Disconnect()
        If gblnRemoteClientConnected Then RemoteMonitorClient.Disconnect()
        ' disconnect from ViWo
        If Len(gszViWoAddress) > 0 Then
            SetStatus("Closing ViWo...")
            ViWo.Disconnect()
        End If

        INIOptions.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\" & My.Application.Info.Title & "\" & My.Application.Info.Title & ".ini")
        'close all sub forms
        ClipCursorToWindow(0)
        For lX As Integer = My.Application.OpenForms.Count - 1 To 1 Step -1
            My.Application.OpenForms(lX).Close()
        Next

        ' STIM
        STIM.Destroy()
        ' RIB
        RIB.Terminate()
        RIB2.Terminate()
    End Sub

    'Private Sub tmrMIDI_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrMIDI.Tick
    '    gblnMIDIIgnore = False
    '    tmrMIDI.Enabled = False
    'End Sub

    Private Sub tmrStatus_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrStatus.Tick
        Dim curToc, curElapsed As Long
        Dim szX As String
        Dim lX As Integer
        Dim blnBreakOnTime As Boolean

        ' calculate experiment time
        QueryPerformanceCounter(curElapsed)
        curElapsed = ((curElapsed - gcurHPTic) \ gcurHPFrequency)
        szX = (curElapsed \ 3600).ToString("#0") & ":"
        sbStatusBar.Items.Item(STB_ELAPSEDTIME).Text = szX & ((curElapsed Mod 3600) \ 60).ToString("00") & ":" & (curElapsed Mod 60).ToString("00")

        ' are we in break? -> leave now...
        If chkExpRun.CheckState = 0 Then Exit Sub
        'If (glExpFlags And frmExp.EXPFLAGS.expflStartButton) <> 0 Then Exit Sub

        ' break on time active?
        'If (glBreakFlags And 3) = 1 Then blnBreakOnTime = True
        If glBreakFlags = 1 Then blnBreakOnTime = True 'break flag set to minutes, and active

        ' set progress bar
        If blnBreakOnTime And (INISettings.glExpFlags And frmExp.EXPFLAGS.expflProgressSyncToBreak) <> 0 Then
            ' calc remaining time to the next break
            curToc = (glBreakInterval * 60) - curElapsed
            Dim sX As Double = (curToc) / (glBreakInterval * 60) * 100
            If (curToc Mod 3600) \ 30 = 0 Then
                szX = TStr((curToc Mod 3600) Mod 60) & TEXT_BREAKSECONDS
            ElseIf Math.Round((curToc Mod 3600) / 60) = 1 Then
                szX = TEXT_BREAKMINUTE
            Else
                szX = TStr(Math.Round((curToc Mod 3600) / 60)) & TEXT_BREAKMINUTES
            End If
            Windows.Forms.Application.DoEvents()
            frmExp.SetProgress(sX, szX)
            'Me.SetProgressbar(CInt(sX)) invokerequired
            ' set remaining time to the break
            lX = (CInt(Math.Round(VB.Timer())) + CInt(curToc)) Mod (CInt(24 * 60) * 60)
            szX = (lX \ 3600).ToString("#0") & ":" & ((lX Mod 3600) \ 60).ToString("00")
            sbStatusBar.Items.Item(STB_REMAININGTIME).Text = szX
        ElseIf ItemList.ItemIndex - mlFirstItemOfExp > 3 Then
            ' estimate the end of experiment
            'sProgress = (mlItemCountOfExp - lItemsLeft) / (mlItemCountOfExp) * 100
            'For lX As Integer = mlFirstItemOfExp To mlLastItemOfExp
            '    If ItemList.ItemStatus(lX) = clsItemList.Status.Fresh Or _
            '       ItemList.ItemStatus(lX) = clsItemList.Status.Processing Then mlItemCountOfExp += 1
            'Next
            curToc = (curElapsed * CLng(ItemList.ItemCount) \ CLng(ItemList.ItemIndex - 1)) - curElapsed
            curToc = (curElapsed * CLng(mlItemCountOfExp) \ CLng(ItemList.ItemIndex - mlFirstItemOfExp + 1)) - curElapsed
            lX = (CInt(Math.Round(VB.Timer())) + CInt(curToc)) Mod (CInt(24 * 60) * 60)
            szX = (lX \ 3600).ToString("#0") & ":" & ((lX Mod 3600) \ 60).ToString("00")
            sbStatusBar.Items.Item(STB_REMAININGTIME).Text = szX
        End If

        ' initiate break?
        If blnBreakOnTime Then
            ' time for a break?
            If curElapsed >= CDec(glBreakInterval * 60) Then
                chkExpRun.CheckState = System.Windows.Forms.CheckState.Unchecked
                'If glFlagBeepExp > 0 Then BeepOnBreak()
                If gblnPlayWaveExp = True Then PlayWaveOnBreak()
                ServeData(FWintern.ServeDataEnum.Break)
            End If
        End If

    End Sub

    Private Sub connectionTimer_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles connectionTimer.Tick
        Dim curElapsed As Long
        Dim szX As String

        ' calculate experiment time
        QueryPerformanceCounter(curElapsed)
        curElapsed = ((curElapsed - gcurHPTic) \ gcurHPFrequency)
        szX = (curElapsed \ 3600).ToString("#0") & ":"
        If gblnRemoteClientConnected Then sbStatusBar.Items.Item(5).Text = gszGotSettings & szX & ((curElapsed Mod 3600) \ 60).ToString("00") & ":" & (curElapsed Mod 60).ToString("00")
    End Sub

    Private Sub txtSelItem_TextChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtSelItem.TextChanged
        If Me.IsInitializing = True Then
            Exit Sub
        Else
            VB6.SetDefault(cmdItemSet, True)
        End If
    End Sub

    Private Sub txtSelItem_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtSelItem.Enter
        txtSelItem.SelectionStart = 0
        txtSelItem.SelectionLength = Len(txtSelItem.Text)
    End Sub
    Private Sub lstLog_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstLog.SelectedIndexChanged
        Me.ToolTip1.SetToolTip(Me.lstLog, VB6.GetItemString(Me.lstLog, Me.lstLog.SelectedIndex))
    End Sub

    Private Sub popupMenuStrip_Opening(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles popupMenuStrip.Opening
        'set only this context menu items enabled which are also in the edit menu available
        ctxtmnuItemUndo.Enabled = mnuItemUndo.Enabled
        ctxtmnuItemCopy.Enabled = mnuItemCopy.Enabled
        ctxtmnuItemPaste.Enabled = mnuItemPaste.Enabled
        ctxtmnuItemClearCells.Enabled = mnuItemClearCells.Enabled
        ctxtmnuItemInsert.Enabled = mnuItemInsert.Enabled
        ctxtmnuItemDuplicateBlock.Enabled = mnuItemDuplicateBlock.Enabled
        ctxtmnuItemDel.Enabled = mnuItemDel.Enabled
        ctxtmnuItemShuffleBlock.Enabled = mnuItemShuffleBlock.Enabled
        ctxtmnuItemSetExperimentBlock.Enabled = mnuItemSetExperimentBlock.Enabled
        ctxtmnuFillAutomatically.Enabled = mnuFillAutomatically.Enabled
        ctxtmnuItemRenumber.Enabled = mnuItemRenumber.Enabled
        ctxtmnuOptColWidth.Enabled = mnuOptColWidth.Enabled
    End Sub

    Private Sub mnuExpContinue_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExpContinue.Click
        Dim szErr As String = StartExperiment(AutomatisationFlags.ContinueExperiment)
        If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Continue Experiment")
    End Sub

    Private Sub mnuItemSetFresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuItemSetFresh.Click
        UndoSnapshot()
        For Each lX As Integer In ItemList.SelectedItems
            ItemList.ItemStatus(lX) = clsItemList.Status.Fresh
        Next
        ServeData(FWintern.ServeDataEnum.ChangeListStatus, 0, 0, "")
    End Sub

    Private Sub mnuItemSetProcessing_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuItemSetProcessing.Click
        UndoSnapshot()
        For Each lX As Integer In ItemList.SelectedItems
            ItemList.ItemStatus(lX) = clsItemList.Status.Processing
        Next
        ServeData(FWintern.ServeDataEnum.ChangeListStatus, 0, 0, "")
    End Sub

    Private Sub mnuItemSetFinishedOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuItemSetFinishedOK.Click
        UndoSnapshot()
        For Each lX As Integer In ItemList.SelectedItems
            ItemList.ItemStatus(lX) = clsItemList.Status.FinishedOK
        Next
        ServeData(FWintern.ServeDataEnum.ChangeListStatus, 0, 0, "")
    End Sub

    Private Sub mnuItemSetFinishedWithErrors_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuItemSetFinishedWithErrors.Click
        Dim szErr As String = InputBox( _
                "Warning: Error text is not saved when saving item list." & vbCrLf & vbCrLf & _
                "Error text:", "Set Status to Finished with Errors")
        UndoSnapshot()
        For Each lX As Integer In ItemList.SelectedItems
            ItemList.ItemStatus(lX, szErr) = clsItemList.Status.FinishedError
        Next
        ServeData(FWintern.ServeDataEnum.ChangeListStatus, 0, 0, "")
    End Sub

    Private Sub IgnoredToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles IgnoredToolStripMenuItem.Click
        UndoSnapshot()
        For Each lX As Integer In ItemList.SelectedItems
            ItemList.ItemStatus(lX) = clsItemList.Status.Ignored
        Next
        ServeData(FWintern.ServeDataEnum.ChangeListStatus, 0, 0, "")
    End Sub

    Private Sub cmdContinueExp_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdContinueExp.Click
        Dim szErr As String = StartExperiment(AutomatisationFlags.ContinueExperiment)
        If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Continue Experiment")
    End Sub

    ''' <summary>
    ''' Sets the callback on loading Settings in the dialog.
    ''' </summary>
    ''' <param name="lAddr">Address of the callback function.</param>
    ''' <remarks>OnLoad callback will be executed on loading of the Settings dialog.<br>
    ''' This gives a possibility to adapt the Settings dialog, before it have been shown.</br> </remarks>
    Public Sub SetOnBreakCallback(ByVal lAddr As OnBreakDelegate)
        gOnBreakAddr = lAddr
    End Sub

    Private Sub cmdItemMoveTop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdItemMoveTop.Click

        SetUIBusy()
        Dim arr() As Integer = ItemList.SelectedItems
        If arr(0) = 0 Then SetUIReady() : Return
        UndoSnapshot()
        Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
        dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
        dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        For lX As Integer = 0 To arr.Length - 1 ' loop for all selected items
            dgvItemList.Rows.InsertCopy(arr(lX), lX) 'insert empty row
            'Windows.Forms.Application.DoEvents()
            CopyDataGridRow(dgvItemList.Rows(arr(lX) + 1), dgvItemList.Rows(lX)) 'copy data
            dgvItemList.Rows(lX).Selected = True 'Enable (highlight)
            dgvItemList.Rows.RemoveAt(arr(lX) + 1) 'remove "old" row
        Next

        dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
        dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)
        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        SetUIReady()

    End Sub

    Private Sub cmdItemMoveBottom_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdItemMoveBottom.Click

        SetUIBusy()
        If ItemList.SelectedItemLast = ItemList.ItemCount - 1 Then SetUIReady() : Return
        Dim arr() As Integer = ItemList.SelectedItems
        If arr(arr.Length - 1) >= ItemList.ItemCount - 1 Then SetUIReady() : Return
        UndoSnapshot()
        Dim ColMode As DataGridViewAutoSizeColumnMode = DirectCast(dgvItemList.AutoSizeColumnsMode, DataGridViewAutoSizeColumnMode)
        dgvItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        Dim RowMode As DataGridViewAutoSizeRowMode = DirectCast(dgvItemList.AutoSizeRowsMode, DataGridViewAutoSizeRowMode)
        dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        For lX As Integer = 0 To arr.Length - 1 'loop for all selected items
            dgvItemList.Rows.InsertCopy(arr(lX), ItemList.ItemCount) 'insert empty row at bottom
            'Windows.Forms.Application.DoEvents()
            CopyDataGridRow(dgvItemList.Rows(arr(lX) - lX), dgvItemList.Rows(ItemList.ItemCount - 1)) 'copy data to new row
            dgvItemList.Rows(ItemList.ItemCount - 1).Selected = True 'Enable (highlight)
            dgvItemList.Rows.RemoveAt(arr(lX) - lX) 'remove "old" row
        Next
        dgvItemList.AutoSizeColumnsMode = DirectCast(ColMode, DataGridViewAutoSizeColumnsMode)
        dgvItemList.AutoSizeRowsMode = DirectCast(RowMode, DataGridViewAutoSizeRowsMode)
        ServeData(FWintern.ServeDataEnum.Itemlist)
        ServeData(FWintern.ServeDataEnum.ChangeListStatus)
        SetUIReady()

    End Sub

    Private Sub mnuFillAutomatically_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFillAutomatically.Click
        Dim StartValue, StepSize As Double
        Dim inpX As New frmInputBox

        If dgvItemList.SelectedCells.Count = 0 Then MsgBox("No cells selected!", MsgBoxStyle.Exclamation, "Fill cells automatically") : Exit Sub

        inpX.Add("Start Value", FWintern.VariableFlags.vfNumeric, "0", "")
        inpX.Add("Step Size", FWintern.VariableFlags.vfNumeric, "1", "")
        inpX.SetLeft = Me.Left + 100
        inpX.SetTop = Me.Top + 200
        Me.TopMost = False
        If Not inpX.ShowForm("Fill cells automatically with incrementing/decrementing values.") Then
            inpX.Dispose() : Return
        End If
        StartValue = Val(inpX.GetValue(0))
        StepSize = Val(inpX.GetValue(1))
        inpX.Dispose()

        Dim lX As Long = dgvItemList.SelectedCells.Count - 1 'Fill cells
        For Each cell As DataGridViewCell In dgvItemList.SelectedCells
            cell.Value = TStr(StartValue + lX * StepSize)
            lX -= 1
        Next
        ServeData(ServeDataEnum.Itemlist)
    End Sub

    Private Sub mnuRemoteMonitorGetSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRemoteMonitorGetSettings.Click
        If gblnRemoteServerConnected Then Exit Sub
        If Not gblnRemoteClientConnected Then Exit Sub

        Dim setLoaded As Boolean = RemoteMonitorClient.SettingsRequest()

        If setLoaded Then
            gSettingsDone.WaitOne()

            If Not IsNothing(glOnSettingsExpTypeChangeAddr) Then glOnSettingsExpTypeChangeAddr(-1, CInt(Val(gszSettingstype)))

            ClearParameters() ' clear only on loading
            gblnSettingsChanged = False
            gblnIsDotNETSetting = False ' assume a VB6-settings version

            ddTemp = New clsDataDirectory
            ddTemp.Reset()

            For i As Integer = 0 To glSettingslength - 1
                INISettings.RemoteParseLine(gszSettings(i))
            Next

            Dim szErr As String
            Dim szKey As String
            Dim lX, lY As Integer

            ' copy data directory data
            lY = ddTemp.Count
            If ddTemp.Count > DataDirectory.Count Then lY = DataDirectory.Count
            For lX = 0 To lY - 1
                If Len(ddTemp.Title(lX)) > 0 Then DataDirectory.Title(lX) = ddTemp.Title(lX)
                DataDirectory.Path(lX) = ddTemp.Path(lX)
            Next
            ' display new parameters
            gszSettingTitle = gszSettingsfilename
            lX = InStrRev(gszSettingsfilename, "\")
            If lX > 0 Then
                gszCurrentDir = Mid(gszSettingsfilename, 1, lX - 1)
            Else
                gszCurrentDir = ""
            End If
            ' get the new title and file name
            If InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) > 0 Then
                gszSettingTitle = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) - 1)
            ElseIf InStrRev(gszSettingTitle, ".esf") > 0 Then
                gszSettingTitle = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, ".esf") - 1)
            End If
            gszSettingFileName = gszSettingsfilename
            If InStrRev(gszSettingTitle, "\") > 0 Then gszSettingTitle = Mid(gszSettingTitle, InStrRev(gszSettingTitle, "\") + 1)
            If gblnIsDotNETSetting Then
                SetStatus(".NET-Settings: " & gszSettingTitle)
            Else
                SetStatus("VB6-Settings: " & gszSettingTitle)
            End If

            ' check if settings are consistent with fitting files in electrical mode
            If (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB) Or (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB2) Or (INISettings.gStimOutput = STIM.GENMODE.genVocoder) Then
                szKey = gszSourceDir
                If Len(gszFittFileLeft) <> 0 Then
                    szErr = CheckSignalConsistency(gfreqParL, szKey & "\" & gszFittFileLeft)
                    If Len(szErr) > 0 Then If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Loading settings for the left channel:")
                End If
                If Len(gszFittFileRight) <> 0 Then
                    szErr = CheckSignalConsistency(gfreqParR, szKey & "\" & gszFittFileRight)
                    If Len(szErr) > 0 Then If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Loading settings for the right channel:")
                End If
            End If
            ' notice EventsSettings
            If Not IsNothing(glOnSettingsSetAddr) Then glOnSettingsSetAddr()
            SetUIReady()

            gblnSettingsForm = False
            gblnSettingsLoaded = True
            gszGotSettings = ""
        End If

        ItemList.ItemCount = 100
        ItemList.ItemCount = 0

        RemoteMonitorClient.ItemListRequest()
    End Sub

    Delegate Sub mnuRemoteMonitorGetSettings_RemoteCallback()
    Public Sub mnuRemoteMonitorGetSettings_Remote()
        If gblnRemoteServerConnected Then Exit Sub
        If Not gblnRemoteClientConnected Then Exit Sub

        If Not IsNothing(glOnSettingsExpTypeChangeAddr) Then glOnSettingsExpTypeChangeAddr(-1, CInt(Val(gszSettingstype)))

        ClearParameters() ' clear only on loading
        gblnSettingsChanged = False
        gblnIsDotNETSetting = False ' assume a VB6-settings version

        ddTemp = New clsDataDirectory
        ddTemp.Reset()

        For i As Integer = 0 To glSettingslength - 1
            INISettings.RemoteParseLine(gszSettings(i))
        Next

        Dim szErr As String
        Dim szKey As String
        Dim lX, lY As Integer

        ' copy data directory data
        lY = ddTemp.Count
        If ddTemp.Count > DataDirectory.Count Then lY = DataDirectory.Count
        For lX = 0 To lY - 1
            If Len(ddTemp.Title(lX)) > 0 Then DataDirectory.Title(lX) = ddTemp.Title(lX)
            DataDirectory.Path(lX) = ddTemp.Path(lX)
        Next
        ' display new parameters
        gszSettingTitle = gszSettingsfilename
        lX = InStrRev(gszSettingsfilename, "\")
        If lX > 0 Then
            gszCurrentDir = Mid(gszSettingsfilename, 1, lX - 1)
        Else
            gszCurrentDir = ""
        End If
        ' get the new title and file name
        If InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) > 0 Then
            gszSettingTitle = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) - 1)
        ElseIf InStrRev(gszSettingTitle, ".esf") > 0 Then
            gszSettingTitle = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, ".esf") - 1)
        End If
        gszSettingFileName = gszSettingsfilename
        If InStrRev(gszSettingTitle, "\") > 0 Then gszSettingTitle = Mid(gszSettingTitle, InStrRev(gszSettingTitle, "\") + 1)
        If gblnIsDotNETSetting Then
            Proc.SetStatus(glistBox, gtssl1, ".NET-Settings: " & gszSettingTitle)
        Else
            Proc.SetStatus(glistBox, gtssl1, "VB6-Settings: " & gszSettingTitle)
        End If

        ' check if settings are consistent with fitting files in electrical mode
        If (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB) Or (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB2) Or (INISettings.gStimOutput = STIM.GENMODE.genVocoder) Then
            szKey = gszSourceDir
            If Len(gszFittFileLeft) <> 0 Then
                szErr = CheckSignalConsistency(gfreqParL, szKey & "\" & gszFittFileLeft)
                If Len(szErr) > 0 Then If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Loading settings for the left channel:")
            End If
            If Len(gszFittFileRight) <> 0 Then
                szErr = CheckSignalConsistency(gfreqParR, szKey & "\" & gszFittFileRight)
                If Len(szErr) > 0 Then If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Loading settings for the right channel:")
            End If
        End If
        ' notice EventsSettings
        If Not IsNothing(glOnSettingsSetAddr) Then glOnSettingsSetAddr()
        SetUIReady()
        Proc.LabelText(glabel2, gszExpTypeNames(glExpType))

        gblnSettingsForm = False
        gblnSettingsLoaded = True

        'ItemList.ItemCount = 100
        'ItemList.ItemCount = 0

    End Sub

    Private Sub mnuCheckForUpdates_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCheckForUpdates.Click
        CheckForUpdates(False)
    End Sub

    Private Sub CheckForUpdates(Optional ByVal bSuppressMsg As Boolean = False)
        Dim szErr As String = ""
        Dim szServer As String = "Sourceforge"
        Dim sr As IO.StreamReader
        Dim szVersionLine As String = ""
        Dim szMyVersionLine As String= My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor  & "." & My.Application.Info.Version.Build 
        Dim szHistLine As String = ""
        Dim szVersion As String = ""
        Dim bOutOfDate As Boolean = False ' boolean = true when application out of date
        Dim lMajor As Integer = 0
        Dim lMinor As Integer = 0
        Dim Build As Integer = 0
        Dim Revision As Integer = 0
        Dim bFromPDrive As Boolean = False
        Dim bFromkfs4Drive As Boolean = False

        If My.Application.Info.Version.Revision > 0 Then szMyVersionLine = szMyVersionLine & "." & My.Application.Info.Version.Revision 

        Dim szUpdateServers() As String = Nothing
        ReDim szUpdateServers(27)
        Dim lUS As Integer = 0
        szUpdateServers(lUS) = "master" : lUS += 1 'sourceforge server itself
        'szUpdateServers(lUS) = "astuteinternet" : lUS += 1 '?
        szUpdateServers(lUS) = "versaweb" : lUS += 1
        szUpdateServers(lUS) = "altushost-swe" : lUS += 1
        'szUpdateServers(lUS) = "ayera" : lUS  += 1
        szUpdateServers(lUS) = "cfhcable" : lUS += 1
        'szUpdateServers(lUS) = "cytranet" : lUS += 1
        'szUpdateServers(lUS) = "datapacket" : lUS += 1
        'szUpdateServers(lUS) = "deac-riga" : lUS += 1
        szUpdateServers(lUS) = "deac-ams" : lUS += 1
        szUpdateServers(lUS) = "deac-fra" : lUS += 1
        szUpdateServers(lUS) = "excellmedia" : lUS += 1
        szUpdateServers(lUS) = "freefr" : lUS += 1
        'szUpdateServers(lUS) = "gigenet" : lUS += 1
        szUpdateServers(lUS) = "iweb" : lUS += 1
        szUpdateServers(lUS) = "ixpeering" : lUS += 1
        szUpdateServers(lUS) = "jaist" : lUS += 1
        szUpdateServers(lUS) = "jztkft" : lUS += 1
        szUpdateServers(lUS) = "kumisystems" : lUS += 1
        szUpdateServers(lUS) = "liquidtelecom" : lUS += 1
        szUpdateServers(lUS) = "managedway" : lUS += 1
        szUpdateServers(lUS) = "megalink" : lUS += 1
        szUpdateServers(lUS) = "nchc" : lUS += 1
        szUpdateServers(lUS) = "netactuate" : lUS += 1
        szUpdateServers(lUS) = "netcologne" : lUS += 1
        szUpdateServers(lUS) = "netix" : lUS += 1
        szUpdateServers(lUS) = "newcontinuum" : lUS += 1
        szUpdateServers(lUS) = "phoenixnap" : lUS += 1
        szUpdateServers(lUS) = "pilotfiber" : lUS += 1
        szUpdateServers(lUS) = "razaoinfo" : lUS += 1
        szUpdateServers(lUS) = "sonik" : lUS += 1
        szUpdateServers(lUS) = "tenet" : lUS += 1
        szUpdateServers(lUS) = "ufpr" : lUS += 1
        szUpdateServers(lUS) = "vorboss" : lUS += 1
        lUS = 0
        Me.SetStatus("Checking for updates, this may take a couple of seconds...")
        Me.Cursor = Cursors.WaitCursor
        SetUIBusy()
        Application.DoEvents()

        ' create temp file name
        Dim szTempFileName As String = System.IO.Path.GetTempPath.ToString & My.Application.Info.Title & _
        "_history" & "_" & System.DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".txt"

        If File.Exists("\\w07kfs4.kfs.oeaw.ac.at\eap\Projects\ExpSuite\ExpSuite.NET\Installable Setup Packages\Applications\" & My.Application.Info.Title & "\history.txt") Then
            bFromkfs4Drive = True
            szServer = "w07kfs4"
        ElseIf File.Exists("P:\Projects\ExpSuite\ExpSuite.NET\Installable Setup Packages\Applications\" & My.Application.Info.Title & "\history.txt") Then
            bFromPDrive = True
            szServer = "P-Drive"
        End If

        Try
            If bFromkfs4Drive Then
                'access to p-drive?
                File.Copy("\\w07kfs4.kfs.oeaw.ac.at\eap\Projects\ExpSuite\ExpSuite.NET\Installable Setup Packages\Applications\" & My.Application.Info.Title & "\history.txt", szTempFileName)
            ElseIf bFromPDrive Then
                'access to p-drive?
                File.Copy("P:\Projects\ExpSuite\ExpSuite.NET\Installable Setup Packages\Applications\" & My.Application.Info.Title & "\history.txt", szTempFileName)
            Else
TryOtherServer:
                ' download application history to temp directory
                ' https://versaweb.dl.sourceforge.net/project/expsuite/Applications/AMTatARI/history.txt
                'Debug.Print("Try to download:   https://" & szUpdateServers(lUS) & ".dl.sourceforge.net/project/expsuite/Applications/" & My.Application.Info.Title & "/history.txt")
                Me.Setstatus("Try to download from mirror:   https://" & szUpdateServers(lUS) & ".dl.sourceforge.net/project/expsuite/Applications/" & My.Application.Info.Title & "/history.txt")
                My.Computer.Network.DownloadFile("https://" & szUpdateServers(lUS) & ".dl.sourceforge.net/project/expsuite/Applications/" & My.Application.Info.Title & "/history.txt", szTempFileName)
            End If
        Catch
            Debug.Print("  Not available:   https://" & szUpdateServers(lUS) & ".dl.sourceforge.net/project/expsuite/Applications/" & My.Application.Info.Title & "/history.txt")
            'Me.Setstatus("Not available:   https://" & szUpdateServers(lUS) & ".dl.sourceforge.net/project/expsuite/Applications/" & My.Application.Info.Title & "/history.txt")
            If lUS < UBound(szUpdateServers) Then lUS += 1 : GoTo TryOtherServer
        End Try

        gLastUpdateCheck = System.DateTime.Now.ToString("yyyyMMdd")
        If gblnCancel Then GoTo SubCancel
        ' check if dowload was successful?
        If Dir(szTempFileName) = "" Or Len(szTempFileName) = 0 Then szErr = "History file could not be downloaded automatically. Check for updates could not succeed." : GoTo SubError

        sr = New IO.StreamReader(szTempFileName)
        Dim bBlabla As Boolean = False
        Dim szBlabla As String = ""
        Dim lX As Integer = 0
        Do
            szHistLine = (sr.ReadLine())
            If IsNothing(szHistLine) Then Exit Do
            If bBlabla Then
                If szHistLine.StartsWith("*** v") Then Exit Do
                szBlabla = szBlabla & vbCrLf & szHistLine
                lX += 1
                If lX = 20 Then szBlabla = szBlabla & vbCrLf & szHistLine & vbCrLf & "..." : Exit Do
            ElseIf szHistLine.StartsWith("*** v") Then
                Console.WriteLine(szHistLine)
                szVersionLine = RTrim(szHistLine).Remove(0, 5)
                'Exit Do
                bBlabla = True
            End If
            If gblnCancel Then sr.Close() : GoTo SubCancel
        Loop
        sr.Close()
        My.Computer.FileSystem.DeleteFile(szTempFileName)
        If szVersionLine = "" Then szErr = "No version number found" : GoTo suberror

        Dim MyPos As Integer
        MyPos = InStr(szVersionLine, " ") ' searching for first space in history file
        If MyPos > 0 Then
            szVersion = szVersionLine.Remove(MyPos - 1) ' cutting from space to right everything; can be used to compare version numbers
            Console.WriteLine("mypos " & MyPos & vbCrLf & "1 " & szVersion)
        End If

        Dim szVersionDL As String = szVersion
        Dim szSilent As String = " /SILENT /LaunchApp=True"
        For lX = 0 To 3 ' loop for: major, minor, build, revision
            Dim lVersion As Integer = 0 ' temp. variable
            Dim lSeperator As Integer = InStr(szVersion, ".") ' find first "." in version number
            If lSeperator <> 0 Then ' found a "."?
                lVersion = CInt(Val(szVersion.Remove(lSeperator - 1))) ' yes -> split
            Else
                lVersion = CInt(Val(szVersion)) ' no -> use value as it is
            End If
            If Len(lVersion) = 0 Then szErr = "Version number could not be established!" : GoTo SubError 'not numeric
            Select Case lX
                Case 0 ' Get Major Release version
                    Console.WriteLine(szServer & " Major: " & lVersion)
                    If lVersion < My.Application.Info.Version.Major Then Exit For ' Up-to-date
                    If lVersion > My.Application.Info.Version.Major Then szSilent = "" : bOutOfDate = True ' Out-of-date,  NO SILENT installation at major update
                Case 1 ' Get Minor Release version
                    Console.WriteLine(szServer & " Minor: " & lVersion)
                    If lVersion < My.Application.Info.Version.Minor Then Exit For ' Up-to-date
                    If lVersion > My.Application.Info.Version.Minor Then bOutOfDate = True ' Out-of-date
                Case 2 ' Get Build version
                    Console.WriteLine(szServer & " Build: " & lVersion)
                    If lVersion < My.Application.Info.Version.Build Then Exit For ' Up-to-date
                    If lVersion > My.Application.Info.Version.Build Then bOutOfDate = True ' Out-of-date
                Case 3 ' Get Revision version
                    Console.WriteLine(szServer & " Revision: " & lVersion)
                    If lVersion < My.Application.Info.Version.Revision Then Exit For ' Up-to-date
                    If lVersion > My.Application.Info.Version.Revision Then bOutOfDate = True ' Out-of-date
            End Select
            If bOutOfDate = True Or lSeperator = 0 Then ' exit loop when out-of-date or last digit checked
                Exit For
            Else
                szVersion = szVersion.Remove(0, lSeperator) ' cut first part
            End If
        Next

        Windows.Forms.Application.DoEvents()
        If gblnCancel Then GoTo SubCancel
        If bOutOfDate = False Then ' up-to-date
            'If My.Application.Info.Version.Revision = 0 Then
                Me.SetStatus("Your version is up-to-date! Current Version: v" & _
                    szMyVersionLine  & " (checked on " & szServer & ")")
                If Not bSuppressMsg Then MsgBox("Your version is up-to-date!" & vbCrLf & " (checked on " & szServer & ")" & vbCrLf & vbCrLf & "Current Version: v" & _
                    szMyVersionLine , MsgBoxStyle.Information, _
                    My.Application.Info.Title & " v" & szMyVersionLine)
            'Else
            '    Me.SetStatus("Your version is up-to-date! Current Version: v" & _
            '        My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Build & "." & My.Application.Info.Version.Revision & " (checked on " & szServer & ")")
            '    If Not bSuppressMsg Then MsgBox("Your version is up-to-date!" & vbCrLf & " (checked on " & szServer & ")" & vbCrLf & vbCrLf & "Current Version: v" & _
            '        My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Build & "." & My.Application.Info.Version.Revision, MsgBoxStyle.Information, _
            '        My.Application.Info.Title & " v" & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Build & "." & My.Application.Info.Version.Revision)
            'End If
            
        Else 'msgbox tells user which version is installed and which can be found on sourceforge/w07kfs4
            Me.SetStatus("New version on " & szServer & ": v" & szVersionLine)
            If MsgBox("A new version of " & My.Application.Info.Title & " is available:" & vbCrLf & vbCrLf & _
                      "Do you want to update " & My.Application.Info.Title & " from " & szServer & "? The application will be closed if you click YES!" & vbCrLf & vbCrLf & _
                "Current Version: " & vbTab & "v" & szMyVersionLine & vbCrLf & _
                "On " & szServer & ": " & vbTab & "v" & szVersionDL & vbCrLf & vbCrLf & _
                   "Last changes:" & vbCrLf & My.Application.Info.Title & " v" & szVersionLine & szBlabla, _
                MsgBoxStyle.Information Or MsgBoxStyle.YesNo, My.Application.Info.Title & " Update") = MsgBoxResult.Yes Then
                ' System.Diagnostics.Process.Start("http://sourceforge.net/projects/expsuite/files/Applications/" & My.Application.Info.Title)

                Dim pos As Integer = -1
                Dim count As Integer = 0
                Dim CompareType As StringComparison
                Do
                    pos = szVersionDL.IndexOf(".", pos + 1, CompareType)
                    If pos >= 0 Then count += 1
                Loop Until pos < 0

                Dim szFileName As String = ""
                If count = 2 Then
                    szFileName = My.Application.Info.Title & "_setup_" & szVersionDL & ".0.exe"
                ElseIf count = 3 Then
                    szFileName = My.Application.Info.Title & "_setup_" & szVersionDL & ".exe"
                End If

                Debug.Print(szFileName)
                If gblnCancel Then GoTo SubCancel
                Try
                    If bFromkfs4Drive Then
                        Me.SetStatus("Downloading " & szFileName & " from w07kfs4 server")
                        File.Copy("\\w07kfs4.kfs.oeaw.ac.at\eap\Projects\ExpSuite\ExpSuite.NET\Installable Setup Packages\Applications\" & My.Application.Info.Title & "\" & szFileName, Environment.GetEnvironmentVariable("Temp") & "\" & szFileName, True)
                    ElseIf bFromPDrive Then
                        Me.SetStatus("Downloading " & szFileName & " from P-drive")
                        File.Copy("P:\Projects\ExpSuite\ExpSuite.NET\Installable Setup Packages\Applications\" & My.Application.Info.Title & "\" & szFileName, Environment.GetEnvironmentVariable("Temp") & "\" & szFileName, True)
                    Else
                        Debug.Print("Try to download: " & "http://" & szUpdateServers(lUS) & ".dl.sourceforge.net/project/expsuite/Applications/" & My.Application.Info.Title & "/" & szFileName)
                        Me.SetStatus("Downloading " & szFileName & " from Sourceforge")
                        My.Computer.Network.DownloadFile("http://" & szUpdateServers(lUS) & ".dl.sourceforge.net/project/expsuite/Applications/" & My.Application.Info.Title & "/" & szFileName, _
                                     Environment.GetEnvironmentVariable("Temp") & "\" & szFileName, "", "", True, 100, True, FileIO.UICancelOption.ThrowException)
                    End If

                    Me.SetStatus("Download finished, executing " & szFileName)
                    
                    Windows.Forms.Application.DoEvents()
                    If gblnCancel Then GoTo SubCancel
                    Shell(Environment.GetEnvironmentVariable("Temp") & "\" & szFileName & szSilent)
                    Me.Cursor = Cursors.Default
                    Windows.Forms.Application.DoEvents()
                    If gblnCancel Then GoTo SubCancel
                    Me.Close()
                    Exit Sub
                Catch
                    GoTo SubError
                End Try
                'System.Diagnostics.Process.Start(szErr)
            End If
        End If
        Me.Cursor = Cursors.Default
        SetUIReady()
        Exit Sub

SubCancel:
        Me.SetStatus("Checking for updates cancelled.")
        Me.Cursor = Cursors.Default
        SetUIReady()
        Exit Sub

SubError:
        Me.SetStatus("Error when checking for updates: Current version number: v" & szMyVersionLine)
        If Not bSuppressMsg Then
            If MsgBox("Error when checking for updates on " & szServer & ":" & vbCrLf & vbCrLf & szErr & vbCrLf & vbCrLf & _
                        "Current version number: v" & szMyVersionLine & vbCrLf & vbCrLf & _
                        "The reason might be that some Sourceforge mirrors are not available anymore." & vbCrLf & vbCrLf & _
                        "Do you want to check for updates manually on the Sourceforge project page?", _
                        MsgBoxStyle.Critical Or MsgBoxStyle.YesNo, My.Application.Info.Title & " Version") = MsgBoxResult.Yes Then
                System.Diagnostics.Process.Start("http://sourceforge.net/projects/expsuite/files/Applications/" & My.Application.Info.Title)

            End If
        End If
        Me.Cursor = Cursors.Default
        SetUIReady()
    End Sub

    Private Sub mnuExpSuiteOnSourceforge_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExpSuiteOnSourceforge.Click
        System.Diagnostics.Process.Start("http://sourceforge.net/projects/expsuite/")
    End Sub

    Private Sub mnuRemoteMonitorDisconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRemoteMonitorDisconnect.Click
        RemoteMonitorClient.Disconnect()

        gszGotSettings = ""
        mnuRemoteMonitorGetSettings.Enabled = False
        mnuRemoteMonitorGetItemlist.Enabled = False
        mnuRemoteMonitorDisconnect.Enabled = False
        mnuRemoteMonitorFollowCurrentItem.Enabled = False
        mnuRemoteMonitorUpdateSettings.Enabled = False
        mnuRemoteMonitorUpdateSettings.Checked = True
        gblnRemoteMonitorUpdateSettings = True

        SetUIReady()
    End Sub

    Private Sub mnuRemoteMonitorGetItemlist_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRemoteMonitorGetItemlist.Click
        RemoteMonitorClient.ItemListRequest()
    End Sub

    Private Sub mnuRemoteMonitorFollowCurrentItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRemoteMonitorFollowCurrentItem.Click
        If mnuRemoteMonitorFollowCurrentItem.Checked Then
            mnuRemoteMonitorFollowCurrentItem.Checked = False
            gblnRemoteMonitorFollowCurrentItem = False
        Else
            mnuRemoteMonitorFollowCurrentItem.Checked = True
            gblnRemoteMonitorFollowCurrentItem = True
        End If
    End Sub

    Private Sub mnuRemoteMonitorDisconnectAllClients_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRemoteMonitorDisconnectAllClients.Click
        gblnKickClients = True
        RemoteMonitorServer.Disconnect()
        gblnKickClients = False
    End Sub

    Private Sub mnuRemoteMonitorUpdateSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRemoteMonitorUpdateSettings.Click
        Dim response As Boolean
        response = RemoteMonitorClient.UpdateSettings()
        mnuRemoteMonitorUpdateSettings.Checked = response
        gblnRemoteMonitorUpdateSettings = response
    End Sub

    Private Sub mnuRemoteMonitorConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRemoteMonitorConnect.Click
        If gblnRemoteServerConnected Then Exit Sub
        If gblnRemoteClientConnected Then Exit Sub
        gblnRemoteMonitorUpdateSettings = True
        mnuRemoteMonitorUpdateSettings.Checked = True

        Dim szX As String
        If gblnRemoteConnectOutput Then
            szX = gszRemoteMonitorServerAdress
        Else
            'If gszRemoteMonitorServerAdress Is "" Then
            '    If gszHostConnected Is "" Then
            '        szX = InputBox("IP-Address of the Remote Server:", "Connect to Remote Machine", "193.171.195.42")
            '    Else
            '        szX = InputBox("IP-Address of the Remote Server:", "Connect to Remote Machine", gszHostConnected)
            '    End If
            'Else
            szX = InputBox("IP-Address/Name of the Remote Server:", "Connect to Remote Server", gszRemoteMonitorServerAdress)
            'End If
        End If

        If Len(szX) = 0 Then Return
        gszRemoteMonitorServerAdress = szX
        'If UCase(gszRemoteMonitorServerAdress) = UCase(My.Computer.Name) Then MsgBox("Server and Client may not run on same computer!", MsgBoxStyle.Critical, "Connect to Remote Server") : Return
        gblnCancel = False
        SetUIBusy()
        Dim connected As Boolean = RemoteMonitorClient.ConnectTo(szX, Me, lstStatus, lblItemNr, _sbStatusBar_Panel0, _sbStatusBar_Panel5, _sbStatusBar_Panel3, _sbStatusBar_Panel4, dgvItemList, mnuRemoteMonitor, mnuRemoteMonitorDisconnectAllClients, mnuRemoteMonitorConnect, lblExpType)
        SetUIReady()
        If Not connected Then Exit Sub
        If gblnRemoteConnectOutput Then Exit Sub

        QueryPerformanceCounter(gcurHPTic)

        'If MsgBox("Do you want to load Settings" & vbCrLf & "from Server Application?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton1) = MsgBoxResult.Yes Then

        Dim setLoaded As Boolean = RemoteMonitorClient.SettingsRequest()

        If setLoaded Then
            gSettingsDone.WaitOne()

            If Not IsNothing(glOnSettingsExpTypeChangeAddr) Then glOnSettingsExpTypeChangeAddr(-1, CInt(Val(gszSettingstype)))

            ClearParameters() ' clear only on loading
            gblnSettingsChanged = False
            gblnIsDotNETSetting = False ' assume a VB6-settings version

            ddTemp = New clsDataDirectory
            ddTemp.Reset()

            For i As Integer = 0 To glSettingslength - 1
                INISettings.RemoteParseLine(gszSettings(i))
            Next

            Dim szErr As String
            Dim szKey As String
            Dim lX, lY As Integer

            ' copy data directory data
            lY = ddTemp.Count
            If ddTemp.Count > DataDirectory.Count Then lY = DataDirectory.Count
            For lX = 0 To lY - 1
                If Len(ddTemp.Title(lX)) > 0 Then DataDirectory.Title(lX) = ddTemp.Title(lX)
                DataDirectory.Path(lX) = ddTemp.Path(lX)
            Next
            ' display new parameters
            gszSettingTitle = gszSettingsfilename
            lX = InStrRev(gszSettingsfilename, "\")
            If lX > 0 Then
                gszCurrentDir = Mid(gszSettingsfilename, 1, lX - 1)
            Else
                gszCurrentDir = ""
            End If
            ' get the new title and file name
            If InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) > 0 Then
                gszSettingTitle = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, "." & My.Application.Info.AssemblyName) - 1)
            ElseIf InStrRev(gszSettingTitle, ".esf") > 0 Then
                gszSettingTitle = Mid(gszSettingTitle, 1, InStrRev(gszSettingTitle, ".esf") - 1)
            End If
            gszSettingFileName = gszSettingsfilename
            If InStrRev(gszSettingTitle, "\") > 0 Then gszSettingTitle = Mid(gszSettingTitle, InStrRev(gszSettingTitle, "\") + 1)
            If gblnIsDotNETSetting Then
                SetStatus(".NET-Settings: " & gszSettingTitle)
            Else
                SetStatus("VB6-Settings: " & gszSettingTitle)
            End If

            ' check if settings are consistent with fitting files in electrical mode
            If (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB) Or (INISettings.gStimOutput = STIM.GENMODE.genElectricalRIB2) Or (INISettings.gStimOutput = STIM.GENMODE.genVocoder) Then
                szKey = gszSourceDir
                If Len(gszFittFileLeft) <> 0 Then
                    szErr = CheckSignalConsistency(gfreqParL, szKey & "\" & gszFittFileLeft)
                    If Len(szErr) > 0 Then If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Loading settings for the left channel:")
                End If
                If Len(gszFittFileRight) <> 0 Then
                    szErr = CheckSignalConsistency(gfreqParR, szKey & "\" & gszFittFileRight)
                    If Len(szErr) > 0 Then If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Loading settings for the right channel:")
                End If
            End If
            ' notice EventsSettings
            If Not IsNothing(glOnSettingsSetAddr) Then glOnSettingsSetAddr()
            SetUIReady()

            gblnSettingsForm = False
            gblnSettingsLoaded = True
            gszGotSettings = ""
        End If
        'End If

        'dgvItemList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None

        ItemList.ItemCount = 100
        ItemList.ItemCount = 0

        RemoteMonitorClient.ItemListRequest()

        mnuRemoteMonitorGetSettings.Enabled = True
        mnuRemoteMonitorGetItemlist.Enabled = True
        mnuRemoteMonitorDisconnect.Enabled = True
        mnuRemoteMonitorFollowCurrentItem.Enabled = True
        mnuRemoteMonitorUpdateSettings.Enabled = True
    End Sub

    Private Sub CreditsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CreditsToolStripMenuItem.Click
        MsgBox("ExpSuite Main Developers" & vbCrLf & _
            "       Piotr Majdak" & vbCrLf & _
            "       Michael Mihocic" & vbCrLf & vbCrLf & _
            "Porting from VB6 to VB.NET" & vbCrLf & "       Katharina Egger" & vbCrLf & vbCrLf & _
            "Setup Installer and Help File" & vbCrLf & "       Guillem Quer Romeo" & vbCrLf & vbCrLf & _
            "Remote Monitor Feature" & vbCrLf & "       Harald Ziegelwanger" & vbCrLf & vbCrLf & _
            "RIB2 Support" & vbCrLf & "       Katharina Egger" & vbCrLf & "       Otto Peter, University of Innsbruck" & vbCrLf & vbCrLf & _
            "Icon ExpSuite FW v1.0" & vbCrLf & "       Martin Lindenbeck", MsgBoxStyle.Information, "Credits")
    End Sub

    Private Sub FlagsToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles FlagsToolStripMenuItem.Click
        Dim szLine As String
        szLine = "The following flags are available when you start this application in a command window or a batch file:" & vbCrLf & vbCrLf & _
            Chr(34) & "My Settings File." & My.Application.Info.AssemblyName & Chr(34) & vbTab & "Open " & My.Application.Info.AssemblyName & " settings file" & vbCrLf & _
            "/c or /connect" & vbTab & vbTab & "Connect" & vbCrLf & _
            "/s or /startexperiment" & vbTab & "Start experiment" & vbCrLf & _
            "/stimulateall" & vbTab & vbTab & "Stimulate all items" & vbCrLf & _
            "/continue or" & vbCrLf & _
            "/continueexperiment" & vbTab & "Continue experiment" & vbCrLf & _
            "/shuffle or /shuffleitemlist" & vbTab & "Shuffle item list" & vbCrLf & _
            "/createitemlist" & vbTab & vbTab & "Create item list" & vbCrLf & _
            "/addrepetition" & vbTab & vbTab & "Add repetition to item list" & vbCrLf & _
            "/createallstimuli" & vbTab & vbTab & "Create all stimuli" & vbCrLf & _
            "/saveitemlist" & vbTab & vbTab & "Save item list" & vbCrLf & _
            "/d or /disconnect" & vbTab & vbTab & "Disconnect" & vbCrLf & _
            "/e 1 or /execute 1" & vbTab & vbTab & "Execute first function in results menu" & vbCrLf & _
            vbTab & vbTab & vbTab & "(replace '1' by index in results menu)" & vbCrLf & _
            vbCrLf & "The parameters will be executed sequentially. Example:" & vbCrLf & _
            Chr(34) & My.Application.Info.AssemblyName & ".exe" & Chr(34) & " " & Chr(34) & "My Settings File." & _
            My.Application.Info.AssemblyName & Chr(34) & _
            " /connect /startexperiment /saveitemlist" & vbCrLf & vbCrLf & _
            "(If you create long batch files you can use '^CR' as line feed to separate the flag commands to multiple lines.)"

        MsgBox(szLine, MsgBoxStyle.Information, My.Application.Info.AssemblyName & " Command Window Flags")
    End Sub

    Private Sub frmMain_DragOver(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles Me.DragOver
        If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
            e.Effect = DragDropEffects.All      ' allow Drag and Drop effect
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub frmMain_ResizeEnd(sender As Object, e As System.EventArgs) Handles Me.ResizeEnd
        If Me.WindowState <> FormWindowState.Minimized Then 'only if window is not minimized (otherwise the values for .Left and .Top are set to -32000)
            grectMain.Left = Me.Left
            grectMain.Top = Me.Top
            grectMain.Height = Me.Height
            grectMain.Width = Me.Width
        End If
    End Sub

    Private Sub ApplicationHistoryToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ApplicationHistoryToolStripMenuItem.Click
           ' open history.txt file (if available)
        Dim HistoryPath  As String = Nothing
        
        If Strings.Right(My.Application.Info.DirectoryPath, Len("\bin")) = "\bin" Then
            HistoryPath = Strings.Left(My.Application.Info.DirectoryPath, Len(My.Application.Info.DirectoryPath) - Len("\bin")) 
        elseIf Strings.Right(My.Application.Info.DirectoryPath, Len("\obj")) = "\obj" Then
            HistoryPath = Strings.Left(My.Application.Info.DirectoryPath, Len(My.Application.Info.DirectoryPath) - Len("\obj")) 
        Else
            HistoryPath =  My.Application.Info.DirectoryPath & "\doc"
        End If

        Dim  HistoryFile As String = HistoryPath & "\history.txt"

        If System.IO.File.Exists(HistoryFile) = True Then
            Process.Start(HistoryFile)
        Else
            MsgBox("File not existing:" & vbCrLf & HistoryFile, MsgBoxStyle.Exclamation,"Show History")
        End If
        
    End Sub

    Private Sub FWHistoryToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FWHistoryToolStripMenuItem.Click
             ' open FWhistory.txt file (if available)
        Dim HistoryPath  As String = Nothing
        
        If Strings.Right(My.Application.Info.DirectoryPath, Len("\bin")) = "\bin" Then
            HistoryPath = Strings.Left(My.Application.Info.DirectoryPath, Len(My.Application.Info.DirectoryPath) - Len("\bin")) & "\FW"
        elseIf Strings.Right(My.Application.Info.DirectoryPath, Len("\obj")) = "\obj" Then
            HistoryPath = Strings.Left(My.Application.Info.DirectoryPath, Len(My.Application.Info.DirectoryPath) - Len("\obj")) & "\FW"
        Else 'installed version
            HistoryPath =  My.Application.Info.DirectoryPath & "\doc"
        End If

        Dim  HistoryFile As String = HistoryPath & "\FWhistory.txt"

        If System.IO.File.Exists(HistoryFile) = True Then
            Process.Start(HistoryFile)
        Else
            MsgBox("File not existing:" & vbCrLf & HistoryFile, MsgBoxStyle.Exclamation,"Show History")
        End If
        
    End Sub

    Private Sub OpenDocumentationFolderToolStripMenuItem_Click_1(sender As Object, e As EventArgs) Handles OpenDocumentationFolderToolStripMenuItem.Click
        Dim DocPath As String = Nothing

        If Strings.Right(My.Application.Info.DirectoryPath, Len("\bin")) = "\bin" Then
            DocPath = Strings.Left(My.Application.Info.DirectoryPath, Len(My.Application.Info.DirectoryPath) - Len("\bin"))
        ElseIf Strings.Right(My.Application.Info.DirectoryPath, Len("\obj")) = "\obj" Then
            DocPath = Strings.Left(My.Application.Info.DirectoryPath, Len(My.Application.Info.DirectoryPath) - Len("\obj"))
        Else
            DocPath = My.Application.Info.DirectoryPath ' & "\doc"
        End If

        'Dim HistoryFile As String = HistoryPath & "\history.txt"
        Process.Start(DocPath & "\doc")
    End Sub

    Public Sub DoubleBufferedDGV(ByVal dgv As DataGridView, ByVal setting As Boolean)
        Dim dgvType As Type = dgv.[GetType]()
        Dim pi As PropertyInfo = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)
        pi.SetValue(dgv, setting, Nothing)
    End Sub

    Private Sub ctxtmnuItemPaste_Click(sender As Object, e As EventArgs) Handles ctxtmnuItemPaste.Click
        PasteFromClipboard
    End Sub

    Private Sub cmdInitButton_Click(sender As Object, e As EventArgs) Handles cmdInitButton.Click
        Dim szErr As String
        szErr = CalibrateOptitrack()
        If Len(szErr) <> 0 Then MsgBox(szErr, MsgBoxStyle.Critical, "Calibrate Optitrack")
        Turntable.SetAngle(0)
    End Sub

    Private Sub cmdGenerateSOFA_Click(sender As Object, e As EventArgs) Handles cmdGenerateSOFA.Click
        frmGenerateSOFA.ShowDialog()
    End Sub

    Private Sub tmrTracker_Tick(sender As Object, e As EventArgs) Handles tmrTracker.Tick
        Dim tsData As TrackerSensor
        Dim szErr As String
        szErr = Tracker.GetCurrentValues(tmrTracker.Interval \ 2, 0, tsData)

        If Len(szErr) <> 0 Then GoTo SubTrackingError ' no tracking info received

        labelNcams.Text = TStr(tsData.nCameras)
        If tsData.nCameras >= 14 Then
            labelNcams.BackColor = Color.Lime
        ElseIf tsData.nCameras >= 3 Then
            labelNcams.BackColor = Color.Yellow
        Else
            labelNcams.BackColor = Color.Red
        End If

        If tsData.visible = False Then GoTo SubNotTracking

        labelTrackingYesNo.Text = "Yes"
        labelTrackingYesNo.BackColor = Color.Lime
        labelX.Text = TStr(Math.Round(tsData.sngX, 1))
        labelY.Text = TStr(Math.Round(tsData.sngY, 1))
        labelZ.Text = TStr(Math.Round(tsData.sngZ, 1))
        labelYaw.Text = TStr(Math.Round(tsData.sngA, 1))
        labelPitch.Text = TStr(Math.Round(tsData.sngE, 1))
        labelRoll.Text = TStr(Math.Round(tsData.sngR, 1))

        Return
SubTrackingError:
        labelNcams.BackColor = Color.Red
        labelNcams.Text = "0"
SubNotTracking:
        labelTrackingYesNo.BackColor = Color.Red
        labelTrackingYesNo.Text = "No"
        labelX.Text = "-"
        labelY.Text = "-"
        labelZ.Text = "-"
        labelYaw.Text = "-"
        labelPitch.Text = "-"
        labelRoll.Text = "-"
    End Sub

    Private Sub cmdShowPlots_Click(sender As Object, e As EventArgs) Handles cmdShowPlots.Click

        ' INISettings.WriteFile(STIM.WorkDir & "\" & settingsFile)
        ' ItemList.Save(STIM.WorkDir & "\" & itemListFile)
        cmdShowPlots.Enabled = False
        Result.QuickPlotIR()
        cmdShowPlots.Enabled = True

    End Sub

    Private Sub cmdInitialCheck_Click(sender As Object, e As EventArgs) Handles cmdInitialCheck.Click

        cmdInitialCheck.Enabled = False
        Result.InitialCheck()
        cmdInitialCheck.Enabled = True

    End Sub

    Private Sub cmdSanityCheck_Click(sender As Object, e As EventArgs) Handles cmdSanityCheck.Click

        cmdSanityCheck.Enabled = False
        Result.SanityCheck()
        cmdSanityCheck.Enabled = True

    End Sub

    Private Sub cmdTTsendTo0_Click(sender As Object, e As EventArgs) Handles cmdTTsendTo0.Click
        Dim ttSpeed_tmp As Double = ttSpeed
        ttSpeed = 4 ' temporarily increase speed for this move
        cmdSetTo0.Enabled = False
        cmdTTShow.Enabled = False
        Turntable.MoveToAngle(359) ' overshoot one degree
        ttSpeed = ttSpeed_tmp
        Turntable.MoveToAngle(0) ' then move back anticlockwise to reduce the play on the turntable gears
        cmdSetTo0.Enabled = True
        cmdTTShow.Enabled = True
    End Sub

    Private Sub cmdSetTo0_Click(sender As Object, e As EventArgs) Handles cmdSetTo0.Click
        Turntable.SetAngle(0)
    End Sub

    'Private Sub dgvItemList_KeyPress(sender As Object, e As KeyPressEventArgs) Handles dgvItemList.KeyPress

    '             'asdasd
    '            'Console.WriteLine("paste")
    'End Sub
End Class

Public Class DataGridViewRowHeaderCellCustom
    Inherits DataGridViewRowHeaderCell

    Protected Overrides Sub Paint(ByVal graphics As System.Drawing.Graphics, ByVal clipBounds As System.Drawing.Rectangle, _
                ByVal cellBounds As System.Drawing.Rectangle, ByVal rowIndex As Integer, _
                ByVal cellState As System.Windows.Forms.DataGridViewElementStates, _
                ByVal value As Object, ByVal formattedValue As Object, _
                ByVal errorText As String, ByVal cellStyle As System.Windows.Forms.DataGridViewCellStyle, _
                ByVal advancedBorderStyle As System.Windows.Forms.DataGridViewAdvancedBorderStyle, _
                ByVal paintParts As System.Windows.Forms.DataGridViewPaintParts)

        MyBase.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts)
        ' Create the text that is going to be displayed in the row header.   
        Dim rowNumStr As String = CStr(rowIndex + 1)

        ' Adjust the text layout rectangle to center the text vertically   
        ' within the cell and to move it slightly to the right for greater  
        ' visual appearence.  
        Dim ofs As Single = Convert.ToSingle(cellBounds.Height - cellStyle.Font.Height) / 2
        Dim layoutRect As RectangleF = cellBounds
        layoutRect.Inflate(0, -ofs)
        layoutRect.X += 5
        layoutRect.Width -= 5

        ' Draw the text using the Cell's Graphics object.  
        graphics.DrawString(rowNumStr, _
            cellStyle.Font, Brushes.Black, layoutRect)
    End Sub
End Class