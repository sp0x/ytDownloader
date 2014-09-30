Imports ytDownloader.Extraction

Public Class DownloadOptionsBuilder
    Public Property OnlyVideo As Boolean
    Public Property Format As String
    Public Property Quality As Int32
    Public Property Url As String
    Public Property Output As String

    Public Sub New(ByRef onlyvideo As Boolean, Optional format As String = "mp3", Optional quality As Int32 = 0)
        Me.OnlyVideo = onlyvideo
        Me.Format = format
        Me.Quality = quality
    End Sub
    Public Sub New(videoUrl As String, outputPath As String, ByRef onlyvideo As Boolean, Optional format As String = "mp3", Optional quality As Int32 = 0)
        Url = videoUrl
        Output = outputPath
        Me.OnlyVideo = onlyvideo
        Me.Format = format
        Me.Quality = quality
    End Sub
    ''' <summary>
    ''' The most prefered way of getting a downloader.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetDownloader() As Downloader
        'fetch the codecs, find the right one, create the downloader
        Dim ytVid = Downloader.Factory(Of VideoDownloader).FetchVideo(Url)
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
    Public Function CompileCodecSelector() As Func(Of VideoCodecInfo, Boolean)
        'If String.IsNullOrEmpty(format) Then format = "mp3"
        If {"flv", "mp4", "webm", "3gp"}.Contains(format) Then
            onlyvideo = True
        End If
        Dim onlyVideoBk As Boolean = onlyvideo
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