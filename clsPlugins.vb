' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright � 2007  www.nerdoftheherd.com
'
' This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
' Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
' option) any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
' implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
' License for more details.
'
' You should have received a copy of the GNU General Public License along with this program; if not, write
' to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

Option Strict On
Option Explicit On

Imports System.Reflection
Imports System.IO

Public Interface IRadioProvider
    Class StationTable
        Inherits Hashtable

        Private SortedList(-1) As StationInfo

        Public Shadows Sub Add(ByVal strStationUniqueID As String, ByVal strStationName As String, ByVal booVisibleByDefault As Boolean, Optional ByVal icoStationIcon As Icon = Nothing)
            Dim Station As StationInfo

            Station.StationUniqueID = strStationUniqueID
            Station.StationName = strStationName
            Station.VisibleByDefault = booVisibleByDefault
            Station.StationIcon = icoStationIcon

            ReDim Preserve SortedList(SortedList.GetUpperBound(0) + 1)
            SortedList(SortedList.GetUpperBound(0)) = Station

            Call MyBase.Add(strStationUniqueID, Station)
        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal strKey As String) As StationInfo
            Get
                Return DirectCast(MyBase.Item(strKey), StationInfo)
            End Get
        End Property

        Public ReadOnly Property SortedValues() As StationInfo()
            Get
                Return SortedList
            End Get
        End Property
    End Class

    Structure StationInfo
        Dim StationUniqueID As String
        Dim StationName As String
        Dim StationIcon As Icon
        Dim VisibleByDefault As Boolean
    End Structure

    Structure ProgramInfo
        Dim Result As ProgInfoResult
        Dim ProgramName As String
        Dim ProgramDescription As String
        Dim ProgramDuration As Long
        Dim ProgramDate As Date
        Dim ProgramDldUrl As String
        Dim Image As Bitmap
    End Structure

    Structure ProgramListItem
        Dim ProgramID As String
        Dim StationID As String
        Dim ProgramName As String
    End Structure

    Enum ProgInfoResult
        OtherError
        TempError
        Skipped
        Success
    End Enum

    Enum ProgressIcon
        Downloading
        Converting
    End Enum

    Enum ErrorType
        UnknownError
        MissingDependency
    End Enum

    ReadOnly Property ProviderUniqueID() As String
    ReadOnly Property ProviderName() As String
    ReadOnly Property ProviderDescription() As String
    ReadOnly Property DynamicSubscriptionName() As Boolean

    Function ReturnStations() As StationTable
    Function ListProgramIDs(ByVal strStationID As String) As ProgramListItem()
    Function GetLatestProgramInfo(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteLastInfoFor As Date, ByVal dteLastAttempt As Date) As ProgramInfo
    Function CouldBeNewEpisode(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Boolean
    Function IsStillAvailable(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booIsLatestProg As Boolean) As Boolean

    Event Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As ProgressIcon)
    Event DldError(ByVal errType As ErrorType, ByVal strErrorDetails As String)
    Event Finished()

    Sub DownloadProgram(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal intProgLength As Integer, ByVal strProgDldUrl As String, ByVal strFinalName As String, ByVal intBandwidthLimitKBytes As Integer)
End Interface

Public Class clsPlugins
    Private Const strInterfaceName As String = "IRadioProvider"
    Private htbPlugins As New Hashtable

    Private Structure AvailablePlugin
        Dim AssemblyPath As String
        Dim ClassName As String
    End Structure

    Public Function PluginExists(ByVal strPluginID As String) As Boolean
        Return htbPlugins.ContainsKey(strPluginID)
    End Function

    Public Function GetPluginInstance(ByVal strPluginID As String) As IRadioProvider
        If PluginExists(strPluginID) Then
            Return CreateInstance(DirectCast(htbPlugins.Item(strPluginID), AvailablePlugin))
        Else
            Return Nothing
        End If
    End Function

    Public Function GetPluginIdList() As String()
        ReDim GetPluginIdList(htbPlugins.Keys.Count - 1)
        htbPlugins.Keys.CopyTo(GetPluginIdList, 0)
    End Function

    ' Next three functions are based on code from http://www.developerfusion.co.uk/show/4371/3/

    Public Sub New(ByVal strPath As String)
        Dim strDLLs() As String, intIndex As Integer
        Dim objDLL As Assembly

        'Go through all DLLs in the directory, attempting to load them
        strDLLs = Directory.GetFileSystemEntries(strPath, "*.dll")
        For intIndex = 0 To strDLLs.Length - 1
            Try
                objDLL = [Assembly].LoadFrom(strDLLs(intIndex))
                ExamineAssembly(objDLL)
            Catch
                'Error loading DLL, we don't need to do anything special
            End Try
        Next
    End Sub

    Private Sub ExamineAssembly(ByVal objDLL As Assembly)
        Dim objType As Type
        Dim objInterface As Type
        Dim Plugin As AvailablePlugin
        'Loop through each type in the DLL
        For Each objType In objDLL.GetTypes
            'Only look at public types
            If objType.IsPublic = True Then
                'Ignore abstract classes
                If Not ((objType.Attributes And TypeAttributes.Abstract) = TypeAttributes.Abstract) Then
                    'See if this type implements our interface
                    objInterface = objType.GetInterface(strInterfaceName, True)
                    If Not (objInterface Is Nothing) Then
                        Plugin = New AvailablePlugin()
                        Plugin.AssemblyPath = objDLL.Location
                        Plugin.ClassName = objType.FullName

                        Try
                            Dim objPlugin As IRadioProvider
                            objPlugin = CreateInstance(Plugin)
                            htbPlugins.Add(objPlugin.ProviderUniqueID, Plugin)
                        Catch
                            Continue For
                        End Try
                    End If
                End If
            End If
        Next
    End Sub

    Private Function CreateInstance(ByVal Plugin As AvailablePlugin) As IRadioProvider
        Dim objDLL As Assembly
        Dim objPlugin As Object

        Try
            'Load dll
            objDLL = Assembly.LoadFrom(Plugin.AssemblyPath)
            'Create and return class instance
            objPlugin = objDLL.CreateInstance(Plugin.ClassName)
            ' Cast to IRadioProvider and return
            Return DirectCast(objPlugin, IRadioProvider)
        Catch
            Return Nothing
        End Try
    End Function
End Class