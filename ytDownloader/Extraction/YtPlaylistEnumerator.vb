Imports System.Text.RegularExpressions
Imports System.Net
Imports System.Security.Policy
Imports System.Text.Encoding
Imports System.IO
Imports ytDownloader.RegexExtensions


Namespace Extraction
    Public Class YtPlaylistEnumerator
        Implements IEnumerator(Of YtVideo)
        Public Property Id As String
        Public Property OriginalUrl As String
        Public Property NextVideoId As String
        Public Property Index As Int32 = 0
        Private Property HttpClient As HttpWebRequest
        Public Property DownloadOptions As DownloadOptionsBuilder

#Region "Provates"
        Private _ytvCurrent As YtVideo
        Private _ytuDecoder As New YtUrlDecoder
#End Region

#Region "Regex"
        Private Shared ReadOnly RxList As Regex = New Regex("(list=)(.*?)(&|$)", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
        Private Shared ReadOnly RxVideoId As New Regex("(watch\?v=)(.*?)(&|$)", RegexOptions.Compiled Or RegexOptions.IgnoreCase)
#End Region


        Public Sub New(url As String, Optional downloadOptionsBuilder As DownloadOptionsBuilder = Nothing)
            YtUrlDecoder.TryNormalizeYoutubeUrl(url)
            Id = RxList.MatchGroupValue(url, 2)
            If String.IsNullOrEmpty(Id) Then
                Throw New InvalidOperationException("Could not parse url, please check if it has list=? in it!")
            End If
            OriginalUrl = url
            DownloadOptions = downloadOptionsBuilder
        End Sub

        Private Function downloadPage(url As String) As String
            HttpClient = HttpWebRequest.Create(url)
            HttpClient.Headers(HttpRequestHeader.AcceptEncoding) = "gzip, deflate"
            Dim respStrm As stream = HttpClient.GetResponse.GetResponseStream
            Return New StreamReader(respStrm).ReadToEnd()
        End Function



        ' (watch\?v=)(.*)?&(&amp;list=)(listId)(&amp;index=)(\d)
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            Index += 1
            Dim rxNextVideo As Regex

            Dim videoSrc As String
            Dim crVideoId As String
            If Index = 1 Then
                videoSrc = downloadPage(OriginalUrl)
              crVideoId = RxVideoId.MatchGroupValue(OriginalUrl, 2)
            Else 'On next video
                crVideoId = NextVideoId
                Dim tmpUrl As String = GetVideoListUrl(Id, crVideoId, Index)
                videoSrc = downloadPage(tmpUrl)
            End If
            'Next videoId must be kept

            If String.IsNullOrEmpty(videoSrc) Then
                Throw New NullReferenceException("Could not fetch URL's content!")
            End If
            If (YtUrlDecoder.IsVideoUnavailable(videoSrc)) Then
                Throw New VideoNotAvailableException()
            End If

            rxNextVideo = GenNextVideoRegex(Id, Index)
            NextVideoId = rxNextVideo.MatchGroupValue(videoSrc, 2)
            If String.IsNullOrEmpty(NextVideoId) Then
                Return False
            Else
                _ytvCurrent = New YtVideo(crVideoId) '_ytuDecoder.ParseVideoPage(videoSrc, crVideoId)
                Return True
            End If
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            Index = 1
        End Sub

#Region "Current"
        Public ReadOnly Property Current() As YtVideo Implements IEnumerator(Of YtVideo).Current
            Get
                Return _ytvCurrent
            End Get
        End Property
        Public ReadOnly Property IEnumerator_Current() As Object Implements IEnumerator.Current
            Get
                Return _ytvCurrent
            End Get
        End Property
#End Region

#Region "IDispose"
        Public Sub Dispose() Implements IDisposable.Dispose
            Id = Nothing
        End Sub
#End Region

#Region "Helpers"
        Public Shared Function GetVideoListUrl(listId As String, videoId As String, index As Int32)
            Return String.Format("https://www.youtube.com/watch?v={0}&list={1}&index={2}", videoId, listId, index)
        End Function
        Private Shared Function GenNextVideoRegex(listId As String, cIndex As Int32)
            Const template As String = "(watch\?v=)(.*)((&amp;list={0}&amp;index={1}"")|(&amp;index={1}&amp;list={0}""))"
            Return New Regex(String.Format(template, listId, cIndex + 1), RegexOptions.IgnoreCase Or RegexOptions.Compiled)
        End Function
#End Region
    End Class
End Namespace