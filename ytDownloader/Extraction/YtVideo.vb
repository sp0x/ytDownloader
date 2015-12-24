Imports System.Text.RegularExpressions
Imports HtmlAgilityPack
Imports Fizzler
Imports Fizzler.Systems.HtmlAgilityPack


Namespace Extraction
    Public Class YtVideo
        Public Property Id As String
        Public Property Codecs As IEnumerable(Of VideoCodecInfo)
        Public Property Available As Boolean
        Public property Name As String
        Public property Author As String
        Public property Duration As ULong
        Public property ViewCount As ULong
        Public property Added As ULong 

        Public Property Description As String
        ''' <summary>
        ''' A song - timestamp list
        ''' </summary>
        ''' <returns></returns>
        Public Property InnerItems As New Dictionary(Of String, String)

        Public Shared ReadOnly RxVideoId As New Regex("(watch\?v=)(.*?)(&|$)", RegexOptions.Compiled Or RegexOptions.IgnoreCase)
        Public Sub New(videoIdOrUrl As String, Optional bAvailable As Boolean = True)
            Me.New(videoIdOrUrl, bAvailable, "")
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="videoIdOrUrl">The videoIdOrUrl to use, this can be a link to the video also.</param>
        ''' <param name="bAvailable"></param>
        ''' <param name="pageSource">Parse a videopage in order to fetch information from it</param>
        Public Sub New(videoIdOrUrl As String, bAvailable As Boolean, pageSource As String)
            Id = GetVideoId(videoIdOrUrl)
            Available = bAvailable
            If Not String.IsNullOrEmpty(pageSource) Then
                Dim htmlDoc = New HtmlDocument
                htmlDoc.LoadHtml(pageSource)
                Dim descObj = htmlDoc.DocumentNode.QuerySelectorAll("#watch-description-text p#eow-description")
                If descObj.Count > 0 Then
                    Dim descNode As HtmlNode = descObj.FirstOrDefault()
                    If descNode IsNot Nothing Then
                        Dim descRows = descNode.InnerHtml.ToString().Split(New String() {"<br>"}, StringSplitOptions.None)
                        Dim fn As String = "onclick=""yt.www.watch.player.seekTo"
                        Dim pattern = New Regex("(.*)\s*(<a.*?>)(.*?)(</a>)(.*)", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
                        For Each descRow In descRows
                            If String.IsNullOrEmpty(descRow) Or Not descRow.Contains(fn) Then Continue For
                            Dim matches As MatchCollection = pattern.Matches(descRow)
                            If matches.Count > 0 Then
                                Dim m = matches(0)
                                Dim title As String = m.Groups(1).Value
                                Try 
                                     If String.IsNullOrEmpty(title) Then title = m.Groups(5).Value 
                                Catch ex As Exception
                                End Try
                                Dim time As String = m.Groups(3).Value
                                Try 
                                    
                                 InnerItems.Add(title, time)
                                Catch ex As Exception
                                End Try
                            End If
                            descRow = descRow

                        Next

                        If descNode.ChildNodes.Count > 0 Then
                            descNode = descNode.ChildNodes.Item(0)
                            Description = descNode.InnerText
                        End If
                    End If
                End If
            End If
        End Sub

        Public Function GetCodecs() As IEnumerable(Of VideoCodecInfo)
            Dim tmpVideo As YtVideo = Downloader.Factory(Of AudioDownloader).FetchVideo(ToString())
            If tmpVideo Is Nothing Then Return Nothing
            Codecs = tmpVideo.Codecs
            Dim codec = Codecs.FirstOrDefault()
            If tmpVideo IsNot Nothing 
                With tmpVideo
                    If(String.IsNullOrEmpty(Name)) then Name = .Name
                    If(String.IsNullOrEmpty(Description)) then Description = .Description
                    ViewCount = .ViewCount
                    If(String.IsNullOrEmpty(Author)) then Author = .Author
                    Added = .Added
                End With
            End If
            Return Codecs
        End Function

        Public Function GetDownloader(options As DownloadOptions, Optional isPlaylist As Boolean = False, Optional lazy As Boolean = True) As Downloader
            Dim result As Downloader = Nothing
            If lazy Then
                result = Downloader.CreateEmpty(ToString(), options, isPlaylist)
            Else
                result = Downloader.Factory.Create(Me, options, isPlaylist, lazy)
            End If
            result.InputUrl = ToString()
            Return result
        End Function

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(Id) Then
                Throw New InvalidOperationException("Video idOrUrl Is null")
            End If
            Return String.Format("https://www.youtube.com/watch?v={0}", Id)
        End Function

        Public shared function ToUrl(idOrUrl As string) As String
            If idOrUrl.StartsWith("http://") Or idOrUrl.StartsWith("https://") Then Return idOrUrl 
            return String.Format("https://www.youtube.com/watch?v={0}", idOrUrl)
        End function

        Public Shared Function GetVideoId(videoId As String) As String
            If Not videoId.Contains("http://") And Not videoId.Contains("https://") Then Return videoId
            Return RxVideoId.MatchGroupValue(videoId, 2)
        End Function
        Public Shared Function GetVideoCoverUrl(videoUrl As String) As String
            Dim id As String = GetVideoId(videoUrl)
            If String.IsNullOrEmpty(id) Then Throw New InvalidOperationException("Invalid Youtube URL!")
            Return String.Format("http://img.youtube.com/vi/{0}/2.jpg", id)
        End Function
    End Class
End Namespace