Imports ytDownloader.Extraction

Partial Public Class Downloader
    Public Class Factory

        ''' <summary>
        ''' Creates a new downloader.
        ''' </summary>
        ''' <param name="codec"></param>
        ''' <param name="ops"></param>
        ''' <param name="isPlaylist"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(codec As VideoCodecInfo, ops As DownloadOptions, Optional isPlaylist As Boolean = False) As Downloader
            Dim downloader As Downloader
            ops.IsPlaylist = isPlaylist

            If ops.OnlyVideo Then
                downloader = Factory(Of VideoDownloader).Create(codec, ops.Output)
            Else
                downloader = Factory(Of AudioDownloader).Create(codec, ops.Output)
            End If

            downloader.IsPlaylistMember = isPlaylist
            downloader.SetInitialized(True)
            Return downloader
        End Function



        Public Shared Function Create(url As String, options As DownloadOptions, Optional isPlaylist As Boolean = False, _
                                      Optional lazy As Boolean = True) As Downloader
            Dim result As Downloader = Nothing
            If lazy Then
                result = Downloader.CreateEmpty(url, options, isPlaylist)
            Else
                Dim codec As VideoCodecInfo = options.GetCodecs(url)
                result = Create(codec, options, isPlaylist)
            End If
            result.InputUrl = url
            Return result
        End Function

        Public Shared Function Create(ByRef video As YtVideo, options As DownloadOptions, Optional isPlaylist As Boolean = False, Optional lazy As Boolean = True) As Downloader
            Dim result As Downloader = Nothing
            If lazy Then
                result = Downloader.CreateEmpty(video.ToString, options, isPlaylist)
            Else
                Dim codec As VideoCodecInfo = options.GetCodec(video)
                result = Create(codec, options, isPlaylist)
            End If
            result.InputUrl = video.ToString
            Return result
        End Function


        Public Shared Async Function CreateListAsync(link As String, outputpath As String, onlyVideo As Boolean, _
                                                     format As String, quality As Int32, _
                                                     Optional isLazy As Boolean = True) As Task(Of List(Of Downloader))

            Return Await Task.Factory.StartNew(Function()
                                                   Return CreateList(link, outputpath, onlyVideo, format, quality, isLazy)
                                               End Function)
        End Function
        Public Shared Async Sub ExecuteListAsync(link As String, outputpath As String, onlyVideo As Boolean, _
                                                  format As String, quality As Int32, action As Action(Of Downloader))

            Await Task.Factory.StartNew(Sub()
                                            ExecuteList(link, outputpath, onlyVideo, format, quality, action)
                                        End Sub)
        End Sub
        Public Shared Sub ExecuteList(link As String, outputPath As String, onlyVideo As Boolean, _
                                  format As String, quality As Int32, action As Action(Of Downloader))
            Dim dldOps As DownloadOptions = DownloadOptionsBuilder.Build(outputPath, _
                                                                   onlyVideo, format, quality)
            ParseLink(link, dldOps, action)
        End Sub


        Public Shared Function CreateList(link As String, outputPath As String, onlyVideo As Boolean, _
                                  format As String, quality As Int32, Optional lazy As Boolean = True) As List(Of Downloader)
            Dim dldOps As DownloadOptions = DownloadOptionsBuilder.Build(outputPath, _
                                                                    onlyVideo, format, quality)
            Return ParseLink(link, dldOps, Nothing, lazy)
        End Function
        ''' <summary>
        ''' Parses a given link with it's options, and returns a list of downloaders.
        ''' </summary>
        ''' <param name="link">The link to the video or playlist</param>
        ''' <param name="dldOps">Download options for codec selection</param>
        ''' <param name="actionOnParse">Action to perform once fetched</param>
        ''' <param name="lazy">Fetch codecs for every downloader, or fetch them later?</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function ParseLink(link As String, dldOps As DownloadOptions, actionOnParse As Action(Of Downloader), _
                                          Optional lazy As Boolean = True) As IEnumerable(Of Downloader)
            Dim output As New List(Of Downloader)
            Dim dldr As Downloader = Nothing
            If YtPlaylist.IsPlaylist(link) Then
                Dim ytl As New YtPlaylist(link, dldOps)
                Console.WriteLine("Downloading playlist " + link)
                For Each video As YtVideo In ytl
                    dldr = video.GetDownloader(ytl.DownloadOptions, True, lazy:=lazy) ' Lazy should be here, no preprocessing

                    If actionOnParse IsNot Nothing Then
                        actionOnParse(dldr)
                    Else
                        output.Add(dldr)
                    End If
                Next
            Else
                dldr = (Downloader.Factory.Create(link, dldOps, lazy:=lazy))
                If actionOnParse IsNot Nothing Then
                    actionOnParse(dldr)
                Else
                    output.Add(dldr)
                End If
            End If
            Return output
        End Function
        ''' <summary>
        ''' Parses a quality string into a valid integer, specifying the quality to select.
        ''' </summary>
        ''' <param name="quality">Quality string. You can also use highest,lowest.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ParseQuality(quality As String) As Int32
            Dim tmpQuality As Int32
            If quality.ToLower = "highest" Then
                tmpQuality = Int32.MaxValue
            ElseIf quality.ToLower = "lowest" Then
                tmpQuality = -1
            Else
                tmpQuality = CInt(quality)
            End If
            Return tmpQuality
        End Function

        Public Shared Function TryParseQuality(ByRef quality As String) As Boolean
            Try
                quality = ParseQuality(quality)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function
    End Class

    Public Class Factory(Of TDldType As {Downloader, New})
        Public Shared Function Create(video As VideoCodecInfo, outputFile As String, Optional bytesToDownload As Int32 = Nothing, _
                                      Optional isPlaylsit As Boolean = False, Optional lazy As Boolean = False) As TDldType
            Dim dldr As Downloader = New TDldType
            dldr.Options = New DownloadOptions
            dldr.Options.SizeLimit = bytesToDownload
            dldr.OutputPath = outputFile
            dldr.VideoCodec = video
            dldr.SetInitialized(True)
            Return dldr
        End Function
        ''' <summary>
        ''' Modifies the type of the downloader, and keeps it's options
        ''' </summary>
        ''' <param name="dldr"></param>
        ''' <remarks></remarks>
        Public Shared Sub SetExtendor(ByRef dldr As Downloader)
            Dim tmpCopy As Downloader = dldr.Clone()
            dldr = New TDldType
            tmpCopy.CloneMembersTo(dldr)
        End Sub


        Public Shared Function FetchVideo(url As String) As YtVideo
            Dim ytUrl As New YtUrlDecoder()
            If String.IsNullOrEmpty(YtVideo.GetVideoId(url)) Then
                Throw New ArgumentException("You must specify a valid url containing the youtube video id.", "url")
            End If
            Try
                Return ytUrl.GetVideo(url)
            Catch ex As VideoNotAvailableException
                Trace.WriteLine(ex.Message)
                Return New YtVideo("", False)
            End Try
        End Function

        Public Shared Async Function CreateAsync(url As String, ops As DownloadOptions, Optional isPlaylist As Boolean = False) As Task(Of TDldType)
            Return Await Task.Factory.StartNew( _
                Function()
                    Return Create(url, ops, isPlaylist)
                End Function)
        End Function

        Public Shared Function Create(url As String, ops As DownloadOptions, Optional isPlaylsit As Boolean = False) As TDldType
            If String.IsNullOrEmpty(url) Then Throw New ArgumentNullException("url")
            Dim ytVid As YtVideo = FetchVideo(url)
            Dim vCodec As VideoCodecInfo = Nothing
            If ops.Filter Is Nothing Then
                If ops.OnlyVideo Then
                    vCodec = (From xVideo In ytVid.Codecs Select xVideo Order By xVideo.AudioBitrate Take 1).FirstOrDefault
                Else
                    vCodec = (From xVideo In ytVid.Codecs Select xVideo Where xVideo.CanExtractAudio Order By xVideo.AudioBitrate Take 1).FirstOrDefault
                End If

            Else
                vCodec = ops.GetCodec(ytVid)
                If vCodec Is Nothing Then
                    Throw New VideoNotAvailableException("Can't find a valid video codec.")
                End If
            End If
            Dim result As Downloader = Create(vCodec, ops.Output, ops.SizeLimit)
            result.InputUrl = url
            Return result
        End Function
    End Class

  
End Class