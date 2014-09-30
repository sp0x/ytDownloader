Namespace Extraction
    Public Class YtVideo
        Public Id As String
        Public Codecs As IEnumerable(Of VideoCodecInfo)
        Public Sub New(id As String)
            Me.Id = id
        End Sub

        Public Function GetCodecs() As IEnumerable(Of VideoCodecInfo)
            Dim url As String = ToString()
            Codecs = Downloader.Factory(Of AudioDownloader).FetchVideo(url)
            Return Codecs
        End Function

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(Id) Then
                Throw New InvalidOperationException("Video ID is null")
            End If
            Return String.Format("https://www.youtube.com/watch?v={0}", Id)
        End Function
    End Class
End NameSpace