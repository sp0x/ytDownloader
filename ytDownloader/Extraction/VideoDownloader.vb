Imports System
Imports System.IO
Imports System.Net
Imports ytDownloader.Extraction
Imports ytDownloader.STD


Namespace Extraction
''' <summary>
''' Provides a method to download a video from YouTube.
''' </summary>
Public Class VideoDownloader
    Inherits Downloader
        Public Const DownloadChunkSize As Int32 = 15 * 65536


        ''' <summary>
        ''' Occurs when the downlaod progress of the video file has changed.
        ''' </summary>
        Public Event DownloadProgressChanged As EventHandler(Of ProgressEventArgs)

        ''' <summary>
        ''' Starts the video download.
        ''' </summary>
        ''' <exception cref="IOException">The video file could not be saved.</exception>
        ''' <exception cref="WebException">An error occured while downloading the video.</exception>
        Public Overrides Sub StartDownloading()
            If VideoCodec Is Nothing Then
                Throw New VideoNotAvailableException("Video codec not set")
            End If
            MyBase.RaiseDownloadStarted(Me, EventArgs.Empty)

            Dim request As HttpWebRequest = HttpWebRequest.Create(Me.VideoCodec.DownloadUrl)
            request.MaximumResponseHeadersLength = -1 'DownloadChunkSize

            If Me.BytesToDownload > 0 Then
                request.AddRange(0, Me.BytesToDownload.Value - 1)
            End If

            ' the following code is alternative, you may implement the function after your needs
            Using response As WebResponse = request.GetResponse()
                Using source As Stream = response.GetResponseStream()
                    Using target As FileStream = File.Open(Me.OutputPath, FileMode.Create, FileAccess.Write)
                        Dim buffer As Byte() = New Byte(DownloadChunkSize) {}
                        Dim cancel As Boolean = False
                        Dim bytes As Integer
                        Dim copiedBytes As Integer = 0

                        While Not cancel AndAlso (inlineHelper(bytes, source.Read(buffer, 0, buffer.Length)) > 0)
                            target.Write(buffer, 0, bytes)
                            copiedBytes += bytes
                            Trace.WriteLine(bytes)
                            Dim eventArgs As New ProgressEventArgs((copiedBytes * 1.0 / response.ContentLength) * 100)
                            eventArgs.Flag = ProgressFlags.Download

                            RaiseEvent DownloadProgressChanged(Me, eventArgs)
                            If eventArgs.Cancel Then cancel = True
                        End While
                    End Using
                End Using
            End Using
            MyBase.RaiseDownloadFinished(Me, New IoFinishedEventArgs() With {.Path = Me.OutputPath})
        End Sub
End Class

End Namespace