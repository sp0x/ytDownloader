Imports ytDownloader.Extraction

Public Class DownloadOptions
    Private _onlyVideo As Boolean
    Public ReadOnly Property OnlyVideo As Boolean
        Get
            Return _onlyVideo
        End Get
    End Property
    Public Property Format As String
    Public Property Quality As Int32
    Public Property SizeLimit As Long
    Public Property Output As String
    Public Property Filter As Func(Of VideoCodecInfo, Boolean)
    Public Function GetCodec(videoId As String) As VideoCodecInfo
        If videoId.Contains("http:") Or videoId.Contains("https:") Then
            videoId = YtVideo.GetVideoId(videoId)
            ' Throw New InvalidOperationException("VideoID must be specified, but videoURL has been passed! Please pass only VideoIDs.")
        End If
        If String.IsNullOrEmpty(videoId) Then
            Throw New ArgumentNullException("videoId")
        End If
        Return GetCodec(New YtVideo(videoId))
    End Function
    Public Function GetCodec(ByRef video As YtVideo) As VideoCodecInfo
        If video.Codecs Is Nothing Then
            video = Downloader.Factory(Of VideoDownloader).FetchVideo(video.ToString())
        End If
        Dim codec As VideoCodecInfo = video.Codecs.FirstOrDefault(Filter)
        If codec Is Nothing Then
            Throw New VideoNotAvailableException("Can't find a codec matching the parameters!")
        End If
        If Not OnlyVideo Then ' we're interesed when can we download an audio
            _onlyVideo = Not codec.CanExtractAudio ' Audio can't be extracted, so fetch the video only
        End If
        Return codec
    End Function
#Region "Construction"
    Friend Sub New(ByRef onlyvideo As Boolean, Optional format As String = "mp3", Optional quality As Int32 = 0)
        _onlyVideo = onlyvideo
        Me.Format = format
        Me.Quality = quality
    End Sub
    Friend Sub New(outputPath As String, ByRef onlyvideo As Boolean, Optional format As String = "mp3", Optional quality As Int32 = 0)
        Output = outputPath
        _onlyVideo = onlyvideo
        Me.Format = format
        Me.Quality = quality
    End Sub
#End Region
End Class