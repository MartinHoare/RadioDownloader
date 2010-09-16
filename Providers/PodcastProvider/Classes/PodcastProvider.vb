' Plugin for Radio Downloader to download general podcasts.
' Copyright © 2007-2010 Matt Robinson
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

Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Web
Imports System.Windows.Forms
Imports System.Xml

Imports RadioDld

Public Class PodcastProvider
    Implements IRadioProvider

    Public Event FindNewViewChange(ByVal objView As Object) Implements IRadioProvider.FindNewViewChange
    Public Event FindNewException(ByVal exception As Exception, ByVal unhandled As Boolean) Implements IRadioProvider.FindNewException
    Public Event FoundNew(ByVal strProgExtID As String) Implements IRadioProvider.FoundNew
    Public Event Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As ProgressIcon) Implements IRadioProvider.Progress
    Public Event Finished(ByVal strFileExtension As String) Implements IRadioProvider.Finished

    Friend Const intCacheHTTPHours As Integer = 2

    Private WithEvents doDownload As DownloadWrapper

    Public ReadOnly Property ProviderId() As Guid Implements IRadioProvider.ProviderId
        Get
            Return New Guid("3cfbe63e-95b8-4f80-8570-4ace909e0921")
        End Get
    End Property

    Public ReadOnly Property ProviderName() As String Implements IRadioProvider.ProviderName
        Get
            Return "Podcast"
        End Get
    End Property

    Public ReadOnly Property ProviderIcon() As Bitmap Implements IRadioProvider.ProviderIcon
        Get
            Return My.Resources.provider_icon
        End Get
    End Property

    Public ReadOnly Property ProviderDescription() As String Implements IRadioProvider.ProviderDescription
        Get
            Return "Audio files made available as enclosures on an RSS feed."
        End Get
    End Property

    ReadOnly Property ProgInfoUpdateFreqDays() As Integer Implements IRadioProvider.ProgInfoUpdateFreqDays
        Get
            ' Updating the programme info every week should be a reasonable trade-off
            Return 7
        End Get
    End Property

    Public Function GetShowOptionsHandler() As EventHandler Implements IRadioProvider.GetShowOptionsHandler
        Return Nothing
    End Function

    Public Function GetFindNewPanel(ByVal view As Object) As Panel Implements IRadioProvider.GetFindNewPanel
        Dim FindNewInst As New FindNew
        FindNewInst.clsPluginInst = Me
        Return FindNewInst.pnlFindNew
    End Function

    Public Function GetProgrammeInfo(ByVal progExtId As String) As GetProgrammeInfoReturn Implements IRadioProvider.GetProgrammeInfo
        Dim getProgInfo As New GetProgrammeInfoReturn
        getProgInfo.Success = False

        Dim xmlRSS As XmlDocument
        Dim xmlNamespaceMgr As XmlNamespaceManager

        Try
            xmlRSS = LoadFeedXml(New Uri(progExtID))
        Catch expWeb As WebException
            Return getProgInfo
        Catch expXML As XmlException
            Return getProgInfo
        End Try

        Try
            xmlNamespaceMgr = CreateNamespaceMgr(xmlRSS)
        Catch
            xmlNamespaceMgr = Nothing
        End Try

        Dim xmlTitle As XmlNode = xmlRSS.SelectSingleNode("./rss/channel/title")
        Dim xmlDescription As XmlNode = xmlRSS.SelectSingleNode("./rss/channel/description")

        If xmlTitle Is Nothing Or xmlDescription Is Nothing Then
            Return getProgInfo
        End If

        getProgInfo.ProgrammeInfo.Name = xmlTitle.InnerText

        If getProgInfo.ProgrammeInfo.Name = "" Then
            Return getProgInfo
        End If

        getProgInfo.ProgrammeInfo.Description = xmlDescription.InnerText
        getProgInfo.ProgrammeInfo.Image = RSSNodeImage(xmlRSS.SelectSingleNode("./rss/channel"), xmlNamespaceMgr)

        getProgInfo.Success = True
        Return getProgInfo
    End Function

    Public Function GetAvailableEpisodeIds(ByVal progExtId As String) As String() Implements IRadioProvider.GetAvailableEpisodeIds
        Dim strEpisodeIDs(-1) As String
        GetAvailableEpisodeIDs = strEpisodeIDs

        Dim xmlRSS As XmlDocument

        Try
            xmlRSS = LoadFeedXml(New Uri(progExtID))
        Catch expWeb As WebException
            Exit Function
        Catch expXML As XmlException
            Exit Function
        End Try

        Dim xmlItems As XmlNodeList
        xmlItems = xmlRSS.SelectNodes("./rss/channel/item")

        If xmlItems Is Nothing Then
            Exit Function
        End If

        Dim strItemID As String

        For Each xmlItem As XmlNode In xmlItems
            strItemID = ItemNodeToEpisodeID(xmlItem)

            If strItemID <> "" Then
                ReDim Preserve strEpisodeIDs(strEpisodeIDs.GetUpperBound(0) + 1)
                strEpisodeIDs(strEpisodeIDs.GetUpperBound(0)) = strItemID
            End If
        Next

        Return strEpisodeIDs
    End Function

    Function GetEpisodeInfo(ByVal progExtId As String, ByVal episodeExtId As String) As GetEpisodeInfoReturn Implements IRadioProvider.GetEpisodeInfo
        Dim episodeInfoReturn As New GetEpisodeInfoReturn
        episodeInfoReturn.Success = False

        Dim xmlRSS As XmlDocument
        Dim xmlNamespaceMgr As XmlNamespaceManager

        Try
            xmlRSS = LoadFeedXml(New Uri(progExtID))
        Catch expWeb As WebException
            Return episodeInfoReturn
        Catch expXML As XmlException
            Return episodeInfoReturn
        End Try

        Try
            xmlNamespaceMgr = CreateNamespaceMgr(xmlRSS)
        Catch
            xmlNamespaceMgr = Nothing
        End Try

        Dim xmlItems As XmlNodeList
        xmlItems = xmlRSS.SelectNodes("./rss/channel/item")

        If xmlItems Is Nothing Then
            Return episodeInfoReturn
        End If

        Dim strItemID As String

        For Each xmlItem As XmlNode In xmlItems
            strItemID = ItemNodeToEpisodeID(xmlItem)

            If strItemID = episodeExtId Then
                Dim xmlTitle As XmlNode = xmlItem.SelectSingleNode("./title")
                Dim xmlDescription As XmlNode = xmlItem.SelectSingleNode("./description")
                Dim xmlPubDate As XmlNode = xmlItem.SelectSingleNode("./pubDate")
                Dim xmlEnclosure As XmlNode = xmlItem.SelectSingleNode("./enclosure")

                If xmlEnclosure Is Nothing Then
                    Return episodeInfoReturn
                End If

                Dim xmlUrl As XmlAttribute = xmlEnclosure.Attributes("url")

                If xmlUrl Is Nothing Then
                    Return episodeInfoReturn
                End If

                Try
                    Dim uriTestValid As New Uri(xmlUrl.Value)
                Catch expUriFormat As UriFormatException
                    ' The enclosure url is empty or malformed, so return false for success
                    Return episodeInfoReturn
                End Try

                Dim dicExtInfo As New Dictionary(Of String, String)
                dicExtInfo.Add("EnclosureURL", xmlUrl.Value)

                If xmlTitle IsNot Nothing Then
                    episodeInfoReturn.EpisodeInfo.Name = xmlTitle.InnerText
                End If

                If episodeInfoReturn.EpisodeInfo.Name = "" Then
                    Return episodeInfoReturn
                End If

                If xmlDescription IsNot Nothing Then
                    Dim description As String = xmlDescription.InnerText

                    ' Replace common block level tags with newlines
                    description = description.Replace("<br", vbCrLf + "<br")
                    description = description.Replace("<p", vbCrLf + "<p")
                    description = description.Replace("<div", vbCrLf + "<div")

                    ' Replace HTML entities with their character counterparts
                    description = HttpUtility.HtmlDecode(description)

                    ' Strip out any HTML tags
                    Dim RegExpression As New Regex("<(.|\n)+?>")
                    episodeInfoReturn.EpisodeInfo.Description = RegExpression.Replace(description, "")
                End If

                Try
                    Dim xmlDuration As XmlNode = xmlItem.SelectSingleNode("./itunes:duration", xmlNamespaceMgr)

                    If xmlDuration IsNot Nothing Then
                        Dim strSplitDuration() As String = Split(xmlDuration.InnerText.Replace(".", ":"), ":")

                        If strSplitDuration.GetUpperBound(0) = 0 Then
                            episodeInfoReturn.EpisodeInfo.DurationSecs = CInt(strSplitDuration(0))
                        ElseIf strSplitDuration.GetUpperBound(0) = 1 Then
                            episodeInfoReturn.EpisodeInfo.DurationSecs = (CInt(strSplitDuration(0)) * 60) + CInt(strSplitDuration(1))
                        Else
                            episodeInfoReturn.EpisodeInfo.DurationSecs = ((CInt(strSplitDuration(0)) * 60) + CInt(strSplitDuration(1))) * 60 + CInt(strSplitDuration(2))
                        End If
                    Else
                        episodeInfoReturn.EpisodeInfo.DurationSecs = Nothing
                    End If
                Catch
                    episodeInfoReturn.EpisodeInfo.DurationSecs = Nothing
                End Try

                If xmlPubDate IsNot Nothing Then
                    Dim strPubDate As String = xmlPubDate.InnerText.Trim
                    Dim intZonePos As Integer = strPubDate.LastIndexOf(" ", StringComparison.Ordinal)
                    Dim tspOffset As TimeSpan = New TimeSpan(0)

                    If intZonePos > 0 Then
                        Dim strZone As String = strPubDate.Substring(intZonePos + 1)
                        Dim strZoneFree As String = strPubDate.Substring(0, intZonePos)

                        Select Case strZone
                            Case "GMT"
                                ' No need to do anything
                            Case "UT"
                                tspOffset = New TimeSpan(0)
                                strPubDate = strZoneFree
                            Case "EDT"
                                tspOffset = New TimeSpan(-4, 0, 0)
                                strPubDate = strZoneFree
                            Case "EST", "CDT"
                                tspOffset = New TimeSpan(-5, 0, 0)
                                strPubDate = strZoneFree
                            Case "CST", "MDT"
                                tspOffset = New TimeSpan(-6, 0, 0)
                                strPubDate = strZoneFree
                            Case "MST", "PDT"
                                tspOffset = New TimeSpan(-7, 0, 0)
                                strPubDate = strZoneFree
                            Case "PST"
                                tspOffset = New TimeSpan(-8, 0, 0)
                                strPubDate = strZoneFree
                            Case Else
                                If strZone.Length >= 4 And IsNumeric(strZone) Or IsNumeric(strZone.Substring(1)) Then
                                    Try
                                        Dim intValue As Integer = Integer.Parse(strZone, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture)
                                        tspOffset = New TimeSpan(intValue \ 100, intValue Mod 100, 0)
                                        strPubDate = strZoneFree
                                    Catch expFormat As FormatException
                                        ' The last part of the date was not a time offset
                                    End Try
                                End If
                        End Select
                    End If

                    ' Strip the day of the week from the beginning of the date string if it is there,
                    ' as it can contradict the date itself.
                    Dim strDays() As String = {"mon,", "tue,", "wed,", "thu,", "fri,", "sat,", "sun,"}

                    For Each strDay As String In strDays
                        If strPubDate.StartsWith(strDay, StringComparison.OrdinalIgnoreCase) Then
                            strPubDate = strPubDate.Substring(strDay.Length).Trim
                            Exit For
                        End If
                    Next

                    Try
                        episodeInfoReturn.EpisodeInfo.Date = Date.Parse(strPubDate, Nothing, DateTimeStyles.AssumeUniversal)
                    Catch expFormat As FormatException
                        episodeInfoReturn.EpisodeInfo.Date = Now
                        tspOffset = New TimeSpan(0)
                    End Try

                    episodeInfoReturn.EpisodeInfo.Date = episodeInfoReturn.EpisodeInfo.Date.Subtract(tspOffset)
                Else
                    episodeInfoReturn.EpisodeInfo.Date = Now
                End If

                episodeInfoReturn.EpisodeInfo.Image = RSSNodeImage(xmlItem, xmlNamespaceMgr)

                If episodeInfoReturn.EpisodeInfo.Image Is Nothing Then
                    episodeInfoReturn.EpisodeInfo.Image = RSSNodeImage(xmlRSS.SelectSingleNode("./rss/channel"), xmlNamespaceMgr)
                End If

                episodeInfoReturn.EpisodeInfo.ExtInfo = dicExtInfo
                episodeInfoReturn.Success = True

                Return episodeInfoReturn
            End If
        Next

        Return episodeInfoReturn
    End Function

    Public Sub DownloadProgramme(ByVal progExtId As String, ByVal episodeExtId As String, ByVal progInfo As ProgrammeInfo, ByVal epInfo As EpisodeInfo, ByVal finalName As String) Implements IRadioProvider.DownloadProgramme
        Dim downloadUrl As Uri = New Uri(epInfo.ExtInfo("EnclosureURL"))

        Dim fileNamePos As Integer = finalName.LastIndexOf("\", StringComparison.Ordinal)
        Dim extensionPos As Integer = downloadUrl.AbsolutePath.LastIndexOf(".", StringComparison.Ordinal)
        Dim extension As String = "mp3"

        If extensionPos > -1 Then
            extension = downloadUrl.AbsolutePath.Substring(extensionPos + 1)
        End If

        Dim downloadFileName As String = Path.Combine(System.IO.Path.GetTempPath, Path.Combine("RadioDownloader", finalName.Substring(fileNamePos + 1) + "." + extension))
        finalName += "." + extension

        doDownload = New DownloadWrapper(downloadUrl, downloadFileName)
        doDownload.Download()

        While (Not doDownload.Complete) And doDownload.Error Is Nothing
            Thread.Sleep(500)
        End While

        If doDownload.Error IsNot Nothing Then
            If TypeOf doDownload.Error Is WebException Then
                Dim webExp As WebException = CType(doDownload.Error, WebException)

                If webExp.Status = WebExceptionStatus.NameResolutionFailure Then
                    Throw New DownloadException(ErrorType.NetworkProblem, "Unable to resolve the domain to download this episode from.  Check your internet connection, or try again later.")
                ElseIf TypeOf webExp.Response Is HttpWebResponse Then
                    Dim webErrorResponse As HttpWebResponse = CType(webExp.Response, HttpWebResponse)

                    If webErrorResponse.StatusCode = HttpStatusCode.NotFound Then
                        Throw New DownloadException(ErrorType.NotAvailable, "This episode appears to be no longer available.  You can either try again later, or cancel the download to remove it from the list and clear the error.")
                    End If
                End If
            End If

            Throw doDownload.Error
        End If

        RaiseEvent Progress(100, "Downloading...", ProgressIcon.Downloading)
        Call File.Move(downloadFileName, finalName)
        RaiseEvent Finished(extension)
    End Sub

    Friend Sub RaiseFindNewException(ByVal expException As Exception)
        RaiseEvent FindNewException(expException, True)
    End Sub

    Friend Sub RaiseFoundNew(ByVal strExtID As String)
        RaiseEvent FoundNew(strExtID)
    End Sub

    Friend Function LoadFeedXml(ByVal url As Uri) As XmlDocument
        Dim feedXml As New XmlDocument
        Dim cachedWeb As CachedWebClient = CachedWebClient.GetInstance

        Dim feedString As String = cachedWeb.DownloadString(url, PodcastProvider.intCacheHTTPHours)

        ' The LoadXml method of XmlDocument doesn't work correctly all of the time,
        ' so convert the string to a UTF-8 byte array
        Dim encodedString As Byte() = Encoding.UTF8.GetBytes(feedString)

        ' And then load this into the XmlDocument via a stream
        Dim feedStream As New MemoryStream(encodedString)
        feedStream.Flush()
        feedStream.Position = 0

        feedXml.Load(feedStream)
        Return feedXml
    End Function

    Private Function ItemNodeToEpisodeID(ByVal xmlItem As XmlNode) As String
        Dim strItemID As String = ""
        Dim xmlItemID As XmlNode = xmlItem.SelectSingleNode("./guid")

        If xmlItemID IsNot Nothing Then
            strItemID = xmlItemID.InnerText
        End If

        If strItemID = "" Then
            xmlItemID = xmlItem.SelectSingleNode("./enclosure")

            If xmlItemID IsNot Nothing Then
                Dim xmlUrl As XmlAttribute = xmlItemID.Attributes("url")

                If xmlUrl IsNot Nothing Then
                    strItemID = xmlUrl.Value
                End If
            End If
        End If

        Return strItemID
    End Function

    Private Function RSSNodeImage(ByVal xmlNode As XmlNode, ByVal xmlNamespaceMgr As XmlNamespaceManager) As Bitmap
        Dim cachedWeb As CachedWebClient = CachedWebClient.GetInstance

        Try
            Dim xmlImageNode As XmlNode = xmlNode.SelectSingleNode("itunes:image", xmlNamespaceMgr)

            If xmlImageNode IsNot Nothing Then
                Dim imageUrl As New Uri(xmlImageNode.Attributes("href").Value)
                Dim bteImageData As Byte() = cachedWeb.DownloadData(imageUrl, intCacheHTTPHours)
                RSSNodeImage = New Bitmap(New IO.MemoryStream(bteImageData))
            Else
                RSSNodeImage = Nothing
            End If
        Catch
            RSSNodeImage = Nothing
        End Try

        If RSSNodeImage Is Nothing Then
            Try
                Dim xmlImageUrlNode As XmlNode = xmlNode.SelectSingleNode("image/url")

                If xmlImageUrlNode IsNot Nothing Then
                    Dim imageUrl As New Uri(xmlImageUrlNode.InnerText)
                    Dim bteImageData As Byte() = cachedWeb.DownloadData(imageUrl, intCacheHTTPHours)
                    RSSNodeImage = New Bitmap(New IO.MemoryStream(bteImageData))
                Else
                    RSSNodeImage = Nothing
                End If
            Catch
                RSSNodeImage = Nothing
            End Try

            If RSSNodeImage Is Nothing Then
                Try
                    Dim xmlImageNode As XmlNode = xmlNode.SelectSingleNode("media:thumbnail", xmlNamespaceMgr)

                    If xmlImageNode IsNot Nothing Then
                        Dim imageUrl As New Uri(xmlImageNode.Attributes("url").Value)
                        Dim bteImageData As Byte() = cachedWeb.DownloadData(imageUrl, intCacheHTTPHours)
                        RSSNodeImage = New Bitmap(New IO.MemoryStream(bteImageData))
                    Else
                        RSSNodeImage = Nothing
                    End If
                Catch
                    RSSNodeImage = Nothing
                End Try
            End If
        End If
    End Function

    Private Function CreateNamespaceMgr(ByVal xmlDocument As XmlDocument) As XmlNamespaceManager
        Dim nsManager As New XmlNamespaceManager(xmlDocument.NameTable)

        For Each xmlAttrib As XmlAttribute In xmlDocument.SelectSingleNode("/*").Attributes
            If xmlAttrib.Prefix = "xmlns" Then
                nsManager.AddNamespace(xmlAttrib.LocalName, xmlAttrib.Value)
            End If
        Next

        Return nsManager
    End Function

    Private Sub doDownload_DownloadProgress(ByVal sender As Object, ByVal e As System.Net.DownloadProgressChangedEventArgs) Handles doDownload.DownloadProgress
        Dim intPercent As Integer = e.ProgressPercentage

        If intPercent > 99 Then
            intPercent = 99
        End If

        RaiseEvent Progress(intPercent, "Downloading...", ProgressIcon.Downloading)
    End Sub
End Class
