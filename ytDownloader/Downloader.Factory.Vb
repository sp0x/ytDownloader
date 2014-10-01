Imports ytDownloader.Extraction

Partial Public Class Downloader
    Public Class Factory
        Public Shared Function Create(codec As VideoCodecInfo, options As DownloadOptions, Optional isPlaylist As Boolean = False)
            Dim downloader As Downloader
            If options.OnlyVideo Then
                If Not isPlaylist Then options.Output = System.IO.Path.ChangeExtension(options.Output, codec.VideoExtension)
                downloader = Factory(Of VideoDownloader).Create(codec, options.Output)
            Else
                If Not isPlaylist Then options.Output = System.IO.Path.ChangeExtension(options.Output, codec.AudioExtension)
                downloader = Factory(Of AudioDownloader).Create(codec, options.Output)
            End If
            downloader.BytesToDownload = options.SizeLimit
            downloader.IsPlaylistMember = isPlaylist
            Return downloader
        End Function
        Public Shared Function Create(url As String, options As DownloadOptions, Optional isPlaylist As Boolean = False)
            Dim codec As VideoCodecInfo = options.GetCodec(url)
            Dim result As Downloader = Create(codec, options, isPlaylist)
            result.InputUrl = url
            Return result
        End Function
        Public Shared Function Create(ByRef video As YtVideo, options As DownloadOptions, Optional isPlaylist As Boolean = False) As Downloader
            Dim codec As VideoCodecInfo = options.GetCodec(video)
            Dim result As Downloader = Create(codec, options, isPlaylist)
            result.InputUrl = video.ToString
            Return result
        End Function

        Public Shared Function CreateList(link As String, outputPath As String, onlyVideo As Boolean, _
                                  format As String, quality As Int32, action As Action(Of Downloader)) As List(Of Downloader)
            Dim output As New List(Of Downloader)
            Dim dldOps As DownloadOptions = DownloadOptionsBuilder.Build(outputPath, _
                                                                    onlyVideo, format, quality)
            Dim dldr As Downloader
            If YtPlaylist.IsPlaylist(link) Then
                Dim ytl As New YtPlaylist(link, dldOps)
               For Each video As YtVideo In ytl
                    Console.WriteLine(video)
                    dldr = video.GetDownloader(ytl.DownloadOptions, True)
                    If action IsNot Nothing Then
                        action(dldr)
                    End If
                    output.Add(dldr)
                Next
            Else
                dldr = (Downloader.Factory.Create(link, dldOps))
                If action IsNot Nothing Then
                    action(dldr)
                End If
                output.Add(dldr)
            End If
            Return output
        End Function
    End Class

    Public Class Factory(Of TDldType As {Downloader, New})
        Public Shared Function Create(video As VideoCodecInfo, outputFile As String, Optional bytesToDownload As System.Nullable(Of Integer) = Nothing, _
                                      Optional isPlaylsit As Boolean = False) As TDldType
            Dim dldr As Downloader = New TDldType
            dldr.OutputPath = outputFile
            dldr.BytesToDownload = bytesToDownload
            dldr.VideoCodec = video
            Return dldr
        End Function
        Public Shared Function FetchVideo(url As String) As YtVideo
            Dim ytUrl As New YtUrlDecoder()
            Return ytUrl.GetVideo(url)
        End Function
        Public Shared Function Create(url As String, ops As DownloadOptions, Optional isPlaylsit As Boolean = False) As TDldType
            If String.IsNullOrEmpty(url) Then Throw New ArgumentNullException("url")
            Dim ytVid As YtVideo = FetchVideo(url)
            Dim vCodec As VideoCodecInfo = Nothing
            If ops.Filter Is Nothing Then
                If ops.OnlyVideo Then
                    vCodec = (From xVideo In ytVid.Codecs Select xVideo Order By xVideo.AudioBitrate Take 1).FirstOrDefault
                Else
                    vCodec = (From xVideo In ytVid.Codecs Select xVideo Where xVideo.CanExtractAudio Order By xVideo.AudioBitrate Take 1).FirstOrDefault
                End If

            Else
                vCodec = ops.GetCodec(ytVid)
                If vCodec Is Nothing Then
                    Throw New VideoNotAvailableException("Can't find a valid video codec.")
                End If
            End If
            Dim result As Downloader = Create(vCodec, ops.Output, ops.SizeLimit)
            result.InputUrl = url
            Return result
        End Function
        Public Shared Async Function CreateAsync(url As String, ops As DownloadOptions, Optional isPlaylist As Boolean = False) As Task(Of TDldType)
            Return Await Task.Factory.StartNew( _
                Function()
                    Return Create(url, ops, isPlaylist)
                End Function)
        End Function
    End Class

End Class