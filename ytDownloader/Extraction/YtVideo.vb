Imports System.Text.RegularExpressions

Namespace Extraction
    Public Class YtVideo
        Public Property Id As String
        Public Property Codecs As IEnumerable(Of VideoCodecInfo)
       Public Property Available As Boolean
        Public Shared ReadOnly RxVideoId As New Regex("(watch\?v=)(.*?)(&|$)", RegexOptions.Compiled Or RegexOptions.IgnoreCase)




        Public Sub New(videoId As String, Optional bAvailable As Boolean = True)
            Id = videoId
            Available = bAvailable
        End Sub

        Public Function GetCodecs() As IEnumerable(Of VideoCodecInfo)
            Dim tmpVideo As YtVideo = Downloader.Factory(Of AudioDownloader).FetchVideo(ToString())
            If tmpVideo Is Nothing Then Return Nothing
            Codecs = tmpVideo.Codecs
            Return Codecs
        End Function

        Public Function GetDownloader(options As DownloadOptions, Optional isPlaylist As Boolean = False, Optional lazy As Boolean = True) As Downloader
            Dim result As Downloader = Nothing
            If lazy Then
                result = Downloader.CreateEmpty(ToString(), options, isPlaylist)
            Else
                result = Downloader.Factory.Create(Me, options, isPlaylist, lazy)
            End If
            result.InputUrl = ToString()
            Return result
        End Function

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(Id) Then
                Throw New InvalidOperationException("Video ID is null")
            End If
            Return String.Format("https://www.youtube.com/watch?v={0}", Id)
        End Function

        Public Shared Function GetVideoId(videoId As String) As String
            Return RxVideoId.MatchGroupValue(videoId, 2)
        End Function
        Public Shared Function GetVideoCoverUrl(videoUrl As String) As String
            Dim id As String = GetVideoId(videoUrl)
            If String.IsNullOrEmpty(id) Then Throw New InvalidOperationException("Invalid Youtube URL!")
            Return String.Format("http://img.youtube.com/vi/{0}/2.jpg", id)
        End Function
    End Class
End Namespace