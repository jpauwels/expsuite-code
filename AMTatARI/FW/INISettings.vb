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
''' <summary>
''' FrameWork - Handling of Settings.
''' </summary>
''' <remarks>
''' This module allows to use parameter files to read/write
''' settings used in a project.
''' Once defined all parameters, simple functions to
''' read and write the complete parameter files are provided.
'''
''' <br>Author: Piotr Majdak</br>
'''
''' <br>How to use:
'''  <li> complete all tasks marked as TODO in this module </li>
'''  <li> set all parameters to default values before reading the file </li>
'''       (Form_Load is a good place to do that task)
'''  <li> to read the parameter file call ReadFile "myparameterfile.ini" </li>
'''  <li> to write the parameter file call WriteFile "myparameterfile.ini" </li>
'''  <li> to search for a key call FindKey "myparameterfile.ini", "wantedkey" </li></br></remarks>
Module INISettings
   

    ' frame work
   ''' <summary>
    ''' Source Directory of fitting files (Settings/Fitting Left)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszSourceDir As String
   ''' <summary>
    ''' Root Directory (Settings/General/Output directory)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszDestinationDir As String
   ''' <summary>
    ''' Create a new work directory? (Settings/General/Output directory)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnNewWorkDir As Boolean
   ''' <summary>
    ''' Silent Mode (=no logging to files or list, logging mode will be 0)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnSilentMode As Boolean
   ''' <summary>
    ''' File name of the fitting file left (Settings/Fitting Left)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszFittFileLeft As String
   ''' <summary>
    ''' File name of the fitting file right (Settings/Fitting Right)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszFittFileRight As String
    ''' <summary>
    ''' Left implant used (RIB2)
    ''' </summary>
    ''' <remarks></remarks>
    Public glImpLeft As Integer
    ''' <summary>
    ''' Right implant used (RIB2)
    ''' </summary>
    ''' <remarks></remarks>
    Public glImpRight As Integer
    ''' <summary>
    ''' Do not connect to device (no pd player for acoustical stimulation and no RIB/NIC server for electrical stimulation)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnDoNotConnectToDevice As Boolean
   ''' <summary>
    ''' Position and size of the experimental form (Settings/Experiment Screen)
    ''' </summary>
    ''' <remarks></remarks>
    Public grectExp As RECT
   ''' <summary>
    ''' Use the windows temporary directory as the root directory? (Settings/General/Output directory)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnDestinationDir As Boolean
   ''' <summary>
    ''' Description of the experiment (Settings/Description)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszDescription As String
   ''' <summary>
    ''' The ID of the experiment (Settings/Description)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszExpID As String
   ''' <summary>
    ''' The title (file name without path) of the item list linked to the settings (Main Window/General information)
    ''' </summary>
    ''' <remarks></remarks>
    Public gszItemListTitle As String
   ''' <summary>
    ''' Show the experiment form alwyas on the top? (Settings/Experiment Screen)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnExpOnTop As Boolean
    ''' <summary>
    ''' Override experiment mode with a different mode
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnOverrideExpMode As Boolean
    ''' <summary>
    ''' Override experiment mode with mode...
    ''' </summary>
    ''' <remarks></remarks>
    Public gOExpMode As Integer
   ''' <summary>
    ''' How to present the stimuli? (Settings/General)
    ''' </summary>
    ''' <remarks></remarks>
    Public gStimOutput As GENMODE
   ''' <summary>
    ''' Synthesizer parameters (Settings/Audio)
    ''' </summary>
    ''' <remarks></remarks>
    Public gAudioSynth(1) As AudioSynth
   ''' <summary>
    ''' Connect a synthesizer to an output channel (Settings/Audio)
    ''' </summary>
    ''' <remarks></remarks>
    Public glAudioDACAddStream(PLAYER_MAXCHANNELS - 1) As Integer
   ''' <summary>
    ''' Break after a given interval (Settings/Procedure)
    ''' </summary>
    ''' <remarks></remarks>
    Public glBreakInterval As Integer
   ''' <summary>
    ''' Flags to specify the break interval (Settings/Procedure)
    ''' </summary>
    ''' <remarks><list>
    ''' <item>0: no break (minutes)</item>
    ''' <item>1: break (minutes)</item>
    ''' <item>2: no break (items)</item>
    ''' <item>3: break (items)</item>
    ''' <item>4: no break (precent)</item>
    ''' <item>5: break (precent)</item>
    ''' </list></remarks>
    Public glBreakFlags As Integer
   ''' <summary>
    ''' Start item of the experiment
    ''' </summary>
    ''' <remarks></remarks>
    Public glExperimentStartItem As Integer
   ''' <summary>
    ''' End item of the experiment
    ''' </summary>
    ''' <remarks></remarks>
    Public glExperimentEndItem As Integer


    ' electrical/audio framework
   ''' <summary>
    ''' Selected electrode left (Settings/Signal/[Electrode | Acoust. Channel])
    ''' </summary>
    ''' <remarks></remarks>
    Public glElectrodeL As Integer
   ''' <summary>
    ''' Selected electrode right (Settings/Signal/[Electrode | Acoust. Channel])
    ''' </summary>
    ''' <remarks></remarks>
    Public glElectrodeR As Integer
   ''' <summary>
    ''' Parameters of defined electrodes or acoust. channels (Setting/Signal)
    ''' </summary>
    ''' <remarks></remarks>
    Public gfreqParL() As clsFREQUENCY
    ''' <summary>
    ''' Parameters of defined electrodes or acoust. channels (Setting/Signal)
    ''' </summary>
    ''' <remarks></remarks>
    Public gfreqParR() As clsFREQUENCY

    ' experiment variables and constants
   ''' <summary>
    ''' All experimental variables (Settings/Variables)
    ''' </summary>
    ''' <remarks></remarks>
    Public gvarExp() As ExpVariable
   ''' <summary>
    ''' All experimental constants (Settings/Constants)
    ''' </summary>
    ''' <remarks></remarks>
    Public gconstExp() As ExpConstant

   ''' <summary>
    ''' Interstimulus break (Settings/Procedure)
    ''' </summary>
    ''' <remarks></remarks>
    Public glInterStimBreak As Integer
   ''' <summary>
    ''' Selected experiment type (Settings/Procedure)
    ''' </summary>
    ''' <remarks></remarks>
    Public glExpType As Integer
   ''' <summary>
    ''' Selected Human Interface Device (Settings/Experiment Screen)
    ''' </summary>
    ''' <remarks></remarks>
    Public glExpHUIID As Integer
   ''' <summary>
    ''' Experiment Flags (Settings/Experiment Screen)
    ''' </summary>
    ''' <remarks></remarks>
    Public glExpFlags As frmExp.EXPFLAGS
   ''' <summary>
    ''' Stimulus Offset Left (Settings/Procedure)
    ''' </summary>
    ''' <remarks></remarks>
    Public glOffsetL As Integer
   ''' <summary>
    ''' Stimulus Offset Right (Settings/Procedure)
    ''' </summary>
    ''' <remarks></remarks>
    Public glOffsetR As Integer
   ''' <summary>
    ''' Repetitions of a block of items (Settings/Procedure)
    ''' </summary>
    ''' <remarks></remarks>
    Public glRepetition As Integer
   ''' <summary>
    ''' Sampling rate of the audio device (Settings/Audio)
    ''' </summary>
    ''' <remarks></remarks>
    Public glSamplingRate As Integer
   ''' <summary>
    ''' Resolution (quantization) of the audio output signal (Settings/Audio)
    ''' </summary>
    ''' <remarks></remarks>
    Public glResolution As Integer
   ''' <summary>
    ''' Fade in every audio signal (Settings/Audio)
    ''' </summary>
    ''' <remarks></remarks>
    Public gsFadeIn As Double
   ''' <summary>
    ''' Fade out every audio signal (Settings/Audio)
    ''' </summary>
    ''' <remarks></remarks>
    Public gsFadeOut As Double
    ''' <summary>
    ''' Use audio channel to transmit data
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnUseDataChannel As Boolean
    ''' <summary>
    ''' Use audio channel to transmit (onset) trigger signal
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnUseTriggerChannel As Boolean
   ''' <summary>
    ''' Prestimulus break (Settings/Procedure)
    ''' </summary>
    ''' <remarks></remarks>
    Public glPreStimBreak As Integer
   ''' <summary>
    ''' Prestimulus Visual Offset (Settings/Procedure)
    ''' </summary>
    ''' <remarks></remarks>
    Public glPreStimVisu As Integer
   ''' <summary>
    ''' Poststimulus Visual Offset (Settings/Procedure)
    ''' </summary>
    ''' <remarks></remarks>
    Public glPostStimVisu As Integer


    ' tracker
   ''' <summary>
    ''' Use tracker? (Settings/Tracker)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnTrackerUse As Boolean
   ''' <summary>
    ''' Repetition Rate of the tracker data (Settings/Tracker)
    ''' </summary>
    ''' <remarks></remarks>
    Public glTrackerRepRate As Integer
   ''' <summary>
    ''' Position Scaling (max. range) of the tracker data (Settings/Tracker)
    ''' </summary>
    ''' <remarks></remarks>
    Public gsngTrackerPosScaling As Double
   ''' <summary>
    ''' Minimum ranges of a tracker sensor (Settings/Tracker/Click on a position/angle)
    ''' </summary>
    ''' <remarks></remarks>
    Public gtsTrackerMin(1) As TrackerSensor
   ''' <summary>
    ''' Maximum ranges of a tracker sensor (Settings/Tracker/Click on a position/angle)
    ''' </summary>
    ''' <remarks></remarks>
    Public gtsTrackerMax(1) As TrackerSensor
   ''' <summary>
    ''' Offset of a tracker sensor (Settings/Tracker/Set Offset)
    ''' </summary>
    ''' <remarks></remarks>
    Public gtsTrackerOffset(1) As TrackerSensor
   ''' <summary>
    ''' Default Values of a tracker sensor (Settings/Tracker/Set Values)
    ''' </summary>
    ''' <remarks></remarks>
    Public gtsTrackerValues(1) As TrackerSensor

    ' turntable
   ''' <summary>
    ''' Use Turntable? (Settings/General/Turntable)
    ''' </summary>
    ''' <remarks></remarks>
    Public gblnTTUse As Boolean

    ' ViWo - Virtual World
   ''' <summary>
    ''' Averaging window for the data of the head sensor
    ''' </summary>
    ''' <remarks></remarks>
    Public gsngViWoAvgHead As Double
   ''' <summary>
    ''' Averaging window for the data of the pointer sensor
    ''' </summary>
    ''' <remarks></remarks>
    Public gsngViWoAvgPointer As Double
   ''' <summary>
    ''' Name of the selected world.
    ''' </summary>
    ''' <remarks></remarks>
    Public gszViWoWorld As String


    ' Level Dancer constants
    Public gblnLDDelayed As Boolean
    Public gblnLDLeftFirst As Boolean
    Public glLDStimLength As Integer
    Public gsLDPulsePeriodL As Double
    Public gsLDPulsePeriodR As Double

    ' ElectricVocoder option constants
    Public voc As String
    Public divFactor As Double
    Public srateAcoustic As Integer

    ' Fitt4Fun constants
    'Public glF4FStimLength As Integer  ' LD used
    'Public gsF4FPulsePeriod As Double  ' LD used

    Private Declare Function GetDC Lib "user32" Alias "GetDC" (ByVal hWnd As IntPtr) As IntPtr
    Private Declare Function GetDeviceCaps Lib "gdi32" Alias "GetDeviceCaps" (ByVal hDC As IntPtr, ByVal nIndex As Integer) As Integer

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
        'Dim szArr() As String
        Dim szX As String
        Dim lX As Integer

        Select Case szName ' the name is a lower case string!
            ' general parameters
            Case "framework version"
                Dim szArr() As String = Split(szValue, ".")
                If szArr.Length >= 2 AndAlso _
                       (CInt(Val(szArr(0))) > 0 Or _
                        CInt(Val(szArr(0))) = 0 And CInt(Val(szArr(1))) >= 8) Then gblnIsDotNETSetting = True
            Case "stimuli output"
                gStimOutput = CType(Val(szValue), ExpSuite.GENMODE)
            Case "destination dir"
                gszDestinationDir = szValue
            Case "create new work dir"
                gblnNewWorkDir = CBool(Val(szValue))
            Case "silent mode"
                gblnSilentMode = CBool(Val(szValue))
            Case "data dir"
                ddTemp.AddDir("", szValue) ' backwards compatibility to FW0.5
            Case "data directory"
                Dim szArr(0, 0) As String
                Dim csvX As New CSVParser
                csvX.Separator = "," : csvX.Quota = """"
                szX = szValue
                csvX.ParseString(szX, szArr)
                If szArr.Length = 1 Then ddTemp.AddDir(szArr(0, 0), "") Else ddTemp.AddDir(szArr(0, 0), szArr(0, 1))
                csvX = Nothing
            Case "destination directory active"
                gblnDestinationDir = CBool(Val(szValue))
            Case "do not connect to device"
                gblnDoNotConnectToDevice = CBool(Val(szValue))
            Case "experiment description"
                gszDescription = URLDecode(szValue)
            Case "experiment id"
                gszExpID = szValue
            Case "item list title"
                gszItemListTitle = szValue

                ' experiment screen
            Case "experiment window left"
                If Not gblnIsDotNETSetting Then
                    grectExp.Left = CInt(Val(szValue) * GetDeviceCaps(GetDC(frmMain.Handle), 88) / 1440)
                Else
                    grectExp.Left = CInt(Val(szValue))
                End If
            Case "experiment window width"
                If Not gblnIsDotNETSetting Then
                    grectExp.Width = CInt(Val(szValue) * GetDeviceCaps(GetDC(frmMain.Handle), 88) / 1440)
                Else
                    grectExp.Width = CInt(Val(szValue))
                End If
            Case "experiment window top"
                If Not gblnIsDotNETSetting Then
                    grectExp.Top = CInt(Val(szValue) * GetDeviceCaps(GetDC(frmMain.Handle), 90) / 1440)
                Else
                    grectExp.Top = CInt(Val(szValue))
                End If
            Case "experiment window height"
                If Not gblnIsDotNETSetting Then
                    grectExp.Height = CInt(Val(szValue) * GetDeviceCaps(GetDC(frmMain.Handle), 90) / 1440)
                Else
                    grectExp.Height = CInt(Val(szValue))
                End If
            Case "experiment window on top"
                gblnExpOnTop = CBool(Val(szValue))
            Case "override exp. mode"
                gblnOverrideExpMode = CBool(Val(szValue))
            Case "exp. mode"
                gOExpMode = CInt(Val(szValue))
            Case "experiment hui id"
                glExpHUIID = CInt(Val(szValue))
            Case "experiment screen flags"
                glExpFlags = CType(Val(szValue), frmExp.EXPFLAGS)

                ' fitting files
            Case "source dir"
                gszSourceDir = szValue
            Case "fitting file left"
                gszFittFileLeft = szValue
                glImpLeft = 1
            Case "fitting file right"
                gszFittFileRight = szValue
                glImpRight = 2

                ' signal - electrode arrays
            Case "frequency list left"
                Dim szArr() As String
                If IsNothing(gfreqParL) Then lX = 0 Else lX = gfreqParL.Length
                ReDim Preserve gfreqParL(lX)
                gfreqParL(lX) = New clsFREQUENCY
                szArr = Split(szValue, ";")
                If szArr.Length > -1 Then gfreqParL(lX).sAmp = Val(szArr(0))
                If szArr.Length > 0 Then gfreqParL(lX).lRange = CInt(Val(szArr(1)))
                If szArr.Length > 1 Then gfreqParL(lX).lPhDur = CInt(Val(szArr(2)))
                If szArr.Length > 2 Then gfreqParL(lX).sSPLOffset = Val(szArr(3))
                If szArr.Length > 3 Then gfreqParL(lX).sCenterFreq = Val(szArr(4))
                If szArr.Length > 4 Then gfreqParL(lX).sBandwidth = Val(szArr(5))
                If szArr.Length > 5 Then gfreqParL(lX).sTHR = Val(szArr(6))
                If szArr.Length > 6 Then gfreqParL(lX).sMCL = Val(szArr(7))
            Case "frequency list right"
                Dim szArr() As String
                If IsNothing(gfreqParR) Then lX = 0 Else lX = gfreqParR.Length
                ReDim Preserve gfreqParR(lX)
                gfreqParR(lX) = New clsFREQUENCY
                szArr = Split(szValue, ";")
                If szArr.Length > -1 Then gfreqParR(lX).sAmp = Val(szArr(0))
                If szArr.Length > 0 Then gfreqParR(lX).lRange = CInt(Val(szArr(1)))
                If szArr.Length > 1 Then gfreqParR(lX).lPhDur = CInt(Val(szArr(2)))
                If szArr.Length > 2 Then gfreqParR(lX).sSPLOffset = Val(szArr(3))
                If szArr.Length > 3 Then gfreqParR(lX).sCenterFreq = Val(szArr(4))
                If szArr.Length > 4 Then gfreqParR(lX).sBandwidth = Val(szArr(5))
                If szArr.Length > 5 Then gfreqParR(lX).sTHR = Val(szArr(6))
                If szArr.Length > 6 Then gfreqParR(lX).sMCL = Val(szArr(7))
            Case "electrode left"
                glElectrodeL = CInt(Val(szValue))
            Case "electrode right"
                glElectrodeR = CInt(Val(szValue))

                ' audio
            Case "audio synthesizer unit"
                Dim szArr() As String
                szArr = Split(szValue, ";")
                If Not IsNothing(szArr) Then lX = CInt(Val(szArr(0)))
                If szArr.Length > 0 Then gAudioSynth(lX).Signal = CInt(Val(szArr(1)))
                If szArr.Length > 1 Then gAudioSynth(lX).Par1 = Val(szArr(2))
                If szArr.Length > 2 Then gAudioSynth(lX).HighCut = Val(szArr(3))
                If szArr.Length > 3 Then gAudioSynth(lX).LowCut = Val(szArr(4))
                If szArr.Length > 4 Then gAudioSynth(lX).Vol = Val(szArr(5))
            Case "audio dac addstream"
                Dim szArr() As String
                szArr = Split(szValue, ";")
                For lX = 0 To Math.Min (UBound(szArr),ubound(glAudioDACAddStream)) ' do not load more channels than possible
                    glAudioDACAddStream(lX) = CInt(Val(szArr(lX)))
                    'If lX > ubound(glAudioDACAddStream)  then Exit For 
                Next
            Case "sampling rate"
                glSamplingRate = CInt(Val(szValue))
            Case "division factor"
                divFactor = CDbl(Val(szValue))
            Case "resolution"
                glResolution = CInt(Val(szValue))
            Case "fade in"
                gsFadeIn = Val(szValue)
            Case "fade out"
                gsFadeOut = Val(szValue)
            Case "use data channel"
                gblnUseDataChannel = CBool(Val(szValue))
            Case "use trigger channel"
                gblnUseTriggerChannel = CBool(Val(szValue))

                ' procedure constants
            Case "interstimulus break"
                glInterStimBreak = CInt(Val(szValue))
            Case "experiment type"
                glExpType = CInt(Val(szValue))
            Case "offset left"
                glOffsetL = CInt(Val(szValue))
            Case "offset right"
                glOffsetR = CInt(Val(szValue))
            Case "item repetition"
                glRepetition = CInt(Val(szValue))
            Case "prestimulus break"
                glPreStimBreak = CInt(Val(szValue))
            Case "prestimulus visual offset"
                glPreStimVisu = CInt(Val(szValue))
            Case "poststimulus visual offset"
                glPostStimVisu = CInt(Val(szValue))
            Case "break interval"
                glBreakInterval = CInt(Val(szValue))
            Case "break flags"
                glBreakFlags = CInt(Val(szValue))
            Case "experiment start item"
                If Not gblnIsDotNETSetting Then
                    glExperimentStartItem = CInt(Val(szValue) - 1)
                Else
                    glExperimentStartItem = CInt(Val(szValue))
                End If
                If glExperimentStartItem > glExperimentEndItem Then glExperimentEndItem = glExperimentStartItem
            Case "experiment end item"
                If Not gblnIsDotNETSetting Then
                    glExperimentEndItem = CInt(Val(szValue) - 1)
                Else
                    glExperimentEndItem = CInt(Val(szValue))
                End If
                If glExperimentEndItem < glExperimentStartItem Then
                    lX = glExperimentStartItem
                    glExperimentStartItem = glExperimentEndItem
                    glExperimentEndItem = lX
                End If
                If glExperimentEndItem = 0 And glExperimentStartItem = 0 Then
                    glExperimentEndItem = -1    ' backwards compat.
                    glExperimentStartItem = -1
                End If

                ' tracker
            Case "tracker use"
                gblnTrackerUse = CBool(Val(szValue))
            Case "tracker repetition rate"
                glTrackerRepRate = CInt(Val(szValue))
            Case "tracker position scaling"
                gsngTrackerPosScaling = Val(szValue)
            Case "tracker range enabled sensor 0 min"
                gtsTrackerMin(0).lStatus = CInt(Val(szValue))
            Case "tracker range enabled sensor 1 min"
                gtsTrackerMin(1).lStatus = CInt(Val(szValue))
            Case "tracker range enabled sensor 0 max"
                gtsTrackerMax(0).lStatus = CInt(Val(szValue))
            Case "tracker range enabled sensor 1 max"
                gtsTrackerMax(1).lStatus = CInt(Val(szValue))
            Case "tracker range sensor 0"
                Dim szArr() As String
                szArr = Split(szValue, ";")
                If szArr.Length > -1 Then gtsTrackerMin(0).sngX = Val(szArr(0))
                If szArr.Length > 0 Then gtsTrackerMax(0).sngX = Val(szArr(1))
                If szArr.Length > 1 Then gtsTrackerMin(0).sngY = Val(szArr(2))
                If szArr.Length > 2 Then gtsTrackerMax(0).sngY = Val(szArr(3))
                If szArr.Length > 3 Then gtsTrackerMin(0).sngZ = Val(szArr(4))
                If szArr.Length > 4 Then gtsTrackerMax(0).sngZ = Val(szArr(5))
                If szArr.Length > 5 Then gtsTrackerMin(0).sngA = Val(szArr(6))
                If szArr.Length > 6 Then gtsTrackerMax(0).sngA = Val(szArr(7))
                If szArr.Length > 7 Then gtsTrackerMin(0).sngE = Val(szArr(8))
                If szArr.Length > 8 Then gtsTrackerMax(0).sngE = Val(szArr(9))
                If szArr.Length > 9 Then gtsTrackerMin(0).sngR = Val(szArr(10))
                If szArr.Length > 10 Then gtsTrackerMax(0).sngR = Val(szArr(11))
            Case "tracker range sensor 1"
                Dim szArr() As String
                szArr = Split(szValue, ";")
                If szArr.Length > -1 Then gtsTrackerMin(1).sngX = Val(szArr(0))
                If szArr.Length > 0 Then gtsTrackerMax(1).sngX = Val(szArr(1))
                If szArr.Length > 1 Then gtsTrackerMin(1).sngY = Val(szArr(2))
                If szArr.Length > 2 Then gtsTrackerMax(1).sngY = Val(szArr(3))
                If szArr.Length > 3 Then gtsTrackerMin(1).sngZ = Val(szArr(4))
                If szArr.Length > 4 Then gtsTrackerMax(1).sngZ = Val(szArr(5))
                If szArr.Length > 5 Then gtsTrackerMin(1).sngA = Val(szArr(6))
                If szArr.Length > 6 Then gtsTrackerMax(1).sngA = Val(szArr(7))
                If szArr.Length > 7 Then gtsTrackerMin(1).sngE = Val(szArr(8))
                If szArr.Length > 8 Then gtsTrackerMax(1).sngE = Val(szArr(9))
                If szArr.Length > 9 Then gtsTrackerMin(1).sngR = Val(szArr(10))
                If szArr.Length > 10 Then gtsTrackerMax(1).sngR = Val(szArr(11))
            Case "tracker sensor 0 default values"
                Dim szArr() As String
                szArr = Split(szValue, ";")
                If szArr.Length > -1 Then gtsTrackerValues(0).sngX = Val(szArr(0))
                If szArr.Length > 0 Then gtsTrackerValues(0).sngY = Val(szArr(1))
                If szArr.Length > 1 Then gtsTrackerValues(0).sngZ = Val(szArr(2))
                If szArr.Length > 2 Then gtsTrackerValues(0).sngA = Val(szArr(3))
                If szArr.Length > 3 Then gtsTrackerValues(0).sngE = Val(szArr(4))
                If szArr.Length > 4 Then gtsTrackerValues(0).sngR = Val(szArr(5))
            Case "tracker sensor 1 default values"
                Dim szArr() As String
                szArr = Split(szValue, ";")
                If szArr.Length > -1 Then gtsTrackerValues(1).sngX = Val(szArr(0))
                If szArr.Length > 0 Then gtsTrackerValues(1).sngY = Val(szArr(1))
                If szArr.Length > 1 Then gtsTrackerValues(1).sngZ = Val(szArr(2))
                If szArr.Length > 2 Then gtsTrackerValues(1).sngA = Val(szArr(3))
                If szArr.Length > 3 Then gtsTrackerValues(1).sngE = Val(szArr(4))
                If szArr.Length > 4 Then gtsTrackerValues(1).sngR = Val(szArr(5))
            Case "tracker sensor 0 offset"
                Dim szArr() As String
                szArr = Split(szValue, ";")
                If szArr.Length > -1 Then gtsTrackerOffset(0).sngX = Val(szArr(0))
                If szArr.Length > 0 Then gtsTrackerOffset(0).sngY = Val(szArr(1))
                If szArr.Length > 1 Then gtsTrackerOffset(0).sngZ = Val(szArr(2))
                If szArr.Length > 2 Then gtsTrackerOffset(0).sngA = Val(szArr(3))
                If szArr.Length > 3 Then gtsTrackerOffset(0).sngE = Val(szArr(4))
                If szArr.Length > 4 Then gtsTrackerOffset(0).sngR = Val(szArr(5))
            Case "tracker sensor 1 offset"
                Dim szArr() As String
                szArr = Split(szValue, ";")
                If szArr.Length > -1 Then gtsTrackerOffset(1).sngX = Val(szArr(0))
                If szArr.Length > 0 Then gtsTrackerOffset(1).sngY = Val(szArr(1))
                If szArr.Length > 1 Then gtsTrackerOffset(1).sngZ = Val(szArr(2))
                If szArr.Length > 2 Then gtsTrackerOffset(1).sngA = Val(szArr(3))
                If szArr.Length > 3 Then gtsTrackerOffset(1).sngE = Val(szArr(4))
                If szArr.Length > 4 Then gtsTrackerOffset(1).sngR = Val(szArr(5))

                ' turntable
            Case "turntable use"
                gblnTTUse = CBool(Val(szValue))
                ' viWo
            Case "viwo average window head"
                gsngViWoAvgHead = Val(szValue)
            Case "viwo average window pointer"
                gsngViWoAvgPointer = Val(szValue)
            Case "viwo selected world"
                gszViWoWorld = szValue
            Case "viwo world parameter"
                Dim szArr(0, 0) As String
                Dim csvX As New CSVParser
                csvX.Separator = "," : csvX.Quota = """"
                csvX.ParseString(szValue, szArr)
                Dim viwoparX As New ViWoParameter
                viwoparX.Command = szArr(0, 0)
                viwoparX.Type = szArr(0, 1)
                viwoparX.MIDI = szArr(0, 2)
                viwoparX.Default = szArr(0, 3)
                viwoparX.Par1 = szArr(0, 4)
                viwoparX.Par2 = szArr(0, 5)
                If szArr.Length >= 7 Then viwoparX.Name = szArr(0, 6)
                If szArr.Length >= 8 Then viwoparX.Value = szArr(0, 7)
                ViWo.AddParameter(viwoparX)

                ' Level Dancer parameters
            Case "stimulate delayed", "level dancer stimulate delayed"    ' BLBManu
                gblnLDDelayed = CBool(Val(szValue))
            Case "stimulate left first", "level dancer stimulate left first" ' BLBManu
                gblnLDLeftFirst = CBool(Val(szValue))
            Case "level dancer stimulus length"
                glLDStimLength = CInt(Val(szValue))
            Case "level dancer pulse period left"
                gsLDPulsePeriodL = Val(szValue)
            Case "level dancer pulse period right"
                gsLDPulsePeriodR = Val(szValue)
            Case "appendpulsetrain"
                glAppendPulseTrainIndex = CInt(Val(szValue))
            Case "appendpulsetrain parameters"
                gszAppendPulseTrain = szValue

                ' Fitt4Fun parameters
            Case "fitt4fun stimulus length"
                'glF4FStimLength = CInt(Val(szValue))   ' LD used
            Case "fitt4fun pulse period"
                'gsF4FPulsePeriod = CInt(Val(szValue))  ' LD used

            Case Else
                ' variables
                If Not IsNothing(gvarExp) Then
                    For lX = 0 To gvarExp.Length - 1
                        With gvarExp(lX)
                            If szName = LCase(.szName) & " list" Then
                                ReDim Preserve .varValue(GetUbound(.varValue) + 1)
                                .varValue(UBound(.varValue)) = szValue
                            End If
                        End With
                    Next
                End If

                ' constants
                If GetUboundConstants() <> -1 Then
                    For lX = 0 To UBound(gconstExp)
                        With gconstExp(lX)
                            If szName = LCase(.szName) Then
                                .varValue = szValue
                            End If
                        End With
                    Next
                End If


        End Select

    End Sub

    ' This function is used to save all settings in the settings file.
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
        Dim lX, lY As Integer
        Dim szX As String

        ' frame work parameters
        WriteLine("Application Title", My.Application.Info.AssemblyName)
        WriteLine("Application Version", My.Application.Info.Version.Major & "." & _
                                         My.Application.Info.Version.Minor & "." & _
                                         My.Application.Info.Version.Build)
        WriteLine("Framework Version", TStr(FW_MAJOR) & "." & TStr(FW_MINOR) & "." & TStr(FW_REVISION))
        WriteLine("Computer Name", My.Computer.Name.ToString)
        WriteLine("Source Dir", gszSourceDir)
        WriteLine("Destination Dir", gszDestinationDir)
        WriteLine("Create New Work Dir", TStr(CInt(gblnNewWorkDir)))
        WriteLine("Silent Mode", TStr(CInt(gblnSilentMode)))
        For lX = 0 To DataDirectory.Count - 1
            Dim csvX As New CSVParser
            csvX.Quota = """" : csvX.Separator = ","
            szX = csvX.QuoteCell(DataDirectory.Title(lX)) & csvX.Separator & csvX.QuoteCell(DataDirectory.Path(lX))
            WriteLine("Data Directory", szX)
        Next
        WriteLine("Fitting File Left", gszFittFileLeft)
        WriteLine("Fitting File Right", gszFittFileRight)
        WriteLine("Destination Directory Active", TStr(CInt(gblnDestinationDir)))
        WriteLine("Do Not Connect To Device", TStr(CInt(gblnDoNotConnectToDevice)))
        WriteLine("Experiment Description", URLEncode(gszDescription))
        WriteLine("Experiment ID", gszExpID)
        WriteLine("Item List Title", gszItemListTitle)
        WriteLine("Experiment Window Left", TStr(grectExp.Left))
        WriteLine("Experiment Window Width", TStr(grectExp.Width))
        WriteLine("Experiment Window Top", TStr(grectExp.Top))
        WriteLine("Experiment Window Height", TStr(grectExp.Height))
        WriteLine("Experiment Window On Top", TStr(CInt(gblnExpOnTop)))
        WriteLine("Override Exp. Mode", TStr(CInt(gblnOverrideExpMode)))
        WriteLine("Exp. Mode", TStr(gOExpMode))
        For lX = 0 To 1
            With gAudioSynth(lX)
                szX = TStr(lX) & ";" & TStr(.Signal) & ";" & TStr(.Par1) & ";" & TStr(.HighCut) & ";" & TStr(.LowCut) & ";" & TStr(.Vol)
            End With
            WriteLine("Audio Synthesizer Unit", szX)
        Next
        szX = TStr(glAudioDACAddStream(0))
        For lX = 1 To PLAYER_MAXCHANNELS - 1 : szX = szX & ";" & TStr(glAudioDACAddStream(lX)) : Next
        WriteLine("Audio DAC AddStream", szX)
        WriteLine("Break Flags", TStr(glBreakFlags))
        WriteLine("Break Interval", TStr(glBreakInterval))
        WriteLine("Experiment Start Item", TStr(glExperimentStartItem))
        WriteLine("Experiment End Item", TStr(glExperimentEndItem))

        ' electrode arrays
        If Not IsNothing(gfreqParL) Then
            For lX = 0 To gfreqParL.Length - 1
                With gfreqParL(lX)
                    WriteLine("Frequency List Left", TStr(.sAmp) & ";" & TStr(.lRange) & ";" & TStr(.lPhDur) & ";" & TStr(.sSPLOffset) & ";" & TStr(.sCenterFreq) & ";" & TStr(.sBandwidth) & ";" & TStr(.sTHR) & ";" & TStr(.sMCL))
                End With
            Next
        End If
        If Not IsNothing(gfreqParR) Then
            For lX = 0 To gfreqParR.Length - 1
                With gfreqParR(lX)
                    WriteLine("Frequency List Right", TStr(.sAmp) & ";" & TStr(.lRange) & ";" & TStr(.lPhDur) & ";" & TStr(.sSPLOffset) & ";" & TStr(.sCenterFreq) & ";" & TStr(.sBandwidth) & ";" & TStr(.sTHR) & ";" & TStr(.sMCL))
                End With
            Next
        End If
        ' variables
        If GetUboundVariables() <> -1 Then
            For lX = 0 To UBound(gvarExp)
                With gvarExp(lX)
                    If GetUbound(.varValue) > -1 Then
                        For lY = 0 To GetUbound(.varValue)
                            WriteLine(.szName & " List", .varValue(lY))
                        Next
                    End If
                End With
            Next
        End If

        ' constants
        If GetUboundConstants() <> -1 Then
            For lX = 0 To UBound(gconstExp)
                With gconstExp(lX)
                    WriteLine(.szName, .varValue)
                End With
            Next
        End If

        ' procedure constants
        WriteLine("Interstimulus Break", TStr(glInterStimBreak))
        WriteLine("Experiment Type", TStr(glExpType))
        WriteLine("Experiment HUI ID", TStr(glExpHUIID))
        WriteLine("Experiment Screen Flags", TStr(glExpFlags))
        WriteLine("Offset Left", TStr(glOffsetL))
        WriteLine("Offset Right", TStr(glOffsetR))
        WriteLine("Item Repetition", TStr(glRepetition))
        WriteLine("Electrode Left", TStr(glElectrodeL))
        WriteLine("Electrode Right", TStr(glElectrodeR))
        WriteLine("Stimuli Creation", TStr(gStimOutput))        ' backwards comp. to vb6-apps
        WriteLine("Stimuli Output", TStr(gStimOutput))
        WriteLine("Sampling Rate", TStr(glSamplingRate))
        WriteLine("Division Factor", TStr(divFactor))
        WriteLine("Resolution", TStr(glResolution))
        WriteLine("Fade In", TStr(gsFadeIn))
        WriteLine("Fade Out", TStr(gsFadeOut))
        WriteLine("Use Data Channel", TStr(CInt(gblnUseDataChannel)))
        WriteLine("Use Trigger Channel", TStr(CInt(gblnUseTriggerChannel)))
        WriteLine("Prestimulus Break", TStr(glPreStimBreak))
        WriteLine("Prestimulus Visual Offset", TStr(glPreStimVisu))
        WriteLine("Poststimulus Visual Offset", TStr(glPostStimVisu))

        ' tracker
        WriteLine("Tracker Use", TStr(CInt(gblnTrackerUse)))
        WriteLine("Tracker Repetition Rate", TStr(glTrackerRepRate))
        WriteLine("Tracker Position Scaling", TStr(gsngTrackerPosScaling))
        WriteLine("Tracker Range Enabled Sensor 0 Min", TStr(gtsTrackerMin(0).lStatus))
        WriteLine("Tracker Range Enabled Sensor 1 Min", TStr(gtsTrackerMin(1).lStatus))
        WriteLine("Tracker Range Enabled Sensor 0 Max", TStr(gtsTrackerMax(0).lStatus))
        WriteLine("Tracker Range Enabled Sensor 1 Max", TStr(gtsTrackerMax(1).lStatus))
        For lX = 0 To 1
            szX = TStr(gtsTrackerMin(lX).sngX) & ";" & TStr(gtsTrackerMax(lX).sngX) & ";" & TStr(gtsTrackerMin(lX).sngY) & ";" & TStr(gtsTrackerMax(lX).sngY) & ";" & TStr(gtsTrackerMin(lX).sngZ) & ";" & TStr(gtsTrackerMax(lX).sngZ) & ";" & TStr(gtsTrackerMin(lX).sngA) & ";" & TStr(gtsTrackerMax(lX).sngA) & ";" & TStr(gtsTrackerMin(lX).sngE) & ";" & TStr(gtsTrackerMax(lX).sngE) & ";" & TStr(gtsTrackerMin(lX).sngR) & ";" & TStr(gtsTrackerMax(lX).sngR) & ";"
            WriteLine("Tracker Range Sensor " & TStr(lX), szX)
        Next
        For lX = 0 To 1
            szX = TStr(gtsTrackerValues(lX).sngX) & ";" & TStr(gtsTrackerValues(lX).sngY) & ";" & TStr(gtsTrackerValues(lX).sngZ) & ";" & TStr(gtsTrackerValues(lX).sngA) & ";" & TStr(gtsTrackerValues(lX).sngE) & ";" & TStr(gtsTrackerValues(lX).sngR)
            WriteLine("Tracker Sensor " & TStr(lX) & " Default Values", szX)
        Next
        For lX = 0 To 1
            szX = TStr(gtsTrackerOffset(lX).sngX) & ";" & TStr(gtsTrackerOffset(lX).sngY) & ";" & TStr(gtsTrackerOffset(lX).sngZ) & ";" & TStr(gtsTrackerOffset(lX).sngA) & ";" & TStr(gtsTrackerOffset(lX).sngE) & ";" & TStr(gtsTrackerOffset(lX).sngR)
            WriteLine("Tracker Sensor " & TStr(lX) & " Offset", szX)
        Next

        ' turntable
        WriteLine("Turntable Use", TStr(CInt(gblnTTUse)))

        ' viwo
        WriteLine("ViWo Average Window Head", TStr(gsngViWoAvgHead))
        WriteLine("ViWo Average Window Pointer", TStr(gsngViWoAvgPointer))
        WriteLine("ViWo Selected World", gszViWoWorld)
        For lX = 0 To ViWo.GetParametersCount - 1
            Dim csvX As New CSVParser
            csvX.Quota = """" : csvX.Separator = ","
            WriteLine("ViWo World Parameter", _
                    csvX.QuoteCell(gviwoparParameters(lX).Command) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Type) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).MIDI) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Default) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Par1) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Par2) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Name) & csvX.Separator & _
                    csvX.QuoteCell(gviwoparParameters(lX).Value))
        Next

        ' Level Dancer
        WriteLine("Level Dancer Stimulate Delayed", TStr(CInt(gblnLDDelayed)))
        WriteLine("Level Dancer Stimulate Left First", TStr(CInt(gblnLDLeftFirst)))
        WriteLine("Level Dancer Stimulus Length", TStr(glLDStimLength))
        WriteLine("Level Dancer Pulse Period Left", TStr(gsLDPulsePeriodL))
        WriteLine("Level Dancer Pulse Period Right", TStr(gsLDPulsePeriodR))
        WriteLine("AppendPulseTrain", TStr(glAppendPulseTrainIndex))
        WriteLine("AppendPulseTrain Parameters", gszAppendPulseTrain)

        ' Fitt4Fun parameters
        'WriteLine("Fitt4Fun Stimulus Length", TStr(glF4FStimLength))   ' LD used
        'WriteLine("Fitt4Fun Pulse Period", TStr(gsF4FPulsePeriod))     ' LD used
    End Sub

    ''' <summary>
    ''' Clear settings parameters and set to default values.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ClearParameters()
        Dim lX As Integer
        ' framework
        gszSourceDir = My.Application.Info.DirectoryPath
        gszDestinationDir = ""
        gblnNewWorkDir = True
        gblnSilentMode = False
        gblnDestinationDir = False
        gblnDoNotConnectToDevice = False
        For lX = 0 To DataDirectory.Count - 1
            DataDirectory.Path(lX) = ""
        Next
        gszFittFileLeft = ""
        gszFittFileRight = ""
        glImpLeft = 0
        glImpRight = 0
        grectExp.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Left
        grectExp.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Top
        grectExp.Height = 560
        grectExp.Width = 750
        'gszExpTitle = ""
        gszItemListTitle = ""
        gszDescription = My.Application.Info.AssemblyName
        gszSettingTitle = "Untitled." & My.Application.Info.AssemblyName
        gszSettingFileName = "Untitled." & My.Application.Info.AssemblyName
        gblnExpOnTop = True
        gblnOverrideExpMode = False
        gOExpMode = 0
        gStimOutput = GENMODE.genAcoustical
        glSamplingRate = 48000
        divFactor = 1.0
        glResolution = 24
        glExpType = 0
        glExpHUIID = 0
        glExpFlags = CType(frmExp.EXPFLAGS.expflFeedback + frmExp.EXPFLAGS.expflHighlight + frmExp.EXPFLAGS.expflWaitForNext + frmExp.EXPFLAGS.expflDelayRequest + frmExp.EXPFLAGS.expflWaitAfterBreak, frmExp.EXPFLAGS)
        For lX = 0 To 1
            With gAudioSynth(lX)
                .HighCut = 10050
                .LowCut = 50
                .Vol = -50
                .Signal = 0
                .Par1 = 1000
            End With
        Next
        For lX = 0 To PLAYER_MAXCHANNELS - 1 : glAudioDACAddStream(lX) = 0 : Next
        glBreakInterval = 30
        glBreakFlags = 2
        glElectrodeL = 1
        glElectrodeR = 1
        Erase gfreqParL
        Erase gfreqParR
        glExperimentStartItem = -1
        glExperimentEndItem = -1

        ' procedure constants
        glInterStimBreak = 200
        glRepetition = 1
        gsFadeIn = 0
        gsFadeOut = 0
        gblnUseDataChannel = False
        gblnUseTriggerChannel = False
        gblnAcceptSettings = False
        glPreStimBreak = 0
        glPreStimVisu = 0
        glPostStimVisu = 0

        ' tracker
        gblnTrackerUse = False
        glTrackerRepRate = 20
        gsngTrackerPosScaling = 36
        For lX = 0 To 1
            gtsTrackerMin(lX).sngX = -5
            gtsTrackerMax(lX).sngX = 5
            gtsTrackerMin(lX).sngY = -5
            gtsTrackerMax(lX).sngY = 5
            gtsTrackerMin(lX).sngZ = -5
            gtsTrackerMax(lX).sngZ = 5
            gtsTrackerMin(lX).sngA = -5
            gtsTrackerMax(lX).sngA = 5
            gtsTrackerMin(lX).sngE = -5
            gtsTrackerMax(lX).sngE = 5
            gtsTrackerMin(lX).sngR = -5
            gtsTrackerMax(lX).sngR = 5
            gtsTrackerMin(lX).lStatus = 0
            gtsTrackerMax(lX).lStatus = 0
        Next

        ' turntable
        'gblnTTUse = False
        If (glTTMode = 1 Or (glTTMode = 2 And glTTLPT > 0) Or glTTMode = 3) Then
            gblnTTUse = True
        Else
            gblnTTUse = False
        End If

        ' viwo
        gsngViWoAvgHead = 11
        gsngViWoAvgPointer = 21
        gszViWoWorld = VIWO_NOWORLD
        ViWo.ClearParameters()

        ' constants
        If Not IsNothing(gconstExp) Then
            For lX = 0 To UBound(gconstExp)
                gconstExp(lX).varValue = gconstExp(lX).varDefault
            Next
        End If
        ' variables
        If Not IsNothing(gvarExp) Then
            For lX = 0 To UBound(gvarExp)
                Erase gvarExp(lX).varValue
            Next
        End If

        ' Level Dancer 
        gblnLDDelayed = False
        gblnLDLeftFirst = True
        glLDStimLength = 500
        gsLDPulsePeriodL = 660
        gsLDPulsePeriodR = 660
        ' Fitt4Fun
        'glF4FStimLength = 500
        'gsF4FPulsePeriod = 2500

    End Sub

    Public Function WriteFile(ByVal szFileName As String) As String
        Dim lX As Integer

        ' Open File
        On Error GoTo WriteFile_Error
        If Dir(szFileName) <> "" Then Kill(szFileName)
        FileOpen(1, szFileName, OpenMode.Binary)
        ' Write Parameter
        WriteAllParameters()
        ' Close file
        FileClose(1)
        On Error GoTo 0
        Return ""

WriteFile_Error:
        FileClose(1)
        If Err.Number = 75 Then
            Return "Error: " & Err.Description & vbCrLf & "while writing settings file: " & vbCrLf & szFileName & vbCrLf & "Did you set it to read only?"
        Else
            Return "Error: " & Err.Description & vbCrLf & "while writing settings file: " & vbCrLf & szFileName & vbCrLf & _
            vbCrLf & "Error number: " & Err.Number.ToString
        End If
    End Function

    Public Function ReadFile(ByVal szFileName As String) As String
        Dim szTemp As String
        ' Ini-Datei vorhanden ???
        If Dir(szFileName) = "" Then
            Return "Settings file " & szFileName & vbCrLf & " couldn't be found. A new file was created." & vbCrLf & "All parameters are set to default values, check them for proper working."
        End If

        Dim file As System.IO.StreamReader = _
                My.Computer.FileSystem.OpenTextFileReader(szFileName, System.Text.Encoding.GetEncoding(1252))
        Do
            szTemp = file.ReadLine
            If Not IsNothing(szTemp) Then ParseLine(szTemp)
        Loop Until IsNothing(szTemp)
        Return ""

ReadFile_Error:
        Return "Error " & Err.Description & vbCrLf & "while reading settings file: " & vbCrLf & szFileName
    End Function

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

    Public Sub RemoteParseLine(ByVal szData As String)
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

        szData = szName & "=" + varValue

        For lX = 1 To Len(szData)
            FilePut(1, CByte(Asc(Mid(szData, lX, 1))))
        Next
        FilePut(1, CByte(13))
        FilePut(1, CByte(10))
    End Sub

    Public Function URLEncode(ByVal szText As String) As String
        Dim lX As Integer
        Dim szBuf As String
        szBuf = Replace(szText, "%", "%25")
        For lX = 1 To 255
            If (lX < 48 Or lX > 122) And lX <> 37 And lX <> 124 And lX <> 32 And CBool(InStr(szBuf, Chr(lX))) Then
                szBuf = Replace(szBuf, Chr(lX), "%" & Right("0" & Hex(lX), 2))
            End If
        Next
        szBuf = Replace(szBuf, " ", "+")
        Return szBuf
    End Function

    Private Function URLDecode(ByVal szText As String) As String
        Dim szBuf As String
        Dim lX As Integer
        szBuf = Replace(szText, "+", " ")
        lX = 1
        Do
            lX = InStr(lX, szBuf, "%")
            If lX = 0 Then Exit Do
            szBuf = Left(szBuf, lX - 1) & Chr(CInt("&H" & Mid(szBuf, lX + 1, 2))) & Mid(szBuf, lX + 3)
            lX = lX + 1
        Loop
        Return szBuf
    End Function

    Public Function FindParameter(ByVal szFileName As String, ByRef szKey As String) As String
        ' look for file
        If Dir(szFileName) = "" Then Return "File not found: " & szFileName
        ' Open file
        Dim file As System.IO.StreamReader = _
            My.Computer.FileSystem.OpenTextFileReader(szFileName, System.Text.Encoding.GetEncoding(1252))

        Dim szTemp As String
        Do
            szTemp = file.ReadLine
            If Not IsNothing(szTemp) Then
                ' search for a parameter
                Dim lX As Integer = InStr(szTemp, "=")
                If lX <> 0 Then
                    ' = found -> a parameter found
                    If szKey = RTrim(LCase(Left(szTemp, lX - 1))) Then
                        ' wanted key found -> get and exit
                        szKey = LTrim(Mid(szTemp, lX + 1))
                        Return ""
                    End If
                End If
            End If
        Loop Until IsNothing(szTemp)
        szKey = ""  ' nothing found
        Return ""

        Return "Error " & Err.Description & vbCrLf & "while reading settings file: " & vbCrLf & szFileName
    End Function

   ''' <summary>
    ''' Set the default values for new electrodes in Settings/Signal
    ''' </summary>
    ''' <param name="lCh">0 for the left electrodes, 1 for the right electrodes.</param>
    ''' <param name="sAmp">Default value for the field "Amplitude"</param>
    ''' <param name="sSPLOffset">Default value for the field "FS to SPL offset"</param>
    ''' <param name="sCenterFreq">Default value for the field "Center freq."</param>
    ''' <param name="sBandwidth">Default value for the field "Bandwidth"</param>
    ''' <param name="sTHR">Default value for the field "THR"</param>
    ''' <param name="sMCL">Default value for the field "MCL"</param>
    ''' <param name="lPhDur">Default value for the field "Phase dur." in �s. This value will be showed in samples and as the true quantized value. This value must be different than zero to set the default values as valid.</param>
    ''' <remarks>Separate default values can be set for the left and right electrodes.
    ''' SignalDefault can be called any time, e.g. after the exp. type changed. Then, if the user
    ''' click on "new electrode" the default values are applied.</remarks>
    Public Sub SignalDefault(ByVal lCh As Integer, ByVal sAmp As Single, ByVal sSPLOffset As Single, ByVal sCenterFreq As Single, ByVal sBandwidth As Single, ByVal sTHR As Single, ByVal sMCL As Single, ByVal lPhDur As Integer)

        If lCh < 0 Or lCh > 1 Then Err.Raise(0, , "lCh must be 0 (left) or 1 (right).")
        With gfreqDef(lCh)
            .lPhDur = lPhDur
            .lRange = 0
            .sAmp = sAmp
            .sBandwidth = sBandwidth
            .sCenterFreq = sCenterFreq
            .sMCL = sMCL
            .sSPLOffset = sSPLOffset
            .sTHR = sTHR
        End With
    End Sub
End Module