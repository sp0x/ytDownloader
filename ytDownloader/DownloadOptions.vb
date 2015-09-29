Imports ytDownloader.Extraction

Public Class DownloadOptions

#Region "Properties"
    Private _onlyVideo As Boolean
    Public ReadOnly Property OnlyVideo As Boolean
        Get
            Return _onlyVideo
        End Get
    End Property
    Public Property Format As String
    Public Property Quality As Int32
    Public Property SizeLimit As Long
    Private _strOutput As String
    Public Property Output As String
        Get
            Return _strOutput
        End Get
        Set(value As String)
            _strOutput = value
        End Set
    End Property
    Public Property IsPlaylist As Boolean

    Private _dFilter As Func(Of VideoCodecInfo, Boolean)
    Public Property Filter As Func(Of VideoCodecInfo, Boolean)
        Get
            Return _dFilter
        End Get
        Set(value As Func(Of VideoCodecInfo, Boolean))
            _dFilter = value
        End Set
    End Property
#End Region

#Region "Codec resolvers"

    Public Function GetCodec(videoId As String, Optional ByRef video As YtVideo = Nothing) As VideoCodecInfo
        video = New YtVideo(videoId)
        Return GetCodec(video)
    End Function
    Public Function GetCodec(ByRef video As YtVideo) As VideoCodecInfo
        Dim codec As VideoCodecInfo = GetCodecs(video).FirstOrDefault
        If codec Is Nothing Then
            Throw New VideoNotAvailableException("Can't find a codec matching the parameters!")
        End If
        If Not OnlyVideo Then ' we're interesed when can we download an audio
            _onlyVideo = Not codec.CanExtractAudio ' Audio can't be extracted, so fetch the video only
        End If
        Return codec
    End Function

    Public Function GetCodecs(videoId As String) As IEnumerable(Of VideoCodecInfo)
        If videoId.Contains("http:") Or videoId.Contains("https:") Then
            videoId = YtVideo.GetVideoId(videoId)
            ' Throw New InvalidOperationException("VideoID must be specified, but videoURL has been passed! Please pass only VideoIDs.")
        End If
        If String.IsNullOrEmpty(videoId) Then
            Throw New ArgumentNullException("videoId")
        End If
        Return GetCodecs(New YtVideo(videoId))
    End Function
    Public Function GetCodecs(ByRef video As YtVideo) As IEnumerable(Of VideoCodecInfo)
        If video.Codecs Is Nothing Then
            video = Downloader.Factory(Of VideoDownloader).FetchVideo(video.ToString())
        End If
        Dim codecs As IEnumerable(Of VideoCodecInfo) = Nothing
        If Quality > -1 And Quality < Int32.MaxValue Then
            codecs = video.Codecs.Where(Filter)
        ElseIf Quality = -1 Then
            codecs = video.Codecs.Where(Filter).OrderBy(Function(x) x.Resolution)
        ElseIf Quality = Int32.MaxValue Then
            codecs = video.Codecs.Where(Filter).OrderByDescending(Function(x) x.Resolution)
        End If
        If codecs Is Nothing Then
            Throw New VideoNotAvailableException("Can't find a codec matching the parameters!")
        End If
        Return codecs
    End Function

#End Region

#Region "Construction"
    Public Sub New()
    End Sub
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
    Public Sub SetOnlyVideo(value As Boolean)
        _onlyVideo = value
    End Sub
#End Region

    Public Function Clone() As DownloadOptions
        Dim res As New DownloadOptions(OnlyVideo, Format, Quality)
        res.SizeLimit = SizeLimit
        res.Output = Output
        res.IsPlaylist = IsPlaylist
        res.Filter = Filter
        Return res
    End Function

    Public Sub CloneTo(ByRef options As DownloadOptions)
        options = New DownloadOptions(OnlyVideo, Format, Quality)
        options.SizeLimit = SizeLimit
        options.Output = Output
        options.IsPlaylist = IsPlaylist
        options.Filter = Filter
    End Sub
End Class