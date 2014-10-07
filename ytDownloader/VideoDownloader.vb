Imports System
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports ytDownloader.Extraction


''' <summary>
''' Provides a method to download a video from YouTube.
''' </summary>
Public Class VideoDownloader
    Inherits Downloader
    Public Const DownloadChunkSize As Int32 = 15 * 65536
    Public Sub New(videoCodec As VideoCodecInfo)
        MyBase.New()
        Me.VideoCodec = videoCodec
    End Sub
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Occurs when the downlaod progress of the video file has changed.
    ''' </summary>
    Public Shadows Event DownloadProgressChanged As EventHandler(Of ProgressEventArgs)

    ''' <summary>
    ''' Starts the video download.
    ''' </summary>
    ''' <exception cref="IOException">The video file could not be saved.</exception>
    ''' <exception cref="WebException">An error occured while downloading the video.</exception>
    Protected Overrides Sub StartDownloading()
        If VideoCodec Is Nothing Then
            Throw New VideoNotAvailableException("Video codec not set")
        End If
        MyBase.RaiseDownloadStarted(Me, EventArgs.Empty)

        Dim request As HttpWebRequest = HttpWebRequest.Create(Me.VideoCodec.DownloadUrl)
        request.MaximumResponseHeadersLength = -1 'DownloadChunkSize
        request.KeepAlive = True
       

        If Options.SizeLimit > 0 Then
            request.AddRange(0, Options.SizeLimit - 1)
        End If

        ' the following code is alternative, you may implement the function after your needs
        Using response As WebResponse = request.GetResponse()
            Using source As Stream = response.GetResponseStream()
                Using target As FileStream = File.Open(Me.OutputPath, FileMode.Create, FileAccess.Write)
                    Dim buffer As Byte() = New Byte(DownloadChunkSize) {}
                    Dim cancel As Boolean = False
                    Dim bytes As Integer
                    Dim copiedBytes As Integer = 0
                    Dim argUpdate As New ProgressEventArgs(0)
                    argUpdate.Flag = ProgressFlags.Download
                    bytes = source.Read(buffer, 0, buffer.Length)
                    While Not cancel AndAlso bytes > 0
                        target.Write(buffer, 0, bytes)
                        copiedBytes += bytes
                        argUpdate.ProgressPercentage = (copiedBytes * 1.0 / response.ContentLength) * 100
                        If IsUpdateReady(argUpdate) Then
                            RaiseEvent DownloadProgressChanged(Me, argUpdate)
                            MyBase.RaiseDownloadProgressChanged(Me, argUpdate)
                            cancel = argUpdate.Cancel
                        End If
                        If source.CanRead Then
                            bytes = source.Read(buffer, 0, buffer.Length)
                        Else
                            bytes = 0
                        End If
                    End While
                End Using
            End Using
        End Using
        MyBase.RaiseDownloadFinished(Me, New IoFinishedEventArgs() With {.Path = Me.OutputPath})
    End Sub
End Class