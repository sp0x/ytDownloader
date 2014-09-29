Imports ytDownloader.Extraction

Namespace Extraction
    ''' <summary>
    ''' Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
    ''' </summary>
    Public MustInherit Class Downloader
        ' cn+IKot+LnQVUQG8EQekiJgJjJWzPEQX
        Public Class Factory(Of TDldType As {Downloader, New})
            Public Shared Function Create(video As VideoCodecInfo, outputFile As String, Optional bytesToDownload As System.Nullable(Of Integer) = Nothing)
                Dim dldr As Downloader = New TDldType
                dldr.AudioPath = outputFile
                dldr.BytesToDownload = bytesToDownload
                dldr.VideoCodec = video
                Return dldr
            End Function
            Public Shared Function Create(url As String, outputFile As String, Optional bytesToDownload As Int32 = 0, Optional codecSelector As Func(Of VideoCodecInfo, Boolean) = Nothing)
                If String.IsNullOrEmpty(url) Then Throw New ArgumentNullException("url")
                Dim vCodec As VideoCodecInfo = Nothing
                Dim ytUrl As New YtUrlDecoder()
                Dim videoModes As IEnumerable(Of VideoCodecInfo) = ytUrl.GetDownloadUrls(url)
                If codecSelector Is Nothing Then
                    vCodec = (From xVideo In videoModes Select xVideo Where xVideo.CanExtractAudio Order By xVideo.AudioBitrate Take 1).FirstOrDefault
                Else
                    vCodec = videoModes.FirstOrDefault(codecSelector)
                    If vCodec Is Nothing Then
                        Throw New VideoNotAvailableException("Can't find a valid video codec.")
                    End If
                End If
                Return Create(vCodec, outputFile, bytesToDownload)
            End Function
            Public Shared Async Function CreateAsync(url As String, outputFile As String, Optional bytesToDownload As Int32 = 0, Optional codecSelector As Func(Of VideoCodecInfo, Boolean) = Nothing) As Task(Of AudioDownloader)
                Return Await Task.Factory.StartNew( _
                    Function()
                        Return Create(url, outputFile, bytesToDownload, codecSelector)
                    End Function)
            End Function
        End Class


        ' ''' <summary>
        ' ''' Initializes a new instance of the <see cref="Downloader"/> class.
        ' ''' </summary>
        ' ''' <param name="video">The video to download/convert.</param>
        ' ''' <param name="savePath">The path to save the video/audio.</param>
        ' ''' /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        ' ''' <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        'Protected Sub New(video As VideoCodecInfo, savePath As String, Optional bytesToDownload As System.Nullable(Of Integer) = Nothing)
        '    If video Is Nothing Then Throw New ArgumentNullException("video")
        '    If savePath Is Nothing Then Throw New ArgumentNullException("savePath")
        '    Me.VideoCodec = video
        '    Me.AudioPath = savePath
        '    Me.BytesToDownload = bytesToDownload
        'End Sub

        'Protected Sub New(url As String, savePath As String, Optional bytesToDownload As Nullable(Of Int32) = Nothing, _
        '                  Optional ByVal codecSelector As Func(Of VideoCodecInfo, Boolean) = Nothing)
        '    If String.IsNullOrEmpty(url) Then Throw New ArgumentNullException("url")
        '    Dim ytUrl As New YtUrlDecoder()
        '    Dim videoModes As IEnumerable(Of VideoCodecInfo) = ytUrl.GetDownloadUrls(url)
        '    If codecSelector Is Nothing Then
        '        Me.VideoCodec = (From xVideo In videoModes Select xVideo Where xVideo.CanExtractAudio Order By xVideo.AudioBitrate Take 1).FirstOrDefault
        '    Else
        '        Me.VideoCodec = videoModes.FirstOrDefault(codecSelector)
        '        If Me.VideoCodec Is Nothing Then
        '            Throw New VideoNotAvailableException("Can't find a valid video codec.")
        '        End If
        '    End If
        '    Me.AudioPath = savePath
        '    Me.BytesToDownload = bytesToDownload
        'End Sub



        ''' <summary>
        ''' Occurs when the download finished.
        ''' </summary>
        Public Event DownloadFinished As EventHandler(Of IOFinishedEventArgs)

        ''' <summary>
        ''' Occurs when the download is starts.
        ''' </summary>
        Public Event DownloadStarted As EventHandler



        Friend Sub RaiseDownloadFinished(sender As Object, e As IOFinishedEventArgs)
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
        Public Property AudioPath() As String

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