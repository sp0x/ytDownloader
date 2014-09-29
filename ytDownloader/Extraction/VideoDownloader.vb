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
        ' ''' <summary>
        ' ''' Initializes a new instance of the <see cref="VideoDownloader"/> class.
        ' ''' </summary>
        ' ''' <param name="video">The video to download.</param>
        ' ''' <param name="savePath">The path to save the video.</param>
        ' ''' <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        ' ''' <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        'Public Sub New(video As VideoCodecInfo, savePath As String, Optional bytesToDownload As System.Nullable(Of Integer) = Nothing)
        '    MyBase.New(video, savePath, bytesToDownload)
        'End Sub

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
            MyBase.raiseDownloadStarted(Me, EventArgs.Empty)

            Dim request As HttpWebRequest = HttpWebRequest.Create(Me.videoCodec.DownloadUrl)

            If Me.bytesToDownload > 0 Then
                request.AddRange(0, Me.bytesToDownload.Value - 1)
            End If

            ' the following code is alternative, you may implement the function after your needs
            Using response As WebResponse = request.GetResponse()
                Using source As Stream = response.GetResponseStream()
                    Using target As FileStream = File.Open(Me.audioPath, FileMode.Create, FileAccess.Write)
                        Dim buffer As Byte() = New Byte(1 * 1000 * 1024) {}
                        Dim cancel As Boolean = False
                        Dim bytes As Integer
                        Dim copiedBytes As Integer = 0

                        While Not cancel AndAlso (inlineHelper(bytes, source.Read(buffer, 0, buffer.Length)) > 0)
                            target.Write(buffer, 0, bytes)
                            copiedBytes += bytes

                            Dim eventArgs As New ProgressEventArgs((copiedBytes * 1.0 / response.ContentLength) * 100)
                            RaiseEvent DownloadProgressChanged(Me, eventArgs)
                            If eventArgs.Cancel Then cancel = True
                        End While
                    End Using
                End Using
            End Using
            MyBase.RaiseDownloadFinished(Me, New IOFinishedEventArgs() With {.Path = Me.AudioPath})
            End Sub
End Class

End Namespace