Imports ytDownloader.Extraction

Public Class DownloadOptionsBuilder
   
    Public Function Build(videoUrl As String, outputPath As String, ByRef onlyvideo As Boolean, Optional format As String = "mp3", Optional quality As Int32 = 0) As DownloadOptions
        Dim ytVid As YtVideo = Downloader.Factory(Of VideoDownloader).FetchVideo(videoUrl)
        If {"flv", "mp4", "webm", "3gp"}.Contains(format) Then
            onlyvideo = True
        End If
        Dim filter = CompileCodecSelector(onlyvideo, format, quality)
        Dim codec As VideoCodecInfo = ytVid.Codecs.FirstOrDefault(filter)
        If codec Is Nothing Then
            Throw New VideoNotAvailableException("Can't find a codec matching the parameters!")
        End If
        If Not onlyvideo Then ' we're interesed when can we download an audio
            onlyvideo = Not codec.CanExtractAudio ' Audio can't be extracted, so fetch the video only
        End If

        Dim resOptions As New DownloadOptions(videoUrl, outputPath, onlyvideo, format, quality)
        resOptions.Filter = Me.CompileCodecSelector(onlyvideo, format, quality)
        resOptions.SelectedCodec = codec
        Return resOptions
    End Function

    ''' <summary>
    ''' The most prefered way of getting a downloader.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetDownloader() As Downloader
        'fetch the codecs, find the right one, create the downloader

        Dim ytVid As YtVideo = Downloader.Factory(Of VideoDownloader).FetchVideo(Url)
        Dim downOps As DownloadOptions = Build(url)
        Dim filter = CompileCodecSelector()
        Dim codec As VideoCodecInfo = ytVid.Codecs.FirstOrDefault(filter)


        If codec Is Nothing Then
            Throw New VideoNotAvailableException("Can't find a codec matching the parameters!")
        End If
        If Not OnlyVideo Then ' we're interesed when can we download an audio
            OnlyVideo = Not codec.CanExtractAudio ' Audio can't be extracted, so fetch the video only
        End If

        Dim dldr As Downloader
        If OnlyVideo Then
            Output = System.IO.Path.ChangeExtension(Output, codec.VideoExtension)
            dldr = Downloader.Factory(Of VideoDownloader).Create(codec, Output)
        Else
            Output = System.IO.Path.ChangeExtension(Output, codec.AudioExtension)
            dldr = Downloader.Factory(Of AudioDownloader).Create(codec, Output)
        End If
        Return dldr
    End Function

    ''' <summary>
    ''' Creates a predicate for the filtering of available video codecs.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CompileCodecSelector(onlyVideo As Boolean, format As String, quality As Int32) As Func(Of VideoCodecInfo, Boolean)
        'If String.IsNullOrEmpty(format) Then format = "mp3"
        Dim onlyVideoBk As Boolean = OnlyVideo
        Return Function(vCodec)
                   Dim validCount = 0
                   Dim validsNeeded = 0

                   If Not String.IsNullOrEmpty(Format) Then

                       If vCodec.CanExtractAudio AndAlso vCodec.AudioExtension.Substring(1).ToLower = Format.ToLower Then
                           If Not vCodec.VideoExtension = ".flv" And Not onlyVideoBk Then
                               ' Trying to download an audio for a non flv file
                               Console.WriteLine("You can only extract audio from FLV formats.")
                               Return False
                           End If
                           validCount += 1
                       End If
                       If vCodec.VideoExtension.Substring(1).ToLower = Format.ToLower And onlyVideoBk Then
                           validCount += 1
                       End If
                       validsNeeded += 1
                   ElseIf (Quality > 0) Then
                       If (vCodec.Resolution = CInt(Quality)) Then
                           validCount += 1
                       End If
                       validsNeeded += 1
                   End If
                   Return (validsNeeded > 0 AndAlso validCount = validsNeeded)
               End Function
    End Function
End Class