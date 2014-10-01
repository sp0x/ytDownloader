Namespace Extraction
    Public Class YtPlaylist
        Implements IEnumerable(Of YtVideo)
        Public Property Url As String
        Public Property DownloadOptions As DownloadOptions
        Public Sub New(plUrl As String, Optional dldOps As DownloadOptions = Nothing)
            Url = plUrl
            DownloadOptions = dldOps
        End Sub

        Public Shared Function IsPlaylist(url As String)
            If String.IsNullOrEmpty(url) Then Return False
            Return Not String.IsNullOrEmpty(YtPlaylistEnumerator.RxList.MatchGroupValue(url, 2))
        End Function

        Public Function GetEnumerator() As IEnumerator(Of YtVideo) Implements IEnumerable(Of YtVideo).GetEnumerator
            Return New YtPlaylistEnumerator(Url, DownloadOptions)
        End Function

        Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return New YtPlaylistEnumerator(Url, DownloadOptions)
        End Function
    End Class
End NameSpace