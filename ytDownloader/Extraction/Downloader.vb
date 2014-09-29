Imports ytDownloader.Extraction

Namespace Extraction
    ''' <summary>
    ''' Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
    ''' </summary>
    Partial Public MustInherit Class Downloader
    ''' <summary>
        ''' Occurs when the download finished.
        ''' </summary>
        Public Event DownloadFinished As EventHandler(Of IoFinishedEventArgs)

        ''' <summary>
        ''' Occurs when the download is starts.
        ''' </summary>
        Public Event DownloadStarted As EventHandler
        
        Friend Sub RaiseDownloadFinished(sender As Object, e As IoFinishedEventArgs)
            RaiseEvent DownloadFinished(sender, e)
        End Sub
        Protected Sub RaiseDownloadStarted(sender As Object, e As EventArgs)
            RaiseEvent DownloadStarted(sender, e)
        End Sub


        ''' <summary>
        ''' Gets the number of bytes to download. <c>null</c>, if everything is downloaded.
        ''' </summary>
        Public Property BytesToDownload As System.Nullable(Of Integer) = Nothing

        ''' <summary>
        ''' Gets the path to save the video/audio.
        ''' </summary>
        Public Property OutputPath() As String

        ''' <summary>
        ''' Gets the video to download/convert.
        ''' </summary>
        Public Property VideoCodec() As VideoCodecInfo

        ''' <summary>
        ''' Starts the work of the <see cref="Downloader"/>.
        ''' </summary>
        Public MustOverride Sub StartDownloading()
        Public Overloads Async Sub StartDownloadingAsync()
            Await Task.Factory.StartNew(AddressOf StartDownloading)
        End Sub
    End Class


End Namespace