﻿Imports ytDownloader.Extraction

Partial Public Class Downloader
    Public Class Factory
        Public Shared Function Create(options As DownloadOptions) As Downloader
            Dim dldr As Downloader
            If options.OnlyVideo Then
                options.Output = System.IO.Path.ChangeExtension(options.Output, options.SelectedCodec.VideoExtension)
                dldr = Factory(Of VideoDownloader).Create(options.SelectedCodec, options.Output)
            Else
                options.Output = System.IO.Path.ChangeExtension(options.Output, options.SelectedCodec.AudioExtension)
                dldr = Factory(Of AudioDownloader).Create(options.SelectedCodec, options.Output)
            End If
            dldr.BytesToDownload = options.ByteLitmit
            Return dldr
        End Function
    End Class

    Public Class Factory(Of TDldType As {Downloader, New})
        Public Shared Function Create(video As VideoCodecInfo, outputFile As String, Optional bytesToDownload As System.Nullable(Of Integer) = Nothing)
            Dim dldr As Downloader = New TDldType
            dldr.OutputPath = outputFile
            dldr.BytesToDownload = bytesToDownload
            dldr.VideoCodec = video
            Return dldr
        End Function
        Public Shared Function FetchVideo(url As String) As YtVideo
            Dim vCodec As VideoCodecInfo = Nothing
            Dim ytUrl As New YtUrlDecoder()
            Return ytUrl.GetVideo(url)
        End Function
        Public Shared Function Create(url As String, outputFile As String, Optional bytesToDownload As Integer = 0, Optional codecSelector As Func(Of VideoCodecInfo, Boolean) = Nothing)
            If String.IsNullOrEmpty(url) Then Throw New ArgumentNullException("url")
            Dim vModes As IEnumerable(Of VideoCodecInfo) = FetchVideo(url)
            Dim vCodec As VideoCodecInfo = Nothing
            If codecSelector Is Nothing Then
                vCodec = (From xVideo In vModes Select xVideo Where xVideo.CanExtractAudio Order By xVideo.AudioBitrate Take 1).FirstOrDefault
            Else
                vCodec = vModes.FirstOrDefault(codecSelector)
                If vCodec Is Nothing Then
                    Throw New VideoNotAvailableException("Can't find a valid video codec.")
                End If
            End If
            Return Create(vCodec, outputFile, bytesToDownload)
        End Function
        Public Shared Async Function CreateAsync(url As String, outputFile As String, Optional bytesToDownload As Int32 = 0, _
                                                 Optional codecSelector As Func(Of VideoCodecInfo, Boolean) = Nothing) As Task(Of AudioDownloader)
            Return Await Task.Factory.StartNew( _
                Function()
                    Return Create(url, outputFile, bytesToDownload, codecSelector)
                End Function)
        End Function
    End Class

End Class