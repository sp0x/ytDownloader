Imports System.Text.RegularExpressions
Imports System.Net
Imports System.IO

Namespace Extraction
    Public Class YtPlaylistEnumerator
        Implements IEnumerator(Of YtVideo)
        Public Property Id As String
        Public Property OriginalUrl As String
        Public Property NextVideoId As String
        Public Property NextVideosQue As New Queue(Of String)
        Private Property Cookies As New CookieContainer


        Public Property Index As Int32 = 0
        Private Property HttpClient As HttpWebRequest
        Public Property DownloadOptions As DownloadOptions

#Region "Provates"
        Private _ytvCurrent As YtVideo
        Private _ytuDecoder As New YtUrlDecoder
#End Region

#Region "Regex"
        Public Shared ReadOnly RxList As Regex = New Regex("(list=)(.*?)(&|$)", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
        Public Shared ReadOnly RxVideoId As New Regex("(watch\?v=)(.*?)(&|$)", RegexOptions.Compiled Or RegexOptions.IgnoreCase)
#End Region


        Public Sub New(url As String, Optional downloadOptionsBuilder As DownloadOptions = Nothing)
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
            HttpClient.CookieContainer = Cookies
            HttpClient.Headers(HttpRequestHeader.AcceptEncoding) = "gzip, deflate"
            Dim respStrm As stream = HttpClient.GetResponse.GetResponseStream
            Return New StreamReader(respStrm).ReadToEnd()
        End Function

        ''' <summary>
        ''' Iterates through each video. Caches with a range of 25(defined by youtube) so it doesn't do double requests.
        ''' Each video is just a new video with it's ID.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            Index += 1
            Dim videoSrc As String
            Dim crVideoId As String

            If Index = 1 And NextVideosQue.Count = 0 Then
                videoSrc = downloadPage(OriginalUrl)
                crVideoId = RxVideoId.MatchGroupValue(OriginalUrl, 2)
            Else 'On next video
                crVideoId = NextVideosQue.Dequeue
                If NextVideosQue.Count = 0 Then
                    'Next videos needs to be fetched
                    Dim tmpUrl As String = GetVideoListUrl(Id, crVideoId, Index)
                    videoSrc = downloadPage(tmpUrl)
                End If
            End If
            'Next videoId must be kept

            If String.IsNullOrEmpty(videoSrc) And NextVideosQue.Count = 0 Then
                Throw New NullReferenceException("Could not fetch URL's content!")
            ElseIf Not String.IsNullOrEmpty(videoSrc) Then
                If (YtUrlDecoder.IsVideoUnavailable(videoSrc)) Then Throw New VideoNotAvailableException()
                If NextVideosQue.Count = 0 Then
                    ExtractNextVIds(videoSrc)
                End If
            End If


            If String.IsNullOrEmpty(NextVideoId) Then
                Return False
            Else
                _ytvCurrent = New YtVideo(crVideoId) '
                Return True
            End If
        End Function

        Private Sub ExtractNextVIds(videoSrc As String)
            Dim rxNextVideo As Regex
            Dim ixTmp As Int32 = Index
            rxNextVideo = GenNextVideoRegex(Id, Index)
            NextVideoId = rxNextVideo.MatchGroupValue(videoSrc, 2)
            Dim tmpNextVideoId As String = NextVideoId
            ''Builds a stack of the next visible URL's
            Do
                NextVideosQue.Enqueue(tmpNextVideoId)
                ixTmp += 1
                rxNextVideo = GenNextVideoRegex(Id, ixTmp)
                tmpNextVideoId = rxNextVideo.MatchGroupValue(videoSrc, 2)
            Loop While Not String.IsNullOrEmpty(tmpNextVideoId)
        End Sub

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