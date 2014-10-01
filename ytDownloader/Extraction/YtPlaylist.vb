Namespace Extraction
    Public Class YtPlaylist
        Implements IEnumerable(Of YtVideo)
        Public Property Url As String
        Public Property DownloadOptions As DownloadOptionsBuilder
        Public Sub New(plUrl As String, Optional dldOps As DownloadOptionsBuilder = Nothing)
            Url = plUrl
            DownloadOptions = dldOps
        End Sub

        Public Function GetEnumerator() As IEnumerator(Of YtVideo) Implements IEnumerable(Of YtVideo).GetEnumerator
            Return New YtPlaylistEnumerator(Url, DownloadOptions)
        End Function

        Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return New YtPlaylistEnumerator(Url, DownloadOptions)
        End Function
    End Class
End NameSpace