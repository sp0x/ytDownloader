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
    Public Property ByteLitmit As Long
    Public Property Url As String
    Public Property Output As String
    Public Property Filter As Func(Of VideoCodecInfo, Boolean)
    Public Property SelectedCodec As VideoCodecInfo
    Public Sub New(ByRef onlyvideo As Boolean, Optional format As String = "mp3", Optional quality As Int32 = 0)
        _onlyVideo = onlyvideo
        Me.Format = format
        Me.Quality = quality
    End Sub
    Public Sub New(videoUrl As String, outputPath As String, ByRef onlyvideo As Boolean, Optional format As String = "mp3", Optional quality As Int32 = 0)
        Url = videoUrl
        Output = outputPath
        _onlyVideo = onlyvideo
        Me.Format = format
        Me.Quality = quality
    End Sub
End Class