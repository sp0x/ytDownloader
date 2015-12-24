Imports System.IO
Imports System.Net
Imports ytDownloader.Extraction
Imports ytDownloader.Extraction.VideoParsers


''' <summary>
''' Provides a method to download a video and extract its audio track.
''' </summary>
Public Class AudioDownloader
    Inherits Downloader
    Private _isCanceled As Boolean


#Region "Events"
    ''' <summary>
    ''' Occurs when the progress of the audio extraction has changed.
    ''' </summary>
    Public Event AudioExtractionProgressChanged As EventHandler(Of ProgressEventArgs)
    ''' <summary>
    ''' Occurs when the download progress of the video file has changed.
    ''' </summary>
    Public Shadows Event DownloadProgressChanged As EventHandler(Of ProgressEventArgs)
#End Region

    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(videoCodec As VideoCodecInfo )
        Me.VideoCodec = videoCodec
    End Sub

    ''' <summary>
    ''' Downloads the video from YouTube and then extracts the audio track out if it.
    ''' </summary>
    ''' <exception cref="IOException">
    ''' The temporary video file could not be created.
    ''' - or -
    ''' The audio file could not be created.
    ''' </exception>
    ''' <exception cref="AudioExtractors.AudioExtractionException">An error occured during audio extraction.</exception>
    ''' <exception cref="WebException">An error occured while downloading the video.</exception>
    Protected Overrides Sub StartDownloading()
        Dim tempPath As String = Path.GetTempFileName()
        Dim tempPathOpt = GetStoragePath("video_storage")
        Dim filename = Path.GetFileNameWithoutExtension(OutputPath)
        If tempPathOpt IsNot Nothing then tempPath = Path.combine(tempPathOpt , filename & ".tmp")
        'Remplace tempPath with video storage directory which is frequently emptied, but provides caching
        Me.DownloadVideo(tempPath)
        If Not Me._isCanceled Then
            Me.ExtractAudio(tempPath)
        End If
        Me.OnDownloadFinished(New IOFinishedEventArgs With {.Path = Me.OutputPath})
    End Sub


    Protected Sub OnDownloadFinished(e As IOFinishedEventArgs)
        MyBase.RaiseDownloadFinished(Me, e)
    End Sub


    ''' <summary>
    ''' Downloads the FLV.
    ''' </summary>
    ''' <param name="path">The path where it should be saved</param>
    ''' <remarks></remarks>
    Private Sub DownloadVideo(path As String)
        Dim videoDownloader As VideoDownloader = Factory(Of VideoDownloader).Create(Me.VideoCodec, path, Options.SizeLimit)
        AddHandler videoDownloader.DownloadProgressChanged, _
            Sub(sender As Object, e As ProgressEventArgs)
                If IsUpdateReady(e) Then
                    e.IsReady = True
                    RaiseEvent DownloadProgressChanged(Me, e)
                    MyBase.RaiseDownloadProgressChanged(Me, e)
                    Me._isCanceled = e.Cancel
                End If
            End Sub
        videoDownloader.Start()
    End Sub

    ''' <summary>
    ''' Extracts the audio stream from a given F
    ''' </summary>
    ''' <param name="path">The path to the downloaded Flv file.</param>
    ''' <remarks></remarks>
    Private Sub ExtractAudio(path As String)
      Using flvFile = New FlvFileParser(path, OutputPath)
            AddHandler flvFile.ConversionProgressChanged, _
                Sub(sender As Object, e As ProgressEventArgs)
                    If IsUpdateReady(e) Then
                        RaiseEvent AudioExtractionProgressChanged(Me, e)
                        MyBase.RaiseExtractionProgressChanged(Me, e)
                    End If
                End Sub
            flvFile.ExtractStreams()
        End Using
    End Sub
End Class