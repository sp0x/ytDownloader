Imports ytDownloader.Extraction
Imports ytDownloader.StringExtensions


''' <summary>
''' Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
''' </summary>
Partial Public MustInherit Class Downloader

#Region "Events"
    ''' <summary>
    ''' Occurs when the download finished.
    ''' </summary>
    Public Event DownloadFinished As EventHandler(Of IoFinishedEventArgs)

    ''' <summary>
    ''' Occurs when the download is starts.
    ''' </summary>
    Public Event DownloadStarted As EventHandler
    Public Event DownloadProgressChanged As EventHandler(Of ProgressEventArgs)
    Public Event ExtractionProgressChanged As EventHandler(Of ProgressEventArgs)


    Friend Sub RaiseDownloadFinished(sender As Object, e As IoFinishedEventArgs)
        RaiseEvent DownloadFinished(sender, e)
    End Sub
    Protected Sub RaiseDownloadStarted(sender As Object, e As EventArgs)
        RaiseEvent DownloadStarted(sender, e)
    End Sub
    Protected Sub RaiseDownloadProgressChanged(sender As Object, e As ProgressEventArgs)
        RaiseEvent DownloadProgressChanged(sender, e)
    End Sub
    Protected Sub RaiseExtractionProgressChanged(sender As Object, e As ProgressEventArgs)
        RaiseEvent ExtractionProgressChanged(sender, e)
    End Sub
#End Region

#Region "Props"

    ''' <summary>
    ''' Gets the number of bytes to download. <c>null</c>, if everything is downloaded.
    ''' </summary>
    Public Property BytesToDownload As Nullable(Of Integer) = Nothing

    ''' <summary>
    ''' Gets the path to save the video/audio.
    ''' </summary>
    Public Property OutputPath() As String

    ''' <summary>
    ''' Gets the video to download/convert.
    ''' </summary>
    Public Property VideoCodec() As VideoCodecInfo
#End Region


    ''' <summary>
    ''' Starts the work of the <see cref="Downloader"/>.
    ''' </summary>
    Protected MustOverride Sub StartDownloading()
    Public Sub Start()
        If String.IsNullOrEmpty(OutputPath) Then
            OutputPath = VideoCodec.Title.RemoveIllegalPathCharacters
            If TypeOf Me Is AudioDownloader Then
                OutputPath &= VideoCodec.AudioExtension
            Else
                OutputPath &= VideoCodec.VideoExtension
            End If
        End If
        StartDownloading()
    End Sub
    Public Overloads Async Sub StartAsync()
        Await Task.Factory.StartNew(AddressOf Start)
    End Sub
    Public Overloads Sub StartDownloadingThreaded()
        Task.Factory.StartNew(AddressOf Start)
    End Sub
End Class