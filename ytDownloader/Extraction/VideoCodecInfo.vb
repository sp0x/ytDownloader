Imports System.Collections.Generic
Namespace Extraction
    Public Class VideoCodecInfo
        Friend Shared Defaults As IEnumerable(Of VideoCodecInfo) = New List(Of VideoCodecInfo)() From { _
          New VideoCodecInfo(5, VideoType.Flash, 240, False, AudioType.Mp3, 64, _
            AdaptiveType.None), _
          New VideoCodecInfo(6, VideoType.Flash, 270, False, AudioType.Mp3, 64, _
            AdaptiveType.None), _
          New VideoCodecInfo(13, VideoType.Mobile, 0, False, AudioType.Aac, 0, _
            AdaptiveType.None), _
          New VideoCodecInfo(17, VideoType.Mobile, 144, False, AudioType.Aac, 24, _
            AdaptiveType.None), _
          New VideoCodecInfo(18, VideoType.Mp4, 360, False, AudioType.Aac, 96, _
            AdaptiveType.None), _
          New VideoCodecInfo(22, VideoType.Mp4, 720, False, AudioType.Aac, 192, _
            AdaptiveType.None), _
          New VideoCodecInfo(34, VideoType.Flash, 360, False, AudioType.Aac, 128, _
            AdaptiveType.None), _
          New VideoCodecInfo(35, VideoType.Flash, 480, False, AudioType.Aac, 128, _
            AdaptiveType.None), _
          New VideoCodecInfo(36, VideoType.Mobile, 240, False, AudioType.Aac, 38, _
            AdaptiveType.None), _
          New VideoCodecInfo(37, VideoType.Mp4, 1080, False, AudioType.Aac, 192, _
            AdaptiveType.None), _
          New VideoCodecInfo(38, VideoType.Mp4, 3072, False, AudioType.Aac, 192, _
            AdaptiveType.None), _
          New VideoCodecInfo(43, VideoType.WebM, 360, False, AudioType.Vorbis, 128, _
            AdaptiveType.None), _
          New VideoCodecInfo(44, VideoType.WebM, 480, False, AudioType.Vorbis, 128, _
            AdaptiveType.None), _
          New VideoCodecInfo(45, VideoType.WebM, 720, False, AudioType.Vorbis, 192, _
            AdaptiveType.None), _
          New VideoCodecInfo(46, VideoType.WebM, 1080, False, AudioType.Vorbis, 192, _
            AdaptiveType.None), _
          New VideoCodecInfo(82, VideoType.Mp4, 360, True, AudioType.Aac, 96, _
            AdaptiveType.None), _
          New VideoCodecInfo(83, VideoType.Mp4, 240, True, AudioType.Aac, 96, _
            AdaptiveType.None), _
          New VideoCodecInfo(84, VideoType.Mp4, 720, True, AudioType.Aac, 152, _
            AdaptiveType.None), _
          New VideoCodecInfo(85, VideoType.Mp4, 520, True, AudioType.Aac, 152, _
            AdaptiveType.None), _
          New VideoCodecInfo(100, VideoType.WebM, 360, True, AudioType.Vorbis, 128, _
            AdaptiveType.None), _
          New VideoCodecInfo(101, VideoType.WebM, 360, True, AudioType.Vorbis, 192, _
            AdaptiveType.None), _
          New VideoCodecInfo(102, VideoType.WebM, 720, True, AudioType.Vorbis, 192, _
            AdaptiveType.None), _
          New VideoCodecInfo(133, VideoType.Mp4, 240, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(134, VideoType.Mp4, 360, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(135, VideoType.Mp4, 480, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(136, VideoType.Mp4, 720, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(137, VideoType.Mp4, 1080, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(160, VideoType.Mp4, 144, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(242, VideoType.WebM, 240, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(243, VideoType.WebM, 360, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(244, VideoType.WebM, 480, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(247, VideoType.WebM, 720, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(248, VideoType.WebM, 1080, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(264, VideoType.Mp4, 1440, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(278, VideoType.WebM, 144, False, AudioType.Unknown, 0, _
            AdaptiveType.Video), _
          New VideoCodecInfo(139, VideoType.Mp4, 0, False, AudioType.Aac, 48, _
            AdaptiveType.Audio), _
          New VideoCodecInfo(140, VideoType.Mp4, 0, False, AudioType.Aac, 128, _
            AdaptiveType.Audio), _
          New VideoCodecInfo(141, VideoType.Mp4, 0, False, AudioType.Aac, 256, _
            AdaptiveType.Audio), _
          New VideoCodecInfo(171, VideoType.WebM, 0, False, AudioType.Vorbis, 128, _
            AdaptiveType.Audio), _
          New VideoCodecInfo(172, VideoType.WebM, 0, False, AudioType.Vorbis, 192, _
            AdaptiveType.Audio) _
        }

        Friend Sub New(formatCode As Integer)
            Me.New(formatCode, VideoType.Unknown, 0, False, AudioType.Unknown, 0, _
              AdaptiveType.None)
        End Sub

        Friend Sub New(info As VideoCodecInfo)
            Me.New(info.FormatCode, info.VideoType, info.Resolution, info.Is3D, info.AudioType, info.AudioBitrate, _
              info.AdaptiveType)
        End Sub

        Private Sub New(formatCode As Integer, videoType As VideoType, resolution As Integer, is3D As Boolean, audioType As AudioType, audioBitrate As Integer, _
          adaptiveType As AdaptiveType)
            Me.FormatCode = formatCode
            Me.VideoType = videoType
            Me.Resolution = resolution
            Me.Is3D = is3D
            Me.AudioType = audioType
            Me.AudioBitrate = audioBitrate
            Me.AdaptiveType = adaptiveType
        End Sub

        ''' <summary>
        ''' Gets an enum indicating whether the format is adaptive or not.
        '''</summary>
        '''<value>
        '''<c>AdaptiveType.Audio</c> or <c>AdaptiveType.Video</c> if the format is adaptive;
        '''otherwise, <c>AdaptiveType.None</c>
        '''</value>
        Public Property AdaptiveType() As AdaptiveType
        '''<summary>"
        '''The approximate audio bitrate in kbit/s."
        '''</summary>"
        '''<value>The approximate audio bitrate in kbit/s, or 0 if the bitrate is unknown.</value>"
        Public Property AudioBitrate() As Integer
        '''<summary>"
        '''Gets the audio extension."
        '''</summary>"
        '''<value>The audio extension, or <c>null</c> if the audio extension is unknown.</value>"
        Public ReadOnly Property AudioExtension() As String
            Get
                Select Case Me.AudioType
                    Case AudioType.Aac
                        Return ".aac"

                    Case AudioType.Mp3
                        Return ".mp3"

                    Case AudioType.Vorbis
                        Return ".ogg"
                End Select

                Return Nothing
            End Get
        End Property

        '''<summary>"
        '''Gets the audio type (encoding)."
        '''</summary>"
        Public Property AudioType() As AudioType
        '''<summary>"
        '''Gets a value indicating whether the audio of this video can be extracted by YoutubeExtractor."
        '''</summary>"
        '''<value>"
        '''<c>true</c> if the audio of this video can be extracted by YoutubeExtractor; otherwise, <c>false</c>."
        '''</value>"
        Public ReadOnly Property CanExtractAudio() As Boolean
            Get
                Return Me.VideoType = VideoType.Flash
            End Get
        End Property
        '''<summary>"
        '''Gets the download URL."
        '''</summary>"
        Public Property DownloadUrl() As String
        '''<summary>"
        '''Gets the format code, that is used by YouTube internally to differentiate between"
        '''quality profiles."
        '''</summary>"
        Public Property FormatCode() As Integer
        Public Property Is3D() As Boolean
        '''<summary>"
        '''Gets a value indicating whether this video info requires a signature decryption before"
        '''the download URL can be used."
        '''
        '''This can be achieved with the <see cref=DownloadUrlResolver.DecryptDownloadUrl/>"
        '''</summary>"
        Public Property RequiresDecryption() As Boolean
        '''<summary>"
        '''Gets the resolution of the video."
        '''</summary>"
        '''<value>The resolution of the video, or 0 if the resolution is unkown.</value>"
        Public Property Resolution() As Integer
        '''<summary>"
        '''Gets the video title."
        '''</summary>"
        Public Property Title() As String
        '''<summary>"
        '''Gets the video extension."
        '''</summary>"
        '''<value>The video extension, or <c>null</c> if the video extension is unknown.</value>"
        Public ReadOnly Property VideoExtension() As String
            Get
                Select Case Me.VideoType
                    Case VideoType.Mp4
                        Return ".mp4"

                    Case VideoType.Mobile
                        Return ".3gp"

                    Case VideoType.Flash
                        Return ".flv"

                    Case VideoType.WebM
                        Return ".webm"
                End Select

                Return Nothing
            End Get
        End Property
        '''<summary>"
        '''Gets the video type (container)."
        '''</summary>"
        Public Property VideoType() As VideoType
        '''<summary>"
        '''We use this in the <see cref=§§DownloadUrlResolver.DecryptDownloadUrl§§ /> method to"
        '''decrypt the signature"
        '''</summary>"
        '''<returns></returns>"
        Friend Property HtmlPlayerVersion() As String




        Public Overrides Function ToString() As String
            Return String.Format("Full Title: {0}, Type: {1}, Resolution: {2}p", Me.Title + Me.VideoExtension, Me.VideoType, Me.Resolution)
        End Function
    End Class


End Namespace