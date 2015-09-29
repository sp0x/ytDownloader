Imports ytDownloader.Extraction

Public Class DownloadOptionsBuilder
   
    Public Shared Function Build(outputPath As String, ByRef onlyvideo As Boolean, Optional format As String = "mp3", Optional quality As Int32 = 0) As DownloadOptions
        If {"flv", "mp4", "webm", "3gp"}.Contains(format) Then
            onlyvideo = True
        End If
        Dim resOptions As New DownloadOptions(outputPath, onlyvideo, format, quality)

        Dim tmpQual As Int32 = quality
        If quality = -1 Or quality = Int32.MaxValue Then tmpQual = 0
        resOptions.Filter = CompileCodecSelector(onlyvideo, format, tmpQual)
        Return resOptions
    End Function




            ''' <summary>
            ''' Creates a predicate for the filtering of available video codecs.
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
    Public Shared Function CompileCodecSelector(onlyVideo As Boolean, format As String, quality As Int32) As Func(Of VideoCodecInfo, Boolean)
        'If String.IsNullOrEmpty(format) Then format = "mp3"
        Dim onlyVideoBk As Boolean = onlyVideo
        Return Function(vCodec)
                   Dim validCount = 0
                   Dim validsNeeded = 0

                   If Not String.IsNullOrEmpty(format) Then

                       If vCodec.CanExtractAudio AndAlso vCodec.AudioExtension.Substring(1).ToLower = format.ToLower Then
                           If Not vCodec.VideoExtension = ".flv" And Not onlyVideoBk Then
                               ' Trying to download an audio for a non flv file
                               Console.WriteLine("You can only extract audio from FLV formats.")
                               Return False
                           End If
                           validCount += 1
                       End If
                       If Not nonVideoFormat(format.ToLower) And onlyVideoBk Then
                           validCount += Math.Abs(CInt(vCodec.VideoExtension.Substring(1).ToLower = format.ToLower))
                       End If
                       validCount += Math.Abs(CInt(onlyVideoBk))
                       validsNeeded += 1

                   ElseIf (quality > 0) Then
                       If (vCodec.Resolution = CInt(quality)) Then
                           validCount += 1
                       End If
                       validsNeeded += 1
                   ElseIf quality = -1 Then
                       ' Select highest quality
                       Throw New NotImplementedException("Highest quality can not be selected from within the filter.")
                   Else
                       Return True
                   End If

                   Return (validsNeeded > 0 AndAlso validCount = validsNeeded)
               End Function
    End Function

    Private Shared Function nonVideoFormat(formatStr As String) As Boolean
        Return {"mp3", "ogg", "acc"}.Contains(formatStr)
    End Function
End Class