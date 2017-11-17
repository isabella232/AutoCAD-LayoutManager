﻿Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices

Public Class ucSettings
    Dim sIniDir As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\RNtools"
    Dim sIniFile As String = "\layoutmanager.ini"
    Dim iniFile As clsINI
    Dim sPDFuserFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    Dim sDefaultOutputLocation As String = ""
    Dim sCurrVersion As String = Assembly.GetExecutingAssembly().GetName().Version.ToString
    Dim sDefaultPlottingDevice As String = "AutoCAD PDF (General Documentation).PC3"
    Private Sub radioPDFuserFolder_CheckedChanged(sender As Object, e As EventArgs) Handles radioPDFuserFolder.CheckedChanged
        If radioPDFuserFolder.Checked Then
            txtUserSaveFolder.Enabled = True
            cmdSelectUserSaveFolder.Enabled = True
            sDefaultOutputLocation = "userlocation"
            saveDefaultOutput()
        Else
            txtUserSaveFolder.Enabled = False
            cmdSelectUserSaveFolder.Enabled = False
        End If
    End Sub


    Private Sub cmdSelectUserSaveFolder_Click(sender As Object, e As EventArgs) Handles cmdSelectUserSaveFolder.Click
        Using fldrDia As FolderBrowserDialog = New FolderBrowserDialog
            If txtUserSaveFolder.Text.Length > 0 And Directory.Exists(txtUserSaveFolder.Text) Then
                fldrDia.SelectedPath = txtUserSaveFolder.Text
            End If
            If fldrDia.ShowDialog() = DialogResult.OK Then
                'eerst check of map wel bestaat
                If My.Computer.FileSystem.DirectoryExists(sIniDir) = False Then
                    My.Computer.FileSystem.CreateDirectory(sIniDir)
                End If
                sPDFuserFolder = fldrDia.SelectedPath
                txtUserSaveFolder.Text = sPDFuserFolder
                'bestand aanmaken
                'settings schrijven
                iniFile = New clsINI(sIniDir & sIniFile)
                iniFile.WriteString("publishsettings", "outputfolder", sPDFuserFolder)
            End If
        End Using
    End Sub

    Private Sub ucSettings_Load(sender As Object, e As EventArgs) Handles Me.Load
        loadSettings()
    End Sub
    Public Sub loadSettings()
        'check of ini bestand bestaat, zo ja: instellingen laden
        If File.Exists(sIniDir & sIniFile) Then
            'bestand bestaat, instelingen laden
            iniFile = New clsINI(sIniDir & sIniFile)
            'versie updaten
            iniFile.WriteString("appsettings", "version", sCurrVersion)
            sPDFuserFolder = iniFile.GetString("publishsettings", "outputfolder", sPDFuserFolder)
            txtUserSaveFolder.Text = sPDFuserFolder
            sDefaultOutputLocation = iniFile.GetString("publishsettings", "defaultoutput", sDefaultOutputLocation)
            Select Case sDefaultOutputLocation
                Case "drawingfolder"
                    radioPDFdrawingFolder.Checked = True
                Case "userlocation"
                    radioPDFuserFolder.Checked = True
                Case "askonplot"
                    radioPDFfolderAsk.Checked = True
                Case Else
                    radioPDFdrawingFolder.Checked = True
            End Select
            sDefaultPlottingDevice = iniFile.GetString("publishsettings", "defaultplotter", sDefaultPlottingDevice)
            loadPlotConfigs()
        Else
            'eerst check of map wel bestaat
            If My.Computer.FileSystem.DirectoryExists(sIniDir) = False Then
                My.Computer.FileSystem.CreateDirectory(sIniDir)
                iniFile = New clsINI(sIniDir & sIniFile)
                iniFile.WriteString("appsettings", "version", sCurrVersion)
            End If
        End If
        lblVersion.Text = sCurrVersion
    End Sub

    Private Sub radioPDFfolderAsk_CheckedChanged(sender As Object, e As EventArgs) Handles radioPDFfolderAsk.CheckedChanged
        sDefaultOutputLocation = "askonplot"
        saveDefaultOutput()
    End Sub

    Private Sub radioPDFdrawingFolder_CheckedChanged(sender As Object, e As EventArgs) Handles radioPDFdrawingFolder.CheckedChanged
        sDefaultOutputLocation = "drawingfolder"
        saveDefaultOutput()
    End Sub
    Public Sub saveDefaultOutput()
        iniFile = New clsINI(sIniDir & sIniFile)
        iniFile.WriteString("publishsettings", "defaultoutput", sDefaultOutputLocation)
    End Sub

    Public Sub loadPlotConfigs()
        Dim acDoc As Document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument
        cmbPlottingDevice.Items.Clear()
        cmbPlottingDevice.Items.Add(sDefaultPlottingDevice)
        For Each plotDevice As String In PlotSettingsValidator.Current.GetPlotDeviceList()
            ' Output the names of the available plotter devices
            If plotDevice.Contains("AutoCAD PDF") And plotDevice <> sDefaultPlottingDevice Then
                cmbPlottingDevice.Items.Add(plotDevice)
            End If
        Next
        cmbPlottingDevice.SelectedIndex = 0
    End Sub

    Private Sub cmbPlottingDevice_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPlottingDevice.SelectedIndexChanged
        iniFile = New clsINI(sIniDir & sIniFile)
        sDefaultPlottingDevice = cmbPlottingDevice.Text
        iniFile.WriteString("publishsettings", "defaultplotter", sDefaultPlottingDevice)
    End Sub


    Private Sub lblVersion_DoubleClick(sender As Object, e As EventArgs) Handles lblVersion.DoubleClick
        loadSettings()
    End Sub
End Class
