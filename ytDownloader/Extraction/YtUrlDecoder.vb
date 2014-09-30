Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq
Imports ytDownloader.STD
Imports ytDownloader.Extraction
Imports ytDownloader.UrlExtensions

Imports System.Text.Encoding


Namespace Extraction
    '''<summary>
    '''Provides a method to get the download link of a YouTube video.
    '''</summary>
    Public Class YtUrlDecoder

        Private Const CorrectSignatureLength As Integer = 81
        Private Const SignatureQuery As String = "signature"
        Private Property ytDecoder As YtSignitureDecoder


        '''<summary>
        '''Decrypts the signature in the <see cref="VideoCodecInfo.DownloadUrl" /> property and sets it
        '''to the decrypted URL. Use this method, if you have decryptSignature in the <see
        '''cref="GetDownloadUrls" /> method set to false.
        '''</summary>
        '''<param name="videoInfo">The video info which's downlaod URL should be decrypted.</param>
        '''<exception cref="YoutubeParseException">
        '''There was an error while deciphering the signature.
        '''</exception>
        Public Sub DecryptDownloadUrl(videoInfo As VideoCodecInfo)

            Dim queries As IDictionary(Of String, String) = URLHelper.ParseQueryString(videoInfo.DownloadUrl)
            If (queries.ContainsKey(SignatureQuery)) Then
                Dim encryptedSignature As String = queries(SignatureQuery)
                Dim decrypted As String = ""
                Try
                    decrypted = GetDecipheredSignature(videoInfo.HtmlPlayerVersion, encryptedSignature)
                Catch ex As Exception
                    Throw New YoutubeParseException("Could not decipher signature", ex)
                End Try
                videoInfo.DownloadUrl = URLHelper.ReplaceQueryStringParameter(videoInfo.DownloadUrl, SignatureQuery, decrypted)
                videoInfo.RequiresDecryption = False
            End If
        End Sub

        Private Shared rxVideoId As New Regex("(watch\?v=)(.*?)(&|$)", RegexOptions.Compiled Or RegexOptions.IgnoreCase)
        ''' <summary>
        ''' Gets a list of <see cref="VideoCodecInfo" />s for the specified URL.
        ''' </summary>
        ''' <param name="videoUrl">The URL of the YouTube video.</param>
        ''' <param name="decryptSignature">
        ''' A value indicating whether the video signatures should be decrypted or not. Decrypting
        ''' consists of a HTTP request for each <see cref="VideoCodecInfo" />, so you may want to set
        ''' this to false and call <see cref="DecryptDownloadUrl" /> on your selected <see
        ''' cref="VideoCodecInfo" /> later.
        ''' </param>
        ''' <returns>A list of <see cref="VideoCodecInfo" />s that can be used to download the video.</returns>
        ''' <exception cref="ArgumentNullException">
        ''' The <paramref name="videoUrl" /> parameter is <c>null</c>.
        ''' </exception>
        ''' <exception cref="ArgumentException">
        ''' The <paramref name="videoUrl" /> parameter is not a valid YouTube URL.
        ''' </exception>
        ''' <exception cref="VideoNotAvailableException">The video is not available.</exception>
        ''' <exception cref="WebException">
        ''' An error occurred while downloading the YouTube page html.
        ''' </exception>
        ''' <exception cref="YoutubeParseException">The Youtube page could not be parsed.</exception>
        Public Function GetVideo(videoUrl As String, Optional decryptSignature As Boolean = True) As YtVideo
            If (videoUrl Is Nothing) Then Throw New ArgumentNullException("videoUrl")
            If (Not TryNormalizeYoutubeUrl(videoUrl)) Then
                Throw New ArgumentException("URL is not a valid youtube URL!")
            End If
            Dim id As String = rxVideoId.MatchGroupValue(videoUrl, 2)
            If id Is Nothing Then
                Throw New ArgumentException("URL is not a valid youtube URL!")
            End If

            Try
                Dim json = LoadYTPlayerJson(videoUrl)
                Dim videoTitle As String = GetVideoTitle(json)
                Dim downloadUrls As IEnumerable(Of ExtractionInfo) = ExtractDownloadUrls(json)
                Dim infos As IEnumerable(Of VideoCodecInfo) = GetVideoInfos(downloadUrls, videoTitle).ToList()
                Dim htmlPlayerVersion As String = GetHtml5PlayerVersion(json)
                For Each info As VideoCodecInfo In infos
                    info.HtmlPlayerVersion = htmlPlayerVersion
                    If (decryptSignature And info.RequiresDecryption) Then
                        DecryptDownloadUrl(info)
                    End If
                Next
                Return New YtVideo(id) With {.Codecs = infos}
            Catch ex As Exception
                If (TypeOf ex Is WebException Or TypeOf ex Is VideoNotAvailableException) Then
                    Throw New Exception
                End If
                ThrowYoutubeParseException(ex, videoUrl)
            End Try
            Return Nothing
        End Function

        Public Function ParseVideoPage(videoPage As String, videoId As String, Optional decryptSignature As Boolean = True) As YtVideo
            Try
                Dim json = LoadYTPlayerJson("", videoPage)
                Dim videoTitle As String = GetVideoTitle(json)
                Dim downloadUrls As IEnumerable(Of ExtractionInfo) = ExtractDownloadUrls(json)
                Dim infos As IEnumerable(Of VideoCodecInfo) = GetVideoInfos(downloadUrls, videoTitle).ToList()
                Dim htmlPlayerVersion As String = GetHtml5PlayerVersion(json)
                For Each info As VideoCodecInfo In infos
                    info.HtmlPlayerVersion = htmlPlayerVersion
                    If (decryptSignature And info.RequiresDecryption) Then
                        DecryptDownloadUrl(info)
                    End If
                Next
                Return New YtVideo(videoId) With {.Codecs = infos}
            Catch ex As Exception
                If (TypeOf ex Is WebException Or TypeOf ex Is VideoNotAvailableException) Then
                    Throw New Exception
                End If
                ThrowYoutubeParseException(ex, "YtPlaylist")
            End Try
            Return Nothing
        End Function

#If PORTABLE Then

Public Shared Function GetDownloadUrlsAsync(videoUrl As String, Optional decryptSignature As Boolean = True) As System.Threading.Tasks.Task(Of IEnumerable(Of VideoCodecInfo))
    Return System.Threading.Tasks.Task.Run(Function() GetDownloadUrls(videoUrl, decryptSignature))
End Function

#End If
        '''<summary>
        '''Normalizes the given YouTube URL to the format http://youtube.com/watch?v={youtube-id}
        '''and returns whether the normalization was successful or not.
        '''</summary>
        '''<param name="url">The YouTube URL to normalize.</param>
        '''<returns>
        '''<c>true</c>, if the normalization was successful; <c>false</c>, if the URL is invalid.
        '''</returns>
        Public Shared Function TryNormalizeYoutubeUrl(ByRef url As String) As Boolean
            url = url.Trim()
            url = url.Replace("youtu.be/", "youtube.com/watch?v=")
            url = url.Replace("www.youtube", "youtube")
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=")
            If (url.Contains("/v/")) Then
                url = sprintf("http://youtube.com{0}", New Uri(url).AbsolutePath.Replace("/v/", "/watch?v="))
            End If
            url = url.Replace("/watch#", "/watch?")
            Dim query As Dictionary(Of String, String) = URLHelper.ParseQueryString(url)
            Dim v As String = ""
            If (Not query.TryGetValue("v", v)) Then
                url = Nothing
                Return False
            End If
            url = sprintf("http://youtube.com/watch?v={0}", v)
            If query.Keys.Contains("list") Then url = String.Format("{0}&{1}={2}", url, "list", query("list"))
            Return True
        End Function

        Private Shared Function ExtractDownloadUrls(json As JObject) As IEnumerable(Of ExtractionInfo)
            Dim output As New List(Of ExtractionInfo)
            Dim splitByUrls As String() = GetStreamMap(json).Split(",")
            Dim adaptiveFmtSplitByUrls As String() = GetAdaptiveStreamMap(json).Split(",")
            splitByUrls = splitByUrls.Concat(adaptiveFmtSplitByUrls).ToArray()

            For Each s As String In splitByUrls
                Dim queries As Dictionary(Of String, String) = URLHelper.ParseQueryString(s)
                Dim url As String
                Dim requiresDecryption As Boolean = False
                If (queries.ContainsKey("s") Or queries.ContainsKey("sig")) Then

                    requiresDecryption = queries.ContainsKey("s")
                    Dim signature As String = If(queries.ContainsKey("s"), queries("s"), queries("sig"))
                    url = String.Format("{0}&{1}={2}", queries("url"), SignatureQuery, signature)
                    Dim fallbackHost As String = If(queries.ContainsKey("fallback_host"), _
                                                     "&fallback_host=" + queries("fallback_host"), String.Empty)

                    url += fallbackHost
                Else
                    url = queries("url")
                End If
                url = HtmlDecoder.Decode(url)
                url = url.Urldecode
                output.Add(New ExtractionInfo With {.RequiresDecryption = requiresDecryption, .Uri = New Uri(url)})
            Next
            Return output
        End Function

        Private Shared Function GetAdaptiveStreamMap(json As JObject) As String
            Dim streamMap As JToken = json("args")("adaptive_fmts")
            Return streamMap.ToString()
        End Function

        Private Function GetDecipheredSignature(htmlPlayerVersion As String, signature As String) As String
            If (signature.Length = CorrectSignatureLength) Then
                Return signature
            End If
            If ytDecoder IsNot Nothing Then
                If Not ytDecoder.PlayerVersion.Equals(htmlPlayerVersion) Then ytDecoder = New YtSignitureDecoder(htmlPlayerVersion)
            Else
                ytDecoder = New YtSignitureDecoder(htmlPlayerVersion)
            End If
            Return ytDecoder.DecipherWithVersion(signature)
        End Function

        Private Shared Function GetHtml5PlayerVersion(json As JObject) As String
            Dim regex As New Regex("html5player-(.+?)\.js")
            Dim js As String = json("assets")("js").ToString()
            Return regex.Match(js).Result("$1")
        End Function

        Private Shared Function GetStreamMap(json As JObject) As String
            Dim streamMap As JToken = json("args")("url_encoded_fmt_stream_map")
            Dim streamMapString As String = If(streamMap Is Nothing, Nothing, streamMap.ToString()) '' streamMap == null ? null : streamMap.ToString()
            If (streamMapString Is Nothing Or streamMapString.Contains("been+removed")) Then
                Throw New VideoNotAvailableException("Video is removed or has an age restriction.")
            End If
            Return streamMapString
        End Function

        Private Function GetVideoInfos(extractionInfos As IEnumerable(Of ExtractionInfo), videoTitle As String) As IEnumerable(Of VideoCodecInfo)

            Dim downLoadInfos As New List(Of VideoCodecInfo)()

            For Each extractionInfo As ExtractionInfo In extractionInfos
                Dim itag As String = URLHelper.ParseQueryString(extractionInfo.Uri.Query)("itag")
                Dim formatCode As Int32 = Int32.Parse(itag)

                Dim info As VideoCodecInfo = VideoCodecInfo.Defaults.SingleOrDefault(Function(vi) vi.FormatCode = formatCode)

                If (info IsNot Nothing) Then
                    info = New VideoCodecInfo(info) With _
                    {
                        .DownloadUrl = extractionInfo.Uri.ToString(), _
                        .Title = videoTitle, _
                        .RequiresDecryption = extractionInfo.RequiresDecryption
                    }
                Else
                    info = New VideoCodecInfo(formatCode) With {.DownloadUrl = extractionInfo.Uri.ToString()}
                End If
                info.DownloadUrl = URLHelper.LinkSetArg(info.DownloadUrl, "ratebypass", "yes")
                downLoadInfos.Add(info)
            Next
            Return downLoadInfos
        End Function

        Private Shared Function GetVideoTitle(json As JObject) As String
            Dim title As JToken = json("args")("title")
            Return If(title Is Nothing, "", title.ToString())
        End Function

        Public Shared Function IsVideoUnavailable(pageSource As String) As Boolean
            Const unavailableContainer As String = "<div id=\""watch-player-unavailable\"">"""
            Return pageSource.Contains(unavailableContainer)
        End Function

        Private Shared Function LoadYTPlayerJson(url As String, Optional pageSource As String = Nothing) As JObject
            If String.IsNullOrEmpty(pageSource) Then
                pageSource = New WebClient() With {.Encoding = UTF8}.DownloadString(url) 'URLHelper.DldurlTxt(url, UTF8)
            End If
            If String.IsNullOrEmpty(pageSource) Then
                Throw New NullReferenceException("Could not fetch URL's content!")
                Return Nothing
            End If
            If (IsVideoUnavailable(pageSource)) Then
                Throw New VideoNotAvailableException()
            End If

            Dim dataRegex As New Regex("ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline)
            Dim rxMatch As Match = dataRegex.Match(pageSource)
            If rxMatch Is Nothing Then
                ThrowYoutubeParseException(New Exception("INVALID JSON!"), url)
            End If
            Dim extractedJson As String = rxMatch.Result("$1")
            Return JObject.Parse(extractedJson)
        End Function

        Private Shared Sub ThrowYoutubeParseException(innerException As Exception, videoUrl As String)
            Throw New YoutubeParseException("Could not parse the Youtube page for URL " & videoUrl + "\n" & _
                            "This may be due to a change of the Youtube page structure.\n" & _
                            "Please report this bug at www.github.com/flagbug/YoutubeExtractor/issues", innerException)
        End Sub

        Private Class ExtractionInfo
            Public Property RequiresDecryption As Boolean
            Public Property Uri As Uri
        End Class

    End Class

End Namespace
