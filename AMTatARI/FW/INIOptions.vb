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

Imports System.IO

''' <summary>
''' FrameWork - Handling of Options.
''' </summary>
''' <remarks>This module allows to use parameter files to read/write
''' global parameters used in a project.
''' Once defined all parameters, simple functions to
''' read and write the complete parameter files are provided.
'''
'''
''' <br>How to use:
'''  <li> complete all tasks marked as TODO in this module </li>
'''  <li> set all parameters to default values before reading the file
'''       (Form_Load is a good place to do that task) </li>
'''  <li> to read the parameter file call ReadFile "myparameterfile.ini" </li>
'''  <li> to write the parameter file call WriteFile "myparameterfile.ini" </li>
'''  <li> to search for a key call FindKey "myparameterfile.ini", "wantedkey" </li></br></remarks>
Module INIOptions

   

    ' global options
   ''' <summary>
    ''' COM port for the left RIB (Options/RIB)
    ''' </summary>
    ''' <remarks></remarks>
    Public glCOMLeft As Integer
   ''' <summary>
    ''' COM port for the right RIB (Options/RIB)
    ''' </summary>
    ''' <remarks></remarks>
    Public glCOMRight As Integer
   ''' <summary>
    ''' Filename of the RIB server application (Options/RIB)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszRIBServer As String
   ''' <summary>
    ''' Simulate the RIB server? (Options/RIB)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnRIBSimulation As Boolean
    ''' <summary>
    ''' Simulate the RIB2 device? (Options/RIB2)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnRIB2Simulation As Boolean
    ''' <summary>
    ''' Current active directory (not available in any form)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszCurrentDir As String
    ''' <summary>
    ''' Position/size of the main window
    ''' </summary>
    ''' <remarks></remarks>
    Public grectMain As RECT
    ''' <summary>
    ''' Position/size of the Settings dialog.
    ''' </summary>
    ''' <remarks></remarks>
    Public grectSettings As RECT
    ''' <summary>
    ''' Position/size of the Options dialog.
    ''' </summary>
    ''' <remarks></remarks>
    Public grectOptions As RECT
    ''' <summary>
    ''' Position/size of the Level Dancer dialog.
    ''' </summary>
    ''' <remarks></remarks>
    Public grectLevelDancer As RECT
    ''' <summary>
    ''' Position/size of the Fitt4Fun dialog.
    ''' </summary>
    ''' <remarks></remarks>
    Public grectFitt4Fun As RECT
    ''' <summary>
    ''' Position/size of the Results dialog.
    ''' </summary>
    ''' <remarks></remarks>
    Public grectResults As RECT
    ''' <summary>
    ''' Use Beeps in Experiment (Options/General)
    ''' </summary>
    ''' <remarks></remarks>
    Public glFlagBeepExp As Integer
    ''' <summary>
    ''' Play wave file in break or when finishing experiment (Options/General)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnPlayWaveExp As Boolean
    ''' <summary>
    ''' Auto Backup Log File? (Options/General)
    ''' </summary>
    ''' <remarks>Automatically backup (save) log file when disconnecting</remarks>
    Public gblnAutoBackupLogFile As Boolean
        ''' <summary>
    ''' Auto Backup Log File? (Options/General)
    ''' </summary>
    ''' <remarks>Automatically backup (save) log file when disconnecting, without a file dialogue</remarks>
    Public gblnAutoBackupLogFileSilent As Boolean
    ''' <summary>
    ''' Auto Backup Item List? (Options/General)
    ''' </summary>
    ''' <remarks>Automatically backup (save) item list in output folder</remarks>
    Public gblnAutoBackupItemList As Boolean
    ''' <summary>
    ''' Use File Naming rules? See: File naming system.sxw (Options/General)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnUseFileNaming As Boolean
    ''' <summary>
    ''' Stimulus Flags (Show Stimulus or Matlab/ShowStimulus.m)
    ''' </summary>
    ''' <remarks></remarks>
    Public glShowStimulusFlags As Integer
    ''' <summary>
    ''' Axes Parameters (Show Stimulus or Matlab/ShowStimulus.m)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszShowStimulusAxes As String
    ''' <summary>
    ''' Parameters (Show Stimulus or Matlab/ShowStimulus.m)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszShowStimulusParameter As String
    ''' <summary>
    ''' User post fix of the item list (frmItemListPostFix, appears on Save Item List)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszItemListPostFix As String
    ''' <summary>
    ''' Flags of the post fix naming of the item list (frmItemListPostFix, appears on Save Item List)
    ''' </summary>
    ''' <remarks></remarks>
    Public glFileNamingFlags As Integer
    ''' <summary>
    ''' Warning Switches (Options/General)
    ''' </summary>
    ''' <remarks></remarks>
    Public glWarningSwitches As FWintern.WarningSwitches
    ''' <summary>
    ''' Disable automatic setting of column width
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnDisableSetOptimalColWidth As Boolean
    ''' <summary>
    ''' Enable automatic check for updates when starting program
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnCheckForUpdates As Boolean
    ''' <summary>
    ''' Interval to check for updates (days)
    ''' </summary>
    ''' <remarks></remarks>
    Public gUpdateInterval As Integer
    ''' <summary>
    ''' Last check for updates
    ''' </summary>
    ''' <remarks></remarks>
    Public gLastUpdateCheck As String
    ''' <summary>
    ''' Full path and file name of options file
    ''' </summary>
    ''' <remarks></remarks>
    Public gszOptionsFile As String
    ''' <summary>
    ''' Priority Index
    ''' </summary>
    ''' <remarks></remarks>
    Public glPriority As Integer
    ' STIM
    ''' <summary>
    ''' Mode of logging the experiment (Options/STIM)
    ''' </summary>
    ''' <remarks></remarks>
    Public glLogMode As Integer
    ''' <summary>
    ''' Use MATLAB?
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnUseMATLAB As Boolean

    ''' <summary>
    ''' Path to MATLAB scripts
    ''' </summary>
    ''' <remarks></remarks>
    Public gszMATLABServer As String
    ''' <summary>
    ''' Path to MATLAB scripts
    ''' </summary>
    ''' <remarks></remarks>
    Public gszMATLABPath As String
    ''' <summary>
    ''' Path to RIB files (Options/RIB)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszRIBPath As String
    ''' <summary>
    ''' Device name for IO-card for RIB2 (Options/RIB2)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszRIB2DeviceName As String
    ''' <summary>
    ''' Path to RIB2 files (Options/RIB2)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszRIB2Path As String
    ''' <summary>
    ''' Path to NIC files (Options/NIC)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszNICPath As String
    ''' <summary>
    ''' Show Dynamic Range in LevelDancer
    ''' </summary>
    ''' <remarks></remarks>
    Public gbShowDR As Boolean

    ' Audio - Pd
    ''' <summary>
    ''' IP Address of the application from the point of view of YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public gszLocalNetAddr As String
    ''' <summary>
    ''' Port of the application from the point of view of YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public glLocalNetPort As Integer
    ''' <summary>
    ''' IP Address of YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public gszPlayerNetAddr As String
    ''' <summary>
    ''' Port of YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public glPlayerNetPort As Integer
    ''' <summary>
    ''' File name of the YAMI application (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public gszPlayerFileName As String
    ''' <summary>
    ''' Flags of YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public glPlayerFlags As Output.PLAYERFLAGS
    ''' <summary>
    ''' Index of the audio device used by YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public glPlayerAudioDevice As Integer
    ''' <summary>
    ''' Index of the audio ADC (input) device used by YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public glPlayerADCAudioDevice As Integer
    ''' <summary>
    ''' Number of channels used by YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public glPlayerChannels As Integer
    ''' <summary>
    ''' Index of MIDI Out Device used by YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public glPlayerMIDIOutDevice As Integer
    ''' <summary>
    ''' Index of MIDI In Device used by YAMI (Options/Audio (Pd))
    ''' </summary>
    ''' <remarks></remarks>
    Public glPlayerMIDIInDevice As Integer
    ''' <summary>
    ''' Headphones: Left channel (pd)
    ''' </summary>
    ''' <remarks></remarks>
    Public glPlayerHPLeft As Integer
    ''' <summary>
    ''' Headphones: Right channel (pd)
    ''' </summary>
    ''' <remarks></remarks>
    Public glPlayerHPRight As Integer
    ''' <summary>
    ''' Audio channel used for data transmission
    ''' </summary>
    ''' <remarks></remarks>
    Public glDataChannel As Integer
    ''' <summary>
    ''' Audio channel used for (onset) trigger playback
    ''' </summary>
    ''' <remarks></remarks>
    Public glTriggerChannel As Integer
    ''' <summary>
    ''' Audio DAC device name, alternative to index
    ''' </summary>
    ''' <remarks></remarks>
    Public gszDACName As String
    ''' <summary>
    ''' Audio ADC device name, alternative to index
    ''' </summary>
    ''' <remarks></remarks>
    Public gszADCName As String
    ''' <summary>
    ''' Use audio device NAMES, not audio device index
    ''' </summary>
    ''' <remarks></remarks>
    Public gbAudioName As Boolean

    ' Audio - Unity
    ''' <summary>
    ''' Port of the application from the point of view of Unity (Options/Audio (Unity))
    ''' </summary>
    ''' <remarks></remarks>
    Public glUnityLocalNetPort As Integer
    ''' <summary>
    ''' IP Address of Unity (Options/Audio (Unity))
    ''' </summary>
    ''' <remarks></remarks>
    Public gszUnityNetAddr As String
    ''' <summary>
    ''' Port of Unity (Options/Audio (Unity))
    ''' </summary>
    ''' <remarks></remarks>
    Public glUnityNetPort As Integer
 
    ' CSV Export/Import
    ''' <summary>
    ''' Delimiter between arguments in a CSV file (Options/CSV Export)
    ''' </summary>
    ''' <remarks></remarks>
    Public glCSVDelimiter As Integer
    ''' <summary>
    ''' Quota for arguments containing delimiter in a CSV file (Options(CSV Export)
    ''' </summary>
    ''' <remarks></remarks>
    Public glCSVQuota As Integer
    ''' <summary>
    ''' Include Headers when copying cells from datagridview
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnIncludeHeadersInClipboard As Boolean

    ' Tracker
    ''' <summary>
    ''' Tracker Mode:
    '''  0 = deactivated
    '''  1 = YAMI
    '''  2 = ViWo (Unity)
    ''' </summary>
    ''' <remarks></remarks>
    Public glTrackerMode As Integer
    ''' <summary>
    ''' COM port for tracker (Options/Tracker). 0=deactivated
    ''' </summary>
    ''' <remarks></remarks>
    Public glTrackerCOM As Integer
    ''' <summary>
    ''' Tracker: Baudrate of the communication (Options/Tracker)
    ''' </summary>
    ''' <remarks></remarks>
    Public glTrackerBaudrate As Integer
    ''' <summary>
    ''' Tracker: Number of sensors connected available (Options/Tracker)
    ''' </summary>
    ''' <remarks></remarks>
    Public glTrackerSensorCount As Integer
    ''' <summary>
    ''' Simulate tracker? (Options/Tracker)
    ''' Available in YAMI tracker only.
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnTrackerSimulation As Boolean
    ''' <summary>
    ''' Tracker: Interval of data update in the Settings/Tracker form (Options/Tracker)
    ''' </summary>
    ''' <remarks></remarks>
    Public glTrackerSettingsInterval As Integer
    'Public gblnTrackerInWiVo As Boolean

    ' Turntable
    ''' <summary>
    ''' Turntable Mode. 0: no turntable; 1: Four Audio ANT turntable; 2: Outline ST2 turntable
    ''' </summary>
    ''' <remarks></remarks>
    public glTTMode As Integer
    ''' <summary>
    ''' LPT (Printer port) for Turntable (Options/Turntable). 0=deactivated
    ''' </summary>
    ''' <remarks></remarks>
    Public glTTLPT As Integer
    ''' <summary>
    ''' Offset of the turntable Outline ST2, this azimuth corresponds to real zero (Options/Turntable)
    ''' </summary>
    ''' <remarks></remarks>
    Public gsngTTOffset As Double
    ''' <summary>
    ''' Resolution of the Turntable (n.a. in a form, hardware depended)
    ''' </summary>
    ''' <remarks></remarks>
    Public gsngTTResolution As Double
    ''' <summary>
    ''' Offset of the turntable Four Audio ANT, this azimuth corresponds to real zero (can be redefined)
    ''' </summary>
    ''' <remarks></remarks>
    Public gsngTT4AOffset As Double
    ''' <summary>
    ''' Timer for turntable Four Audio ANT, xxx seconds after the last movement the brake is pulled. -1: disabled, 0: immediatly
    ''' </summary>
    ''' <remarks></remarks>
    Public glTT4ABrakeTimer As Integer
    ''' <summary>
    ''' Allow pre-rotation for turntable Four Audio ANT, in order to send the rotation command to turntable already during previous item, since the command takes about 1 second before the turntable starts rotating
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnAllowPreRotation As Boolean

    ' ViWo - Virtual World
    ''' <summary>
    ''' IP Address (or name) of the computer running ViWo
    ''' </summary>
    ''' <remarks></remarks>
    Public gszViWoAddress As String
    ''' <summary>
    ''' Port of the service listening on the computer running ViWo
    ''' </summary>
    ''' <remarks></remarks>
    Public glViWoPort As Integer

    ' Joypad
    ''' <summary>
    ''' Class with information about a Joypad
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure JoyPad
        Friend Description As String
        Friend ButtonCount As Integer
        Friend ResponseCodes() As Integer
    End Structure
    Public JoyPads() As JoyPad

    ' This function is used to parse a parameter line and
    ' store the value in a parameter - you will Get (a) Parameter.
    ' It is a private function, DON'T CALL IT (use ReadFile instead)
    '
    ' ----------------------------------------------
    ' Use the form:
    '    case "parameter name"
    '      MyParameter = Convert(szValue)
    '
    ' parameter name: the name of parameter, must be lower case!
    ' MyParameter:    the name of the variable storing the parameter
    ' Convert:        since szValue is a string you need a conversion
    '                 function to get the right format of your parameter
    ' szValue:        contains the value of parameter in string form
    '
    Private Sub GetParameter(ByVal szName As String, ByVal szValue As String)

        Select Case szName ' the name is a lower case string!
            Case "com left"
                glCOMLeft = CInt(Val(szValue))
            Case "com right"
                glCOMRight = CInt(Val(szValue))
            Case "ribserver"
                gszRIBServer = szValue
            Case "rib simulation"
                gblnRIBSimulation = CBool(Val(szValue))
            Case "rib2 simulation"
                gblnRIB2Simulation = CBool(Val(szValue))
            Case "current directory"
                gszCurrentDir = szValue
            Case "main window left"
                grectMain.Left = CInt(Val(szValue))
            Case "main window width"
                grectMain.Width = CInt(Val(szValue))
            Case "main window top"
                grectMain.Top = CInt(Val(szValue))
            Case "main window height"
                grectMain.Height = CInt(Val(szValue))
            Case "main window state"
                grectMain.WindowState = CType(CInt(Val(szValue)), FormWindowState)
            Case "options window left"
                grectOptions.Left = CInt(Val(szValue))
            Case "options window width"
                grectOptions.Width = CInt(Val(szValue))
            Case "options window top"
                grectOptions.Top = CInt(Val(szValue))
            Case "options window height"
                grectOptions.Height = CInt(Val(szValue))
            Case "settings window left"
                grectSettings.Left = CInt(Val(szValue))
            Case "settings window width"
                grectSettings.Width = CInt(Val(szValue))
            Case "settings window top"
                grectSettings.Top = CInt(Val(szValue))
            Case "settings window height"
                grectSettings.Height = CInt(Val(szValue))
            Case "level dancer window left"
                grectLevelDancer.Left = CInt(Val(szValue))
            Case "level dancer window width"
                grectLevelDancer.Width = CInt(Val(szValue))
            Case "level dancer window top"
                grectLevelDancer.Top = CInt(Val(szValue))
            Case "level dancer window height"
                grectLevelDancer.Height = CInt(Val(szValue))
            Case "fitt4fun window left"
                grectFitt4Fun.Left = CInt(Val(szValue))
            Case "fitt4fun window width"
                grectFitt4Fun.Width = CInt(Val(szValue))
            Case "fitt4fun window top"
                grectFitt4Fun.Top = CInt(Val(szValue))
            Case "fitt4fun window height"
                grectFitt4Fun.Height = CInt(Val(szValue))
            Case "results window left"
                grectResults.Left = CInt(Val(szValue))
            Case "results window width"
                grectResults.Width = CInt(Val(szValue))
            Case "results window top"
                grectResults.Top = CInt(Val(szValue))
            Case "results window height"
                grectResults.Height = CInt(Val(szValue))
            Case "auto backup log file"
                gblnAutoBackupLogFile = CBool(Val(szValue))
            Case "auto backup log file without dialogue"
                gblnAutoBackupLogFileSilent = CBool(Val(szValue))
            Case "auto backup item list"
                gblnAutoBackupItemList = CBool(Val(szValue))
            Case "show stimulus flags"
                glShowStimulusFlags = CInt(Val(szValue))
            Case "show stimulus axes"
                gszShowStimulusAxes = szValue
            Case "show stimulus parameter"
                gszShowStimulusParameter = szValue
            Case "use file naming"
                gblnUseFileNaming = CBool(Val(szValue))
            Case "beep in experiment flags"
                glFlagBeepExp = CInt(Val(szValue))
            Case "play wave after experiment"
                gblnPlayWaveExp = CBool(Val(szValue))
            Case "item list postfix"
                gszItemListPostFix = szValue
            Case "file naming flags"
                glFileNamingFlags = CInt(Val(szValue))
            Case "warning switches"
                glWarningSwitches = CType(Val(szValue), FWintern.WarningSwitches)
            Case "disable set optimal col width"
                gblnDisableSetOptimalColWidth = CBool(Val(szValue))
            Case "priority"
                glPriority = CInt(Val(szValue)) ' CInt(Val(cmbPriority.SelectedIndex))
            Case "check for updates"
                gblnCheckForUpdates = CBool(Val(szValue))
            Case "update interval"
                gUpdateInterval = CInt(Val(szValue))
            Case "last update check"
                gLastUpdateCheck = szValue
                ' STIM
            Case "log mode"
                glLogMode = CInt(Val(szValue))
            Case "include headers in clipboard"
                gblnIncludeHeadersInClipboard = CBool(Val(szValue))
            Case "matlab server"
                gszMATLABServer = szValue
            Case "matlab path"
                gszMATLABPath = szValue
            Case "rib path"
                gszRIBPath = szValue
            Case "rib2 path"
                gszRIB2Path = szValue
            Case "rib2 device name"
                gszRIB2DeviceName = szValue
            Case "nic path"
                gszNICPath = szValue
            Case "show dr"
                gbShowDR = CBool(Val(szValue))

                ' Audio (Pd)
            Case "local net address"
                gszLocalNetAddr = szValue
            Case "player net address"
                gszPlayerNetAddr = szValue
            Case "local net port"
                glLocalNetPort = CInt(Val(szValue))
            Case "player net port"
                glPlayerNetPort = CInt(Val(szValue))
            Case "player file name"
                gszPlayerFileName = szValue
            Case "player flags"
                glPlayerFlags = CType(Val(szValue), Output.PLAYERFLAGS)
            Case "player audio device"
                glPlayerAudioDevice = CInt(Val(szValue))
                glPlayerADCAudioDevice = CInt(Val(szValue)) 'compatibility with old options
            Case "player adc audio device"
                glPlayerADCAudioDevice = CInt(Val(szValue))
            Case "player channels"
                glPlayerChannels = Math.Min( CInt(Val(szValue)),PLAYER_MAXCHANNELS)
                glOutputPlay = New BitArray(glPlayerChannels)
            Case "player midi out device"
                glPlayerMIDIOutDevice = CInt(Val(szValue))
            Case "player midi in device"
                glPlayerMIDIInDevice = CInt(Val(szValue))
            Case "player headphones left"
                glPlayerHPLeft = CInt(Val(szValue))
            Case "player headphones right"
                glPlayerHPRight = CInt(Val(szValue))
            Case "data channel"
                glDataChannel = CInt(Val(szValue))
            Case "trigger channel"
                glTriggerChannel = CInt(Val(szValue))
            Case "player dac name"
                gszDACName = szValue
            Case "player adc name"
                gszADCName = szValue
            Case "use audio device name"
                gbAudioName = CBool(Val(szValue))

                ' Audio (Unity)
            Case "unity net address"
                gszUnityNetAddr = szValue
            Case "unity local net port"
                glUnityLocalNetPort = CInt(Val(szValue))
            Case "unity net port"
                glUnityNetPort = CInt(Val(szValue))

                ' CSV Export/Import
            Case "csv delimiter"
                glCSVDelimiter = CInt(Val(szValue))
            Case "csv quota"
                glCSVQuota = CInt(Val(szValue))
            Case "use matlab"
                gblnUseMATLAB = CBool(Val(szValue))
                ' Tracker
            Case "tracker mode"
                glTrackerMode = CInt(Val(szValue))
            Case "tracker com port"
                glTrackerCOM = CInt(Val(szValue))
            Case "tracker baudrate"
                glTrackerBaudrate = CInt(Val(szValue))
            Case "tracker sensor count"
                glTrackerSensorCount = CInt(Val(szValue))
            Case "tracker simulation"
                gblnTrackerSimulation = CBool(Val(szValue))
            Case "tracker settings interval"
                glTrackerSettingsInterval = CInt(Val(szValue))
            Case "motive project file"
                motiveFile = szValue
            Case "motive osc streamer executable file"
                OSCstreamerFile = szValue
            Case "motive udp port"
                motiveUDPport = CInt(Val(szValue))
                'Case "tracker in viwo"
                '    gblnTrackerInWiVo = CBool(Val(szValue))
                ' Turntable
            Case "turntable mode"
                glTTMode = CInt(Val(szValue))
            Case "turntable lpt"
                glTTLPT = CInt(Val(szValue))
            Case "turntable offset"
                gsngTTOffset = Val(szValue)
            Case "turntable resolution"
                gsngTTResolution = Val(szValue)
            Case "turntable four audio offset"
                gsngTT4AOffset = Val(szValue)
            Case "turntable four audio brake timer"
                glTT4aBrakeTimer = cint(Val(szValue))
            Case "turntable four audio prerotation"
                gblnAllowPreRotation = CBool(Val(szValue))
            Case "turntable ip address"
                ttAddress = szValue
            Case "turntable port"
                ttPort = CInt(Val(szValue))
            Case "turntable speed"
                ttSpeed = Val(szValue)

                ' ViWo
            Case "viwo address"
                gszViWoAddress = szValue
            Case "viwo port"
                glViWoPort = CInt(Val(szValue))
                'Case "viwo osc"
                'gbViWoOSC = CBool(Val(szValue))
                ' Joypad
            Case "joypad"
                Dim csvX As New CSVParser
                Dim szArr(,) As String = Nothing
                csvX.ParseString(szValue, szArr)
                Dim szDesc As String = szArr(0, 0)
                Dim lCnt As Integer = 0
                ' search for the same joypad
                If Not IsNothing(JoyPads) Then
                    For lCnt = 0 To JoyPads.Length - 1
                        If szDesc = JoyPads(lCnt).Description Then Exit For
                    Next
                    If lCnt > JoyPads.Length - 1 Then ReDim Preserve JoyPads(lCnt)
                Else
                    ReDim JoyPads(0)
                End If
                With JoyPads(lCnt)
                    .Description = szDesc
                    .ButtonCount = CInt(Val(szArr(0, 1)))
                    ReDim .ResponseCodes(szArr.GetLength(1) - 3)
                    For lX As Integer = 0 To .ResponseCodes.Length - 1
                        If Len(szArr(0, lX + 2)) = 0 Then
                            .ResponseCodes(lX) = -1
                        Else
                            .ResponseCodes(lX) = CInt(Val(szArr(0, lX + 2)))
                        End If
                    Next
                End With
                'RemoteMonitor
            Case "remote monitor server address"
                gszRemoteMonitorServerAdress = szValue
            Case "remote monitor server enabled"
                gblnRemoteMonitorServerEnabled = CBool(Val(szValue))
            Case Else
                ' unknown parameter found - do something...
        End Select

    End Sub

    ' This function is used to save parameters in the
    ' options file.
    ' It is a private function, DON'T CALL IT (use WriteFile instead)
    '
    ' ----------------------------------------------
    ' Use the form:
    '    WriteLine "Parameter Name", Convert(MyParameter)
    '
    ' Parameter Name: the name of parameter, may be mixed-case and
    ' MyParameter:    the name of the variable storing the parameter
    ' Convert:        since the second parameter of WriteLine must be
    '                 a string you need a conversion function to get
    '                 the right format from your parameter
    Private Sub WriteAllParameters()

        WriteLine("Current Directory", gszCurrentDir)
        WriteLine("Main Window Left", TStr(grectMain.Left))
        WriteLine("Main Window Width", TStr(grectMain.Width))
        WriteLine("Main Window Top", TStr(grectMain.Top))
        WriteLine("Main Window Height", TStr(grectMain.Height))
        WriteLine("Main Window State", TStr(frmMain.WindowState))
        WriteLine("Options Window Left", TStr(grectOptions.Left))
        WriteLine("Options Window width", TStr(grectOptions.Width))
        WriteLine("Options Window Top", TStr(grectOptions.Top))
        WriteLine("Options Window Height", TStr(grectOptions.Height))
        WriteLine("Settings Window Left", TStr(grectSettings.Left))
        WriteLine("Settings Window Width", TStr(grectSettings.Width))
        WriteLine("Settings Window Top", TStr(grectSettings.Top))
        WriteLine("Settings Window Height", TStr(grectSettings.Height))
        WriteLine("Level Dancer Window Top", TStr(grectLevelDancer.Top))
        WriteLine("Level Dancer Window Height", TStr(grectLevelDancer.Height))
        WriteLine("Level Dancer Window Left", TStr(grectLevelDancer.Left))
        WriteLine("Level Dancer Window Width", TStr(grectLevelDancer.Width))
        WriteLine("Fitt4Fun Window Top", TStr(grectFitt4Fun.Top))
        WriteLine("Fitt4Fun Window Height", TStr(grectFitt4Fun.Height))
        WriteLine("Fitt4Fun Window Left", TStr(grectFitt4Fun.Left))
        WriteLine("Fitt4Fun Window Width", TStr(grectFitt4Fun.Width))
        WriteLine("Results Window Top", TStr(grectResults.Top))
        WriteLine("Results Window Height", TStr(grectResults.Height))
        WriteLine("Results Window Left", TStr(grectResults.Left))
        WriteLine("Results Window Width", TStr(grectResults.Width))
        WriteLine("Auto Backup Log File", TStr(CInt(gblnAutoBackupLogFile)))
        WriteLine("Auto Backup Log File Without Dialogue", TStr(CInt(gblnAutoBackupLogFileSilent)))
        WriteLine("Auto Backup Item List", TStr(CInt(gblnAutoBackupItemList)))
        WriteLine("Show Stimulus Flags", TStr(glShowStimulusFlags))
        If gszShowStimulusAxes <> "" Then WriteLine("Show Stimulus Axes", gszShowStimulusAxes)
        If gszShowStimulusParameter <> "" Then WriteLine("Show Stimulus Parameter", gszShowStimulusParameter)
        WriteLine("Use File Naming", TStr(CInt(gblnUseFileNaming)))
        WriteLine("Beep in Experiment Flags", TStr(glFlagBeepExp))
        WriteLine("Play Wave after Experiment", TStr(CInt(gblnPlayWaveExp)))
        WriteLine("Item List Postfix", gszItemListPostFix)
        WriteLine("File Naming Flags", TStr(glFileNamingFlags))
        WriteLine("Warning Switches", TStr(glWarningSwitches))
        WriteLine("Priority", TStr(glPriority))
        WriteLine("Disable Set Optimal Col Width", TStr(CInt(gblnDisableSetOptimalColWidth)))
        WriteLine("Check for Updates", TStr(CInt(gblnCheckForUpdates)))
        WriteLine("Update Interval", TStr(CInt(gUpdateInterval)))
        If gLastUpdateCheck <> "" Then WriteLine("Last Update Check", gLastUpdateCheck)
        ' RIB
        WriteLine("COM Left", TStr(glCOMLeft))
        WriteLine("COM Right", TStr(glCOMRight))
        WriteLine("RIBServer", gszRIBServer)
        WriteLine("RIB Simulation", TStr(CInt(gblnRIBSimulation)))
        WriteLine("RIB Path", gszRIBPath)
        ' RIB2
        WriteLine("RIB2 Simulation", TStr(CInt(gblnRIB2Simulation)))
        WriteLine("RIB2 Device Name", gszRIB2DeviceName)
        WriteLine("RIB2 Path", gszRIB2Path)
        ' NIC
        WriteLine("NIC Path", gszNICPath)
        ' Level Dancer
        WriteLine("Show DR", TStr(CInt(gbShowDR)))
        ' Audio (Pd)
        WriteLine("Local Net Address", gszLocalNetAddr)
        WriteLine("Local Net Port", TStr(glLocalNetPort))
        WriteLine("Player Net Address", gszPlayerNetAddr)
        WriteLine("Player Net Port", TStr(glPlayerNetPort))
        WriteLine("Player File Name", gszPlayerFileName)
        WriteLine("Player Flags", TStr(glPlayerFlags))
        WriteLine("Player Audio Device", TStr(glPlayerAudioDevice))
        WriteLine("Player ADC Audio Device", TStr(glPlayerADCAudioDevice))
        WriteLine("Player Channels", TStr(glPlayerChannels))
        WriteLine("Player MIDI Out Device", TStr(glPlayerMIDIOutDevice))
        WriteLine("Player MIDI In Device", TStr(glPlayerMIDIInDevice))
        WriteLine("Player Headphones Left", TStr(glPlayerHPLeft))
        WriteLine("Player Headphones Right", TStr(glPlayerHPRight))
        WriteLine("Data Channel", TStr(glDataChannel))
        WriteLine("Trigger Channel", TStr(glTriggerChannel))
        If gszDACName <> "" Then WriteLine("Player DAC Name", gszDACName)
        If gszADCName <> "" Then WriteLine("Player ADC Name", gszADCName)
        WriteLine("Use Audio Device Name", TStr(CInt(gbAudioName)))

        ' Audio (Unity)
        WriteLine("Unity Net Address", gszUnityNetAddr)
        WriteLine("Unity Net Port", TStr(glUnityNetPort))
        WriteLine("Unity Local Net Port", TStr(glUnityLocalNetPort))


        'WriteLine("Player OSC", TStr(CInt(gbPlayerOSC)))
        ' Stim
        WriteLine("Log Mode", TStr(glLogMode))
        WriteLine("Use MATLAB", TStr(CInt(gblnUseMATLAB)))
        If gszMATLABServer <> "" Then WriteLine("MATLAB Server", gszMATLABServer)
        WriteLine("MATLAB Path", gszMATLABPath)
        ' CSV Export/Import
        WriteLine("CSV Delimiter", TStr(glCSVDelimiter))
        WriteLine("CSV Quota", TStr(glCSVQuota))
        WriteLine("Include Headers in Clipboard", TStr(CInt(gblnIncludeHeadersInClipboard)))
        ' Tracker
        WriteLine("Tracker Mode", TStr(glTrackerMode))
        WriteLine("Tracker COM Port", TStr(glTrackerCOM))
        WriteLine("Tracker Baudrate", TStr(glTrackerBaudrate))
        WriteLine("Tracker Sensor Count", TStr(glTrackerSensorCount))
        WriteLine("Tracker Simulation", TStr(CInt(gblnTrackerSimulation)))
        WriteLine("Tracker Settings Interval", TStr(glTrackerSettingsInterval))
        WriteLine("Motive Project File", motiveFile)
        WriteLine("Motive OSC Streamer Executable File", OSCstreamerFile)
        WriteLine("Motive UDP Port", TStr(motiveUDPport))
        'WriteLine("Tracker in ViWo", TStr(CInt(gblnTrackerInWiVo)))
        ' turntable
        WriteLine("Turntable Mode", TStr(glttMode))
        WriteLine("Turntable LPT", TStr(glTTLPT))
        WriteLine("Turntable Offset", TStr(gsngTTOffset))
        WriteLine("Turntable Resolution", TStr(gsngTTResolution))
        WriteLine("Turntable Four Audio Offset", TStr(gsngTT4AOffset))
        WriteLine("Turntable Four Audio Brake Timer", TStr(glTT4aBrakeTimer))
        WriteLine("Turntable Four Audio PreRotation", TStr(CInt(gblnAllowPreRotation)))
        WriteLine("Turntable IP Address", ttAddress)
        WriteLine("Turntable Port", TStr(ttPort))
        WriteLine("Turntable Speed", TStr(ttSpeed))

        ' ViWo
        If gszViWoAddress <> "" Then WriteLine("ViWo Address", gszViWoAddress)
        WriteLine("ViWo Port", TStr(glViWoPort))
        'WriteLine("ViWo OSC", TStr(CInt(gbViWoOSC)))
        ' Joypad
        For Each joyX As JoyPad In JoyPads
            Dim csvX As New CSVParser
            Dim szX As String = csvX.QuoteCell(joyX.Description) & csvX.Separator & csvX.QuoteCell(TStr(joyX.ButtonCount))
            For Each lX As Integer In joyX.ResponseCodes
                If lX = -1 Then
                    szX = szX & csvX.Separator
                Else
                    szX = szX & csvX.Separator & csvX.QuoteCell(TStr(lX))
                End If
            Next
            WriteLine("Joypad", szX)
        Next
        ' RemoteMonitor
        WriteLine("Remote Monitor Server Enabled", TStr(CInt(gblnRemoteMonitorServerEnabled)))
        WriteLine("Remote Monitor Server Address", gszRemoteMonitorServerAdress)
    End Sub

    ''' <summary>
    ''' Clear option parameters and set to default values.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ClearParameters()
        glCOMLeft = 1
        glCOMRight = 2
        gszRIBServer = "Rib32.exe"
        gszExpID = My.Application.Info.AssemblyName
        gblnRIBSimulation = False
        gblnRIB2Simulation = False
        'gszCurrentDir = My.Application.Info.DirectoryPath
        gszCurrentDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\" & My.Application.Info.Title & "\Test"
        'grectMain.Left = 0
        'grectMain.Top = 0
        'grectOptions.Left = 0
        'grectOptions.Top = 0
        'grectSettings.Left = 0
        'grectSettings.Top = 0
        gblnAutoBackupLogFile = False
        gblnAutoBackupLogFileSilent = False
        gblnAutoBackupItemList = True
        glFlagBeepExp = 0
        gblnPlayWaveExp = False
        gblnUseFileNaming = True
        gszItemListPostFix = "test"
        glFileNamingFlags = 1
        glWarningSwitches = CType(FWintern.WarningSwitches.wsExpPerformedOnShuffle + FWintern.WarningSwitches.wsNotRepOnExpStart + FWintern.WarningSwitches.wsNotShuffledOnExpStart + FWintern.WarningSwitches.wsResponseItemListOnExpRep, FWintern.WarningSwitches)
        glShowStimulusFlags = 0
        gszShowStimulusAxes = ""
        gszShowStimulusParameter = ""
        glPriority = 0
        gblnDisableSetOptimalColWidth = False
        gblnCheckForUpdates = False
        gUpdateInterval = 7
        'gLastUpdateCheck = System.DateTime.Now.ToString("yyyyMMdd")
        ' STIM
        glLogMode = 13
        gblnUseMATLAB = True
        gszMATLABServer = ""
        gszMATLABPath = "Matlab"
        gszRIBPath = "RIB"
        gszRIB2Path = "RIB2"
        'gszRIB2Path = "..\_FW\RIB2"
        gszRIB2DeviceName = "Dev1"
        gszNICPath = "NIC"
        ' Level Dancer
        gbShowDR = False
        ' Audio (Pd)
        gszLocalNetAddr = "localhost"
        glLocalNetPort = 10001
        gszPlayerNetAddr = "localhost"
        glPlayerNetPort = 10003
        glPlayerChannels = PLAYER_MAXCHANNELS
        If PLAYER_MAXCHANNELS <= 2 Then
            gszPlayerFileName = "C:\pd\YAMI\YAMI2.bat"
        Else
            gszPlayerFileName = "C:\pd\YAMI\YAMI.bat"
        End If
        glPlayerFlags = 0
        glPlayerAudioDevice = 0
        glPlayerADCAudioDevice = 0
        glPlayerHPLeft = 1
        glPlayerHPRight = 2
        glDataChannel = 0
        glTriggerChannel = 0
        'gbPlayerOSC = False

        ' Audio (Unity)
        gszUnityNetAddr = "localhost"
        glUnityNetPort = 10013
        glUnityLocalNetPort = 10011

        ' csv export/import
        glCSVDelimiter = Asc(",")
        glCSVQuota = Asc("""")
        gblnIncludeHeadersInClipboard=True

        ' tracker
        glTrackerMode = 0
        glTrackerCOM = 1
        glTrackerBaudrate = 57600
        glTrackerSensorCount = 1
        gblnTrackerSimulation = True
        glTrackerSettingsInterval = 100
        'gblnTrackerInWiVo = False
        ' turntable
        glTTMode = 0
        glTTLPT = 0
        gsngTTOffset = 0
        gsngTTResolution = 2.5
        gsngTT4AOffset = 0
        glTT4aBrakeTimer=10
        gblnAllowPreRotation=False

        ' ViWo
        gszViWoAddress = ""
        glViWoPort = 10007
        'gbViWoOSC = False
        ' Joypad
        ReDim JoyPads(0)
        JoyPads(0).ButtonCount = 9
        JoyPads(0).Description = "WingMan Action Pad"
        ReDim JoyPads(0).ResponseCodes(9 + 4 - 1)
        For lX As Integer = 0 To 9 - 1
            JoyPads(0).ResponseCodes(lX) = lX
        Next
        JoyPads(0).ResponseCodes(9) = -2    'X+
        JoyPads(0).ResponseCodes(10) = -3   'X-
        JoyPads(0).ResponseCodes(11) = -4   'Y+
        JoyPads(0).ResponseCodes(12) = -5   'Y-

        ' RemoteMonitor
        gblnRemoteMonitorServerEnabled = False
        gszRemoteMonitorServerAdress = My.Computer.Name
        'WriteLine("Remote Monitor Server Enabled", TStr(CInt(gblnRemoteMonitorServerEnabled)))
        'WriteLine("Remote Monitor Server Address", gszHostConnected)


    End Sub

    Public Sub WriteFile(ByVal szFileName As String)
        Dim lX As Integer

        ' Open File
        On Error GoTo WriteFile_Error
        If Dir(szFileName) <> "" Then Kill(szFileName)

        ' Check if environment folder existing, otherwise create it
        If Dir(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\" & My.Application.Info.Title, vbDirectory) = vbNullString Then
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\" & My.Application.Info.Title)
        End If

        FileOpen(1, szFileName, OpenMode.Binary)
        ' Write Parameter
        WriteAllParameters()
        ' Close file
        FileClose(1)
        On Error GoTo 0
        Exit Sub

WriteFile_Error:
        MsgBox("Error " & Err.Description & vbCrLf & " while writing options file:" & vbCrLf & szFileName, MsgBoxStyle.Critical, My.Application.Info.Title)
        FileClose(1)
        Exit Sub
    End Sub

    Public Sub ReadFile(ByVal szFileName As String)
        Dim szTemp As String
        Dim bX As Byte
        Dim szTemplate As String = Nothing

        gszOptionsFile = szFileName

        ' Ini-Datei vorhanden ???
        If Dir(szFileName) = "" Then ' options file not existing

            szTemplate = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\Template.ini"

            If Dir(szTemplate) <> "" AndAlso MsgBox("Options file" & vbCrLf & gszOptionsFile & vbCrLf & "couldn't be found." & vbCrLf & vbCrLf & "Do you want to create the new options file based the template options file?" & vbCrLf & szTemplate & vbCrLf & vbCrLf & "If not, default values are used.", _
                    MsgBoxStyle.Question Or MsgBoxStyle.YesNo, My.Application.Info.Title) = MsgBoxResult.Yes Then ' Template file existing

                szFileName = szTemplate
                frmMain.SetStatus("Options template file loaded: " & szFileName)
                'MsgBox("A new options file was created from the template options file:" & vbCrLf & szFileName & vbCrLf & vbCrLf & _
                '    "Please check the options if they meet all requirements for your application!", _
                '    MsgBoxStyle.Information, My.Application.Info.Title)
            Else
                If Dir(szTemplate) = "" Then MsgBox("PLEASE READ CAREFULLY:" & vbCrLf & vbCrLf & _
                    "Options file " & gszOptionsFile & vbCrLf & "couldn't be found. A new file was created." & vbCrLf & vbCrLf & _
                    "Please check the options, the default values are used!", _
                    MsgBoxStyle.Information, My.Application.Info.Title)
                WriteFile(gszOptionsFile) ' create options ini file (because not existing)
                frmMain.SetStatus("Options file created: " & gszOptionsFile)
            End If
        Else ' options file existing
            frmMain.SetStatus("Options file loaded: " & gszOptionsFile)
        End If
        ' Open file

        Try
            FileOpen(1, szFileName, OpenMode.Binary)
        Catch
            If MsgBox("Options file " & szFileName & " cannot be opened, maybe because of insufficient permissions! Default Options are loaded but changes cannot be saved!" & vbCrLf & vbCrLf & _
                       "Do you want to open the folder to change the file permissions or manually delete file?", MsgBoxStyle.YesNo Or MsgBoxStyle.Critical, My.Application.Info.Title) = vbYes Then
                Shell("explorer " & Chr(34) & Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) & "\ExpSuite\" & My.Application.Info.Title, AppWinStyle.NormalFocus)
            End If
            FileClose(1)
            Exit Sub
        End Try
        'On Error GoTo ReadFile_Error
        Try
            Do  ' Read new line
                szTemp = ""
                Do
                    FileGet(1, bX)
                    If bX <> 13 Then
                        szTemp = szTemp & Chr(bX)
                    End If
                Loop Until (bX = 13) Or EOF(1)
                ParseLine(szTemp)
                szTemp = ""
                If Not EOF(1) Then
                    FileGet(1, bX) ' LF holen
                End If
            Loop Until EOF(1)
        Catch
            MsgBox("Options file " & szFileName & " is corrupt. Please check the options before you continue!", MsgBoxStyle.Critical, My.Application.Info.Title)
        End Try

        FileClose(1)

        If Not (szTemplate = Nothing) AndAlso szFileName = szTemplate Then
            WriteFile(gszOptionsFile) ' create options ini file (because template was loaded)
            frmMain.SetStatus("Options file created: " & gszOptionsFile)
        End If

        'On Error GoTo 0
        Exit Sub

        'ReadFile_Error:
        '        MsgBox("Error " & Err.Description & vbCrLf & "while reading options file:" & vbCrLf & szFileName, MsgBoxStyle.Critical, My.Application.Info.Title)
        '        FileClose(1)
        '        Exit Sub
    End Sub

    Private Sub ParseLine(ByVal szData As String)
        Dim lX As Integer
        Dim szName As String
        Dim szValue As String
        ' parse a line to get the parameter name and its value
        ' Use INI-format: name=value
        lX = InStr(szData, "=")
        If lX <> 0 Then
            ' = gefunden
            szName = Left(szData, lX - 1)
            szValue = Mid(szData, lX + 1)
            szName = LCase(szName)
            szName = RTrim(szName)
            szValue = LTrim(szValue)
            GetParameter(szName, szValue)
        End If
    End Sub

    Private Sub WriteLine(ByVal szName As String, ByVal varValue As String)
        Dim lX As Integer
        Dim szData As String

        szData = szName + "=" + varValue

        For lX = 1 To Len(szData)
            FilePut(1, CByte(Asc(Mid(szData, lX, 1))))
        Next
        FilePut(1, CByte(13))
        FilePut(1, CByte(10))
    End Sub

End Module