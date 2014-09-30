Namespace Extraction
    Partial Public Class Downloader
        Public Class Factory(Of TDldType As {Downloader, New})
            Public Shared Function Create(video As VideoCodecInfo, outputFile As String, Optional bytesToDownload As System.Nullable(Of Integer) = Nothing)
                Dim dldr As Downloader = New TDldType
                dldr.OutputPath = outputFile
                dldr.BytesToDownload = bytesToDownload
                dldr.VideoCodec = video
                Return dldr
            End Function
            Public Shared Function Create(ByVal url As String, ByVal outputFile As String, Optional ByVal bytesToDownload As Integer = 0, Optional ByVal codecSelector As Func(Of VideoCodecInfo, Boolean) = Nothing)
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
            Public Shared Async Function CreateAsync(url As String, outputFile As String, Optional bytesToDownload As Int32 = 0, _
                                                     Optional codecSelector As Func(Of VideoCodecInfo, Boolean) = Nothing) As Task(Of AudioDownloader)
                Return Await Task.Factory.StartNew( _
                    Function()
                        Return Create(url, outputFile, bytesToDownload, codecSelector)
                    End Function)
            End Function
        End Class

    End Class
End Namespace