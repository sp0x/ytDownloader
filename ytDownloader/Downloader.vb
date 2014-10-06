Imports System.Net
Imports ytDownloader.Extraction
Imports ytDownloader.StringExtensions


''' <summary>
''' Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
''' </summary>
Partial Public MustInherit Class Downloader

#Region "Events"
    ''' <summary>
    ''' Occurs when the download finished.
    ''' </summary>
    Public Event DownloadFinished As EventHandler(Of IoFinishedEventArgs)

    ''' <summary>
    ''' Occurs when the download is starts.
    ''' </summary>
    Public Event DownloadStarted As EventHandler
    Public Event DownloadProgressChanged As EventHandler(Of ProgressEventArgs)
    Public Event ExtractionProgressChanged As EventHandler(Of ProgressEventArgs)


    Friend Sub RaiseDownloadFinished(sender As Object, e As IoFinishedEventArgs)
        RaiseEvent DownloadFinished(sender, e)
    End Sub
    Protected Sub RaiseDownloadStarted(sender As Object, e As EventArgs)
        RaiseEvent DownloadStarted(sender, e)
    End Sub
    Protected Sub RaiseDownloadProgressChanged(sender As Object, e As ProgressEventArgs)
        If e.IsReady OrElse IsUpdateReady(e) Then RaiseEvent DownloadProgressChanged(sender, e)
    End Sub
    Protected Sub RaiseExtractionProgressChanged(sender As Object, e As ProgressEventArgs)
        If e.IsReady OrElse IsUpdateReady(e) Then RaiseEvent ExtractionProgressChanged(sender, e)
    End Sub
#End Region

#Region "Props"
    ''' <summary>
    ''' The url for the video/audio you want to download
    ''' </summary>
    Public Property InputUrl As String

    ''' <summary>
    ''' Gets the number of bytes to download. <c>null</c>, if everything is downloaded.
    ''' </summary>
    Public Property BytesToDownload As Nullable(Of Integer) = Nothing

    ''' <summary>
    ''' Gets the path to save the video/audio.
    ''' </summary>
    Public Property OutputPath As String

    Private _doOptions As New DownloadOptions
    Public Property Options As DownloadOptions
        Get
            If _doOptions Is Nothing Then _doOptions = New DownloadOptions
            Return _doOptions
        End Get
        Set(value As DownloadOptions)
            _doOptions = value
        End Set
    End Property

    Private _vCodec As VideoCodecInfo
    ''' <summary>
    ''' Gets the video to download/convert.
    ''' </summary>
    Public Property VideoCodec As VideoCodecInfo
        Get
            If _vCodec Is Nothing Then
                _vCodec = Options.GetCodec(InputUrl)
                _bInitialized = True
            End If
            Return _vCodec
        End Get
        Set(value As VideoCodecInfo)
            _vCodec = value
        End Set
    End Property

    Public Property IsPlaylistMember As Boolean
    Private Property _bInitialized As Boolean = False
    Public ReadOnly Property Initialized() As Boolean
        Get
            Return _bInitialized
        End Get
    End Property
    Public Sub SetInitialized(bInited As Boolean)
        _bInitialized = bInited
    End Sub


#End Region

#Region "Construction"
    Public Sub New()
    End Sub
    ''' <summary>
    ''' This should be called, only after the first element from the video list ha been fetched.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Initialize() As Downloader
        _vCodec = Options.GetCodec(InputUrl)
        _bInitialized = True
        Return Factory.Create(_vCodec, Options, IsPlaylistMember)
    End Function
    Sub New(url As String, ops As DownloadOptions, isPlaylist As Boolean)
        Me.InputUrl = url
        Me.IsPlaylistMember = isPlaylist
        ops.CloneTo(Me.Options)
        Me.Options.SetOnlyVideo(ops.OnlyVideo)
        'Me.Options.Format = ops.Format
        'Me.Options.Quality = ops.Quality
        'Me.Options.SizeLimit = ops.SizeLimit
        'Me.Options.Output = ops.Output
        'Me.Options.IsPlaylist = ops.IsPlaylist
        'Me.Options.Filter = ops.Filter
        'ops = Nothing
    End Sub
    Public Shared Function CreateEmpty(url As String, ops As DownloadOptions, isPlaylist As Boolean) As Downloader
        Dim dlm As Downloader = New VideoDownloader
        dlm.InputUrl = url
        dlm.IsPlaylistMember = isPlaylist
        ops.CloneTo(dlm.Options)
        dlm.Options.SetOnlyVideo(ops.OnlyVideo)
        Return dlm
    End Function
#End Region


#Region "Update filtering"
    Private Property _updateDelta As Double = 0D
    Public Property UpdateInterval As Double = 2D

    Protected Function IsUpdateReady(ByRef pge As ProgressEventArgs) As Boolean
        Dim shouldPrint As Boolean = False
        Dim delta As Double = Math.Abs(pge.ProgressPercentage - _updateDelta)
        If _updateDelta.Equals(0D) Or _updateDelta.Equals(100D) Then
            shouldPrint = True
            _updateDelta = pge.ProgressPercentage
        ElseIf delta > UpdateInterval Then ' get delta 
            shouldPrint = True
            _updateDelta = pge.ProgressPercentage
        End If
        pge.IsReady = shouldPrint
        Return shouldPrint
    End Function
#End Region

#Region "Starting"
    ''' <summary>
    ''' Starts the work of the <see cref="Downloader"/>.
    ''' </summary>
    Protected MustOverride Sub StartDownloading()

    Public Sub Start()
        If IsPlaylistMember Then
            If Not String.IsNullOrEmpty(OutputPath) Then
                If Not IO.Directory.Exists(OutputPath) Then
                    IO.Directory.CreateDirectory(OutputPath)
                End If
            End If
        End If

        If String.IsNullOrEmpty(OutputPath) Or IsPlaylistMember Then
            If Not String.IsNullOrEmpty(OutputPath) And IsPlaylistMember Then
                If Not OutputPath.EndsWith("\") Then OutputPath &= "\"
            End If
            OutputPath = String.Format("{0}{1}", OutputPath, VideoCodec.Title.RemoveIllegalPathCharacters)
            If TypeOf Me Is AudioDownloader Then
                OutputPath = IO.Path.ChangeExtension(OutputPath, VideoCodec.AudioExtension)
            ElseIf TypeOf Me Is VideoDownloader Then
                OutputPath = IO.Path.ChangeExtension(OutputPath, VideoCodec.VideoExtension)
            End If
        End If


        StartDownloading()
    End Sub
    Public Overloads Async Sub StartAsync()
        Await Task.Factory.StartNew(AddressOf Start)
    End Sub
    Public Overloads Sub StartThreaded()
        Task.Factory.StartNew(AddressOf Start)
    End Sub

#End Region

#Region "Overrides"
    Public Overrides Function ToString() As String
        Return MyBase.ToString()
    End Function
#End Region

    Public Function GetVideoImageBytes() As Byte()
        Dim url As String = YtVideo.GetVideoCoverUrl(InputUrl)
        Dim wc As New WebClient()
        Return wc.DownloadData(url)
    End Function


End Class